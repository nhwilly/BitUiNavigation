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