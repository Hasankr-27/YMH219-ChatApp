\# Chat Uygulaması – C# Windows Forms (TCP İstemci-Sunucu)



YMH 219 Nesne Tabanlı Programlama dersi ek uygulama ödevi kapsamında geliştirilmiş, TCP/IP üzerinden çalışan istemci-sunucu sohbet uygulamasıdır. Bir sunucuya birden fazla istemci kullanıcı adıyla bağlanabilir; gönderilen her mesaj sunucu tarafından bağlı tüm kullanıcılara iletilir.



\*\*Öğrenci:\*\* Hasan Kara – 250541015



\## Ekran Görüntüleri



\*\*Sunucu ve iki istemci arasında sohbet\*\*



!\[Sohbet]<img width="1643" height="750" alt="sohbet" src="https://github.com/user-attachments/assets/612899d4-b54d-40eb-8d52-aacb8f6235a4" />




\*\*Sunucu kapalıyken bağlanma denemesi (hata yönetimi)\*\*



!\[Bağlantı hatası](screenshots/hata.png)



\## Kullanılan Teknolojiler



\- C#

\- .NET 8

\- Windows Forms

\- `TcpListener` ve `TcpClient` (TCP/IP)

\- `StreamReader` / `StreamWriter` ile `NetworkStream` üzerinden metin iletişimi

\- `async` / `await` ile asenkron bağlantı ve mesaj dinleme

\- Olay (event) tabanlı sınıf–arayüz iletişimi



\## Kurulum ve Çalıştırma



1\. Depoyu klonlayın:git clone https://github.com/Hasankr-27/YMH219-ChatApp.git



2\. `YMH219-ChatApp.sln` dosyasını Visual Studio 2022 veya üzeriyle açın.

3\. `F5` ile çalıştırın.



\## Kullanım



1\. Açılış menüsünden \*\*Sunucu Penceresi Aç\*\* → port (varsayılan `5000`) → \*\*Başlat\*\*.

&#x20;  - Windows Güvenlik Duvarı izin isterse \*\*İzin ver\*\* seçilmelidir.

2\. Açılış menüsünden \*\*İstemci Penceresi Aç\*\* → sunucu adresi (`127.0.0.1`), port ve kullanıcı adı → \*\*Bağlan\*\*.

3\. Birden fazla kullanıcı için \*\*İstemci Penceresi Aç\*\* butonuna tekrar basılabilir.

4\. Mesajı yazıp \*\*Gönder\*\* butonuna veya `Enter` tuşuna basın.

5\. \*\*Bağlantıyı Kes\*\* ile sohbetten ayrılın.



Aynı yerel ağdaki başka bir bilgisayardan bağlanmak için sunucu adresi olarak sunucu bilgisayarın yerel IP adresi (ör. `192.168.1.20`) yazılmalıdır.



\## Özellikler



\- Kullanıcı adıyla bağlanma

\- Gönderilen ve alınan mesajların saat bilgisiyle gösterimi

\- Birden fazla istemcinin aynı sohbet odasına katılması

\- Kullanıcının kendi mesajları mavi, diğer mesajlar siyah, sistem mesajları (katıldı / ayrıldı) gri renkte gösterilir

\- Sunucuda bağlı istemcilerin listesi (kullanıcı adı, IP:port, bağlanma saati)

\- Sunucu kayıtları (başlatma, bağlanma, mesaj, ayrılma)

\- Bağlantı durumu göstergeleri (\*\*ÇALIŞIYOR / KAPALI\*\*, \*\*BAĞLI / BAĞLI DEĞİL\*\*)

\- Hata yönetimi: sunucuya ulaşılamaması, kullanılan port, kopan bağlantı, boş veya geçersiz kullanıcı adı ve port durumlarında uygulama kapanmadan kullanıcı bilgilendirilir



\## Proje Yapısı



| Sınıf | Görevi |

|---|---|

| `ChatServer` | Portu dinler, bağlantıları kabul eder, mesajları tüm istemcilere dağıtır |

| `ConnectedClient` | Sunucu tarafında tek bir bağlı istemcinin bağlantısı ve bilgileri |

| `ChatClient` | Sunucuya bağlanma, mesaj gönderme ve gelen mesajları dinleme |

| `ServerForm` | Sunucu arayüzü: başlat/durdur, bağlı istemciler, kayıtlar |

| `ClientForm` | İstemci arayüzü: bağlan, mesaj gönder, bağlantıyı kes |

| `Form1` | Açılış menüsü (sunucu veya istemci penceresi açar) |



Ağ işlemleri (`ChatServer`, `ChatClient`) arayüzden ayrı sınıflarda tutulmuştur; arayüz bu sınıfların olaylarına (`MessageReceived`, `ClientsChanged` vb.) abone olarak güncellenir.



\## Mesaj Protokolü



İletişim satır tabanlıdır, her mesaj tek satırdır:



| Yön | Biçim | Anlamı |

|---|---|---|

| İstemci → Sunucu | `JOIN\\|kullanıcı` | Bağlanınca kullanıcı adını bildirir |

| İstemci → Sunucu | `MSG\\|metin` | Mesaj gönderir |

| Sunucu → İstemciler | `MSG\\|kullanıcı\\|metin` | Bir kullanıcının mesajını herkese iletir |

| Sunucu → İstemciler | `SYS\\|metin` | Sistem bildirimi (katıldı / ayrıldı) |



\## Bilinen Sınırlamalar



\- Mesajlar şifrelenmeden gönderilir; kullanıcı doğrulaması (şifre) yoktur.

\- Özel mesaj özelliği yoktur, tüm mesajlar herkese gider.

\- Mesaj geçmişi kaydedilmez; pencere kapatılınca mesajlar silinir.

\- Aynı kullanıcı adıyla birden fazla kişi bağlanabilir.

\- Kullanıcı adında `|` karakteri kullanılamaz.

