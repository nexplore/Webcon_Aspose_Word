namespace Nx.Aspose.Webcon.Core
{
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Data;
    using System.Dynamic;
    using System.Text;
    using System.Threading.Tasks;
    using WebCon.WorkFlow.SDK.ActionPlugins.Model;
    using WebCon.WorkFlow.SDK.Documents.Model.Fields;
    using WebCon.WorkFlow.SDK.Tools.Data;
    using System.Linq;
    using WebCon.WorkFlow.SDK.ConfigAttributes;
    using Nx.Aspose.Webcon.Extensions;

    public class JsonGenerator
    {
        private ActionContextInfo context {  get; set; }  
        private StringBuilder logger = new StringBuilder();
        private StringBuilder adminLogger = new StringBuilder();

        public JsonGenerator(ActionContextInfo context, StringBuilder logger, StringBuilder adminLogger) {
            this.context = context; 
            this.logger = logger;
            this.adminLogger = adminLogger;
        }
        
        public async Task<string> GenerateJsonAsync()
        {
            string lastGroupName = "";
            dynamic lastGroup = null;
            IDictionary<string, object> lastGroupJson;
            DataTable fields = await GetProcessFieldsAsync(this.context.CurrentDocument.ProcessID);
            this.adminLogger.AppendLine($"Read element fields: {fields.Rows.Count} / {this.context.CurrentDocument.Fields.Count}");
            dynamic flexible = new ExpandoObject();
            var jsonObject = (IDictionary<string, object>)flexible;
            IDictionary<string, object> dictionaryToAdd;
            dictionaryToAdd = jsonObject;
            foreach (DataRow field in fields.Rows)
            {
                var fieldId = Int32.Parse(field["FieldId"].ToString());
                BaseField formField;
                if (this.context.CurrentDocument.Fields.TryGetByID(fieldId, out formField))
                {
                    // Handle grouping
                    var groupName = field["GroupName"];
                    // No new grouping for SqlRow/SqlGrid and ItemsList as they are already grouped
                    if (groupName != DBNull.Value
                        && formField.FormFieldType != FormFieldTypes.SqlRow
                        && formField.FormFieldType != FormFieldTypes.SqlGrid
                        && formField.FormFieldType != FormFieldTypes.ItemsList
                        )
                    {
                        if (lastGroupName != groupName.ToString())
                        {
                            this.adminLogger.AppendLine($"Group {groupName}");
                            if (lastGroup != null)
                            {
                                jsonObject.Add(lastGroupName, lastGroup);
                            }
                            lastGroup = new ExpandoObject();
                            lastGroupJson = (IDictionary<string, object>)lastGroup;
                            dictionaryToAdd = lastGroupJson;
                            lastGroupName = groupName.ToString();
                        }
                    }

                    this.adminLogger.AppendLine($"\t\tField {field["FieldName"]} / {formField.FormFieldType}");
                    switch (formField.FormFieldType)
                    {
                        // Choice can be of Type PickerField or ChooseField
                        case FormFieldTypes.Choice:
                            if (formField.GetType() == typeof(PickerField))
                            {
                                dictionaryToAdd.Add(field.ToReadableString("FieldName"), this.GetPickerValue(fieldId));
                            }
                            else
                            {
                                dictionaryToAdd.Add(field.ToReadableString("FieldName"), formField.GetValue());
                            }
                            break;
                        // Sql-Row
                        case FormFieldTypes.SqlRow:
                            var sqlRow = await this.GetSqlRowDataAsync(fieldId);
                            jsonObject.Add(field.ToReadableString("FieldName"), sqlRow);
                            break;
                        // SQL-Grid
                        case FormFieldTypes.SqlGrid:
                            var sqlGridData = await this.GetSqlGridDataAsync(fieldId);
                            jsonObject.Add(field.ToReadableString("FieldName"), sqlGridData);
                            break;
                        // Itemlist
                        case FormFieldTypes.ItemsList:
                            jsonObject.Add(field.ToReadableString("FieldName"), this.GetItemListData(fieldId));
                            break;
                        default:
                            dictionaryToAdd.Add(field.ToReadableString("FieldName"), formField.GetValue());
                            break;
                    }
                }
            }

            // Don't forget to add the last group!
            if (lastGroupName != null)
            {
                jsonObject.Add(lastGroupName, lastGroup);
            }

            // now we are ready to serialize
            var converter = new ExpandoObjectConverter();
            var jsonString = JsonConvert.SerializeObject(jsonObject, converter);
            return jsonString;
        }

        /// <summary>
        /// Retrieves process fields used for field mappings
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        private async Task<DataTable> GetProcessFieldsAsync(int processId)
        {
            SqlCommand cmd = new SqlCommand
            {
                CommandText = "SELECT  WFC.WFCON_ID As FieldId, WFC.WFCON_Prompt AS FieldName, FD.FDEF_Name AS InternalName, WFCG.WFCON_Prompt as GroupName, WFC.WFCON_FieldTypeID AS FieldType FROM  WFConfigurations WFC INNER JOIN WFFieldDefinitions FD ON WFC.WFCON_FDEFID = FD.FDEF_ID LEFT JOIN WFConfigurations WFCG ON WFC.WFCON_GroupID = WFCG.WFCON_ID WHERE WFC.WFCON_DEFID = @ProcessId AND WFC.WFCON_FieldTypeID NOT IN (24, 45, 33, 34, 35, 36, 38, 37) ORDER BY WFCG.WFCON_Prompt, WFC.WFCON_ID"
            };
            cmd.Parameters.AddWithValue("ProcessId", processId);
            var executionHelper = new SqlExecutionHelper(this.context);
            this.adminLogger.AppendLine($"Read fields for process ({processId})");
            var fields = await executionHelper.GetDataTableForSqlCommandAsync(cmd);
            return fields;
        }

        private object GetPickerValue(int fieldId)
        {
            var pickerField = this.context.CurrentDocument.PickerFields.GetByID(fieldId);
            if (pickerField.Values.Count == 1)
            {
                return pickerField.Values.First();
            }
            return pickerField.Values;
        }

        private async Task<object> GetSqlGridDataAsync(int fieldId)
        {
            var items = new List<dynamic>();
            var sqlField = this.context.CurrentDocument.SqlFields.GetByID(fieldId);
            var data = await sqlField.GetDataAsync();
            if (data.Rows.Count > 0)
            {
                DataRow firstRow = data.Rows[0];
                // Get field mappings if returns the process id
                var fieldMappings = await TryGetFieldMappingsAsync(firstRow);

                foreach (DataRow row in data.Rows)
                {
                    dynamic flexible = new ExpandoObject();
                    var jsonObject = (IDictionary<string, object>)flexible;
                    foreach (DataColumn col in data.Columns)
                    {
                        AddFieldMappingOrColumnName(fieldMappings, row, col, jsonObject);
                    }
                    items.Add(jsonObject);
                }
            }
            return items;
        }

        private static void AddFieldMappingOrColumnName(DataTable fieldMappings, DataRow row, DataColumn col, IDictionary<string, object> jsonObject)
        {
            if (fieldMappings == null)
            {
                jsonObject.Add(col.ColumnName, row[col]);
            }
            else
            {
                var fieldMapping = fieldMappings.Select($"InternalName = '{col.ColumnName}'").FirstOrDefault();
                if (fieldMapping != null)
                {
                    jsonObject.Add(fieldMapping.ToReadableString("FieldName"), row[col]);
                }
                else
                {
                    jsonObject.Add(col.ColumnName.ToReadableString(), row[col]);
                }
            }
        }

        private async Task<object> GetSqlRowDataAsync(int fieldId)
        {
            var sqlField = this.context.CurrentDocument.SqlFields.GetByID(fieldId);
            var data = await sqlField.GetDataAsync();
            dynamic sqlFields = new ExpandoObject();
            var jsonObject = (IDictionary<string, object>)sqlFields;
            if (data.Rows.Count > 0)
            {
                DataRow row = data.Rows[0];
                // Get field mappings if returns the process id
                var fieldMappings = await TryGetFieldMappingsAsync(row);
                foreach (DataColumn col in data.Columns)
                {
                    AddFieldMappingOrColumnName(fieldMappings, row, col, jsonObject);
                }
            }
            return sqlFields;
        }

        private async Task<DataTable> TryGetFieldMappingsAsync(DataRow row)
        {
            if (row.Table.Columns.Contains("DEF_ID"))
            {
                int def_id = Convert.ToInt32(row["DEF_ID"]);
                return await this.GetProcessFieldsAsync(def_id);
            }

            return null;
        }

        private object GetItemListData(int fieldId)
        {
            var items = new List<dynamic>();
            var itemlist = this.context.CurrentDocument.ItemsLists.GetByID(fieldId);
            foreach (var row in itemlist.Rows)
            {
                dynamic flexible = new ExpandoObject();
                var jsonObject = (IDictionary<string, object>)flexible;
                foreach (var col in itemlist.Columns)
                {
                    switch (col.ColumnType)
                    {
                        case ItemListColumnTypes.Choice:
                            jsonObject.Add(col.ToReadableString(), row.ChooseCells.GetByID(col.ID).GetValue());
                            break;
                        case ItemListColumnTypes.ChoosePicer:
                            jsonObject.Add(col.ToReadableString(), row.PickerCells.GetByID(col.ID).GetValue());
                            break;
                        default:
                            jsonObject.Add(col.ToReadableString(), row.GetCellValue(col.ID));
                            break;
                    }
                }
            }
            return items;
        }

    }
}
