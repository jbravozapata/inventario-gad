using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventarioSimple;
using InventarioSimple.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace InventarioUI.Views
{
    public partial class BienesView : UserControl
    {
        private List<Bien> bienesCache;
        private string filtroEstado = null;
        private string filtroUbicacion = null;
        private string filtroTipo = null;

        public BienesView()
        {
            InitializeComponent();

            cmbEstado.SelectedIndex = 0; 
            cmbTipo.SelectedIndex = 0;  
            CargarUbicacionesFiltro();
            CargarBienes();
        }

        private void CargarUbicacionesFiltro()
        {
            using var db = new AppDbContext();

            var lista = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .Select(u => u.Nombre)
                .ToList();

            lista.Insert(0, "Todas");

            cmbUbicacion.ItemsSource = lista;
            cmbUbicacion.SelectedIndex = 0;
        }

        private void FiltroEstadoChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEstado.SelectedItem is not ComboBoxItem item)
                return;

            if (bienesCache == null)
                return;

            if (item.Content.ToString() == "Todos")
                filtroEstado = null;
            else
                filtroEstado = item.Tag?.ToString();

            AplicarFiltros();
        }

        private void FiltroUbicacionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUbicacion.SelectedItem == null)
                return;

            if (bienesCache == null)
                return;

            var nombre = cmbUbicacion.SelectedItem.ToString();

            if (nombre == "Todas")
                filtroUbicacion = null;
            else
                filtroUbicacion = nombre;

            AplicarFiltros();
        }

        private void CargarBienes()
        {
            using var db = new AppDbContext();

            var bienes = db.Bienes
                .Include(b => b.Ubicacion)
                .OrderBy(b => b.Descripcion)
                .ToList();

            foreach (var bien in bienes)
            {
                RecalcularDepreciacion(bien);
            }

            db.SaveChanges();

            bienesCache = bienes;

            AplicarFiltros();
        }

        private void RecalcularDepreciacion(Bien bien)
        {
            // =========================
            // CLASIFICACIÓN AUTOMÁTICA
            // =========================
            if (bien.Precio < 100)
            {
                bien.Clasificacion = "Control Administrativo";

                // ❌ NO TIENE DEPRECIACIÓN
                bien.DepreciacionAcumulada = 0;
                bien.ValorEnLibros = bien.Precio;

                return;
            }
            else
            {
                bien.Clasificacion = "Propiedad, Planta y Equipo";
            }

         
            if (bien.FechaCompra == null)
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

            decimal valorTotal = bien.Precio * bien.Cantidad;

            decimal depreciacionAnual = valorTotal / vidaUtil;

            decimal depreciacion = depreciacionAnual * años;

            if (depreciacion > valorTotal)
                depreciacion = valorTotal;

            bien.DepreciacionAcumulada = Math.Round(depreciacion, 2);
            bien.ValorEnLibros = Math.Round(valorTotal - bien.DepreciacionAcumulada, 2);

            bien.FechaActualizacion = DateTime.Now;
        }

        private void FiltroTipoChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipo.SelectedItem is not ComboBoxItem item)
                return;

            if (bienesCache == null)
                return;

            if (item.Content.ToString() == "Todos")
                filtroTipo = null;
            else
                filtroTipo = item.Content.ToString();

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (bienesCache == null)
                return;

            var datos = bienesCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroEstado))
                datos = datos.Where(b => b.EstadoBien == filtroEstado);

            if (!string.IsNullOrWhiteSpace(filtroUbicacion))
                datos = datos.Where(b => (b.Ubicacion?.Nombre ?? "") == filtroUbicacion);

            if (!string.IsNullOrWhiteSpace(filtroTipo))
                datos = datos.Where(b => b.Clasificacion == filtroTipo);

            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                string t = txtBuscar.Text.Trim().ToLower();

                datos = datos.Where(b =>
                    (b.Descripcion?.ToLower().Contains(t) ?? false) ||
                    (b.CodigoContable?.ToLower().Contains(t) ?? false) ||
                    (b.Ubicacion?.Nombre?.ToLower().Contains(t) ?? false)
                );
            }

            dgBienes.ItemsSource = datos.ToList();
        }

        private string GenerarNombreArchivo()
        {
            var partes = new List<string> { "Bienes" };

            if (!string.IsNullOrWhiteSpace(filtroEstado))
            {
                var estado = filtroEstado switch
                {
                    "B" => "Buenos",
                    "R" => "Regulares",
                    "M" => "Malos",
                    _ => filtroEstado
                };

                partes.Add(estado);
            }

            if (!string.IsNullOrWhiteSpace(filtroUbicacion))
                partes.Add(filtroUbicacion.Replace(" ", ""));

            if (!string.IsNullOrWhiteSpace(filtroTipo))
                partes.Add(filtroTipo.Replace(" ", ""));

            // 👉 fecha al final (pro)
            partes.Add(DateTime.Now.ToString("yyyyMMdd"));

            return string.Join("_", partes) + ".xlsx";
        }

        private void BtnImportarBienes_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Importar bienes desde Excel",
                Filter = "Archivo Excel (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                using var wb = new XLWorkbook(dlg.FileName);
                var ws = wb.Worksheet(1);

                using var db = new AppDbContext();

                int actualizados = 0;
                int errores = 0;

                foreach (var row in ws.RowsUsed().Skip(3))
                {
                    try
                    {
                        int id = row.Cell(1).GetValue<int>();

                        var bien = db.Bienes
                            .Include(b => b.Ubicacion)
                            .FirstOrDefault(b => b.Id == id);

                        if (bien == null)
                        {
                            errores++;
                            continue;
                        }

                        // ===== ACTUALIZACIONES SEGURAS =====

                        bien.Descripcion = row.Cell(4).GetString();
                        bien.Marca = row.Cell(7).GetString();
                        bien.Modelo = row.Cell(8).GetString();
                        bien.Serie = row.Cell(9).GetString();

                        // PRECIO
                        if (decimal.TryParse(row.Cell(10).GetString(), out decimal precio))
                            bien.Precio = precio;

                        // FECHA
                        if (DateTime.TryParse(row.Cell(11).GetString(), out DateTime fecha))
                            bien.FechaCompra = fecha;

                        bien.VidaUtil = row.Cell(12).GetString();

                        // ESTADO
                        var estado = row.Cell(13).GetString();
                        if (estado == "B" || estado == "R" || estado == "M")
                            bien.EstadoBien = estado;

                        // UBICACIÓN (IMPORTANTE)
                        var nombreUbicacion = row.Cell(5).GetString();

                        var ubicacion = db.Ubicaciones
                            .FirstOrDefault(u => u.Nombre == nombreUbicacion);

                        if (ubicacion != null)
                            bien.UbicacionId = ubicacion.Id;

                        actualizados++;
                    }
                    catch
                    {
                        errores++;
                    }
                }

                db.SaveChanges();

                MessageBox.Show(
                    $"Importación completada\n\nActualizados: {actualizados}\nErrores: {errores}",
                    "Resultado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                CargarBienes(); // refrescar vista
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar:\n{ex.Message}");
            }
        }
        private void BtnExportarBienes_Click(object sender, RoutedEventArgs e)
        {
            var lista = dgBienes.ItemsSource as List<Bien>;

            if (lista == null || !lista.Any())
            {
                MessageBox.Show("No hay datos para exportar.");
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Exportar bienes",
                FileName = GenerarNombreArchivo(),
                Filter = "Archivo Excel (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Bienes");
            
            // ===== ENCABEZADOS =====
            string[] headers = {
        "ID", "Cantidad", "Código", "Descripción", "Ubicación",
        "Tipo", "Marca", "Modelo", "Serie",
        "Precio", "Fecha Compra", "Vida Útil",
        "Estado", "Depreciación", "Valor en Libros"
    };
            // ===== TÍTULO =====
            ws.Cell(1, 1).Value = "Inventario de Bienes Institucionales";
            ws.Range(1, 1, 1, headers.Length).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

          

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(3, i + 1).Value = headers[i];
            }

            var headerRange = ws.Range(3, 1, 3, headers.Length);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ===== DATOS =====
            int row = 4;

            foreach (var b in lista)
            {
                ws.Cell(row, 1).Value = b.Id;
                ws.Cell(row, 2).Value = b.Cantidad;
                ws.Cell(row, 3).Value = b.CodigoContable;
                ws.Cell(row, 4).Value = b.Descripcion;
                ws.Cell(row, 5).Value = b.Ubicacion?.Nombre ?? "";
                ws.Cell(row, 6).Value = b.Clasificacion;
                ws.Cell(row, 7).Value = b.Marca;
                ws.Cell(row, 8).Value = b.Modelo;
                ws.Cell(row, 9).Value = b.Serie;
                ws.Cell(row, 10).Value = b.Precio;
                ws.Cell(row, 11).Value = b.FechaCompra;
                ws.Cell(row, 12).Value = b.VidaUtil;
                ws.Cell(row, 13).Value = b.EstadoBien;
                ws.Cell(row, 14).Value = b.DepreciacionAcumulada;
                ws.Cell(row, 15).Value = b.ValorEnLibros;

                row++;
            }

            // ===== FORMATO =====
            ws.Columns().AdjustToContents();

            var table = ws.Range(3, 1, row - 1, headers.Length);
            table.CreateTable();

         
            ws.Cells().Style.Protection.Locked = false;

            ws.Column(1).Style.Protection.Locked = true; // ID
            ws.Column(3).Style.Protection.Locked = true; // Código

            ws.Protect();

            wb.SaveAs(dlg.FileName);

            MessageBox.Show("Bienes exportados correctamente.",
                "Exportación",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            // Reset visual
            cmbEstado.SelectedIndex = 0;
            cmbUbicacion.SelectedIndex = 0;
            cmbTipo.SelectedIndex = 0;

            txtBuscar.Text = "";

            // Reset variables internas
            filtroEstado = null;
            filtroUbicacion = null;
            filtroTipo = null;

            // Refrescar datos
            AplicarFiltros();
        }
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void TabTodos_Click(object sender, MouseButtonEventArgs e)
        {
            filtroEstado = null;
            filtroUbicacion = null;
            AplicarFiltros();
        }

        private void TabEstado_Click(object sender, MouseButtonEventArgs e)
        {
            var menu = new ContextMenu();

            menu.Items.Add(new MenuItem { Header = "Buenos (B)", Tag = "B" });
            menu.Items.Add(new MenuItem { Header = "Regulares (R)", Tag = "R" });
            menu.Items.Add(new MenuItem { Header = "Malos (M)", Tag = "M" });

            foreach (MenuItem item in menu.Items)
            {
                item.Click += (s, ev) =>
                {
                    filtroEstado = item.Tag.ToString();
                    filtroUbicacion = null;
                    AplicarFiltros();
                };
            }

            menu.IsOpen = true;
        }

        private void TabUbicacion_Click(object sender, MouseButtonEventArgs e)
        {
            var menu = new ContextMenu();

            using var db = new AppDbContext();
            var ubicaciones = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .ToList();

            foreach (var ub in ubicaciones)
            {
                var item = new MenuItem { Header = ub.Nombre };
                item.Click += (s, ev) =>
                {
                    filtroUbicacion = ub.Nombre;
                    filtroEstado = null;
                    AplicarFiltros();
                };
                menu.Items.Add(item);
            }

            menu.IsOpen = true;
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var form = new BienForm();
            if (form.ShowDialog() == true)
                CargarBienes();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.DataContext is not Bien bien) return;

            var form = new BienForm(bien)
            {
                Owner = Window.GetWindow(this)
            };

            if (form.ShowDialog() == true)
                CargarBienes();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.DataContext is not Bien seleccionado) return;

            var mensaje = $"¿Eliminar el bien?\n\n" +
                          $"Descripción: {seleccionado.Descripcion}\n" +
                          $"Código: {seleccionado.CodigoContable}\n" +
                          $"Ubicación: {seleccionado.Ubicacion?.Nombre}\n\n" +
                          $"Esta acción no se puede deshacer.";

            if (MessageBox.Show(mensaje,
                                "Confirmar eliminación",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using var db = new AppDbContext();

                var bienDb = db.Bienes.Find(seleccionado.Id);

                if (bienDb != null)
                {
                    db.Bienes.Remove(bienDb);
                    db.SaveChanges();
                }

                CargarBienes();
            }
        }

        private void BtnFotos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.DataContext is not Bien bien) return;

            var win = new BienFotosWindow(bien)
            {
                Owner = Window.GetWindow(this)
            };

            win.ShowDialog();
        }

        // ✅ NUEVO: ABRIR FORMULARIO DE MOVIMIENTO DE BIEN
        private void BtnMovimientoBien_Click(object sender, RoutedEventArgs e)
        {
            var win = new MovimientoBienForm
            {
                Owner = Window.GetWindow(this)
            };

            if (win.ShowDialog() == true)
            {
                // refrescamos bienes para reflejar cambios de ubicación/estado
                CargarBienes();
            }
        }

        private void BtnHistorialBien_Click(object sender, RoutedEventArgs e)
        {
            if (dgBienes.SelectedItem is not Bien bien)
            {
                MessageBox.Show("Selecciona un bien primero.",
                                "Aviso",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var win = new MovimientosBienPorBienWindow(bien.Id);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void DgBienes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgBienes.SelectedItem is not Bien bien)
                return;

            var ventana = new BienDetalleWindow(bien)
            {
                Owner = Window.GetWindow(this)
            };

            ventana.ShowDialog();
        }

        private void dgBienes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
