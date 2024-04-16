namespace Nx.Aspose.Webcon.Configurations
{
    using WebCon.WorkFlow.SDK.Common;
    using WebCon.WorkFlow.SDK.ConfigAttributes;

    public class ReportBuilderConfigInput : PluginConfiguration
    {
        [ConfigEditableText(DisplayName = "The guid of the template", Description = "Template Identifier (Guid)", IsRequired = true, Order = 1)]
        public string TemplateIdentifier
        {
            get; set;
        }

        [ConfigEditableEnum(DisplayName = "Json Type", Description = "Standard generates a Json object based on the current elements fields", Order = 2, DefaultValue = 0)]
        public JsonType JsonType
        {
            get; set;
        }


        [ConfigEditableText(DisplayName = "Custom Json", Description = "<p>Check the different options of the json at <a href='https://docs.aspose.com/words/net/accessing-json-data/'>Aspose.Words for .NET - Accessing JSON Data</a></p>", DescriptionAsHTML = true, IsRequired = false, Multiline = true, Lines = 20, Order = 3)]
        public string Json
        {
            get; set;
        }

        [ConfigEditableText(DisplayName = "Root element", Description = "<p>If a root JSON element is an array, a JsonDataSource instance should be treated as a sequence of items of this array and therefore given a name. See also <a href='https://docs.aspose.com/words/net/accessing-json-data/'>Aspose.Words for .NET - Accessing JSON Data</a></p>", DescriptionAsHTML = true, IsRequired = false, Order = 4)]
        public string RootElement
        {
            get; set;
        }
    }
}
