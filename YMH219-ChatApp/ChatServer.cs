using System.Net;
using System.Net.Sockets;

namespace YMH219_ChatApp
{
    // Bağlantıları kabul eden ve mesajları herkese dağıtan sunucu
    public class ChatServer
    {
        private TcpListener? listener;
        private readonly List<ConnectedClient> clients = new List<ConnectedClient>();

        public bool IsRunning { get; private set; }
        public IReadOnlyList<ConnectedClient> Clients => clients;

        // Form bu olaylara abone olup ekranı günceller
        public event Action<string>? LogMessage;
        public event Action? ClientsChanged;

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();   // port doluysa SocketException fırlatır, Form yakalar
            IsRunning = true;
            Log($"Sunucu başlatıldı (port {port})");

            _ = AcceptLoopAsync();   // bağlantı beklemeyi arka planda başlat
        }

        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            listener?.Stop();

            foreach (var c in clients)
                c.Close();
            clients.Clear();

            ClientsChanged?.Invoke();
            Log("Sunucu durduruldu");
        }

        // Sürekli yeni bağlantı bekler
        private async Task AcceptLoopAsync()
        {
            while (IsRunning)
            {
                try
                {
                    TcpClient tcp = await listener!.AcceptTcpClientAsync();
                    _ = HandleClientAsync(tcp);   // her istemciyi ayrı ayrı dinle
                }
                catch (ObjectDisposedException) { break; }   // Stop çağrıldı
                catch (SocketException) { break; }
            }
        }

        // Tek bir istemciden gelen satırları okur
        private async Task HandleClientAsync(TcpClient tcp)
        {
            var client = new ConnectedClient(tcp);

            try
            {
                // İlk satır kullanıcı adı olmalı: JOIN|hasan
                string? first = await client.Reader.ReadLineAsync();
                if (first == null || !first.StartsWith("JOIN|"))
                {
                    client.Close();
                    return;
                }

                client.UserName = first.Substring(5);
                clients.Add(client);
                ClientsChanged?.Invoke();
                Log($"{client.UserName} bağlandı ({client.Address})");
                Broadcast($"SYS|{client.UserName} sohbete katıldı");

                // Sonraki satırlar mesaj: MSG|merhaba
                string? line;
                while ((line = await client.Reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("MSG|"))
                    {
                        string text = line.Substring(4);
                        Log($"{client.UserName}: {text}");
                        Broadcast($"MSG|{client.UserName}|{text}");
                    }
                }
            }
            catch (IOException) { }               // istemci aniden koptu
            catch (ObjectDisposedException) { }   // sunucu durduruldu
            finally
            {
                client.Close();
                if (clients.Remove(client))
                {
                    ClientsChanged?.Invoke();
                    Log($"{client.UserName} ayrıldı");
                    Broadcast($"SYS|{client.UserName} sohbetten ayrıldı");
                }
            }
        }

        // Bir satırı bağlı herkese gönderir
        private void Broadcast(string line)
        {
            foreach (var c in clients.ToList())
            {
                try
                {
                    c.Send(line);
                }
                catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
                {
                    // gönderilemeyen istemci kendi döngüsünde zaten listeden çıkacak
                }
            }
        }

        private void Log(string text)
        {
            LogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {text}");
        }
    }
}