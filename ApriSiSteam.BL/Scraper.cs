using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace ApriSiSteam.BL;

public static class Scraper
{
    private const string UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";
    public static List<HtmlNode> GetSingleNodes(string url, List<string> requestItems, List<Cookie>? cookies)
    {
        try
        {
            var uri = new Uri(url);
            var web = new HtmlWeb();
            using var client = new HttpClient();

            web.UserAgent = UserAgent;
            if (cookies != null)
            {
                web.UseCookies = true;
                web.PreRequest += request =>
                {
                    var cookieContainer = new CookieContainer();
                    foreach (var cookie in cookies)
                    {
                        cookieContainer.Add(GenerateCookie(cookie, uri));
                    }
                    request.CookieContainer = cookieContainer;
                    return true;
                };
            }

            var doc = web.Load(uri);

            return requestItems.Select(request => doc.DocumentNode.SelectSingleNode(request)).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null!;
        }
    }
    public static IEnumerable<HtmlNodeCollection> GetMultipleNodes(string url, List<string> requestItems, List<Cookie>? cookies)
    {
        try
        {
            var uri = new Uri(url);
            var web = new HtmlWeb();
            using var client = new HttpClient();

            web.UserAgent = UserAgent;

            if (cookies != null)
            {
                web.UseCookies = true;
                web.PreRequest += request =>
                {
                    var cookieContainer = new CookieContainer();
                    foreach (var cookie in cookies)
                    {
                        cookieContainer.Add(GenerateCookie(cookie, uri));
                    }
                    request.CookieContainer = cookieContainer;
                    return true;
                };
            }

            var doc = web.Load(uri);

            return requestItems.Select(request => doc.DocumentNode.SelectNodes(request)).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null!;
        }
    }
    private static Cookie GenerateCookie(Cookie cookieData, Uri uri)
    {
        var cookie = new Cookie
        {
            Domain = uri.Host,
            Name = cookieData.Name,
            Value = cookieData.Value
        };
        return cookie;
    }
}