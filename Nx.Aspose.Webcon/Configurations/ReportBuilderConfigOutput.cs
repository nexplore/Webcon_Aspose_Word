namespace Nx.Aspose.Webcon.Configurations
{
    using WebCon.WorkFlow.SDK.Common;
    using WebCon.WorkFlow.SDK.ConfigAttributes;

    public class ReportBuilderConfigOutput : PluginConfiguration
    {
        [ConfigEditableEnum(DisplayName = "Mode", Description = "Creates or overwrite if existing", Order = 1, DefaultValue = 0)]
        public FileCreatingMode FileCreatingMode { get; set; }

        [ConfigEditableText(DisplayName = "Filename", Description = "Filename of the generated document", IsRequired = true, Order = 2)]
        public string FileName { get; set; }

        [ConfigEditableText(DisplayName = "Documentcategory", Description = "Category where the document should be added to", IsRequired = false, Order = 3)]
        public string Category { get; set; }

        [ConfigEditableText(DisplayName = "Target element Id", Description = "Id of the element where the generated document will be added as an attachment. Uses the current element when empty", Order = 4)]
        public int? TargetElementId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Generated document Id", Description = "Field where the id of the generated document should be stored", IsRequired = false, Order = 5)]
        public int? GeneratedAttachmentId { get; set; }
    }
}

