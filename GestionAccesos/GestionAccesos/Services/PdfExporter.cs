using GestionAccesos.DTO;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using MediatR;
using Microsoft.JSInterop;

namespace GestionAccesos.Services;

public class PdfExporter
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IToastService _toastService;
    private readonly IMediator _mediator;

    public PdfExporter(IJSRuntime jsRuntime, IToastService toastService, IMediator mediator)
    {
        _jsRuntime = jsRuntime;
        _toastService = toastService;
        _mediator = mediator;
    }

    public async Task ExportarAcuerdo(string idioma, string nombreVisitante, string firmaBase64)
    {
        try
        {
            var pdfBytes = await ModificarPdfConCampos(idioma, nombreVisitante, firmaBase64);

            var fileName = $"Acuerdo_{nombreVisitante}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            _toastService.MostrarOk("Acuerdo firmado exportado con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Ocurrió un error al exportar el acuerdo: {ex.Message}");
        }
    }

    private Task<byte[]> ModificarPdfConCampos(string idioma, string nombreVisitante, string firmaBase64)
    {
        if (firmaBase64.StartsWith("data:image/png;base64,"))
        {
            firmaBase64 = firmaBase64["data:image/png;base64,".Length..];
        }

        if (string.IsNullOrWhiteSpace(firmaBase64))
        {
            throw new ArgumentException("La firma no está en un formato válido.");
        }

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromBase64String(firmaBase64);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("La firma no tiene un formato Base64 válido.", ex);
        }

        try
        {
            ImageDataFactory.Create(signatureBytes);
        }
        catch (IOException ex)
        {
            throw new ArgumentException("No se pudo crear la imagen a partir de los datos Base64.", ex);
        }

        string pdfPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "pdfs",
            idioma switch
            {
                "es" => "acuerdo_es.pdf",
                "en" => "acuerdo_en.pdf",
                "fr" => "acuerdo_fr.pdf",
                "ar" => "acuerdo_ar.pdf",
                _ => "acuerdo_es.pdf"
            });

        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException("No se encontró el archivo PDF en la ruta especificada.", pdfPath);
        }

        var translations = new Dictionary<string, string>
        {
            ["visitante"] = idioma switch
            {
                "es" => "Visitante",
                "en" => "Visitor",
                "fr" => "Visiteur",
                "ar" => "Visitor",
                _ => "Visitante"
            },
            ["fecha_firma"] = idioma switch
            {
                "es" => "Fecha de firma",
                "en" => "Date of signature",
                "fr" => "Date de signature",
                "ar" => "Date of signature",
                _ => "Fecha de firma"
            }
        };

        using var memoryStream = new MemoryStream();
        using var reader = new PdfReader(pdfPath);
        using var writer = new PdfWriter(memoryStream);
        using var pdfDoc = new PdfDocument(reader, writer);

        int pageIndex = 1;
        int totalPages = pdfDoc.GetNumberOfPages();

        if (pageIndex < 1 || pageIndex > totalPages)
        {
            throw new InvalidOperationException(
                $"La página {pageIndex} no existe en el PDF. Total de páginas: {totalPages}.");
        }

        var page = pdfDoc.GetPage(pageIndex);
        var canvas = new PdfCanvas(page);
        var font = iText.Kernel.Font.PdfFontFactory.CreateFont(
            iText.IO.Font.Constants.StandardFonts.HELVETICA,
            iText.IO.Font.PdfEncodings.CP1252,
            iText.Kernel.Font.PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

        // Coordenadas de los campos de texto
        float valueX  = 160f;
        float nombreY = 285f;
        float fechaY  = 248f;

        // Dimensiones máximas del recuadro de firma (200×55 puntos)
        float firmaImgW  = 200f;
        float firmaImgH  = 55f;
        float firmaLineY = 185f;

        var nombreSafe = nombreVisitante ?? string.Empty;
        var fechaFirma = DateTime.Now.ToString("dd/MM/yyyy");

        // Nombre del visitante
        canvas.BeginText()
              .SetFontAndSize(font, 10)
              .MoveText(valueX, nombreY)
              .ShowText(nombreSafe)
              .EndText();

        // Fecha de firma
        canvas.BeginText()
              .SetFontAndSize(font, 10)
              .MoveText(valueX, fechaY)
              .ShowText(fechaFirma)
              .EndText();

        // Imagen de firma — escalada proporcionalmente para caber en firmaImgW × firmaImgH
        var imageData = ImageDataFactory.Create(signatureBytes);
        var xObject   = new iText.Kernel.Pdf.Xobject.PdfImageXObject(imageData);
        float imgW    = imageData.GetWidth();
        float imgH    = imageData.GetHeight();
        float scale   = Math.Min(firmaImgW / imgW, firmaImgH / imgH);
        float scaledW = imgW * scale;
        float scaledH = imgH * scale;
        float imgStartX = valueX;
        canvas.AddXObjectWithTransformationMatrix(
            xObject, scaledW, 0, 0, scaledH,
            imgStartX, firmaLineY - scaledH + 4f);

        pdfDoc.Close();

        return Task.FromResult(memoryStream.ToArray());
    }

    public async Task<byte[]> GetPdfContent(string idioma, string nombreVisitante, string firmaBase64)
    {
        try
        {
            return await ModificarPdfConCampos(idioma, nombreVisitante, firmaBase64);
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"PDF ERROR: {ex}");
            throw;
        }
    }

    public async Task ExportarVisitasPdf(List<VisitaDTO> visitas)
    {
        try
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

            doc.SetMargins(30, 30, 30, 30);

            var fontRegular = PdfFontFactory.CreateFont(
                iText.IO.Font.Constants.StandardFonts.HELVETICA,
                iText.IO.Font.PdfEncodings.CP1252,
                PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            var fontBold = PdfFontFactory.CreateFont(
                iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD,
                iText.IO.Font.PdfEncodings.CP1252,
                PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            var rojo = new DeviceRgb(169, 28, 50);
            var grisClaro = new DeviceRgb(248, 250, 252);
            var blanco = new DeviceRgb(255, 255, 255);
            var textoOscuro = new DeviceRgb(15, 23, 42);
            var textoGris = new DeviceRgb(100, 116, 139);

            // Título
            var titulo = new Paragraph("Informe de Visitas")
                .SetFont(fontBold)
                .SetFontSize(18)
                .SetFontColor(textoOscuro)
                .SetMarginBottom(4);
            doc.Add(titulo);

            var subtitulo = new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}  ·  {visitas.Count} visita{(visitas.Count == 1 ? "" : "s")}")
                .SetFont(fontRegular)
                .SetFontSize(9)
                .SetFontColor(textoGris)
                .SetMarginBottom(16);
            doc.Add(subtitulo);

            // Tabla
            var headers = new[] { "Estado", "Checked-in", "Checked-out", "Visitante", "Empresa", "Anfitrión", "Teléfono" };
            var colWidths = new float[] { 70f, 95f, 95f, 130f, 110f, 130f, 80f };

            var table = new Table(UnitValue.CreatePointArray(colWidths))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            // Cabecera
            foreach (var h in headers)
            {
                table.AddHeaderCell(
                    new Cell()
                        .SetBackgroundColor(rojo)
                        .SetBorder(Border.NO_BORDER)
                        .SetPaddingTop(7).SetPaddingBottom(7)
                        .SetPaddingLeft(8).SetPaddingRight(8)
                        .Add(new Paragraph(h)
                            .SetFont(fontBold)
                            .SetFontSize(8)
                            .SetFontColor(blanco)
                            .SetMargin(0)));
            }

            // Filas
            for (var i = 0; i < visitas.Count; i++)
            {
                var v = visitas[i];
                var bg = i % 2 == 0 ? blanco : grisClaro;

                var valores = new[]
                {
                    v.Estado,
                    v.FechaEntrada.ToString("dd/MM/yyyy HH:mm"),
                    v.FechaSalida.HasValue ? v.FechaSalida.Value.ToString("dd/MM/yyyy HH:mm") : "-",
                    v.NombreCompletoVisitante,
                    v.EmpresaVisitante ?? "-",
                    v.NombreCompletoAnfitrion,
                    v.TelefonoVisitante?.ToString() ?? "-"
                };

                foreach (var val in valores)
                {
                    table.AddCell(
                        new Cell()
                            .SetBackgroundColor(bg)
                            .SetBorder(Border.NO_BORDER)
                            .SetBorderBottom(new SolidBorder(new DeviceRgb(226, 232, 240), 0.5f))
                            .SetPaddingTop(6).SetPaddingBottom(6)
                            .SetPaddingLeft(8).SetPaddingRight(8)
                            .Add(new Paragraph(val)
                                .SetFont(fontRegular)
                                .SetFontSize(8)
                                .SetFontColor(textoOscuro)
                                .SetMargin(0)));
                }
            }

            doc.Add(table);
            doc.Close();

            var fileName = $"Visitas_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "application/pdf",
                Convert.ToBase64String(ms.ToArray()));

            _toastService.MostrarOk("Visitas exportadas a PDF con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Error al exportar PDF: {ex.Message}");
        }
    }

    public async Task ExportarFichajesPdf(List<FichajesTrabajadorDTO> fichajes)
    {
        try
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
            doc.SetMargins(30, 30, 30, 30);

            var fontR = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA, iText.IO.Font.PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            var fontB = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD, iText.IO.Font.PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            var rojo = new DeviceRgb(169, 28, 50);
            var gris = new DeviceRgb(248, 250, 252);
            var blanco = new DeviceRgb(255, 255, 255);
            var dark = new DeviceRgb(15, 23, 42);
            var muted = new DeviceRgb(100, 116, 139);

            doc.Add(new Paragraph("Informe de Fichajes").SetFont(fontB).SetFontSize(18).SetFontColor(dark).SetMarginBottom(4));
            doc.Add(new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}  ·  {fichajes.Count} fichaje{(fichajes.Count == 1 ? "" : "s")}").SetFont(fontR).SetFontSize(9).SetFontColor(muted).SetMarginBottom(16));

            var headers = new[] { "ID", "Trabajador", "Entrada", "Salida", "Duración", "Estado" };
            var widths = new float[] { 40f, 180f, 110f, 110f, 80f, 70f };

            var table = new Table(UnitValue.CreatePointArray(widths)).SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var h in headers)
                table.AddHeaderCell(new Cell().SetBackgroundColor(rojo).SetBorder(Border.NO_BORDER).SetPaddingTop(7).SetPaddingBottom(7).SetPaddingLeft(8).SetPaddingRight(8)
                    .Add(new Paragraph(h).SetFont(fontB).SetFontSize(8).SetFontColor(blanco).SetMargin(0)));

            for (var i = 0; i < fichajes.Count; i++)
            {
                var f = fichajes[i];
                var bg = i % 2 == 0 ? blanco : gris;
                var duracion = f.HoraEntrada.HasValue
                    ? $"{(int)((f.HoraSalida ?? DateTime.Now) - f.HoraEntrada.Value).TotalHours}h {((f.HoraSalida ?? DateTime.Now) - f.HoraEntrada.Value).Minutes:D2}m"
                    : "-";

                var valores = new[] {
                    f.IdFichaje.ToString(),
                    f.NombreCompleto,
                    f.HoraEntrada.HasValue ? f.HoraEntrada.Value.ToString("dd/MM/yyyy HH:mm") : "-",
                    f.HoraSalida.HasValue ? f.HoraSalida.Value.ToString("dd/MM/yyyy HH:mm") : "-",
                    duracion,
                    f.HoraSalida.HasValue ? "Cerrado" : "Abierto"
                };

                foreach (var v in valores)
                    table.AddCell(new Cell().SetBackgroundColor(bg).SetBorder(Border.NO_BORDER)
                        .SetBorderBottom(new SolidBorder(new DeviceRgb(226, 232, 240), 0.5f))
                        .SetPaddingTop(6).SetPaddingBottom(6).SetPaddingLeft(8).SetPaddingRight(8)
                        .Add(new Paragraph(v).SetFont(fontR).SetFontSize(8).SetFontColor(dark).SetMargin(0)));
            }

            doc.Add(table);
            doc.Close();

            await _jsRuntime.InvokeVoidAsync("downloadFile", $"Fichajes_{DateTime.Now:yyyyMMdd_HHmm}.pdf", "application/pdf", Convert.ToBase64String(ms.ToArray()));
            _toastService.MostrarOk("Fichajes exportados a PDF con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Error al exportar PDF: {ex.Message}");
        }
    }

    public async Task ExportarInformeHorasPdf(List<FichajesTrabajadorDTO> fichajes, List<AusenciasTrabajadorDTO> ausencias, DateTime desde, DateTime hasta, List<TrabajadoresDTO>? trabajadoresExplicitos = null)
    {
        try
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            doc.SetMargins(40, 40, 40, 40);

            var fontR = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA, iText.IO.Font.PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            var fontB = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD, iText.IO.Font.PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            var fontI = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE, iText.IO.Font.PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            // Colores
            var azulOscuro  = new DeviceRgb(37,  99, 235);
            var azulClaro   = new DeviceRgb(219, 234, 254);
            var verdeOscuro = new DeviceRgb(22,  101, 52);
            var verdeClaro  = new DeviceRgb(220, 252, 231);
            var naranjaOsc  = new DeviceRgb(180,  75,  0);
            var naranjaClr  = new DeviceRgb(255, 237, 213);
            var azulHeader  = new DeviceRgb(59,  130, 246);
            var verdeHeader = new DeviceRgb(34,  197, 94);
            var blanco      = new DeviceRgb(255, 255, 255);
            var grisClaro   = new DeviceRgb(248, 250, 252);
            var grisBorde   = new DeviceRgb(226, 232, 240);
            var dark        = new DeviceRgb(15,  23,  42);
            var muted       = new DeviceRgb(100, 116, 139);
            var rojoCard    = new DeviceRgb(239, 68,  68);
            var rojoBg      = new DeviceRgb(254, 226, 226);
            var amarilloOsc = new DeviceRgb(146, 64,  14);
            var amarilloBg  = new DeviceRgb(254, 243, 199);

            const double LimiteHorasDiarias = 9.0;

            // Agrupar datos
            var fichajesPorTrabajador = fichajes
                .GroupBy(f => new { f.IdTrabajador, f.NombreCompleto })
                .ToDictionary(g => g.Key.IdTrabajador, g => (nombre: g.Key.NombreCompleto, lista: g.OrderBy(f => f.HoraEntrada).ToList()));

            var ausenciasPorTrabajador = ausencias
                .GroupBy(a => a.IdTrabajador)
                .ToDictionary(g => g.Key, g => g.OrderBy(a => a.HoraInicio).ToList());

            var todosIds = fichajesPorTrabajador.Keys
                .Union(ausenciasPorTrabajador.Keys)
                .Union(trabajadoresExplicitos?.Select(t => t.IdTrabajador) ?? Enumerable.Empty<int>())
                .Distinct().OrderBy(id => id).ToList();

            // Mapa nombre por id para trabajadores explícitos sin datos
            var nombrePorId = trabajadoresExplicitos?.ToDictionary(t => t.IdTrabajador,
                t => $"{t.Nombre} {t.Apellido1} {t.Apellido2}".Trim())
                ?? new Dictionary<int, string>();
            var departamentoPorId = trabajadoresExplicitos?.ToDictionary(t => t.IdTrabajador,
                t => t.Departamento ?? "-")
                ?? new Dictionary<int, string>();

            bool primerTrabajador = true;

            foreach (var idTrabajador in todosIds)
            {
                if (!primerTrabajador)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                primerTrabajador = false;

                fichajesPorTrabajador.TryGetValue(idTrabajador, out var fData);
                var nombreTrabajador = fData.nombre
                    ?? ausenciasPorTrabajador.GetValueOrDefault(idTrabajador)?.FirstOrDefault()?.NombreCompleto
                    ?? nombrePorId.GetValueOrDefault(idTrabajador)
                    ?? $"Trabajador {idTrabajador}";
                var fichajesLista = fData.lista ?? new List<FichajesTrabajadorDTO>();
                var ausenciasLista = ausenciasPorTrabajador.GetValueOrDefault(idTrabajador) ?? new List<AusenciasTrabajadorDTO>();
                var depto = fichajesLista.FirstOrDefault()?.Departamento
                    ?? departamentoPorId.GetValueOrDefault(idTrabajador)
                    ?? "-";
                var turno = fichajesLista.FirstOrDefault()?.Observaciones
                    ?? trabajadoresExplicitos?.FirstOrDefault(t => t.IdTrabajador == idTrabajador)?.Observaciones
                    ?? "-";

                // ── CABECERA ────────────────────────────────────────────
                var headerTable = new Table(new float[] { 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(0);
                headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(0)
                    .Add(new Paragraph("OnSite").SetFont(fontB).SetFontSize(20).SetFontColor(azulOscuro).SetMargin(0))
                    .Add(new Paragraph("Informe de horas y ausencias").SetFont(fontR).SetFontSize(8).SetFontColor(muted).SetMargin(0)));
                var cellDer = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).SetPaddingBottom(0);
                cellDer.Add(new Paragraph($"Período:    {desde:MMMM yyyy}").SetFont(fontR).SetFontSize(9).SetFontColor(dark).SetMargin(0));
                cellDer.Add(new Paragraph($"Generado:  {DateTime.Now:dd/MM/yyyy HH:mm}").SetFont(fontR).SetFontSize(9).SetFontColor(muted).SetMargin(0));
                headerTable.AddCell(cellDer);
                doc.Add(headerTable);
                doc.Add(new Table(1).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(6).SetMarginBottom(12)
                    .AddCell(new Cell().SetHeight(1).SetBackgroundColor(grisBorde).SetBorder(Border.NO_BORDER).SetPadding(0)));

                // ── DATOS DEL TRABAJADOR ────────────────────────────────
                doc.Add(new Paragraph("Datos del trabajador").SetFont(fontB).SetFontSize(11).SetFontColor(azulOscuro).SetMarginBottom(4));
                var infoTable = new Table(new float[] { 1, 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(12);
                void AddInfoCell(Table t, string label, string value, bool header = false)
                {
                    var cell = new Cell().SetBorder(Border.NO_BORDER)
                        .SetBorderBottom(new SolidBorder(grisBorde, 0.5f))
                        .SetPaddingTop(4).SetPaddingBottom(4).SetPaddingLeft(6).SetPaddingRight(6);
                    if (header) cell.SetBackgroundColor(grisClaro);
                    cell.Add(new Paragraph(label).SetFont(fontR).SetFontSize(7).SetFontColor(muted).SetMargin(0));
                    cell.Add(new Paragraph(value).SetFont(fontB).SetFontSize(9).SetFontColor(dark).SetMargin(0));
                    t.AddCell(cell);
                }
                AddInfoCell(infoTable, "Nombre", nombreTrabajador);
                AddInfoCell(infoTable, "ID empleado", $"EMP-{idTrabajador:D4}");
                AddInfoCell(infoTable, "Departamento", depto);
                AddInfoCell(infoTable, "Turno", turno);
                doc.Add(infoTable);

                // ── RESUMEN ─────────────────────────────────────────────
                var horasTrabMin = fichajesLista
                    .Where(f => f.HoraEntrada.HasValue && f.HoraSalida.HasValue)
                    .Sum(f => (f.HoraSalida!.Value - f.HoraEntrada!.Value).TotalMinutes);
                var horasAusMin = ausenciasLista
                    .Where(a => a.HoraFin.HasValue)
                    .Sum(a => (a.HoraFin!.Value - a.HoraInicio).TotalMinutes);
                var totalMin = horasTrabMin + horasAusMin;

                static string Fmt(double mins) => $"{(int)(mins / 60)}h {(int)(mins % 60):D2}m";

                doc.Add(new Paragraph("Resumen del período").SetFont(fontB).SetFontSize(11).SetFontColor(azulOscuro).SetMarginBottom(4));
                var resumenTable = new Table(new float[] { 1, 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(14);

                void AddResumenCard(Table t, string titulo, string valor, string subtitulo, DeviceRgb colorVal, DeviceRgb bgCard)
                {
                    var c = new Cell().SetBorder(new SolidBorder(grisBorde, 1)).SetBorderRadius(new BorderRadius(6))
                        .SetPaddingTop(10).SetPaddingBottom(10).SetPaddingLeft(10).SetPaddingRight(10)
                        .SetBackgroundColor(bgCard);
                    c.Add(new Paragraph(titulo).SetFont(fontR).SetFontSize(8).SetFontColor(muted).SetMargin(0).SetMarginBottom(4));
                    c.Add(new Paragraph(valor).SetFont(fontB).SetFontSize(16).SetFontColor(colorVal).SetMargin(0));
                    c.Add(new Paragraph(subtitulo).SetFont(fontR).SetFontSize(7).SetFontColor(muted).SetMargin(0).SetMarginTop(2));
                    t.AddCell(c);
                }

                AddResumenCard(resumenTable, "Horas trabajadas", Fmt(horasTrabMin), "Fichajes efectivos", dark, grisClaro);
                AddResumenCard(resumenTable, "Horas de ausencia", Fmt(horasAusMin), "Justificadas", naranjaOsc, naranjaClr);
                AddResumenCard(resumenTable, "Total cómputo", Fmt(totalMin), "Trabajo + ausencia", dark, grisClaro);
                AddResumenCard(resumenTable, "Balance vs contrato", "-", "Contrato: -", verdeOscuro, verdeClaro);
                doc.Add(resumenTable);

                // ── DETALLE DE FICHAJES ─────────────────────────────────
                doc.Add(new Paragraph("Detalle de fichajes").SetFont(fontB).SetFontSize(11).SetFontColor(azulOscuro).SetMarginBottom(4));

                var colW = new float[] { 65f, 52f, 70f, 52f, 80f, 45f, 50f, 65f };
                var tFich = new Table(UnitValue.CreatePointArray(colW)).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(12);

                Cell HdrCell(string txt, DeviceRgb bg) =>
                    new Cell().SetBackgroundColor(bg).SetBorder(Border.NO_BORDER)
                        .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5)
                        .Add(new Paragraph(txt).SetFont(fontB).SetFontSize(7).SetFontColor(blanco).SetMargin(0));

                tFich.AddHeaderCell(HdrCell("Día", azulHeader));
                tFich.AddHeaderCell(HdrCell("Entrada", azulHeader));
                tFich.AddHeaderCell(HdrCell("Salida", azulHeader));
                tFich.AddHeaderCell(HdrCell("H. trabajo", azulHeader));
                tFich.AddHeaderCell(HdrCell("Ausencia", azulHeader));
                tFich.AddHeaderCell(HdrCell("H. ausencia", azulHeader));
                tFich.AddHeaderCell(HdrCell("Total día", azulHeader));
                tFich.AddHeaderCell(HdrCell("Estado", azulHeader));

                // Construir filas día a día
                var diasRango = Enumerable.Range(0, (hasta - desde).Days + 1)
                    .Select(d => desde.AddDays(d))
                    .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    .ToList();

                double totalFichajesDiaMin = 0, totalAusenciasDiaMin = 0;

                for (int rowIdx = 0; rowIdx < diasRango.Count; rowIdx++)
                {
                    var dia = diasRango[rowIdx];
                    var fichajesDia = fichajesLista
                        .Where(f => f.HoraEntrada.HasValue && f.HoraEntrada.Value.Date == dia.Date)
                        .OrderBy(f => f.HoraEntrada).ToList();
                    var ausenciasDia = ausenciasLista
                        .Where(a => a.HoraInicio.Date == dia.Date || (a.HoraFin.HasValue && a.HoraFin.Value.Date == dia.Date))
                        .ToList();

                    var horasTrabDia = fichajesDia
                        .Where(f => f.HoraSalida.HasValue)
                        .Sum(f => (f.HoraSalida!.Value - f.HoraEntrada!.Value).TotalMinutes);
                    var horasAusDia = ausenciasDia
                        .Where(a => a.HoraFin.HasValue)
                        .Sum(a => (a.HoraFin!.Value - a.HoraInicio).TotalMinutes);
                    var totalDia = horasTrabDia + horasAusDia;

                    totalFichajesDiaMin += horasTrabDia;
                    totalAusenciasDiaMin += horasAusDia;

                    string estado, ausNombre;
                    DeviceRgb bgFila, estadoColor;

                    if (!fichajesDia.Any() && ausenciasDia.Any())
                    {
                        estado = "Ausencia total"; bgFila = rojoBg; estadoColor = rojoCard;
                        ausNombre = ausenciasDia.First().DescripcionMotivo;
                    }
                    else if (fichajesDia.Any() && ausenciasDia.Any())
                    {
                        estado = totalDia > LimiteHorasDiarias * 60 ? "Revisión" : "Ausencia parcial";
                        bgFila = totalDia > LimiteHorasDiarias * 60 ? amarilloBg : naranjaClr;
                        estadoColor = totalDia > LimiteHorasDiarias * 60 ? amarilloOsc : naranjaOsc;
                        ausNombre = ausenciasDia.First().DescripcionMotivo;
                    }
                    else if (fichajesDia.Any())
                    {
                        if (totalDia > LimiteHorasDiarias * 60)
                        {
                            estado = "Revisión"; bgFila = amarilloBg; estadoColor = amarilloOsc;
                        }
                        else
                        {
                            estado = "Correcto"; bgFila = rowIdx % 2 == 0 ? blanco : grisClaro; estadoColor = verdeOscuro;
                        }
                        ausNombre = "—";
                    }
                    else
                    {
                        estado = "-"; bgFila = rowIdx % 2 == 0 ? blanco : grisClaro; estadoColor = muted;
                        ausNombre = "—";
                    }

                    // Entrada y salida: si hay múltiples fichajes los concatena
                    string entradaStr, salidaStr;
                    if (!fichajesDia.Any())
                    { entradaStr = "—"; salidaStr = "—"; }
                    else if (fichajesDia.Count == 1)
                    {
                        entradaStr = fichajesDia[0].HoraEntrada?.ToString("HH:mm") ?? "—";
                        salidaStr  = fichajesDia[0].HoraSalida?.ToString("HH:mm") ?? "(abierto)";
                    }
                    else
                    {
                        entradaStr = fichajesDia[0].HoraEntrada?.ToString("HH:mm") ?? "—";
                        salidaStr  = string.Join("\n", fichajesDia.Select(f =>
                            $"{f.HoraEntrada?.ToString("HH:mm") ?? "?"}-{f.HoraSalida?.ToString("HH:mm") ?? "?"}"));
                    }

                    Cell DataCell(string txt, bool bold = false, TextAlignment align = TextAlignment.LEFT) =>
                        new Cell().SetBackgroundColor(bgFila).SetBorder(Border.NO_BORDER)
                            .SetBorderBottom(new SolidBorder(grisBorde, 0.4f))
                            .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5)
                            .Add(new Paragraph(txt).SetFont(bold ? fontB : fontR).SetFontSize(8).SetFontColor(dark).SetMargin(0).SetTextAlignment(align));

                    tFich.AddCell(DataCell($"{dia:ddd dd/MM}", true));
                    tFich.AddCell(DataCell(entradaStr));
                    tFich.AddCell(DataCell(salidaStr));
                    tFich.AddCell(DataCell(horasTrabDia > 0 ? Fmt(horasTrabDia) : "—"));
                    tFich.AddCell(DataCell(ausNombre));
                    tFich.AddCell(DataCell(horasAusDia > 0 ? Fmt(horasAusDia) : "—"));
                    tFich.AddCell(DataCell(totalDia > 0 ? Fmt(totalDia) : "—", true));
                    tFich.AddCell(new Cell().SetBackgroundColor(bgFila).SetBorder(Border.NO_BORDER)
                        .SetBorderBottom(new SolidBorder(grisBorde, 0.4f))
                        .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5)
                        .Add(new Paragraph(estado).SetFont(fontB).SetFontSize(7).SetFontColor(estadoColor).SetMargin(0)));
                }

                // Fila TOTAL
                var totalDiaMin = totalFichajesDiaMin + totalAusenciasDiaMin;
                Cell TotCell(string txt, bool bold = true) =>
                    new Cell().SetBackgroundColor(grisClaro).SetBorder(Border.NO_BORDER)
                        .SetBorderTop(new SolidBorder(grisBorde, 1f))
                        .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5)
                        .Add(new Paragraph(txt).SetFont(bold ? fontB : fontR).SetFontSize(8).SetFontColor(dark).SetMargin(0));
                tFich.AddCell(TotCell("TOTAL"));
                tFich.AddCell(TotCell(""));
                tFich.AddCell(TotCell(""));
                tFich.AddCell(TotCell(Fmt(totalFichajesDiaMin)));
                tFich.AddCell(TotCell(""));
                tFich.AddCell(TotCell(Fmt(totalAusenciasDiaMin)));
                tFich.AddCell(TotCell(Fmt(totalDiaMin)));
                tFich.AddCell(TotCell(""));
                doc.Add(tFich);

                // ── DETALLE DE AUSENCIAS ────────────────────────────────
                if (ausenciasLista.Any())
                {
                    doc.Add(new Paragraph("Detalle de ausencias").SetFont(fontB).SetFontSize(11).SetFontColor(azulOscuro).SetMarginBottom(4));
                    var colWA = new float[] { 80f, 100f, 120f, 60f, 80f };
                    var tAus = new Table(UnitValue.CreatePointArray(colWA)).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(14);

                    tAus.AddHeaderCell(HdrCell("Fecha", verdeHeader));
                    tAus.AddHeaderCell(HdrCell("Tipo", verdeHeader));
                    tAus.AddHeaderCell(HdrCell("Duración", verdeHeader));
                    tAus.AddHeaderCell(HdrCell("Justif.", verdeHeader));
                    tAus.AddHeaderCell(HdrCell("Estado", verdeHeader));

                    for (int i = 0; i < ausenciasLista.Count; i++)
                    {
                        var a = ausenciasLista[i];
                        var bgA = i % 2 == 0 ? blanco : grisClaro;
                        var durMin = a.HoraFin.HasValue ? (a.HoraFin.Value - a.HoraInicio).TotalMinutes : 0;
                        string durDesc;
                        if (durMin >= 60 * 7) durDesc = "Día completo";
                        else if (a.HoraInicio.Hour >= 12) durDesc = $"Parcial (tarde)";
                        else durDesc = $"Parcial ({(int)(durMin / 60)}h {(int)(durMin % 60):D2}min)";

                        Cell AusCell(string txt, DeviceRgb? color = null) =>
                            new Cell().SetBackgroundColor(bgA).SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(new SolidBorder(grisBorde, 0.4f))
                                .SetPaddingTop(5).SetPaddingBottom(5).SetPaddingLeft(5).SetPaddingRight(5)
                                .Add(new Paragraph(txt).SetFont(fontR).SetFontSize(8).SetFontColor(color ?? dark).SetMargin(0));

                        tAus.AddCell(AusCell(a.HoraInicio.ToString("dd/MM/yyyy")));
                        tAus.AddCell(AusCell(a.DescripcionMotivo ?? "-"));
                        tAus.AddCell(AusCell(durDesc));
                        tAus.AddCell(AusCell("Justificada", verdeOscuro));
                        tAus.AddCell(AusCell("Aprobada", azulOscuro));
                    }
                    doc.Add(tAus);
                }

                // ── NOTA LEGAL ──────────────────────────────────────────
                doc.Add(new Table(1).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(6)
                    .AddCell(new Cell().SetHeight(1).SetBackgroundColor(grisBorde).SetBorder(Border.NO_BORDER).SetPadding(0)));
                doc.Add(new Paragraph(
                    "El cómputo total incluye horas de trabajo efectivo más horas de ausencia justificada, garantizando que el balance refleja la jornada completa del trabajador. " +
                    "Las ausencias parciales (cita médica, gestiones) se registran como gap dentro del fichaje del día y se suman al total de forma diferenciada. " +
                    "Límite legal en España: 40h semanales / 9h diarias / 80h extra anuales (ET art. 34-35).")
                    .SetFont(fontI).SetFontSize(7).SetFontColor(muted).SetMarginBottom(20));

                // ── FIRMAS ──────────────────────────────────────────────
                doc.Add(new Paragraph("Firmas y validación").SetFont(fontB).SetFontSize(11).SetFontColor(azulOscuro).SetMarginBottom(12));
                var firmaTable = new Table(new float[] { 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100));
                void AddFirmaCol(Table t, string label, string value)
                {
                    var c = new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(4).SetPaddingBottom(4);
                    c.Add(new Paragraph(label).SetFont(fontR).SetFontSize(8).SetFontColor(muted).SetMargin(0).SetMarginBottom(20));
                    c.Add(new Table(1).SetWidth(UnitValue.CreatePercentValue(100)).SetMarginBottom(4)
                        .AddCell(new Cell().SetHeight(1).SetBackgroundColor(dark).SetBorder(Border.NO_BORDER).SetPadding(0)));
                    c.Add(new Paragraph(value).SetFont(fontR).SetFontSize(8).SetFontColor(dark).SetMargin(0));
                    t.AddCell(c);
                }
                AddFirmaCol(firmaTable, "Trabajador", nombreTrabajador);
                AddFirmaCol(firmaTable, "Responsable RRHH", "Nombre y sello");
                AddFirmaCol(firmaTable, "Fecha de validación", "__/__/______");
                doc.Add(firmaTable);
            }

            doc.Close();

            await _jsRuntime.InvokeVoidAsync("downloadFile", $"InformeHoras_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf", "application/pdf", Convert.ToBase64String(ms.ToArray()));
            _toastService.MostrarOk("Informe de horas exportado con éxito.");
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Error al exportar informe: {ex.Message}");
        }
    }

    public async Task DescargarAcuerdoFirmado(byte[] archivo)
    {
        try
        {
            if (archivo != null)
            {
                var archivoBase64 = Convert.ToBase64String(archivo);

                var fileName = $"Acuerdo_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "application/pdf", archivoBase64);

                _toastService.MostrarOk("Acuerdo descargado con éxito.");
            }
            else
            {
                _toastService.MostrarError("No se encontró el archivo del acuerdo firmado.");
            }
        }
        catch (Exception ex)
        {
            _toastService.MostrarError($"Ocurrió un error al intentar descargar el acuerdo: {ex.Message}");
        }
    }
}