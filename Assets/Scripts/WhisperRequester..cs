using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class WhisperRequester : MonoBehaviour
{
    [Header("API Key (Sadece key, Bearer ekleme kodda yapılıyor)")]
    public string apiKey = "APIKEY";

    [Header("UI Elemanları")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public Button recordButton;
    public Button sendTextButton;
    public Button stopButton;

    [Header("Buton Arka Planları")]
    public Image recordButtonImage;
    public Image sendTextButtonImage;
    public Image stopButtonImage;

    [Header("Buton Renkleri")]
    public Color defaultColor = new Color(1f, 1f, 1f, 1f);
    public Color disabledColor = new Color(1f, 1f, 1f, 0.5f);
    public Color stopActiveColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Listeleme Kontrolü")]
    public ConversationListController conversationListController;

    private AudioSource audioSource;
    private bool isStopped = false;
    private DatabaseService db;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        stopButton.gameObject.SetActive(false);
        db = new DatabaseService("gpt_records.db");
    }

    public void OnSendTextButtonClicked()
    {
        string userText = inputField.text;
        if (string.IsNullOrEmpty(userText))
        {
            outputText.text = "Lütfen bir metin girin!";
            return;
        }

        StartCoroutine(SendToGPT(userText));
    }

    public void OnRecordButtonClicked()
    {
        if (Microphone.devices.Length == 0)
        {
            outputText.text = "Mikrofon bulunamadı!";
            return;
        }
        outputText.text = "Kayıt başlıyor...";
        AudioClip clip = Microphone.Start(null, false, 5, 44100);
        StartCoroutine(StopRecordingAfter(clip, 5));
    }

    IEnumerator StopRecordingAfter(AudioClip clip, int duration)
    {
        yield return new WaitForSeconds(duration);
        Microphone.End(null);
        outputText.text = "Kayıt bitti. Ses işleniyor...";

        string path = Path.Combine(Application.persistentDataPath, "recorded.wav");
        SaveWav(path, clip);

        yield return StartCoroutine(SendToWhisper(path));
    }

    IEnumerator SendToWhisper(string filePath)
    {
        byte[] audioData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "recorded.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            TranscriptionResult result = JsonUtility.FromJson<TranscriptionResult>(response);
            string text = result.text;

            outputText.text = "Ses metne çevrildi: " + text;
            StartCoroutine(SendToGPT(text));
        }
        else
        {
            outputText.text = "Whisper hatası: " + request.error;
        }
    }

    IEnumerator SendToGPT(string inputText)
    {
        outputText.text = "GPT yanıtı bekleniyor...";

        string prompt =  inputText;

        GPTRequestBody body = new GPTRequestBody()
        {
            model = "gpt-3.5-turbo",
            messages = new Message[] { new Message { role = "user", content = prompt } }
        };

        string jsonBody = JsonUtility.ToJson(body);
        byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            GPTResponse response = JsonUtility.FromJson<GPTResponse>(jsonResponse);

            if (response != null && response.choices != null && response.choices.Length > 0)
            {
                string gptAnswer = response.choices[0].message.content.Trim();
                outputText.text = gptAnswer;
                yield return StartCoroutine(SendToTTS(gptAnswer));

                // ✅ Veritabanına kayıt
                db.InsertConversation(inputText, gptAnswer);

                
            }
            else
            {
                outputText.text = "GPT cevabı alınamadı.";
            }
        }
        else
        {
            outputText.text = "GPT isteği başarısız: " + request.error;
        }
    }

    IEnumerator SendToTTS(string text)
    {
        string ttsUrl = "https://api.openai.com/v1/audio/speech";
        string json = "{\"model\":\"tts-1\",\"input\":\"" + EscapeJson(text) + "\",\"voice\":\"nova\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest ttsRequest = new UnityWebRequest(ttsUrl, "POST");
        ttsRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        ttsRequest.downloadHandler = new DownloadHandlerAudioClip(ttsUrl, AudioType.MPEG);
        ttsRequest.SetRequestHeader("Content-Type", "application/json");
        ttsRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return ttsRequest.SendWebRequest();

        if (ttsRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(ttsRequest);
            StartCoroutine(PlayAudioClipWithStop(clip));
        }
        else
        {
            outputText.text = "TTS hatası: " + ttsRequest.error;
        }
    }

    IEnumerator PlayAudioClipWithStop(AudioClip clip)
    {
        isStopped = false;
        ToggleButtonsDuringPlayback(true);

        audioSource.clip = clip;
        audioSource.Play();

        while (audioSource.isPlaying)
        {
            if (isStopped)
                break;

            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = null;

        ToggleButtonsDuringPlayback(false);
    }

    public void OnStopButtonClicked()
    {
        isStopped = true;
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = null;
        ToggleButtonsDuringPlayback(false);
    }

    void ToggleButtonsDuringPlayback(bool isPlaying)
    {
        recordButton.gameObject.SetActive(!isPlaying);
        sendTextButton.gameObject.SetActive(!isPlaying);
        stopButton.gameObject.SetActive(isPlaying);

        if (isPlaying)
        {
            stopButtonImage.color = stopActiveColor;
        }
        else
        {
            recordButtonImage.color = defaultColor;
            sendTextButtonImage.color = defaultColor;
            stopButtonImage.color = defaultColor;
        }
    }

    string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    void SaveWav(string path, AudioClip clip)
    {
        byte[] wav = WavUtility.FromAudioClip(clip);
        File.WriteAllBytes(path, wav);
    }

    [System.Serializable] public class TranscriptionResult { public string text; }
    [System.Serializable] public class GPTRequestBody { public string model; public Message[] messages; }
    [System.Serializable] public class Message { public string role; public string content; }
    [System.Serializable] public class GPTResponse { public Choice[] choices; }
    [System.Serializable] public class Choice { public Message message; }
}
