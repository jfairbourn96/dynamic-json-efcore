using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.PostgreSql;

/// <summary>
/// Verifies the contract exposed by the built PostgreSQL NuGet package rather than only
/// checking that its project compiles.
/// </summary>
/// <remarks>
/// Package identities and dependency ranges are asserted exactly because consumers and
/// NuGet resolution depend on those values. Human-facing title and description text is
/// checked by meaning so harmless copy edits do not create test churn.
/// </remarks>
public sealed class PostgreSqlPackageMetadataTests(
    PostgreSqlPackageFixture package) : IClassFixture<PostgreSqlPackageFixture>
{
    private static readonly XNamespace Nuspec = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";

    /// <summary>
    /// Protects the supported Npgsql and EF Core major-version boundary and the required
    /// dependency on the provider-neutral core package.
    /// </summary>
    [Fact]
    public void Package_DeclaresSupportedNpgsqlAndEfCoreVersions()
    {
        var dependencies = package.Metadata
            .Element(Nuspec + "dependencies")!
            .Elements(Nuspec + "group")
            .Single(group => (string?)group.Attribute("targetFramework") == "net10.0")
            .Elements(Nuspec + "dependency")
            .ToDictionary(
                dependency => (string)dependency.Attribute("id")!,
                dependency => (string)dependency.Attribute("version")!);

        dependencies["Npgsql.EntityFrameworkCore.PostgreSQL"]
            .Should().Be("[10.0.3, 11.0.0)");
        dependencies["Microsoft.EntityFrameworkCore.Relational"]
            .Should().Be("[10.0.9, 11.0.0)");
        dependencies.Should().ContainKey("Dynamic.Json.EfCore");
    }

    /// <summary>
    /// Protects stable NuGet identity, ownership, licensing, discovery, repository, and
    /// readme metadata while allowing human-facing title wording to evolve.
    /// </summary>
    [Fact]
    public void Package_ContainsCompletePostgreSqlMetadata()
    {
        package.Metadata.Element(Nuspec + "id")!.Value
            .Should().Be("Dynamic.Json.EfCore.PostgreSql");
        package.Metadata.Element(Nuspec + "title")!.Value
            .Should().Contain("PostgreSQL");
        package.Metadata.Element(Nuspec + "authors")!.Value
            .Should().Be("Justin Fairbourn");
        package.Metadata.Element(Nuspec + "license")!.Value
            .Should().Be("MIT");
        package.Metadata.Element(Nuspec + "projectUrl")!.Value
            .Should().Be("https://github.com/jfairbourn96/dynamic-json-efcore");
        package.Metadata.Element(Nuspec + "repository")!
            .Attribute("url")!.Value
            .Should().Be("https://github.com/jfairbourn96/dynamic-json-efcore");
        package.Metadata.Element(Nuspec + "tags")!.Value
            .Should().ContainAll("ef-core", "json", "postgresql", "npgsql");
        package.Metadata.Element(Nuspec + "readme")!.Value
            .Should().Be("README.md");
        package.Entries.Should().Contain("README.md");
    }

    /// <summary>
    /// Protects the package description's advertised provider and native storage behavior
    /// without coupling the test to a particular sentence or writing style.
    /// </summary>
    [Fact]
    public void Package_DescribesNativePostgreSqlJsonSupport()
    {
        package.Metadata.Element(Nuspec + "description")!.Value
            .Should().ContainAll("PostgreSQL", "Npgsql", "jsonb");
    }
}

/// <summary>
/// Builds the PostgreSQL package once per test class and exposes its serialized NuGet
/// metadata and archive entries for assertions against the actual consumer artifact.
/// </summary>
public sealed class PostgreSqlPackageFixture : IDisposable
{
    private static readonly XNamespace Nuspec = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
    private readonly string _packageDirectory;

    public PostgreSqlPackageFixture()
    {
        var repositoryRoot = FindRepositoryRoot();
        _packageDirectory = Path.Combine(
            Path.GetTempPath(),
            $"dynamic-json-efcore-package-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_packageDirectory);

        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = repositoryRoot,
        };
        startInfo.ArgumentList.Add("pack");
        startInfo.ArgumentList.Add("Dynamic.Json.EfCore.PostgreSql/Dynamic.Json.EfCore.PostgreSql.csproj");
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(_packageDirectory);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start dotnet pack.");
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet pack failed.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }

        var packagePath = Directory.GetFiles(
                _packageDirectory,
                "Dynamic.Json.EfCore.PostgreSql.*.nupkg")
            .Single(path => !path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase));

        using var archive = ZipFile.OpenRead(packagePath);
        Entries = archive.Entries.Select(entry => entry.FullName).ToArray();
        var nuspecEntry = archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec"));
        using var nuspecStream = nuspecEntry.Open();
        Metadata = XDocument.Load(nuspecStream).Root!.Element(Nuspec + "metadata")!;
    }

    public XElement Metadata { get; }

    public IReadOnlyCollection<string> Entries { get; }

    public void Dispose()
    {
        Directory.Delete(_packageDirectory, recursive: true);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "Dynamic.Json.EfCore.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root.");
    }
}
