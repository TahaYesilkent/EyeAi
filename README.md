# Unity API Destekli Sesli Asistan Projesi

Bu proje, Unity oyun motoru içinde OpenAI'ın üç temel API'ını (Whisper, GPT-3.5-Turbo, TTS) kullanarak sesli ve metin tabanlı bir yapay zeka asistanı oluşturur. Ayrıca, tüm konuşma geçmişini kalıcı olarak depolamak için yerel bir SQLite veritabanı entegrasyonu içerir.

---

### 1. Proje Özellikleri

| Özellik | Kullanılan Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Sesli Giriş (STT)** | OpenAI Whisper API | Kullanıcının mikrofondan kaydettiği sesi metne dönüştürür. Ses kaydı, **WAV** formatına dönüştürülüp API'a gönderilir. |
| **Yapay Zeka İşleme** | OpenAI GPT-3.5-Turbo | Metne çevrilen kullanıcı sorusuna bağlamsal ve anlamlı bir yanıt üretir. |
| **Sesli Çıkış (TTS)** | OpenAI TTS API | GPT'den gelen metin yanıtını gerçekçi bir sese dönüştürür ve Unity `AudioSource` üzerinden oynatır. |
| **Kalıcı Depolama** | SQLite4Unity3d | Tüm soru-cevap çiftlerini yerel bir veritabanı dosyasına (`gpt_records.db`) kaydeder. |
| **Veritabanı Yönetimi** | SQLite | Kullanıcıya konuşma geçmişini listeleme ve tüm kayıtları temizleme imkanı sunar. |

---

### 2. Kurulum ve Gereksinimler

Projeyi Unity ortamında çalıştırmak için aşağıdaki adımları izleyin.

#### A. Harici Kütüphane (SQLite)

Bu proje veritabanı işlemleri için harici bir kütüphane olan **SQLite4Unity3d**'ye bağımlıdır.

1.  **SQLite4Unity3d Entegrasyonu:** Kütüphanenin en son sürümünü indirin.
2.  Gerekli dosyaları (`SQLite.cs` ve platforma özgü ikili dosyaları/DLL'leri) Unity projenizin `Assets` klasörüne ekleyin.

#### B. API Anahtarı Yapılandırması

Tüm OpenAI API çağrıları için geçerli bir API anahtarı gereklidir.

1.  **Anahtar Edinme:** [OpenAI Platformu](https://platform.openai.com/) üzerinden bir API anahtarı oluşturun.
2.  **Koda Entegrasyon:** `WhisperRequester.cs` dosyasını açın ve `apiKey` değişkenini kendi anahtarınızla güncelleyin:

    ```csharp
    [Header("API Key (Sadece key, Bearer ekleme kodda yapılıyor)")]
    public string apiKey = "BURAYA-SENIN-OPENAI-API-KEYIN-GELECEK"; 
    ```

#### C. Sahne (Scene) Ayarları

`SceneController.cs` sınıfının doğru çalışması için, Unity projesindeki sahne isimlerinin bu kod ile eşleştiğinden emin olun ve sahneleri Unity **Build Settings**'e ekleyin:

* `GoToMainScene()` metodu **"Main"** sahnesini yükler.
* `GoToDatabaseScene()` metodu **"Database"** sahnesini yükler.

---

### 3. Önemli Kod Bileşenleri

Bu tabloda, projenin temel işlevselliğini sağlayan ana C# sınıfları listelenmiştir.

| Dosya Adı | Sınıf Adı | Temel Sorumluluklar |
| :--- | :--- | :--- |
| `WhisperRequester.cs` | `WhisperRequester` | Mikrofon girişini yönetir, API isteklerini (STT, GPT, TTS) başlatır ve yanıtları işler. UI elemanlarının durumlarını kontrol eder. |
| `DatabaseService.cs` | `DatabaseService` | SQLite veritabanı bağlantısını kurar, kayıt ekleme ve silme işlemlerini gerçekleştirir. |
| `Conversation.cs` | `Conversation` | SQLite tablosunun veri modelini (`Id`, `Question`, `Answer`, `TimeStamp`) tanımlar. |
| `ConversationListController.cs` | `ConversationListController` | Veritabanından verileri çeker, sıralar (`OrderByDescending`) ve UI'da görüntüler. |
| `SceneController.cs` | `SceneController` | Uygulama içi sahne navigasyonunu ve uygulamayı kapatma işlevini sağlar. |

---

### 4. Ses Kaydı Yardımcı Fonksiyonu Uyarısı

`WhisperRequester.cs` dosyasındaki ses kayıtlarının API'a gönderilebilmesi için, `SaveWav` metodu içinde çağrılan harici bir yardımcı fonksiyona (`WavUtility.FromAudioClip`) ihtiyaç vardır. Bu, bir `AudioClip` nesnesini ham **WAV** baytlarına dönüştürür.

```csharp
void SaveWav(string path, AudioClip clip)
{
    // Hata önleme: WavUtility sınıfı projeye eklenmelidir.
    byte[] wav = WavUtility.FromAudioClip(clip); 
    File.WriteAllBytes(path, wav);
}
