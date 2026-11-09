using System.Text.Json.Serialization;

namespace Movies.Models
{
    public class Usuario
    {
        public string Nombre { get; set; } = "";

        // Es el código de acceso de cuatro dígitos, no la clave primaria de la tabla Usuarios.
        [JsonPropertyName("ID")]
        public string CodigoAcceso { get; set; } = "";

        public Rol Rol { get; set; } = Rol.Usuario;
    }
}
