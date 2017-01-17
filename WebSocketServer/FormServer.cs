using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketServer
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        private async void buttonWait_Click(object sender, EventArgs e)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:30304/");
            listener.Start();

            while (true)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    var ws = wsContext.WebSocket;
                    processClient(ws);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private HashSet<WebSocket> clients = new HashSet<WebSocket>();

        private async void processClient(WebSocket ws)
        {
            clients.Add(ws);

            await ws.SendAsync
            (
                new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello!")),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var buff = new ArraySegment<byte>(new byte[1024]);
                    var ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                    if (ret.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    else if (ret.MessageType == WebSocketMessageType.Text)
                    {
                        Parallel.ForEach(clients, async client =>
                        {
                            await client.SendAsync
                            (
                                new ArraySegment<byte>(buff.Take(ret.Count).ToArray()),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                        });
                    }
                    else if (ret.MessageType == WebSocketMessageType.Binary)
                    {
                    }
                }
                catch
                {
                    break;
                }
            }

            clients.Remove(ws);
            ws.Dispose();
        }
    }
}
