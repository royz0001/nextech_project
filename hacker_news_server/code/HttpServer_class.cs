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

        HttpListener listener = null;

        #endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public HttpServer_class()
        {
           
        }

        public bool run(string prefix)
        {
            bool result = false;

            try
            {

                listener = new HttpListener();

                listener.Prefixes.Add(prefix);

                listener.Start();

                Task.Run(async delegate {

                    while (listener.IsListening)
                    {
                        await listener.GetContextAsync().ContinueWith((obj) => {

                                process_request(obj);
        

                        });
                    }

                });

                result = true;
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.Message + ee.StackTrace);
            }

            return result;
        }

        void process_request(Task<HttpListenerContext> context)
        {
            HttpListenerContext http_context = context.Result;

            var request = http_context.Request;

            var response = http_context.Response;

            try
            {
               

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
