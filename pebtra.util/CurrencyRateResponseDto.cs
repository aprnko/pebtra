using System.Text.Json.Serialization;

namespace Pebtra.Util;

public class CurrencyRateResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public ErrorDto? Error { get; set; }

    public class ErrorDto
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;
    }

    [JsonPropertyName("terms")]
    public string? Terms { get; set; }

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("timeframe")]
    public bool? Timeframe { get; set; }

    [JsonPropertyName("start_date")]
    public string? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public string? EndDate { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("quotes")]
    public Dictionary<string, Dictionary<string, decimal>>? Quotes { get; set; }
}

