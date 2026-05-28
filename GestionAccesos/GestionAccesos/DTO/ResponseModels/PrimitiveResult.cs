using System.Text.Json.Serialization;

namespace GestionAccesos.DTO.ResponseModels;

[Serializable]
public class PrimitiveResult
{
    public PrimitiveResult()
    {
        Errors = new List<string>();
        Info = new List<string>();
    }

    [JsonPropertyName("errors")] public IList<string> Errors { get; set; }
    [JsonPropertyName("info")] public IList<string> Info { get; set; }

    [JsonPropertyName("hasErrors")] public bool HasErrors => Errors.Any();
    [JsonPropertyName("hasInfo")] public bool HasInfo => Info.Any();
}