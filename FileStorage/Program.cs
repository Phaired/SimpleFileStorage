using Microsoft.EntityFrameworkCore;
using FileStorage.Models;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<FileContext>(options =>
    options.UseSqlite("Data Source=Data/documents.db"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

var app = builder.Build();

// S'assurer que le répertoire Data existe
var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

// S'assurer que la base de données est créée.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileContext>();
    dbContext.Database.Migrate();
}

// Configurer le pipeline des requêtes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();