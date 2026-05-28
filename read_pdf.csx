using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

var path = @"C:\Users\brisa\Desktop\TFGBrisaSuarez\GestionAccesos\GestionAccesos\wwwroot\pdfs\acuerdo_es.pdf";
using var reader = new PdfReader(path);
using var doc = new PdfDocument(reader);
var page = doc.GetPage(1);
var size = page.GetPageSize();
Console.WriteLine($"Page size: {size.GetWidth()} x {size.GetHeight()}");
var strategy = new LocationTextExtractionStrategy();
PdfTextExtractor.GetTextFromPage(page, strategy);
