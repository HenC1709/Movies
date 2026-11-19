using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace Movies.Desktop;

public class DialogoAnimado : Window
{
    private bool animando;
    private bool salidaTerminada;

    protected virtual bool Ocupado => false;

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (Ocupado || animando)
        {
            e.Cancel = true;
            return;
        }
        if (e.Cancel || salidaTerminada || !IsVisible || !SystemParameters.ClientAreaAnimation
            || Content is not UIElement contenido) return;

        // WPF restablece DialogResult al cancelar el primer cierre del diálogo.
        bool? resultado = DialogResult;
        e.Cancel = true;
        animando = true;
        contenido.IsEnabled = false;
        var salida = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        salida.Completed += (_, _) =>
        {
            animando = false;
            salidaTerminada = true;
            if (resultado.HasValue && DialogResult != resultado) DialogResult = resultado;
            else Close();
        };
        contenido.BeginAnimation(UIElement.OpacityProperty, salida);
    }
}
