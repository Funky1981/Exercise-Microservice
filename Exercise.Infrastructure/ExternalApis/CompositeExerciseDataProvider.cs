using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class CompositeExerciseDataProvider : IExerciseDataProvider
    {
        private readonly IReadOnlyList<IExerciseCatalogProvider> _catalogProviders;
        private readonly ExerciseProviderOptions _options;
        private readonly ILogger<CompositeExerciseDataProvider> _logger;

        public CompositeExerciseDataProvider(
            IEnumerable<IExerciseCatalogProvider> catalogProviders,
            IOptions<ExerciseProviderOptions> options,
            ILogger<CompositeExerciseDataProvider> logger)
        {
            _catalogProviders = catalogProviders.ToList().AsReadOnly();
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(CancellationToken cancellationToken = default)
        {
            var selectedProviders = ResolveSelectedProviders();
            var exercises = new List<ExternalExerciseDto>();

            foreach (var provider in selectedProviders)
            {
                try
                {
                    var batch = await provider.FetchExercisesAsync(cancellationToken);
                    exercises.AddRange(batch);
                    _logger.LogInformation(
                        "Fetched {Count} exercises from catalog provider {ProviderName}.",
                        batch.Count,
                        provider.ProviderName);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Catalog provider {ProviderName} failed during sync. Continuing with the remaining providers.",
                        provider.ProviderName);
                }
            }

            _logger.LogInformation(
                "Fetched {Count} exercises from {ProviderCount} configured catalog providers.",
                exercises.Count,
                selectedProviders.Count);

            return exercises.AsReadOnly();
        }

        private IReadOnlyList<IExerciseCatalogProvider> ResolveSelectedProviders()
        {
            var requested = _options.CatalogProviders
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requested.Count == 0)
            {
                return _catalogProviders;
            }

            return _catalogProviders
                .Where(provider => requested.Contains(provider.ProviderName))
                .ToList()
                .AsReadOnly();
        }
    }
}
