using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class BeforeQuestionsDataHandler : MonoBehaviour, IPointerDownHandler
{
    public ToggleGroup Q1;
    public ToggleGroup Q2;

    private string onQ1;
    private string onQ2;

    public GameObject paddle;
    public GameObject controller;
    public GameObject display;
    public GameObject pointer;
    public GameObject controllerModel;
    public GameObject warnText;

    public GameObject timeToDropDisplay;
    public GameObject pauseIndicator;
    public GameObject debugDisplay;
    public GameObject ball;
    public GameObject ballEffect;
    public GameObject targetLine;

    private string pid;
    private string filepath;

    public void Start()
    {
        if (Q1.AnyTogglesOn())
            Q1.SetAllTogglesOff();

        if (Q2.AnyTogglesOn())
            Q2.SetAllTogglesOff();

        paddle.SetActive(false);
        warnText.SetActive(false);

        controllerModel = controller.transform.GetChild(0).gameObject;
        pointer = controller.transform.GetChild(2).gameObject;

        pid = GlobalControl.Instance.participantID;
        filepath = "Data/" + pid + "_" + "Questionare_BEFORE.csv";

    }

    public void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            pointer.SetActive(true);
            controllerModel.SetActive(true);
            paddle.SetActive(false);
            targetLine.SetActive(false);


            timeToDropDisplay.SetActive(false);
            pauseIndicator.SetActive(false);
            debugDisplay.SetActive(false);

            ball.GetComponent<MeshRenderer>().enabled = false;
        }
        /*
        else
        {
            pointer.SetActive(false);
            controllerModel.SetActive(false);
            paddle.SetActive(true);


            timeToDropDisplay.SetActive(true);
            pauseIndicator.SetActive(true);
            debugDisplay.SetActive(true);

            ball.GetComponent<MeshRenderer>().enabled = true;
        }
        */
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle toggleQ1 = Q1.ActiveToggles().FirstOrDefault();
        Toggle toggleQ2 = Q2.ActiveToggles().FirstOrDefault();

        onQ1 = toggleQ1.GetComponentInChildren<TextMeshPro>().text;
        onQ2 = toggleQ2.GetComponentInChildren<TextMeshPro>().text;

        if (!Q1.AnyTogglesOn() || !Q2.AnyTogglesOn())
            warnText.SetActive(true);
        else
        {
            if (!File.Exists(filepath))
            {
                File.WriteAllText(filepath, "Level" + ","
                    + "Q1" + ","
                    + "Q2" + ","
                    + "\n");
            }


            string row = GlobalControl.Instance.difficulty.ToString() + ","
                        + onQ1.ToString() + ","
                        + onQ2.ToString() + ","
                        + "\n";
            File.AppendAllText(filepath, row);


            paddle.SetActive(true);
            //pointer.SetActive(false);
            display.SetActive(false);
        }


    }

    public void OnDisable()
    {
        controllerModel.SetActive(false);
        pointer.SetActive(false);
        timeToDropDisplay.SetActive(true);
        pauseIndicator.SetActive(true);
        debugDisplay.SetActive(true);
        ball.GetComponent<MeshRenderer>().enabled = true;
        targetLine.SetActive(true);
        //ballEffect.SetActive(true);
    }
    /*

        if (File.Exists(filepath))
        {
            //File.Create("filepath").Dispose();
            string row = GlobalControl.Instance.difficulty.ToString() + ","
                + onQ1.ToString() + ","
                + onQ2.ToString() + ","
                + "\n";
            File.AppendAllText(filepath, row);
        }
        else
        {

            //string directory = "Data/" + pid;
            //Directory.CreateDirectory(@directory);

            using (CsvFileWriter writer = new CsvFileWriter(filepath))
            {
                Debug.Log("Writing questionare answers to file");

                CsvRow header = new CsvRow();
                header.Add("Level");
                header.Add("Q1");
                header.Add("Q2");

                writer.WriteRow(header);
            }

        }

        paddle.SetActive(true);
        pointer.SetActive(false);
        gameObject.SetActive(false);
    }
*/
}
