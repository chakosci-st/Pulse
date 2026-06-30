using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pulse.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pulse.Core.Converters
{

    public class FormFieldOptionListConverter : JsonConverter<List<FormFieldOption>>
    {
        public override List<FormFieldOption> ReadJson(JsonReader reader, Type objectType, List<FormFieldOption> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new List<FormFieldOption>();
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                foreach (var item in token)
                {
                    if (item.Type == JTokenType.String)
                    {
                        result.Add(new FormFieldOption { OptionValue = item.ToString(), OptionLabel = item.ToString() });
                    }
                    else if (item.Type == JTokenType.Object)
                    {
                        result.Add(item.ToObject<FormFieldOption>());
                    }
                }
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, List<FormFieldOption> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
