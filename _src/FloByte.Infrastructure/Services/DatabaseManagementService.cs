using FloByte.Application.Common.Exceptions;
using FloByte.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FloByte.Infrastructure.Services;

public class DatabaseManagementService : IDatabaseManagementService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly string _connectionString;

    public DatabaseManagementService(
        IConfiguration configuration,
        IApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<IEnumerable<string>> GetTableNamesAsync()
    {
        var tables = new List<string>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(
            @"SELECT TABLE_NAME 
              FROM INFORMATION_SCHEMA.TABLES 
              WHERE TABLE_TYPE = 'BASE TABLE'", connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        
        return tables;
    }

    public async Task<DataTable> GetTableSchemaAsync(string tableName)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var schema = new DataTable();
        
        using var command = new SqlCommand(
            @"SELECT 
                COLUMN_NAME, 
                DATA_TYPE,
                CHARACTER_MAXIMUM_LENGTH,
                IS_NULLABLE,
                COLUMN_DEFAULT
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = @TableName", connection);
        
        command.Parameters.AddWithValue("@TableName", tableName);
        
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(schema);
        
        return schema;
    }

    public async Task<DataTable> ExecuteQueryAsync(string query, IDictionary<string, object>? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(query, connection);
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
        
        var result = new DataTable();
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(result);
        
        return result;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, IDictionary<string, object>? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(query, connection);
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }
        
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetDatabaseNameAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection.Database;
    }

    public async Task BackupDatabaseAsync(string backupPath)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var dbName = connection.Database;
        var query = $"BACKUP DATABASE [{dbName}] TO DISK = @BackupPath";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@BackupPath", backupPath);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task RestoreDatabaseAsync(string backupPath)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var dbName = connection.Database;
        
        // Set database to single user mode
        var singleUserQuery = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        using (var command = new SqlCommand(singleUserQuery, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
        
        try
        {
            // Restore database
            var restoreQuery = $"RESTORE DATABASE [{dbName}] FROM DISK = @BackupPath WITH REPLACE";
            using var command = new SqlCommand(restoreQuery, connection);
            command.Parameters.AddWithValue("@BackupPath", backupPath);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            // Set database back to multi user mode
            var multiUserQuery = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
            using var command = new SqlCommand(multiUserQuery, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
