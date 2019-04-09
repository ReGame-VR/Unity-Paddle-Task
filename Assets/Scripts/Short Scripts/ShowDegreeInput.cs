using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDegreeInput : MonoBehaviour
{
    /// TODO remove this script
    /// No longer being used in menu
    
    GameObject inputBox;

    // Start is called before the first frame update
    void Start()
    {
        inputBox = GameObject.Find("Degrees of Freedom InputField");
        inputBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            inputBox.SetActive(true);
        }
        else
        {
            if (inputBox.activeSelf)
            {
                inputBox.SetActive(false);
            }
        }
    }
}
