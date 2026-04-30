using Exercise.Application.Abstractions.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace Exercise.Infrastructure.ExternalApis
{
    internal static partial class ExerciseMediaMatchScorer
    {
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "at", "by", "exercise", "for", "from", "in", "of", "on", "the", "to", "with", "workout"
        };

        private static readonly string[] PenaltyTerms = ["anatomy", "book", "disease", "drawing", "injury", "life", "massage", "medical", "painting", "pdf", "portrait", "rehab", "sir", "therapy"];
        private static readonly string[] ArchivePenaltyTerms = ["archive", "during", "history", "museum", "port", "postcard", "ship", "vintage"];

        public static double Score(ExerciseMediaSearchQuery query, string? sourceTitle, string? sourcePageUrl = null, string? sourceProvider = null)
        {
            var queryTokens = Tokenize(query.Name);
            if (queryTokens.Count == 0)
            {
                return 0d;
            }

            var sourceTokens = Tokenize(string.Join(' ', [sourceTitle ?? string.Empty, Slugify(sourcePageUrl)]));
            if (sourceTokens.Count == 0)
            {
                return 0d;
            }

            var matchedCount = queryTokens.Count(token => sourceTokens.Contains(token));
            if (matchedCount == 0)
            {
                return 0d;
            }

            var coverage = (double)matchedCount / queryTokens.Count;
            var precision = (double)matchedCount / sourceTokens.Count;
            var score = (coverage * 0.75d) + (precision * 0.20d);
            var normalizedProvider = Normalize(sourceProvider);

            var normalizedTitle = Normalize(sourceTitle);
            var normalizedName = Normalize(query.Name);
            if (!string.IsNullOrWhiteSpace(normalizedTitle) && normalizedTitle.Contains(normalizedName, StringComparison.Ordinal))
            {
                score += 0.08d;
            }

            var descriptorMatches = 0;
            foreach (var descriptor in new[] { query.BodyPart, query.TargetMuscle, query.Equipment })
            {
                var descriptorTokens = Tokenize(descriptor);
                if (descriptorTokens.Any(token => sourceTokens.Contains(token)))
                {
                    descriptorMatches++;
                }
            }

            score += Math.Min(0.15d, descriptorMatches * 0.05d);

            var penaltyText = Normalize(string.Join(' ', [sourceTitle ?? string.Empty, sourcePageUrl ?? string.Empty]));
            if (PenaltyTerms.Any(term => penaltyText.Contains(term, StringComparison.Ordinal)))
            {
                score -= 0.30d;
            }

            if ((normalizedProvider.Contains("openverse", StringComparison.Ordinal) || normalizedProvider.Contains("wikimedia", StringComparison.Ordinal))
                && descriptorMatches == 0)
            {
                score -= 0.22d;
            }

            if ((normalizedProvider.Contains("openverse", StringComparison.Ordinal) || normalizedProvider.Contains("wikimedia", StringComparison.Ordinal))
                && ArchivePenaltyTerms.Any(term => penaltyText.Contains(term, StringComparison.Ordinal)))
            {
                score -= 0.28d;
            }

            if (sourceTokens.Count > queryTokens.Count * 3)
            {
                score -= 0.12d;
            }

            if (DigitRegex().IsMatch(penaltyText) && !(query.Name?.Any(char.IsDigit) ?? false))
            {
                score -= 0.10d;
            }

            return Math.Clamp(score, 0d, 1d);
        }

        public static string? Slugify(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var uriText = value;
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                uriText = uri.AbsolutePath;
            }

            return SeparatorRegex().Replace(uriText, " ");
        }

        private static HashSet<string> Tokenize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return [];
            }

            return SeparatorRegex()
                .Split(Normalize(value))
                .Where(token => token.Length > 1 && !StopWords.Contains(token))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value.ToLowerInvariant())
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
            }

            return builder.ToString();
        }

        [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
        private static partial Regex SeparatorRegex();

        [GeneratedRegex("\\d{3,4}", RegexOptions.Compiled)]
        private static partial Regex DigitRegex();
    }
}