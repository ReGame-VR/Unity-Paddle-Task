using UnityEngine.UI;
using System.Linq;
using ReadWriteCSV;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class AfterQuestionsDataHandler : MonoBehaviour, IPointerDownHandler
{
    public ToggleGroup Q1;
    public ToggleGroup Q2;
    public ToggleGroup Q3;

    private string onQ1;
    private string onQ2;
    private string onQ3;

    public GameObject paddle;
    public GameObject controller;
    public GameObject display;
    private GameObject pointer;
    private GameObject controllerModel;
    public GameObject warnText;

    public GameObject timeToDropDisplay;
    public GameObject pauseIndicator;
    public GameObject debugDisplay;
    public GameObject targetLine;
    //public GameObject ball;
    //public GameObject ballEffect;

    private string pid;
    private string filepath;

    public void OnEnable()
    {
        if (Q1.AnyTogglesOn())
            Q1.SetAllTogglesOff();

        if (Q2.AnyTogglesOn())
            Q2.SetAllTogglesOff();

        if (Q3.AnyTogglesOn())
            Q3.SetAllTogglesOff();

        paddle.SetActive(false);
        warnText.SetActive(false);

        controllerModel = controller.transform.GetChild(0).gameObject;
        pointer = controller.transform.GetChild(2).gameObject;
        pointer.SetActive(true);
        controllerModel.SetActive(true);


        pid = GlobalControl.Instance.participantID;
        filepath = "Data/" + pid + "_" + "Questionare_AFTER.csv";

        timeToDropDisplay.SetActive(false);
        pauseIndicator.SetActive(false);
        debugDisplay.SetActive(false);
        targetLine.SetActive(false);
        //ballEffect.SetActive(false);
       // ball.GetComponent<MeshRenderer>().enabled = false;
        //ball.transform.GetChild(2).gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle toggleQ1 = Q1.ActiveToggles().FirstOrDefault();
        Toggle toggleQ2 = Q2.ActiveToggles().FirstOrDefault();
        Toggle toggleQ3 = Q3.ActiveToggles().FirstOrDefault();

        onQ1 = toggleQ1.GetComponentInChildren<TextMeshPro>().text;
        onQ2 = toggleQ2.GetComponentInChildren<TextMeshPro>().text;
        onQ3 = toggleQ3.GetComponentInChildren<TextMeshPro>().text;

        if (!Q1.AnyTogglesOn() || !Q2.AnyTogglesOn() || !Q3.AnyTogglesOn())
            warnText.SetActive(true);
        else
        {
            if (!File.Exists(filepath))
            {
                File.WriteAllText(filepath, "Level" + ","
                    + "Q1" + ","
                    + "Q2" + ","
                    + "Q3" + ","
                    + "\n");
            }


            string row = GlobalControl.Instance.difficulty.ToString() + ","
                        + onQ1.ToString() + ","
                        + onQ2.ToString() + ","
                        + onQ3.ToString() + ","
                        + "\n";
            File.AppendAllText(filepath, row);


            paddle.SetActive(true);
            pointer.SetActive(false);
            display.SetActive(false);
            SceneManager.LoadScene(0);
        }


    }

    public void OnDisable()
    {
        controllerModel.SetActive(false);
        pointer.SetActive(false);
        timeToDropDisplay.SetActive(true);
        pauseIndicator.SetActive(true);
        debugDisplay.SetActive(true);
        targetLine.SetActive(true);
        //ball.GetComponent<MeshRenderer>().enabled = true;
        //ballEffect.SetActive(true);
    }
}
