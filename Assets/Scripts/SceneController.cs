using UnityEngine;
using UnityEngine.SceneManagement; // Sahne (scene) yönetimi için gerekli kütüphane

// Bu sýnýf, sahneler arasýnda geçiþ yapmak ve uygulamayý kapatmak için kullanýlýr
public class SceneController : MonoBehaviour
{
    // Bu fonksiyon çaðrýldýðýnda, "Database" adlý sahne yüklenir
    public void GoToDatabaseScene()
    {
        SceneManager.LoadScene("Database"); // Sahne adý, Unity'deki sahne ismiyle birebir ayný olmalý
    }

    // Bu fonksiyon çaðrýldýðýnda, "Main" adlý sahne yüklenir
    public void GoToMainScene()
    {
        SceneManager.LoadScene("Main"); // Genellikle ana menü veya baþlangýç sahnesi
    }

    // Uygulamayý tamamen kapatýr
    public void QuitApp()
    {
        Application.Quit(); // Uygulamayý kapatýr (build edilmiþ versiyonda çalýþýr, editor'de etki etmez)
        Debug.Log("Uygulama kapatýlýyor..."); // Editor'de test ederken log düþer, kapatma etkisi görülmez
    }
}
