using System;

using hacker_news_server.code;

namespace hacker_news_server
{
    class Program
    {
        static void Main(string[] args)
        {

            HN_aggregator_class fff = new HN_aggregator_class();

            fff.intialize_aggregation();

            Console.WriteLine("Hello World!");

            HttpServer_class server = new HttpServer_class(fff);

            server.run("http://localhost:4400/");


            Console.ReadLine();

        }

        private static void Fff_new_articles_are_ready()
        {
            Console.WriteLine("ready");
        }
    }
}
