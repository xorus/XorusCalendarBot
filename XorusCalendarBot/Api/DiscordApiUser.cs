using System.Globalization;
using Discord;
using Newtonsoft.Json;

namespace XorusCalendarBot.Api;

public class DiscordApiUser
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Id { get; internal set; } = null!;

    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Username { get; internal set; } = null!;

    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Discriminator { get; internal set; } = null!;

    [JsonIgnore]
    private int DiscriminatorInt
        => int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string AvatarHash { get; internal set; } = null!;

    [JsonIgnore]
    public string AvatarUrl => !string.IsNullOrWhiteSpace(this.AvatarHash)
        ? (this.AvatarHash.StartsWith("a_")
            ? $"https://cdn.discordapp.com/avatars/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.gif?size=1024"
            : $"https://cdn.discordapp.com/avatars/{this.Id}/{this.AvatarHash}.png?size=1024")
        : this.DefaultAvatarUrl;

    [JsonIgnore]
    private string DefaultAvatarUrl =>
        $"https://cdn.discordapp.com/embed/avatars/{(this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=1024";

    [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool IsBot { get; internal set; }

    [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? MfaEnabled { get; internal set; }

    [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsSystem { get; internal set; }

    [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? Verified { get; internal set; }

    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Email { get; internal set; } = null!;

    [JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
    public virtual PremiumType? PremiumType { get; internal set; }

    [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Locale { get; internal set; } = null!;

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public virtual int OAuthFlags { get; internal set; }

    [JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
    public virtual int Flags { get; internal set; }

    // /// <summary>
    // /// Gets the user's avatar URL, in requested format and size.
    // /// </summary>
    // /// <param name="imageFormat">The image format of the avatar to get.</param>
    // /// <param name="imageSize">The maximum size of the avatar. Must be a power of two, minimum 16, maximum 4096.</param>
    // /// <returns>The URL of the user's avatar.</returns>
    // public string GetAvatarUrl(ImageFormat imageFormat, ushort imageSize = 1024)
    // {
    //     if (imageFormat == ImageFormat.Unknown)
    //         throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
    //
    //     // Makes sure the image size is in between Discord's allowed range.
    //     if (imageSize < 16 || imageSize > 4096)
    //         throw new ArgumentOutOfRangeException("Image Size is not in between 16 and 4096: " + nameof(imageSize));
    //
    //     // Checks to see if the image size is not a power of two.
    //     if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
    //         throw new ArgumentOutOfRangeException("Image size is not a power of two: " + nameof(imageSize));
    //
    //     // Get the string varients of the method parameters to use in the urls.
    //     var stringImageFormat = imageFormat switch
    //     {
    //         ImageFormat.Gif => "gif",
    //         ImageFormat.Jpeg => "jpg",
    //         ImageFormat.Png => "png",
    //         ImageFormat.WebP => "webp",
    //         ImageFormat.Auto => !string.IsNullOrWhiteSpace(this.AvatarHash)
    //             ? (this.AvatarHash.StartsWith("a_") ? "gif" : "png")
    //             : "png",
    //         _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
    //     };
    //     var stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);
    //
    //     // If the avatar hash is set, get their avatar. If it isn't set, grab the default avatar calculated from their discriminator.
    //     if (!string.IsNullOrWhiteSpace(this.AvatarHash))
    //     {
    //         var userId = this.Id.ToString(CultureInfo.InvariantCulture);
    //         return
    //             $"https://cdn.discordapp.com{Endpoints.AVATARS}/{userId}/{this.AvatarHash}.{stringImageFormat}?size={stringImageSize}";
    //     }
    //     else
    //     {
    //         // https://discord.com/developers/docs/reference#image-formatting-cdn-endpoints: In the case of the Default User Avatar endpoint, the value for `user_discriminator` in the path should be the user's discriminator `modulo 5—Test#1337` would be `1337 % 5`, which evaluates to 2.
    //         var defaultAvatarType = (this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture);
    //         return
    //             $"https://cdn.discordapp.com/embed{Endpoints.AVATARS}/{defaultAvatarType}.{stringImageFormat}?size={stringImageSize}";
    //     }
    // }

    public override string ToString() => $"User {this.Id}; {this.Username}#{this.Discriminator}";
}