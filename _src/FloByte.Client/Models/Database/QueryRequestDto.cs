namespace FloByte.Client.Models.Database;

public class QueryRequestDto
{
    public string Query { get; set; } = string.Empty;
    public IDictionary<string, object>? Parameters { get; set; }
}
