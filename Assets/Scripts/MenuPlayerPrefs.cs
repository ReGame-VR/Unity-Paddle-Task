using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerPrefs : MonoBehaviour
{
    private MenuController menuController;

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
    }
    public void SaveMaxTrials(string giMaxTrials)
    {
        PlayerPrefs.SetString("maxtrials", giMaxTrials);
    }
    public void SaveHoverTime(float sliderValue)
    {
        PlayerPrefs.SetFloat("hovertime", sliderValue);
    }
    public void SaveTargetRadius(float sliderValue)
    {
        PlayerPrefs.SetFloat("targetradius", sliderValue);
    }
    public void SaveCondition(int menuInt)
    {
        PlayerPrefs.SetInt("condition", menuInt);
    }
    public void SaveExplorationMode(int menuInt)
    {
        PlayerPrefs.SetInt("exploration", menuInt);
    }
    public void SaveSession(int menuInt)
    {
        PlayerPrefs.SetInt("session", menuInt);
    }
    public void SaveTargetHeight(int menuInt)
    {
        PlayerPrefs.SetInt("targetheight", menuInt);
    }
    public void SaveNumPaddles(int menuInt)
    {
        PlayerPrefs.SetInt("numpaddles", menuInt);
    }

    // Public getters. Should return parameters corresponding with MenuController functions
    public string LoadMaxTrials()
    {
        return PlayerPrefs.GetString("maxtrials");
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
                        GameObject.Find("Number of Trials InputField").GetComponent<InputField>().text = LoadMaxTrials();
                        break;
                    case "hovertime":
                        GameObject.Find("Ball Respawn Time Slider").GetComponent<Slider>().value = LoadHoverTime();
                        break;
                    case "targetradius":
                        GameObject.Find("Success Threshold Slider").GetComponent<Slider>().value = LoadTargetRadius();
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
        PlayerPrefs.DeleteAll();
    }
}
