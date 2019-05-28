using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;


using hacker_news_server;
using hacker_news_server.code;
using hacker_news_server.models;

namespace TestProject
{
    [TestClass]
    public class HN_aggregatorTester_class
    {
        [TestMethod]
        public void test_we_can_grab_list_of_hacker_news_ids()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            List<string> list_of_ids = new List<string>();

            list_of_ids.AddRange(new string [] { "20026186",
                                                 "20026177",
                                                 "20026139",
                                                 "20026137",
                                                 "20026136",
                                                 "20026133",
                                                 "20026124",
                                                 "20026109",
                                                 "20026102",
                                                 "20026101",
                                                 "20026097",
                                                 "20026094",
                                                 "20026063" });

            List<hn_item_class>  result_hn_items_list = vvvv.grab_hacker_news_items(list_of_ids, 4);

            /// Check that the number of items that were returned are matching number
            /// of items searched.
            Assert.IsTrue(result_hn_items_list.Count == list_of_ids.Count);

        }

        [TestMethod]
        public void test_individual_hacker_news_item_returns_data()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            List<string> list_of_ids = new List<string>();

            list_of_ids.AddRange(new string[] { "20026186"});

            List<hn_item_class> result_hn_items_list = vvvv.grab_hacker_news_items(list_of_ids,4);

            int value = 0;

            /// Check that the number of items that were returned are matching number
            /// of items searched.
            Assert.IsTrue(result_hn_items_list[0].Author == "evo_9");
            Assert.IsTrue(result_hn_items_list[0].URL == "https://www.sciencedaily.com/releases/2008/12/081209221709.htm");
            Assert.IsTrue(result_hn_items_list[0].Title == "Why Climbers Die on Mount Everest");

            /// test ID is an actual integer by parsing the string.
            Assert.IsTrue(int.TryParse(result_hn_items_list[0].ID, out value));

        }

        [TestMethod]
        public void test_bad_hacker_news_id_return_nulls()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            List<string> list_of_ids = new List<string>();

            list_of_ids.AddRange(new string[] { "9999999999999999" , "9999999999999998", "9999999999999997" });

            List<hn_item_class> result_hn_items_list = vvvv.grab_hacker_news_items(list_of_ids,4);

            /// Make sure we get a list of results 
            Assert.IsNotNull(result_hn_items_list);

            /// but that all of the results are nulls
            for (int i = 0; i < result_hn_items_list.Count; i++)
            {
                Assert.IsNull(result_hn_items_list[i]);
            }
        }

        [TestMethod]
        public void test_loading_all_latest_ids_on_startup()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            var list = vvvv.grab_missing_news_ids();

            /// Test that nothing timed out.
            Assert.IsNotNull(list);

            /// Hacker news returns 500 items when requesting new stories. 
            /// we are testing 500 always returns true.
            Assert.IsTrue(list.Count == 500);
        
        }

        [TestMethod]
        public void test_grab_json_returns_a_string()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            var str = vvvv.grab_json(new System.Uri( "https://hacker-news.firebaseio.com/v0/newstories.json?print=pretty")).Result;

            Assert.IsNotNull(str);

            Assert.IsTrue(str.Length > 0);

            //Check if first character is a braket, we are expecting a JSON response.
            Assert.IsTrue(str[0] == '[');
        }

        [TestMethod]
        public void test_searching_for_specific_word_in_title()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            List<string> list_of_ids = new List<string>();

            list_of_ids.AddRange(new string[] { "20026186",
                                                 "20026177",
                                                 "20026139",
                                                 "20026137",
                                                 "20026136",
                                                 "20026133",
                                                 "20026124",
                                                 "20026109",
                                                 "20026102",
                                                 "20026101",
                                                 "20026097",
                                                 "20026094",
                                                 "20026063" });

            List<hn_item_class> result_hn_items_list = vvvv.grab_hacker_news_items(list_of_ids,4);

            vvvv.add_new_hacker_news_items(result_hn_items_list);

            var search_result = vvvv.search_title("Climbers");

            /// We expect at minimum a no result list.
            Assert.IsTrue(search_result != null);

            /// We expect at least one result 
            Assert.IsTrue(search_result.Count > 0);

        }

        [TestMethod]
        public void test_searching_for_empty_input_so_all_results_should_comeback()
        {
            HN_aggregator_class vvvv = new HN_aggregator_class();

            List<string> list_of_ids = new List<string>();

            list_of_ids.AddRange(new string[] { "20026186",
                                                 "20026177",
                                                 "20026139",
                                                 "20026137",
                                                 "20026136",
                                                 "20026133",
                                                 "20026124",
                                                 "20026109",
                                                 "20026102",
                                                 "20026101",
                                                 "20026097",
                                                 "20026094",
                                                 "20026063" });

            List<hn_item_class> result_hn_items_list = vvvv.grab_hacker_news_items(list_of_ids, 4);

            vvvv.add_new_hacker_news_items(result_hn_items_list);

            var search_result = vvvv.search_title("");

            /// We expect at minimum a no result list.
            Assert.IsTrue(search_result != null);

            /// We expect at least one result 
            Assert.IsTrue(search_result.Count > 0);

        }

    }
}
