using MongoDB.Bson;

namespace Knihovna;

public class Reservation
{
    public ObjectId _id;
    public ObjectId bookId;
    public ObjectId readerId;
    public DateTime createdAt;
    public string status; 

    public Reservation(ObjectId bookId, ObjectId readerId)
    {
        this.bookId = bookId;
        this.readerId = readerId;
        this.createdAt = DateTime.Now;
        this.status = "waiting";
    }
}