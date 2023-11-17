using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HuggingFace.API;
using System.IO;
using NAudio.Flac;
using NAudio.Wave;
using UnityEngine.UI;

public class HuggingFaceAudio : MonoBehaviour
{
    public string Prompt;
    public AudioSource AudioSource;

    public Button PlayButton;
    public Button GenerateButton;
    public TMPro.TextMeshProUGUI ErrorText;

    private bool succeed = false, querying = false;

    public void Generate()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        PlayButton.enabled = false;
        GenerateButton.enabled = false;

        GetAudioFromHF();
        while (querying) yield return new WaitForEndOfFrame();

        PlayButton.enabled = succeed;
        GenerateButton.enabled = true;
    }

    [ContextMenu("GetAudio")]
    private void GetAudioFromHF()
    {
        querying = true;
        succeed = false;

        HuggingFaceAPI.TextToAudio(
            Prompt,
            response =>
            {
                AudioSource.clip = GetAudioClip(response);
                succeed = true;
                querying = false;
            },
            error =>
            {
                Debug.Log(error);
                ErrorText.text = error;
                querying = false;
            }
        );
    }

    private AudioClip GetAudioClip(byte[] bytes)
    {
        using MemoryStream inputStream = new MemoryStream(bytes);
        using var reader = new FlacReader(inputStream);
        using MemoryStream outputStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(outputStream, reader);
        return OpenWavParser.ByteArrayToAudioClip(outputStream.ToArray());
    }
}
