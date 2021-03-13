using UnityEngine.UI;
using System.Linq;
using ReadWriteCSV;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine;

public class SubmitData : MonoBehaviour, IPointerDownHandler
{
    public ToggleGroup Q1;
    public ToggleGroup Q2;

    public string onQ1;
    public string onQ2;

    public GameObject paddle;

    public void Start()
    {
        paddle.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle toggleQ1 = Q1.ActiveToggles().FirstOrDefault();
        Toggle toggleQ2 = Q2.ActiveToggles().FirstOrDefault();

        onQ1 = toggleQ1.GetComponentInChildren<Text>().text;
        onQ2 = toggleQ2.GetComponentInChildren<Text>().text;

        if (File.Exists("Data/Questionare.csv"))
        {
            File.Create("filepath").Dispose();
            string row = "Level1" + "," 
                + onQ1.ToString() + ","
                + onQ2.ToString() + ","
                + "\n";
            File.AppendAllText("Data/Questionare.csv", row);
        }
        else
        {

            string directory = "Data/";
            Directory.CreateDirectory(@directory);

            using (CsvFileWriter writer = new CsvFileWriter(@directory + "Questionare.csv"))
            {
                Debug.Log("Writing questionare answers to file");

                CsvRow header = new CsvRow();
                header.Add("Level");
                header.Add("Q1");
                header.Add("Q2");

                writer.WriteRow(header);
                /*
                CsvRow row = new CsvRow();
                row.Add("Level1");
                row.Add(onQ1.ToString());
                row.Add(onQ2.ToString());

                writer.WriteRow(row);
                */
            }
        }

        paddle.SetActive(true);
    }
}
