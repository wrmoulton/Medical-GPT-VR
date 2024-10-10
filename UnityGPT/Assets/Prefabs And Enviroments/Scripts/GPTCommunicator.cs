using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.IO;
using OpenAI_API.Audio;

public class GPTCommunicator : MonoBehaviour
{
    public AudioPlayer audioPlayer;
    public TMP_Text textField;
    public TMP_InputField inputField;
    public Button okButton;
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    
    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAI_API.OpenAIAPI("sk-proj-nUtHbn2IkGEYMwRCzuuXT3BlbkFJfLuN8QqwvB5MJhZd9981");
        StartConversation();
        okButton.onClick.AddListener(() => GetResponse());
    }
    
    private void StartConversation()
    {
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "You are a healthcare instructor teaching kids ages 5-12 about the human body. When asked, please explain to them in easy words what that part of the body does for human function and medical problems associated with that part. Keep your responses very short, max 2 sentences.")};
        inputField.text = "";
        string startString = "Welcome to MedicalGPT, please ask a question about a human body part.";
        textField.text = startString;
        Debug.Log(startString); 
    }
    private async void GetResponse()
    {
        if (inputField.text.Length < 1) 
        {
            return;         
        }
        //Disable OK button
        okButton.enabled = false;

        //Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.TextContent = inputField.text;
        if (userMessage.TextContent.Length > 100) 
        { 
            userMessage.TextContent = userMessage.TextContent.Substring(0, 100);
        }
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole,userMessage.TextContent));
        //Add the message to the list
        messages.Add(userMessage);

        //Update text field with user message
        textField.text = string.Format("You: {0}",userMessage.TextContent);

        //Clear the input field
        inputField.text = "";

        //send the entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = messages
        });

        //Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.TextContent = chatResult.Choices[0].Message.TextContent;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.TextContent));

        //Text to Speech
        
        var speechFilePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
        try
        {

            var response = await api.TextToSpeech.SaveSpeechToFileAsync(new TextToSpeechRequest
            {
                Model = "tts-1",
                Input = responseMessage.TextContent,
                Voice = TextToSpeechRequest.Voices.Alloy,
                ResponseFormat = TextToSpeechRequest.ResponseFormats.MP3,
                Speed = 1.0
            }, speechFilePath);

            if(response != null)
            {
                Debug.Log("Audio saved at: " + response.FullName);
            }
            else
            {
                Debug.Log("Failed to save Audio");
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error generating speech: {ex.Message}");
        }
        // Call the PlayAudio method from the AudioPlayer script
        if (audioPlayer != null)
        {
            audioPlayer.PlayAudio(speechFilePath);
        }
        else
        {
            Debug.LogError("Audio script not working");
        }
        //End of Text to Speech

        //Add the reponse to the list of messages
        messages.Add(responseMessage);

        //Update text field with reponse
        textField.text = string.Format("You: {0}\n\nGPT: {1}", userMessage.TextContent, responseMessage.TextContent);
        //reenable okButton
        okButton.enabled = true;
    }
    // New method to handle speech input from VoiceChat
    public void HandleSpeechInput(string speechText)
    {
        inputField.text = speechText;
        GetResponse();
    }
}
