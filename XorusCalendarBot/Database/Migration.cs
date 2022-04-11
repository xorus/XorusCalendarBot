using LiteDB;
using Swan.DependencyInjection;

namespace XorusCalendarBot.Database;

public class Migration
{
    private readonly DependencyContainer _container;

    public Migration(DependencyContainer container)
    {
        _container = container;
    }

    public void DoIt(ILiteDatabase db)
    {
        Console.WriteLine("META: " + db.UserVersion);

        if (db.UserVersion == 0) MigrateTo1(db);
    }

    private void MigrateTo1(ILiteDatabase db)
    {
        var adminId = _container.Resolve<Env>().DiscordAdminId;

        var userCollection = db.GetCollection<BsonDocument>("users");
        var docs = userCollection.FindAll();
        foreach (var bsonDocument in docs)
        {
            if (bsonDocument.ContainsKey("DiscordId") && adminId == bsonDocument["DiscordId"].AsString)
            {
                bsonDocument["IsAdmin"] = new BsonValue(true);
                userCollection.Update(bsonDocument);
            }
        }

        db.UserVersion = 1;
    }
}