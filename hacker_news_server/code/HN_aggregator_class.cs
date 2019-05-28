using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using System.Linq;


using Newtonsoft;

using hacker_news_server.models;

namespace hacker_news_server.code
{
    /// <summary>
    /// This class caches hacker news news articles and notifies subscribers that new articles are available
    /// </summary>
    public class HN_aggregator_class
    {
        #region Variables

        /// <summary>
        /// Total number of records to cache
        /// </summary>
        readonly static int MAX_RECORD_COUNT = 500;

        /// 
        /// Event to signal all subscribers that new articles are ready.
        /// 
        public event EventHandler NewArticlesAreAvailable_event;

        /// <summary>
        /// Cancelation object to stop processing.
        /// </summary>
        CancellationTokenSource cts = null;

        /// <summary>
        /// URI to fetch max item from Hacker News
        /// </summary>
        readonly Uri max_item_uri = new Uri("https://hacker-news.firebaseio.com/v0/maxitem.json?print=pretty");

        /// <summary>
        /// URI to fetch all the new stories.
        /// </summary>
        readonly Uri new_stories_uri = new Uri("https://hacker-news.firebaseio.com/v0/newstories.json?print=pretty");

        /// <summary>
        /// The 
        /// </summary>
        string last_max_id = string.Empty;

        /// <summary>
        /// List of hacker news articles that can be sorted by item iD
        /// </summary>
        Dictionary<string, hn_item_class> dic = new Dictionary<string, hn_item_class>(MAX_RECORD_COUNT);

        /// <summary>
        /// List of items stacked first in first out.
        /// </summary>
        Queue<hn_item_class> item_queue = new Queue<hn_item_class>();

        /// <summary>
        /// Object to synchronize access to dic and item_stack resources
        /// </summary>
        object __lock__ = new object();

        #endregion

        public void intialize_aggregation()
        {
            cts = new CancellationTokenSource();

            trigger_logic();
        }

        async void trigger_logic()
        {
            if(cts.IsCancellationRequested)
            {
                return;
            }

            Task t =
            Task.Run(() =>
            {
                logic();
                
         
            }, cts.Token);

            await t.ContinueWith(delegate { trigger_logic(); }, cts.Token);
         
        }

        /// <summary>
        /// The actual looping logic to download news
        /// </summary>
        void logic()
        {
            try
            {
                string latest_max_id = grab_json(max_item_uri).Result;

                if(string.IsNullOrEmpty(latest_max_id))
                {
                    Task.Delay(TimeSpan.FromSeconds(5), cts.Token).Wait();

                    return;
                }

                latest_max_id = latest_max_id.Trim();

                if(latest_max_id == last_max_id)
                {
                    Task.Delay(TimeSpan.FromSeconds(5), cts.Token).Wait();

                    return;
                }

                last_max_id = latest_max_id;

                var missing_ids_list = grab_missing_news_ids();

                if(missing_ids_list == null)
                {
                    Task.Delay(TimeSpan.FromSeconds(1), cts.Token).Wait();
                    /// If we were not able to grab latest news then we need to retry 
                    return;
                }

                var new_hn_items_list = grab_hacker_news_items(missing_ids_list, 4);

                if(new_hn_items_list == null)
                {
                    Task.Delay(TimeSpan.FromSeconds(1), cts.Token).Wait();
                    /// If we were not able to grab latest news then we need to retry 
                    return;
                }

                if(add_new_hacker_news_items(new_hn_items_list) == true)
                {
                    NewArticlesAreAvailable_event?.Invoke(this, null);

                    trim_records(MAX_RECORD_COUNT);
                }

                /// Pause
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
            catch (OperationCanceledException oce)
            {

            }
            catch(ThreadAbortException tae)
            {

            }
            catch (Exception ee)
            {

            }

        }

        /// <summary>
        /// This method adds new hacker news items to the list of items
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool add_new_hacker_news_items(List<hn_item_class> list)
        {
            bool result = false;

            try
            {
                lock (__lock__)
                {
                    foreach (var item in list)
                    {
                        if (dic.ContainsKey(item.ID) == false)
                        {
                            dic.Add(item.ID, item);

                            item_queue.Enqueue(item);

                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return result;
        }

        /// <summary>
        /// This method returns a list of missing IDs identified by internal dic[tionary] of item ids
        /// </summary>
        /// <returns></returns>
        public List<string> grab_missing_news_ids()
        {
            List<string> result = null;

            try
            {
                string newstories_json = grab_json(new_stories_uri).Result;

                var list = JsonConvert.DeserializeObject<List<string>>(newstories_json);

                if(list == null)
                {
                    return result;
                }

                result = new List<string>();

                lock (__lock__)
                {
                    foreach (var item_id in list)
                    {
                        if (dic.ContainsKey(item_id) == false)
                        {
                            result.Add(item_id);
                        }
                    }
                }
            }
            catch (Exception ee)
            {

            }

            return result;
        }


        /// <summary>
        /// Download actual hackernews items.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<hn_item_class> grab_hacker_news_items(List<string> list, int total_tasks_to_spin_up)
        {
            List<hn_item_class> result = new List<hn_item_class>();

            List<Task<hn_item_class>> list_of_tasks = new List<Task<hn_item_class>>();

            int completed_id = 0;

            foreach (var item in list)
            {
                string temp = item;

                Task<hn_item_class> t = Task<hn_item_class>.Run(delegate
                {
                    return logic(temp);
                });

                list_of_tasks.Add(t);

                if(list_of_tasks.Count < total_tasks_to_spin_up)
                {
                    continue;
                }

                completed_id = Task.WaitAny(list_of_tasks.ToArray());

                if (list_of_tasks[completed_id].IsCompleted)
                {
                    result.Add( list_of_tasks[completed_id].Result);
                }

                list_of_tasks.RemoveAt(completed_id);
            }

            /// We are getting towards the end of the tasks so 
            Task.WaitAll(list_of_tasks.ToArray());

            for (int i = 0; i < list_of_tasks.Count; i++)
            {
                result.Add(list_of_tasks[i].Result);
            }

            hn_item_class logic(string id)
            {
                hn_item_class _result = null;

                try
                {
                    string item_str = grab_json(new Uri($"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty")).Result;

                    if (string.IsNullOrEmpty(item_str))
                    {
                        return null;
                    }

                    _result = JsonConvert.DeserializeObject<hn_item_class>(item_str);
                }
                catch (Exception ee)
                {

                }

                return _result;
            }

            return result;
        }
        /// <summary>
        /// This method prunes old records by removing records past max_count
        /// </summary>
        /// <param name="max_count"></param>
        /// <returns></returns>
        public bool trim_records(int max_count)
        {
            bool result = false;

            if(max_count < 0)
            {
                return result;
            }

            try
            {
                lock (__lock__)
                {
                    while (item_queue.Count > max_count)
                    {
                        var item = item_queue.Dequeue();

                        dic.Remove(item.ID);
                    }
                }

                result = true;
            }
            catch (Exception ee)
            {

            }

            return result;

        }

        /// <summary>
        /// Grab JSON from passed in URI.
        /// </summary>
        /// <returns></returns>
        public async Task<string> grab_json(Uri uri)
        {
            CancellationTokenSource sub_cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Task<string> t = Task<string>.Run(async delegate { return await logic(uri); }, sub_cts.Token);

            await t;

            if (sub_cts.IsCancellationRequested)
            {
                return null;
            }

            return t.Result;

            async Task<string> logic(Uri _uri)
            {
                string json = null;

                using (var client = new WebClient())
                {
                    json = await client.DownloadStringTaskAsync(uri);
                }

                return json;
            }

        }

        public List<hacker_news_server.models.hn_item_class> search_title(string term)
        {
            List<hacker_news_server.models.hn_item_class> result = new List<hn_item_class>();

            try
            {
                if (string.IsNullOrEmpty(term))
                {
                    lock (__lock__)
                    {
                        result = this.item_queue.ToList< hn_item_class>();
                    }
                }
                else
                {

                    hn_item_class[] array = null;

                    lock (__lock__)
                    {
                        array = this.item_queue.ToArray();
                    }

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].Title.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            result.Add(array[i]);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return result;
        }
    }
}
