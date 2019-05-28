using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace hacker_news_server.code
{
    public class HttpServer_class
    {
        #region Variables

        /// <summary>
        /// Internal flag that tracks if this server is already running
        /// </summary>
        bool already_running = false;

        /// <summary>
        /// Listener object that handles incomming requests.
        /// </summary>
        HttpListener listener = null;

        /// <summary>
        /// The cancellation object that is used to cancel any Task based operations
        /// </summary>
        CancellationTokenSource cts = null;

        /// <summary>
        /// data aggregator
        /// </summary>
        HN_aggregator_class aggregator = null;

        #endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public HttpServer_class(HN_aggregator_class _aggregator)
        {
            aggregator = _aggregator;
        }

        public void shutdown()
        {
            try
            {
                cts?.CancelAfter(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {

            }
        }

        public bool run(string prefix)
        {
            bool result = false;

            try
            {
                if(already_running == true)
                {
                    Console.Out.WriteLine("Server is already running.");

                    return result;
                }

                already_running = true;

                cts = new CancellationTokenSource();

                listener = new HttpListener();

                listener.Prefixes.Add(prefix);

                listener.Start();

                Task.Run(async delegate {

                    while (listener.IsListening && !cts.Token.IsCancellationRequested)
                    {
                        await listener.GetContextAsync().ContinueWith((obj) => {

                                process_request(obj, cts.Token);
        
                        });
                    }

                }, cts.Token);

                result = true;
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.Message + ee.StackTrace);
            }

            return result;
        }

        void process_request(Task<HttpListenerContext> context, CancellationToken token)
        {
            HttpListenerContext http_context = context.Result;

            var request = http_context.Request;

            var response = http_context.Response;

            try
            {
                if(token.IsCancellationRequested)
                {
                    return;
                }
               
                switch (request.RawUrl)
                {
                    default:
                    case "/":
                        {
                            response.StatusCode = 200;
                            response.KeepAlive = true;

                            stream_homepage(response.OutputStream);

                            break;
                        }
                    case "/favicon.ico":
                        {
                            response.StatusCode = 404;
                            response.KeepAlive = true;

                            break;
                        }
                    
                }
                
            }
            catch (Exception eee)
            {
                Console.WriteLine(eee.Message + eee.StackTrace);
            }
            finally
            {
                http_context?.Response?.Close();
            }

        }

        void stream_homepage(Stream stream)
        {
            var list = aggregator.search_title(null);

            using (StreamWriter sw = new StreamWriter(stream))
            {
                TextWriter tw = sw;

                tw.WriteLine("<!doctype html>");
                tw.WriteLine("<html>");
                    tw.WriteLine("<body>");

                    if (list.Count == 0)
                    {
                        tw.WriteLine("<H1>No results available</H1>");
                    }
                    else
                    {
                        tw.WriteLine("<UL>");
                        for (int i = 0; i < list.Count; i++)
                        {
                            tw.WriteLine("<li>");
                            tw.WriteLine($"<p>{list[i].Author}</p><a href='{list[i].URL}'>{list[i].Title}</a>");
                            tw.WriteLine("</li>");
                        }
                        tw.WriteLine("</UL>");

                    }

                tw.WriteLine("</body>");
                tw.WriteLine("</html>");

                tw.Flush();
            }

        }

    }
}
