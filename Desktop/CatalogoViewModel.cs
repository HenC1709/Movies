using System.ComponentModel;
using System.IO;
using Microsoft.Data.Sqlite;
using Movies.Models;
using Movies.Services.Sql;

namespace Movies.Desktop;

internal sealed class CatalogoViewModel(string carpeta) : INotifyPropertyChanged
{
    private bool cargando;
    private int revisionCatalogo;
    private int revisionLista;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Consulta { get; set; } = "";
    public IReadOnlyList<Pelicula> Peliculas { get; private set; } = [];
    public string Resumen { get; private set; } = "";
    public string Estado { get; private set; } = "";
    public bool PuedeBuscar => !cargando;
    public IReadOnlyList<EntradaLista> MiLista { get; private set; } = [];
    public string EstadoLista { get; private set; } = "";
    public bool ViendoLista { get; private set; }
    public bool ViendoCatalogo => !ViendoLista;

    public async Task BuscarAsync(bool verTodo = false)
    {
        if (cargando && !verTodo) return;

        int revision = ++revisionCatalogo;
        string titulo = verTodo ? "" : Consulta.Trim();
        Consulta = titulo;
        cargando = true;
        Estado = "Cargando películas…";
        Resumen = "";
        Peliculas = [];
        NotificarCambios();

        try
        {
            var resultados = await ConsultarAsync(servicio => servicio.Catalogo(titulo.Length == 0 ? null : titulo));
            if (revision != revisionCatalogo) return;
            Peliculas = resultados;

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
            if (revision == revisionCatalogo)
                Estado = "No se pudo leer el catálogo. Vuelve a intentar la búsqueda.";
        }
        finally
        {
            if (revision == revisionCatalogo)
            {
                cargando = false;
                NotificarCambios();
            }
        }
    }

    public void MostrarCatalogo()
    {
        revisionLista++;
        MiLista = [];
        EstadoLista = "";
        ViendoLista = false;
        NotificarCambios();
    }

    public async Task MostrarMiListaAsync(Usuario usuario)
    {
        int revision = ++revisionLista;
        ViendoLista = true;
        MiLista = [];
        EstadoLista = "Cargando tu lista…";
        NotificarCambios();

        try
        {
            var entradas = await ConsultarAsync(servicio => servicio.MiLista(usuario));
            if (revision != revisionLista) return;
            MiLista = entradas;
            EstadoLista = entradas.Count == 0
                ? "Tu lista está vacía. Abre una ficha del catálogo y marca la película."
                : "";
        }
        catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException or FormatException)
        {
            if (revision != revisionLista) return;
            EstadoLista = "No se pudo cargar tu lista. Selecciona Mi lista para volver a intentar.";
        }

        NotificarCambios();
    }

    internal Task<T> ConsultarAsync<T>(Func<PeliculaSqlServicio, T> consultar) => Task.Run(() =>
    {
        using var conexion = BaseDatos.Abrir(carpeta);
        return consultar(new PeliculaSqlServicio(conexion));
    });

    private void NotificarCambios() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
