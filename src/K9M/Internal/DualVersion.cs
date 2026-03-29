namespace K9M;

/// <summary>
/// The internal version of the collection, that invalidates enumerators
/// and callback-based operations when changed.
/// </summary>
/// <param name="Core">Changed by all operations except removals.</param>
/// <param name="Removals">Changed by removals.</param>
internal record struct DualVersion(ushort Core, ushort Removals);
