using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ReadWriteCSV;
using System.IO;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class ToggleData : MonoBehaviour, IPointerDownHandler
{
    public string on;

    public bool selected = false;

    private GameObject a1;
    private GameObject a2;
    private GameObject a3;
    private GameObject a4;

    public void Start()
    {
        //a1 = gameObject.transform.GetChild(0).gameObject;
        //a2 = gameObject.transform.GetChild(1).gameObject;
        //a3 = gameObject.transform.GetChild(2).gameObject;
        //a4 = gameObject.transform.GetChild(3).gameObject;

    }
    public void Update()
    {
         
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = true;
    }


}
