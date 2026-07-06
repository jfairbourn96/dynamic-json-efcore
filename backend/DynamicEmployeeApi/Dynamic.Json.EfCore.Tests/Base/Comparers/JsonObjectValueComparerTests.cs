using System.Text.Json.Nodes;
using Dynamic.Json.EfCore.ChangeTracking;
using FluentAssertions;
using Xunit;

namespace Dynamic.Json.EfCore.Tests.Base.Comparers;

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
    public void GetHashCode_IdenticalContent_ReturnsSameValue()
    {
        JsonObject a = new() { ["key"] = "value" };
        JsonObject b = new() { ["key"] = "value" };

        _comparer.GetHashCode(a).Should().Be(_comparer.GetHashCode(b));
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
}
