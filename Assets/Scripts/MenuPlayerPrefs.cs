using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerPrefs : MonoBehaviour
{
    public MenuController menuController;

    private void Start()
    {
        menuController = GetComponent<MenuController>();
    }
    // All Main Menu parameters 
    public readonly string[] preferenceList =
    {
        //"dof",
        "maxtrials",
        "hovertime",
        "targetradius",
        //"condition",
        //"exploration",
        //"session",
        "targetheight",
        //"numpaddles"
    };

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

    // Public getters. Should return parameters corresponding with MenuController functions
    public int LoadMaxTrials()
    {
        return PlayerPrefs.GetInt("maxtrials");
    }
    public float LoadHoverTime()
    {
        return PlayerPrefs.GetFloat("hovertime");
    }
    public float LoadTargetRadius()
    {
        return PlayerPrefs.GetFloat("targetradius");
    }


    // Private methods to load PlayerPrefs into the menu. 
    private void LoadMaxTrialsToMenu()
    {
        if (PlayerPrefs.HasKey("maxtrials")) menuController.RecordMaxTrials(LoadMaxTrials()); 
    }
    private void LoadHoverTimeToMenu()
    {
        if (PlayerPrefs.HasKey("hovertime")) menuController.UpdateHoverTime(LoadHoverTime());
    }
    private void LoadTargetRadiusToMenu()
    {
        if (PlayerPrefs.HasKey("maxtrials")) menuController.UpdateTargetRadius(LoadTargetRadius());
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
                    default:
                        Debug.Log("Unknown PlayerPref param '" + pref + "'");
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
