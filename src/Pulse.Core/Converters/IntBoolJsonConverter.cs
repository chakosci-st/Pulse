using Newtonsoft.Json;
using System;

namespace Pulse.Core.Converters
{
    public class IntBoolJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) || objectType == typeof(int?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Boolean)
            {
                bool boolValue = (bool)reader.Value;
                return boolValue ? 1 : 0;
            }
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value);
            }

            if (reader.TokenType == JsonToken.String)
            {
                if (Convert.ToString(reader.Value) == "true" || Convert.ToString(reader.Value) == "1")
                    return 1;
                else
                    return 0;
            }
            if (reader.TokenType == JsonToken.Null && objectType == typeof(int?))
            {
                return null;
            }
            throw new JsonSerializationException("Unexpected token type for int-bool conversion.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            int intValue = (int)value;
            writer.WriteValue(intValue != 0); // 0 → false, any other int → true
        }
    }
}
