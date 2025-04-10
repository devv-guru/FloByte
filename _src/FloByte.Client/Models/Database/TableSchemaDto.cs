namespace FloByte.Client.Models.Database;

public class TableSchemaDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
}
