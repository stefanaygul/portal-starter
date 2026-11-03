namespace Saha.Common.Contracts;

/// <summary>A catalog item that field staff can request.</summary>
public sealed record ProductDto(string Code, string Name, bool InStock);
