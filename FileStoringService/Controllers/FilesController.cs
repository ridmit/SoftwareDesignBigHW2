using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileStoringService.Data;
using FileStoringService.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FileStoringService.Dto;

namespace FileStoringService.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public FilesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Guid>> Upload(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var content = ms.ToArray();
        var hash = ComputeHash(content);

        var exists = await _db.Files.FirstOrDefaultAsync(f => f.Hash == hash);
        if (exists != null)
        {
            return Ok(exists.Id);
        }

        var id = Guid.NewGuid();
        var folder = Path.Combine(_env.ContentRootPath, "StoredFiles");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, id + ".txt");
        await System.IO.File.WriteAllBytesAsync(path, content);

        var record = new FileRecord { Id = id, Name = file.FileName, Hash = hash, Location = path };
        _db.Files.Add(record);
        await _db.SaveChangesAsync();

        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var rec = await _db.Files.FindAsync(id);
        if (rec == null)
        {
            return NotFound();
        }

        var data = await System.IO.File.ReadAllBytesAsync(rec.Location);
        return File(data, "text/plain", rec.Name);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileInfoDto>>> GetAll()
    {
        var list = await _db.Files.AsNoTracking().Select(f => new FileInfoDto
        {
            Id = f.Id,
            Name = f.Name
        }).ToListAsync();

        return Ok(list);
    }

    private string ComputeHash(byte[] data)
    {
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "");
    }
}