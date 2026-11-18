using System;
using System.IO;
using System.Text.Json;
using Movies.Models;
using Movies.Logic;


namespace Movies.Services
{
    /** Coordina el alta de películas en memoria y en el JSON actual. */
    class Boveda
    {
        /** Guarda la película en disco antes de añadirla a la lista en memoria. */
        public void Agregar(Pelicula nuevaPelicula)
        {
            try
            {
                Pelicula.GuardarPelicula(nuevaPelicula);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
            {
                Console.Error.WriteLine($"No se pudo guardar la película: {error.Message}");
                return;
            }

            Lista.ListaPeliculas.Add(nuevaPelicula);
            Console.WriteLine($"Película «{nuevaPelicula.Titulo}» agregada con éxito.");
        }
    }
}

