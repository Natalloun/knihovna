using knihovna;
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

        var reader = context.GetCollection<Ctenar>("readers");

        var copiesCollection = context.GetCollection<Exem>("copies");
        var loansCollection = context.GetCollection<Pujc>("loans");

        while (true)
        {
            Console.WriteLine("\n--- MENU ---");
            Console.WriteLine("1 - Zobraz knihy");
            Console.WriteLine("2 - Přidat knihu");
            Console.WriteLine("3 - Upravit knihu");
            Console.WriteLine("4 - Smazat knihu");
            Console.WriteLine("5 - Zobraz čtenáře");
            Console.WriteLine("6 - Přidat čtenáře");
            Console.WriteLine("7 - Upravit čtenáře");
            Console.WriteLine("8 - Deaktivovat čtenáře");
            Console.WriteLine("9 - Přidat exemplář knihy");
            Console.WriteLine("10 - Zobrazit exempláře knih");
            Console.WriteLine("11 - Vytvořit výpůjčku");
            Console.WriteLine("12 - Vrátit knihu");
            Console.WriteLine("13 - Hledat Knihu");
            Console.WriteLine("0 - Konec");

            Console.Write("Vyber: ");
            string volba = Console.ReadLine();

            if (volba == "1")
            {

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
            else if (volba == "5")
            {
                var readers = await reader.Find(_ => true).ToListAsync();

                Console.WriteLine("SEZNAM ČTENÁŘŮ");

                foreach (Ctenar c in readers)
                {
                    Console.WriteLine(
                        $"ID: {c._id} | " +
                        $"Karta: {c.cardNumber} | " +
                        $"Uživatelské jméno: {c.usernameReader} | " +
                        $"Jméno: {c.firstName} {c.lastName} | " +
                        $"Email: {c.email} | " +
                        $"Aktivní: {c.isActive}"
                    );
                }
            }
            else if (volba == "6")
            {
                Console.Write("Jméno: ");
                string firstName = Console.ReadLine();

                Console.Write("Příjmení: ");
                string lastName = Console.ReadLine();

                Console.Write("Email: ");
                string email = Console.ReadLine();

                Console.Write("Uživatelské jméno: ");
                string usernameReader = Console.ReadLine();

                Console.Write("Číslo průkazky: ");
                string cardNumber = Console.ReadLine();

                // Vytvoření čtenáře podle tvé třídy
                var newReader = new Ctenar(cardNumber, usernameReader, firstName, lastName, email);

                await reader.InsertOneAsync(newReader);

                Console.WriteLine("Čtenář přidán");
            }
            else if (volba == "7")
            {
                Console.Write("Zadej ID čtenáře: ");
                string idInput = Console.ReadLine();
                var id = MongoDB.Bson.ObjectId.Parse(idInput);

                Console.Write("Nové jméno: ");
                string firstName = Console.ReadLine();

                Console.Write("Nové příjmení: ");
                string lastName = Console.ReadLine();

                Console.Write("Nové username: ");
                string usernameReader = Console.ReadLine();

                Console.Write("Nový email: ");
                string email = Console.ReadLine();

                // Přidána změna usernameReader
                var update = Builders<Ctenar>.Update
                    .Set(c => c.firstName, firstName)
                    .Set(c => c.lastName, lastName)
                    .Set(c => c.usernameReader, usernameReader)
                    .Set(c => c.email, email);

                await reader.UpdateOneAsync(c => c._id == id, update);

                Console.WriteLine("Čtenář upraven");
            }
            else if (volba == "8")
            {
                Console.Write("Zadej ID čtenáře: ");
                string idInput = Console.ReadLine();
                var id = MongoDB.Bson.ObjectId.Parse(idInput);

                var update = Builders<Ctenar>.Update
                    .Set(c => c.isActive, false);

                await reader.UpdateOneAsync(c => c._id == id, update);

                Console.WriteLine("Čtenář deaktivován");
            }
            else if (volba == "9")
            {
                Console.WriteLine("Seznam knih:");
                foreach (var book in books)
                {
                    Console.WriteLine($"{book._id} | {book.title} | {book.author}");
                }

                Console.Write("Zadej ID knihy, ke které chceš přidat exemplář: ");
                var bookId = MongoDB.Bson.ObjectId.Parse(Console.ReadLine());

                Console.Write("Zadej inventární číslo exempláře: ");
                string inventoryNumber = Console.ReadLine();

                var newCopy = new Exem(bookId, inventoryNumber);

                await copiesCollection.InsertOneAsync(newCopy);
                Console.WriteLine("Exemplář přidán");
            }
            else if (volba == "10")
            {
                var copies = await copiesCollection.Find(_ => true).ToListAsync();

                Console.WriteLine("Seznam exemplářů:");
                foreach (var copy in copies)
                {
                    var book = await bookCollection.Find(b => b._id == copy.bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{copy._id} | {book.title} | Inventární číslo: {copy.inventoryNumber} | Stav: {copy.status}");
                }
            }
            else if (volba == "11")
            {
                Console.Write("Zadej ID čtenáře: ");
                var readerId = MongoDB.Bson.ObjectId.Parse(Console.ReadLine());

                // načti dostupné exempláře
                var availableCopies = await copiesCollection.Find(c => c.status == "available").ToListAsync();
                Console.WriteLine("Dostupné knihy k výpůjčce:");
                foreach (var copy in availableCopies)
                {
                    var book = await bookCollection.Find(b => b._id == copy.bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{copy._id} | {book.title} | {book.author} | Inventární číslo: {copy.inventoryNumber}");
                }

                Console.Write("Zadej ID exempláře k výpůjčce: ");
                var copyId = MongoDB.Bson.ObjectId.Parse(Console.ReadLine());

                var loan = new Pujc(readerId, copyId, DateTime.Now, DateTime.Now.AddDays(14));
                await loansCollection.InsertOneAsync(loan);

                // změna stavu exempláře na "loaned"
                var updateCopy = Builders<Exem>.Update.Set(c => c.status, "loaned");
                await copiesCollection.UpdateOneAsync(c => c._id == copyId, updateCopy);

                Console.WriteLine("Kniha úspěšně vypůjčena");
            }
            else if (volba == "12")
            {
                Console.Write("Zadej ID čtenáře pro vrácení knih: ");
                var readerId = MongoDB.Bson.ObjectId.Parse(Console.ReadLine());

                // aktivní výpůjčky
                var activeLoans = await loansCollection.Find(l => l.readerId == readerId && l.returnDate == null).ToListAsync();
                Console.WriteLine("Aktivní výpůjčky:");
                foreach (var loan in activeLoans)
                {
                    var copy = await copiesCollection.Find(c => c._id == loan.copyId).FirstOrDefaultAsync();
                    var book = await bookCollection.Find(b => b._id == copy.bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{loan._id} | {book.title} | {book.author} | Inventární číslo: {copy.inventoryNumber} | Termín: {loan.dueDate}");
                }

                Console.Write("Zadej ID výpůjčky k vrácení: ");
                var loanId = MongoDB.Bson.ObjectId.Parse(Console.ReadLine());

                var loanToReturn = await loansCollection.Find(l => l._id == loanId).FirstOrDefaultAsync();

                // aktualizace vrácení
                var updateLoan = Builders<Pujc>.Update.Set(l => l.returnDate, DateTime.Now);
                await loansCollection.UpdateOneAsync(l => l._id == loanId, updateLoan);

                // změna stavu exempláře zpět na "available"
                var updateCopy = Builders<Exem>.Update.Set(c => c.status, "available");
                await copiesCollection.UpdateOneAsync(c => c._id == loanToReturn.copyId, updateCopy);

                Console.WriteLine("Kniha úspěšně vrácena");
            }
            else if (volba == "13")
            {
                Console.WriteLine("Vyber typ hledání:");
                Console.WriteLine("1 - Podle názvu");
                Console.WriteLine("2 - Podle autora");
                Console.WriteLine("3 - Podle ISBN");
                Console.WriteLine("4 - Podle kategorie");
                Console.Write("Volba: ");
                string searchType = Console.ReadLine();

                Console.Write("Zadej hledaný text: ");
                string query = Console.ReadLine();

                List<Knih> results = new List<Knih>();

                if (searchType == "1")
                    results = await bookCollection.Find(b => b.title.Contains(query)).ToListAsync();
                else if (searchType == "2")
                    results = await bookCollection.Find(b => b.author.Contains(query)).ToListAsync();
                else if (searchType == "3")
                    results = await bookCollection.Find(b => b.isbn.Contains(query)).ToListAsync();
                else if (searchType == "4")
                    results = await bookCollection.Find(b => b.category.Contains(query)).ToListAsync();
                else
                {
                    Console.WriteLine("Neplatná volba");
                    return;
                }

                Console.WriteLine("Nalezené knihy:");
                foreach (var book in results)
                {
                    Console.WriteLine($"{book._id} | {book.title} | {book.author} | ISBN: {book.isbn} | Kategorie: {book.category}");
                }
            }
            else if (volba == "0")
            {
                break;
            }
        }

    }
}
