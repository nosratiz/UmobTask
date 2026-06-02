namespace GbfsQuiz.Web.Components.State;

/// <summary>
/// Per-circuit state for the currently signed-in player. The Blazor UI talks to the
/// application services directly (server-side), so it only needs the player's identity —
/// the JWT issued by <c>IAuthService</c> is used by the HTTP API, not by the UI.
/// </summary>
public sealed class PlayerSession
{
    public Guid PlayerId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsAuthenticated => PlayerId != Guid.Empty;

    /// <summary>Raised whenever the signed-in player changes, so layout + pages can re-render.</summary>
    public event Action? Changed;

    public void SignIn(Guid playerId, string displayName)
    {
        PlayerId = playerId;
        DisplayName = displayName;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        PlayerId = Guid.Empty;
        DisplayName = string.Empty;
        Changed?.Invoke();
    }
}

/// <summary>What we persist to ProtectedLocalStorage so a refresh keeps the player logged in.</summary>
public sealed record StoredSession(Guid PlayerId, string DisplayName);
