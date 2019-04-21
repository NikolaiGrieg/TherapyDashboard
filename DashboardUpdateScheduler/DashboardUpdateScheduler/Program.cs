using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardUpdateScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("http://localhost");
            var request = new RestRequest("home/updateCache", Method.POST);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content.ToString());
            Console.ReadKey();
        }
    }
}
