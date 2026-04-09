namespace Exercise.Application.Features.Exercises.Support
{
    public static class ExerciseRegionCatalog
    {
        private static readonly IReadOnlyDictionary<string, string[]> BodyPartsByRegionMap =
            new Dictionary<string, string[]>
            {
                ["upper-body"] = ["back", "chest", "shoulders", "upper arms", "lower arms", "neck"],
                ["lower-body"] = ["upper legs", "lower legs"],
                ["core"] = ["waist"],
                ["cardio"] = ["cardio"],
                ["other"] = [],
            };

        public static IReadOnlyList<string> Regions => BodyPartsByRegionMap.Keys.ToList();

        public static IReadOnlyList<string> GetBodyPartsForRegion(string region)
        {
            var normalizedRegion = Normalize(region);
            return BodyPartsByRegionMap.TryGetValue(normalizedRegion, out var bodyParts)
                ? bodyParts
                : [];
        }

        public static string GetRegionForBodyPart(string bodyPart)
        {
            var normalizedBodyPart = Normalize(bodyPart);

            foreach (var (region, bodyParts) in BodyPartsByRegionMap)
            {
                if (bodyParts.Contains(normalizedBodyPart))
                {
                    return region;
                }
            }

            return "other";
        }

        public static IDictionary<string, IReadOnlyList<string>> BuildBodyPartsByRegion(
            IEnumerable<string> bodyParts)
        {
            var unique = bodyParts
                .Where(bodyPart => !string.IsNullOrWhiteSpace(bodyPart))
                .Select(bodyPart => bodyPart.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new Dictionary<string, IReadOnlyList<string>>();
            foreach (var region in Regions)
            {
                var matchingBodyParts = unique
                    .Where(bodyPart => GetRegionForBodyPart(bodyPart) == region)
                    .OrderBy(bodyPart => bodyPart)
                    .ToList();

                result[region] = matchingBodyParts;
            }

            return result;
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }
    }
}
