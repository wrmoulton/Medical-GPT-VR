using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        // Ensure that the GameObject has an AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayAudio(string filePath)
    {
        StartCoroutine(PlayAudioFromFile(filePath));
    }

    private IEnumerator PlayAudioFromFile(string filePath)
    {
        // Load the audio file from the file path using UnityWebRequest
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                // Get the AudioClip from the request
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

                // Assign the audio clip to the AudioSource and play it
                audioSource.clip = audioClip;
                audioSource.Play();

                Debug.Log("Playing audio: " + filePath);
            }
        }
    }
}
