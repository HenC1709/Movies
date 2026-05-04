using System;
using System.Runtime.InteropServices.Marshalling;
using Movies.Services;
using Movies.Logic;

namespace Movies.Services
{
    class Busqueda
{
    public void Buscar(Boveda mibiblioteca)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("Introduce el nombre de la pelicula a bsucar: ");
        string buscar = Console.ReadLine()!;

        var resultado = Lista.ListaPeliculas.Find(l => l.Titulo.Contains(buscar));

        if (resultado != null)
        {
            Console.WriteLine($"Encontrada: {resultado.Titulo} de {resultado.Titulo}.");
        }
        else
        {
            Console.WriteLine("no hay nada registrado aún");
        }
    }
}
}

