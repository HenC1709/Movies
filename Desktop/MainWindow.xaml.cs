using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Movies.Models;

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
        Aviso.Text = "";
        await catalogo.BuscarAsync(verTodo);
        if (IsActive && (Keyboard.FocusedElement is null || ReferenceEquals(Keyboard.FocusedElement, this)))
            Consulta.Focus();
    }

    private void MostrarCatalogo(object sender, RoutedEventArgs e)
    {
        catalogo.MostrarCatalogo();
        Aviso.Text = "";
        Consulta.Focus();
    }

    private async void MostrarMiLista(object sender, RoutedEventArgs e)
    {
        if (sesion.UsuarioActual is not { } usuario) return;
        Aviso.Text = "";
        await catalogo.MostrarMiListaAsync(usuario);
    }

    private async void AbrirFicha(object sender, RoutedEventArgs e) => await AbrirSeleccionAsync();

    private async void FichaConEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await AbrirSeleccionAsync();
    }

    private async void FichaConDobleClic(object sender, MouseButtonEventArgs e)
    {
        var elemento = e.OriginalSource as DependencyObject;
        while (elemento is Visual && elemento is not DataGridRow)
            elemento = VisualTreeHelper.GetParent(elemento);
        if (elemento is not DataGridRow) return;
        e.Handled = true;
        await AbrirSeleccionAsync();
    }

    private async Task AbrirSeleccionAsync()
    {
        Pelicula? pelicula = catalogo.ViendoLista
            ? (TablaLista.SelectedItem as EntradaLista)?.Pelicula
            : TablaCatalogo.SelectedItem as Pelicula;
        if (pelicula is null) return;

        var usuario = sesion.UsuarioActual;
        var dialogo = new FichaWindow(new FichaViewModel(catalogo, usuario, pelicula, editar: false)) { Owner = this };
        bool guardada = dialogo.ShowDialog() == true;
        if (dialogo.SolicitoEditar)
        {
            await AbrirEditorAsync(pelicula);
        }
        else if (guardada && usuario is not null)
        {
            Aviso.Text = "Estado actualizado en Mi lista.";
            if (catalogo.ViendoLista) await catalogo.MostrarMiListaAsync(usuario);
        }

        if (catalogo.ViendoLista) TablaLista.Focus();
        else TablaCatalogo.Focus();
    }

    private async void AgregarPelicula(object sender, RoutedEventArgs e) => await AbrirEditorAsync(null);

    private async Task AbrirEditorAsync(Pelicula? pelicula)
    {
        if (sesion.UsuarioActual is not { Rol: Rol.Admin } usuario) return;
        var dialogo = new FichaWindow(new FichaViewModel(catalogo, usuario, pelicula, editar: true)) { Owner = this };
        if (dialogo.ShowDialog() != true) return;

        catalogo.MostrarCatalogo();
        Aviso.Text = pelicula is null ? "Película agregada." : "Película actualizada.";
        await catalogo.BuscarAsync(verTodo: true);
    }

    private async void Acceder(object sender, RoutedEventArgs e) => await AccederAsync();

    private async void AccederConEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || (!Nombre.IsKeyboardFocused && !Clave.IsKeyboardFocused)) return;
        e.Handled = true;
        await AccederAsync();
    }

    private async Task AccederAsync()
    {
        string clave = Clave.Password;
        Clave.Clear();
        await sesion.AccederAsync(clave);
        if (sesion.TieneSesion) Consulta.Focus();
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
        catalogo.MostrarCatalogo();
        Aviso.Text = "";
        Nombre.Focus();
    }
}
