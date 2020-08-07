using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot
{
    class StarGivingUsersJsonConverter : JsonConverter<StarGivingUsersList>
    {
        public override StarGivingUsersList ReadJson(JsonReader reader, Type objectType, [AllowNull] StarGivingUsersList existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                return new StarGivingUsersList(serializer.Deserialize<Dictionary<ulong, StarboardSource>>(reader));
            }
            catch (Exception)
            {
                int depth = reader.Depth;

                while (reader.Read())
                {
                    if (reader.Depth <= depth && reader.TokenType == JsonToken.EndObject)
                        break;
                }

                return new StarGivingUsersList();
            }
        }
        public override void WriteJson(JsonWriter writer, [AllowNull] StarGivingUsersList value, JsonSerializer serializer) 
        {
            serializer.Serialize(writer, value?.Dictionary);
        }
    }
}
