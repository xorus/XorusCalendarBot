using EmbedIO;
using EmbedIO.Routing;
using XorusCalendarBot.Api;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class SoundboardController : BaseController
{
    private SoundboardModule SoundboardModule => Container.Resolve<SoundboardModule>();

    [Route(HttpVerbs.Get, "/")]
    public List<SoundEntity> Index()
    {
        return SoundboardModule.GetUserSounds(GetUserFromHttpContext());
    }

    [Route(HttpVerbs.Post, "/")]
    public SoundEntity Create([JsonData] SoundEntity soundEntity)
    {
        soundEntity.Id = Guid.NewGuid();
        SoundboardModule.SoundCollection.Insert(soundEntity);
        return soundEntity;
    }

    [Route(HttpVerbs.Put, "/{id}")]
    public SoundEntity Update(string id, [JsonData] SoundEntity sound)
    {
        if (sound == null) throw new HttpException(400);
        if (!sound.Id.Equals(Guid.Parse(id))) throw new HttpException(401);
        if (!GetUserFromHttpContext().Guilds.Contains(sound.GuildId)) throw new HttpException(401);

        SoundboardModule.SoundCollection.Update(sound);
        return sound;
    }

    [Route(HttpVerbs.Delete, "/{id}")]
    public void Delete(string id)
    {
        try
        {
            var sound = SoundboardModule.SoundCollection.FindById(Guid.Parse(id));
            if (!GetUserFromHttpContext().Guilds.Contains(sound.GuildId)) throw new HttpException(401);
            SoundboardModule.SoundCollection.Delete(Guid.Parse(id));
        }
        catch (Exception)
        {
            throw new HttpException(404);
        }
    }
}