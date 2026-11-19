using System.Windows;

namespace Movies.Desktop;

public partial class CuentaWindow : DialogoAnimado
{
    private readonly SesionViewModel sesion;
    protected override bool Ocupado => !sesion.PuedeInteractuar;

    internal CuentaWindow(SesionViewModel sesion)
    {
        InitializeComponent();
        this.sesion = sesion;
        DataContext = sesion;
        Formulario.Visibility = sesion.EsInvitado ? Visibility.Visible : Visibility.Collapsed;
        Perfil.Visibility = sesion.TieneSesion ? Visibility.Visible : Visibility.Collapsed;
        Loaded += (_, _) => { if (sesion.EsInvitado) Nombre.Focus(); };
    }

    private async void Acceder(object sender, RoutedEventArgs e)
    {
        string clave = Clave.Password;
        Clave.Clear();
        await sesion.AccederAsync(clave);
        if (sesion.TieneSesion) DialogResult = true;
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
        DialogResult = true;
    }
}
