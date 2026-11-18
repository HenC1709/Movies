using Microsoft.Data.Sqlite;
using Movies.Models;
using System.Security.Cryptography;

namespace Movies.Services.Sql;

internal sealed class UsuarioSqlServicio(SqliteConnection conexion)
{
    private const int Iteraciones = 600_000;

    public bool HayCuentas(SqliteTransaction? tx = null)
    {
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM Usuarios)";
        return (long)cmd.ExecuteScalar()! != 0;
    }

    public Usuario? Registrar(string nombre, string clave)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Derivar(clave, sal, Iteraciones);
        using var tx = conexion.BeginTransaction();
        Rol rol = HayCuentas(tx) ? Rol.Usuario : Rol.Admin;
        using var cmd = conexion.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO Usuarios (Nombre, NombreAcceso, Rol, Sal, Hash, Iteraciones)
            VALUES ($nombre, $acceso, $rol, $sal, $hash, $iteraciones)
            ON CONFLICT(NombreAcceso) DO NOTHING RETURNING UsuarioId
            """;
        cmd.Parameters.AddWithValue("$nombre", nombre.Trim());
        cmd.Parameters.AddWithValue("$acceso", Normalizar(nombre));
        cmd.Parameters.AddWithValue("$rol", rol.ToString());
        cmd.Parameters.AddWithValue("$sal", sal);
        cmd.Parameters.AddWithValue("$hash", hash);
        cmd.Parameters.AddWithValue("$iteraciones", Iteraciones);
        object? id = cmd.ExecuteScalar();
        tx.Commit();
        return id is long nuevoId ? new Usuario(nuevoId, nombre.Trim(), rol) : null;
    }

    public Usuario? IniciarSesion(string nombre, string clave)
    {
        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT UsuarioId, Nombre, Rol, Sal, Hash, Iteraciones FROM Usuarios WHERE NombreAcceso=$acceso";
        cmd.Parameters.AddWithValue("$acceso", Normalizar(nombre));
        using var fila = cmd.ExecuteReader();
        if (!fila.Read()) return null;
        var calculado = Derivar(clave, (byte[])fila[3], fila.GetInt32(5));
        return CryptographicOperations.FixedTimeEquals(calculado, (byte[])fila[4])
            ? new Usuario(fila.GetInt64(0), fila.GetString(1), Enum.Parse<Rol>(fila.GetString(2))) : null;
    }

    private static string Normalizar(string nombre) => nombre.Trim().Normalize().ToUpperInvariant();

    private static byte[] Derivar(string clave, byte[] sal, int iteraciones) =>
        Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, 32);
}
