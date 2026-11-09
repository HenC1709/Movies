using System;
using System.Collections.Generic;
using Movies.Models;
using Movies.Logic;


namespace Movies.Services
{
    /** Coordina el alta de películas en memoria y en el JSON actual. */
    class Boveda
{
    /** Añade una película a la lista y guarda el registro en disco. */
    public void Agregar(Pelicula nuevaPelicula)
    {
       Lista.ListaPeliculas.Add(nuevaPelicula);

       Pelicula.GuardarPelicula(nuevaPelicula);

      Console.WriteLine($"Pelicula '{nuevaPelicula.Titulo}' agregada con exito ");  
    }
}
}

