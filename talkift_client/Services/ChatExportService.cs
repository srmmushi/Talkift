using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Talkift.Client.Models;

namespace Talkift.Client.Services
{
    public static class ChatExportService
    {
        public static async Task ExportToTxtAsync(List<ChatMessage> messages, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Chat Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('=', 50));

            foreach (var msg in messages)
            {
                var time = msg.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                sb.AppendLine($"[{time}] {msg.Sender}: {msg.Content}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static async Task ExportToCsvAsync(List<ChatMessage> messages, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,Sender,Content");

            foreach (var msg in messages)
            {
                var time = msg.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                var content = msg.Content?.Replace("\"", "\"\"") ?? "";
                sb.AppendLine($"\"{time}\",\"{msg.Sender}\",\"{content}\"");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static async Task ExportToJsonAsync(List<ChatMessage> messages, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 0; i < messages.Count; i++)
            {
                var msg = messages[i];
                var comma = i < messages.Count - 1 ? "," : "";
                sb.AppendLine($"  {{\"time\":\"{msg.Timestamp:O}\",\"sender\":\"{msg.Sender}\",\"content\":\"{msg.Content?.Replace("\"", "\\\"")}\"}}{comma}");
            }
            sb.AppendLine("]");

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
