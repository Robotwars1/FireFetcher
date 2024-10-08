﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireFetcher
{
    internal class JsonInterface
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public void WriteToJson(object Data, string FilePath)
        {
            FileStream JsonFile = File.Create(FilePath);
            var JsonWriter = new Utf8JsonWriter(JsonFile);
            JsonSerializer.Serialize(JsonWriter, Data, WriteOptions);
            JsonFile.Close();

            // Log write operation
            Logger.JsonLog(Data.ToString(), FilePath);
        }

        public object ReadJson(string FilePath, string ReturnValueType)
        {
            object? ReadResult = null;

            FileStream JsonFile = File.OpenRead(FilePath);
            try
            {
                switch (ReturnValueType)
                {
                    case "Users":
                        ReadResult = JsonSerializer.Deserialize<List<Program.Username>>(JsonFile, ReadOptions);
                        break;
                    case "ID":
                        ReadResult = JsonSerializer.Deserialize<ulong>(JsonFile, ReadOptions);
                        break;
                }
            }
            catch
            {
                Logger.GeneralLog($"Failed to get data from file: {FilePath}");
            }
            JsonFile.Close();

            return ReadResult;
        }
    }
}
