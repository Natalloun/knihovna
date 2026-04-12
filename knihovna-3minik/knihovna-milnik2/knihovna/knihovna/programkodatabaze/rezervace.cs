using MongoDB.Bson;
namespace knihovna;

public class Reservation
{
    public ObjectId _id;
    public ObjectId bookId;
    public ObjectId readerId;
    public DateTime createdAt;
    public string status;
    public int queuePosition;

    public Reservation(ObjectId bookId, ObjectId readerId, int queuePosition)
    {
        this.bookId = bookId;
        this.readerId = readerId;
        this.createdAt = DateTime.Now;
        this.queuePosition = queuePosition;
        this.status = "waiting";
    }
}