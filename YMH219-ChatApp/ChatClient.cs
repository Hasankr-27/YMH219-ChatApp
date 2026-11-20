using System.Net.Sockets;
using System.Text;

namespace YMH219_ChatApp
{
    // Sunucuya bağlanan, mesaj gönderen ve gelen mesajları dinleyen istemci
    public class ChatClient
    {
        private TcpClient? tcp;
        private StreamReader? reader;
        private StreamWriter? writer;

        public string UserName { get; private set; } = "";
        public bool IsConnected { get; private set; }

        // Form bu olaylara abone olup ekranı günceller
        public event Action<string, string, DateTime>? MessageReceived;   // kimden, mesaj, saat
        public event Action<string>? SystemMessage;
        public event Action? Disconnected;

        public async Task ConnectAsync(string host, int port, string userName)
        {
            tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(host, port);   // bağlanamazsa SocketException
            }
            catch
            {
                tcp.Dispose();
                tcp = null;
                throw;   // hatayı Form'a ilet, orada kullanıcıya gösterilecek
            }

            var stream = tcp.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            UserName = userName;
            IsConnected = true;
            writer.WriteLine("JOIN|" + userName);

            _ = ListenAsync();   // gelen mesajları arka planda dinlemeye başla
        }

        public void Send(string text)
        {
            if (!IsConnected || writer == null)
                throw new InvalidOperationException("Sunucuya bağlı değilsiniz.");

            writer.WriteLine("MSG|" + text);
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            tcp?.Close();
            Disconnected?.Invoke();
        }

        private async Task ListenAsync()
        {
            try
            {
                string? line;
                while ((line = await reader!.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("MSG|"))
                    {
                        // MSG|kullanıcı|mesaj
                        var parts = line.Split('|', 3);
                        if (parts.Length == 3)
                            MessageReceived?.Invoke(parts[1], parts[2], DateTime.Now);
                    }
                    else if (line.StartsWith("SYS|"))
                    {
                        SystemMessage?.Invoke(line.Substring(4));
                    }
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }

            // Buraya geldiysek bağlantı bitti. Biz kesmediysek sunucu tarafı koptu.
            if (IsConnected)
            {
                IsConnected = false;
                tcp?.Close();
                SystemMessage?.Invoke("Sunucu bağlantısı koptu.");
                Disconnected?.Invoke();
            }
        }
    }
}