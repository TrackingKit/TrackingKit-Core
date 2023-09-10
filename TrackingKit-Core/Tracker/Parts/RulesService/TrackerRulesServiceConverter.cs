using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tracking.Rules;

namespace Tracking.RulesService
{
    public partial class TrackerRulesServiceConverter : JsonConverter<TrackerRulesService>
    {
        public override TrackerRulesService Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;

            /*

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            TrackerRulesService rulesService = new TrackerRulesService();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return rulesService;
                }

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                reader.Read();

                string ruleTypeName = reader.GetString();
                TypeDescription ruleType = TypeLibrary.GetType(ruleTypeName);

                if (ruleType == null)
                {
                    throw new JsonException($"Unknown rule type: {ruleTypeName}");
                }

                // Create the rule and register it with the service
                TrackerRule rule = (TrackerRule)TypeLibrary.Create(ruleTypeName, ruleType.TargetType);


                rulesService.Add(rule);

                // Skip past the end of the rule array
                reader.Read();
            }

            throw new JsonException();

            */
        }


        public override void Write(Utf8JsonWriter writer, TrackerRulesService value, JsonSerializerOptions options)
        {



            writer.WriteStartArray();

            foreach (var rule in value.GetAll<ITrackerRule>())
                writer.WriteStringValue(rule.GetType().FullName);

            writer.WriteEndArray();
        }


    }
}