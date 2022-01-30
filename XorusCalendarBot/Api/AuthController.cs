using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using DSharpPlus;
using DSharpPlus.Entities;
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
        var link = dm.DiscordClient.CurrentApplication.GenerateBotOAuth(
            Permissions.ManageChannels
            | Permissions.AccessChannels
            | Permissions.ManageChannels
            | Permissions.EmbedLinks
            | Permissions.SendMessages
            | Permissions.MentionEveryone
            | Permissions.AddReactions
            | Permissions.UseApplicationCommands
        ).Replace("=bot", "=bot%20applications.commands");
        HttpContext.Redirect(link);
    }

    [Route(HttpVerbs.Get, "/discord/login")]
    public void DiscordLogin()
    {
        var env = Container.Resolve<Env>();
        HttpContext.Redirect("https://discord.com/api/oauth2/authorize?client_id=" + env.DiscordClientId +
                             "&redirect_uri=" + HttpUtility.UrlEncode(env.DiscordRedirectUri) +
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
            (message + " token=" + tokenResponse + " request=" +
             await tokenResponse.RequestMessage.Content.ReadAsStringAsync()).Error();
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
        var discordUser = JsonConvert.DeserializeObject<DiscordUser>(str1);

        var col = Container.Resolve<DatabaseManager>().Users;
        var user = col.FindOne(x => x.DiscordId.Equals(discordUser.Id.ToString()));
        user.DiscordName = discordUser.Username;
        user.DiscordAvatar = discordUser.AvatarUrl;

        var guildsResponse = await authClient.GetAsync("https://discord.com/api/users/@me/guilds");
        var guildsStr = await guildsResponse.Content.ReadAsStringAsync();
        // Logger.Info("/api/users/@me/guilds " + guildsStr);
        if (!response.IsSuccessStatusCode)
            throw new HttpException(401,
                "/users/@me/guilds: could not list guilds. " +
                await response.Content.ReadAsStringAsync());

        var allGuilds = JsonConvert.DeserializeObject<List<IJustNeedTheId>>(guildsStr);

        user.Guilds = (from iJustNeedTheId in allGuilds
            where Container.Resolve<DiscordManager>().GetGuilds().ContainsKey(Convert.ToUInt64(iJustNeedTheId.id))
            select iJustNeedTheId.id).ToArray();
        col.Update(user);

        HttpContext.Redirect(Env.ClientAppHost + "/calendars.html#token=" +
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
    }

    public struct IJustNeedTheId
    {
        public string id = "";
    }
}