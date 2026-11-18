using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Data.Sqlite;
using Movies.Services.Sql;

namespace Movies.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            string carpeta = BaseDatos.CarpetaLocal();
            using var conexion = BaseDatos.Abrir(carpeta);
            BaseDatos.Inicializar(conexion, BaseDatos.RutaCatalogo());
            var cuentas = new UsuarioSqlServicio(conexion);
            MainWindow = new MainWindow(new SesionViewModel(carpeta, !cuentas.HayCuentas()),
                new CatalogoViewModel(carpeta));
            MainWindow.Show();
        }
        catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException
            or JsonException or InvalidDataException)
        {
            MessageBox.Show($"No se pudo preparar la base de datos de Movies.\n\n{error.Message}",
                "Movies", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
