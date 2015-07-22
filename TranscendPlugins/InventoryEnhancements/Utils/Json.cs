using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GTRPlugins.Utils
{
    public static class Json
    {
        public static void Serialize<T>(T obj, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter writer = new StreamWriter(path))
            using (JsonTextWriter jwriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jwriter, obj);
            }
        }

        public static bool SerializeAndCheck<T>(T obj, string path)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                using (StreamWriter writer = new StreamWriter(path))
                using (JsonTextWriter jwriter = new JsonTextWriter(writer))
                {
                    serializer.Serialize(jwriter, obj);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T DeSerialize<T>(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }

        public static T? GetFirstInstance<T>(string propertyName, string path) where T : struct
        {
            using (StreamReader reader = new StreamReader(path))
            using (var jsonReader = new JsonTextReader(reader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName
                        && (string)jsonReader.Value == propertyName)
                    {
                        jsonReader.Read();

                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(jsonReader);
                    }
                }
                return null;
            }
        }

        public static List<T> GetFirstInstanceList<T>(string propertyName, string path)
        {
            using (StreamReader reader = new StreamReader(path))
            using (var jsonReader = new JsonTextReader(reader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName
                        && (string)jsonReader.Value.ToString().ToLower() == propertyName.ToLower())
                    {
                        jsonReader.Read();

                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<List<T>>(jsonReader);
                    }
                }
                return new List<T> { };
            }
        }
    }
}
