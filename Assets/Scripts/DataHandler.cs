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
    HeaderData headerData;

    private string pid = GlobalControl.Instance.participantID;

    /// <summary>
    /// Write all data to a file
    /// </summary>
    void OnDisable()
    {
        if (pid == null | pid == "" | pid.Length == 0)
        {
            pid = System.DateTime.Today.Month.ToString() + "-" + System.DateTime.Today.Day.ToString() + "-" + System.DateTime.Now.Millisecond.ToString();
            Debug.Log("Empty PID, creating folder " + pid);
        }

            WriteTrialFile();
        WriteBounceFile();
        WriteContinuousFile();
    }

    // Records trial data into the data list
    public void recordTrial(float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces)
    {
        trialData.Add(new TrialData(degreesOfFreedom, time, trialNum, numBounces, numAccurateBounces));
    }

    // Records bounce data into the data list
    public void recordBounce(float degreesOfFreedom, float time, int trialNum, int bounceNum, int bounceNumTotal, float apexTargetError, bool success, Vector3 paddleVelocity, Vector3 paddleAccel)
    {
        bounceData.Add(new BounceData(degreesOfFreedom, time, trialNum, bounceNum, bounceNumTotal, apexTargetError, success, paddleVelocity, paddleAccel));
    }

    // Records continuous ball and paddle data into the data list
    public void recordContinuous(float degreesOfFreedom, float time, bool paused, Vector3 ballPos, Vector3 paddleVelocity, Vector3 paddleAccel)
    {
        continuousData.Add(new ContinuousData(degreesOfFreedom, time, paused, ballPos, paddleVelocity, paddleAccel));
    }

    public void recordHeaderInfo(Condition c, Session s, int maxtime, float htime, float tradius)
    {
        headerData = new HeaderData(c, s, maxtime, htime, tradius);
    }

    /// <summary>
    /// A class that stores info on each trial relevant to data recording. Every field is
    /// public readonly, so can always be accessed, but can only be assigned once in the
    /// constructor.
    /// </summary>
    class TrialData
    {
        public readonly float degreesOfFreedom;
        public readonly float time;
        public readonly int trialNum;
        public readonly int numBounces;
        public readonly int numAccurateBounces;

        public TrialData(float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces)
        {
            this.degreesOfFreedom = degreesOfFreedom;
            this.time = time;
            this.trialNum = trialNum;
            this.numBounces = numBounces;
            this.numAccurateBounces = numAccurateBounces;
        }
    }

    class BounceData
    {
        public readonly float degreesOfFreedom;
        public readonly float time;
        public readonly int trialNum;
        public readonly int bounceNum;
        public readonly int bounceNumTotal;
        public readonly float apexTargetError;
        public readonly bool success;
        public readonly Vector3 paddleVelocity;
        public readonly Vector3 paddleAccel;

        public BounceData(float degreesOfFreedom, float time, int trialNum, int bounceNum, int bounceNumTotal, float apexTargetError, bool success, Vector3 paddleVelocity, Vector3 paddleAccel)
        {
            this.degreesOfFreedom = degreesOfFreedom;
            this.time = time;
            this.trialNum = trialNum;
            this.bounceNum = bounceNum;
            this.bounceNumTotal = bounceNumTotal;
            this.apexTargetError = apexTargetError;
            this.success = success;
            this.paddleVelocity = paddleVelocity;
            this.paddleAccel = paddleAccel;
        }

    }

    class ContinuousData
    {
        public readonly float degreesOfFreedom;
        public readonly float time;
        public readonly bool paused;
        public readonly Vector3 ballPos;
        public readonly Vector3 paddleVelocity;
        public readonly Vector3 paddleAccel;
 
        public ContinuousData(float degreesOfFreedom, float time, bool paused, Vector3 ballPos, Vector3 paddleVelocity, Vector3 paddleAccel)
        {
            this.degreesOfFreedom = degreesOfFreedom;
            this.time = time;
            this.paused = paused;
            this.ballPos = ballPos;
            this.paddleVelocity = paddleVelocity;
            this.paddleAccel = paddleAccel;
        }
    }

    class HeaderData
    {
        public readonly Condition condition;
        public readonly Session session;
        public readonly int maxTrialTimeMin;
        public readonly float hoverTime;
        public readonly float targetRadius;

        public HeaderData(Condition c, Session s, int maxtime, float htime, float tradius)
        {
            this.condition = c;
            this.session = s;
            this.maxTrialTimeMin = maxtime;
            this.hoverTime = htime;
            this.targetRadius = tradius;
        }
    }


    private void WriteHeaderInfo(CsvFileWriter writer)
    {
        CsvRow c = new CsvRow();
        c.Add("Condition");
        c.Add(headerData.condition.ToString());

        CsvRow s = new CsvRow();
        s.Add("Session");
        s.Add(headerData.session.ToString());

        CsvRow maxtime = new CsvRow();
        maxtime.Add("Time Limit (s)");
        maxtime.Add(headerData.maxTrialTimeMin.ToString());

        CsvRow hovertime = new CsvRow();
        hovertime.Add("Ball Hover Time (s)");
        hovertime.Add(headerData.hoverTime.ToString());

        CsvRow trad = new CsvRow();
        trad.Add("Target Line Success Radius (m)");
        trad.Add(headerData.targetRadius.ToString());

        writer.WriteRow(c);
        writer.WriteRow(s);
        writer.WriteRow(maxtime);
        writer.WriteRow(hovertime);
        writer.WriteRow(trad);
        writer.WriteRow(new CsvRow());
    }

    /// <summary>
    /// Writes the Trial File to a CSV
    /// </summary>
    private void WriteTrialFile()
    {
        // Write all entries in data list to file
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Trial.csv"))
        {
            Debug.Log("Writing trial data to file");

            // write session data
            WriteHeaderInfo(writer);

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Time");
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
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Bounce.csv"))
        {
            Debug.Log("Writing bounce data to file");

            // write session data
            WriteHeaderInfo(writer);

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Timestamp");
            header.Add("Trial #");
            header.Add("# of Bounces");
            header.Add("Total # of Bounces");
            header.Add("Bounce Error");
            header.Add("Apex Success");
            header.Add("PaddleVelocity Magnitude");
            header.Add("PaddleVelocity X");
            header.Add("PaddleVelocity Y");
            header.Add("PaddleVelocity Z");
            header.Add("PaddleAccel Magnitude");
            header.Add("PaddleAccel X");
            header.Add("PaddleAccel Y");
            header.Add("PaddleAccel Z");

            writer.WriteRow(header);

            // write each line of data
            foreach (BounceData d in bounceData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.time.ToString());
                row.Add(d.trialNum.ToString());
                row.Add(d.bounceNum.ToString());
                row.Add(d.bounceNumTotal.ToString());
                row.Add(d.apexTargetError.ToString());
                row.Add(d.success.ToString());
                row.Add(d.paddleVelocity.magnitude.ToString());
                row.Add(d.paddleVelocity.x.ToString());
                row.Add(d.paddleVelocity.y.ToString());
                row.Add(d.paddleVelocity.z.ToString());
                row.Add(d.paddleAccel.magnitude.ToString());
                row.Add(d.paddleAccel.x.ToString());
                row.Add(d.paddleAccel.y.ToString());
                row.Add(d.paddleAccel.z.ToString());

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
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Continuous.csv"))
        {
            Debug.Log("Writing continuous data to file");

            // write session data
            WriteHeaderInfo(writer);

            // write header
            CsvRow header = new CsvRow();
            header.Add("Participant ID");
            header.Add("Timestamp");
            header.Add("Paused?");
            header.Add("Ball Velocity X");
            header.Add("Ball Velocity Y");
            header.Add("Ball Velocity Z");
            header.Add("Paddle Velocity Magnitude");
            header.Add("Paddle Velocity X");
            header.Add("Paddle Velocity Y");
            header.Add("Paddle Velocity Z");
            header.Add("Paddle Acceleration Magnitude");
            header.Add("Paddle Acceleration X");
            header.Add("Paddle Acceleration Y");
            header.Add("Paddle Acceleration Z");

            writer.WriteRow(header);

            // write each line of data
            foreach (ContinuousData d in continuousData)
            {
                CsvRow row = new CsvRow();

                row.Add(pid);
                row.Add(d.time.ToString());
                row.Add(d.paused ? "PAUSED" : "");
                row.Add(d.ballPos.x.ToString());
                row.Add(d.ballPos.y.ToString());
                row.Add(d.ballPos.z.ToString());
                row.Add(d.paddleVelocity.magnitude.ToString());
                row.Add(d.paddleVelocity.x.ToString());
                row.Add(d.paddleVelocity.y.ToString());
                row.Add(d.paddleVelocity.z.ToString());
                row.Add(d.paddleAccel.magnitude.ToString());
                row.Add(d.paddleAccel.x.ToString());
                row.Add(d.paddleAccel.y.ToString());
                row.Add(d.paddleAccel.z.ToString());

                writer.WriteRow(row);
            }
        }
    }
}
