using LiteDB;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    private readonly LiteDatabase _db;

    public IEnumerable<CalendarEntity> CalendarEntities;
    public readonly ILiteCollection<CalendarEntity> CalendarEntityCollection;

    public DatabaseManager(Env env)
    {
        _db = new LiteDatabase(env.DatabasePath ?? "database.db");
        CalendarEntityCollection = _db.GetCollection<CalendarEntity>("calendars");
        if (CalendarEntityCollection.Count() == 0)
        {
            CalendarEntityCollection.Insert(new CalendarEntity());
        }

        CalendarEntities = CalendarEntityCollection.FindAll();
    }

    public void Update(CalendarEntity calendarEntity)
    {
        CalendarEntityCollection.Update(calendarEntity);
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
        GC.SuppressFinalize(this);
    }
}