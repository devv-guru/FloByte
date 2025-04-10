namespace FloByte.Client.Models.Database;

public class DatabaseInfoDto
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<string> Tables { get; set; } = Array.Empty<string>();
    public bool IsConnected { get; set; }
}
