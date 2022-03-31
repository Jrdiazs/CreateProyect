using CreateProject.Tools.String;

namespace CreateProject.Models
{
    public class PropertiesView
    {
        public string ColumnName { get; set; }
        public int OrderColumn { get; set; }

        public string DataType { get; set; }

        public string IsNullable { get; set; }

        public bool IsNull
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IsNullable))
                    return false;

                return IsNullable.ToLower() == "yes" || IsNullable.ToLower() == "si";
            }
        }

        public bool IsNullableNet
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IsNullable))
                    return false;

                return StringUtil.IsPropertyNullable(DataType);
            }
        }

        public bool IsMaxLength
        { get { return MaximumLength.HasValue && StringUtil.IsPropertyMaxLength(DataType) && MaximumLength > 0; } }

        public int? MaximumLength { get; set; }

        public string Column
        { get { return $"public {DataTypeNet}{(IsNullableNet && IsNull ? "?" : string.Empty)} {ColumnName} {{ get; set; }}"; } }

        public string DataTypeNet
        {
            get
            {
                return StringUtil.DataTypeNetSql(DataType);
            }
        }
    }
}