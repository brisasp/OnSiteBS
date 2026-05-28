using System.Drawing;
using GestionAccesos.DTO;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace GestionAccesos.Services.ExcelExporter;

public class VisitasExcelExporter
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IToastService _toastService;

    public VisitasExcelExporter(IJSRuntime jsRuntime, IToastService toastService)
    {
        _jsRuntime = jsRuntime;
        _toastService = toastService;
    }

    public async Task ExportarFichajes(List<FichajesTrabajadorDTO> fichajes)
    {
        try
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Fichajes");

            var headers = new[] { "ID", "Trabajador", "Entrada", "Salida", "Duración", "Estado" };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(169, 28, 50));
                ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                ws.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var row = 2;
            foreach (var f in fichajes)
            {
                var duracion = f.HoraEntrada.HasValue
                    ? $"{(int)((f.HoraSalida ?? DateTime.Now) - f.HoraEntrada.Value).TotalHours}h {((f.HoraSalida ?? DateTime.Now) - f.HoraEntrada.Value).Minutes:D2}m"
                    : "-";

                ws.Cells[row, 1].Value = f.IdFichaje;
                ws.Cells[row, 2].Value = f.NombreCompleto;
                ws.Cells[row, 3].Value = f.HoraEntrada.HasValue ? f.HoraEntrada.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                ws.Cells[row, 4].Value = f.HoraSalida.HasValue ? f.HoraSalida.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                ws.Cells[row, 5].Value = duracion;
                ws.Cells[row, 6].Value = f.HoraSalida.HasValue ? "Cerrado" : "Abierto";

                if (row % 2 == 0)
                {
                    using var range = ws.Cells[row, 1, row, headers.Length];
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                }

                row++;
            }

            using var tableRange = ws.Cells[1, 1, row - 1, headers.Length];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            tableRange.AutoFitColumns(12, 40);

            var fileContent = package.GetAsByteArray();
            var fileName = $"Fichajes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Convert.ToBase64String(fileContent));

            _toastService.MostrarOk("Fichajes exportados con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Error al exportar fichajes: {ex.Message}");
        }
    }

    public async Task Exportar(List<VisitaDTO> visitas)
    {
        try
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Visitas");

            // Cabecera
            var headers = new[]
            {
                "Estado", "Checked-in", "Checked-out",
                "Visitante", "Empresa", "Anfitrión", "Teléfono"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(169, 28, 50));
                ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                ws.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Datos
            var row = 2;
            foreach (var v in visitas)
            {
                ws.Cells[row, 1].Value = v.Estado;
                ws.Cells[row, 2].Value = v.FechaEntrada.ToString("dd/MM/yyyy HH:mm");
                ws.Cells[row, 3].Value = v.FechaSalida.HasValue ? v.FechaSalida.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                ws.Cells[row, 4].Value = v.NombreCompletoVisitante;
                ws.Cells[row, 5].Value = v.EmpresaVisitante ?? "-";
                ws.Cells[row, 6].Value = v.NombreCompletoAnfitrion;
                ws.Cells[row, 7].Value = v.TelefonoVisitante?.ToString() ?? "-";

                // Fila alterna
                if (row % 2 == 0)
                {
                    using var range = ws.Cells[row, 1, row, headers.Length];
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                }

                row++;
            }

            // Bordes y autoajuste
            using var tableRange = ws.Cells[1, 1, row - 1, headers.Length];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            tableRange.AutoFitColumns(15, 40);

            var fileContent = package.GetAsByteArray();
            var fileName = $"Visitas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Convert.ToBase64String(fileContent));

            _toastService.MostrarOk("Visitas exportadas con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Error al exportar visitas: {ex.Message}");
        }
    }
}