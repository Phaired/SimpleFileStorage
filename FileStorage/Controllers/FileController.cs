using FileStorage.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using FileStorage.Models;

namespace FileStorage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController: ControllerBase
{
        private readonly FileContext _context;
        private readonly string _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public FileController(FileContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string hash;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                hash = HashHelper.ComputeHash(stream);
            }

            // Vérifier si un document avec le même hash existe déjà
            var existingDocument = await _context.Documents.FirstOrDefaultAsync(d => d.Hash == hash);
            if (existingDocument != null)
            {
                // Si le fichier existe déjà, créer une nouvelle entrée avec le même chemin de fichier
                var newDocumentId = Guid.NewGuid();
                var newDocument = new Models.File
                {
                    Id = newDocumentId,
                    Name = file.FileName, // Conserver le nouveau nom de fichier
                    FilePath = existingDocument.FilePath,
                    Hash = hash,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync();
                return Ok(newDocument.Id);
            }

            // Créer un nouvel ID pour le document
            var documentId = Guid.NewGuid();
            var fileExtension = Path.GetExtension(file.FileName);
            var datePath = Path.Combine(DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"));
            var fileName = documentId.ToString() + fileExtension;
            var filePath = Path.Combine(_rootPath, datePath, fileName);

            // Créer le répertoire si nécessaire
            var directoryPath = Path.Combine(_rootPath, datePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Sauvegarder le fichier avec le nom de fichier comme l'ID du document
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var document = new Models.File
            {
                Id = documentId,
                Name = file.FileName, // Conserver le nom de fichier original
                FilePath = filePath,
                Hash = hash,
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return Ok(document.Id);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetDocument(Guid guid)
        {
            var document = await _context.Documents.FindAsync(guid);
            if (document == null)
                return NotFound();

            var fileStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
            var mimeType = GetMimeType(document.FilePath);
            return new FileStreamResult(fileStream, mimeType)
            {
                FileDownloadName = document.Name
            };
        }

        [HttpGet("hash/{hash}")]
        public async Task<IActionResult> GetDocumentByHash(string hash)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Hash == hash);
            if (document == null)
                return NotFound();

            var fileStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
            var mimeType = GetMimeType(document.FilePath);
            return new FileStreamResult(fileStream, mimeType)
            {
                FileDownloadName = document.Name
            };
        }

        private string GetMimeType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

    
}