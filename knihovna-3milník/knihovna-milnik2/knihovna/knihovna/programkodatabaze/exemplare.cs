using MongoDB.Bson;

namespace Knihovna;

public class Exem
{
    public ObjectId _id;
    public ObjectId bookId;
    public string inventoryNumber;
    public string status;

    public Exem(ObjectId bookId, string inventoryNumber)
    {
        this.bookId = bookId;
        this.inventoryNumber = inventoryNumber;
        this.status = "available";
    }
}