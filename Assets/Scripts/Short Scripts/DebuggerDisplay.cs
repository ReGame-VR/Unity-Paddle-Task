using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerDisplay : MonoBehaviour
{
    public GameObject dbgQuad;
    public Text dbgText;

    // Start is called before the first frame update
    void Start()
    {
        dbgQuad.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            dbgQuad.SetActive(!dbgQuad.activeSelf);


        }

    }

    public void Display(string msg)
    {
        dbgText.text = msg;
    }

    public void Clear()
    {
        dbgText.text = "...";
    }
}
