using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

public sealed class FileTransferService : IFileTransferService
{
    private readonly HttpClient _http;
    private bool _disposed;

    public FileTransferService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task<FileTransferProgress> UploadFileAsync(string filePath, string conversationId, IProgress<FileTransferProgress>? progress = null, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return new FileTransferProgress { Error = "File not found" };

        try
        {
            var fileInfo = new FileInfo(filePath);
            var uploadUrl = await GetUploadUrlAsync(fileInfo.Name, conversationId, ct);

            using var fileStream = File.OpenRead(filePath);
            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var buffer = new byte[81920];
            long totalRead = 0;
            var totalBytes = fileInfo.Length;

            using var progressContent = new ProgressableHttpContent(content, (sent) =>
            {
                totalRead += sent;
                progress?.Report(new FileTransferProgress
                {
                    FileName = fileInfo.Name,
                    TotalBytes = totalBytes,
                    TransferredBytes = totalRead
                });
            });

            var response = await _http.PutAsync(uploadUrl, progressContent, ct);
            response.EnsureSuccessStatusCode();

            var resultUrl = response.Headers.Location?.ToString() ?? uploadUrl;

            progress?.Report(new FileTransferProgress
            {
                FileName = fileInfo.Name,
                TotalBytes = totalBytes,
                TransferredBytes = totalRead,
                IsComplete = true,
                RemoteUrl = resultUrl
            });

            return new FileTransferProgress
            {
                FileName = fileInfo.Name,
                TotalBytes = totalBytes,
                TransferredBytes = totalRead,
                IsComplete = true,
                RemoteUrl = resultUrl
            };
        }
        catch (Exception ex)
        {
            return new FileTransferProgress { FileName = Path.GetFileName(filePath), Error = ex.Message };
        }
    }

    public async Task<string> DownloadFileAsync(string fileUrl, string? targetPath = null, IProgress<FileTransferProgress>? progress = null, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
        var savePath = targetPath ?? Path.Combine(Path.GetTempPath(), fileName);

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var fileStream = File.Create(savePath);
        var buffer = new byte[81920];
        long totalRead = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
            totalRead += read;
            progress?.Report(new FileTransferProgress
            {
                FileName = fileName,
                TotalBytes = totalBytes,
                TransferredBytes = totalRead
            });
        }

        progress?.Report(new FileTransferProgress
        {
            FileName = fileName,
            TotalBytes = totalBytes,
            TransferredBytes = totalRead,
            IsComplete = true,
            LocalPath = savePath
        });

        return savePath;
    }

    public Task<string> GetUploadUrlAsync(string fileName, string conversationId, CancellationToken ct = default)
    {
        var url = $"http://47.113.216.177:8002/api/upload?conversation_id={conversationId}&filename={Uri.EscapeDataString(fileName)}";
        return Task.FromResult(url);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _http.Dispose();
        GC.SuppressFinalize(this);
    }
}

internal sealed class ProgressableHttpContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly Action<int> _onProgress;

    public ProgressableHttpContent(HttpContent inner, Action<int> onProgress)
    {
        _inner = inner;
        _onProgress = onProgress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var buffer = new byte[81920];
        var totalStream = await _inner.ReadAsStreamAsync();
        int read;
        while ((read = await totalStream.ReadAsync(buffer)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, read));
            _onProgress(read);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _inner.Headers.ContentLength ?? -1;
        return length >= 0;
    }
}
