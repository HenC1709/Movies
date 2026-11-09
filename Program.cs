using System;
using Movies.Logic;
using Movies.Models;
using Movies.Services;
using Movies.UI;

class Program
{
    /** Punto de entrada de la aplicación de consola. */
    static void Main()
    {

        Auth auth = new Auth();
        List<Usuario> usuarios = UsuarioServicio.Cargar();

        Pelicula.CargarDatos();
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
                    buscador.Buscar(miDepo);
                    break;

                case "3":
                    Console.Write("Titulo: "); string t = Console.ReadLine()!;
                    Console.Write("Autor: "); string a = Console.ReadLine()!;
                    miDepo.Agregar(new Pelicula(t, a));
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
}
