using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zendesk.JsonSearch.Models;

namespace Zendesk.JsonSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initialising, please wait...");
            Logic.Framework.Instance.Initialise();
            Start();
        }

        private static void Start()
        {
            var entityMetadatas = Logic.Framework.Instance.GetEntityMetadatas();
            EntryTxt();
            var input = GetInput();
            switch (input)
            {
                case "1":
                    var entityInput = RetrieveEntityToSearch(entityMetadatas);
                    var entityToSearch = entityMetadatas.FirstOrDefault(e => e.EntityCode.ToString().Equals(entityInput));
                    var propertyInput = RetrievePropertyToSearch(entityToSearch);
                    var propertyToSearch = propertyInput.Trim();
                    var valueToSearch = RetrieveValueToSearch();
                    SearchResult(entityToSearch,propertyToSearch,valueToSearch);
                    Restart();
                    break;
                case "2":
                    var searchableFieldsTxt = BuildSearchableFieldsTxt(entityMetadatas);
                    Console.WriteLine(searchableFieldsTxt);
                    Restart();
                    break;             
                default:
                    Console.WriteLine("Invalid option. Please try again");
                    Restart();
                    break;
            }
        }


        private static void EntryTxt()
        {
            Console.WriteLine("Welcome to Zendesk Search");
            Console.WriteLine("Type 'quit' to exit at any time, Press 'Enter' to continue");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select search options");
            Console.WriteLine("*Presss 1 to search Zendesk");
            Console.WriteLine("*Presss 2 to view a list of searchable fields");
            Console.WriteLine("*Type 'quit' to exit");
            Console.WriteLine(Environment.NewLine);
        }

        private static string BuildEntityTxt(List<EntityMetadata> entities)
        {
            var entityTxtBuilder = new StringBuilder("Select ");
            foreach (var entity in entities)
            {
                entityTxtBuilder.Append($"{entity.EntityCode}) {entity.DisplayName} ");
            }
            return entityTxtBuilder.ToString();
        }

        private static string BuildSearchableFieldsTxt(List<EntityMetadata> entities)
        {
            var searchableFieldsTxtTxtBuilder = new StringBuilder();
            foreach (var entity in entities)
            {
                searchableFieldsTxtTxtBuilder.Append("---------------------------------");
                searchableFieldsTxtTxtBuilder.Append(Environment.NewLine);
                searchableFieldsTxtTxtBuilder.Append($"Search {entity.DisplayName} with");
                searchableFieldsTxtTxtBuilder.Append(Environment.NewLine);
                foreach (var property in entity.Properties)
                {
                    searchableFieldsTxtTxtBuilder.Append($"{property}");
                    searchableFieldsTxtTxtBuilder.Append(Environment.NewLine);
                }
            }
            return searchableFieldsTxtTxtBuilder.ToString();
        }

        private static string GetInput()
        {
            var input = Console.ReadLine();
            if(input == "quit") Environment.Exit(0);
            return input;
        }

        private static string RetrieveEntityToSearch(List<EntityMetadata> entityMetadatas)
        {
            var entityTxt = BuildEntityTxt(entityMetadatas);
            Console.WriteLine(entityTxt);
            var input = GetInput();
            while (!entityMetadatas.Any(e => e.EntityCode.ToString().Equals(input.Trim())))
            {
                Console.WriteLine("Invalid option. Please try again");
                input = GetInput();
            }
            return input;
        }

        private static string RetrievePropertyToSearch(EntityMetadata entityMetadata)
        {
            Console.WriteLine("Enter search terms");
            var input = GetInput();
            while (!entityMetadata.Properties.Any(p => p.Equals(input.Trim())))
            {
                Console.WriteLine("Invalid option. Please try again");
                input = GetInput();
            }
            return input;
        }

        private static string RetrieveValueToSearch()
        {
            Console.WriteLine("Enter search value");
            return GetInput();
        }

        private static void SearchResult(EntityMetadata entityToSearch,string propertyToSearch,string valueToSearch)
        {
            Console.WriteLine($"Searching {entityToSearch.DisplayName} for {propertyToSearch} with a value of {valueToSearch}");
            var result = Logic.Framework.Instance.GetSearchResult(entityToSearch.FileName, propertyToSearch, valueToSearch);
            Console.WriteLine(result);
        }

        private static void Restart()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Click enter to restart");
            GetInput();
            Start();
        }
    }
}
