﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EUCalc
{
    static class Program
    {
        
        static void Main(string[] args)
        {

            List<Country> countries = initCountries(); // Read config file and compile Country List
            displayStartMessage();

            string[] rule_options = new string[] { "qm", "rqm", "sm", "u" };
            Calculations.voting_rule = "qm";
            string input = "";

            while (true) 
            {
                displayCountriesTable(countries); // Display Table
                input = Console.ReadLine();
                string[] user_input = input.Split(' ');
                int number_option = 0;
                if ((user_input.Length == 2) && (int.TryParse(user_input[1], out number_option) == true))
                {
                    if ((Country.all_country_codes.Find(x => x.Equals(user_input[0])) == user_input[0]) && (Country.vote_dict.ContainsKey(number_option))) 
                    {
                        Country.setVote(countries, user_input[0], number_option);
                    }
                }
                else if ((user_input.Length == 2) && (Country.vote_dict.ContainsValue(user_input[1])))
                {
                    if (Country.all_country_codes.Find(x => x.Equals(user_input[0])) == user_input[0]) 
                    {
                        foreach (KeyValuePair<int, string> vote_opt in Country.vote_dict)
                        {
                            if (vote_opt.Value == user_input[1])
                            {
                                Country.setVote(countries, user_input[0], vote_opt.Key);
                            }
                        }
                    }
                }
                else if (user_input[0] == "exit") {
                    break;
                }
                else if (user_input[0] == "h")
                {
                    displayHelp();
                    string escape = Console.ReadLine();
                    while (true)
                    {
                        if (escape.Equals("e") == true)
                        {
                            break;
                        }
                    }
                }
                else if (user_input[0] == "results")
                {
                    displayResults(countries);
                }
                else if ((user_input[0] == "rule") && (rule_options.Contains(user_input[1])))
                {
                    Calculations.voting_rule = user_input[1];
                }
                else if (user_input[0] == "reset")
                {
                    Country.setVote(countries, "all");  // polymorphism: setvote() without argument to reset vote
                }
                else if ((user_input[0] == "participation") && ( (user_input[1] == "eurozone") || (user_input[1] == "all") || (user_input[1] == "none") ))
                {
                    Country.setVote(countries, user_input[1]); // polymorphism: pass with List<Country> and flag to change multiple participation values
                }
                
            }
        }
        
        private static List<Country> initCountries()  // Create an iterable contries List
        {

            // open init file
            string config_file = "country_config.csv";
            StreamReader file = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\../../\" + config_file);

            List<Country> countries = new List<Country>();
            List<string> all_country_codes = new List<string>();
            string line = "";
            bool eurozone = false;
            while ((line = file.ReadLine()) != null)  // For every line in the config file
            {
                string[] details = line.Split(',');
                Country.all_country_codes.Add(details[0]);
                eurozone = int.Parse(details[3]) != 0;  // If eurozone isn't 0, set to true
                countries.Add(new Country(details[0], details[1], int.Parse(details[2]), eurozone));  // Add country to List
            }

            return countries;
        }

        private static void displayStartMessage()
        {
            Console.WriteLine("Type 'h' for help and 'exit' to exit the application.");
        }

        private static void displayHelp ()
        {
            string help = @"
To change a country's vote, type the country code followed by the number or name of the new vote.
For example:
    DE 2
    EE 0
    CZ 3

The available votes are as follows:
    0 = non-partcipating
    1 = yes
    2 = no
    3 = abstain

To reset all votes to 'yes', enter the following command:
    reset

To switch participation between all countries and Eurozone countries only, enter either:
    participation eurozone
    participation all

To change the voting rule, type rule followed by the code of the new voting rule.
For example:
    rule qm
    rule u

The available voting rules are as follows:
    qm Qualified Majority
    rqm Reinforced Qualified Majority
    sm Simple Majority
    u Unanimity

To view if the vote will pass or not, enter the following command:
    results

To return press 'e'
            ";
            Console.WriteLine(help);
        }

        // Creates table of countries and displays it to the console
        private static void displayCountriesTable (List<Country> countries)
        {
            Calculations.calcPop(countries); // Update populations for display

            Console.WriteLine(" ____________________________________________________________");
            Console.WriteLine("| Code | Country         | Population | Eurozone |    Vote   |");
            Console.WriteLine("|======|=================|============|==========|===========|");

            // Iterate and display details
            for (int i = 0; i < countries.Count; i++)
            {
                Console.Write("|  ");
                Console.Write(countries[i].ToString(16, 7, 8)); // Polymorphed ToString() method
                Console.Write(" |\n");
            }

            Console.WriteLine("|______|_________________|____________|__________|___________|");
        }

        private static void displayResults(List<Country> countries)
        {
            if (Calculations.getResult(countries))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Vote passed");
                Console.ResetColor();
                
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Vote Failed");
                Console.ResetColor();
            }

            Console.WriteLine($"Nations Percentage: {Calculations.national_result}% ({Calculations.rule_dict[Calculations.voting_rule][0]}% Needed)");
            if (Calculations.rule_dict[Calculations.voting_rule].Length == 2)
            {
                Console.WriteLine($"Population Percentage: {Calculations.population_result}% ({Calculations.rule_dict[Calculations.voting_rule][1]}% Needed)");
            }
            
            Console.WriteLine("Press any key to continue");
            Console.WriteLine("");
            Console.ReadKey();
        }
    }
}
