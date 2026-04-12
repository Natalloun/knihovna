using MongoDB.Bson;

public class Pujc
{
    public ObjectId _id;
    public ObjectId readerId;
    public ObjectId bookId;
    public ObjectId copyId;

    public DateTime loanDate;
    public DateTime dueDate;
    public DateTime? returnDate;
    public int renewCount;
    public int extensionCount;
    public bool isFinePaid;

    public Pujc(ObjectId readerId, ObjectId bookId, ObjectId copyId, DateTime loanDate, DateTime dueDate)
    {
        this.readerId = readerId;
        this.bookId = bookId;
        this.copyId = copyId;
        this.loanDate = loanDate;
        this.dueDate = dueDate;
        this.returnDate = null;
        this.renewCount = 0;
        this.extensionCount = 0;
        this.isFinePaid = false;
    }
}