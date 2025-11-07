using System.Linq;             // LINQ kullanýmý için (OrderBy gibi sýralama iþlemleri)
using UnityEngine;
using TMPro;                   // TextMeshPro UI bileþenleri için
using UnityEngine.UI;         // Unity UI (Button gibi) bileþenleri için

// Bu sýnýf, veritabanýndaki konuþmalarý UI'da (ScrollView) listelemek ve temizlemekle ilgilenir
public class ConversationListController : MonoBehaviour
{
    public TMP_Text allConversationsText; //  içinde kullanýcýya gösterilecek tüm konuþmalarý yazacaðýmýz TMP_Text alaný

    private DatabaseService db;           // Veritabaný iþlemlerini yönetecek sýnýf
    public Button clearButton;            // UI'daki "Tümünü Sil" butonu

    void Start()
    {
        // Uygulama baþladýðýnda veritabaný baðlantýsýný kurar
        db = new DatabaseService("gpt_records.db");

        // Var olan konuþmalarý ekrana yükler
        LoadConversations();
    }

    // Bu fonksiyon, kullanýcý "Tümünü Sil" butonuna bastýðýnda çalýþýr
    public void ClearConversations()
    {
        db.DeleteAllConversations(); // Veritabanýndaki tüm konuþmalarý siler
        allConversationsText.text = "Tüm konuþmalar silindi."; // Kullanýcýya bilgi mesajý gösterilir
    }

    // Bu fonksiyon, veritabanýndaki tüm konuþmalarý UI üzerine yazdýrýr
    public void LoadConversations()
    {
        allConversationsText.text = ""; // Önce eski metni temizler

        // Tüm konuþmalarý zamanlarýna göre sondan baþa sýralar
        var conversations = db.GetAllConversations()
                              .OrderByDescending(c => c.TimeStamp); // En yeni konuþmalar en üstte

        // Her konuþmayý ekrana yazdýr
        foreach (var convo in conversations)
        {
            allConversationsText.text +=
                $"<i>{convo.TimeStamp:yyyy-MM-dd HH:mm}</i>\n\n" +          // Tarih italik yazýlýr (örnek: 2025-06-18 01:30)
                $"<b>Soru:</b> {convo.Question}\n" +                   // Soru kalýn yazýlýr
                $"<b>Cevap:</b> {convo.Answer}\n";           // Cevap kalýn yazýlýr
     
        }
    }
}
