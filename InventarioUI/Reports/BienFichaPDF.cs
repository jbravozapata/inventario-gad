using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using InventarioSimple.Models;
using System;
using System.IO;

namespace InventarioUI.Reports
{
    public static class BienFichaPDF
    {
        public static void Generar(Bien bien, string rutaFoto, string rutaDestino, string entrega, string recibe)
        {
            var logo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asset", "logo_gad.png");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // ENCABEZADO
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(120).Height(90).Image(logo).FitHeight();

                            row.RelativeItem().AlignMiddle().Column(c =>
                            
                            {
                                c.Item().AlignCenter().Text("GOBIERNO AUTONOMO DESCENTRALIZADO PARROQUIAL RURAL DE")
                                    .FontSize(10);

                                c.Item().AlignCenter().Text("CARACOL")
                                    .FontSize(18).Bold().FontColor("#1F4E79");

                                c.Item().AlignCenter().Text("UNIDAD DE CONTROL DE ACTIVOS")
                                    .FontSize(11).FontColor(Colors.Red.Medium);
                            });
                        });

                        col.Item().PaddingTop(10);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Column(c =>
                            {
                                Campo(c, "CODIGO:", bien.CodigoContable);
                                Campo(c, "DESCRIPCION:", bien.Descripcion);
                                Campo(c, "MARCA:", bien.Marca);
                                Campo(c, "MODELO:", bien.Modelo);
                                Campo(c, "SERIE:", bien.Serie);
                                Campo(c, "PRECIO DE COMPRA:", bien.Precio.ToString("C"));
                                Campo(c, "ESTADO DEL BIEN:", bien.EstadoBien);
                                string clasificacionTexto = bien.Clasificacion;

                                if (clasificacionTexto == "Control Administrativo")
                                {
                                    clasificacionTexto += " (No depreciable)";
                                }

                                Campo(c, "CLASIFICACIÓN:", clasificacionTexto);
                                Campo(c, "OBSERVACION:", bien.Observacion);
                            });

                            row.RelativeItem(1).PaddingLeft(20).Background("#F9FAFB").Border(1).BorderColor("#D1D5DB").Padding(10).Column(foto =>
                            {
                                foto.Item().AlignCenter().AlignMiddle().Element(e =>
                                {
                                    if (!string.IsNullOrEmpty(rutaFoto) && File.Exists(rutaFoto))
                                        e.Image(rutaFoto).FitWidth();
                                    else
                                        e.Text("SIN FOTO");
                                });
                            });
                        });

                        col.Item().PaddingTop(30);

                        // FIRMAS
                        col.Item().Row(row =>
                        {
                            // =====================
                            // ENTREGA
                            // =====================
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                // espacio antes de línea
                                c.Item().Height(40);

                                // línea firma
                                c.Item().Width(220).LineHorizontal(1);

                                // 👇 NOMBRE (MEJORADO)
                                c.Item().PaddingTop(6).AlignCenter().Text(entrega ?? "")
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor("#1F2937");

                                // 👇 TEXTO FIRMA
                                c.Item().PaddingTop(6).AlignCenter().Text("ENTREGUE CONFORME")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor("#111827");
                            });

                            // =====================
                            // RECIBE
                            // =====================
                            row.RelativeItem().AlignCenter().Column(c =>
                            {
                                c.Item().Height(40);

                                c.Item().Width(220).LineHorizontal(1);

                                c.Item().PaddingTop(6).AlignCenter().Text(recibe ?? "")
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor("#1F2937");

                                c.Item().PaddingTop(6).AlignCenter().Text("RECIBI CONFORME")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor("#111827");
                            });
                        });
                    });
                });
            })
            .GeneratePdf(rutaDestino);
        }

        static void Campo(ColumnDescriptor c, string titulo, string valor)
        {
            c.Item().PaddingVertical(4).Row(row =>
            {
                row.ConstantItem(140).Text(titulo)
                    .FontSize(10)
                    .Bold()
                    .FontColor("#374151");

                row.RelativeItem().BorderBottom(1).PaddingBottom(3).Text(valor ?? "")
                    .FontSize(11)
                    .FontColor("#111827");
            });
        }


    }
}