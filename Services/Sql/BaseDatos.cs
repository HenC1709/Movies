using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text.Json;

namespace Movies.Services.Sql;

internal static class BaseDatos
{
    public static string CarpetaLocal() =>
        Path.GetFullPath(Environment.GetEnvironmentVariable("MOVIES_DATA_DIR") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Movies"));

    public static string RutaCatalogo() =>
        Path.Combine(AppContext.BaseDirectory, "Data", "PeliculasGuardadas.json");

    public static SqliteConnection Abrir(string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        var opciones = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(carpeta, "Movies.db"),
            ForeignKeys = true
        };
        var conexion = new SqliteConnection(opciones.ToString());
        conexion.Open();
        return conexion;
    }

    public static int? Inicializar(SqliteConnection conexion, string rutaJson)
    {
        // El esquema, las filas importadas y la marca de migración se confirman juntos.
        using var tx = conexion.BeginTransaction();
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Migraciones (Nombre TEXT PRIMARY KEY);
            CREATE TABLE IF NOT EXISTS Peliculas (
                PeliculaId INTEGER PRIMARY KEY, Titulo TEXT NOT NULL, Autor TEXT NOT NULL,
                FechaDeIngreso TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS Usuarios (
                UsuarioId INTEGER PRIMARY KEY, Nombre TEXT NOT NULL, NombreAcceso TEXT NOT NULL UNIQUE,
                Rol TEXT NOT NULL CHECK (Rol IN ('Admin','Usuario')), Sal BLOB NOT NULL,
                Hash BLOB NOT NULL, Iteraciones INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS UsuarioPelicula (
                UsuarioId INTEGER NOT NULL REFERENCES Usuarios(UsuarioId),
                PeliculaId INTEGER NOT NULL REFERENCES Peliculas(PeliculaId),
                Estado TEXT NOT NULL CHECK (Estado IN ('Pendiente','Vista')),
                FechaActualizacion TEXT NOT NULL, PRIMARY KEY (UsuarioId, PeliculaId));
            """;
        cmd.ExecuteNonQuery();
        cmd.CommandText = "SELECT COUNT(*) FROM Migraciones WHERE Nombre = 'caratula-local-v1'";
        if ((long)cmd.ExecuteScalar()! == 0)
        {
            cmd.CommandText = """
                ALTER TABLE Peliculas ADD COLUMN Caratula BLOB
                    CHECK (Caratula IS NULL OR (typeof(Caratula) = 'blob' AND length(Caratula) BETWEEN 1 AND 8388608));
                INSERT INTO Migraciones (Nombre) VALUES ('caratula-local-v1');
                """;
            cmd.ExecuteNonQuery();
        }

        cmd.CommandText = "SELECT COUNT(*) FROM Migraciones WHERE Nombre = 'sinopsis-v1'";
        if ((long)cmd.ExecuteScalar()! == 0)
        {
            cmd.CommandText = """
                ALTER TABLE Peliculas ADD COLUMN Sinopsis TEXT NOT NULL DEFAULT '';
                INSERT INTO Migraciones (Nombre) VALUES ('sinopsis-v1');
                """;
            cmd.ExecuteNonQuery();
        }

        cmd.CommandText = "SELECT COUNT(*) FROM Migraciones WHERE Nombre = 'catalogo-json-v1'";
        if ((long)cmd.ExecuteScalar()! > 0)
        {
            tx.Commit();
            return null;
        }

        byte[] contenido = File.ReadAllBytes(rutaJson);
        using var documento = JsonDocument.Parse(contenido.AsMemory().Span.StartsWith(new byte[] { 0xef, 0xbb, 0xbf })
            ? contenido.AsMemory(3) : contenido.AsMemory());
        if (documento.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("El catálogo JSON debe contener una lista.");

        int cantidad = 0;
        foreach (var fila in documento.RootElement.EnumerateArray())
        {
            cantidad++;
            string Campo(string nombre)
            {
                if (fila.ValueKind != JsonValueKind.Object || !fila.TryGetProperty(nombre, out var valor)
                    || valor.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(valor.GetString()))
                    throw new InvalidDataException($"Fila {cantidad}: revisa el campo {nombre}.");
                return valor.GetString()!;
            }

            string fecha = Campo("fechaDeIngreso");
            if (!DateTimeOffset.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new InvalidDataException($"Fila {cantidad}: fechaDeIngreso inválida.");
            PeliculaSqlServicio.Insertar(conexion, tx, Campo("Titulo"), Campo("Autor"), fecha);
        }

        // Guardar los mismos bytes importados antes de confirmar la migración.
        string respaldo = Path.Combine(Path.GetDirectoryName(conexion.DataSource)!, "CatalogoImportado.json");
        string temporal = respaldo + ".tmp";
        File.WriteAllBytes(temporal, contenido);
        File.Move(temporal, respaldo, overwrite: true);
        cmd.CommandText = "INSERT INTO Migraciones VALUES ('catalogo-json-v1')";
        cmd.ExecuteNonQuery();
        tx.Commit();
        return cantidad;
    }
}
