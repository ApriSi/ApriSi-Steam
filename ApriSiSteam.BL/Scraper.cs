using System;
using System.Diagnostics;
using System.Net;
using System.Xml;
using HtmlAgilityPack;

namespace ApriSiSteam.BL;

public static class Scraper
{
    public static HtmlNode Scrape(string url, bool steam = false, bool loadHtmlString = false)
    {
        var web = new HtmlWeb();
        var doc = web.Load(new Uri(url));

        if(loadHtmlString)
            doc.LoadHtml(url);

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

        return doc.DocumentNode;
    }
}