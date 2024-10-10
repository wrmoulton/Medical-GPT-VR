using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public GameObject targetObject; // The object you want to toggle
    
    public void Toggle()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
