using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadWriteCSV;
using System.IO;
using UnityEngine.Networking;

/// <summary>
/// Writes a line of data after every trial, giving information on the trial.
/// </summary>
public class DataHandler : MonoBehaviour
{
    // stores the data for writing to file at end of task
    Dictionary<DifficultyEvaluation, List<TrialData>> trialData = new Dictionary<DifficultyEvaluation, List<TrialData>>()
    {
        { DifficultyEvaluation.BASE, new List<TrialData>() },
        { DifficultyEvaluation.MODERATE, new List<TrialData>() },
        { DifficultyEvaluation.MAXIMAL, new List<TrialData>() },
        { DifficultyEvaluation.CUSTOM, new List<TrialData>() },

    };
    Dictionary<DifficultyEvaluation, List<BounceData>> bounceData = new Dictionary<DifficultyEvaluation, List<BounceData>>()
    {
        { DifficultyEvaluation.BASE, new List<BounceData>() },
        { DifficultyEvaluation.MODERATE, new List<BounceData>() },
        { DifficultyEvaluation.MAXIMAL, new List<BounceData>() },
        { DifficultyEvaluation.CUSTOM, new List<BounceData>() },

    };
    Dictionary<DifficultyEvaluation, List<ContinuousData>> continuousData = new Dictionary<DifficultyEvaluation, List<ContinuousData>>()
    {
        { DifficultyEvaluation.BASE, new List<ContinuousData>() },
        { DifficultyEvaluation.MODERATE, new List<ContinuousData>() },
        { DifficultyEvaluation.MAXIMAL, new List<ContinuousData>() },
        { DifficultyEvaluation.CUSTOM, new List<ContinuousData>() },

    };
    Dictionary<DifficultyEvaluation, List<DifficultyData>> difficultyData = new Dictionary<DifficultyEvaluation, List<DifficultyData>>()
    {
        { DifficultyEvaluation.BASE, new List<DifficultyData>() },
        { DifficultyEvaluation.MODERATE, new List<DifficultyData>() },
        { DifficultyEvaluation.MAXIMAL, new List<DifficultyData>() },
        { DifficultyEvaluation.CUSTOM, new List<DifficultyData>() },

    };
    HeaderData headerData;

    private string pid = GlobalControl.Instance.participantID;

    bool isExplorationMode = (GlobalControl.Instance.condition == Condition.ENHANCED);

    /// <summary>
    /// Write all data to files
    /// </summary>
    public void WriteDataToFiles()
    {
        // make pid folder unique
        pid = System.DateTime.Today.Month.ToString() + "-" + System.DateTime.Today.Day.ToString() + "-" + System.DateTime.Now.Millisecond.ToString() + "_" + pid;

        if (GlobalControl.Instance.recordingData)
		{
            WriteTrialFile();
            WriteBounceFile();
            WriteContinuousFile();
            WriteDifficultyFile();
		}
    }

    public List<TrialData> GetTrialData(DifficultyEvaluation difficultyEvaluation) 
    { 
        return trialData[difficultyEvaluation]; 
    }

    public Dictionary<DifficultyEvaluation, List<TrialData>> GetTrialData()
    {
        return trialData;
    }

    // Records trial data into the data list
    public void recordTrial(float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces, DifficultyEvaluation difficultyEvaluation, int difficulty)
    {
        trialData[difficultyEvaluation].Add(new TrialData(degreesOfFreedom, time, trialNum, numBounces, numAccurateBounces, difficulty));
    }

    // Records bounce data into the data list
    public void recordBounce(float degreesOfFreedom, float time, Vector3 bouncemod, int trialNum, int bounceNum, int bounceNumTotal, float apexTargetError, bool success, Vector3 paddleVelocity, Vector3 paddleAccel, DifficultyEvaluation difficultyEvaluation)
    {
        bounceData[difficultyEvaluation].Add(new BounceData(degreesOfFreedom, time, bouncemod, trialNum, bounceNum, bounceNumTotal, apexTargetError, success, paddleVelocity, paddleAccel));
    }

    // Records continuous ball and paddle data into the data list
    public void recordContinuous(float degreesOfFreedom, float time, Vector3 bouncemod, bool paused, Vector3 ballPos, Vector3 paddleVelocity, Vector3 paddleAccel, DifficultyEvaluation difficultyEvaluation)
    {
        continuousData[difficultyEvaluation].Add(new ContinuousData(degreesOfFreedom, time, bouncemod, paused, ballPos, paddleVelocity, paddleAccel));
    }

    public void recordDifficulty(float ballSpeed, bool targetLineActive, float targetLineHeightOffset, float targetLineWidth, float time, DifficultyEvaluation difficultyEvaluation)
	{
        difficultyData[difficultyEvaluation].Add(new DifficultyData(ballSpeed, targetLineActive, targetLineHeightOffset, targetLineWidth, time));
	}

    public void recordHeaderInfo(Condition c, ExpCondition ec, Session s, int maxtime, float htime, float tradius)
    {
        headerData = new HeaderData(c, ec, s, maxtime, htime, tradius);
    }

    private void WriteHeaderInfo(CsvFileWriter writer)
    {
        CsvRow c = new CsvRow();
        c.Add("Condition");
        c.Add(headerData.condition.ToString());

        //CsvRow ec = new CsvRow();
        //c.Add("Physics Modification");
        //c.Add(headerData.expCondition.ToString());

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
        foreach (var trialDataKVP in trialData)
		{
            using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + trialDataKVP.Key.ToString() + "_" + pid + "Trial.csv"))
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
                foreach (TrialData d in trialDataKVP.Value)
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
    }

    /// <summary>
    /// Writes the Bounce file to a CSV
    /// </summary>
    private void WriteBounceFile()
    {
        // Write all entries in data list to file
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        foreach (var bounceDataKVP in bounceData)
		{
            using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + bounceDataKVP.Key.ToString() + "_" + pid + "Bounce.csv"))
            {
                Debug.Log("Writing bounce data to file");

                // write session data
                WriteHeaderInfo(writer);

                // write header
                CsvRow header = new CsvRow();
                header.Add("Participant ID");
                header.Add("Timestamp");
                header.Add("Trial #");
                // header.Add("Enhanced Y-velocity offset"); // bounce modification 
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
                foreach (BounceData d in bounceDataKVP.Value)
                {
                    CsvRow row = new CsvRow();


                    row.Add(pid);
                    row.Add(d.time.ToString());
                    row.Add(d.trialNum.ToString());
                    // row.Add(DhWriteYBounceMod(d.bounceModification));
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
    }

    /// <summary>
    /// Writes the Trial File to a CSV
    /// </summary>
    private void WriteContinuousFile()
    {
        // Write all entries in data list to file
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        foreach (var continuousDataKVP in continuousData)
		{
            using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + continuousDataKVP.Key.ToString() + "_" + pid + "Continuous.csv"))
            {
                Debug.Log("Writing continuous data to file");

                // write session data
                WriteHeaderInfo(writer);

                // write header
                CsvRow header = new CsvRow();
                header.Add("Participant ID");
                header.Add("Timestamp");
                header.Add("Enhanced Y-velocity offset"); // bounce modification 
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
                foreach (ContinuousData d in continuousDataKVP.Value)
                {
                    CsvRow row = new CsvRow();

                    row.Add(pid);
                    row.Add(d.time.ToString());
                    row.Add(DhWriteYBounceMod(d.bounceModification));
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

    /// <summary>
    /// Writes the Difficulty file to a CSV
    /// </summary>
    private void WriteDifficultyFile()
    {
        // Write all entries in data list to file
        string directory = "Data/" + pid;
        Directory.CreateDirectory(@directory);
        foreach (var difficultyDataKVP in difficultyData)
        {
            using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + difficultyDataKVP.Key.ToString() + "_" + pid + "Difficulty.csv"))
            {
                Debug.Log("Writing bounce data to file");

                // write session data
                WriteHeaderInfo(writer);

                // write header
                CsvRow header = new CsvRow();
                header.Add("Participant ID");
                header.Add("Timestamp");
                header.Add("Ball Speed");
                header.Add("Target Line Active");
                header.Add("Target Line Height Offset");
                header.Add("Target Line Width");

                writer.WriteRow(header);

                // write each line of data
                foreach (DifficultyData d in difficultyDataKVP.Value)
                {
                    CsvRow row = new CsvRow();

                    row.Add(pid);
                    row.Add(d.time.ToString());
                    row.Add(d.ballSpeed.ToString());
                    row.Add(d.targetLineActive.ToString());
                    row.Add(d.targetLineHeightOffset.ToString());
                    row.Add(d.targetLineWidth.ToString());

                    writer.WriteRow(row);
                }
            }
        }
    }

    // utility functions --------------------------------------------

    string DhWriteYBounceMod(Vector3 bm)
    {
        return isExplorationMode ? bm.y.ToString() : "NORMAL"; 
    }
}
