using System;
using System.Text.Json;
using Movies.Logic;
using Movies.Models;
using Movies.Services;
using Movies.UI;

class Program
{
    /** Punto de entrada de la aplicación de consola. */
    static void Main()
    {

        try
        {
            Pelicula.CargarDatos();
        }
        catch (JsonException)
        {
            Console.Error.WriteLine($"El catálogo «{Pelicula.RutaCatalogo}» está dañado y se ha conservado.");
            MostrarInstruccionesDeRecuperacion();
            Environment.ExitCode = 1;
            return;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"No se pudo leer el catálogo «{Pelicula.RutaCatalogo}»: {error.Message}");
            MostrarInstruccionesDeRecuperacion();
            Environment.ExitCode = 1;
            return;
        }

        Auth auth = new Auth();
        List<Usuario> usuarios = UsuarioServicio.Cargar();
        Menu miMenu = new Menu();
        Boveda miDepo = new Boveda();
        Busqueda buscador = new Busqueda();

        bool ejecutando = true;
        while (ejecutando)
        {
            miMenu.Mostrar();
            string opc = Console.ReadLine()!;
            switch (opc)
            {

                case "1":
                    Usuario nuevo = auth.Registrar();
                    // La persistencia actual sigue usando JSON hasta preparar la migración a SQL.
                    if (!UsuarioServicio.ExisteUsuario(usuarios, nuevo.Nombre))
                    {
                        usuarios.Add(nuevo);
                        UsuarioServicio.Guardar(usuarios);
                        Console.WriteLine("Usuario registrado con éxito!");
                    }
                    else
                    {
                        Console.WriteLine("ese usuario ya existe");
                    }
                    break;

                case "2":
                    buscador.Buscar();
                    break;

                case "3":
                    string? titulo = LeerCampoObligatorio("Título");
                    if (titulo is null)
                    {
                        return;
                    }

                    string? autor = LeerCampoObligatorio("Autor");
                    if (autor is null)
                    {
                        return;
                    }

                    miDepo.Agregar(new Pelicula(titulo, autor));
                    break;

                case "4":
                    Lista.MostrarLista();
                    break;

                case "0":
                    ejecutando = false;
                    break;
            }
            Console.WriteLine("\npresione cualaquier tecla para continuar...");
            Console.ReadKey();
        }
    }

    private static void MostrarInstruccionesDeRecuperacion()
    {
        if (File.Exists(Pelicula.RutaRespaldo))
        {
            Console.Error.WriteLine(
                $"Guarda aparte el catálogo actual si existe. Copia «{Pelicula.RutaRespaldo}» a «{Pelicula.RutaCatalogo}» y vuelve a iniciar la aplicación.");
        }
        else
        {
            Console.Error.WriteLine("No hay un respaldo local. Revisa el archivo o restáuralo desde una copia válida y vuelve a iniciar la aplicación.");
        }
    }

    private static string? LeerCampoObligatorio(string campo)
    {
        Console.Write($"{campo}: ");
        string? valor = Console.ReadLine()?.Trim();

        while (valor == string.Empty)
        {
            Console.WriteLine($"El campo «{campo}» es obligatorio.");
            Console.Write($"{campo}: ");
            valor = Console.ReadLine()?.Trim();
        }

        return valor;
    }
}
