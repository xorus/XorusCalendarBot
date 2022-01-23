using LiteDB;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    private readonly LiteDatabase _db;

    public IEnumerable<CalendarEntity> CalendarEntities;
    public readonly ILiteCollection<CalendarEntity> CalendarEntityCollection;
    public readonly ILiteCollection<UserEntity> Users;

    public DatabaseManager(Env env)
    {
        _db = new LiteDatabase(env.DatabasePath ?? "database.db");
        CalendarEntityCollection = _db.GetCollection<CalendarEntity>("calendars");
        if (CalendarEntityCollection.Count() == 0)
        {
            CalendarEntityCollection.Insert(new CalendarEntity());
        }

        CalendarEntities = CalendarEntityCollection.FindAll();

        Users = _db.GetCollection<UserEntity>("users");
        if (Users.Count() == 0)
        {
            Users.Insert(new UserEntity()
            {
                Guilds = new[] { 755091710501060688u, 671289826825469975u },
                Key = "8xOwBhmEOMQq2iMo_XTXs2Sy8bdQj1rI",
                Name = "Xorus",
                DiscordId = 153932217591005184u
            });
        }

#if DEBUG
        var first = Users.FindAll().First();
        Console.WriteLine($"> {first.Id} with key {first.Key}");
#endif
    }

    public void Update(CalendarEntity calendarEntity)
    {
        CalendarEntityCollection.Update(calendarEntity);
    }

    public UserEntity? GetUser(string guid)
    {
        try
        {
            return Users.FindById(Guid.Parse(guid));
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public List<CalendarEntity> GetUserCalendars(UserEntity user)
    {
        return CalendarEntities.Where(c => user.Guilds.Contains(c.RegisterCommandsTo)).ToList();
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