namespace Nx.Aspose.Webcon.Extensions
{
    using System.Data;
    using WebCon.WorkFlow.SDK.Documents.Model.ItemLists.Configuration;

    public static class FieldMappingExtension
    {
        public static string ToReadableString(this object value)
        {
            return value.ToString().Replace(" ", "_").Replace("/","").Replace("__","_");
        }

        public static string ToReadableString(this DataRow row, string fieldName)
        {
            return row[fieldName].ToReadableString(); 
        }

        public static string ToReadableString(this ColumnInfo columnInfo)
        {
            return columnInfo.DisplayName.ToReadableString();
        }

    }
}
