using System.Text.Json;
using System.Text.Json.Serialization;

namespace BidXesentation.Infrastructure;

// Ensures that DateTime values are serialized (written) in UTC format and deserialized (read) as UTC (see: https://github.com/dotnet/runtime/issues/1566).
public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime().ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    //// The issue with using DateTime.ToUniversalTime() is that it assumes I save the time in my local timezone but i save it as UTC in the db so it will convert it again to UTC which is will result in an invalid time.
    // public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    // {
    //     writer.WriteStringValue(value.ToUniversalTime());
    // }

}
