using System.Windows.Media;
using Movies.Models;
using Movies.Services.Sql;

namespace Movies.Desktop;

internal sealed record FilaPelicula(Pelicula Pelicula, ImageSource? Caratula, string? Estado)
{
    public int VariantePortada => (int)(Pelicula.Id % 4);

    internal static FilaPelicula Crear(PeliculaSqlServicio servicio, Pelicula pelicula, string? estado = null) =>
        new(pelicula, pelicula.TieneCaratula ? ArchivoCaratula.ParaVista(servicio.LeerCaratula(pelicula.Id), 340) : null, estado);
}
