namespace Nx.Aspose.Webcon.Configurations
{
    using WebCon.WorkFlow.SDK.Common;
    using WebCon.WorkFlow.SDK.ConfigAttributes;

    public class ReportBuilderConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "Input", Description = "Settings for the document to generate", Order = 1)]
        public ReportBuilderConfigInput Input { get; set; }

        [ConfigGroupBox(DisplayName = "Output", Description = "Settings for the generated document", Order = 2)]
        public ReportBuilderConfigOutput Output { get; set; }
    }
}