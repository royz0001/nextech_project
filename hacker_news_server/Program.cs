using System;

using hacker_news_server.code;

namespace hacker_news_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            HttpServer_class server = new HttpServer_class();

            server.run("http://localhost:4400/");


            Console.ReadLine();

        }
    }
}
