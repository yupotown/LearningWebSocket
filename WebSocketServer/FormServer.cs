using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketServerTest
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        private void update()
        {
            textBoxOut.Text = "";
            var i = 1;
            foreach (var client in clients)
            {
                textBoxOut.Text += string.Format("{0:D2} : {1}" + Environment.NewLine,
                    i++,
                    client.Value == null ? "(not received)" : "\"" + client.Value + "\"");
            }
        }

        private delegate void updateDelegate();

        private WebSocketServer server;
        private bool opened = false;

        private void buttonWait_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                opened = true;
                return;
            }

            server = new WebSocketServer();
            var rootConfig = new RootConfig();
            var serverConfig = new ServerConfig()
            {
                Ip = "Any",
                Port = 30304,
                MaxConnectionNumber = 5,
                Mode = SuperSocket.SocketBase.SocketMode.Tcp,
                Name = "WebSocket Test Server"
            };

            server.NewSessionConnected += s =>
            {
                if (opened)
                {
                    clients.Add(s, null);
                    Invoke(new updateDelegate(update));
                }
                else
                {
                    s.Close(SuperSocket.SocketBase.CloseReason.ServerClosing);
                }
            };
            server.SessionClosed += (s, reason) =>
            {
                clients.Remove(s);
                Invoke(new updateDelegate(update));
            };
            server.NewMessageReceived += (s, message) =>
            {
                clients[s] = message;
                Invoke(new updateDelegate(update));
                Parallel.ForEach(clients, c =>
                {
                    c.Key.Send(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)));
                });
            };

            server.Setup(rootConfig, serverConfig);

            opened = true;
            server.Start();
        }

        private Dictionary<WebSocketSession, string> clients = new Dictionary<WebSocketSession, string>();

        private void buttonClose_Click(object sender, EventArgs e)
        {
            opened = false;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Parallel.ForEach(clients, c =>
            {
                c.Key.CloseWithHandshake("Goodbye.");
            });

            server.Stop();
            update();
        }
    }
}
