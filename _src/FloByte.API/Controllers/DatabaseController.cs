using FloByte.Application.Features.Database.DTOs;
using FloByte.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FloByte.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly IDatabaseManagementService _databaseService;

    public DatabaseController(IDatabaseManagementService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet("info")]
    public async Task<ActionResult<DatabaseInfoDto>> GetDatabaseInfo()
    {
        var isConnected = await _databaseService.TestConnectionAsync();
        if (!isConnected)
        {
            return StatusCode(500, "Database connection failed");
        }

        var name = await _databaseService.GetDatabaseNameAsync();
        var tables = await _databaseService.GetTableNamesAsync();

        return new DatabaseInfoDto
        {
            Name = name,
            Tables = tables,
            IsConnected = true
        };
    }

    [HttpGet("tables/{tableName}/schema")]
    public async Task<ActionResult<IEnumerable<TableSchemaDto>>> GetTableSchema(string tableName)
    {
        var schema = await _databaseService.GetTableSchemaAsync(tableName);
        
        var result = new List<TableSchemaDto>();
        foreach (DataRow row in schema.Rows)
        {
            result.Add(new TableSchemaDto
            {
                ColumnName = row["COLUMN_NAME"].ToString()!,
                DataType = row["DATA_TYPE"].ToString()!,
                MaxLength = row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value 
                    ? Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]) 
                    : null,
                IsNullable = row["IS_NULLABLE"].ToString() == "YES",
                DefaultValue = row["COLUMN_DEFAULT"] != DBNull.Value 
                    ? row["COLUMN_DEFAULT"].ToString() 
                    : null
            });
        }

        return result;
    }

    [HttpPost("query")]
    public async Task<ActionResult<DataTable>> ExecuteQuery([FromBody] QueryRequestDto request)
    {
        try
        {
            var result = await _databaseService.ExecuteQueryAsync(request.Query, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("execute")]
    public async Task<ActionResult<int>> ExecuteNonQuery([FromBody] QueryRequestDto request)
    {
        try
        {
            var result = await _databaseService.ExecuteNonQueryAsync(request.Query, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("backup")]
    public async Task<IActionResult> BackupDatabase([FromQuery] string path)
    {
        try
        {
            await _databaseService.BackupDatabaseAsync(path);
            return Ok(new { message = "Database backup completed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreDatabase([FromQuery] string path)
    {
        try
        {
            await _databaseService.RestoreDatabaseAsync(path);
            return Ok(new { message = "Database restore completed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
