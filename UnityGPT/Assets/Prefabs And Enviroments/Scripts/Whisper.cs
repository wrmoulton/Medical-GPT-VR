using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Add this line to resolve the List<> error
using System.IO; // Import for Path and File

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private TMP_InputField inputField; // Reference to the TMP input field
        [SerializeField] private Dropdown microphoneDropdown; // Dropdown to select microphone

        private readonly string fileName = "output.wav";
        
        private AudioClip clip;
        private OpenAIApi openai;
        private string selectedMicrophone;

        private void Start()
        {
            openai = new OpenAIApi("sk-proj-nUtHbn2IkGEYMwRCzuuXT3BlbkFJfLuN8QqwvB5MJhZd9981");

            recordButton.onClick.AddListener(OnRecordButtonPressed);
            recordButton.onClick.AddListener(OnRecordButtonReleased);

            // Populate the microphone dropdown
            PopulateMicrophoneDropdown();
        }

        private void OnRecordButtonPressed()
        {
            // Start recording when the button is pressed
            StartRecording();
        }

        private void OnRecordButtonReleased()
        {
            // Stop recording when the button is released
            StopRecording();
        }

        private void StartRecording()
        {
            clip = Microphone.Start(selectedMicrophone, false, 60, 44100); // Recording with a maximum duration of 60 seconds
        }

        private async void StopRecording()
        {
            Microphone.End(selectedMicrophone);
            
            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            // Set the transcribed text to the input field for user editing
            inputField.text = res.Text;
            Debug.Log(res.Text);


            GPTCommunicator gptCommunicator = FindObjectOfType<GPTCommunicator>();
            if (gptCommunicator != null)
            {
                gptCommunicator.HandleSpeechInput(res.Text);
            }
            else
            {
                Debug.LogError("GPTCommunicator not found in the scene!");
            }

            recordButton.enabled = true;
        }

        private void PopulateMicrophoneDropdown()
        {
            string[] devices = Microphone.devices;
            microphoneDropdown.ClearOptions();
            microphoneDropdown.AddOptions(new List<string>(devices));
            Debug.Log("Available microphones:");
            foreach (var device in devices)
            {
                Debug.Log(device);
            }

            // Set the default microphone to the first one in the list
            selectedMicrophone = devices.Length > 0 ? devices[0] : null;

            microphoneDropdown.onValueChanged.AddListener(delegate { OnMicrophoneSelected(microphoneDropdown); });
        }

        private void OnMicrophoneSelected(Dropdown dropdown)
        {
            selectedMicrophone = dropdown.options[dropdown.value].text;
            Debug.Log("Selected microphone: " + selectedMicrophone);
        }
    }
}

