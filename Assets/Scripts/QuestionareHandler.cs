using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR;
using System.Security.AccessControl;

public class QuestionareHandler : MonoBehaviour
{
    public GameObject hmd;
    public GameObject paddle;
    public GameObject controller;

    public GameObject beforeQuestions;
    public GameObject afterQuestions;

    public void Start()
    {
        beforeQuestions.transform.position = new Vector3(0f, hmd.transform.position.y, 0.8f);
        afterQuestions.transform.position = new Vector3(0f, hmd.transform.position.y, 0.8f);

        //beforeQuestions.SetActive(false);
        afterQuestions.SetActive(false);
    }
    public void BeforeQuestions()
    {
        beforeQuestions.SetActive(true);
        //paddle.SetActive(false);

    }

    public void AfterQuestion()
    {
        afterQuestions.SetActive(true);
    }
}
