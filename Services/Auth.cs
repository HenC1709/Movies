using Movies.Models;
using Movies.Services;

namespace Movies.Services
{
    /** Registro e inicio de sesión básicos para la aplicación de consola. */
    public class Auth
    {
        /** Solicita los datos y valida que el código de acceso contenga cuatro dígitos. */
        public Usuario Registrar()
        {
            Usuario u = new Usuario();

            Console.Write("Nombre: ");
            u.Nombre = Console.ReadLine()!;

            Console.Write("Código de acceso: ");
            string codigoAcceso = Console.ReadLine()!;

            while (!EsCodigoAccesoValido(codigoAcceso))
            {
                Console.WriteLine("[ERROR] El código debe contener cuatro dígitos.");
                codigoAcceso = Console.ReadLine()!;
            }
            u.CodigoAcceso = codigoAcceso;
            return u;
        }
        /** Devuelve el usuario que coincide con el nombre y el código de acceso, si existe. */
        public Usuario? IniciarSesion(List<Usuario> usuarios)
        {
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine()!;

            Console.Write("Código de acceso: ");
            string codigoAcceso = Console.ReadLine()!;

            return usuarios.FirstOrDefault(u => u.Nombre == nombre && u.CodigoAcceso == codigoAcceso);
        }
        private bool EsCodigoAccesoValido(string texto)
        {
            return texto.Length == 4 && texto.All(char.IsDigit);
        }
    }
}
