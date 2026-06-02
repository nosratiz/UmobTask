using System.Globalization;
using System.Text;
using GbfsQuiz.Application.Features.Games.Responses;

namespace GbfsQuiz.Application.Features.Games;

/// <summary>Serialises a player's game history to CSV for export/download.</summary>
public static class HistoryCsvExporter
{
    private const string Header = "GameId,Outcome,Score,CorrectAnswers,WrongAnswers,StartedAtUtc,EndedAtUtc";

    public static string ToCsv(IReadOnlyList<GameSummaryResponse> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        var builder = new StringBuilder();
        builder.AppendLine(Header);
        foreach (var game in history)
        {
            builder.AppendLine(ToRow(game));
        }

        return builder.ToString();
    }

    private static string ToRow(GameSummaryResponse g) => string.Join(',',
        g.GameId,
        g.Outcome,
        g.Score.ToString(CultureInfo.InvariantCulture),
        g.CorrectAnswers.ToString(CultureInfo.InvariantCulture),
        g.WrongAnswers.ToString(CultureInfo.InvariantCulture),
        g.StartedAtUtc.ToString("o", CultureInfo.InvariantCulture),
        g.EndedAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
}
