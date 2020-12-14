using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace warcraftlogs_parser
{
    public static class InputParsing
    {
        public static string GetCombatIdFromArgs(string[] args)
        {
            if (args.Count() > 1)
                throw new ArgumentException("Expected exactly 1 argument in form of link to the combat encounter or the code itself");

            string firstArg;
            if (args.Count() == 0)
            {
                Console.WriteLine("Enter the combat code either in form of https://classic.warcraftlogs.com/reports/J1p4M8gd3b72RLGC or J1p4M8gd3b72RLGC");
                firstArg = Console.ReadLine();
            }
            else
            {
                firstArg = args[0];
            }

            if (firstArg.Length == 16) // direct combat id code
                return firstArg;

            if(!firstArg.StartsWith("https://"))
                throw new ArgumentException("Argument is not a 16-char combat code, neither a link to the combat encounter, terminating...");


            var rx = new Regex(@"reports\/(.{16})");
            var matches = rx.Matches(firstArg);

            if(matches.Count != 1)
                throw new ArgumentException($"Could not parse value {firstArg} with {matches.Count} matches, exact count of 1 matches expected!");

            return matches[0].Groups[1].Value;

        }
    }
}