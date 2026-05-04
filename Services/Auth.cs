using Movies.Models;
using Movies.Services;

namespace Movies.Services
{
    public class Auth
    {
        public Usuario Registrar()
        {
            Usuario u = new Usuario();

            Console.Write("Nombre: ");
            u.Nombre = Console.ReadLine()!;

            Console.Write("ID: ");
            string id = Console.ReadLine()!;

            while (!SoloNumeros(id))
            {
                Console.WriteLine("[ERRO] ID INVALIDA! ");
                id = Console.ReadLine()!;
            }
            u.ID = id;
            return u;
        }
        public Usuario? login(List<Usuario> usuarios)
        {
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine()!;

            Console.Write("ID: ");
            string id = Console.ReadLine()!;
            
            return usuarios.FirstOrDefault(u => u.Nombre == nombre && u.ID == id);
        }
       private bool SoloNumeros(string texto)
        {
            return texto.Length == 4 && texto.All(char.IsDigit);
        }
    }
}