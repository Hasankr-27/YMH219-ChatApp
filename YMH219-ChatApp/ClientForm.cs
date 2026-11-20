using System.Net.Sockets;

namespace YMH219_ChatApp
{
    // İstemci penceresi: bağlan, mesaj gönder, bağlantıyı kes
    public class ClientForm : Form
    {
        private readonly ChatClient client = new ChatClient();

        private readonly TextBox txtHost;
        private readonly TextBox txtPort;
        private readonly TextBox txtUser;
        private readonly Button btnConnect;
        private readonly Button btnDisconnect;
        private readonly Label lblStatus;
        private readonly RichTextBox rtbMessages;
        private readonly TextBox txtMessage;
        private readonly Button btnSend;

        public ClientForm()
        {
            Text = "Chat İstemcisi";
            ClientSize = new Size(520, 500);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10);

            var lblHost = new Label { Text = "Sunucu:", AutoSize = true, Location = new Point(20, 18) };
            txtHost = new TextBox { Text = "127.0.0.1", Location = new Point(100, 15), Width = 100 };
            var lblPort = new Label { Text = "Port:", AutoSize = true, Location = new Point(208, 18) };
            txtPort = new TextBox { Text = "5000", Location = new Point(255, 15), Width = 55 };

            var lblUser = new Label { Text = "Kullanıcı adı:", AutoSize = true, Location = new Point(20, 53) };
            txtUser = new TextBox { Location = new Point(135, 50), Width = 175 };

            btnConnect = new Button { Text = "Bağlan", Location = new Point(320, 12), Size = new Size(180, 30) };
            btnDisconnect = new Button { Text = "Bağlantıyı Kes", Location = new Point(320, 47), Size = new Size(180, 30) };

            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(20, 88),
                Size = new Size(480, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            rtbMessages = new RichTextBox
            {
                Location = new Point(20, 125),
                Size = new Size(480, 310),
                ReadOnly = true,
                BackColor = Color.White
            };

            txtMessage = new TextBox { Location = new Point(20, 452), Width = 380 };
            btnSend = new Button { Text = "Gönder", Location = new Point(410, 448), Size = new Size(90, 32) };
            AcceptButton = btnSend;   // Enter'a basınca mesaj gönderilsin

            Controls.AddRange(new Control[]
            {
                lblHost, txtHost, lblPort, txtPort, lblUser, txtUser,
                btnConnect, btnDisconnect, lblStatus, rtbMessages, txtMessage, btnSend
            });

            btnConnect.Click += async (s, e) => await ConnectAsync();
            btnDisconnect.Click += (s, e) =>
            {
                client.Disconnect();
                AppendLine("Bağlantı kesildi.", Color.Gray, FontStyle.Italic);
            };
            btnSend.Click += (s, e) => SendMessage();

            client.MessageReceived += (user, text, time) => RunOnUi(() =>
            {
                // Kendi mesajlarımız mavi, diğerleri siyah
                Color color = user == client.UserName ? Color.RoyalBlue : Color.Black;
                AppendLine($"[{time:HH:mm:ss}] {user}: {text}", color);
            });
            client.SystemMessage += text => RunOnUi(() =>
                AppendLine($"[{DateTime.Now:HH:mm:ss}] {text}", Color.Gray, FontStyle.Italic));
            client.Disconnected += () => RunOnUi(UpdateUi);

            FormClosing += (s, e) => client.Disconnect();

            UpdateUi();
        }

        private async Task ConnectAsync()
        {
            string user = txtUser.Text.Trim();
            string host = txtHost.Text.Trim();

            if (user == "")
            {
                ShowWarning("Lütfen bir kullanıcı adı girin.");
                return;
            }
            if (user.Contains('|'))
            {
                ShowWarning("Kullanıcı adında | karakteri kullanılamaz.");
                return;
            }
            if (!int.TryParse(txtPort.Text, out int port) || port < 1024 || port > 65535)
            {
                ShowWarning("Geçerli bir port girin (1024 - 65535).");
                return;
            }

            btnConnect.Enabled = false;
            lblStatus.Text = "Bağlanıyor...";
            lblStatus.BackColor = Color.DarkOrange;

            try
            {
                await client.ConnectAsync(host, port, user);
                Text = "Chat İstemcisi - " + user;
                AppendLine($"{host}:{port} sunucusuna bağlandınız.", Color.Gray, FontStyle.Italic);
            }
            catch (SocketException)
            {
                MessageBox.Show("Sunucuya bağlanılamadı.\nSunucunun çalıştığından ve adres/portun doğru olduğundan emin olun.",
                                "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateUi();
        }

        private void SendMessage()
        {
            string text = txtMessage.Text.Trim();
            if (text == "" || !client.IsConnected)
                return;

            try
            {
                client.Send(text);
                txtMessage.Clear();
            }
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is InvalidOperationException)
            {
                MessageBox.Show("Mesaj gönderilemedi:\n" + ex.Message, "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                client.Disconnect();
            }

            txtMessage.Focus();
        }

        // Mesaj kutusuna renkli bir satır ekler
        private void AppendLine(string text, Color color, FontStyle style = FontStyle.Regular)
        {
            rtbMessages.SelectionStart = rtbMessages.TextLength;
            rtbMessages.SelectionLength = 0;
            rtbMessages.SelectionColor = color;
            rtbMessages.SelectionFont = new Font(rtbMessages.Font, style);
            rtbMessages.AppendText(text + Environment.NewLine);
            rtbMessages.ScrollToCaret();
        }

        private void UpdateUi()
        {
            bool connected = client.IsConnected;

            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            txtHost.Enabled = !connected;
            txtPort.Enabled = !connected;
            txtUser.Enabled = !connected;
            txtMessage.Enabled = connected;
            btnSend.Enabled = connected;

            lblStatus.Text = connected ? "● BAĞLI: " + client.UserName : "■ BAĞLI DEĞİL";
            lblStatus.BackColor = connected ? Color.SeaGreen : Color.Firebrick;
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
    }
}