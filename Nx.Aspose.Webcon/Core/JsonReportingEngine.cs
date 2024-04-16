namespace Nx.Aspose.Webcon.Core
{
    using global::Aspose.Words;
    using global::Aspose.Words.Reporting;
    using System;
    using System.IO;

    public class JsonReportingEngine
    {
        public MemoryStream CreateDocument(Stream templateStream, Stream jsonDataStream, string rootElement)
        {
            /// TODO: You need to set the license
            var wordsLicense = new License();
            //wordsLicense.SetLicense("Aspose.Total.lic");

            try
            {
                Document doc = new Document(templateStream);

                JsonDataSource dataSource = new JsonDataSource(jsonDataStream);

                ReportingEngine engine = new ReportingEngine();

                engine.BuildReport(doc, dataSource, rootElement);

                MemoryStream outputStream = new MemoryStream();
                doc.Save(outputStream, SaveFormat.Docx);
                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while building the report: " + ex.Message);
                throw;
            }
        }
    }
}
