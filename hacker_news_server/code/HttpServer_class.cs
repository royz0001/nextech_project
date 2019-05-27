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

        HttpListener listener = null;

        /// <summary>
        /// The cancellation object that is used to cancel any Task based operations
        /// </summary>
        CancellationTokenSource cts = null;

        #endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public HttpServer_class()
        {
           
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
                http_context?.Response.Close();
            }

        }

        void stream_homepage(Stream stream)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                TextWriter tw = sw;

                tw.WriteLine("<!doctype html>");
                tw.WriteLine("<html>");
                    tw.WriteLine("<body>");
                        tw.WriteLine("<H1>It worked</H1>");
                    tw.WriteLine("</body>");
                tw.WriteLine("</html>");

                tw.Flush();
            }

        }

    }
}
