using System.Windows.Media;
using Movies.Models;
using Movies.Services.Sql;

namespace Movies.Desktop;

internal sealed record FilaPelicula(Pelicula Pelicula, ImageSource? Caratula, string? Estado)
{
    internal static FilaPelicula Crear(PeliculaSqlServicio servicio, Pelicula pelicula, string? estado = null) =>
        new(pelicula, pelicula.TieneCaratula ? ArchivoCaratula.ParaVista(servicio.LeerCaratula(pelicula.Id), 96) : null, estado);
}
