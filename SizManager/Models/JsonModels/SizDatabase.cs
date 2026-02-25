using System.Text.Json.Serialization;

namespace SizManager.Models.JsonModels;

public class SizDatabase
{
    [JsonPropertyName("metadata")]
    public SizMetadata Metadata { get; set; } = new();

    [JsonPropertyName("professions")]
    public List<JsonProfession> Professions { get; set; } = new();
}

public class SizMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("total_professions")]
    public int TotalProfessions { get; set; }
}

public class JsonProfession
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("siz_list")]
    public List<JsonSizItem> SizList { get; set; } = new();
}

public class JsonSizItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("norm")]
    public string Norm { get; set; } = string.Empty;
}
