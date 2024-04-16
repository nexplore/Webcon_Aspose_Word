namespace Nx.Aspose.Webcon.CustomActions
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Nx.Aspose.Webcon.Configurations;
    using WebCon.WorkFlow.SDK.ActionPlugins;
    using WebCon.WorkFlow.SDK.ActionPlugins.Model;
    using WebCon.WorkFlow.SDK.Documents;
    using WebCon.WorkFlow.SDK.Tools.Data;
    using Nx.Aspose.Webcon.Core;
    using Nx.Aspose.Webcon.Extensions;
    using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
    using System.Linq;

    public class ReportBuilder : CustomAction<ReportBuilderConfig>
    {
        private readonly StringBuilder logger = new StringBuilder();
        private readonly StringBuilder adminlogger = new StringBuilder();

        public override async Task RunAsync(RunCustomActionParams args)
        {
            var attachmentManager = new DocumentAttachmentsManager(args.Context);
            int templateId;
            
            // Get TemplateId
            try
            {
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "SELECT ATT_ID FROM WFElements INNER JOIN WFDataAttachmets ON WFD_ID = ATT_WFDID WHERE WFD_Guid = @Guid"
                };
                cmd.Parameters.AddWithValue("Guid", this.Configuration.Input.TemplateIdentifier);
                var executionHelper = new SqlExecutionHelper(args.Context);
                var result = await executionHelper.ExecSqlCommandScalarAsync(cmd);
                templateId = (int)result;
            }
            catch (Exception ex)
            {
                args.Context.PluginLogger.AppendDebug(ex.Message);
                this.logger.AppendLine($"No Template found with the guid {this.Configuration.Input.TemplateIdentifier}");
                this.logger.AppendLine(ex.Message);
                args.HasErrors = true;
                return;
            }

            MemoryStream documentStream;
            string jsonString = string.Empty;
            try
            {
                // Get the template
                var template = await attachmentManager.GetAttachmentAsync(templateId);
                if (this.Configuration.Input.JsonType == JsonType.Standard)
                {
                    var jsonGenerator = new JsonGenerator(args.Context, this.logger, this.adminlogger);
                    jsonString = await jsonGenerator.GenerateJsonAsync();
                }
                else
                {
                    jsonString = this.Configuration.Input.Json;
                }
                this.adminlogger.AppendLine(jsonString);
                JsonReportingEngine reportingEngine = new JsonReportingEngine();
                using (var templateStream = new MemoryStream(template.Content))
                {
                    using (var jsonStream = jsonString.ToStream())
                    {
                        documentStream = reportingEngine.CreateDocument(templateStream, jsonStream, this.Configuration.Input.RootElement);
                    }
                    this.adminlogger.AppendLine($"Document {this.Configuration.Output.FileName} created");
                }
            }
            catch (Exception ex)
            {
                args.Context.PluginLogger.AppendDebug("Failed to generate document");
                args.Context.PluginLogger.AppendDebug(ex.Message);
                args.Context.PluginLogger.AppendDebug($"Json: {jsonString}");
                args.Context.PluginLogger.AppendDebug($"RootElement: {this.Configuration.Input.RootElement}");
                args.HasErrors = true;
                args.LogMessage = this.adminlogger.ToString(); ;
                return;
            }
            finally
            {
            }

            int documentId = 0;
            int attachmentId;
            NewAttachmentData attachment;
            try
            {
                documentId = this.Configuration.Output.TargetElementId ?? args.Context.CurrentDocument.ID;
                var fileName = this.Configuration.Output.FileName;
                AttachmentData existing = null;
                if (this.Configuration.Output.FileCreatingMode == FileCreatingMode.Overwrite)
                {
                    var attachments = await attachmentManager.GetAttachmentsAsync(new GetAttachmentsParams() { DocumentId = documentId });
                    existing = attachments?.FirstOrDefault(att => att.FileName == fileName);
                }

                // Check whether a document of the same name already exists
                if (existing == null) { 
                    // And add it to the current instance
                    attachment = attachmentManager.GetNewAttachment(this.Configuration.Output.FileName, documentStream.ToArray());

                    if (!string.IsNullOrEmpty(this.Configuration.Output.Category))
                    {
                        await attachment.SetFileGroupAsync(this.Configuration.Output.Category);
                    }
                    attachmentId = await attachmentManager.AddAttachmentAsync(new AddAttachmentParams() { Attachment = attachment, DocumentId = documentId });
                    args.Message = $"Added {attachment.FileName} to instanceId {documentId}";
                }
                else
                {
                    existing.Content = documentStream.ToArray();
                    attachmentId = existing.ID;
                    await attachmentManager.UpdateAttachmentAsync(new UpdateAttachmentParams() { Attachment = existing });
                    args.Message =  $"Updated {existing.FileName} of instanceId {documentId}";
                }
            }
            catch (Exception ex)
            {
                args.Context.PluginLogger.AppendDebug($"Failed to save attachment to {documentId}");
                args.Context.PluginLogger.AppendDebug(ex.Message);
                args.HasErrors = true;
                args.LogMessage = this.adminlogger.ToString(); ;
                return;
            }

            try
            {
                // Store the id of the attachment
                if (this.Configuration.Output.GeneratedAttachmentId.HasValue)
                {
                    args.Context.CurrentDocument.SetFieldValue(this.Configuration.Output.GeneratedAttachmentId.Value, attachmentId);
                }
            }
            catch (Exception ex)
            {
                args.Context.PluginLogger.AppendDebug($"Failed to save attachmentId {attachmentId} to {args.Context.CurrentDocument.ID}");
                args.Context.PluginLogger.AppendDebug(ex.Message);
                args.HasErrors = true;
                args.LogMessage = this.adminlogger.ToString(); ;
                return;
            }
            args.LogMessage = this.adminlogger.ToString(); ;
        }
    }
}