using iText.PdfCleanup;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using SearchDeleteJokerAndSign.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchDeleteJokerAndSign
{
    public class PDFEdit
    {
        /// <summary>
        /// Find the text and replace in PDF
        /// </summary>
        /// <param name="sourceFile">The source PDF file where text to be searched</param>
        /// <param name="newFile">The new destination PDF file which will be saved with replaced text</param>
        /// <param name="textToBeSearched">The text to be searched in the PDF</param>
        public JokerPagePosition FindAndReplaceFirstJokerWithSignatureFrame(string sourceFile, string newFile, string textToBeSearched)
        {
            return ReplaceFirstJokerWithSignatureFrame(textToBeSearched, newFile, sourceFile);
        }
        private JokerPagePosition ReplaceFirstJokerWithSignatureFrame(string textToBeSearched, string outputFilePath, string inputFilePath)
        {
            TextLocation position = null;
            JokerPagePosition jokerPagePosition = new JokerPagePosition();
            try
            {
                using (Stream inputPdfStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream inputImageStream = new FileStream(@"C:\signature_client.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outputPdfStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (Stream outputPdfStream2 = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //Opens the unmodified PDF for reading
                    PdfReader reader = new PdfReader(inputPdfStream);

                    //Creates a stamper to put an image on the original pdf
                    PdfStamper stamper = new PdfStamper(reader, outputPdfStream); 
                    for (var i = 1; i <= reader.NumberOfPages; i++)
                    {
                        var parser = new PdfReaderContentParser(reader);
                        var strategy = parser.ProcessContent(i, new LocationTextExtractionStrategyWithPosition());
                        var res = strategy.GetLocations();
                        var searchResult = res.Where(p => p.Text.Contains(textToBeSearched)).ToList();
                        if (searchResult.Count > 0)
                        {
                            Console.WriteLine(searchResult[0].Text);
                            Console.WriteLine(searchResult[0].X);
                            Console.WriteLine(searchResult[0].Y);
                            position = searchResult[0];

                            //Delete joker
                            IList<iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpLocation> cleanUpLocations = new List<iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpLocation>();
                            cleanUpLocations.Add(new iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpLocation(i, new iTextSharp.text.Rectangle(position.X, position.Y - 8, position.X + 85, position.Y - 8 + 20), iTextSharp.text.BaseColor.WHITE));
                            iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpProcessor cleaner = new iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpProcessor(cleanUpLocations, stamper);
                            cleaner.CleanUp();
                            
                            //Add Signature frame
                            jokerPagePosition.Position = position;
                            jokerPagePosition.Page = i;
                            Bitmap transparentBitmap = new Bitmap(85, 20);
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(transparentBitmap,BaseColor.WHITE);
                            image.BorderColor = BaseColor.BLACK;
                            image.BorderWidth = 0.2f;
                            image.Border = iTextSharp.text.Image.LEFT_BORDER | iTextSharp.text.Image.TOP_BORDER | iTextSharp.text.Image.RIGHT_BORDER | iTextSharp.text.Image.BOTTOM_BORDER;
                            image.SetAbsolutePosition(position.X, (position.Y - 8));                                                                                 
                            stamper.GetOverContent(i).AddImage(image, true); // i stands for the page no.
                     
                        }                      
                    }
                    stamper.Close();
                    
                    //get the joker page and position 
                    return jokerPagePosition;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
