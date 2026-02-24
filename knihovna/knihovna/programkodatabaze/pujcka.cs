using MongoDB.Bson;

namespace knihovna;

public class Pujc
{
    public ObjectId _id;
    public ObjectId readerId;
    public ObjectId copyId;
    public DateTime loanDate;
    public DateTime dueDate;
    public DateTime? returnDate;
    public int renewCount;

    public Pujc(ObjectId readerId, ObjectId copyId, DateTime loanDate, DateTime dueDate)
    {
        this.readerId = readerId;
        this.copyId = copyId;
        this.loanDate = loanDate;
        this.dueDate = dueDate;
        this.returnDate = null;
        this.renewCount = 0;
    }
}