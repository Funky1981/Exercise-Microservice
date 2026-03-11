namespace Exercise.API.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that need both an unauthenticated client
/// (real JWT validation) and an authenticated client (TestAuthHandler bypass).
///
/// The two <see cref="IClassFixture{T}"/> declarations here are inherited by
/// every derived test class, so xUnit creates and shares the factory singletons
/// without any duplicated fixture boilerplate in the subclasses.
/// </summary>
public abstract class DualFactoryIntegrationTestBase
    : IClassFixture<ExerciseWebApplicationFactory>,
      IClassFixture<AuthBypassWebApplicationFactory>
{
    /// <summary>
    /// Factory that validates real JWTs — use this to assert 401 responses.
    /// </summary>
    protected readonly ExerciseWebApplicationFactory RealFactory;

    /// <summary>
    /// Factory whose JWT validation is replaced by <see cref="TestAuthHandler"/>
    /// — use this for all happy-path and ownership-guard (403) tests.
    /// </summary>
    protected readonly AuthBypassWebApplicationFactory BypassFactory;

    protected DualFactoryIntegrationTestBase(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        RealFactory   = realFactory;
        BypassFactory = bypassFactory;
    }
}
