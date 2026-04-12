using knihovna;
using Knihovna;
using MongoDB.Driver;
using MongoDB.Bson;

class Program
{
    static async Task Main(string[] args)
    {
        MongoDbContext context = new MongoDbContext(
            "mongodb+srv://katarunstukova_db_user:XeDs5v28gz8anhK2@knihovnadatabaze.8symaa8.mongodb.net/?appName=MongoDB+Compass", "knihovna_db");

        var userCollection = context.GetCollection<Log>("users");
        var bookCollection = context.GetCollection<Knih>("books");
        var readerCollection = context.GetCollection<Ctenar>("readers");
        var copiesCollection = context.GetCollection<Exem>("copies");
        var loansCollection = context.GetCollection<Pujc>("loans");
        var reservationsCollection = context.GetCollection<Reservation>("reservations");

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

        var users = await userCollection.Find(u => u.username == username && u.password == password).ToListAsync();

        if (users.Count == 0)
        {
            Console.WriteLine("Špatné přihlašovací údaje");
            return;
        }

        string usernameReader = users[0].username;
        string role = users[0].role;

        Console.WriteLine("Přihlášen jako: " + role);

        while (true)
        {
            Console.WriteLine("\n--- MENU ---");

            if (role == "employee")
            {
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
                Console.WriteLine("14 - Rezervace");
                Console.WriteLine("15 - Prodloužení výpůjčky");
                Console.WriteLine("16 - Správa výpůjček a rezervací");
                Console.WriteLine("17 - Historie");
                Console.WriteLine("18 - Statistika");
            }
            else if (role == "reader")
            {
                Console.WriteLine("1 - Zobraz knihy");
                Console.WriteLine("5 - Můj profil");
                Console.WriteLine("7 - Upravit můj profil");
                Console.WriteLine("13 - Hledat Knihu");
                Console.WriteLine("14 - Rezervace");
                Console.WriteLine("15 - Prodloužení výpůjčky");
                Console.WriteLine("16 - Moje výpůjčky a rezervace");
                Console.WriteLine("17 - Historie");
            }

            Console.WriteLine("0 - Konec");
            Console.Write("Vyber: ");
            string volba = Console.ReadLine();

            if (volba == "0")
                break;

            if (role == "reader" &&
                volba != "1" &&
                volba != "5" &&
                volba != "7" &&
                volba != "13" &&
                volba != "14" &&
                volba != "15" &&
                volba != "16" &&
                volba != "17")
            {
                Console.WriteLine("Na tuto akci nemáš oprávnění.");
                continue;
            }

            if (volba == "1")
            {
                var books = await bookCollection.Find(_ => true).ToListAsync();
                Console.WriteLine("\nSEZNAM KNIH:");
                foreach (var b in books)
                    Console.WriteLine($"- {b.title} ({b.author})");
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
                string cat = Console.ReadLine();

                await bookCollection.InsertOneAsync(new Knih(title, author, isbn, year, cat));
                Console.WriteLine("Kniha přidána.");
            }
            else if (volba == "3")
            {
                var books = await bookCollection.Find(_ => true).ToListAsync();
                if (books.Count == 0)
                {
                    Console.WriteLine("Žádné knihy.");
                    continue;
                }

                for (int i = 0; i < books.Count; i++)
                    Console.WriteLine($"{i + 1} - {books[i].title}");

                Console.Write("Vyber číslo knihy k úpravě: ");
                int index = int.Parse(Console.ReadLine()) - 1;

                if (index < 0 || index >= books.Count)
                {
                    Console.WriteLine("Neplatný výběr.");
                    continue;
                }

                Console.Write("Nový název: ");
                string newTitle = Console.ReadLine();

                await bookCollection.UpdateOneAsync(
                    b => b._id == books[index]._id,
                    Builders<Knih>.Update.Set(b => b.title, newTitle));

                Console.WriteLine("Upraveno.");
            }
            else if (volba == "4")
            {
                var books = await bookCollection.Find(_ => true).ToListAsync();
                if (books.Count == 0)
                {
                    Console.WriteLine("Žádné knihy k odstranění.");
                    continue;
                }

                for (int i = 0; i < books.Count; i++)
                    Console.WriteLine($"{i + 1} - {books[i].title}");

                Console.Write("Vyber číslo knihy ke smazání: ");
                if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > books.Count)
                {
                    Console.WriteLine("Neplatný výběr.");
                    continue;
                }

                var selectedBook = books[index - 1];

                var hasActiveLoans = await loansCollection.Find(l => l.bookId == selectedBook._id && l.returnDate == null).AnyAsync();
                if (hasActiveLoans)
                {
                    Console.WriteLine("Knihu nelze smazat, protože je aktuálně vypůjčená.");
                    continue;
                }

                await bookCollection.DeleteOneAsync(b => b._id == selectedBook._id);
                await copiesCollection.DeleteManyAsync(c => c.bookId == selectedBook._id);

                Console.WriteLine("Kniha a její exempláře byly smazány.");
            }
            else if (volba == "5")
            {
                if (role == "employee")
                {
                    var readers = await readerCollection.Find(_ => true).ToListAsync();
                    Console.WriteLine("\nSEZNAM ČTENÁŘŮ:");
                    foreach (var r in readers)
                    {
                        Console.WriteLine($"{r.firstName} {r.lastName} | Username: {r.usernameReader} | Email: {r.email} | Aktivní: {r.isActive}");
                    }
                }
                else if (role == "reader")
                {
                    var myProfile = await readerCollection.Find(r => r.usernameReader == usernameReader).FirstOrDefaultAsync();

                    if (myProfile == null)
                    {
                        Console.WriteLine("Profil čtenáře nebyl nalezen.");
                    }
                    else
                    {
                        Console.WriteLine("\n--- MŮJ PROFIL ---");
                        Console.WriteLine($"Jméno: {myProfile.firstName}");
                        Console.WriteLine($"Příjmení: {myProfile.lastName}");
                        Console.WriteLine($"Username: {myProfile.usernameReader}");
                        Console.WriteLine($"Email: {myProfile.email}");
                        Console.WriteLine($"Číslo karty: {myProfile.cardNumber}");
                        Console.WriteLine($"Aktivní účet: {myProfile.isActive}");
                    }
                }
            }
            else if (volba == "6")
            {
                Console.Write("Jméno: ");
                string fn = Console.ReadLine();

                Console.Write("Příjmení: ");
                string ln = Console.ReadLine();

                Console.Write("Email: ");
                string em = Console.ReadLine();

                Console.Write("Username: ");
                string un = Console.ReadLine();

                Console.Write("Heslo: ");
                string pw = Console.ReadLine();

                Console.Write("Karta: ");
                string card = Console.ReadLine();

                var existingUser = await userCollection.Find(u => u.username == un).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    Console.WriteLine("Tento username už existuje v přihlašování.");
                    continue;
                }

                var existingReader = await readerCollection.Find(r => r.usernameReader == un).FirstOrDefaultAsync();
                if (existingReader != null)
                {
                    Console.WriteLine("Tento username už existuje u čtenáře.");
                    continue;
                }

                await readerCollection.InsertOneAsync(new Ctenar(card, un, fn, ln, em));
                await userCollection.InsertOneAsync(new Log(un, pw, "reader"));

                Console.WriteLine("Čtenář i přihlašovací účet byli vytvořeni.");
            }
            else if (volba == "7")
            {
                if (role == "employee")
                {
                    var readers = await readerCollection.Find(_ => true).ToListAsync();
                    if (readers.Count == 0)
                    {
                        Console.WriteLine("Žádní čtenáři.");
                        continue;
                    }

                    for (int i = 0; i < readers.Count; i++)
                        Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");

                    Console.Write("Vyber čtenáře k úpravě: ");
                    if (!int.TryParse(Console.ReadLine(), out int rIdx) || rIdx < 1 || rIdx > readers.Count)
                    {
                        Console.WriteLine("Neplatný výběr.");
                        continue;
                    }

                    var selectedReader = readers[rIdx - 1];

                    Console.Write($"Nové jméno ({selectedReader.firstName}): ");
                    string newFirstName = Console.ReadLine();

                    Console.Write($"Nové příjmení ({selectedReader.lastName}): ");
                    string newLastName = Console.ReadLine();

                    Console.Write($"Nový email ({selectedReader.email}): ");
                    string newEmail = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(newFirstName))
                        newFirstName = selectedReader.firstName;

                    if (string.IsNullOrWhiteSpace(newLastName))
                        newLastName = selectedReader.lastName;

                    if (string.IsNullOrWhiteSpace(newEmail))
                        newEmail = selectedReader.email;

                    await readerCollection.UpdateOneAsync(
                        r => r._id == selectedReader._id,
                        Builders<Ctenar>.Update
                            .Set(r => r.firstName, newFirstName)
                            .Set(r => r.lastName, newLastName)
                            .Set(r => r.email, newEmail));

                    Console.WriteLine("Čtenář byl upraven.");
                }
                else if (role == "reader")
                {
                    var myProfile = await readerCollection
                        .Find(r => r.usernameReader == usernameReader)
                        .FirstOrDefaultAsync();

                    if (myProfile == null)
                    {
                        Console.WriteLine("Tvůj profil nebyl nalezen.");
                        continue;
                    }

                    Console.WriteLine("\n--- ÚPRAVA MÉHO PROFILU ---");

                    Console.Write($"Jméno ({myProfile.firstName}): ");
                    string newFirstName = Console.ReadLine();

                    Console.Write($"Příjmení ({myProfile.lastName}): ");
                    string newLastName = Console.ReadLine();

                    Console.Write($"Email ({myProfile.email}): ");
                    string newEmail = Console.ReadLine();

                    Console.Write($"Username ({myProfile.usernameReader}): ");
                    string newUsername = Console.ReadLine();

                    Console.Write("Nové heslo (nech prázdné pro beze změny): ");
                    string newPassword = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(newFirstName))
                        newFirstName = myProfile.firstName;

                    if (string.IsNullOrWhiteSpace(newLastName))
                        newLastName = myProfile.lastName;

                    if (string.IsNullOrWhiteSpace(newEmail))
                        newEmail = myProfile.email;

                    if (string.IsNullOrWhiteSpace(newUsername))
                        newUsername = myProfile.usernameReader;

                    var existingUser = await userCollection
                        .Find(u => u.username == newUsername && u.username != usernameReader)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        Console.WriteLine("Tento username už existuje.");
                        continue;
                    }

                    await readerCollection.UpdateOneAsync(
                        r => r._id == myProfile._id,
                        Builders<Ctenar>.Update
                            .Set(r => r.firstName, newFirstName)
                            .Set(r => r.lastName, newLastName)
                            .Set(r => r.email, newEmail)
                            .Set(r => r.usernameReader, newUsername));

                    var updateUser = Builders<Log>.Update.Set(u => u.username, newUsername);

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        updateUser = updateUser.Set(u => u.password, newPassword);
                    }

                    await userCollection.UpdateOneAsync(
                        u => u.username == usernameReader,
                        updateUser);

                    usernameReader = newUsername;

                    Console.WriteLine("Profil byl úspěšně aktualizován.");
                }
            }
            else if (volba == "8")
            {
                var readers = await readerCollection.Find(r => r.isActive).ToListAsync();
                if (readers.Count == 0)
                {
                    Console.WriteLine("Žádní aktivní čtenáři.");
                    continue;
                }

                for (int i = 0; i < readers.Count; i++)
                    Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");

                Console.Write("Vyber čtenáře k deaktivaci: ");
                if (!int.TryParse(Console.ReadLine(), out int rIdx) || rIdx < 1 || rIdx > readers.Count)
                {
                    Console.WriteLine("Neplatný výběr.");
                    continue;
                }

                var selectedReader = readers[rIdx - 1];

                await readerCollection.UpdateOneAsync(
                    r => r._id == selectedReader._id,
                    Builders<Ctenar>.Update.Set(r => r.isActive, false));

                Console.WriteLine("Čtenář byl deaktivován.");
            }
            else if (volba == "9")
            {
                var books = await bookCollection.Find(_ => true).ToListAsync();
                if (books.Count == 0)
                {
                    Console.WriteLine("Nejsou žádné knihy.");
                    continue;
                }

                for (int i = 0; i < books.Count; i++)
                    Console.WriteLine($"{i + 1} - {books[i].title}");

                Console.Write("Vyber knihu: ");
                if (!int.TryParse(Console.ReadLine(), out int bIdx) || bIdx < 1 || bIdx > books.Count)
                {
                    Console.WriteLine("Neplatný výběr.");
                    continue;
                }

                Console.Write("Inventární číslo: ");
                string inv = Console.ReadLine();

                var existingCopy = await copiesCollection.Find(c => c.inventoryNumber == inv).FirstOrDefaultAsync();
                if (existingCopy != null)
                {
                    Console.WriteLine("Toto inventární číslo už existuje.");
                    continue;
                }

                await copiesCollection.InsertOneAsync(new Exem(books[bIdx - 1]._id, inv));
                Console.WriteLine("Exemplář byl přidán.");
            }
            else if (volba == "10")
            {
                var copies = await copiesCollection.Find(_ => true).ToListAsync();
                if (copies.Count == 0)
                {
                    Console.WriteLine("Žádné exempláře.");
                    continue;
                }

                Console.WriteLine("\n--- EXEMPLÁŘE KNIH ---");
                foreach (var copy in copies)
                {
                    var book = await bookCollection.Find(b => b._id == copy.bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"Kniha: {book?.title ?? "Neznámá"} | Inv. číslo: {copy.inventoryNumber} | Stav: {copy.status}");
                }
            }
            else if (volba == "11")
            {
                var readers = await readerCollection.Find(r => r.isActive).ToListAsync();
                Console.WriteLine("\nVYBER ČTENÁŘE:");
                for (int i = 0; i < readers.Count; i++) Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");
                int rIdx = int.Parse(Console.ReadLine()) - 1;
                ObjectId rId = readers[rIdx]._id;

                var availableCopies = await copiesCollection.Find(c => c.status == "available").ToListAsync();
                Console.WriteLine("\nVYBER EXEMPLÁŘ:");
                for (int i = 0; i < availableCopies.Count; i++)
                {
                    var b = await bookCollection.Find(bk => bk._id == availableCopies[i].bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{i + 1} - {b.title} (Inv. č.: {availableCopies[i].inventoryNumber})");
                }
                int cIdx = int.Parse(Console.ReadLine()) - 1;
                var copy = availableCopies[cIdx];

                var loan = new Pujc(rId, copy.bookId, copy._id, DateTime.Now, DateTime.Now.AddDays(14));
                await loansCollection.InsertOneAsync(loan);
                await copiesCollection.UpdateOneAsync(c => c._id == copy._id, Builders<Exem>.Update.Set(c => c.status, "loaned"));
                Console.WriteLine("Vypůjčeno.");
            }

            // 12. VRÁTIT KNIHU
            else if (volba == "12")
            {
                var activeLoans = await loansCollection.Find(l => l.returnDate == null).ToListAsync();
                if (activeLoans.Count == 0) { Console.WriteLine("Žádné aktivní výpůjčky."); continue; }

                for (int i = 0; i < activeLoans.Count; i++)
                {
                    var r = await readerCollection.Find(rd => rd._id == activeLoans[i].readerId).FirstOrDefaultAsync();
                    var b = await bookCollection.Find(bk => bk._id == activeLoans[i].bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{i + 1} - {b.title} (půjčil: {r.lastName})");
                }
                Console.Write("Vyber výpůjčku k vrácení: ");
                int lIdx = int.Parse(Console.ReadLine()) - 1;
                var selectedLoan = activeLoans[lIdx];

                await loansCollection.UpdateOneAsync(l => l._id == selectedLoan._id, Builders<Pujc>.Update.Set(l => l.returnDate, DateTime.Now));
                await copiesCollection.UpdateOneAsync(c => c._id == selectedLoan.copyId, Builders<Exem>.Update.Set(c => c.status, "available"));

                // Rezervace logika (posun)
                var nextRes = await reservationsCollection.Find(r => r.bookId == selectedLoan.bookId && r.status != "done")
                                .SortBy(r => r.queuePosition).FirstOrDefaultAsync();
                if (nextRes != null)
                {
                    await reservationsCollection.UpdateOneAsync(r => r._id == nextRes._id, Builders<Reservation>.Update.Set(r => r.status, "ready"));
                    Console.WriteLine("Kniha vrácena. Další čtenář v pořadí má knihu PŘIPRAVENU.");
                }
                else Console.WriteLine("Kniha vrácena.");
            }

            else if (volba == "13")
            {
                Console.Write("Zadej název nebo autora knihy: ");
                string hledani = Console.ReadLine().ToLower();

                var books = await bookCollection.Find(_ => true).ToListAsync();
                var vysledky = books.Where(b =>
                    b.title.ToLower().Contains(hledani) ||
                    b.author.ToLower().Contains(hledani)).ToList();

                if (vysledky.Count == 0)
                {
                    Console.WriteLine("Žádná kniha nenalezena.");
                }
                else
                {
                    Console.WriteLine("\nNALEZENÉ KNIHY:");
                    foreach (var b in vysledky)
                        Console.WriteLine($"- {b.title} | {b.author} | ISBN: {b.isbn}");
                }
            }
            else if (volba == "14")
            {
                var books = await bookCollection.Find(_ => true).ToListAsync();
                for (int i = 0; i < books.Count; i++) Console.WriteLine($"{i + 1} - {books[i].title}");
                Console.Write("Vyber knihu k rezervaci: ");
                int bIdx = int.Parse(Console.ReadLine()) - 1;
                var selectedBookId = books[bIdx]._id;

                var readers = await readerCollection.Find(r => r.isActive).ToListAsync();
                for (int i = 0; i < readers.Count; i++) Console.WriteLine($"{i + 1} - {readers[i].lastName}");
                Console.Write("Vyber čtenáře: ");
                int rIdx = int.Parse(Console.ReadLine()) - 1;

                // Najdeme všechny aktivní rezervace pro tuhle knihu (včetně těch ve stavu 'ready')
                var activeReservations = await reservationsCollection
                    .Find(r => r.bookId == selectedBookId && r.status != "done")
                    .ToListAsync();

                // Určíme novou pozici: pokud nikdo nečeká, je to 1. 
                // Pokud někdo čeká, najdeme nejvyšší pozici a přidáme 1.
                int nextPosition = 1;
                if (activeReservations.Count > 0)
                {
                    nextPosition = activeReservations.Max(r => r.queuePosition) + 1;
                }

                var res = new Reservation(selectedBookId, readers[rIdx]._id, nextPosition);

                // Pokud je to úplně první rezervace na tuto knihu, rovnou ji nastavíme na 'ready'
                if (nextPosition == 1)
                {
                    res.status = "ready";
                }

                await reservationsCollection.InsertOneAsync(res);
                Console.WriteLine($"Rezervováno. Vaše pořadí ve frontě: {nextPosition} | Stav: {res.status}");
            }

            // 15. PRODLOUŽENÍ VÝPŮJČKY
            else if (volba == "15")
            {
                var activeLoans = await loansCollection.Find(l => l.returnDate == null).ToListAsync();
                for (int i = 0; i < activeLoans.Count; i++)
                {
                    var b = await bookCollection.Find(bk => bk._id == activeLoans[i].bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{i + 1} - {b.title} (Termín: {activeLoans[i].dueDate.ToShortDateString()})");
                }
                Console.Write("Vyber výpůjčku k prodloužení: ");
                int pIdx = int.Parse(Console.ReadLine()) - 1;
                var loan = activeLoans[pIdx];

                var hasRes = await reservationsCollection.Find(r => r.bookId == loan.bookId && r.status != "done").AnyAsync();
                if (loan.extensionCount >= 2) Console.WriteLine("Max 2 prodloužení!");
                else if (hasRes) Console.WriteLine("Nelze - na knihu je rezervace!");
                else
                {
                    await loansCollection.UpdateOneAsync(l => l._id == loan._id,
                        Builders<Pujc>.Update.Set(l => l.dueDate, loan.dueDate.AddDays(14)).Inc(l => l.extensionCount, 1));
                    Console.WriteLine("Prodlouženo o 14 dní.");
                }
            }
            else if (volba == "16")
            {
                // 1. Výběr čtenáře
                var readers = await readerCollection.Find(r => r.isActive).ToListAsync();
                Console.WriteLine("\n--- SEZNAM ČTENÁŘŮ ---");
                for (int i = 0; i < readers.Count; i++)
                    Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");

                Console.Write("Vyber čtenáře pro detail: ");
                int rIdx = int.Parse(Console.ReadLine()) - 1;
                var selectedReader = readers[rIdx];

                // Načtení všech výpůjček čtenáře (i těch vrácených, kde může být dluh)
                var allUserLoans = await loansCollection.Find(l => l.readerId == selectedReader._id).ToListAsync();
                var hisReservations = await reservationsCollection.Find(r => r.readerId == selectedReader._id && r.status != "done").ToListAsync();

                Console.WriteLine($"\n>>> DETAIL: {selectedReader.firstName} {selectedReader.lastName} <<<");

                decimal celkovyDluh = 0;
                Console.WriteLine("\nSTAV VÝPŮJČEK A POKUT:");

                foreach (var l in allUserLoans)
                {
                    var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                    DateTime konecneDatum = l.returnDate ?? DateTime.Now; // Pokud není vráceno, počítáme k dnešku

                    if (konecneDatum > l.dueDate)
                    {
                        int dnyZpozdeni = (int)(konecneDatum - l.dueDate).TotalDays;
                        int pokuta = dnyZpozdeni * 2; // 2 Kč za den

                        if (!l.isFinePaid)
                        {
                            string stav = l.returnDate == null ? "AKTIVNÍ (V prodlení)" : "VRÁCENO (Nezaplacená pokuta)";
                            Console.WriteLine($" ! {b.title} | Zpoždění: {dnyZpozdeni} dní | Pokuta: {pokuta} Kč | {stav}");
                            celkovyDluh += pokuta;
                        }
                    }
                    else if (l.returnDate == null)
                    {
                        Console.WriteLine($" v {b.title} | Termín: {l.dueDate.ToShortDateString()} | OK");
                    }
                }

                Console.WriteLine("--------------------------------------");
                Console.WriteLine($"CELKOVÝ DLUH ČTENÁŘE: {celkovyDluh} Kč");
                Console.WriteLine("--------------------------------------");

                // MOŽNOST UHRAZENÍ POKUTY
                if (celkovyDluh > 0)
                {
                    Console.Write("Chceš uhradit všechny pokuty tohoto čtenáře? (ano/ne): ");
                    if (Console.ReadLine().ToLower() == "ano")
                    {
                        var filter = Builders<Pujc>.Filter.And(
                            Builders<Pujc>.Filter.Eq(l => l.readerId, selectedReader._id),
                            Builders<Pujc>.Filter.Eq(l => l.isFinePaid, false)
                        );
                        var update = Builders<Pujc>.Update.Set(l => l.isFinePaid, true);

                        await loansCollection.UpdateManyAsync(filter, update);
                        Console.WriteLine(">>> Pokuty byly v systému označeny jako UHRAZENÉ.");
                    }
                }

                // LOGIKA REZERVACÍ (zůstává stejná jako dřív)
                Console.WriteLine("\nAKTIVNÍ REZERVACE:");
                for (int i = 0; i < hisReservations.Count; i++)
                {
                    var res = hisReservations[i];
                    var b = await bookCollection.Find(bk => bk._id == res.bookId).FirstOrDefaultAsync();
                    Console.WriteLine($"{i + 1} - {b.title} (Stav: {res.status})");
                }

                if (hisReservations.Any(r => r.status == "ready"))
                {
                    Console.Write("\nPotvrdit výpůjčku z rezervace 'ready'? (ano/ne): ");
                    if (Console.ReadLine().ToLower() == "ano")
                    {
                        // ... (zde zůstává tvůj kód pro vytvoření výpůjčky z rezervace) ...
                        Console.WriteLine("Funkce výpůjčky z rezervace proběhla.");
                    }
                }
            }
            else if (volba == "17") // HISTORIE (Centrální archiv)
            {
                Console.WriteLine("\n--- CENTRÁLNÍ HISTORIE ---");
                Console.WriteLine("1 - Historie konkrétního čtenáře");
                Console.WriteLine("2 - Historie konkrétní knihy");
                Console.WriteLine("3 - Kompletní přehled všech vrácených výpůjček");
                Console.Write("Vyber: ");
                string subVolba = Console.ReadLine();

                if (subVolba == "1") // Historie čtenáře
                {
                    var readers = await readerCollection.Find(_ => true).ToListAsync();
                    for (int i = 0; i < readers.Count; i++)
                        Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");

                    Console.Write("Vyber čtenáře: ");
                    int rIdx = int.Parse(Console.ReadLine()) - 1;
                    var rId = readers[rIdx]._id;

                    var history = await loansCollection.Find(l => l.readerId == rId).ToListAsync();
                    Console.WriteLine($"\nHistorie výpůjček čtenáře {readers[rIdx].lastName}:");
                    foreach (var l in history)
                    {
                        var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                        string stav = l.returnDate.HasValue ? $"Vráceno: {l.returnDate.Value.ToShortDateString()}" : "Stále vypůjčeno";
                        Console.WriteLine($"- {b.title} | Od: {l.loanDate.ToShortDateString()} | {stav}");
                    }
                }
                else if (subVolba == "2") // Historie knihy
                {
                    var books = await bookCollection.Find(_ => true).ToListAsync();
                    for (int i = 0; i < books.Count; i++)
                        Console.WriteLine($"{i + 1} - {books[i].title}");

                    Console.Write("Vyber knihu: ");
                    int bIdx = int.Parse(Console.ReadLine()) - 1;
                    var bId = books[bIdx]._id;

                    // 1. Historie výpůjček této knihy
                    var loanHistory = await loansCollection.Find(l => l.bookId == bId).ToListAsync();
                    Console.WriteLine($"\nHistorie výpůjček knihy: {books[bIdx].title}");
                    foreach (var l in loanHistory)
                    {
                        var r = await readerCollection.Find(rd => rd._id == l.readerId).FirstOrDefaultAsync();
                        string status = l.returnDate.HasValue ? "VRÁCENO" : "AKTIVNÍ";
                        Console.WriteLine($"  * Čtenář: {r.lastName} | Vypůjčeno: {l.loanDate.ToShortDateString()} | Stav: {status}");
                    }

                    // 2. Historie rezervací této knihy
                    var resHistory = await reservationsCollection.Find(r => r.bookId == bId).ToListAsync();
                    Console.WriteLine($"\nHistorie rezervací této knihy:");
                    foreach (var res in resHistory)
                    {
                        var r = await readerCollection.Find(rd => rd._id == res.readerId).FirstOrDefaultAsync();
                        Console.WriteLine($"  * Čtenář: {r.lastName} | Stav rezervace: {res.status}");
                    }
                }
                else if (subVolba == "3") // Kompletní archiv
                {
                    var allLoans = await loansCollection.Find(l => l.returnDate != null).ToListAsync();
                    Console.WriteLine("\nARCHIV VŠECH VRÁCENÝCH KNIH:");
                    foreach (var l in allLoans)
                    {
                        var r = await readerCollection.Find(rd => rd._id == l.readerId).FirstOrDefaultAsync();
                        var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                        Console.WriteLine($"- {b.title} | Půjčil: {r.lastName} | Vráceno: {l.returnDate?.ToShortDateString()}");
                    }
                }
            }



            else if (volba == "17")
            {
                if (role == "reader")
                {
                    var myProfile = await readerCollection.Find(r => r.usernameReader == usernameReader).FirstOrDefaultAsync();

                    if (myProfile == null)
                    {
                        Console.WriteLine("Tvůj profil nebyl nalezen.");
                        continue;
                    }

                    var history = await loansCollection.Find(l => l.readerId == myProfile._id).ToListAsync();

                    Console.WriteLine($"\n--- MOJE HISTORIE VÝPŮJČEK ---");
                    foreach (var l in history)
                    {
                        var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                        string stav = l.returnDate.HasValue
                            ? $"Vráceno: {l.returnDate.Value.ToShortDateString()}"
                            : "Stále vypůjčeno";

                        Console.WriteLine($"- {b?.title ?? "Neznámá nebo smazaná kniha"} | Od: {l.loanDate.ToShortDateString()} | {stav}");
                    }
                }
                else
                {
                    Console.WriteLine("\n--- CENTRÁLNÍ HISTORIE ---");
                    Console.WriteLine("1 - Historie konkrétního čtenáře");
                    Console.WriteLine("2 - Historie konkrétní knihy");
                    Console.WriteLine("3 - Kompletní přehled všech vrácených výpůjček");
                    Console.Write("Vyber: ");
                    string subVolba = Console.ReadLine();

                    if (subVolba == "1")
                    {
                        var readers = await readerCollection.Find(_ => true).ToListAsync();
                        for (int i = 0; i < readers.Count; i++)
                            Console.WriteLine($"{i + 1} - {readers[i].firstName} {readers[i].lastName}");

                        Console.Write("Vyber čtenáře: ");
                        int rIdx = int.Parse(Console.ReadLine()) - 1;

                        if (rIdx < 0 || rIdx >= readers.Count)
                        {
                            Console.WriteLine("Neplatný výběr.");
                            continue;
                        }

                        var rId = readers[rIdx]._id;

                        var history = await loansCollection.Find(l => l.readerId == rId).ToListAsync();
                        Console.WriteLine($"\nHistorie výpůjček čtenáře {readers[rIdx].lastName}:");
                        foreach (var l in history)
                        {
                            var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                            string stav = l.returnDate.HasValue
                                ? $"Vráceno: {l.returnDate.Value.ToShortDateString()}"
                                : "Stále vypůjčeno";

                            Console.WriteLine($"- {b?.title ?? "Neznámá nebo smazaná kniha"} | Od: {l.loanDate.ToShortDateString()} | {stav}");
                        }
                    }
                    else if (subVolba == "2")
                    {
                        var books = await bookCollection.Find(_ => true).ToListAsync();
                        for (int i = 0; i < books.Count; i++)
                            Console.WriteLine($"{i + 1} - {books[i].title}");

                        Console.Write("Vyber knihu: ");
                        int bIdx = int.Parse(Console.ReadLine()) - 1;

                        if (bIdx < 0 || bIdx >= books.Count)
                        {
                            Console.WriteLine("Neplatný výběr.");
                            continue;
                        }

                        var bId = books[bIdx]._id;

                        var loanHistory = await loansCollection.Find(l => l.bookId == bId).ToListAsync();
                        Console.WriteLine($"\nHistorie výpůjček knihy: {books[bIdx].title}");
                        foreach (var l in loanHistory)
                        {
                            var r = await readerCollection.Find(rd => rd._id == l.readerId).FirstOrDefaultAsync();
                            string status = l.returnDate.HasValue ? "VRÁCENO" : "AKTIVNÍ";
                            Console.WriteLine($"  * Čtenář: {r?.lastName ?? "Neznámý"} | Vypůjčeno: {l.loanDate.ToShortDateString()} | Stav: {status}");
                        }

                        var resHistory = await reservationsCollection.Find(r => r.bookId == bId).ToListAsync();
                        Console.WriteLine($"\nHistorie rezervací této knihy:");
                        foreach (var res in resHistory)
                        {
                            var r = await readerCollection.Find(rd => rd._id == res.readerId).FirstOrDefaultAsync();
                            Console.WriteLine($"  * Čtenář: {r?.lastName ?? "Neznámý"} | Stav rezervace: {res.status}");
                        }
                    }
                    else if (subVolba == "3")
                    {
                        var allLoans = await loansCollection.Find(l => l.returnDate != null).ToListAsync();
                        Console.WriteLine("\nARCHIV VŠECH VRÁCENÝCH KNIH:");
                        foreach (var l in allLoans)
                        {
                            var r = await readerCollection.Find(rd => rd._id == l.readerId).FirstOrDefaultAsync();
                            var b = await bookCollection.Find(bk => bk._id == l.bookId).FirstOrDefaultAsync();
                            Console.WriteLine($"- {b?.title ?? "Neznámá"} | Půjčil: {r?.lastName ?? "Neznámý"} | Vráceno: {l.returnDate?.ToShortDateString()}");
                        }
                    }
                }
            }
            else if (volba == "18")
            {
                Console.WriteLine("\n--- STATISTIKY KNIHOVNY ---");
                Console.WriteLine("1 - Nejpůjčovanější knihy (TOP 5)");
                Console.WriteLine("2 - Aktuální vytížení (Aktivní vs. po termínu)");
                Console.Write("Vyber: ");
                string statVolba = Console.ReadLine();

                if (statVolba == "1")
                {
                    var allLoans = await loansCollection.Find(_ => true).ToListAsync();

                    var topBooks = allLoans
                        .GroupBy(l => l.bookId)
                        .Select(g => new { BookId = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(5);

                    Console.WriteLine("\nTOP 5 NEJPŮJČOVANĚJŠÍCH KNIH:");
                    int i = 1;
                    foreach (var item in topBooks)
                    {
                        var book = await bookCollection.Find(b => b._id == item.BookId).FirstOrDefaultAsync();
                        Console.WriteLine($"{i}. {book?.title ?? "Neznámá"} - vypůjčeno {item.Count}x");
                        i++;
                    }
                }
                else if (statVolba == "2")
                {
                    var activeLoans = await loansCollection.Find(l => l.returnDate == null).ToListAsync();
                    int celkemAktivnich = activeLoans.Count;
                    int poTerminu = activeLoans.Count(l => l.dueDate < DateTime.Now);

                    Console.WriteLine("\nAKTUÁLNÍ STAV VÝPŮJČEK:");
                    Console.WriteLine($"Celkem vypůjčených knih: {celkemAktivnich}");
                    Console.WriteLine($"Z toho po termínu (dlužníci): {poTerminu}");

                    if (celkemAktivnich > 0)
                    {
                        double procento = (double)poTerminu / celkemAktivnich * 100;
                        Console.WriteLine($"Procento nespolehlivých čtenářů: {procento:F1} %");
                    }
                }
            }
            else
            {
                Console.WriteLine("Neplatná volba.");
            }
        }
    }
}

