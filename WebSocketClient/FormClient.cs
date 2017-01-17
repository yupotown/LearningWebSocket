using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketClient
{
    public partial class FormClient : Form
    {
        public FormClient()
        {
            InitializeComponent();
        }

        private ClientWebSocket ws = new ClientWebSocket();

        private void print(string text)
        {
            textBoxOut.Text = text;
        }

        private delegate void printDelegate(string text);

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            if (ws.State != WebSocketState.Open)
            {
                await ws.ConnectAsync(new Uri(textBoxUri.Text), CancellationToken.None);
                while (ws.State == WebSocketState.Open)
                {
                    var buff = new ArraySegment<byte>(new byte[10]);
                    var ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                    var text = Encoding.UTF8.GetString(buff.Take(ret.Count).ToArray());
                    Invoke(new printDelegate(print), text);
                }
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            if (ws.State != WebSocketState.Open)
            {
                return;
            }
            var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(textBoxMessage.Text));
            await ws.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
