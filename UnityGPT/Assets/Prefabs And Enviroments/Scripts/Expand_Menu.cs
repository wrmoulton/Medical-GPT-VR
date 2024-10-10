using UnityEngine;
using UnityEngine.UI;

public class ExpandMenuController : MonoBehaviour
{
    public GameObject expandedMenu;  // Assign the expanded menu GameObject
    public RectTransform buttonTransform;  // Assign the RectTransform of the button
    public Vector2 expandedSize = new Vector2(500, 500);  // Size when expanded
    public Vector2 originalSize;  // Original size of the button

    private bool isMenuExpanded = false;

    void Start()
    {
        originalSize = buttonTransform.sizeDelta;
        expandedMenu.SetActive(false);  // Ensure the menu is hidden initially
    }

    public void ToggleMenu()
    {
        isMenuExpanded = !isMenuExpanded;

        if (isMenuExpanded)
        {
            ExpandMenu();
        }
        else
        {
            CollapseMenu();
        }
    }

    void ExpandMenu()
    {
        // Expand the button to the menu size
        buttonTransform.sizeDelta = expandedSize;

        // Optionally disable the button's interactive components if you want the menu to take over
        Button button = buttonTransform.GetComponent<Button>();
        if (button != null) button.interactable = false;

        // Show the menu
        expandedMenu.SetActive(true);
    }

    void CollapseMenu()
    {
        // Shrink the button back to its original size
        buttonTransform.sizeDelta = originalSize;

        // Optionally re-enable the button's interactive components
        Button button = buttonTransform.GetComponent<Button>();
        if (button != null) button.interactable = true;

        // Hide the menu
        expandedMenu.SetActive(false);
    }
}

