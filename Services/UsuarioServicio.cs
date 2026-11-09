using System.Text.Json;
using Movies.Models;

namespace Movies.Services
{
    /** Persistencia temporal de usuarios en el archivo JSON. */
    public static class UsuarioServicio
    {
        private static string ruta = Path.Combine("Data", "Usuarios.json");

        /** Lee los usuarios guardados; si el archivo aún no existe, devuelve una lista vacía. */
        public static List<Usuario> Cargar()
        {
            if (!File.Exists(ruta))
            return new List<Usuario>();

            string contenido = File.ReadAllText(ruta);

            return JsonSerializer.Deserialize<List<Usuario>>(contenido) ?? new List<Usuario>();

        }

        /** Guarda la lista completa de usuarios en formato JSON legible. */
        public static void Guardar(List<Usuario> usuarios)
        {
             var json = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText(ruta, json);
        }
        /** Comprueba si ya existe un usuario con el mismo nombre, ignorando mayúsculas. */
        public static bool ExisteUsuario(List<Usuario> lista, string nombre)
        {
            return lista.Any(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }
    }
}
