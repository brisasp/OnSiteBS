using System.Text.Json.Serialization;

namespace GestionAccesos.DTO.ResponseModels;

[Serializable]
public class ListResult<T> : PrimitiveResult
{
    public ListResult()
    {
        Data = new List<T>();
    }

    [JsonPropertyName("data")] public List<T> Data { get; set; }

    [JsonPropertyName("count")] public int Count => Data?.Count ?? 0;
}