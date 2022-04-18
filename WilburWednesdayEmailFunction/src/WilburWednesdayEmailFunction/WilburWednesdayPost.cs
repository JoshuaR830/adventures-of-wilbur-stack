using System.Text.Json.Serialization;

namespace WilburWednesdayEmailFunction;

public class WilburWednesdayPost
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("wilburImage")]
    public string WilburImage { get; set; }
}