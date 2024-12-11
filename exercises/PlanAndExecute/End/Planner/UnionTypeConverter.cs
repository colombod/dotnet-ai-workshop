using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Planner;

internal class UnionTypeConverter<T> : JsonConverter<T> where T : class
{
    private readonly Dictionary<string, Type> _typeMap;

    public UnionTypeConverter()
    {
        _typeMap = typeof(T).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(T)) && t.GetCustomAttribute<JsonDerivedTypeAttribute>() != null)
            .ToDictionary(
                t => (string)t.GetCustomAttribute<JsonDerivedTypeAttribute>()?.TypeDiscriminator ?? throw new InvalidOperationException("TypeDiscriminator cannot be null"),
                t => t
            );
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();
            if (_typeMap.TryGetValue(type, out var targetType))
            {
                return (T)JsonSerializer.Deserialize(root.GetRawText(), targetType, options);
            }
            throw new NotSupportedException($"Type {type} is not supported");
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var type = value.GetType();
        var typeName = _typeMap.FirstOrDefault(kv => kv.Value == type).Key;
        if (typeName == null)
        {
            throw new NotSupportedException($"Type {type} is not supported");
        }
        writer.WriteString("type", typeName);
        foreach (var property in type.GetProperties())
        {
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, property.GetValue(value), property.PropertyType, options);
        }
        writer.WriteEndObject();
    }
}
