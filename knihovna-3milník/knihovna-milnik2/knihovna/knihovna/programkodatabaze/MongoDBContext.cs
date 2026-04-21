using MongoDB.Driver;

namespace Knihovna;

public class MongoDbContext
{
    private IMongoDatabase database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        MongoClient client = new MongoClient(connectionString);
        database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return database.GetCollection<T>(collectionName);
    }
}
