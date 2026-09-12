using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public sealed class FileTransferProgress
{
    public string FileId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long TotalBytes { get; init; }
    public long TransferredBytes { get; init; }
    public double Progress => TotalBytes > 0 ? (double)TransferredBytes / TotalBytes : 0;
    public bool IsComplete { get; init; }
    public string? Error { get; init; }
    public string? LocalPath { get; init; }
    public string? RemoteUrl { get; init; }
}

public interface IFileTransferService : IDisposable
{
    Task<FileTransferProgress> UploadFileAsync(string filePath, string conversationId, IProgress<FileTransferProgress>? progress = null, CancellationToken ct = default);
    Task<string> DownloadFileAsync(string fileUrl, string? targetPath = null, IProgress<FileTransferProgress>? progress = null, CancellationToken ct = default);
    Task<string> GetUploadUrlAsync(string fileName, string conversationId, CancellationToken ct = default);
}
