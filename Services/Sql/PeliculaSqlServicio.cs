using System.Globalization;
using Microsoft.Data.Sqlite;
using Movies.Models;

namespace Movies.Services.Sql;

internal sealed class PeliculaSqlServicio(SqliteConnection conexion)
{
    internal static void Insertar(SqliteConnection db, SqliteTransaction? tx, string titulo, string autor, string fecha)
    {
        using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "INSERT INTO Peliculas (Titulo, Autor, FechaDeIngreso) VALUES ($titulo, $autor, $fecha)";
        cmd.Parameters.AddWithValue("$titulo", titulo);
        cmd.Parameters.AddWithValue("$autor", autor);
        cmd.Parameters.AddWithValue("$fecha", fecha);
        cmd.ExecuteNonQuery();
    }

    public List<Pelicula> Catalogo(string? busqueda = null)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT PeliculaId, Titulo, Autor, FechaDeIngreso FROM Peliculas ORDER BY PeliculaId";
        using var filas = cmd.ExecuteReader();
        var peliculas = new List<Pelicula>();
        while (filas.Read())
        {
            string titulo = filas.GetString(1);
            if (busqueda == null || titulo.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                peliculas.Add(new(filas.GetInt64(0), titulo, filas.GetString(2),
                    DateTimeOffset.Parse(filas.GetString(3), CultureInfo.InvariantCulture)));
        }
        return peliculas;
    }

    public void Agregar(Usuario usuario, string titulo, string autor)
    {
        ExigirAdministrador(usuario);
        Insertar(conexion, null, titulo.Trim(), autor.Trim(), DateTimeOffset.Now.ToString("O"));
    }

    public bool Editar(Usuario usuario, long id, string titulo, string autor)
    {
        ExigirAdministrador(usuario);
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "UPDATE Peliculas SET Titulo=$titulo, Autor=$autor WHERE PeliculaId=$id";
        cmd.Parameters.AddWithValue("$titulo", titulo.Trim());
        cmd.Parameters.AddWithValue("$autor", autor.Trim());
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    private static void ExigirAdministrador(Usuario usuario)
    {
        if (usuario.Rol != Rol.Admin) throw new UnauthorizedAccessException("Se requiere un administrador.");
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

    public List<string> MiLista(Usuario usuario)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT p.PeliculaId, p.Titulo, up.Estado FROM UsuarioPelicula up
            JOIN Peliculas p ON p.PeliculaId=up.PeliculaId
            WHERE up.UsuarioId=$usuario ORDER BY p.Titulo, p.PeliculaId
            """;
        cmd.Parameters.AddWithValue("$usuario", usuario.Id);
        using var filas = cmd.ExecuteReader();
        var lista = new List<string>();
        while (filas.Read()) lista.Add($"[{filas.GetInt64(0)}] {filas.GetString(1)} — {filas.GetString(2)}");
        return lista;
    }
}
