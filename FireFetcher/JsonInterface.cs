using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireFetcher
{
    internal class JsonInterface
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public void WriteToJson(object Data, string FilePath)
        {
            FileStream JsonFile = File.Create(FilePath);
            var JsonWriter = new Utf8JsonWriter(JsonFile);
            JsonSerializer.Serialize(JsonWriter, Data, WriteOptions);
            JsonFile.Close();
        }
    }
}
