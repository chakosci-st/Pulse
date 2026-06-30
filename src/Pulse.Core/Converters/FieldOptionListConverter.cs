using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pulse.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pulse.Core.Converters
{

    public class FieldOptionListConverter : JsonConverter<List<FieldOption>>
    {
        public override List<FieldOption> ReadJson(JsonReader reader, Type objectType, List<FieldOption> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new List<FieldOption>();
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                foreach (var item in token)
                {
                    if (item.Type == JTokenType.String)
                    {
                        result.Add(new FieldOption { OptionValue = item.ToString(), OptionLabel = item.ToString() });
                    }
                    else if (item.Type == JTokenType.Object)
                    {
                        result.Add(item.ToObject<FieldOption>());
                    }
                }
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, List<FieldOption> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
