using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadWriteCSV;
using System.IO;

/// <summary>
/// Writes a line of data after every trial, giving information on the trial.
/// </summary>
public class DataHandler : MonoBehaviour
{

    // stores the data for writing to file at end of task
    List<TrialData> trialData = new List<TrialData>();
    List<BounceData> bounceData = new List<BounceData>();

    private string pid = GlobalControl.Instance.participantID;

    /// <summary>
    /// Write all data to a file
    /// </summary>
    void OnDisable()
    {
        WriteTrialFile();
        WriteBounceFile();
    }

    // Records trial data into the data list
    public void recordTrial(float time, int trialNum, int numBounces, float score, float targetHeight,
            float targetRadius)
    {
        trialData.Add(new TrialData(time, trialNum, numBounces, score, targetHeight, targetRadius));
    }

    // Records bounce data into the data list
    public void recordBounce(float time, int trialNum, int bounceNum, float apexHeight, float apexTargetDistance,
            bool apexInTargetArea, float paddleHeight, float paddleVelocity, float paddleAccel, float targetHeight,
            Vector3 bounceMod)
    {
        bounceData.Add(new BounceData(time, trialNum, bounceNum, apexHeight, apexTargetDistance, apexInTargetArea,
            paddleHeight, paddleVelocity, paddleAccel, targetHeight, bounceMod));
    }

    /// <summary>
    /// A class that stores info on each trial relevant to data recording. Every field is
    /// public readonly, so can always be accessed, but can only be assigned once in the
    /// constructor.
    /// </summary>
    class TrialData
    {
        public readonly float time;
        public readonly int trialNum;
        public readonly int numBounces;
        public readonly float score;
        public readonly float targetHeight;
        public readonly float targetRadius;

        public TrialData(float time, int trialNum, int numBounces, float score, float targetHeight,
            float targetRadius)
        {
            this.time = time;
            this.trialNum = trialNum;
            this.numBounces = numBounces;
            this.score = score;
            this.targetHeight = targetHeight;
            this.targetRadius = targetRadius;
        }
    }

    class BounceData
    {
        public readonly float time;
        public readonly int trialNum;
        public readonly int bounceNum;
        public readonly float apexHeight;
        public readonly float apexTargetDistance;
        public readonly bool apexInTargetArea;
        public readonly float paddleHeight;
        public readonly float paddleVelocity;
        public readonly float paddleAccel;
        public readonly float targetHeight;
        public readonly Vector3 bounceMod;

        public BounceData(float time, int trialNum, int bounceNum, float apexHeight, float apexTargetDistance,
            bool apexInTargetArea, float paddleHeight, float paddleVelocity, float paddleAccel, float targetHeight,
            Vector3 bounceMod)
        {
            this.time = time;
            this.trialNum = trialNum;
            this.bounceNum = bounceNum;
            this.apexHeight = apexHeight;
            this.apexTargetDistance = apexTargetDistance;
            this.apexInTargetArea = apexInTargetArea;
            this.paddleHeight = paddleHeight;
            this.paddleVelocity = paddleVelocity;
            this.paddleAccel = paddleAccel;
            this.targetHeight = targetHeight;
            this.bounceMod = bounceMod;
        }
    }

    /// <summary>
    /// Writes the Trial File to a CSV
    /// </summary>
    private void WriteTrialFile()
    {

        // Write all entries in data list to file
        Directory.CreateDirectory(@"Data/" + pid);
        using (CsvFileWriter writer = new CsvFileWriter(@"Data/" + pid + "/" + pid + "Trial.csv"))
        {
            Debug.Log("Writing trial data to file");

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Time");
            header.Add("Trial Number");
            header.Add("Number of Bounces");
            header.Add("Score");
            header.Add("Target Height");
            header.Add("Target Radius");
            header.Add("Number of Paddles");

            writer.WriteRow(header);

            // write each line of data
            foreach (TrialData d in trialData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.time.ToString());
                row.Add(d.trialNum.ToString());
                row.Add(d.numBounces.ToString());
                row.Add(d.score.ToString());
                row.Add(d.targetHeight.ToString());
                row.Add(d.targetRadius.ToString());
                row.Add(GlobalControl.Instance.numPaddles.ToString());

                writer.WriteRow(row);
            }
        }
    }

    /// <summary>
    /// Writes the Bounce file to a CSV
    /// </summary>
    private void WriteBounceFile()
    {

        // Write all entries in data list to file
        Directory.CreateDirectory(@"Data/" + pid);
        using (CsvFileWriter writer = new CsvFileWriter(@"Data/" + pid + "/" + pid + "Bounce.csv"))
        {
            Debug.Log("Writing bounce data to file");

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Time");
            header.Add("Trial Number");
            header.Add("Bounce Number");
            header.Add("Apex Height");
            header.Add("Apex-Target Distance");
            header.Add("Apex In Target Area?");
            header.Add("Paddle height at hit");
            header.Add("Paddle velocity at hit");
            header.Add("Paddle acceleration at hit");
            header.Add("Target Height");
            header.Add("Bounce Modification Vector X");
            header.Add("Bounce Modification Vector Y");
            header.Add("Bounce Modification Vector Z");

            writer.WriteRow(header);

            // write each line of data
            foreach (BounceData d in bounceData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.time.ToString());
                row.Add(d.trialNum.ToString());
                row.Add(d.bounceNum.ToString());
                row.Add(d.apexHeight.ToString());
                row.Add(d.apexTargetDistance.ToString());
                if (d.apexInTargetArea)
                {
                    row.Add("YES");
                }
                else
                {
                    row.Add("NO");
                }
                row.Add(d.paddleHeight.ToString());
                row.Add(d.paddleVelocity.ToString());
                row.Add(d.paddleAccel.ToString());
                row.Add(d.targetHeight.ToString());
                row.Add(d.bounceMod.x.ToString());
                row.Add(d.bounceMod.y.ToString());
                row.Add(d.bounceMod.z.ToString());

                writer.WriteRow(row);
            }
        }
    }
}
