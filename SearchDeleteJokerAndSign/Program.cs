using SearchDeleteJokerAndSign;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using SearchDeleteJokerAndSign.Model;

Queue<JokerPagePosition> queue = new Queue<JokerPagePosition>();
string sourceFile = @"C:\oldFile.pdf";
string descFile = @"C:\oldFileWithoutFirstJoker.pdf";

string sourceFile1 = @"C:\oldFileWithoutFirstJoker.pdf";
string descFile1 = @"C:\oldFileWithoutSecondJoker.pdf";

//string sourceFile2 = @"C:\oldFileWithoutSecondJoker.pdf";
//string descFile2 = @"C:\oldFileWithoutThirdJoker.pdf";
string signatureJoker = "#SIGNATURE #";

PDFEdit pdfObj = new PDFEdit();
queue.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile, descFile, signatureJoker));
queue.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile1, descFile1, signatureJoker));

static void signPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream,
                        string keyPassword, string reason, string location, JokerPagePosition jokerPagePosition,
                        bool visibleSignature, string signatureImagePath, string signatureNameField)
{
    Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());
    privateKeyStream.Dispose();

    //then Iterate throught certificate entries to find the private key entry
    string alias = null;
    foreach (string tAlias in pk12.Aliases)
    {
        if (pk12.IsKeyEntry(tAlias))
        {
            alias = tAlias;
            break;
        }
    }
    var pk = pk12.GetKey(alias).Key;

    // reader and stamper    
    PdfReader reader = new PdfReader(sourceDocument);
    using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))

    {
        using (PdfStamper stamper = PdfStamper.CreateSignature(reader, fout, '\0', null, true)) //true for append signature mode
        {

            PdfSignatureAppearance appearance = stamper.SignatureAppearance;
            if (visibleSignature)
            {
                TextLocation position = jokerPagePosition.Position;
                var widthRectangle = 85;
                var heightRectangle = 20;
                iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle(position.X, position.Y - 8, position.X + widthRectangle, position.Y - 8 + heightRectangle); 

                appearance.SetVisibleSignature(rectangle, jokerPagePosition.Page, signatureNameField);
                appearance.SignatureGraphic = iTextSharp.text.Image.GetInstance(signatureImagePath);
                appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
            }
            // digital signature
            IExternalSignature es = new PrivateKeySignature(pk, "SHA256");
            MakeSignature.SignDetached(appearance, es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, CryptoStandard.CMS);

            stamper.Close();
        }
    }
}
using (Stream MyCert = new FileStream(@"C:\public_privatekey.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(descFile1, @"C:\FirstSignature.pdf", MyCert, "/*twayf*/", "", "", queue.Dequeue(), true, @"C:\signature_client.png", "SignatureClient");

}
Thread.Sleep(1000);
using (Stream MyCert = new FileStream(@"C:\cert.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(@"C:\FirstSignature.pdf", @"C:\SecondSignature.pdf", MyCert, "root", "", "", queue.Dequeue(), true, @"C:\signature_société.png", "SignatureCompany");
}

