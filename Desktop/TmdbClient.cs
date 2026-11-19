using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Movies.Desktop;

internal sealed record ResultadoTmdb(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("title")] string Titulo,
    [property: JsonPropertyName("release_date")] string? Fecha,
    [property: JsonPropertyName("poster_path")] string? Ruta,
    [property: JsonPropertyName("overview")] string? Sinopsis)
{
    public string Etiqueta => Fecha?.Length >= 4 ? $"{Titulo} ({Fecha[..4]})" : Titulo;
    public bool TieneCaratula => TmdbClient.RutaValida(Ruta);
    public bool TieneSinopsis => !string.IsNullOrWhiteSpace(Sinopsis);
}

internal sealed class TmdbClient
{
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private sealed record Respuesta([property: JsonPropertyName("results")] List<ResultadoTmdb>? Resultados);
    private sealed record Creditos([property: JsonPropertyName("crew")] List<Credito>? Equipo);
    private sealed record Credito(
        [property: JsonPropertyName("job")] string? Cargo,
        [property: JsonPropertyName("name")] string? Nombre);

    public async Task<IReadOnlyList<ResultadoTmdb>> BuscarAsync(string titulo, string token)
    {
        var contenido = await ConsultarAsync<Respuesta>(
            $"search/movie?query={Uri.EscapeDataString(titulo)}&language=es-ES&include_adult=false", token);
        return contenido?.Resultados?.Where(resultado => resultado is { Id: > 0 })
            .Take(10).ToList() ?? [];
    }

    public async Task<string> BuscarDireccionAsync(ResultadoTmdb resultado, string token)
    {
        var creditos = await ConsultarAsync<Creditos>(
            FormattableString.Invariant($"movie/{resultado.Id}/credits?language=es-ES"), token);
        var nombres = creditos?.Equipo?
            .Where(persona => persona?.Cargo == "Director" && !string.IsNullOrWhiteSpace(persona.Nombre))
            .Select(persona => persona.Nombre!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase) ?? [];
        return string.Join(", ", nombres);
    }

    private static async Task<T?> ConsultarAsync<T>(string ruta, string token)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"https://api.themoviedb.org/3/{ruta}");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var respuesta = await Http.SendAsync(solicitud);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>();
    }

    public async Task<byte[]> DescargarAsync(ResultadoTmdb resultado)
    {
        if (!resultado.TieneCaratula)
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

    internal static bool RutaValida(string? ruta) =>
        ruta is not null && Regex.IsMatch(ruta, @"\A/[A-Za-z0-9_-]+\.(jpg|jpeg|png)\z", RegexOptions.IgnoreCase);
}
