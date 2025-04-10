using System.Data;

namespace FloByte.Application.Interfaces;

public interface IDatabaseManagementService
{
    Task<IEnumerable<string>> GetTableNamesAsync();
    Task<DataTable> GetTableSchemaAsync(string tableName);
    Task<DataTable> ExecuteQueryAsync(string query, IDictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object>? parameters = null);
    Task<bool> TestConnectionAsync();
    Task<string> GetDatabaseNameAsync();
    Task BackupDatabaseAsync(string backupPath);
    Task RestoreDatabaseAsync(string backupPath);
}
