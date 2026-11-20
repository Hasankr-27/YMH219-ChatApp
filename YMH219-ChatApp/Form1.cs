namespace YMH219_ChatApp
{
    // Açılış menüsü: sunucu veya istemci penceresi açar
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Text = "YMH219 Chat";
            ClientSize = new Size(320, 180);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10);

            var lblTitle = new Label
            {
                Text = "Chat Uygulaması",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(280, 40),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnServer = new Button
            {
                Text = "Sunucu Penceresi Aç",
                Location = new Point(40, 70),
                Size = new Size(240, 40)
            };
            btnServer.Click += (s, e) => new ServerForm().Show();

            var btnClient = new Button
            {
                Text = "İstemci Penceresi Aç",
                Location = new Point(40, 120),
                Size = new Size(240, 40)
            };
            btnClient.Click += (s, e) => new ClientForm().Show();

            Controls.AddRange(new Control[] { lblTitle, btnServer, btnClient });
        }
    }
}