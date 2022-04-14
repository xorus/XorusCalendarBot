using EmbedIO;
using EmbedIO.WebApi;
using LiteDB;
using Swan.DependencyInjection;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class Soundboard : Base.Module
{
    private readonly LiteDatabase _db;
    public readonly ILiteCollection<SoundEntity> SoundCollection;

    public Soundboard(DependencyContainer container) : base(container)
    {
        Container.Register(this);
        _db = Container.Resolve<DatabaseManager>().Database;
        Migrate();
        SoundCollection = _db.GetCollection<SoundEntity>("sounds");
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override void RegisterControllers(WebServer server)
    {
        server.WithWebApi("/api/soundboard", Web.Serializer,
            m => m.WithController(() => new SoundboardController().WithContainer(Container)));
    }

    public List<SoundEntity> GetUserSounds(UserEntity user)
    {
        return (
            from s in SoundCollection.FindAll()
            where user.Guilds.Contains(s.GuildId)
            select s
        ).ToList();
    }

    protected override string GetName()
    {
        return "soundboard";
    }

    protected override int GetSchemaVersion()
    {
        return 0;
    }

    protected override void ApplyMigration(int toVersion)
    {
        Console.WriteLine("Apply migration to soundboard" + toVersion);
    }
}