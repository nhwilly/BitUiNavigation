using System.Web;

namespace BitUiNavigation.Client.Pages.Modals;

public static class QueryStringHelper
{
    public static string AddQueryParameters(string uri, Dictionary<string, object?> parameters)
    {
        var baseUri = new Uri(uri);
        var query = HttpUtility.ParseQueryString(baseUri.Query);

        foreach (var (key, value) in parameters)
        {
            query[key] = value?.ToString();
        }

        var builder = new UriBuilder(baseUri)
        {
            Query = query.ToString()!
        };

        return builder.Uri.ToString();
    }
}

public static class UrlExtensions
{
    public static string Normalize(string? value, string defaultPanel)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultPanel;
        var v = value.Trim();
        return v.StartsWith('/') ? v[1..] : v;
    }
}