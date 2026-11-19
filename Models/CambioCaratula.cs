namespace Movies.Models;

// Un cambio con imagen nula quita la carátula. La ausencia de cambio conserva la actual.
internal sealed record CambioCaratula(byte[]? Imagen);
