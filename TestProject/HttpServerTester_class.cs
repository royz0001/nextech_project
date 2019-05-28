using Microsoft.VisualStudio.TestTools.UnitTesting;

using hacker_news_server.code;
using hacker_news_server;

namespace TestProject
{
    [TestClass]
    public class HttpServerTester_class
    {
        [TestMethod]
        public void test_we_can_spinup_server()
        {
            HN_aggregator_class agg = new HN_aggregator_class();

            hacker_news_server.code.HttpServer_class server = new hacker_news_server.code.HttpServer_class(agg);

            Assert.IsTrue(server.run("http://localhost:4400/"));

            server.shutdown();
        }


    }
}
