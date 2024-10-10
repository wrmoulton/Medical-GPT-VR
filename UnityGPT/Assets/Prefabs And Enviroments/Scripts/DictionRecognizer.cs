using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class DictionRecognizer : MonoBehaviour
{
    public Button micButton;
    public Text responseText;
    private DictationRecognizer dictationRecognizer;
    private bool isRecording = false;
    private GPTCommunicator gptCommunicator; 

    void Start()
    {
        if (micButton == null)
        {
            Debug.LogError("MicButton is not assigned.");
        }
        

        micButton.onClick.AddListener(ToggleRecording);

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += OnDictationError;

        // Find and reference the GPTCommunicator script
        gptCommunicator = FindObjectOfType<GPTCommunicator>();

        if (gptCommunicator == null)
        {
            Debug.LogError("GPTCommunicator not found in the scene.");
        }
    }

    void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        if (dictationRecognizer == null)
        {
            Debug.LogError("DictationRecognizer is not initialized.");
            return;
        }

        isRecording = true;
        dictationRecognizer.Start();
        Debug.Log("Recording started.");
    }

    void StopRecording()
    {
        if (dictationRecognizer == null)
        {
            Debug.LogError("DictationRecognizer is not initialized.");
            return;
        }

        isRecording = false;
        dictationRecognizer.Stop();
        Debug.Log("Recording stopped.");
    }

    void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        responseText.text = "You said: " + text;
        Debug.Log(text);
        gptCommunicator.HandleSpeechInput(text);
    }

    void OnDictationHypothesis(string text)
    {
        responseText.text = "Recognizing: " + text;
    }

    void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogWarning("Dictation completed unsuccessfully: " + cause);
        }
    }

    void OnDictationError(string error, int hresult)
    {
        Debug.LogError("Dictation error: " + error);
    }
}
