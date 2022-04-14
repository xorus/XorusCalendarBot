using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Discord;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Swan.Logging;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Api;

public class AuthController : BaseController
{
    private Env Env => Container.Resolve<Env>();

    [Route(HttpVerbs.Get, "/invite")]
    public void Invite()
    {
        var dm = Container.Resolve<DiscordManager>();

        const GuildPermission permission = GuildPermission.ViewChannel & GuildPermission.SendMessages &
                                           GuildPermission.UseApplicationCommands & GuildPermission.MentionEveryone;

        var link = "https://discord.com/api/oauth2/authorize?client_id=" + Env.DiscordClientId + "&permissions=" +
                   permission + "&scope=bot%20applications.commands";
        // dm.DiscordClient.CurrentApplication.GenerateBotOAuth(
        //     /* Permissions.ManageChannels
        //      | Permissions.EmbedLinks
        //      | Permissions.AddReactions
        //     |*/ Permissions.AccessChannels
        //         | Permissions.SendMessages
        //         | Permissions.UseApplicationCommands
        //         | Permissions.MentionEveryone
        // ).Replace("=bot", "=bot%20applications.commands");
        HttpContext.Redirect(link);
    }

    [Route(HttpVerbs.Get, "/discord/login")]
    public void DiscordLogin()
    {
        HttpContext.Redirect("https://discord.com/api/oauth2/authorize?client_id=" + Env.DiscordClientId +
                             "&redirect_uri=" + HttpUtility.UrlEncode(Env.DiscordRedirectUri) +
                             "&response_type=code&scope=identify%20guilds");
    }

    [Route(HttpVerbs.Get, "/discord")]
    public async Task DiscordLogin([QueryField] string code)
    {
        using var client = new HttpClient();
        var tokenResponse = await client.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("client_id", Env.DiscordClientId),
                new KeyValuePair<string, string>("client_secret", Env.DiscordClientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", Env.DiscordRedirectUri)
            }
        ));
        var tokenString = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var message = "/oauth2/token cannot validate token " + tokenString;
            (message + " token=" + tokenResponse + " request=" + tokenString).Error();
            throw new HttpException(401, message);
        }

        var token = JsonConvert.DeserializeObject<DiscordOAuthAccessToken>(tokenString);

        using var authClient = new HttpClient();
        authClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token.access_token);
        var response = await authClient.GetAsync("https://discord.com/api/users/@me");
        if (!response.IsSuccessStatusCode)
        {
            var message = "/users/@me: could not identify user ; token is probably expired. " +
                          await response.Content.ReadAsStringAsync();
            (message + " token=" + tokenResponse).Error();
            throw new HttpException(401, message);
        }

        var str1 = await response.Content.ReadAsStringAsync();
        // Logger.Info("/api/users/@me " + str1);
        var discordUser = JsonConvert.DeserializeObject<DiscordApiUser>(str1);
        if (discordUser == null)
        {
            ("/users/@me: cannot deserialize user from " + str1).Error();
            throw new HttpException(500, "/users/@me: cannot deserialize user. ");
        }

        var col = Container.Resolve<DatabaseManager>().Users;
        var user = col.FindOne(x => x.DiscordId.Equals(discordUser.Id));

        if (user == null)
        {
            user = new UserEntity
            {
                DiscordId = discordUser.Id
            };
            col.Insert(user);
        }

        user.DiscordName = discordUser.Username;
        user.DiscordAvatar = discordUser.AvatarUrl;

        var guildsResponse = await authClient.GetAsync("https://discord.com/api/users/@me/guilds");
        var guildsStr = await guildsResponse.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpException(401,
                "/users/@me/guilds: could not list guilds. " +
                await response.Content.ReadAsStringAsync());

        var allGuilds = JsonConvert.DeserializeObject<List<IJustNeedTheId>>(guildsStr);

        user.Guilds = (from iJustNeedTheId in allGuilds
            where Container.Resolve<DiscordManager>().GetGuilds().ContainsKey(Convert.ToUInt64(iJustNeedTheId.id))
            select iJustNeedTheId.id).ToArray();
        col.Update(user);

        HttpContext.Redirect(Env.ClientAppHost + "/calendars/#token=" +
                             HttpUtility.UrlEncode(CreateJwt(user, token.expires_in)));
    }

    private string CreateJwt(UserEntity user, int duration)
    {
        var th = new JwtSecurityTokenHandler();
        var td = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddSeconds(duration),
            // Issuer = myIssuer,
            // Audience = myAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Env.Secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = th.CreateToken(td);
        return th.WriteToken(token);
    }

    public struct DiscordOAuthAccessToken
    {
        public string access_token = "";
        public int expires_in = 0;
        public string refresh_token = "";
        public string scope = "";
        public string token_type = "";

        public DiscordOAuthAccessToken()
        {
        }
    }

    public struct IJustNeedTheId
    {
        public string id = "";

        public IJustNeedTheId()
        {
        }
    }
}