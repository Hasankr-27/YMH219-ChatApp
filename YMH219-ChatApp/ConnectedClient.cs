using System.Net.Sockets;
using System.Text;

namespace YMH219_ChatApp
{
    // Sunucuya bağlanmış tek bir istemciyi temsil eder
    public class ConnectedClient
    {
        private readonly TcpClient tcp;
        private readonly StreamWriter writer;

        public StreamReader Reader { get; }
        public string UserName { get; set; } = "?";
        public string Address { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public ConnectedClient(TcpClient tcp)
        {
            this.tcp = tcp;
            var stream = tcp.GetStream();
            Reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
            Address = tcp.Client.RemoteEndPoint?.ToString() ?? "bilinmiyor";
        }

        // Bu istemciye bir satır gönderir
        public void Send(string line)
        {
            writer.WriteLine(line);
        }

        public void Close()
        {
            tcp.Close();
        }

        // Sunucudaki listede böyle görünür
        public override string ToString()
        {
            return $"{UserName}   {Address}   (bağlandı: {ConnectedAt:HH:mm:ss})";
        }
    }
}
