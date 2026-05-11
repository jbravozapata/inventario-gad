using System.Windows;

namespace InventarioUI.Views
{
    public partial class ResponsablesWindow : Window
{
    public string NombreEntrega { get; private set; }
    public string NombreRecibe { get; private set; }

    public ResponsablesWindow()
    {
        InitializeComponent();
    }

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEntrega.Text) ||
            string.IsNullOrWhiteSpace(txtRecibe.Text))
        {
            MessageBox.Show("Debes completar ambos campos.");
            return;
        }

        NombreEntrega = txtEntrega.Text.Trim();
        NombreRecibe = txtRecibe.Text.Trim();

        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
}
