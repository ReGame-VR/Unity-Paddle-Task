using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDegreeInput : MonoBehaviour
{
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
        Debug.Log("test");
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            Debug.Log("condition is reduced");
            inputBox.SetActive(true);
        }
        else
        {
            Debug.Log("condition is else");
            if (inputBox.activeSelf)
            {
                Debug.Log("condition was active, disabling");
                inputBox.SetActive(false);
            }
        }
    }
}
