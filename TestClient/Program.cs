using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var tokenEndpoint = "https://localhost:44375/token";
            var launchEndpoint = "https://localhost:44375/api/launch/1";

            var user = new Dictionary<string, string>
            {
                { "username", "essenbee"},
                { "password", "password"}
            };

            try
            {
                var userJson = JsonConvert.SerializeObject(user);

                var client = new HttpClient();

                var httpResponse = client.PostAsync(tokenEndpoint, 
                    new StringContent(userJson, Encoding.UTF8, "application/json"))
                    .Result;

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var token = httpResponse.Content.ReadAsStringAsync().Result;

                    Console.WriteLine("We have got the bearer token!");

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    Console.WriteLine("Calling API ...");

                    var result = client.GetAsync(launchEndpoint).Result;

                    switch (result.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:

                            var launchDetails = result.Content.ReadAsStringAsync().Result;
                            Console.WriteLine($"Launch details retrieved ... {launchDetails}");
                            break;
                        case System.Net.HttpStatusCode.Unauthorized:
                            Console.WriteLine("Unauthorized!");
                            break;
                        case System.Net.HttpStatusCode.Forbidden:
                            Console.WriteLine("Forbidden!");
                            break;
                    }

                }
                else
                {
                    Console.WriteLine("Bad Request from /token endpoint!");
                }
            }
            catch
            {

            }

            Console.ReadLine();
        }
    }
}
