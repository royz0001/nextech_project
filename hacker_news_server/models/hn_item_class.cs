using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace hacker_news_server.models
{
    public class hn_item_class
    {
        /// <summary>
        /// ID of the item on hacker news
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// URL of the article
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string URL { get; set; }

        /// <summary>
        /// The Author of the article
        /// </summary>
        [JsonProperty(PropertyName = "by")]
        public string Author { get; set; }

        /// <summary>
        /// Title of the article
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        
    }
}
