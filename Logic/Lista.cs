using System;
using Movies.Models;


namespace Movies.Logic
{
    /** Mantiene la lista de películas en memoria y permite mostrarla. */
    class Lista
    {
        public static List<Pelicula> ListaPeliculas = new List<Pelicula>();

        public static void MostrarLista()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(" ==== LISTA V1 ==== ");
            Console.ResetColor();

            if (ListaPeliculas.Count == 0)
            {
                Console.WriteLine("VACIO, no puedes consultar. ");
            }
            else
            {
                foreach (var item in ListaPeliculas)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("-" + item.ToString());
                }
            }

        }
    }
}

