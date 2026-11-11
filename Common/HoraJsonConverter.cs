using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TareasService.Common;

// TimeOnly ⇄ "HH:mm" (el contrato pide "18:00", no "18:00:00")
public class HoraJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TimeOnly.ParseExact(reader.GetString()!, "HH:mm", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("HH:mm", CultureInfo.InvariantCulture));
}