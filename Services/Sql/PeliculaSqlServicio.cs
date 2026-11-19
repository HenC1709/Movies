using System.Globalization;
using Microsoft.Data.Sqlite;
using Movies.Models;

namespace Movies.Services.Sql;

internal sealed class PeliculaSqlServicio(SqliteConnection conexion)
{
    internal static void Insertar(SqliteConnection db, SqliteTransaction? tx, string titulo, string autor, string fecha,
        byte[]? caratula = null)
    {
        using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO Peliculas (Titulo, Autor, FechaDeIngreso, Caratula)
            VALUES ($titulo, $autor, $fecha, $caratula)
            """;
        cmd.Parameters.AddWithValue("$titulo", titulo);
        cmd.Parameters.AddWithValue("$autor", autor);
        cmd.Parameters.AddWithValue("$fecha", fecha);
        cmd.Parameters.Add("$caratula", SqliteType.Blob).Value = (object?)caratula ?? DBNull.Value;
        cmd.ExecuteNonQuery();
    }

    public List<Pelicula> Catalogo(string? busqueda = null)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT PeliculaId, Titulo, Autor, FechaDeIngreso, Caratula IS NOT NULL
            FROM Peliculas ORDER BY PeliculaId
            """;
        using var filas = cmd.ExecuteReader();
        var peliculas = new List<Pelicula>();
        while (filas.Read())
        {
            string titulo = filas.GetString(1);
            if (busqueda == null || titulo.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                peliculas.Add(LeerPelicula(filas));
        }
        return peliculas;
    }

    public byte[]? LeerCaratula(long peliculaId)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT Caratula FROM Peliculas WHERE PeliculaId = $id";
        cmd.Parameters.AddWithValue("$id", peliculaId);
        return cmd.ExecuteScalar() as byte[];
    }

    public void Agregar(Usuario usuario, string titulo, string autor, byte[]? caratula = null)
    {
        ExigirAdministrador(usuario);
        ValidarCampos(titulo, autor);
        Insertar(conexion, null, titulo.Trim(), autor.Trim(), DateTimeOffset.Now.ToString("O"), caratula);
    }

    public bool Editar(Usuario usuario, long id, string titulo, string autor, CambioCaratula? cambioCaratula = null)
    {
        ExigirAdministrador(usuario);
        ValidarCampos(titulo, autor);
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = cambioCaratula is null
            ? "UPDATE Peliculas SET Titulo=$titulo, Autor=$autor WHERE PeliculaId=$id"
            : "UPDATE Peliculas SET Titulo=$titulo, Autor=$autor, Caratula=$caratula WHERE PeliculaId=$id";
        if (cambioCaratula is not null)
            cmd.Parameters.Add("$caratula", SqliteType.Blob).Value = (object?)cambioCaratula.Imagen ?? DBNull.Value;
        cmd.Parameters.AddWithValue("$titulo", titulo.Trim());
        cmd.Parameters.AddWithValue("$autor", autor.Trim());
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    private static void ExigirAdministrador(Usuario usuario)
    {
        if (usuario.Rol != Rol.Admin) throw new UnauthorizedAccessException("Se requiere un administrador.");
    }

    private static void ValidarCampos(string titulo, string autor)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autor))
            throw new ArgumentException("El título y el autor son obligatorios.");
    }

    public bool Marcar(Usuario usuario, long peliculaId, string estado)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            INSERT INTO UsuarioPelicula (UsuarioId, PeliculaId, Estado, FechaActualizacion)
            SELECT $usuario, PeliculaId, $estado, $fecha FROM Peliculas WHERE PeliculaId=$pelicula
            ON CONFLICT (UsuarioId, PeliculaId) DO UPDATE SET
                Estado=excluded.Estado, FechaActualizacion=excluded.FechaActualizacion
            """;
        cmd.Parameters.AddWithValue("$usuario", usuario.Id);
        cmd.Parameters.AddWithValue("$pelicula", peliculaId);
        cmd.Parameters.AddWithValue("$estado", estado);
        cmd.Parameters.AddWithValue("$fecha", DateTimeOffset.UtcNow.ToString("O"));
        return cmd.ExecuteNonQuery() == 1;
    }

    public string? EstadoEnLista(Usuario usuario, long peliculaId)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT Estado FROM UsuarioPelicula
            WHERE UsuarioId=$usuario AND PeliculaId=$pelicula
            """;
        cmd.Parameters.AddWithValue("$usuario", usuario.Id);
        cmd.Parameters.AddWithValue("$pelicula", peliculaId);
        return cmd.ExecuteScalar() as string;
    }

    public List<EntradaLista> MiLista(Usuario usuario)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT p.PeliculaId, p.Titulo, p.Autor, p.FechaDeIngreso,
                   p.Caratula IS NOT NULL, up.Estado FROM UsuarioPelicula up
            JOIN Peliculas p ON p.PeliculaId=up.PeliculaId
            WHERE up.UsuarioId=$usuario ORDER BY p.Titulo, p.PeliculaId
            """;
        cmd.Parameters.AddWithValue("$usuario", usuario.Id);
        using var filas = cmd.ExecuteReader();
        var lista = new List<EntradaLista>();
        while (filas.Read()) lista.Add(new(LeerPelicula(filas), filas.GetString(5)));
        return lista;
    }

    private static Pelicula LeerPelicula(SqliteDataReader fila) =>
        new(fila.GetInt64(0), fila.GetString(1), fila.GetString(2),
            DateTimeOffset.Parse(fila.GetString(3), CultureInfo.InvariantCulture), fila.GetBoolean(4));
}
