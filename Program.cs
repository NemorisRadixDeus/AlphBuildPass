using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PasswordManager.Data;
using PasswordManager.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
// По умолчанию SQLite (для простого запуска). 
// Для PostgreSQL — раскомментируйте секцию ниже и закомментируйте SQLite.

// --- SQLite (по умолчанию) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
        ?? "Data Source=password_manager.db"));

// --- PostgreSQL (раскомментируйте для использования) ---
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")
//         ?? "Host=localhost;Port=5432;Database=password_manager;Username=postgres;Password=postgres"));

// ===== SERVICES =====
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IKeyDerivationService, KeyDerivationService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ===== JWT AUTHENTICATION =====
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "DefaultSuperSecretKeyForDevelopment_32chars!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PasswordManager",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PasswordManager",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== CONTROLLERS + SWAGGER =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Password Manager API",
        Version = "v1",
        Description = "Менеджер паролей с выбором криптографических протоколов и механизмом деривации ключей"
    });

    // Кнопка Authorize в Swagger для JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен: Bearer {ваш_токен}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== CORS (для Blazor или любого фронтенда) =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== AUTO MIGRATION (создание таблиц при первом запуске) =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ===== MIDDLEWARE =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Password Manager API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();
app.UseDefaultFiles();   // index.html как главная страница
app.UseStaticFiles();    // wwwroot/ раздаётся как статика
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("===========================================");
Console.WriteLine("  Password Manager запущен!");
Console.WriteLine("  Веб-интерфейс: http://localhost:5000");
Console.WriteLine("  Swagger API:   http://localhost:5000/swagger");
Console.WriteLine("  API Base:      http://localhost:5000/api");
Console.WriteLine("===========================================");

app.Run();
