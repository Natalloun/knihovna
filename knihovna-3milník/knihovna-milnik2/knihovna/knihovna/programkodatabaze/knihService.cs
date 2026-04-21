using MongoDB.Driver;
using MongoDB.Bson;

namespace Knihovna;

public class KnihService
{
    private IMongoCollection<Knih> _collection;

    public KnihService()
    {
        var client = new MongoClient("mongodb+srv://katarunstukova_db_user:XeDs5v28gz8anhK2@knihovnadatabaze.8symaa8.mongodb.net/");
        var database = client.GetDatabase("Knihovna_DB");

        _collection = database.GetCollection<Knih>("Books");
    }

    // CREATE
    public void AddBook(Knih book)
    {
        _collection.InsertOne(book);
    }

    // READ (všechny knihy)
    public List<Knih> GetAllBooks()
    {
        return _collection.Find(_ => true).ToList();
    }

    // READ (jedna kniha podle ID)
    public Knih GetBookById(ObjectId id)
    {
        return _collection.Find(b => b._id == id).FirstOrDefault();
    }

    // UPDATE
    public void UpdateBook(ObjectId id, Knih updatedBook)
    {
        var filter = Builders<Knih>.Filter.Eq(b => b._id, id);

        var update = Builders<Knih>.Update
            .Set(b => b.title, updatedBook.title)
            .Set(b => b.author, updatedBook.author)
            .Set(b => b.isbn, updatedBook.isbn)
            .Set(b => b.year, updatedBook.year)
            .Set(b => b.category, updatedBook.category);

        _collection.UpdateOne(filter, update);
    }

    // DELETE
    public void DeleteBook(ObjectId id)
    {
        _collection.DeleteOne(b => b._id == id);
    }
}