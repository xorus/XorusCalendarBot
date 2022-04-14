using EmbedIO;
using EmbedIO.Routing;
using XorusCalendarBot.Api;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class SoundboardController : BaseController
{
    private Soundboard Soundboard => Container.Resolve<Soundboard>();

    [Route(HttpVerbs.Get, "/")]
    public List<SoundEntity> Index()
    {
        return Soundboard.GetUserSounds(GetUserFromHttpContext());
    }

    [Route(HttpVerbs.Post, "/")]
    public SoundEntity Create([JsonData] SoundEntity soundEntity)
    {
        soundEntity.Id = Guid.NewGuid();

        return soundEntity;
    }
}