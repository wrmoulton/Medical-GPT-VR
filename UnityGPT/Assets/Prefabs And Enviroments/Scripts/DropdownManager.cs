using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DropdownManager : MonoBehaviour
{
    public TMP_Dropdown microphoneDropdown;
    public string selectedMicrophone;

    void Start()
    {
        // Initialize the TMP dropdown with available microphones
        microphoneDropdown.ClearOptions();

        // Convert the list of strings to a list of TMP_Dropdown.OptionData
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (string device in Microphone.devices)
        {
            options.Add(new TMP_Dropdown.OptionData(device));
        }

        // Add the options to the TMP_Dropdown
        microphoneDropdown.AddOptions(options);

        // Default microphone selection
        if (Microphone.devices.Length > 0)
        {
            selectedMicrophone = Microphone.devices[0];
        }

        // Add a listener for when the dropdown value changes
        microphoneDropdown.onValueChanged.AddListener(OnMicrophoneSelected);
    }

    void OnMicrophoneSelected(int index)
    {
        selectedMicrophone = Microphone.devices[index];
        Debug.Log("Selected microphone: " + selectedMicrophone);
    }
}
