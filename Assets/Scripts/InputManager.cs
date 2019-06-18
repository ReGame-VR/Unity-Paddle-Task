using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// InputManager listens for keyboard input and calls the appropriate function.
public class InputManager : MonoBehaviour
{  
    void Update()
    {
        ListenForInput();
    }

    private void ListenForInput()
    {
        // Toggle pause/resume state
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>().TogglePause();
        }

        // Toggle debugger overlay
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject.Find("Debugger Display").GetComponent<DebuggerDisplay>().ToggleDisplay();
        }

        // Re-calibrate target line height
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameObject.Find("[SteamVR]").GetComponent<PaddleGame>().SetTargetLineHeight();
        }
    }
}
