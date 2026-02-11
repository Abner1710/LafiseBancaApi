using LafiseBancaApi.Data;
using LafiseBancaApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BancaContext>(options =>
    options.UseSqlite(connectionString));

// Inyeccion de Dependencias del Servicio Bancario
builder.Services.AddScoped<IBancaService, BancaService>();

// Add services to the container.

builder.Services.AddControllers().
    AddJsonOptions(options =>
{
    // esto corta el ciclo infinito ignorando las referencias repetidas
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()   // Permite cualquier origen
              .AllowAnyHeader()   // Permite cualquier encabezado
              .AllowAnyMethod();  // Permite GET, POST, PUT, DELETE
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 2. ACTIVAR CORS 
app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();
