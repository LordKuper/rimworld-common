using Verse;

namespace LordKuper.Common.Tests;

/// <summary>
///     A test fake implementation of <see cref="IDefProvider" /> that returns hand-built defs
///     for controlled testing without requiring a live game context.
/// </summary>
internal class FakeDefProvider : IDefProvider
{
    private readonly Dictionary<Type, object> _defsByType = new();
    private readonly Dictionary<(Type, string?), object?> _namedDefsCache = new();
    private IReadOnlyList<WorkTypeDef> _workTypeDefsInPriorityOrder = [];

    /// <inheritdoc />
    public IEnumerable<T> AllDefs<T>() where T : Def
    {
        return AllDefsListForReading<T>();
    }

    /// <inheritdoc />
    public IReadOnlyList<T> AllDefsListForReading<T>() where T : Def
    {
        var type = typeof(T);
        if (_defsByType.TryGetValue(type, out var existing) && existing is List<T> list)
            return list.AsReadOnly();
        return Array.Empty<T>();
    }

    /// <inheritdoc />
    public T? GetNamedSilentFail<T>(string? defName) where T : Def
    {
        if (string.IsNullOrEmpty(defName))
            return null;
        var key = (typeof(T), defName);
        if (_namedDefsCache.TryGetValue(key, out var cached))
            return (T?)cached;
        var type = typeof(T);
        if (!_defsByType.TryGetValue(type, out var existing) || existing is not List<T> list)
            return null;
        var found = list.FirstOrDefault(d => d.defName == defName);
        _namedDefsCache[key] = found;
        return found;
    }

    /// <inheritdoc />
    public IReadOnlyList<WorkTypeDef> WorkTypeDefsInPriorityOrder()
    {
        return _workTypeDefsInPriorityOrder;
    }

    /// <summary>
    ///     Adds a def to the fake provider. If <paramref name="def" /> already has a defName,
    ///     it will be indexed by that name for <see cref="GetNamedSilentFail{T}" /> lookups.
    /// </summary>
    internal FakeDefProvider AddDef<T>(T def) where T : Def
    {
        var type = typeof(T);
        if (!_defsByType.TryGetValue(type, out var existing))
            _defsByType[type] = new List<T> { def };
        else if (existing is List<T> list) list.Add(def);
        return this;
    }

    /// <summary>
    ///     Sets the list of work type defs in priority order for <see cref="WorkTypeDefsInPriorityOrder" />.
    /// </summary>
    internal FakeDefProvider SetWorkTypeDefsInPriorityOrder(params WorkTypeDef[] workTypeDefs)
    {
        _workTypeDefsInPriorityOrder = workTypeDefs.ToList().AsReadOnly();
        return this;
    }
}