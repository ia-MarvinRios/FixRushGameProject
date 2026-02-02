using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UINode
{
    internal GameObject previous;
    internal GameObject next;
}

public class GUIBrain : MonoBehaviour
{
    public static GUIBrain Instance { get; private set; }

    List<UINode> uiNodes;
    int currentIndex;

    private void Awake()
    {
        Instance = this;

        uiNodes = new List<UINode>();
    }

    private void OnDisable()
    {
        EndInteraction();
    }

    /// <summary>
    /// Starts a new UI interaction sequence with the given menu as the first node.
    /// </summary>
    /// <param name="uiMenu"></param>
    public void StartNewInteraction(GameObject uiMenu)
    {
        uiNodes.Clear();

        uiNodes.Add(new UINode { previous = null, next = uiMenu });
        currentIndex = 0;

        uiMenu.SetActive(true);
    }
    /// <summary>
    /// Opens a new UI menu, adding it to the interaction stack.
    /// </summary>
    /// <param name="uiMenu"></param>
    public void OpenNewMenu(GameObject uiMenu)
    {
        uiNodes[^1].next.SetActive(false);
        uiNodes.Add(new UINode { previous = uiNodes[^1].next, next = uiMenu });
        currentIndex++;
        uiMenu.SetActive(true);
    }
    /// <summary>
    /// Closes the current UI menu and returns to the previous one.
    /// </summary>
    /// <returns></returns>
    public GameObject CloseCurrentMenu()
    {
        GameObject closed = uiNodes[^1].next;
        closed.SetActive(false);
        uiNodes.RemoveAt(uiNodes.Count - 1);
        currentIndex--;
        if (uiNodes.Count > 0)
        {
            uiNodes[^1].next.SetActive(true);
            return closed;
        }
        else
        {
            Debug.Log("No more UI Menus to go back to.");
            return null;
        }
    }
    /// <summary>
    /// Goes back to the previous UI menu in the interaction stack without removing the current one.
    /// </summary>
    public void GoBack()
    {
        if (uiNodes.Count < 2)
        {
            Debug.Log("No previous UI Menu to go back to.");
            return;
        }
        uiNodes[currentIndex + 1].previous.SetActive(false);
        uiNodes[currentIndex].previous.SetActive(true);
        currentIndex--;
    }
    /// <summary>
    /// Goes forward to the next UI menu in the interaction stack without adding a new one. Instead of opening a new menu, it reactivates the next one.
    /// </summary>
    public void GoForward()
    {
        if (uiNodes.Count < 2)
        {
            Debug.Log("No next UI Menu to go forward to.");
            return;
        }

        uiNodes[currentIndex + 1].previous.SetActive(false);
        uiNodes[currentIndex].next.SetActive(true);
        currentIndex++;
    }
    /// <summary>
    /// Ends the current UI interaction sequence, closing all menus.
    /// </summary>
    public void EndInteraction()
    {
        for (int i = uiNodes.Count - 1; i >= 0; i--)
        {
            if (uiNodes[i].next != null) uiNodes[i].next.SetActive(false);
        }
        uiNodes.Clear();
    }

    // ------------ More Useful Methods ------------

    /// <summary>
    /// Locks the cursor to the center of the screen and makes it invisible.
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    /// <summary>
    /// Unlocks the cursor and makes it visible.
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
