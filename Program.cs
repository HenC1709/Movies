using System.Text.Json;
using Microsoft.Data.Sqlite;
using Movies.Services.Sql;
using Movies.UI;

try
{
    string carpeta = BaseDatos.CarpetaLocal();
    using var conexion = BaseDatos.Abrir(carpeta);
    int? importadas = BaseDatos.Inicializar(conexion, BaseDatos.RutaCatalogo());
    if (importadas.HasValue)
        Console.WriteLine($"Importación completada: {importadas} películas. Copia del JSON en {carpeta}.");

    var menu = new Menu(new UsuarioSqlServicio(conexion), new PeliculaSqlServicio(conexion));
    menu.Ejecutar();
}
catch (EndOfStreamException)
{
    // La entrada se cerró durante una pregunta. No se envía la operación pendiente.
}
catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException
    or JsonException or InvalidDataException)
{
    Console.Error.WriteLine($"No se pudo abrir o preparar el catálogo: {error.Message}");
    Console.Error.WriteLine("Si falló la importación, revisa Data/PeliculasGuardadas.json y vuelve a iniciar Movies.");
    Environment.ExitCode = 1;
}
