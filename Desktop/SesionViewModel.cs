using System.ComponentModel;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Movies.Models;
using Movies.Services.Sql;

namespace Movies.Desktop;

internal sealed class SesionViewModel : INotifyPropertyChanged
{
    private readonly string carpeta;
    private Usuario? usuario;
    private bool ocupado;
    private bool registro;
    private bool primeraCuenta;

    public SesionViewModel(string carpeta, bool primeraCuenta)
    {
        this.carpeta = carpeta;
        this.primeraCuenta = primeraCuenta;
        registro = primeraCuenta;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Nombre { get; set; } = "";
    public string Mensaje { get; private set; } = "";
    public bool PuedeInteractuar => !ocupado;
    public bool EsInvitado => usuario is null;
    public bool TieneSesion => usuario is not null;
    internal Usuario? UsuarioActual => usuario;
    public bool EsAdministrador => usuario?.Rol == Rol.Admin;
    public string NombreSesion => usuario?.Nombre ?? "";
    public string InicialSesion => StringInfo.GetNextTextElement(NombreSesion).ToUpper(CultureInfo.CurrentCulture);
    public string RolSesion => EsAdministrador ? "Administrador" : "Usuario";
    public string TituloFormulario => registro ? "Crear cuenta" : "Iniciar sesión";
    public string AccionAlternativa => registro ? "Ya tengo cuenta" : "Crear cuenta";
    public string Ayuda => registro
        ? primeraCuenta
            ? "La primera cuenta será administradora. Elige una contraseña de al menos 8 caracteres."
            : "Elige una contraseña de al menos 8 caracteres."
        : "Accede con una cuenta creada en Movies.";

    public void CambiarFormulario()
    {
        registro = !registro;
        Mensaje = "";
        ActualizarVista();
    }

    public async Task AccederAsync(string clave)
    {
        if (ocupado || TieneSesion) return;

        string nombre = Nombre.Trim();
        if (nombre.Length == 0 || string.IsNullOrWhiteSpace(clave))
        {
            Mensaje = "Escribe tu nombre y contraseña.";
            ActualizarVista();
            return;
        }
        if (registro && clave.Length < 8)
        {
            Mensaje = "La contraseña debe tener al menos 8 caracteres.";
            ActualizarVista();
            return;
        }

        ocupado = true;
        Mensaje = "Comprobando la cuenta…";
        ActualizarVista();
        try
        {
            usuario = await Task.Run(() =>
            {
                using var conexion = BaseDatos.Abrir(carpeta);
                var cuentas = new UsuarioSqlServicio(conexion);
                return registro ? cuentas.Registrar(nombre, clave) : cuentas.IniciarSesion(nombre, clave);
            });

            Mensaje = usuario is not null ? "" : registro
                ? "Ese nombre ya está registrado."
                : "Nombre o contraseña incorrectos.";
            if (usuario is not null) primeraCuenta = false;
        }
        catch (Exception error) when (error is SqliteException or System.IO.IOException or UnauthorizedAccessException)
        {
            Mensaje = $"No se pudo acceder a la base de datos. {error.Message}";
        }
        finally
        {
            ocupado = false;
            ActualizarVista();
        }
    }

    public void CerrarSesion()
    {
        usuario = null;
        Nombre = "";
        registro = false;
        Mensaje = "";
        ActualizarVista();
    }

    private void ActualizarVista() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
