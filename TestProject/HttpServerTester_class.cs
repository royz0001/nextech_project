using Microsoft.VisualStudio.TestTools.UnitTesting;

using hacker_news_server;

namespace TestProject
{
    [TestClass]
    public class HttpServerTester_class
    {
        [TestMethod]
        public void test_we_can_spinup_server()
        {
            hacker_news_server.code.HttpServer_class server = new hacker_news_server.code.HttpServer_class();

            Assert.IsTrue(server.run("http://localhost:4400/"));

            server.shutdown();
        }


    }
}
