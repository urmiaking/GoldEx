using System.Text.RegularExpressions;

namespace GoldEx.Server.Application.Utilities;

public static class SmsTemplateParser
{
    public static List<string> ExtractParameters(string template)
    {
        if (template == null) 
            throw new ArgumentNullException(nameof(template));

        const string pattern = @"$([^)]*?)$";
        var regex = new Regex(pattern, RegexOptions.Compiled);

        var matches = regex.Matches(template);
        var result = new List<string>(matches.Count);

        foreach (Match m in matches)
        {
            var param = m.Groups[1].Value.Trim();

            if (!string.IsNullOrEmpty(param))
                result.Add(param);
        }

        return result;
    }
}