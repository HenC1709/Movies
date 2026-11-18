using System.ComponentModel;
using System.IO;
using Microsoft.Data.Sqlite;
using Movies.Models;

namespace Movies.Desktop;

internal sealed class FichaViewModel(
    CatalogoViewModel catalogo, Usuario? usuario, Pelicula? pelicula, bool editar) : INotifyPropertyChanged
{
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

    public async Task CargarEstadoAsync()
    {
        if (!PuedeMarcar) return;
        Estado = "Consultando tu lista…";
        await EjecutarAsync(async () =>
        {
            string? estado = await catalogo.ConsultarAsync(servicio => servicio.EstadoEnLista(usuario!, pelicula!.Id));
            Estado = estado is null ? "Aún no está en tu lista." : $"En tu lista · {estado}";
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
            if (pelicula is not null) return servicio.Editar(usuario, pelicula.Id, titulo, autor);
            servicio.Agregar(usuario, titulo, autor);
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
