using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ReadWriteCSV;
using System.IO;
using UnityEngine.EventSystems;
using System;

public class ToggleControl : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = true;
    }
}
