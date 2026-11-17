using System.Text.Json.Serialization;
using BigCup.Data;
using BigCup.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Botão "Authorize" no Swagger: cole o accessToken retornado por /api/auth/login
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Token retornado por POST /api/auth/login"
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
});

// Banco: provedor definido por Database:Provider (SqlServer | Sqlite). Ver Data/DatabaseSetup.cs.
builder.Services.AddBigCupDatabase(builder.Configuration, builder.Environment.ContentRootPath);

// Usuários: cadastro/login prontos do ASP.NET Core Identity (token Bearer)
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<Usuario>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Aplica migrations (e o seed, via UseSeeding) só quando configurado — por padrão apenas em Development.
// Em produção: dotnet ef database update --context SqlServerAppDbContext
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Banco: {Provider} ({Database})",
        DatabaseSetup.GetProvider(app.Configuration), db.Database.GetDbConnection().DataSource);
    await db.Database.MigrateAsync();
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { message = "BigCup API", status = "running" }));
app.MapGroup("/api/auth").WithTags("Auth").MapIdentityApi<Usuario>();
app.MapControllers();

app.Run();
