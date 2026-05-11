using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InventarioSimple;
using InventarioSimple.Models;
using InventarioUI.Utils;

namespace InventarioUI.Views
{
    public partial class MovimientoBienForm : Window
    {
        public MovimientoBienForm()
        {
            InitializeComponent();
            CargarCombos();
            AjustarFormulario();
        }

        private void CargarCombos()
        {
            using var db = new AppDbContext();

            cbBien.ItemsSource = db.Bienes
                .OrderBy(b => b.Descripcion)
                .ToList();

            cbOrigen.ItemsSource = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .ToList();

            cbDestino.ItemsSource = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        private void CbBien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbBien.SelectedItem is Bien bien)
            {
                txtEstadoAnterior.Text = bien.EstadoBien;
                cbOrigen.SelectedValue = bien.UbicacionId;
            }
        }

        private void CbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AjustarFormulario();
        }

        private void AjustarFormulario()
        {
            string tipo = (cbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            lblOrigen.Visibility = cbOrigen.Visibility =
                tipo == "TRASLADO" ? Visibility.Visible : Visibility.Collapsed;

            lblDestino.Visibility = cbDestino.Visibility =
                tipo == "INGRESO" || tipo == "TRASLADO" ? Visibility.Visible : Visibility.Collapsed;

            lblEstadoAnterior.Visibility = txtEstadoAnterior.Visibility =
                tipo == "CAMBIO_ESTADO" ? Visibility.Visible : Visibility.Collapsed;

            lblEstadoNuevo.Visibility = cbEstadoNuevo.Visibility =
                tipo == "CAMBIO_ESTADO" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cbBien.SelectedItem is not Bien bienSeleccionado)
            {
                MessageBox.Show("Selecciona un bien.");
                return;
            }

            using var db = new AppDbContext();

            var bien = db.Bienes.Find(bienSeleccionado.Id);

            if (bien == null)
            {
                MessageBox.Show("No se pudo cargar el bien.");
                return;
            }

            string tipo = (cbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(tipo))
            {
                MessageBox.Show("Selecciona el tipo de movimiento.");
                return;
            }

            if (tipo == "TRASLADO")
            {
                if (cbOrigen.SelectedValue == null || cbDestino.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar ubicación origen y destino.");
                    return;
                }

                if ((int)cbOrigen.SelectedValue == (int)cbDestino.SelectedValue)
                {
                    MessageBox.Show("El origen y destino no pueden ser iguales.");
                    return;
                }
            }



            var movimiento = new MovimientoBien
            {
                BienId = bien.Id,
                Fecha = DateTime.Now,
                Tipo = tipo,
                UbicacionOrigenId = bien.UbicacionId,
                UbicacionDestinoId = cbDestino.SelectedValue as int?,
                EstadoAnterior = bien.EstadoBien,
                EstadoNuevo = (cbEstadoNuevo.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                Observacion = txtObservacion.Text,

                UsuarioId = SessionManager.UsuarioActual.Id
            };

            // ===== APLICAR EFECTOS AL BIEN =====

            if (tipo == "INGRESO" && movimiento.UbicacionDestinoId.HasValue)
            {
                bien.UbicacionId = movimiento.UbicacionDestinoId;
            }

            if (tipo == "TRASLADO" && movimiento.UbicacionDestinoId.HasValue)
            {
                bien.UbicacionId = movimiento.UbicacionDestinoId;
            }

            if (tipo == "CAMBIO_ESTADO" && movimiento.EstadoNuevo != null)
            {
                bien.EstadoBien = movimiento.EstadoNuevo;
            }

            if (tipo == "BAJA")
            {
                bien.EstadoBien = "M";
            }

            db.MovimientosBienes.Add(movimiento);
            db.Bienes.Update(bien);
            db.SaveChanges();

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
