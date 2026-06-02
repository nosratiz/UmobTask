namespace GbfsQuiz.Application.Features.Players.Interfaces;

/// <summary>
/// Persists avatar image files to a backing store (e.g. the web root's <c>avatars/</c>
/// folder) and returns the relative URL the file is served from. Implemented in the
/// hosting layer, which owns the physical location and how it is served.
/// </summary>
public interface IAvatarStorage
{
    /// <summary>
    /// Writes <paramref name="data"/> as a new avatar file and returns its relative URL
    /// (e.g. <c>/avatars/{id}.png</c>). If <paramref name="previousPath"/> is supplied,
    /// the file it points at is deleted, so an account never accumulates orphaned images.
    /// </summary>
    Task<string> SaveAsync(
        byte[] data, string contentType, string? previousPath, CancellationToken ct = default);
}
