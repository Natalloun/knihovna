using Knihovna;
using MongoDB.Driver;

class Program
{
    static async Task Main(string[] args)
    {
        MongoDbContext context = new MongoDbContext(
            "mongodb://localhost:27017", "knihovna_db");

        var userCollection = context.GetCollection<Log>("users");
        var bookCollection = context.GetCollection<Knih>("books");

        if (await userCollection.Find(_ => true).AnyAsync() == false)
        {
            await userCollection.InsertOneAsync(new Log("admin", "admin", "employee"));
            await userCollection.InsertOneAsync(new Log("reader", "reader", "reader"));
        }

        if (await bookCollection.Find(_ => true).AnyAsync() == false)
        {
            await bookCollection.InsertOneAsync(new Knih("Babička", "Božena Němcová", "111", 1855, "povídka"));
            await bookCollection.InsertOneAsync(new Knih("Kytice", "K. J. Erben", "222", 1853, "balady"));
        }

        Console.Write("Username: ");
        string username = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        var users = await userCollection
            .Find(u => u.username == username && u.password == password)
            .ToListAsync();

        if (users.Count == 0)
        {
            Console.WriteLine("Špatné přihlašovací údaje");
            return;
        }

        Log loggedUser = users[0];
        Console.WriteLine("Přihlášen jako: " + loggedUser.role);

        var books = await bookCollection.Find(_ => true).ToListAsync();
        
        Console.WriteLine("pro zobrazení seznamu knih stiskněte s");
        var key = Console.ReadKey();
        if (key.KeyChar == 's')
        {
            var Knih = await bookCollection.Find(_ => true).ToListAsync();

            Console.WriteLine("SEZNAM KNIH");

            foreach (Knih b in books)
            {
                Console.WriteLine(b.title + " | " + b.author + " | ISBN: " + b.isbn);
            }
        }
    }
}    
    
        