using Microsoft.Data.Sqlite;
using Movies.Models;
using Movies.Services.Sql;
using System.Text;

namespace Movies.UI;

internal sealed class Menu(UsuarioSqlServicio usuarios, PeliculaSqlServicio peliculas)
{
    private Usuario? sesion;

    public void Ejecutar()
    {
        if (!usuarios.HayCuentas())
            Console.WriteLine("Configuración inicial: registra la primera cuenta para administrar este catálogo local.");

        while (true)
        {
            Console.WriteLine($"\nMovies | {sesion?.Nombre ?? "Invitado"}");
            Console.WriteLine("2. Buscar película");
            Console.WriteLine("4. Ver catálogo");
            if (sesion == null)
            {
                Console.WriteLine("1. Registrar cuenta");
                Console.WriteLine("5. Iniciar sesión");
            }
            else
            {
                Console.WriteLine("6. Mi lista");
                Console.WriteLine("7. Marcar película");
                Console.WriteLine("8. Cerrar sesión");
                if (sesion.Rol == Rol.Admin)
                {
                    Console.WriteLine("3. Agregar película");
                    Console.WriteLine("9. Editar película");
                }
            }
            Console.WriteLine("0. Salir");
            Console.Write("Opción: ");
            string? opcion = Console.ReadLine()?.Trim();
            if (opcion is null or "0") return;
            try
            {
                switch (opcion)
                {
                    case "1" when sesion == null:
                        Registrar();
                        break;
                    case "5" when sesion == null:
                        sesion = usuarios.IniciarSesion(Campo("Nombre"), Clave());
                        Console.WriteLine(sesion == null ? "Nombre o contraseña incorrectos." : $"Bienvenido, {sesion.Nombre}.");
                        break;
                    case "2":
                        MostrarCatalogo(Campo("Título a buscar"));
                        break;
                    case "4":
                        MostrarCatalogo();
                        break;
                    case "3" when sesion?.Rol == Rol.Admin:
                        peliculas.Agregar(sesion, Campo("Título"), Campo("Autor"));
                        Console.WriteLine("Película agregada.");
                        break;
                    case "9" when sesion?.Rol == Rol.Admin:
                        if (LeerId() is long id)
                            Console.WriteLine(peliculas.Editar(sesion, id, Campo("Nuevo título"), Campo("Autor"))
                                ? "Película actualizada." : "No existe esa película.");
                        break;
                    case "6" when sesion != null:
                        var lista = peliculas.MiLista(sesion);
                        if (lista.Count == 0) Console.WriteLine("Tu lista está vacía.");
                        foreach (string fila in lista) Console.WriteLine(fila);
                        break;
                    case "7" when sesion != null:
                        Marcar(sesion);
                        break;
                    case "8" when sesion != null:
                        sesion = null;
                        Console.WriteLine("Sesión cerrada.");
                        break;
                    default:
                        Console.WriteLine("Opción no disponible para esta sesión.");
                        break;
                }
            }
            catch (SqliteException error)
            {
                Console.Error.WriteLine($"No se pudo completar la operación: {error.Message}");
            }
        }
    }

    private void Registrar()
    {
        string nombre = Campo("Nombre");
        Console.WriteLine("Elige una contraseña de al menos 8 caracteres.");
        string clave = Clave();
        if (clave.Length < 8 || string.IsNullOrWhiteSpace(clave))
        {
            Console.WriteLine("La contraseña debe tener al menos 8 caracteres y contener texto.");
            return;
        }
        sesion = usuarios.Registrar(nombre, clave);
        Console.WriteLine(sesion == null ? "Ese nombre ya está registrado." : $"Cuenta creada. Rol: {sesion.Rol}.");
    }

    private void MostrarCatalogo(string? busqueda = null)
    {
        var lista = peliculas.Catalogo(busqueda);
        if (lista.Count == 0)
            Console.WriteLine(busqueda == null ? "El catálogo está vacío." : "No se encontraron coincidencias.");
        foreach (var pelicula in lista)
            Console.WriteLine($"[{pelicula.Id}] {pelicula.Titulo} — {pelicula.Autor} ({pelicula.FechaDeIngreso:d})");
    }

    private void Marcar(Usuario usuario)
    {
        if (LeerId() is not long id) return;
        string opcion = Campo("Estado (1. Pendiente, 2. Vista)");
        string? estado = opcion switch { "1" => "Pendiente", "2" => "Vista", _ => null };
        if (estado == null)
        {
            Console.WriteLine("Estado inválido.");
            return;
        }
        Console.WriteLine(peliculas.Marcar(usuario, id, estado)
            ? $"Guardada en Mi lista como {estado}." : "No existe esa película.");
    }

    private static long? LeerId()
    {
        if (long.TryParse(Campo("ID de película (ver catálogo)"), out long id) && id > 0) return id;
        Console.WriteLine("Introduce un ID positivo.");
        return null;
    }

    private static string Campo(string etiqueta)
    {
        while (true)
        {
            Console.Write($"{etiqueta}: ");
            string texto = (Console.ReadLine() ?? throw new EndOfStreamException()).Trim();
            if (texto.Length > 0) return texto;
            Console.WriteLine("El campo es obligatorio.");
        }
    }

    private static string Clave()
    {
        Console.Write("Contraseña: ");
        if (Console.IsInputRedirected) return Console.ReadLine() ?? throw new EndOfStreamException();
        var texto = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return texto.ToString();
            }
            if (tecla.Key == ConsoleKey.Backspace && texto.Length > 0) texto.Length--;
            else if (!char.IsControl(tecla.KeyChar)) texto.Append(tecla.KeyChar);
        }
    }
}
