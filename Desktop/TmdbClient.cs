using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Movies.Desktop;

internal sealed record ResultadoTmdb(
    [property: JsonPropertyName("title")] string Titulo,
    [property: JsonPropertyName("release_date")] string? Fecha,
    [property: JsonPropertyName("poster_path")] string? Ruta)
{
    public string Etiqueta => Fecha?.Length >= 4 ? $"{Titulo} ({Fecha[..4]})" : Titulo;
}

internal sealed class TmdbClient
{
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private sealed record Respuesta([property: JsonPropertyName("results")] List<ResultadoTmdb>? Resultados);

    public async Task<IReadOnlyList<ResultadoTmdb>> BuscarAsync(string titulo, string token)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.themoviedb.org/3/search/movie?query={Uri.EscapeDataString(titulo)}&language=es-ES&include_adult=false");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var respuesta = await Http.SendAsync(solicitud);
        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadFromJsonAsync<Respuesta>();
        return contenido?.Resultados?.Where(resultado => resultado is not null && RutaValida(resultado.Ruta))
            .Take(10).ToList() ?? [];
    }

    public async Task<byte[]> DescargarAsync(ResultadoTmdb resultado)
    {
        if (!RutaValida(resultado.Ruta))
            throw new InvalidDataException("El resultado no tiene una carátula válida.");

        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var respuesta = await Http.GetAsync($"https://image.tmdb.org/t/p/w500{resultado.Ruta}",
            HttpCompletionOption.ResponseHeadersRead, limite.Token);
        respuesta.EnsureSuccessStatusCode();
        if (respuesta.Content.Headers.ContentLength > ArchivoCaratula.MaxBytes)
            throw new InvalidDataException("La carátula de TMDB supera los 8 MiB.");

        using var datos = await respuesta.Content.ReadAsStreamAsync(limite.Token);
        using var salida = new MemoryStream();
        byte[] bloque = new byte[16 * 1024];
        int cantidad;
        while ((cantidad = await datos.ReadAsync(bloque, limite.Token)) > 0)
        {
            if (salida.Length + cantidad > ArchivoCaratula.MaxBytes)
                throw new InvalidDataException("La carátula de TMDB supera los 8 MiB.");
            salida.Write(bloque, 0, cantidad);
        }

        return salida.ToArray();
    }

    private static bool RutaValida(string? ruta) =>
        ruta is not null && Regex.IsMatch(ruta, @"\A/[A-Za-z0-9_-]+\.(jpg|jpeg|png)\z", RegexOptions.IgnoreCase);
}
