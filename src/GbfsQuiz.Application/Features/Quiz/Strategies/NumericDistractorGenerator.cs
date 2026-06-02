namespace GbfsQuiz.Application.Features.Quiz.Strategies;

/// <summary>Produces plausible, distinct wrong numeric answers around a correct value.</summary>
public static class NumericDistractorGenerator
{
    public static IReadOnlyList<string> Generate(int correct, int count, Random rng)
    {
        var values = new HashSet<int> { correct };
        var spread = Math.Max(2, (int)Math.Round(correct * 0.25));

        var guard = 0;
        while (values.Count < count + 1 && guard++ < 100)
        {
            var delta = rng.Next(1, spread + 1) * (rng.Next(2) == 0 ? -1 : 1);
            var candidate = correct + delta;
            if (candidate >= 0)
            {
                values.Add(candidate);
            }
        }

        return values.Where(v => v != correct).Take(count).Select(v => v.ToString()).ToList();
    }
}
