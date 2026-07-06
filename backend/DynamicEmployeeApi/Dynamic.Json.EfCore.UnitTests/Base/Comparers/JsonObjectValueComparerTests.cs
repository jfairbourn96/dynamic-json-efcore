using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.UnitTests.Base.Comparers;

public class JsonObjectValueComparerTests
{
    private readonly JsonObjectValueComparer _comparer = new();

    [Fact]
    public void Equals_IdenticalContent_ReturnsTrue()
    {
        JsonObject a = new() { ["name"] = "Alice", ["age"] = 30 };
        JsonObject b = new() { ["name"] = "Alice", ["age"] = 30 };

        _comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        JsonObject a = new() { ["name"] = "Alice" };
        JsonObject b = new() { ["name"] = "Bob" };

        _comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void Equals_EmptyObjects_ReturnsTrue()
    {
        _comparer.Equals(new JsonObject(), new JsonObject()).Should().BeTrue();
    }

    [Fact]
    public void Equals_SamePropertiesInDifferentOrder_ReturnsTrue()
    {
        JsonObject a = new() { ["name"] = "Elsa", ["role"] = "Queen" };
        JsonObject b = new() { ["role"] = "Queen", ["name"] = "Elsa" };

        _comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void Equals_NestedObjectsWithSameContent_ReturnsTrue()
    {
        JsonObject a = new()
        {
            ["name"] = "Anna",
            ["details"] = new JsonObject
            {
                ["kingdom"] = "Arendelle",
                ["loyal"] = true
            }
        };
        JsonObject b = new()
        {
            ["details"] = new JsonObject
            {
                ["loyal"] = true,
                ["kingdom"] = "Arendelle"
            },
            ["name"] = "Anna"
        };

        _comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void Equals_NestedArraysWithSameContent_ReturnsTrue()
    {
        JsonObject a = new()
        {
            ["songs"] = new JsonArray("Let It Go", "For the First Time in Forever")
        };
        JsonObject b = new()
        {
            ["songs"] = new JsonArray("Let It Go", "For the First Time in Forever")
        };

        _comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void Equals_NestedArraysWithDifferentOrder_ReturnsFalse()
    {
        JsonObject a = new()
        {
            ["songs"] = new JsonArray("Let It Go", "For the First Time in Forever")
        };
        JsonObject b = new()
        {
            ["songs"] = new JsonArray("For the First Time in Forever", "Let It Go")
        };

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
    public void GetHashCode_IdenticalContent_ReturnsSameValue()
    {
        JsonObject a = new() { ["key"] = "value" };
        JsonObject b = new() { ["key"] = "value" };

        _comparer.GetHashCode(a).Should().Be(_comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_SamePropertiesInDifferentOrder_ReturnsSameValue()
    {
        JsonObject a = new() { ["name"] = "Elsa", ["role"] = "Queen" };
        JsonObject b = new() { ["role"] = "Queen", ["name"] = "Elsa" };

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
        JsonObject original = new() { ["count"] = 1 };

        JsonObject snapshot = _comparer.Snapshot(original);
        original["count"] = 99;

        snapshot["count"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void Snapshot_IsDeepCopy_MutatingSnapshotDoesNotAffectOriginal()
    {
        JsonObject original = new() { ["count"] = 1 };

        JsonObject snapshot = _comparer.Snapshot(original);
        snapshot["count"] = 99;

        original["count"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void Snapshot_NestedValuesAreDeepCopied()
    {
        JsonObject original = new()
        {
            ["details"] = new JsonObject
            {
                ["friends"] = new JsonArray("Sven", "Olaf")
            }
        };

        JsonObject snapshot = _comparer.Snapshot(original);
        JsonArray friends = original["details"]!["friends"]!.AsArray();
        friends.Add("Kristoff");

        snapshot["details"]!["friends"]!.AsArray().Select(v => v!.GetValue<string>())
            .Should().Equal("Sven", "Olaf");
    }
}
