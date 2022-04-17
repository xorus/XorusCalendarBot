using EmbedIO;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using LiteDB;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class SoundboardModule : Base.Module
{
    private readonly LiteDatabase _db;
    public readonly ILiteCollection<SoundEntity> SoundCollection;
    private readonly DiscordCommand _command;

    public SoundboardModule(DependencyContainer container) : base(container)
    {
        "Initializing soundboard module".Info(GetName());
        Container.Register(this);
        _db = Container.Resolve<DatabaseManager>().Database;
        Migrate();
        SoundCollection = _db.GetCollection<SoundEntity>("sounds");
        _command = new DiscordCommand(Container);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override void RegisterControllers(WebServer server)
    {
        server
            .WithBearerToken("/api/soundboard", Container.Resolve<Env>().Secret)
            .WithWebApi("/api/soundboard", Web.Serializer,
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

    public sealed override string GetName()
    {
        return "Soundboard";
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