using System.Windows;
using System.Windows.Input;

namespace Movies.Desktop;

public partial class MainWindow : Window
{
    private readonly SesionViewModel sesion;
    private readonly CatalogoViewModel catalogo;

    internal MainWindow(SesionViewModel sesion, CatalogoViewModel catalogo)
    {
        InitializeComponent();
        this.sesion = sesion;
        this.catalogo = catalogo;
        DataContext = sesion;
        PanelCatalogo.DataContext = catalogo;
        Loaded += async (_, _) =>
        {
            await catalogo.BuscarAsync();
            if (Keyboard.FocusedElement is null || ReferenceEquals(Keyboard.FocusedElement, this))
                Consulta.Focus();
        };
    }

    private async void Buscar(object sender, RoutedEventArgs e) => await BuscarYVolverAlCampo();

    private async void VerTodo(object sender, RoutedEventArgs e) => await BuscarYVolverAlCampo(verTodo: true);

    private async void BuscarConEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await BuscarYVolverAlCampo();
    }

    private async Task BuscarYVolverAlCampo(bool verTodo = false)
    {
        await catalogo.BuscarAsync(verTodo);
        if (IsActive && (Keyboard.FocusedElement is null || ReferenceEquals(Keyboard.FocusedElement, this)))
            Consulta.Focus();
    }

    private async void Acceder(object sender, RoutedEventArgs e)
    {
        string clave = Clave.Password;
        Clave.Clear();
        await sesion.AccederAsync(clave);
        if (sesion.TieneSesion) Salir.Focus();
        else if (string.IsNullOrWhiteSpace(sesion.Nombre)) Nombre.Focus();
        else Clave.Focus();
    }

    private void CambiarFormulario(object sender, RoutedEventArgs e)
    {
        Clave.Clear();
        sesion.CambiarFormulario();
        Nombre.Focus();
    }

    private void CerrarSesion(object sender, RoutedEventArgs e)
    {
        sesion.CerrarSesion();
        Nombre.Focus();
    }
}
