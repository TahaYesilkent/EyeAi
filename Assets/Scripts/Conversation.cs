using SQLite4Unity3d; // SQLite veritabaný desteði için gerekli kütüphane
using System;

// Bu sýnýf, veritabanýndaki "Conversation" tablosunun yapýsýný tanýmlar
public class Conversation
{
    [PrimaryKey, AutoIncrement]         // Bu alan tablo için birincil anahtar (ID) olacak ve her eklemede otomatik artacak
    public int Id { get; set; }

    public string Question { get; set; }  // Kullanýcýnýn sorduðu metin (örneðin: "Göz bebeði nedir?")
    public string Answer { get; set; }    // GPT tarafýndan verilen yanýt
    public DateTime TimeStamp { get; set; } // Bu kaydýn ne zaman oluþturulduðu (tarih + saat)
}
