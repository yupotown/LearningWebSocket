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

        private ClientWebSocket ws;

        private void print(string text)
        {
            textBoxOut.Text = text;
        }

        private delegate void printDelegate(string text);

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (ws == null)
                {
                    ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(textBoxUri.Text), CancellationToken.None);
                    while (ws != null && ws.State == WebSocketState.Open)
                    {
                        var buff = new ArraySegment<byte>(new byte[1024 * 16]);
                        var ret = await ws.ReceiveAsync(buff, CancellationToken.None);
                        var text = Encoding.UTF8.GetString(buff.Take(ret.Count).ToArray());
                        Invoke(new printDelegate(print), text);
                    }
                }
            }
            finally
            {
                ws = null;
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            if (ws == null || ws.State != WebSocketState.Open)
            {
                return;
            }
            var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(textBoxMessage.Text));
            await ws.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (ws == null)
            {
                return;
            }
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye.", CancellationToken.None);
            ws = null;
        }
    }
}
