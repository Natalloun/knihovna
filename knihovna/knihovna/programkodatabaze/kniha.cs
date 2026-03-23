using MongoDB.Bson;

public class Knih
{
    public ObjectId _id;
    public string title;
    public string author;
    public string isbn;
    public int year;
    public string category;

    public Knih(string title, string author, string isbn, int year, string category)
    {
        this.title = title;
        this.author = author;
        this.isbn = isbn;
        this.year = year;
        this.category = category;
    }
}