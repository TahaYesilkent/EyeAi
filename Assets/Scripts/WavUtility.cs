using UnityEngine;
using System.IO;
using System;

/// <summary>
/// AudioClip'i .wav formatýna çeviren yardýmcý sýnýf.
/// Unity kendi içinde doðrudan Wav export etmediði için bu sýnýf özel olarak yazýlmýþtýr.
/// </summary>
public static class WavUtility
{
    /// <summary>
    /// Verilen AudioClip'i WAV formatýnda byte dizisine çevirir.
    /// </summary>
    /// <param name="clip">Ses verisi içeren AudioClip</param>
    /// <returns>WAV formatýnda byte dizisi</returns>
    public static byte[] FromAudioClip(AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0); // Ses verilerini float dizisine aktar

        // Float verileri 16-bit PCM byte dizisine dönüþtür
        byte[] wavData = ConvertAudioClipDataToInt16ByteArray(samples);

        // WAV dosyasý baþlýðýný hazýrla
        byte[] header = GetWavHeader(clip, wavData.Length);

        // Baþlýk ve ses verisini birleþtir
        byte[] finalBytes = new byte[header.Length + wavData.Length];
        Buffer.BlockCopy(header, 0, finalBytes, 0, header.Length);
        Buffer.BlockCopy(wavData, 0, finalBytes, header.Length, wavData.Length);

        return finalBytes;
    }

    /// <summary>
    /// WAV dosyasýnýn 44 byte'lýk baþlýk kýsmýný hazýrlar.
    /// </summary>
    private static byte[] GetWavHeader(AudioClip clip, int dataLength)
    {
        int sampleRate = clip.frequency;
        short channels = (short)clip.channels;
        short bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;
        int fileSize = 36 + dataLength;

        using (MemoryStream stream = new MemoryStream(44))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF baþlýðý
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(fileSize); // Dosya boyutu
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE")); // Format

            // fmt alt baþlýk (format bilgisi)
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Alt baþlýk boyutu
            writer.Write((short)1); // Ses formatý: PCM (1)
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write(bitsPerSample);

            // data alt baþlýðý (veri kýsmý)
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(dataLength); // Ses verisinin uzunluðu

            return stream.ToArray();
        }
    }

    /// <summary>
    /// Float türündeki ses verisini 16-bit PCM formatýna çevirir.
    /// </summary>
    private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
    {
        int rescaleFactor = 32767; // Float [-1,1] deðerini Int16'ya dönüþtürme katsayýsý
        byte[] intData = new byte[data.Length * 2]; // Her bir sample 2 byte (16 bit)

        for (int i = 0; i < data.Length; i++)
        {
            short value = (short)(data[i] * rescaleFactor); // Float -> short dönüþüm
            byte[] byteArr = BitConverter.GetBytes(value);
            intData[i * 2] = byteArr[0];     // Düþük byte
            intData[i * 2 + 1] = byteArr[1]; // Yüksek byte
        }

        return intData;
    }
}
