using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using OpenAI; // Import for OpenAI API
using TMPro;
using Keyboard;

public class MicrophoneRecorder : MonoBehaviour
{
    public Button microphoneButton;
    public Button stopButton;
    public DropdownManager dropdownManager;
    public KeyboardManager inputField; 
    public string selectedMicrophone;

    private AudioClip recordingClip;
    private bool isRecording = false;
    private int recordingFrequency = 44100;
    private string outputFilePath;
    private OpenAIApi openai; // Instance of OpenAI API

    void Start()
    {
        // Initialize the OpenAI API
        openai = new OpenAIApi("sk-proj-nUtHbn2IkGEYMwRCzuuXT3BlbkFJfLuN8QqwvB5MJhZd9981");

        // Initialize the buttons
        microphoneButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        
        stopButton.interactable = false;
    }
    void OnMicrophoneSelected(int index)
    {
        selectedMicrophone = Microphone.devices[index];
        Debug.Log("Selected microphone: " + selectedMicrophone);
    }

    void StartRecording()
    {
        if (!isRecording)
        {
            string selectedMicrophone = dropdownManager.selectedMicrophone;
            // Start recording
            recordingClip = Microphone.Start(selectedMicrophone, false, 15, recordingFrequency); // 15 seconds max duration
            isRecording = true;
            microphoneButton.interactable = false;
            stopButton.interactable = true;
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            // Stop recording
            Microphone.End(selectedMicrophone);
            isRecording = false;
            microphoneButton.interactable = true;
            stopButton.interactable = false;

            // Save the recording to a WAV file
            SaveRecording();
        }
    }

    void SaveRecording()
    {
        if (recordingClip == null)
        {
            Debug.LogError("Recording clip is null, cannot save.");
            return;
        }

        float[] samples = new float[recordingClip.samples * recordingClip.channels];
        recordingClip.GetData(samples, 0);

        byte[] wavFile = ConvertToWav(samples, recordingClip.channels, recordingClip.frequency);

        outputFilePath = Path.Combine(Application.persistentDataPath, "output.wav");
        File.WriteAllBytes(outputFilePath, wavFile);
        
        Debug.Log("Recording saved to " + outputFilePath);

        // Call the transcription function
        TranscribeAudio(outputFilePath);
    }

    byte[] ConvertToWav(float[] samples, int channels, int frequency)
    {
        int sampleCount = samples.Length;
        int byteCount = sampleCount * 2; // 2 bytes per sample (16-bit audio)
        int headerSize = 44;
        byte[] wavFile = new byte[byteCount + headerSize];

        // RIFF header
        System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(wavFile, 0);
        System.BitConverter.GetBytes(byteCount + headerSize - 8).CopyTo(wavFile, 4);
        System.Text.Encoding.ASCII.GetBytes("WAVE").CopyTo(wavFile, 8);

        // fmt subchunk
        System.Text.Encoding.ASCII.GetBytes("fmt ").CopyTo(wavFile, 12);
        System.BitConverter.GetBytes(16).CopyTo(wavFile, 16);
        System.BitConverter.GetBytes((short)1).CopyTo(wavFile, 20);
        System.BitConverter.GetBytes((short)channels).CopyTo(wavFile, 22);
        System.BitConverter.GetBytes(frequency).CopyTo(wavFile, 24);
        System.BitConverter.GetBytes(frequency * channels * 2).CopyTo(wavFile, 28); // Byte rate
        System.BitConverter.GetBytes((short)(channels * 2)).CopyTo(wavFile, 32); // Block align
        System.BitConverter.GetBytes((short)16).CopyTo(wavFile, 34); // Bits per sample

        // data subchunk
        System.Text.Encoding.ASCII.GetBytes("data").CopyTo(wavFile, 36);
        System.BitConverter.GetBytes(byteCount).CopyTo(wavFile, 40);

        // Audio data
        for (int i = 0; i < sampleCount; i++)
        {
            short intSample = (short)(samples[i] * short.MaxValue);
            System.BitConverter.GetBytes(intSample).CopyTo(wavFile, headerSize + i * 2);
        }

        return wavFile;
    }

    async void TranscribeAudio(string filePath)
    {
        byte[] audioData = File.ReadAllBytes(filePath);

        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = audioData, Name = "audio.wav" },
            Model = "whisper-1",
            Language = "en"
        };
        var res = await openai.CreateAudioTranscription(req);

        // Set the transcribed text to the input field for user editing
        inputField.Transcribe(res.Text);
        Debug.Log(res.Text);

        // GPTCommunicator gptCommunicator = FindObjectOfType<GPTCommunicator>();
        // if (gptCommunicator != null)
        // {
        //     gptCommunicator.HandleSpeechInput(res.Text);
        // }
        // else
        // {
        //     Debug.LogError("GPTCommunicator not found in the scene!");
        // }
    }
}
