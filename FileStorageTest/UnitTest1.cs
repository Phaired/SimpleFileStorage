using System.Reflection;
using FileStorage.Controllers;
using FileStorage.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using File = FileStorage.Models.File;

namespace FileStorageTest
{
    public class FileControllerTests
    {
        private FileController _controller;
        private FileContext _context;
        private string _rootPath;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FileContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new FileContext(options);
            _rootPath = Path.Combine(Path.GetTempPath(), "FileStorageTest");
            Directory.CreateDirectory(_rootPath);

            _controller = new FileController(_context);

            // Use reflection to set the private _rootPath field
            var rootPathField = typeof(FileController).GetField("_rootPath", BindingFlags.NonPublic | BindingFlags.Instance);
            rootPathField.SetValue(_controller, _rootPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, true);
            }
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task UploadDocument_NoFileUploaded_ReturnsBadRequest()
        {
            var result = await _controller.UploadDocument(null);
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).Value, Is.EqualTo("No file uploaded."));
        }

        [Test]
        public async Task UploadDocument_FileUploaded_ReturnsOk()
        {
            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);

            var result = await _controller.UploadDocument(mockFile.Object);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task GetDocument_NonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _controller.GetDocument(nonExistentId);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetDocument_ExistingId_ReturnsFileStreamResult()
        {
            var documentId = Guid.NewGuid();
            var document = new File
            {
                Id = documentId,
                Name = "test.txt",
                FilePath = Path.Combine(_rootPath, "test.txt"),
                Hash = "hash",
                UploadedAt = DateTime.UtcNow
            };
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Create the file
            _testFilePath = document.FilePath;
            if (!System.IO.File.Exists(_testFilePath))
            {
                await System.IO.File.WriteAllTextAsync(_testFilePath, "File content");
            }

            var result = await _controller.GetDocument(documentId);

            Assert.That(result, Is.TypeOf<FileStreamResult>());
        }

        [Test]
        public async Task GetDocumentByHash_NonExistentHash_ReturnsNotFound()
        {
            var nonExistentHash = "nonexistenthash";

            var result = await _controller.GetDocumentByHash(nonExistentHash);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetDocumentByHash_ExistingHash_ReturnsFileStreamResult()
        {
            var document = new File
            {
                Id = Guid.NewGuid(),
                Name = "test.txt",
                FilePath = Path.Combine(_rootPath, "test.txt"),
                Hash = "existinghash",
                UploadedAt = DateTime.UtcNow
            };
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Create the file
            _testFilePath = document.FilePath;
            if (!System.IO.File.Exists(_testFilePath))
            {
                await System.IO.File.WriteAllTextAsync(_testFilePath, "File content");
            }

            var result = await _controller.GetDocumentByHash("existinghash");

            Assert.That(result, Is.TypeOf<FileStreamResult>());
        }

        [Test]
        public async Task DeleteDocument_NonExistentId_ReturnsNotFound()
        {
            var result = await _controller.DeleteDocument(Guid.NewGuid());

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteDocument_ExistingId_RemovesFileAndEntry()
        {
            var documentId = Guid.NewGuid();
            var document = new File
            {
                Id = documentId,
                Name = "test.txt",
                FilePath = Path.Combine(_rootPath, "test.txt"),
                Hash = "hash",
                UploadedAt = DateTime.UtcNow
            };
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Create the file
            _testFilePath = document.FilePath;
            await System.IO.File.WriteAllTextAsync(_testFilePath, "content");

            var result = await _controller.DeleteDocument(documentId);

            Assert.That(result, Is.TypeOf<OkResult>());
            Assert.That(await _context.Documents.FindAsync(documentId), Is.Null);
            Assert.That(System.IO.File.Exists(_testFilePath), Is.False);
        }
    }
}