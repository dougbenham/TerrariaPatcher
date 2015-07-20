using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GTRPlugins.Utils
{
    public static class Json
    {
        public static void Serialize<T>(T obj, string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    jsonSerializer.Serialize(jsonTextWriter, obj);
                }
            }
        }
        public static bool SerializeAndCheck<T>(T obj, string path)
        {
            bool result;
            try
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Formatting = Formatting.Indented;
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                    {
                        jsonSerializer.Serialize(jsonTextWriter, obj);
                    }
                }
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public static T DeSerialize<T>(string path)
        {
            T result;
            using (StreamReader streamReader = new StreamReader(path))
            {
                result = JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
            }
            return result;
        }
        public static T? GetFirstInstance<T>(string propertyName, string path) where T : struct
        {
            T? result;
            using (StreamReader streamReader = new StreamReader(path))
            {
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    while (jsonTextReader.Read())
                    {
                        if (jsonTextReader.TokenType == JsonToken.PropertyName && (string)jsonTextReader.Value == propertyName)
                        {
                            jsonTextReader.Read();
                            JsonSerializer jsonSerializer = new JsonSerializer();
                            result = new T?(jsonSerializer.Deserialize<T>(jsonTextReader));
                            return result;
                        }
                    }
                    result = null;
                }
            }
            return result;
        }
        public static List<T> GetFirstInstanceList<T>(string propertyName, string path)
        {
            List<T> result;
            using (StreamReader streamReader = new StreamReader(path))
            {
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    while (jsonTextReader.Read())
                    {
                        if (jsonTextReader.TokenType == JsonToken.PropertyName && jsonTextReader.Value.ToString().ToLower() == propertyName.ToLower())
                        {
                            jsonTextReader.Read();
                            JsonSerializer jsonSerializer = new JsonSerializer();
                            result = jsonSerializer.Deserialize<List<T>>(jsonTextReader);
                            return result;
                        }
                    }
                    result = new List<T>();
                }
            }
            return result;
        }
    }
}
