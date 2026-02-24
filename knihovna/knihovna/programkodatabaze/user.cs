using MongoDB.Bson;

namespace Knihovna;

public class Ctenar
{
    public ObjectId _id;
    public string cardNumber;
    public string username;
    public string firstName;
    public string lastName;
    public string email;
    public bool isActive;

    public Ctenar(string cardNumber, string username,string firstName, string lastName, string email)
    {
        this.cardNumber = cardNumber;
        this.username = username;
        this.firstName = firstName;
        this.lastName = lastName;
        this.email = email;
        this.isActive = true;
    }
}