using System.Collections.Specialized;
using System.Web;

namespace BitUiNavigation.Client.Pages.ModalHost.Helpers;

public static class UrlExtensions
{
    public const string modalKey = "modal";
    public static string Normalize(string? value, string defaultPanel)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultPanel;
        var v = value.Trim();
        return v.StartsWith('/') ? v[1..] : v;
    }
    public static string AddQueryParameters(string uri, Dictionary<string, object?> parameters)
    {
        var baseUri = new Uri(uri);
        var query = HttpUtility.ParseQueryString(baseUri.Query);

        foreach (var (key, value) in parameters)
        {
            if (value is null)
            {
                query.Remove(key); // Optional: Remove key entirely if null
            }
            else
            {
                query[key] = value.ToString();
            }
        }

        return new UriBuilder(baseUri)
        {
            Query = query.ToString()!
        }.Uri.ToString();
    }

    public static string RemoveModalQueryParameters(string fullUri)
    {
        return RemoveTrailingQueryParameters(fullUri, modalKey);
    }
    public static string RemoveTrailingQueryParameters(string fullUri, string modalKey)
    {
        var uri = new Uri(fullUri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        // Take all keys before reaching modalKey
        var trimmed = query
            .AllKeys!
            .TakeWhile(key => !string.Equals(key, modalKey, StringComparison.OrdinalIgnoreCase))
            .Where(key => key is not null)
            .ToDictionary(key => key!, key => (object?)query[key]);

        return AddQueryParameters(uri.GetLeftPart(UriPartial.Path), trimmed);
    }
}
