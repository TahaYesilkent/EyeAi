using System;
using System.IO;
using UnityEngine;
using SQLite4Unity3d; // Unity için SQLite kütüphanesi

// Bu sýnýf, veritabaný iþlemlerini yönetmek için kullanýlýr
public class DatabaseService
{
    private SQLiteConnection db; // Veritabaný baðlantý nesnesi

    // Yapýcý metot: Veritabanýný baþlatýr ve baðlantýyý kurar
    public DatabaseService(string databaseName)
    {
        // Veritabaný dosyasýnýn tam yolunu oluþturur (cihaza göre deðiþebilir)
        string dbPath = Path.Combine(Application.persistentDataPath, databaseName);

        // Veritabanýný açar veya yoksa oluþturur
        db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        // "Conversation" tablosunu oluþturur (zaten varsa dokunmaz)
        db.CreateTable<Conversation>();
    }

    // Tüm konuþmalarý siler (veritabanýndaki tüm kayýtlarý kaldýrýr)
    public void DeleteAllConversations()
    {
        db.DeleteAll<Conversation>(); // Conversation tablosunu tamamen temizler
        Debug.Log("Tüm konuþmalar silindi."); // Konsola bilgi verir
    }

    // Yeni bir konuþmayý veritabanýna ekler
    public void InsertConversation(string question, string answer)
    {
        // Yeni bir Conversation nesnesi oluþturur
        var convo = new Conversation
        {
            Question = question,           // Soru metni
            Answer = answer,               // Cevap metni
            TimeStamp = DateTime.Now       // Þu anki tarih ve saat
        };

        db.Insert(convo); // Veritabanýna ekler
        Debug.Log("Kayýt eklendi: " + question); // Konsola log yazar
    }

    // Tüm konuþmalarý getirir (sorgu nesnesi döner)
    public TableQuery<Conversation> GetAllConversations()
    {
        return db.Table<Conversation>(); // Conversation tablosundaki tüm satýrlarý döner
    }
}
