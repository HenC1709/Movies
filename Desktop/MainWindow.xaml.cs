using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
        Navegacion.DataContext = catalogo;
        Loaded += async (_, _) => await catalogo.BuscarAsync();
    }

    private void AjustarColumnas(object sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged && sender is UniformGrid panel)
            panel.Columns = Math.Max(1, (int)(e.NewSize.Width / 158));
    }

    private async void Buscar(object sender, RoutedEventArgs e) => await BuscarAsync();

    private async void BuscarConEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await BuscarAsync();
    }

    private async Task BuscarAsync(bool verTodo = false)
    {
        Aviso.Text = "";
        Desplazamiento.ScrollToTop();
        await catalogo.BuscarAsync(verTodo);
    }

    private async void MostrarCatalogo(object sender, RoutedEventArgs e)
    {
        catalogo.MostrarCatalogo();
        await BuscarAsync(verTodo: true);
    }

    private async void MostrarMiLista(object sender, RoutedEventArgs e)
    {
        if (sesion.EsInvitado && !MostrarCuenta()) return;
        if (sesion.UsuarioActual is not { } usuario) return;
        Aviso.Text = "";
        Desplazamiento.ScrollToTop();
        await catalogo.MostrarMiListaAsync(usuario);
    }

    private void AbrirCuenta(object sender, RoutedEventArgs e)
    {
        if (!MostrarCuenta()) return;

        catalogo.MostrarCatalogo();
        Desplazamiento.ScrollToTop();
        Aviso.Text = sesion.TieneSesion ? $"Sesión iniciada como {sesion.NombreSesion}." : "Sesión cerrada.";
    }

    private bool MostrarCuenta() => new CuentaWindow(sesion) { Owner = this }.ShowDialog() == true;

    private async void AbrirFicha(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: FilaPelicula fila }) return;
        var usuario = sesion.UsuarioActual;
        var dialogo = new FichaWindow(new FichaViewModel(catalogo, usuario, fila.Pelicula, editar: false)) { Owner = this };
        bool guardada = dialogo.ShowDialog() == true;
        if (dialogo.SolicitoEditar)
        {
            await AbrirEditorAsync(fila.Pelicula);
        }
        else if (guardada && usuario is not null)
        {
            Aviso.Text = $"Se actualizó «{fila.Pelicula.Titulo}» en Mi lista.";
            if (catalogo.ViendoLista) await catalogo.MostrarMiListaAsync(usuario);
        }
    }

    private async void AgregarPelicula(object sender, RoutedEventArgs e) => await AbrirEditorAsync(null);

    private async Task AbrirEditorAsync(Pelicula? pelicula)
    {
        if (sesion.UsuarioActual is not { Rol: Rol.Admin } usuario) return;
        var ficha = new FichaViewModel(catalogo, usuario, pelicula, editar: true);
        var dialogo = new FichaWindow(ficha) { Owner = this };
        if (dialogo.ShowDialog() != true) return;

        catalogo.MostrarCatalogo();
        string accion = pelicula is null ? "Se agregó" : "Se actualizó";
        Aviso.Text = $"{accion} «{ficha.Titulo.Trim()}».";
        Desplazamiento.ScrollToTop();
        await catalogo.BuscarAsync(verTodo: true);
    }
}
