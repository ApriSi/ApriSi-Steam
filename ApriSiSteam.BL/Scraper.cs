using System;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace ApriSiSteam.BL;

public static class Scraper
{
    private const string UserAgent = "Mozilla/5.0 (X11; CrOS x86_64 14816.131.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";

    public static HtmlNode Scrape(string url, bool steam)
    {
        var web = new HtmlWeb
        {
            UserAgent = UserAgent
        };

        if (steam)
        {
            web.UseCookies = true;
            web.PreRequest += request =>
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Cookie("birthtime", "312850801") { Domain = new Uri(url).Host });
                request.CookieContainer = cookieContainer;
                return true;
            };
        }

        var doc = web.Load(new Uri(url));

        return doc.DocumentNode;
    }
}