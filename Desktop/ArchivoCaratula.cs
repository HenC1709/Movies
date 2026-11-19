using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Movies.Desktop;

internal static class ArchivoCaratula
{
    internal const int MaxBytes = 8 * 1024 * 1024;
    private const int MaxLado = 4096;

    public static byte[] Leer(string ruta)
    {
        using var archivo = File.OpenRead(ruta);
        long longitud = archivo.Length;
        if (longitud == 0 || longitud > MaxBytes)
            throw new InvalidDataException("El archivo debe pesar entre 1 byte y 8 MiB.");

        byte[] imagen = new byte[(int)longitud];
        archivo.ReadExactly(imagen);
        Validar(imagen);
        return imagen;
    }

    public static void Validar(byte[] imagen)
    {
        if (imagen.Length == 0 || imagen.Length > MaxBytes)
            throw new InvalidDataException("El archivo debe pesar entre 1 byte y 8 MiB.");
        try
        {
            using var datos = new MemoryStream(imagen, writable: false);
            var decodificador = BitmapDecoder.Create(datos, BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.None);
            if (decodificador is not (PngBitmapDecoder or JpegBitmapDecoder) || decodificador.Frames.Count != 1)
                throw new InvalidDataException("Elige una imagen JPEG o PNG.");

            var cuadro = decodificador.Frames[0];
            if (cuadro.PixelWidth <= 0 || cuadro.PixelHeight <= 0 ||
                cuadro.PixelWidth > MaxLado || cuadro.PixelHeight > MaxLado)
                throw new InvalidDataException("La imagen debe medir como máximo 4096 × 4096 píxeles.");

            // Decodifica los píxeles para detectar archivos que anuncian una imagen pero no se pueden abrir.
            var pixeles = new FormatConvertedBitmap(cuadro, PixelFormats.Bgra32, null, 0);
            int anchoEnBytes = pixeles.PixelWidth * 4;
            pixeles.CopyPixels(new byte[anchoEnBytes * pixeles.PixelHeight], anchoEnBytes, 0);
        }
        catch (Exception error) when (error is FormatException or NotSupportedException or ArgumentException
            or COMException or IOException)
        {
            throw new InvalidDataException("No se pudo abrir la carátula. Elige un JPEG o PNG válido.", error);
        }

    }

    public static BitmapSource? ParaVista(byte[]? imagen, int ancho)
    {
        if (imagen is null) return null;
        try
        {
            using var datos = new MemoryStream(imagen, writable: false);
            var decoder = BitmapDecoder.Create(datos, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            if (decoder.Frames.Count == 0) return null;
            var cuadro = decoder.Frames[0];
            if (cuadro.PixelWidth is <= 0 or > MaxLado || cuadro.PixelHeight is <= 0 or > MaxLado) return null;

            using var fuente = new MemoryStream(imagen, writable: false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            int alto = ancho * 3 / 2;
            if (cuadro.PixelWidth * alto >= cuadro.PixelHeight * ancho)
                bitmap.DecodePixelWidth = Math.Min(ancho, cuadro.PixelWidth);
            else
                bitmap.DecodePixelHeight = Math.Min(alto, cuadro.PixelHeight);
            bitmap.StreamSource = fuente;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception error) when (error is FormatException or NotSupportedException or ArgumentException
            or COMException or IOException or InvalidOperationException)
        {
            return null;
        }
    }
}
