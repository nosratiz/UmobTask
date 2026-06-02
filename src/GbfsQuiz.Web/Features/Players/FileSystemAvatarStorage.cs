using GbfsQuiz.Application.Features.Players.Interfaces;

namespace GbfsQuiz.Web.Features.Players;

/// <summary>
/// Stores avatar images as files under <c>wwwroot/avatars/</c>, served as static content
/// (same-origin, so the strict CSP allows them). The database keeps only the relative URL.
/// </summary>
/// <remarks>
/// Files live on the local filesystem, so on an ephemeral host (e.g. a free-tier container)
/// uploads are lost on redeploy/restart. Move to object storage (S3/Blob) + a stored key
/// if durable, multi-instance avatars are needed.
/// </remarks>
public sealed class FileSystemAvatarStorage(IWebHostEnvironment environment) : IAvatarStorage
{
    private const string FolderName = "avatars";

    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };

    public async Task<string> SaveAsync(
        byte[] data, string contentType, string? previousPath, CancellationToken ct = default)
    {
        var folder = Path.Combine(environment.WebRootPath, FolderName);
        Directory.CreateDirectory(folder);

        var extension = Extensions.GetValueOrDefault(contentType, ".bin");
        var fileName = $"{Guid.CreateVersion7()}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), data, ct);

        DeletePrevious(previousPath);
        return $"/{FolderName}/{fileName}";
    }

    /// <summary>Removes the prior file so replaced avatars don't accumulate as orphans.</summary>
    private void DeletePrevious(string? previousPath)
    {
        if (string.IsNullOrWhiteSpace(previousPath))
        {
            return;
        }

        var fileName = Path.GetFileName(previousPath);
        var fullPath = Path.Combine(environment.WebRootPath, FolderName, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
