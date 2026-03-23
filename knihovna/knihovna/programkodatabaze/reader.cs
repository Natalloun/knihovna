using MongoDB.Bson;

namespace Knihovna;

public class Ctenar
{
    public ObjectId _id;
    public string cardNumber;
    public string usernameReader;
    public string firstName;
    public string lastName;
    public string email;
    public bool isActive;

    public Ctenar(string cardNumber, string usernameReader,string firstName, string lastName, string email)
    {
        this.cardNumber = cardNumber;
        this.usernameReader = usernameReader;
        this.firstName = firstName;
        this.lastName = lastName;
        this.email = email;
        this.isActive = true;
    }
}