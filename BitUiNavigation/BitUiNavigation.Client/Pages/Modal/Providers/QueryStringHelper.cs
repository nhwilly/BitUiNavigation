using System.Collections.Specialized;
using System.Web;

namespace BitUiNavigation.Client.Pages.Modal.Providers;

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

    public static string RemoveQueryParam(string fullUri, string key)
    {
        var pKey = "provider";
        var uri = new Uri(fullUri);
        var qs = HttpUtility.ParseQueryString(uri.Query);

        var providerIndex =
            qs.AllKeys.ToList().FindIndex(k => string.Equals(k, pKey, StringComparison.OrdinalIgnoreCase));

        var trimmed = new NameValueCollection();
        
        for (int i = 0; i <= providerIndex; i++)
        {
            trimmed.Add(qs.GetKey(i), qs.Get(i));
        }
        return QueryStringHelper.AddQueryParameters()
        var ub = new UriBuilder(uri) {  Query = trimmed.ToString() ?? string.Empty };
        return ub.Uri.ToString();
    }
}