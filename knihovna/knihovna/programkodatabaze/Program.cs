using Knihovna;
using MongoDB.Driver;

class Program
{
    static async Task Main(string[] args)
    {
        MongoDbContext context = new MongoDbContext(
            "mongodb+srv://katarunstukova_db_user:XeDs5v28gz8anhK2@knihovnadatabaze.8symaa8.mongodb.net/", "knihovna_db");

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


        while (true)
        {
            Console.WriteLine("\n--- MENU ---");
            Console.WriteLine("1 - Zobraz knihy");
            Console.WriteLine("2 - Přidat knihu");
            Console.WriteLine("3 - Upravit knihu");
            Console.WriteLine("4 - Smazat knihu");
            Console.WriteLine("0 - Konec");

            Console.Write("Vyber: ");
            string volba = Console.ReadLine();

            if (volba == "1")
            {
                var allbooks = await bookCollection.Find(_ => true).ToListAsync();

                Console.WriteLine("SEZNAM KNIH");

                foreach (Knih b in books)
                {
                    Console.WriteLine(b._id + " | " + b.title + " | " + b.author);
                }
            }
            else if (volba == "2")
            {
                Console.Write("Název: ");
                string title = Console.ReadLine();

                Console.Write("Autor: ");
                string author = Console.ReadLine();

                Console.Write("ISBN: ");
                string isbn = Console.ReadLine();

                Console.Write("Rok: ");
                int year = int.Parse(Console.ReadLine());

                Console.Write("Kategorie: ");
                string category = Console.ReadLine();

                var newBook = new Knih(title, author, isbn, year, category);
                await bookCollection.InsertOneAsync(newBook);

                Console.WriteLine("Kniha přidána");
            }
            else if (volba == "3")
            {
                Console.Write("Zadej ID knihy: ");
                string idInput = Console.ReadLine();
                var id = MongoDB.Bson.ObjectId.Parse(idInput);

                Console.Write("Nový název: ");
                string title = Console.ReadLine();

                var update = Builders<Knih>.Update
                    .Set(b => b.title, title);

                await bookCollection.UpdateOneAsync(b => b._id == id, update);

                Console.WriteLine("Kniha upravena");
            }
            else if (volba == "4")
            {
                Console.Write("Zadej ID knihy: ");
                string idInput = Console.ReadLine();
                var id = MongoDB.Bson.ObjectId.Parse(idInput);

                await bookCollection.DeleteOneAsync(b => b._id == id);

                Console.WriteLine("Kniha smazána");
            }
            else if (volba == "0")
            {
                break;
            }
        }
    }
}
