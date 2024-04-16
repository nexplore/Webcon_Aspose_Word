using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Nx.Aspose.Webcon.Core;
using System.Dynamic;
using System.Reflection;

namespace Nx.Aspose.CoreTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var template = assembly.GetManifestResourceStream("Nx.Aspose.WebconTests.Templates.Template.docx");
            var jsonData = assembly.GetManifestResourceStream("Nx.Aspose.WebconTests.JSON.json1.json");

            JsonReportingEngine reportingEngine = new JsonReportingEngine();

            Stream result = reportingEngine.CreateDocument(template, jsonData, "persons");

            var fileStream = File.Create(@"C:\Temp\result.docx");
            result.CopyTo(fileStream);
        }

        [Test]
        public void VKFDox42ConversionTest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var template = assembly.GetManifestResourceStream("Nx.Aspose.WebconTests.Templates.Anerkennung_BS-Produkt_de.docx");
            var jsonData = assembly.GetManifestResourceStream("Nx.Aspose.WebconTests.JSON.Anerkennung.json");

            JsonReportingEngine reportingEngine = new JsonReportingEngine();

            Stream result = reportingEngine.CreateDocument(template, jsonData, null);

            var fileStream = File.Create(@"C:\Temp\Anerkennung.docx");
            result.CopyTo(fileStream);
        }

        [Test]
        public void ExpandoTest()
        {
            dynamic flexible = new ExpandoObject();
            var jsonObject = (IDictionary<string, object>)flexible;
            jsonObject.Add("Test", 1234);
            var converter = new ExpandoObjectConverter();
            var jsonString = JsonConvert.SerializeObject(jsonObject, converter);
            Assert.AreEqual("{\"Test\":1234}", jsonString);
        }
    }
}