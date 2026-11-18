using System;
using Movies.Logic;

namespace Movies.Services
{
    /** Busca películas en la lista cargada en memoria. */
    class Busqueda
    {
        public void Buscar()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("Introduce el nombre de la película a buscar: ");
            string? buscar = Console.ReadLine()?.Trim();

            while (buscar == string.Empty)
            {
                Console.Write("Escribe un título para buscar: ");
                buscar = Console.ReadLine()?.Trim();
            }

            if (buscar is null)
            {
                return;
            }

            var coincidencias = Lista.ListaPeliculas.FindAll(pelicula =>
                pelicula.Titulo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (coincidencias.Count == 0)
            {
                Console.WriteLine("No se encontraron coincidencias.");
                return;
            }

            foreach (var pelicula in coincidencias)
            {
                Console.WriteLine($"- {pelicula.Titulo} — {pelicula.Autor}");
            }
        }
    }
}

