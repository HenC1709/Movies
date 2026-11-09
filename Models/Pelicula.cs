using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Movies.Logic;

namespace Movies.Models
{


    /** Representa una película y conserva temporalmente su persistencia JSON. */
    public class Pelicula
    {
        public string Titulo { get; set; } = "";

        public string Autor { get; set; } = "";

        // Conserva el nombre usado por los registros existentes en PeliculasGuardadas.json.
        [JsonPropertyName("fechaDeIngreso")]
        public DateTime FechaDeIngreso { get; set; }


        /** Guarda la película agregada en el archivo JSON actual. */
        public static void GuardarPelicula(Pelicula NuevaPeli)
        {
            string ruta = Path.Combine("Data", "PeliculasGuardadas.json");
            List<Pelicula> lista = new List<Pelicula>();
            if (File.Exists(ruta))
            {
                string contenido = File.ReadAllText(ruta);
                if (!string.IsNullOrWhiteSpace(contenido))
                {
                    lista = JsonSerializer.Deserialize<List<Pelicula>>(contenido) ?? new List<Pelicula>();
                }
            }
            lista.Add(NuevaPeli);
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            string jsonfinal = JsonSerializer.Serialize(lista, opciones);
            File.WriteAllText(ruta, jsonfinal);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Pelicula guardada con exito. ");
        }

        /** Carga en memoria las películas guardadas en JSON. */
        public static void CargarDatos()
        {
            string ruta = Path.Combine("Data", "PeliculasGuardadas.json");
            if (!File.Exists(ruta))
            {
                Lista.ListaPeliculas = new List<Pelicula>();
                return;
            }

            string contenido = File.ReadAllText(ruta);
            Lista.ListaPeliculas = string.IsNullOrWhiteSpace(contenido)
                ? new List<Pelicula>()
                : JsonSerializer.Deserialize<List<Pelicula>>(contenido) ?? new List<Pelicula>();
        }


        public Pelicula(string titulo, string autor)
        {
            Titulo = titulo;

            Autor = autor;

            FechaDeIngreso = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Titulo} - {Autor} - {FechaDeIngreso}";
        }

    }
}

