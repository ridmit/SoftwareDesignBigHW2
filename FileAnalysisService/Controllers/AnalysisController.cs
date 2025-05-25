using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Data;
using FileAnalysisService.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("analysis")]
public class AnalysisController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly HttpClient _fileClient;
    private readonly HttpClient _wcClient;


    public AnalysisController(AppDbContext db, IHttpClientFactory httpFactory, IConfiguration cfg)
    {
        _db = db;

        _fileClient = httpFactory.CreateClient("fileService");
        var fsUrl = cfg["FileService:BaseUrl"]
                    ?? throw new InvalidOperationException("FileService:BaseUrl not set");
        _fileClient.BaseAddress = new Uri(fsUrl);

        _wcClient = httpFactory.CreateClient();
    }

    [HttpGet("{fileId:guid}")]
    public async Task<ActionResult<AnalysisResult>> GetAnalysis(Guid fileId)
    {
        var rec = await _db.Analyses.AsNoTracking().FirstOrDefaultAsync(a => a.FileId == fileId);

        if (rec != null)
        {
            return Ok(rec);
        }

        rec = await AnalyzeAndStore(fileId);
        return rec != null ? Ok(rec) : NotFound();
    }

    [HttpGet("{fileId:guid}/wordcloud")]
    public async Task<IActionResult> GetWordCloud(Guid fileId)
    {
        var rec = await _db.Analyses.FirstOrDefaultAsync(a => a.FileId == fileId);
        if (rec == null || rec.WordCloudSvg == null)
        {
            rec = await AnalyzeAndStore(fileId);
            if (rec == null || rec.WordCloudSvg == null)
            {
                return NotFound();
            }
        }

        Response.Headers["Content-Disposition"] =
            new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"wordcloud-{fileId}.svg"
            }.ToString();

        var bytes = Encoding.UTF8.GetBytes(rec.WordCloudSvg);
        return File(bytes, "image/svg+xml");
    }


    private async Task<AnalysisResult?> AnalyzeAndStore(Guid fileId)
    {
        var record = await _db.Analyses.FirstOrDefaultAsync(a => a.FileId == fileId);

        var resp = await _fileClient.GetAsync($"/files/{fileId}");
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        var text = await resp.Content.ReadAsStringAsync();

        if (record != null && record.WordCloudSvg != null)
        {
            return record;
        }

        int paragraphs, words, chars;
        if (record == null)
        {
            paragraphs = Regex.Split(text, @"\r?\n\s*\r?\n")
                .Count(s => !string.IsNullOrWhiteSpace(s));
            words = Regex.Matches(text, @"\w+").Count;
            chars = text.Length;
        }
        else
        {
            paragraphs = record.Paragraphs;
            words = record.Words;
            chars = record.Characters;
        }

        var cloudUrl = BuildWordCloudUrl(text);
        var svg = await _wcClient.GetStringAsync(cloudUrl);

        if (record == null)
        {
            record = new AnalysisResult
            {
                FileId = fileId,
                Paragraphs = paragraphs,
                Words = words,
                Characters = chars,
                WordCloudSvg = svg,
                AnalysisDate = DateTime.UtcNow
            };
            _db.Analyses.Add(record);
        }
        else
        {
            record.WordCloudSvg = svg;
            _db.Analyses.Update(record);
        }

        await _db.SaveChangesAsync();
        return record;
    }


    private string BuildWordCloudUrl(string text)
    {
        var query = new Dictionary<string, string>
        {
            ["text"] = text
        };
        return QueryHelpers.AddQueryString("https://quickchart.io/wordcloud", query);
    }
}