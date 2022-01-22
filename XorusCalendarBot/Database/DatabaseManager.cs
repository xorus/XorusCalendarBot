using LiteDB;
using XorusCalendarBot.Configuration;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    private readonly EnvConfig _envConfig;
    private readonly LiteDatabase _db;

    public DatabaseManager(EnvConfig envConfig)
    {
        _envConfig = envConfig;
        _db = new LiteDatabase(_envConfig.DatabasePath ?? "database.db");
        var col = _db.GetCollection<CalendarEntity>("calendars");

        if (col.Count() == 0)
        {
            col.Insert(new CalendarEntity());
        }
    }

    public void DoTheThing()
    {
        // // Open database (or create if doesn't exist)
        // // Get customer collection
        //
        // // Create your new customer instance
        // var customer = new Calendar();
        //
        // // Create unique index in Name field
        // // col.EnsureIndex(x => x.CalendarId, true);
        //
        // // Insert new customer document (Id will be auto-incremented)
        // col.Insert(customer);
        //
        // // Update a document inside a collection
        // // customer.Name = "Joana Doe";
        //
        // // col.Update(customer);
        //
        // // Use LINQ to query documents (with no index)
        // var results = col.FindAll();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}