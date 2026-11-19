using System.Windows;
using Microsoft.Win32;

namespace Movies.Desktop;

public partial class FichaWindow : DialogoAnimado
{
    private readonly FichaViewModel ficha;
    protected override bool Ocupado => ficha.Ocupado;
    internal bool SolicitoEditar { get; private set; }

    internal FichaWindow(FichaViewModel ficha)
    {
        InitializeComponent();
        this.ficha = ficha;
        DataContext = ficha;
        Loaded += async (_, _) =>
        {
            await ficha.CargarFichaAsync();
            if (ficha.EsFormulario) CampoTitulo.Focus();
            if (ficha.EsFicha)
            {
                if (ficha.PuedeMarcar) Pendiente.Focus();
                else Cerrar.Focus();
            }
        };
    }

    private void Editar(object sender, RoutedEventArgs e)
    {
        if (!ficha.PuedeEditar) return;
        SolicitoEditar = true;
        Close();
    }

    private async void Guardar(object sender, RoutedEventArgs e)
    {
        if (await ficha.GuardarAsync()) DialogResult = true;
        else if (string.IsNullOrWhiteSpace(ficha.Titulo)) CampoTitulo.Focus();
        else if (string.IsNullOrWhiteSpace(ficha.Autor)) CampoAutor.Focus();
        else GuardarPelicula.Focus();
    }

    private async void ElegirCaratula(object sender, RoutedEventArgs e)
    {
        var selector = new OpenFileDialog
        {
            Title = "Elegir carátula",
            Filter = "Imágenes JPEG y PNG|*.jpg;*.jpeg;*.png",
            CheckFileExists = true
        };
        if (selector.ShowDialog(this) == true) await ficha.ElegirCaratulaAsync(selector.FileName);
        BotonElegirCaratula.Focus();
    }

    private void QuitarCaratula(object sender, RoutedEventArgs e)
    {
        ficha.QuitarCaratula();
        BotonElegirCaratula.Focus();
    }

    private async void BuscarEnTmdb(object sender, RoutedEventArgs e)
    {
        await ficha.BuscarEnTmdbAsync();
        if (ficha.HayResultadosTmdb)
        {
            CoincidenciasTmdb.SelectedIndex = 0;
            CoincidenciasTmdb.Focus();
        }
        else BotonBuscarTmdb.Focus();
    }

    private async void UsarResultadoTmdb(object sender, RoutedEventArgs e)
    {
        if (CoincidenciasTmdb.SelectedItem is ResultadoTmdb resultado)
            await ficha.ElegirResultadoTmdbAsync(resultado);
        if (ficha.HayResultadosTmdb) CoincidenciasTmdb.Focus();
        else GuardarPelicula.Focus();
    }

    private async void MarcarPendiente(object sender, RoutedEventArgs e)
    {
        if (await ficha.MarcarAsync("Pendiente")) DialogResult = true;
        else Pendiente.Focus();
    }

    private async void MarcarVista(object sender, RoutedEventArgs e)
    {
        if (await ficha.MarcarAsync("Vista")) DialogResult = true;
        else Vista.Focus();
    }
}
