using InventarioSimple;
using InventarioSimple.Models;
using System;
using System.Linq;
using System.Windows;

namespace InventarioUI.Views
{
    public partial class BienForm : Window
    {
        private readonly Bien? _bienExistente;

        public BienForm()
        {
            InitializeComponent();
            CargarUbicaciones();
        }

        public BienForm(Bien bien) : this()
        {
            _bienExistente = bien;

            txtCodigo.Text = bien.CodigoContable;
            txtDescripcion.Text = bien.Descripcion;
            txtMarca.Text = bien.Marca;
            txtModelo.Text = bien.Modelo;
            txtSerie.Text = bien.Serie;
            txtCantidad.Text = bien.Cantidad.ToString();
            txtPrecio.Text = bien.Precio.ToString();
            dpFechaCompra.SelectedDate = bien.FechaCompra;
            txtVidaUtil.Text = bien.VidaUtil;   // ES STRING EN TU MODELO
            cbEstado.Text = bien.EstadoBien;
            txtObservacion.Text = bien.Observacion;
            cbUbicacion.SelectedValue = bien.UbicacionId;
        }

        private void CargarUbicaciones()
        {
            using var db = new AppDbContext();

            cbUbicacion.ItemsSource = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .ToList();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();

            int cantidad = int.TryParse(txtCantidad.Text, out var c) ? c : 1;
            decimal precio = decimal.TryParse(txtPrecio.Text, out var p) ? p : 0;

            DateTime fechaCompra = dpFechaCompra.SelectedDate ?? DateTime.Now;

            if (_bienExistente == null)
            {
                var nuevo = new Bien
                {
                    CodigoContable = txtCodigo.Text,
                    Descripcion = txtDescripcion.Text,
                    Marca = txtMarca.Text,
                    Modelo = txtModelo.Text,
                    Serie = txtSerie.Text,
                    Cantidad = cantidad,
                    Precio = precio,
                    FechaCompra = fechaCompra,
                    VidaUtil = txtVidaUtil.Text,   // STRING
                    EstadoBien = cbEstado.Text,
                    Observacion = txtObservacion.Text,
                    UbicacionId = (int?)cbUbicacion.SelectedValue
                };

                CalcularDepreciacion(nuevo);

                db.Bienes.Add(nuevo);
            }
            else
            {
                _bienExistente.CodigoContable = txtCodigo.Text;
                _bienExistente.Descripcion = txtDescripcion.Text;
                _bienExistente.Marca = txtMarca.Text;
                _bienExistente.Modelo = txtModelo.Text;
                _bienExistente.Serie = txtSerie.Text;
                _bienExistente.Cantidad = cantidad;
                _bienExistente.Precio = precio;
                _bienExistente.FechaCompra = fechaCompra;
                _bienExistente.VidaUtil = txtVidaUtil.Text;
                _bienExistente.EstadoBien = cbEstado.Text;
                _bienExistente.Observacion = txtObservacion.Text;
                _bienExistente.UbicacionId = (int?)cbUbicacion.SelectedValue;

                CalcularDepreciacion(_bienExistente);

                db.Bienes.Update(_bienExistente);
            }

            db.SaveChanges();

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CalcularDepreciacion(Bien bien)
        {
            if (bien.FechaCompra == null)
                return;

            if (bien.FechaCompra > DateTime.Now)
                return;

            int vidaUtil = int.TryParse(bien.VidaUtil, out var v) ? v : 1;

            if (vidaUtil <= 0)
                vidaUtil = 1;

            DateTime hoy = DateTime.Now;

            int años = hoy.Year - bien.FechaCompra.Value.Year;

            if (bien.FechaCompra.Value.Date > hoy.AddYears(-años))
                años--;

            if (años < 0)
                años = 0;

            // 🔥 CORRECCIÓN CLAVE
            decimal valorTotal = bien.Precio * bien.Cantidad;

            decimal depreciacionAnual = valorTotal / vidaUtil;

            decimal depreciacion = depreciacionAnual * años;

            if (depreciacion > valorTotal)
                depreciacion = valorTotal;

            bien.DepreciacionAcumulada = Math.Round(depreciacion, 2);

            bien.ValorEnLibros = Math.Round(valorTotal - bien.DepreciacionAcumulada, 2);

            bien.FechaActualizacion = DateTime.Now;
        }
    }
}