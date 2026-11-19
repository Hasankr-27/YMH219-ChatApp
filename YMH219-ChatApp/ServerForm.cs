using System.Net.Sockets;

namespace YMH219_ChatApp
{
    // Sunucu penceresi: başlat/durdur, bağlı istemciler ve kayıtlar
    public class ServerForm : Form
    {
        private readonly ChatServer server = new ChatServer();

        private readonly TextBox txtPort;
        private readonly Button btnStart;
        private readonly Button btnStop;
        private readonly Label lblStatus;
        private readonly ListBox lstClients;
        private readonly ListBox lstLog;

        public ServerForm()
        {
            Text = "Chat Sunucusu";
            ClientSize = new Size(600, 460);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10);

            var lblPort = new Label { Text = "Port:", AutoSize = true, Location = new Point(20, 22) };
            txtPort = new TextBox { Text = "5000", Location = new Point(65, 19), Width = 80 };
            btnStart = new Button { Text = "Sunucuyu Başlat", Location = new Point(160, 15), Size = new Size(140, 32) };
            btnStop = new Button { Text = "Durdur", Location = new Point(310, 15), Size = new Size(90, 32) };
            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(415, 15),
                Size = new Size(165, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblClients = new Label { Text = "Bağlı istemciler:", AutoSize = true, Location = new Point(20, 62) };
            lstClients = new ListBox { Location = new Point(20, 85), Size = new Size(560, 110) };

            var lblLog = new Label { Text = "Sunucu kayıtları:", AutoSize = true, Location = new Point(20, 205) };
            lstLog = new ListBox { Location = new Point(20, 228), Size = new Size(560, 214) };

            Controls.AddRange(new Control[] { lblPort, txtPort, btnStart, btnStop, lblStatus, lblClients, lstClients, lblLog, lstLog });

            btnStart.Click += (s, e) => StartServer();
            btnStop.Click += (s, e) => { server.Stop(); UpdateUi(); };

            server.LogMessage += text => RunOnUi(() =>
            {
                lstLog.Items.Add(text);
                lstLog.TopIndex = lstLog.Items.Count - 1;
            });
            server.ClientsChanged += () => RunOnUi(RefreshClients);

            FormClosing += (s, e) => server.Stop();   // pencere kapanınca sunucu da dursun

            UpdateUi();
        }

        private void StartServer()
        {
            if (!int.TryParse(txtPort.Text, out int port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Geçerli bir port girin (1024 - 65535).", "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                server.Start(port);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Sunucu başlatılamadı. Port başka bir program tarafından kullanılıyor olabilir.\n\n" + ex.Message,
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateUi();
        }

        private void RefreshClients()
        {
            lstClients.Items.Clear();
            foreach (var c in server.Clients)
                lstClients.Items.Add(c);
        }

        private void UpdateUi()
        {
            bool running = server.IsRunning;
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            txtPort.Enabled = !running;
            lblStatus.Text = running ? "● ÇALIŞIYOR" : "■ KAPALI";
            lblStatus.BackColor = running ? Color.SeaGreen : Color.Firebrick;
        }

        // Ekranı her zaman arayüz iş parçacığından güncelle
        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
    }
}