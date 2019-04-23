using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseIndicator : MonoBehaviour
{
    private GameObject quad;

    // Start is called before the first frame update
    void Start()
    {
        quad = GameObject.Find("Pause Indicator");
        quad.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.forward;
        HandleIndicatorVisibility();
    }

    void HandleIndicatorVisibility()
    {
        if (GlobalControl.Instance.paused)
        {
            if (!quad.activeSelf)
            {
                quad.SetActive(true);
            }
        }
        else
        {
            if (quad.activeSelf)
            {
                quad.SetActive(false);
            }
        }
    }
}
