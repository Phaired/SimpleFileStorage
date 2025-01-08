using FileStorage.Models;
using FileStorage.Auth;
using Microsoft.AspNetCore.Authentication; // Namespace for authentication handlers
using Microsoft.EntityFrameworkCore;
using File = System.IO.File;

var builder = WebApplication.CreateBuilder(args);

// [1] Add authentication with two schemes: "Upload" and "Download"
builder.Services
    .AddAuthentication(options =>
    {
        // No default scheme is enforced here
        options.DefaultScheme = null;
    })
    .AddScheme<AuthenticationSchemeOptions, UploadTokenAuthenticationHandler>("Upload", options => { })
    .AddScheme<AuthenticationSchemeOptions, DownloadTokenAuthenticationHandler>("Download", options => { });

// [2] Add MVC services, Entity Framework, Swagger, CORS, etc.

// Define the data directory path
var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");

// Check if the data directory exists; if not, create it
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

// Define the database file path
var dbPath = Path.Combine(dataDirectory, "documents.db");

// Check if the database file exists; if not, create it
if (!File.Exists(dbPath))
{
    // Create an empty database file
    File.Create(dbPath).Dispose();
    Console.WriteLine($"Database file created at: {dbPath}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework to use SQLite with the specified database path
builder.Services.AddDbContext<FileContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Configure CORS to allow all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// [3] Build the application
var app = builder.Build();

// Enable CORS with the defined policy
app.UseCors("AllowAll");

// Apply migrations and ensure the database is up-to-date
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        throw; // Re-throw the exception after logging
    }
}

// Enable Swagger in development environments
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// [4] Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// [5] Map controller routes
app.MapControllers();

// Start the application
app.Run();
