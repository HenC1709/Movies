using System.Windows;

namespace Movies.Desktop;

public partial class FichaWindow : Window
{
    private readonly FichaViewModel ficha;
    internal bool SolicitoEditar { get; private set; }

    internal FichaWindow(FichaViewModel ficha)
    {
        InitializeComponent();
        this.ficha = ficha;
        DataContext = ficha;
        Loaded += async (_, _) =>
        {
            if (ficha.EsFormulario) CampoTitulo.Focus();
            await ficha.CargarEstadoAsync();
            if (ficha.EsFicha)
            {
                if (ficha.PuedeMarcar) Pendiente.Focus();
                else Cerrar.Focus();
            }
        };
        Closing += (_, e) => e.Cancel = ficha.Ocupado;
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
