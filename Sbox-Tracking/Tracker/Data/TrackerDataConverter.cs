using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public class TrackerDataConverter : JsonConverter<TrackerData>
    {
        public override TrackerData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            TrackerData trackerData = new TrackerData();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return trackerData;
                }

                if (reader.ValueTextEquals("Data") && reader.Read())
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }

                        // At this point, reader should be positioned at the property name (i.e., key of outer dictionary)
                        string propertyName = reader.GetString();

                        // Create inner dictionary for the current property
                        SortedDictionary<int, SortedDictionary<int, TrackerData.TaggedData>> tickDict = new SortedDictionary<int, SortedDictionary<int, TrackerData.TaggedData>>();

                        if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                        {
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndObject)
                                {
                                    break;
                                }

                                // Similarly, read tick and version dictionaries and populate `tickDict`
                                // This will involve further nested reading which can be done in a similar manner
                            }
                        }

                        trackerData.data[propertyName] = tickDict;
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, TrackerData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Data");
            writer.WriteStartObject();

            foreach (var propertyEntry in value.data)
            {
                writer.WritePropertyName(propertyEntry.Key);
                writer.WriteStartObject();

                foreach (var tickEntry in propertyEntry.Value)
                {
                    writer.WritePropertyName(tickEntry.Key.ToString());
                    writer.WriteStartObject();

                    foreach (var versionEntry in tickEntry.Value)
                    {
                        writer.WritePropertyName(versionEntry.Key.ToString());
                        // Assuming TaggedData can be serialized directly
                        JsonSerializer.Serialize(writer, versionEntry.Value, options);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }

    }
}