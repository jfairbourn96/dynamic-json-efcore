using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.Base.Comparers;

public class SerializedJsonObjectValueComparerTests
{
    private readonly SerializedJsonObjectValueComparer _comparer = new();

    [Fact]
    public void Equals_IdenticalSerializedContent_ReturnsTrue()
    {
        JsonObject a = new() { ["name"] = "Elsa", ["role"] = "Queen" };
        JsonObject b = new() { ["name"] = "Elsa", ["role"] = "Queen" };

        _comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void Equals_SamePropertiesInDifferentOrder_ReturnsFalse()
    {
        JsonObject a = new() { ["name"] = "Elsa", ["role"] = "Queen" };
        JsonObject b = new() { ["role"] = "Queen", ["name"] = "Elsa" };

        _comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void Equals_NullValues_ReturnsExpectedResults()
    {
        JsonObject value = new() { ["name"] = "Olaf" };

        _comparer.Equals(null, null).Should().BeTrue();
        _comparer.Equals(value, null).Should().BeFalse();
        _comparer.Equals(null, value).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_IdenticalSerializedContent_ReturnsSameValue()
    {
        JsonObject a = new() { ["name"] = "Elsa", ["role"] = "Queen" };
        JsonObject b = new() { ["name"] = "Elsa", ["role"] = "Queen" };

        _comparer.GetHashCode(a).Should().Be(_comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_NullValue_ReturnsZero()
    {
        JsonObject? value = null;

        _comparer.GetHashCode(value!).Should().Be(0);
    }

    [Fact]
    public void Snapshot_IsDeepCopy_MutatingOriginalDoesNotAffectSnapshot()
    {
        JsonObject original = new()
        {
            ["details"] = new JsonObject
            {
                ["friends"] = new JsonArray("Sven", "Olaf")
            }
        };

        JsonObject snapshot = _comparer.Snapshot(original);
        original["details"]!["friends"]!.AsArray().Add("Kristoff");

        snapshot["details"]!["friends"]!.AsArray().Select(v => v!.GetValue<string>())
            .Should().Equal("Sven", "Olaf");
    }
}
