using System.Text.Json.Serialization;
using BigCup.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext: try SQL Server first; if LocalDB is not available, fall back to SQLite for development convenience.
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection") ??
                  "Server=(localdb)\\mssqllocaldb;Database=BigCup;Trusted_Connection=True;";

bool useSqlite = false;
try
{
    // Quick connectivity test for SQL Server (short timeout)
    var builderSql = new SqlConnectionStringBuilder(defaultConn)
    {
        ConnectTimeout = 3
    };
    using (var testConn = new SqlConnection(builderSql.ConnectionString))
    {
        testConn.Open();
        testConn.Close();
    }
}
catch (Exception ex)
{
    // If we cannot open SQL Server (e.g. LocalDB not installed), fall back to SQLite and continue.
    Console.WriteLine("Warning: SQL Server not available, falling back to SQLite. Error: " + ex.Message);
    useSqlite = true;
}

if (useSqlite)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=bigcup_dev.db"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(defaultConn));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new { message = "BigCup API", status = "running" }));
app.MapControllers();

app.Run();