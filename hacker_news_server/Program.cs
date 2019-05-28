using System;

using hacker_news_server.code;

namespace hacker_news_server
{
    class Program
    {
        static void Main(string[] args)
        {
            string prefix = "http://*:80/";

            string version = "1.001";

            HN_aggregator_class fff = new HN_aggregator_class();

            fff.intialize_aggregation();

            Console.WriteLine("Server ready. Listening on: " + prefix + " version: " + version);

            HttpServer_class server = new HttpServer_class(fff);

            server.run(prefix);


            Console.ReadLine();

        }

        private static void Fff_new_articles_are_ready()
        {
            Console.WriteLine("ready");
        }
    }
}
