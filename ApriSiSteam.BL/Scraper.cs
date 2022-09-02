using System;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace ApriSiSteam.BL;

public static class Scraper
{
    private const string UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

    public static List<string> ScrapeMuliple(string url, RequestItem requestItems, List<Cookie>? cookies)
    {
        var htmlNodes = ScrapeHtmlNodes(url, new List<RequestItem>() { requestItems }, cookies);

        return htmlNodes[0].Select(scrape => scrape.InnerText).ToList();
    }

    public static List<List<HtmlNode>> ScrapeHtmlNodes(string url, List<RequestItem> requestItems, List<Cookie>? cookies)
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
        var htmlNodeListContainer = requestItems.Select(requestItem => doc.DocumentNode.SelectNodes(requestItem.Request).ToList()).ToList();

        return htmlNodeListContainer;
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

public class RequestItem
{
    public string? Request { get; set; }

    public string? Attribute { get; set; }
}