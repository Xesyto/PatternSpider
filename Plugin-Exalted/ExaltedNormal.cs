﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using PatternSpider.Irc;
using PatternSpider.Plugins;
using PatternSpider.Utility;

namespace Plugin_Exalted
{
    [Export(typeof(IPlugin))]    
    class ExaltedNormal: IPlugin
    {
        public string Name { get { return "ExaltedDefault"; } }
        public string Description { get { return "Throws Exalted (2e) dicepools and counts successes."; } }

        public List<string> Commands { get { return new List<string>{"e"}; } }

        private DiceRoller _fate = new DiceRoller();

        public List<string> IrcCommand(IrcBot ircBot, string server, IrcMessage m)
        {
            var response = new List<string>();
            var mesasge = m.Text;
            var messageParts = mesasge.Split(' ');

            if (messageParts.Length < 2)
            {
                return new List<string> { "Usage: e <poolsize> [poolsize]..." };
            }

            foreach (var messagePart in messageParts)
            {
                int poolSize;
                if (int.TryParse(messagePart,out poolSize))
                {
                    if (poolSize > 2000)
                    {
                        response.Add("No Pools over 2000.");
                    }
                    else
                    {
                        response.Add(string.Format("{0} -- {1}", m.Sender, RollPool(poolSize)));
                    }
                }
            }

            return response;
        }

        private string RollPool(int poolSize)
        {
            int[] rolls = new int[poolSize];
            var successes = 0;
            var response = "";

            for (var i = 0; i < poolSize; i++)
            {
                rolls[i] = _fate.RollDice(10);
                if (rolls[i] >= 7)
                {
                    successes++;
                }

                if (rolls[i] == 10)
                {
                    successes++;
                }
            }

            if (rolls.Length <= 50)
            {
                response += String.Format("[{0}]", string.Join(", ", rolls.ToList().OrderBy(r => r)));
            }
            else
            {
                response += "Over 50 rolls, truncated to reduce spam";
            }
            
            if (successes == 0 && rolls.Contains(1))
            {
                response += " -- BOTCH";
            }
            else
            {
                response += string.Format(" -- {0} success(es).",successes);
            }

            return response;
        }

        public List<string> OnChannelMessage(IrcBot ircBot, string server, string channel, IrcMessage m)
        {
            return null;
        }

        public List<string> OnUserMessage(IrcBot ircBot, string server, IrcMessage m)
        {
            return null;
        }
    }
}
