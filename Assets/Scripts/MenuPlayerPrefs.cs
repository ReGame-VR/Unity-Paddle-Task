using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerPrefs : MonoBehaviour
{
    public MenuController menuController;
    // All Main Menu parameters 
    public string[] preferenceList =
    {
        //"dof",
        "maxtrials",
        "hovertime",
        "targetradius",
        "condition",
        //"exploration",
        "expcondition",
        "session",
        "targetheight",
        //"numpaddles"
    };

    private void Start()
    {
        menuController = GetComponent<MenuController>();
    }

    // Public setters. Parameters should correspond to MenuController parameters for easy loading.
    public void SaveDOF(float giDOF)
    {
        PlayerPrefs.SetFloat("dof", giDOF);
        PlayerPrefs.Save();
    }
    public void SaveMaxTrials(int giMaxTrials)
    {
        PlayerPrefs.SetInt("maxtrials", giMaxTrials);
        PlayerPrefs.Save();
    }
    public void SaveHoverTime(float sliderValue)
    {
        PlayerPrefs.SetFloat("hovertime", sliderValue);
        PlayerPrefs.Save();
    }
    public void SaveTargetRadius(float sliderValue)
    {
        PlayerPrefs.SetFloat("targetradius", sliderValue);
        PlayerPrefs.Save();
    }
    public void SaveCondition(int menuInt)
    {
        PlayerPrefs.SetInt("condition", menuInt);
        PlayerPrefs.Save();
    }
    public void SaveExplorationMode(int menuInt)
    {
        PlayerPrefs.SetInt("exploration", menuInt);
        PlayerPrefs.Save();
    }
    public void SaveExpCondition(int menuInt)
    {
        PlayerPrefs.SetInt("expcondition", menuInt);
        PlayerPrefs.Save();
    }
    public void SaveSession(int menuInt)
    {
        PlayerPrefs.SetInt("session", menuInt);
        PlayerPrefs.Save();
    }
    public void SaveTargetHeight(int menuInt)
    {
        PlayerPrefs.SetInt("targetheight", menuInt);
        PlayerPrefs.Save();
    }
    public void SaveNumPaddles(int menuInt)
    {
        PlayerPrefs.SetInt("numpaddles", menuInt);
        PlayerPrefs.Save();
    }


    // Private methods to load PlayerPrefs into the menu. 
    private void LoadMaxTrialsToMenu()
    {
        if (PlayerPrefs.HasKey("maxtrials"))
        {
            menuController.RecordMaxTrials(PlayerPrefs.GetInt("maxtrials"));
        }
    }
    private void LoadHoverTimeToMenu()
    {
        if (PlayerPrefs.HasKey("hovertime"))
        {
            menuController.UpdateHoverTime(PlayerPrefs.GetFloat("hovertime"));
        }
    }
    private void LoadTargetRadiusToMenu()
    {
        if (PlayerPrefs.HasKey("maxtrials"))
        {
            menuController.UpdateTargetRadius(PlayerPrefs.GetFloat("targetradius"));
        }
    }
    private void LoadConditionToMenu()
    {
        if (PlayerPrefs.HasKey("condition"))
        {
            menuController.RecordCondition(PlayerPrefs.GetInt("condition"));
        }
    }

    private void LoadExpConditionToMenu()
    {
        if (PlayerPrefs.HasKey("expcondition"))
        {
            menuController.RecordExpCond(PlayerPrefs.GetInt("expcondition"));
        }
    }
    private void LoadSessionToMenu()
    {
        if (PlayerPrefs.HasKey("session"))
        {
            menuController.RecordSession(PlayerPrefs.GetInt("session"));
        }
    }
    private void LoadTargetHeightToMenu()
    {
        if (PlayerPrefs.HasKey("targetheight"))
        {
            menuController.RecordTargetHeight(PlayerPrefs.GetInt("targetheight"));
        }
    }
    


    // Loads all saved preferences to the main menu
    public void LoadAllPreferences()
    {
        foreach (string pref in preferenceList) {
            if (PlayerPrefs.HasKey(pref))
            {
                switch (pref)
                {
                    case "maxtrials":
                        LoadMaxTrialsToMenu();
                        break;
                    case "hovertime":
                        LoadHoverTimeToMenu();
                        break;
                    case "targetradius":
                        LoadTargetRadiusToMenu();
                        break;
                    case "condition":
                        LoadConditionToMenu();
                        break;
                    case "expcondition":
                        LoadExpConditionToMenu();
                        break;
                    case "session":
                        LoadSessionToMenu();
                        break;
                    case "targetheight":
                        LoadTargetHeightToMenu();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Clears all saved main menu preferences
    public void ResetPlayerPrefs()
    {
        Debug.Log("Reset Menu Preferences");
        PlayerPrefs.DeleteAll();
        // TODO 
        // SetDefaultSettings();
    }
}
