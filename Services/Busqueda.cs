using System;
using Movies.Logic;

namespace Movies.Services
{
    /** Busca películas en la lista cargada en memoria. */
    class Busqueda
{
    public void Buscar(Boveda mibiblioteca)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("Introduce el nombre de la película a buscar: ");
        string buscar = Console.ReadLine()!;

        var resultado = Lista.ListaPeliculas.Find(l => l.Titulo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

        if (resultado != null)
        {
            Console.WriteLine($"Encontrada: {resultado.Titulo} de {resultado.Autor}.");
        }
        else
        {
            Console.WriteLine("no hay nada registrado aún");
        }
    }
}
}

