using System.Globalization;
using System.Text;

namespace XorusCalendarBot.Discord;

public static class DiscordUtils
{
    // private static Map<ulong, DiscordGuild> GuildCache = new ();

    public static string RemoveAccents(this string text)
    {
        var sbReturn = new StringBuilder();
        var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
        foreach (var letter in arrayText)
            if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                sbReturn.Append(letter);
        return sbReturn.ToString();
    }
}