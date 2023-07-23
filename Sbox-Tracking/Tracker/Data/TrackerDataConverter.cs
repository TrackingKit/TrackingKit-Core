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

                        string propertyName = reader.GetString();

                        if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                        {
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndObject)
                                {
                                    break;
                                }

                                string tickString = reader.GetString();
                                if (!Int32.TryParse(tickString, out int tick))
                                {
                                    throw new JsonException("Expected tick to be an integer");
                                }

                                if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonTokenType.EndObject)
                                        {
                                            break;
                                        }

                                        string versionString = reader.GetString();
                                        if (!Int32.TryParse(versionString, out int version))
                                        {
                                            throw new JsonException("Expected version to be an integer");
                                        }

                                        if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
                                        {
                                            TrackerData.TaggedData taggedData = JsonSerializer.Deserialize<TrackerData.TaggedData>(ref reader, options);
                                            trackerData.SetValue(propertyName, tick, version, taggedData.Data, taggedData.Tags);
                                        }
                                    }
                                }
                            }
                        }
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

            foreach (string property in value.GetProperties())
            {
                writer.WritePropertyName(property);
                writer.WriteStartObject();

                foreach (int tick in value.GetTicks(property))
                {
                    writer.WritePropertyName(tick.ToString());
                    writer.WriteStartObject();



                    foreach (int version in value.GetVersions(property, tick))
                    {
                        writer.WritePropertyName(version.ToString());
                        // This will only work if the class TaggedData has a suitable ToString() method or if it can be automatically serialized
                        JsonSerializer.Serialize(writer, value.GetTaggedData(property, tick, version), options);
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