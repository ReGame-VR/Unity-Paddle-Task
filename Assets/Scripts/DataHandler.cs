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
    List<ContinuousData> continuousData = new List<ContinuousData>();

    private string pid = GlobalControl.Instance.participantID;

    /// <summary>
    /// Write all data to a file
    /// </summary>
    void OnDisable()
    {
        WriteTrialFile();
        WriteBounceFile();
        WriteContinuousFile();
    }

    // Records trial data into the data list
    public void recordTrial(Condition condition, Visit visit, float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces)
    {
        trialData.Add(new TrialData(condition, visit, degreesOfFreedom, time, trialNum, numBounces, numAccurateBounces));
    }

    // Records bounce data into the data list
    public void recordBounce(Condition condition, Visit visit, float degreesOfFreedom, int trialNum, int bounceNum, float apexTargetError,
            float paddleVelocity, float paddleAccel)
    {
        bounceData.Add(new BounceData(condition, visit, degreesOfFreedom, trialNum, bounceNum, apexTargetError, paddleVelocity, paddleAccel));
    }

    // Records continuous ball and paddle data into the data list
    public void recordContinuous(Condition condition, Visit visit, float degreesOfFreedom, float time, float ballPosX,
            float ballPosY, float ballPosZ, float paddleVelocity, float paddleAccel)
    {
        continuousData.Add(new ContinuousData(condition, visit, degreesOfFreedom, time,
            ballPosX, ballPosY, ballPosZ, paddleVelocity, paddleAccel));
    }

    /// <summary>
    /// A class that stores info on each trial relevant to data recording. Every field is
    /// public readonly, so can always be accessed, but can only be assigned once in the
    /// constructor.
    /// </summary>
    class TrialData
    {
        /*
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
        */
        public readonly Condition condition;
        public readonly Visit visit;
        public readonly float degreesOfFreedom;
        public readonly float time;
        public readonly int trialNum;
        public readonly int numBounces;
        public readonly int numAccurateBounces;

        public TrialData(Condition condition, Visit visit, float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces)
        {
            this.time = time;
            this.trialNum = trialNum;
            this.numBounces = numBounces;
            this.numAccurateBounces = numAccurateBounces;
        }
    }

    class BounceData
    {
        /*
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
        */
        public readonly Condition condition;
        public readonly Visit visit;
        public readonly float degreesOfFreedom;
        public readonly int trialNum;
        public readonly int bounceNum;
        public readonly float apexTargetError;
        public readonly float paddleVelocity;
        public readonly float paddleAccel;

        public BounceData(Condition condition, Visit visit, float degreesOfFreedom, int trialNum, int bounceNum, float apexTargetError, float paddleVelocity, float paddleAccel)
        {
            this.condition = condition;
            this.visit = visit;
            this.trialNum = trialNum;
            this.bounceNum = bounceNum;
            this.apexTargetError = apexTargetError;
            this.paddleVelocity = paddleVelocity;
            this.paddleAccel = paddleAccel;
        }

    }

    class ContinuousData
    {
        public readonly Condition condition;
        public readonly Visit visit;
        public readonly float degreesOfFreedom;
        public readonly float time;
        public readonly float ballPosX;
        public readonly float ballPosY;
        public readonly float ballPosZ;
        public readonly float paddleVelocity;
        public readonly float paddleAccel;

        public ContinuousData(Condition condition, Visit visit, float degreesOfFreedom, float time, 
            float ballPosX, float ballPosY, float ballPosZ, float paddleVelocity, float paddleAccel)
        {
            this.condition = condition;
            this.visit = visit;
            this.degreesOfFreedom = degreesOfFreedom;
            this.time = time;
            this.ballPosX = ballPosX;
            this.ballPosY = ballPosY;
            this.ballPosZ = ballPosZ;
            this.paddleVelocity = paddleVelocity;
            this.paddleAccel = paddleAccel;
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
            header.Add("Condition");      
            header.Add("Degrees of Freedom");
            header.Add("Visit");
            header.Add("Trial #");
            header.Add("# of Consecutive Bounces");
            header.Add("# of Accurate Bounces");

            writer.WriteRow(header);

            // write each line of data
            foreach (TrialData d in trialData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.time.ToString());
                row.Add(d.condition.ToString());
                //row.Add(d.degreesOfFreedom.ToString());
                row.Add(d.visit.ToString());
                row.Add(d.trialNum.ToString());
                row.Add(d.numBounces.ToString());
                row.Add(d.numAccurateBounces.ToString());

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
            header.Add("Condition");  
            header.Add("Degrees of Freedom");
            header.Add("Visit");
            header.Add("Trial Number");
            header.Add("Bounce Number");
            header.Add("Bounce Error");
            header.Add("Paddle velocity at hit");
            header.Add("Paddle acceleration at hit");

            writer.WriteRow(header);

            // write each line of data
            foreach (BounceData d in bounceData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.condition.ToString());
                //row.Add(d.degreesOfFreedom.ToString());
                row.Add(d.visit.ToString());
                row.Add(d.trialNum.ToString());
                row.Add(d.bounceNum.ToString());
                row.Add(d.apexTargetError.ToString());
                row.Add(d.paddleVelocity.ToString());
                row.Add(d.paddleAccel.ToString());

                writer.WriteRow(row);
            }
        }
    }

    /// <summary>
    /// Writes the Trial File to a CSV
    /// </summary>
    private void WriteContinuousFile()
    {

        // Write all entries in data list to file
        Directory.CreateDirectory(@"Data/" + pid);
        using (CsvFileWriter writer = new CsvFileWriter(@"Data/" + pid + "/" + pid + "Continuous.csv"))
        {
            Debug.Log("Writing continuous data to file");

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Condition");     
            header.Add("Degrees of Freedom");
            header.Add("Visit");
            header.Add("Timestamp");
            header.Add("Ball Position X");
            header.Add("Ball Position Y");
            header.Add("Ball Position Z");
            header.Add("Paddle Velocity");
            header.Add("Paddle Acceleration");

            writer.WriteRow(header);

            // write each line of data
            foreach (ContinuousData d in continuousData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.condition.ToString());
                //row.Add(d.degreesOfFreedom.ToString());
                row.Add(d.visit.ToString());
                row.Add(d.time.ToString());
                row.Add(d.ballPosX.ToString());
                row.Add(d.ballPosY.ToString());
                row.Add(d.ballPosZ.ToString());
                row.Add(d.paddleVelocity.ToString());
                row.Add(d.paddleAccel.ToString());

                writer.WriteRow(row);
            }
        }
    }
}
