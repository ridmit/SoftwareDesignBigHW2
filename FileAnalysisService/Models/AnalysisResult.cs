using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FileAnalysisService.Models;

public class AnalysisResult
{
    public int Id { get; set; }
    public Guid FileId { get; set; }
    public int Paragraphs { get; set; }
    public int Words { get; set; }
    public int Characters { get; set; }

    [JsonIgnore]
    public string? WordCloudSvg { get; set; }

    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}