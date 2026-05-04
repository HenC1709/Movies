using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Data;
using Movies.Logic;

namespace Movies.Models
{


    class Pelicula
{
    public string Titulo {get; set;} = "";

    public string Autor {get; set;} = "";

    public DateTime fechaDeIngreso {get; set; }


public static void GuardarPelicula(Pelicula NuevaPeli)
    {
        string ruta = Path.Combine("Data", "PeliculasGuardadas.json");
        List<Pelicula> lista = new List<Pelicula>();
        if (File.Exists(ruta))
        {
            string contenido = File.ReadAllText(ruta);
            if (!string.IsNullOrEmpty(contenido))
            lista = JsonSerializer.Deserialize<List<Pelicula>>(contenido)!;
        }
        lista.Add(NuevaPeli);
        var opciones = new JsonSerializerOptions {WriteIndented = true};
        string jsonfinal = JsonSerializer.Serialize(lista, opciones);
        File.WriteAllText(ruta, jsonfinal);
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("Pelicula guardada con exito. ");
    }

    public static void CargarDatos()
    {
        string ruta = Path.Combine("Data", "PeliculasGuardadas.json");
        if (File.Exists(ruta))
        {
            string contenido = File.ReadAllText(ruta);
            Lista.ListaPeliculas = JsonSerializer.Deserialize<List<Pelicula>>(contenido) ?? new List<Pelicula>();
        }
    }


public Pelicula(string titulo, string autor)
{
    Titulo = titulo;

    Autor = autor;

    fechaDeIngreso = DateTime.Now;
}

    public override string ToString()
    {
        return $"{Titulo} - {Autor} - {fechaDeIngreso}";
    }

  }
}

