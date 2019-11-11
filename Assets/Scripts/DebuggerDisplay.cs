using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerDisplay : MonoBehaviour
{
    public GameObject dbgQuad;
    public Text dbgLn1, dbgLn2, dbgLn3;

    // Start is called before the first frame update
    void Start()
    {
        dbgQuad.SetActive(false);
    }

    //--------------------------------------------------------------------------

    public void ToggleDisplay()
    {
        dbgQuad.SetActive(!dbgQuad.activeSelf);
    }

    public void Display(string msg, int register = 0)
    {
        switch(register)
        {
            case 1:
                dbgLn1.text = msg;
                break;
            case 2:
                dbgLn2.text = msg;
                break;
            case 3:
                dbgLn3.text = msg;
                break;

            default:
                dbgLn3.text = msg;
                break;
        }
    }

    public void Clear(int register = 0)
    {
        switch(register)
        {
            case 1:
                dbgLn1.text = ">";
                break;
            case 2:
                dbgLn2.text = ">";
                break;
            case 3:
                dbgLn1.text = ">";
                break;

            default:
                dbgLn1.text = dbgLn2.text = dbgLn3.text = ">";
                break;
        }
    }
}
