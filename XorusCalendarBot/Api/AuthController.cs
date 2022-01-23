using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using LiteDB;
using Microsoft.IdentityModel.Tokens;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public class AuthController : AbstractController
{
    private Env _env;

    public AuthController(Env env, DatabaseManager database) : base(database)
    {
        _env = env;
    }

    [Route(HttpVerbs.Get, "/token/{userId}")]
    public void GetToken(string userId, [QueryField] string key)
    {
        var user = Database.Users.FindById(Guid.Parse(userId));
        if (user == null) throw new HttpException(401);
        if (user.Key != key) throw new HttpException(401);

        var th = new JwtSecurityTokenHandler();
        var td = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userId)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            // Issuer = myIssuer,
            // Audience = myAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_env.Secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = th.CreateToken(td);

        HttpContext.SendStringAsync(th.WriteToken(token), "text/plain", Encoding.Default);
    }
}