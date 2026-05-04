using System;
using System.Collections.Generic;
using Movies.Models;
using Movies.Logic;


namespace Movies.Services
{
    class Boveda
{
    public void Agregar(Pelicula nuevaPelicula)
    {
       Lista.ListaPeliculas.Add(nuevaPelicula);

       Pelicula.GuardarPelicula(nuevaPelicula);

      Console.WriteLine($"Pelicula '{nuevaPelicula.Titulo}' agregada con exito ");  
    }
}
}

