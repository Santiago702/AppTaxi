using Tesseract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using Microsoft.AspNetCore.Http;

namespace AppTaxi.Servicios
{
    public class ValidacionDocumentos
    {
        private readonly string _tessDataPath;
        private readonly string _outputFolder;

        public ValidacionDocumentos()
        {
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tesseract");
            _outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");

            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
            }
        }

        /// <summary>
        /// Convierte un archivo PDF en imágenes y retorna la lista de rutas de imágenes generadas.
        /// </summary>
        public List<string> ConvertirPdfAImagenes(IFormFile pdfFile)
        {
            var imagenesTemporales = new List<string>();

            // Genera un nombre aleatorio y combínalo con la ruta temporal
            string randomFileName = Path.GetRandomFileName();
            string tempPdfPath = Path.Combine(Path.GetTempPath(), randomFileName);

            using (var stream = new FileStream(tempPdfPath, FileMode.Create))
            {
                pdfFile.CopyTo(stream);
            }

            // Configurar opciones: alta resolución
            var settings = new MagickReadSettings()
            {
                Density = new Density(300) // 300 DPI
            };

            using (var images = new MagickImageCollection())
            {
                images.Read(tempPdfPath, settings);
                int contador = 1;
                foreach (var image in images)
                {
                    // Opcional: optimizar la imagen (mejorar contraste, binarización, etc.)
                    image.AutoOrient();
                    image.Contrast(); // aumenta el contraste
                    image.Normalize(); // mejora la uniformidad de la imagen
                    var tempImagePath = Path.Combine(Path.GetTempPath(), $"pagina_{contador}.png");
                    image.Write(tempImagePath);
                    imagenesTemporales.Add(tempImagePath);
                    contador++;
                }
            }

            // Elimina el archivo PDF temporal
            File.Delete(tempPdfPath);
            return imagenesTemporales;
        }

        public string ProcesarSoloPrimeraPagina(IFormFile pdfFile)
        {
            var imagenes = ConvertirPdfAImagenes(pdfFile);

            if (imagenes.Count == 0)
            {
                return "No se pudo extraer texto, el PDF está vacío o no se pudo procesar.";
            }

            string textoExtraido = ProcesarImagenConOCR(imagenes[0]); // Solo procesa la primera imagen

            // Eliminar solo la primera imagen (o todas si lo deseas)
            File.Delete(imagenes[0]);

            return textoExtraido.Trim();
        }

        /// <summary>
        /// Procesa una imagen con OCR y extrae el texto.
        /// </summary>
        public string ProcesarImagenConOCR(string imagePath)
        {
            using var engine = new TesseractEngine(_tessDataPath, "spa", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            return page.GetText(); // Retorna el texto extraído
        }

        /// <summary>
        /// Procesa un PDF con OCR y devuelve el texto extraído de todas las páginas.
        /// </summary>
        public string ProcesarPdfConOCR(IFormFile pdfFile)
        {
            var imagenes = ConvertirPdfAImagenes(pdfFile);
            string textoExtraido = "";
            int pagina = 1;

            foreach (var imagen in imagenes)
            {
                textoExtraido += $"--- Página {pagina} ---\n";
                textoExtraido += ProcesarImagenConOCR(imagen) + "\n";
                //File.Delete(imagen); // Eliminar la imagen temporal
                pagina++;
            }

            return textoExtraido.Trim();
        }

        /// <summary>
        /// Verifica si un texto contiene ciertas palabras con los operadores 'Y' u 'O'.
        /// </summary>
        public bool Contiene(string texto, string[] palabras, char operador)
        {
            return operador switch
            {
                'Y' => palabras.All(palabra => texto.Contains(palabra, StringComparison.OrdinalIgnoreCase)),
                'O' => palabras.Any(palabra => texto.Contains(palabra, StringComparison.OrdinalIgnoreCase)),
                _ => throw new ArgumentException("El operador debe ser 'Y' o 'O'.")
            };
        }
    }
}
