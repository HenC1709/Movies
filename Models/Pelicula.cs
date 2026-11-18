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
        internal static readonly string RutaCatalogo = Path.Combine("Data", "PeliculasGuardadas.json");
        internal static readonly string RutaRespaldo = RutaCatalogo + ".bak";

        public string Titulo { get; set; } = "";

        public string Autor { get; set; } = "";

        // Conserva el nombre usado por los registros existentes en PeliculasGuardadas.json.
        [JsonPropertyName("fechaDeIngreso")]
        public DateTime FechaDeIngreso { get; set; }


        /** Guarda la película agregada en el archivo JSON actual. */
        public static void GuardarPelicula(Pelicula nuevaPelicula)
        {
            List<Pelicula> lista = LeerCatalogo();
            lista.Add(nuevaPelicula);

            Directory.CreateDirectory(Path.GetDirectoryName(RutaCatalogo)!);
            string rutaTemporal = $"{RutaCatalogo}.{Guid.NewGuid():N}.tmp";
            try
            {
                using (var archivo = new FileStream(rutaTemporal, FileMode.CreateNew, FileAccess.Write))
                {
                    JsonSerializer.Serialize(archivo, lista, new JsonSerializerOptions { WriteIndented = true });
                    archivo.Flush(flushToDisk: true);
                }

                // El temporal se escribe en la misma carpeta antes de reemplazar el original.
                if (File.Exists(RutaCatalogo))
                {
                    File.Replace(rutaTemporal, RutaCatalogo, RutaRespaldo);
                }
                else
                {
                    File.Move(rutaTemporal, RutaCatalogo);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(rutaTemporal))
                    {
                        File.Delete(rutaTemporal);
                    }
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    // Un error de limpieza no debe ocultar el fallo del guardado.
                }
            }
        }

        /** Carga en memoria las películas guardadas en JSON. */
        public static void CargarDatos()
        {
            Lista.ListaPeliculas = LeerCatalogo();
        }

        private static List<Pelicula> LeerCatalogo()
        {
            string contenido;
            try
            {
                contenido = File.ReadAllText(RutaCatalogo);
            }
            catch (FileNotFoundException) when (!File.Exists(RutaRespaldo))
            {
                return new List<Pelicula>();
            }
            catch (DirectoryNotFoundException)
            {
                return new List<Pelicula>();
            }

            var lista = JsonSerializer.Deserialize<List<Pelicula>>(contenido);
            if (lista is null || lista.Any(pelicula =>
                pelicula is null || string.IsNullOrWhiteSpace(pelicula.Titulo) ||
                string.IsNullOrWhiteSpace(pelicula.Autor)))
            {
                throw new JsonException("Se esperaba una lista de películas con título y autor.");
            }

            return lista;
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

