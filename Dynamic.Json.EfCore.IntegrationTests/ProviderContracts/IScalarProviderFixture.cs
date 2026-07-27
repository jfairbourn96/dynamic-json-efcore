namespace Dynamic.Json.EfCore.IntegrationTests.ProviderContracts;

/// <summary>
/// Common connection boundary for real-database scalar provider contract tests.
/// </summary>
public interface IScalarProviderFixture
{
    /// <summary>Gets the provider container connection string.</summary>
    string ConnectionString { get; }
}
