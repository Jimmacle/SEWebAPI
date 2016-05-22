using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Concurrent;

namespace WebAPIPlugin
{
    public class APIServer
    {
        public List<APIHandler> Handlers { get; } = new List<APIHandler>();
        private HttpListener listener = new HttpListener();
        private ConcurrentQueue<HttpListenerContext> contexts = new ConcurrentQueue<HttpListenerContext>();
        private Task listenerTask;
        private HttpListenerContext context;

        public APIServer(string prefix)
        {
            listener.Prefixes.Add(prefix);

            listenerTask = new Task(() =>
            {
                listener.Start();
                while (listener.IsListening)
                {
                    var context = listener.GetContext();
                    contexts.Enqueue(context);
                }
            });
        }

        public void ProcessQueue(int limit = int.MaxValue)
        {
            for (int i = 0; i < limit && !contexts.IsEmpty; i++)
            {
                if (contexts.TryDequeue(out context))
                {
                    var path = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
                    Console.WriteLine(path);
                    foreach (var handler in Handlers)
                    {
                        if (handler.CanHandle(path))
                        {
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    handler.Get(context);
                                    break;
                                case "PUT":
                                    handler.Put(context);
                                    break;
                            }
                            break;
                        }
                    }
                    context = null;
                }
            }
        }

        public void Start()
        {
            listenerTask.Start();
        }

        public void Stop()
        {
            listener.Stop();
        }
    }
}
