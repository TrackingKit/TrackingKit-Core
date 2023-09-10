using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tracking;

namespace TrackingKit_Core
{
    internal class JsonConverterTrackerStorage<TTrackerStorage> : JsonConverter<TTrackerStorage>
        where TTrackerStorage : ITrackerStorage, new()
    {
        public override TTrackerStorage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var storage = new TTrackerStorage(); // You may need a way to create a new instance of ITrackerStorage.

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var property = reader.GetString();
                reader.Read(); // Move to the next token, which should be the start of the tick object.

                while (reader.TokenType == JsonTokenType.StartObject)
                {
                    var tick = int.Parse(reader.GetString()); // Assuming ticks are integers.
                    reader.Read(); // Move to the next token, which should be the start of the version object.

                    while (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var version = int.Parse(reader.GetString());
                        reader.Read(); // Move to the next token, which should be the property name for data.

                        if (reader.GetString() != "Data")
                        {
                            throw new JsonException();
                        }

                        reader.Read(); // Move to the next token, which is the actual data.
                        var data = JsonSerializer.Deserialize<TaggedData<object>>(ref reader, options);

                        reader.Read(); // Move to the next token, which should be the "Tags" property name.

                        if (reader.GetString() != "Tags")
                        {
                            throw new JsonException();
                        }

                        reader.Read(); // Move to the start of the array.

                        var tags = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            tags.Add(reader.GetString());
                        }

                        // Store the data in the storage.
                        var successful = storage.AddValueWithTags(property, tick, version, data.Object, tags);

                        if (!successful)
                        {
                            throw new JsonException($"Failed to add {property}, {tick}, {version}, {data.Object} and {tags}.");
                        }

                        reader.Read(); // Move to the end of the version object or the next version.
                    }

                    reader.Read(); // Move to the end of the tick object or the next tick.
                }

                reader.Read(); // Move to the end of the property object or the next property.
            }

            return storage;
        }


        public override void Write(Utf8JsonWriter writer, TTrackerStorage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var property in value.Properties)
            {
                writer.WriteStartObject(property);

                if (value.TryGetTicks(property, out var ticks))
                {
                    foreach (var tick in ticks)
                    {
                        writer.WriteStartObject(tick.ToString());

                        if (value.TryGetVersions(property, tick, out var versions))
                        {
                            foreach (var version in versions)
                            {
                                if (value.TryGetValue<TaggedData<object>>(property, tick, version, out var taggedData))
                                {
                                    writer.WriteStartObject(version.ToString());

                                    writer.WritePropertyName("Data");
                                    JsonSerializer.Serialize(writer, taggedData.Object, options); // serialize actual data

                                    writer.WriteStartArray("Tags");
                                    foreach (var tag in taggedData.Tags)
                                    {
                                        writer.WriteStringValue(tag);
                                    }
                                    writer.WriteEndArray();

                                    writer.WriteEndObject(); // end of version object
                                }
                            }
                        }

                        writer.WriteEndObject(); // end of tick object
                    }
                }

                writer.WriteEndObject(); // end of property object
            }

            writer.WriteEndObject(); // end of the root object
        }

    }
}
