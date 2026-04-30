namespace Exercise.Application.Common
{
    public static class ExerciseMediaPolicy
    {
        private static readonly string[] VideoExtensions = [".mp4", ".webm", ".mov", ".m4v", ".avi", ".ogg"];
        private static readonly string[] AuthoritativeProviders = ["Wger", "CuratedManifest"];

        public static bool IsSupportedExampleMedia(string? mediaKind, string? mediaUrl)
        {
            if (!string.IsNullOrWhiteSpace(mediaKind)
                && mediaKind.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                return false;
            }

            return VideoExtensions.Any(extension => mediaUrl.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsAuthoritativeProvider(string? provider)
        {
            return !string.IsNullOrWhiteSpace(provider)
                && AuthoritativeProviders.Any(candidate => string.Equals(candidate, provider, StringComparison.OrdinalIgnoreCase));
        }
    }
}