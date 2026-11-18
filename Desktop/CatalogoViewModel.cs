using System.ComponentModel;
using System.IO;
using Microsoft.Data.Sqlite;
using Movies.Models;
using Movies.Services.Sql;

namespace Movies.Desktop;

internal sealed class CatalogoViewModel(string carpeta) : INotifyPropertyChanged
{
    private bool cargando;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Consulta { get; set; } = "";
    public IReadOnlyList<Pelicula> Peliculas { get; private set; } = [];
    public string Resumen { get; private set; } = "";
    public string Estado { get; private set; } = "";
    public bool PuedeBuscar => !cargando;

    public async Task BuscarAsync(bool verTodo = false)
    {
        if (cargando) return;

        string titulo = verTodo ? "" : Consulta.Trim();
        Consulta = titulo;
        cargando = true;
        Estado = "Cargando películas…";
        Resumen = "";
        Peliculas = [];
        NotificarCambios();

        try
        {
            Peliculas = await Task.Run(() =>
            {
                using var conexion = BaseDatos.Abrir(carpeta);
                var servicio = new PeliculaSqlServicio(conexion);
                return servicio.Catalogo(titulo.Length == 0 ? null : titulo);
            });

            Estado = Peliculas.Count > 0 ? "" : titulo.Length == 0
                ? "Todavía no hay películas en el catálogo."
                : $"No se encontraron películas con «{titulo}» en el título.";
            Resumen = Peliculas.Count switch
            {
                0 => "",
                1 => "1 película",
                _ => $"{Peliculas.Count} películas"
            };
        }
        catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException or FormatException)
        {
            Estado = "No se pudo leer el catálogo. Vuelve a intentar la búsqueda.";
        }
        finally
        {
            cargando = false;
            NotificarCambios();
        }
    }

    private void NotificarCambios() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
