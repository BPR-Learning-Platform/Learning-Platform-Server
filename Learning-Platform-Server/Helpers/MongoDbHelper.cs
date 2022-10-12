using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Learning_Platform_Server.Helpers
{
    public class MongoDbHelper
    {
        public static BsonArray MapToBsonArray(HttpResponseMessage httpResponseMessage)
        {
            string? bsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            BsonArray bsonArray = BsonSerializer.Deserialize<BsonArray>(bsonString);
            return bsonArray;
        }

        public static string? MapToJson(BsonValue bsonValue)
        {
            {
                JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };
                return bsonValue.ToJson(jsonWriterSettings);
            }
        }

        public static HttpClient GetHttpClient()
        {
            return new();
        }
    }
}
