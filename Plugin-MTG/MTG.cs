﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PatternSpider.Irc;
using PatternSpider.Plugins;

namespace Plugin_MTG
{
    [Export(typeof(IPlugin))]
    public class MTG : IPlugin
    {
        public string Name { get { return "MTG"; } }
        public string Description { get { return "Gives a link to a MTG card when invoked."; } }

        public List<string> Commands { get { return new List<string> { "mtg" }; } }

        public List<string> IrcCommand(IrcBot ircBot, string server, IrcMessage m)
        {
            var text = m.Text.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            var searchResult = SearchMagicCards(searchString);

            return new List<string> { searchResult };            
        }

        public List<string> OnChannelMessage(IrcBot ircBot, string server, string channel, IrcMessage m)
        {
            return null;
        }

        public List<string> OnUserMessage(IrcBot ircBot, string server, IrcMessage m)
        {
            return null;        
        }

        private string SearchMagicCards(string searchString)
        {
            HtmlDocument document;

            try
            {
                document = UrlRequest(string.Format("http://magiccards.info/query?q={0}&v=card&s=cname", searchString));
            }
            catch
            {
                return "Error Occured trying to search for card.";
            }

            string resultCount;

            try
            {
                resultCount = document.DocumentNode.SelectSingleNode("//body/table[3]/tr/td[3]").InnerHtml.Trim();

            }
            catch
            {
                return "No Results found for: " + searchString;
            }

            var matchString = @"([\d]+).cards";
            var regex = new Regex(matchString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            if (regex.IsMatch(resultCount))
            {
                var count = regex.Match(resultCount).Groups[1];
                return String.Format("[http://magiccards.info/query?q={1}&v=card&s=cname] Found {0} cards.", count, searchString);
            }
            else
            {
                var cardLink = document.DocumentNode.SelectSingleNode("//body/table[4]/tr/td[2]/span/a").Attributes["href"].Value;
                return String.Format("[http://magiccards.info{0}]", cardLink);
            }
            
        }

        private HtmlDocument UrlRequest(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Accept-Language", "en;q=0.8");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";

            var responseStream = req.GetResponse().GetResponseStream();
            var document = new HtmlDocument();

            if (responseStream == null)
            {
                throw new NoNullAllowedException();
            }

            using (var reader = new StreamReader(responseStream))
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream))
                    {
                        writer.Write(reader.ReadToEnd());
                        memoryStream.Position = 0;
                        document.Load(memoryStream, new UTF8Encoding());
                    }
                }
            }

            return document;
        }
    }
}
