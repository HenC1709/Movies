namespace Movies.Models;

internal sealed record Pelicula(long Id, string Titulo, string Autor, DateTimeOffset FechaDeIngreso,
    bool TieneCaratula = false, string Sinopsis = "");
