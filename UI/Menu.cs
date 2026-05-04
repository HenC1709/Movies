using System;

namespace Movies.UI
{
    class Menu
{
    public void Mostrar()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(" ==== SISTEMA DE PELICULAS V1 === ");
        Console.WriteLine("1. Registrar Usuario. ");
        Console.WriteLine("2. Buscar pelicula. ");
        Console.WriteLine("3. Agregar pelicula. ");
        Console.WriteLine("4. Lista de Peliculas ");
        Console.WriteLine("0. Salir. ");
        Console.Write("\nSelecciona una opcion. ");
    }
}
}

