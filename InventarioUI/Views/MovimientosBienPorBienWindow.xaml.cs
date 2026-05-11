using InventarioSimple;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using ClosedXML.Excel;


namespace InventarioUI.Views
{
    public partial class MovimientosBienPorBienWindow : Window
    {
        private readonly int _bienId;

        public MovimientosBienPorBienWindow(int bienId)
        {
            InitializeComponent();
            _bienId = bienId;
            CargarDatos();
        }

        private void CargarDatos()
        {
            using var db = new AppDbContext();

            var bien = db.Bienes.Find(_bienId);
            if (bien == null)
            {
                MessageBox.Show("Bien no encontrado.");
                Close();
                return;
            }

            txtTitulo.Text = $"Historial del bien: {bien.CodigoContable} - {bien.Descripcion}";

            var movimientos = db.MovimientosBienes
    .Include(m => m.UbicacionOrigen)
    .Include(m => m.UbicacionDestino)
    .Include(m => m.Usuario)
    .Where(m => m.BienId == _bienId)
    .OrderByDescending(m => m.Fecha)
    .ToList();

            dgMovimientos.ItemsSource = movimientos;
            timelineMovimientos.ItemsSource = movimientos;
        }

        // ======================================
        // EXPORTAR HISTORIAL A EXCEL (CSV)
        // ======================================
        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();

            var bien = db.Bienes.Find(_bienId);
            if (bien == null) return;

            var movimientos = db.MovimientosBienes
                .Include(m => m.UbicacionOrigen)
                .Include(m => m.UbicacionDestino)
                .Include(m => m.Usuario)
                .Where(m => m.BienId == _bienId)
                .OrderBy(m => m.Fecha)
                .ToList();

            if (!movimientos.Any())
            {
                MessageBox.Show("No hay movimientos para exportar.");
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Exportar historial del bien",
                FileName = $"Historial_{bien.CodigoContable}.xlsx",
                Filter = "Archivo Excel (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Historial");

            // ===== TÍTULO =====
            ws.Cell(1, 1).Value = $"Historial del bien: {bien.CodigoContable} - {bien.Descripcion}";
            ws.Range(1, 1, 1, 8).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            // ===== ENCABEZADOS =====
            ws.Cell(3, 1).Value = "Fecha";
            ws.Cell(3, 2).Value = "Tipo";
            ws.Cell(3, 3).Value = "Usuario";
            ws.Cell(3, 4).Value = "Ubicación Origen";
            ws.Cell(3, 5).Value = "Ubicación Destino";
            ws.Cell(3, 6).Value = "Estado Anterior";
            ws.Cell(3, 7).Value = "Estado Nuevo";
            ws.Cell(3, 8).Value = "Observación";

            var header = ws.Range(3, 1, 3, 8);

            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ===== DATOS =====
            int row = 4;

            foreach (var m in movimientos)
            {
                ws.Cell(row, 1).Value = m.Fecha;
                ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                ws.Cell(row, 2).Value = m.Tipo;
                ws.Cell(row, 3).Value = m.Usuario?.NombreCompleto ?? "Sistema";
                ws.Cell(row, 4).Value = m.UbicacionOrigen?.Nombre ?? "";
                ws.Cell(row, 5).Value = m.UbicacionDestino?.Nombre ?? "";
                ws.Cell(row, 6).Value = m.EstadoAnterior ?? "";
                ws.Cell(row, 7).Value = m.EstadoNuevo ?? "";
                ws.Cell(row, 8).Value = m.Observacion ?? "";

                row++;
            }

            // ===== TABLA CON FILTROS =====
            var table = ws.Range(3, 1, row - 1, 8);
            table.CreateTable();

            // ===== AJUSTAR COLUMNAS =====
            ws.Columns().AdjustToContents();

            wb.SaveAs(dlg.FileName);

            MessageBox.Show("Archivo exportado correctamente.",
                "Exportación",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
