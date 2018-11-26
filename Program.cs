using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cosmosdb
{
    class Program
    {
        static void Main(string[] args)
        {
            // The endpoint to your cosmosdb instance
            var endpointUrl = "https://cosmosdbdaniel2.documents.azure.com:443/";

            // The key to you cosmosdb
            var key = "kehGwKESukWttYh914DePmad7Til5eMJKkVkoIjx9VMR9qwFcWVNKjfmahR6tTmls505i63RXVk7RGgJaLPOCQ==";

            // The name of the database
            var databaseName = "Coins";

            // The name of the collection of json documents
            var databaseCollection = "Items";

            GetData(endpointUrl, key, databaseName, databaseCollection).Wait();
            Console.Write("heelo");
            // Create a cosmosdb client
            
        }

        static async Task GetData(string endpointUrl, string key, string databaseName, string databaseCollection)
        {
            //We will make a GET request to a really cool website...
        
            string baseUrl = "https://api.coinmarketcap.com/v2/ticker/?limit=20&sort=id";
            //The 'using' will help to prevent memory leaks.
            //Create a new instance of HttpClient
            using (HttpClient httpClient = new HttpClient())
            using (HttpResponseMessage res = await httpClient.GetAsync(baseUrl))
            using (HttpContent content = res.Content)
            {
                Console.WriteLine("go in here or not");
                string result = await content.ReadAsStringAsync();
                var root = JObject.Parse(result.ToString());
                int i = 1, count = 0;
                while (count < 20)
                {
                    if (root["data"][$"{i}"] != null)
                    {
                        var coinName = root["data"][$"{i}"]["name"];
                        var coinPrice = root["data"][$"{i}"]["quotes"]["USD"]["price"];
                        var cryptoCoin = new Coin(Guid.NewGuid(), coinName.ToString(), coinPrice.ToString());
                        using (var client = new DocumentClient(new Uri(endpointUrl), key))
                        {
                            // Create the database
                            client.CreateDatabaseIfNotExistsAsync(new Database() { Id = databaseName }).GetAwaiter().GetResult();

                            // Create the collection
                            client.CreateDocumentCollectionIfNotExistsAsync(
                                UriFactory.CreateDatabaseUri(databaseName),
                                new DocumentCollection { Id = databaseCollection }).
                                GetAwaiter()
                                .GetResult();

                            // Sava the document to cosmosdb
                            client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection), cryptoCoin)
                                .GetAwaiter().GetResult();

                            // Query for the student by last name
                            var query = client.CreateDocumentQuery<Coin>(
                                    UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection))
                                    .Where(f => f.Name == coinName.ToString())
                                    .ToList();

                            count++;
                        }
                    } 
                    i++;
                }
                
            }
        }
    }

    /// <summary>
    /// A simple class representing a Student
    /// </summary>
    public class Coin
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Price { get; set; }
        
        public Coin(Guid id, string name, string price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }
}
