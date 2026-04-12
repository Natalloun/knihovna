using MongoDB.Bson;

namespace Knihovna;

public class Log
{
    public ObjectId _id;
    public string username;
    public string password;
    public string role; 

    public Log(string username, string password, string role)
    {
        this.username = username;
        this.password = password;
        this.role = role;
    }
}