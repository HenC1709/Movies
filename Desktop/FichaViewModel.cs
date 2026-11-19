using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using Movies.Models;

namespace Movies.Desktop;

internal sealed class FichaViewModel(
    CatalogoViewModel catalogo, Usuario? usuario, Pelicula? pelicula, bool editar) : INotifyPropertyChanged
{
    private CambioCaratula? cambioCaratula;
    private string? nombreCaratulaElegida;
    private readonly TmdbClient tmdb = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool EsFormulario => editar;
    public bool EsFicha => !editar;
    public bool PuedeMarcar => EsFicha && usuario is not null;
    public bool PuedeEditar => EsFicha && usuario?.Rol == Rol.Admin;
    public bool EsInvitado => EsFicha && usuario is null;
    public bool Ocupado { get; private set; }
    public bool PuedeInteractuar => !Ocupado;
    public string Encabezado => EsFicha ? "Ficha de película" : pelicula is null ? "Agregar película" : "Editar película";
    public string Titulo { get; set; } = pelicula?.Titulo ?? "";
    public string Autor { get; set; } = pelicula?.Autor ?? "";
    public string Fecha => pelicula?.FechaDeIngreso.ToString("dd/MM/yyyy") ?? "";
    public string Estado { get; private set; } = "";
    public string Mensaje { get; private set; } = "";
    public ImageSource? Caratula { get; private set; }
    public int VariantePortada => (int)((pelicula?.Id ?? 0) % 4);
    public IReadOnlyList<ResultadoTmdb> ResultadosTmdb { get; private set; } = [];
    public bool HayResultadosTmdb => ResultadosTmdb.Count > 0;
    public bool PuedeQuitarCaratula => cambioCaratula is null ? pelicula?.TieneCaratula == true : cambioCaratula.Imagen is not null;
    public string ResumenCaratula => nombreCaratulaElegida is not null ? $"Seleccionada · {nombreCaratulaElegida}"
        : cambioCaratula is not null && pelicula?.TieneCaratula == true ? "Se quitará al guardar."
        : pelicula?.TieneCaratula == true ? "Carátula guardada" : "Sin carátula";

    public async Task ElegirCaratulaAsync(string ruta)
    {
        if (!EsFormulario || Ocupado || usuario?.Rol != Rol.Admin) return;
        Ocupado = true;
        Mensaje = "Comprobando la imagen…";
        NotificarCambios();
        try
        {
            var elegida = await Task.Run(() =>
            {
                byte[] imagen = ArchivoCaratula.Leer(ruta);
                return (imagen, vista: ArchivoCaratula.ParaVista(imagen, 300));
            });
            AplicarCaratula(elegida.imagen, elegida.vista, Path.GetFileName(ruta));
            Mensaje = "";
        }
        catch (InvalidDataException error)
        {
            Mensaje = error.Message;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Mensaje = "No se pudo leer el archivo. Comprueba que puedes abrirlo y vuelve a elegirlo.";
        }
        finally
        {
            Ocupado = false;
            NotificarCambios();
        }
    }

    public async Task BuscarEnTmdbAsync()
    {
        if (!EsFormulario || Ocupado || usuario?.Rol != Rol.Admin) return;
        ResultadosTmdb = [];
        string titulo = Titulo.Trim();
        string? token = Environment.GetEnvironmentVariable("TMDB_READ_ACCESS_TOKEN")?.Trim();
        if (titulo.Length == 0 || string.IsNullOrWhiteSpace(token))
        {
            Mensaje = titulo.Length == 0
                ? "Escribe el título antes de buscar en TMDB."
                : "Configura TMDB_READ_ACCESS_TOKEN y vuelve a abrir Movies.";
            NotificarCambios();
            return;
        }
        if (token.Any(char.IsWhiteSpace))
        {
            Mensaje = "TMDB_READ_ACCESS_TOKEN debe contener solo el token, sin el prefijo Bearer.";
            NotificarCambios();
            return;
        }

        bool ok = await EjecutarAsync(async () =>
        {
            ResultadosTmdb = await tmdb.BuscarAsync(titulo, token);
            return true;
        });
        if (ok && !HayResultadosTmdb)
        {
            Mensaje = "TMDB no encontró películas con carátula para ese título.";
            NotificarCambios();
        }
    }

    public async Task ElegirResultadoTmdbAsync(ResultadoTmdb resultado)
    {
        if (!EsFormulario || Ocupado || usuario?.Rol != Rol.Admin || !ResultadosTmdb.Contains(resultado)) return;
        await EjecutarAsync(async () =>
        {
            byte[] imagen = await tmdb.DescargarAsync(resultado);
            var vista = await Task.Run(() =>
            {
                ArchivoCaratula.Validar(imagen);
                return ArchivoCaratula.ParaVista(imagen, 300);
            });
            AplicarCaratula(imagen, vista, $"TMDB · {resultado.Etiqueta}");
            ResultadosTmdb = [];
            return true;
        });
    }

    private void AplicarCaratula(byte[] imagen, ImageSource? vista, string nombre)
    {
        cambioCaratula = new CambioCaratula(imagen);
        nombreCaratulaElegida = nombre;
        Caratula = vista;
    }

    public void QuitarCaratula()
    {
        if (!EsFormulario || Ocupado || usuario?.Rol != Rol.Admin || !PuedeQuitarCaratula) return;
        cambioCaratula = new CambioCaratula(null);
        nombreCaratulaElegida = null;
        Caratula = null;
        Mensaje = "";
        NotificarCambios();
    }

    public async Task CargarFichaAsync()
    {
        if (pelicula is null || (!PuedeMarcar && !pelicula.TieneCaratula)) return;
        if (PuedeMarcar) Estado = "Consultando tu lista…";
        await EjecutarAsync(async () =>
        {
            var datos = await catalogo.ConsultarAsync(servicio =>
                (caratula: pelicula.TieneCaratula
                    ? ArchivoCaratula.ParaVista(servicio.LeerCaratula(pelicula.Id), 300) : null,
                 estado: PuedeMarcar ? servicio.EstadoEnLista(usuario!, pelicula.Id) : null));
            Caratula = datos.caratula;
            if (PuedeMarcar)
                Estado = datos.estado is null ? "Aún no está en tu lista." : $"En tu lista · {datos.estado}";
            return true;
        });
        if (Mensaje.Length > 0)
        {
            Estado = "";
            NotificarCambios();
        }
    }

    public Task<bool> MarcarAsync(string estado)
    {
        if (!PuedeMarcar) return Task.FromResult(false);
        return EjecutarAsync(() => catalogo.ConsultarAsync(servicio => servicio.Marcar(usuario!, pelicula!.Id, estado)));
    }

    public Task<bool> GuardarAsync()
    {
        if (!EsFormulario || usuario?.Rol != Rol.Admin) return Task.FromResult(false);
        string titulo = Titulo.Trim();
        string autor = Autor.Trim();
        if (titulo.Length == 0 || autor.Length == 0)
        {
            Mensaje = "Escribe el título y el autor.";
            NotificarCambios();
            return Task.FromResult(false);
        }

        return EjecutarAsync(() => catalogo.ConsultarAsync(servicio =>
        {
            if (pelicula is not null) return servicio.Editar(usuario, pelicula.Id, titulo, autor, cambioCaratula);
            servicio.Agregar(usuario, titulo, autor, cambioCaratula?.Imagen);
            return true;
        }));
    }

    private async Task<bool> EjecutarAsync(Func<Task<bool>> operacion)
    {
        if (Ocupado) return false;
        Ocupado = true;
        Mensaje = "Un momento…";
        NotificarCambios();
        try
        {
            bool completada = await operacion();
            Mensaje = completada ? "" : "La película ya no está en el catálogo.";
            return completada;
        }
        catch (HttpRequestException error)
        {
            Mensaje = error.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "TMDB rechazó el token de lectura.",
                HttpStatusCode.TooManyRequests => "TMDB recibió demasiadas solicitudes. Intenta más tarde.",
                _ => "No se pudo conectar con TMDB o descargar la carátula."
            };
            return false;
        }
        catch (OperationCanceledException)
        {
            Mensaje = "TMDB tardó demasiado en responder. Vuelve a intentarlo.";
            return false;
        }
        catch (JsonException)
        {
            Mensaje = "TMDB devolvió una respuesta que Movies no pudo leer.";
            return false;
        }
        catch (InvalidDataException error)
        {
            Mensaje = error.Message;
            return false;
        }
        catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException)
        {
            Mensaje = "No se pudo completar la operación. Vuelve a intentarlo.";
            return false;
        }
        finally
        {
            Ocupado = false;
            NotificarCambios();
        }
    }

    private void NotificarCambios() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
