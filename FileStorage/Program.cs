using FileStorage.Models;
using FileStorage.Auth;
using Microsoft.AspNetCore.Authentication; // <- Le namespace où vous avez mis vos handlers
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// [1] Ajouter l’authentification avec deux schémas : "Upload" et "Download"
builder.Services
    .AddAuthentication(options =>
    {
        // Nous n’imposons pas de schéma par défaut ici
        options.DefaultScheme = null; 
    })
    .AddScheme<AuthenticationSchemeOptions, UploadTokenAuthenticationHandler>("Upload", options => {})
    .AddScheme<AuthenticationSchemeOptions, DownloadTokenAuthenticationHandler>("Download", options => {});

// [2] Ajouter les services MVC, EF, etc.

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<FileContext>(options =>
    options.UseSqlite("Data Source=Data/documents.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});



// [3] Construction de l’application
var app = builder.Build();
app.UseCors("AllowAll");

// Migrations + création des dossiers...
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileContext>();
    dbContext.Database.Migrate();
}

// Swagger en dev...
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// [4] Activer l’authentification et l’autorisation
app.UseAuthentication();
app.UseAuthorization();

// [5] Contrôleurs
app.MapControllers();
app.Run();