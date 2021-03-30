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
	List<DifficultyEvaluationData<TrialData>> trialDatas = new List<DifficultyEvaluationData<TrialData>>();
	//{
	//    { DifficultyEvaluation.BASE, new List<TrialData>() },
	//    { DifficultyEvaluation.MODERATE, new List<TrialData>() },
	//    { DifficultyEvaluation.MAXIMAL, new List<TrialData>() },
	//    { DifficultyEvaluation.CUSTOM, new List<TrialData>() },
	//};
	List<DifficultyEvaluationData<BounceData>> bounceDatas = new List<DifficultyEvaluationData<BounceData>>();
	//{
	//	{ DifficultyEvaluation.BASE, new List<BounceData>() },
	//	{ DifficultyEvaluation.MODERATE, new List<BounceData>() },
	//	{ DifficultyEvaluation.MAXIMAL, new List<BounceData>() },
	//	{ DifficultyEvaluation.CUSTOM, new List<BounceData>() },
	//};
	List<DifficultyEvaluationData<ContinuousData>> continuousDatas = new List<DifficultyEvaluationData<ContinuousData>>();
	//{
	//	{ DifficultyEvaluation.BASE, new List<ContinuousData>() },
	//	{ DifficultyEvaluation.MODERATE, new List<ContinuousData>() },
	//	{ DifficultyEvaluation.MAXIMAL, new List<ContinuousData>() },
	//	{ DifficultyEvaluation.CUSTOM, new List<ContinuousData>() },
	//};
	List<DifficultyEvaluationData<DifficultyData>> difficultyDatas = new List<DifficultyEvaluationData<DifficultyData>>();
	//{
	//	{ DifficultyEvaluation.BASE, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.MODERATE, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.MAXIMAL, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.CUSTOM, new List<DifficultyData>() },
	//};

	// record eye data
	List<DifficultyEvaluationData<EyeData>> eyeDatas = new List<DifficultyEvaluationData<EyeData>>();

	HeaderData headerData;

	private string pid;

	bool isExplorationMode = (GlobalControl.Instance.condition == Condition.ENHANCED);

	int difficultyEvaluationIndex = -1;

	Dictionary<DifficultyEvaluation, int> evaluationsCount = new Dictionary<DifficultyEvaluation, int>();

	public bool dataWritten = false;

	public void InitializeDifficultyEvaluationData(DifficultyEvaluation difficultyEvaluation)
	{
		trialDatas.Add(new DifficultyEvaluationData<TrialData>(difficultyEvaluation, new List<TrialData>()));
		bounceDatas.Add(new DifficultyEvaluationData<BounceData>(difficultyEvaluation, new List<BounceData>()));
		continuousDatas.Add(new DifficultyEvaluationData<ContinuousData>(difficultyEvaluation, new List<ContinuousData>()));
		difficultyDatas.Add(new DifficultyEvaluationData<DifficultyData>(difficultyEvaluation, new List<DifficultyData>()));
		// record eye data
		eyeDatas.Add(new DifficultyEvaluationData<EyeData>(difficultyEvaluation, new List<EyeData>()));

		difficultyEvaluationIndex++;
	}

	int GetEvaluationsIteration(DifficultyEvaluation difficultyEvaluation)
	{
		int evaluation = 0;
		if (!evaluationsCount.ContainsKey(difficultyEvaluation))
		{
			evaluation = 1;
			evaluationsCount.Add(difficultyEvaluation, 1);
		}
		else
		{
			evaluationsCount[difficultyEvaluation]++;
			evaluation = evaluationsCount[difficultyEvaluation];
		}

		return evaluation;
	}

	void ResetEvaluationsIteration()
	{
		evaluationsCount = new Dictionary<DifficultyEvaluation, int>();
	}


	/// <summary>
	/// Write all data to files
	/// </summary>
	public void WriteDataToFiles()
	{
		if (dataWritten)
		{
			Debug.Log("Data already written, skipping...");
			return;
		}

		dataWritten = true;
		// make pid folder unique
		System.DateTime now = System.DateTime.Now;
		pid = GlobalControl.Instance.participantID + "_" + "Level" + GlobalControl.Instance.difficulty.ToString() + "_" + now.Month.ToString() + "-" + now.Day.ToString() + "-" + "_" + now.Hour + "-" + now.Minute + "-" + now.Second; // + "_" + pid;

		if (GlobalControl.Instance.recordingData)
		{
			WriteTrialFile();
			WriteBounceFile();
			WriteContinuousFile();
			WriteDifficultyFile();
			// record eye
			WriteEyeFile();
		}
	}

	//public List<TrialData> GetTrialData(DifficultyEvaluation difficultyEvaluation)
	//{
	//	return trialData[difficultyEvaluation];
	//}

	//public Dictionary<DifficultyEvaluation, List<TrialData>> GetTrialData()
	//{
	//	return trialData;
	//}

	// Records trial data into the data list
	public void recordTrial(float degreesOfFreedom, float time, float trialTime, int trialNum, int numBounces, int numAccurateBounces, DifficultyEvaluation difficultyEvaluation, int difficulty)
	{
		trialDatas[difficultyEvaluationIndex].datas.Add(new TrialData(degreesOfFreedom, time, trialTime, trialNum, numBounces, numAccurateBounces, difficulty));
	}

	// Records bounce data into the data list
	public void recordBounce(float degreesOfFreedom, float time, Vector3 bouncemod, int trialNum, int bounceNum, int bounceNumTotal, float apexTargetError, bool success, Vector3 paddleVelocity, Vector3 paddleAccel, DifficultyEvaluation difficultyEvaluation)
	{
		bounceDatas[difficultyEvaluationIndex].datas.Add(new BounceData(degreesOfFreedom, time, bouncemod, trialNum, bounceNum, bounceNumTotal, apexTargetError, success, paddleVelocity, paddleAccel));
	}

	// Records continuous ball and paddle data into the data list
	public void recordContinuous(float degreesOfFreedom, float time, Vector3 bouncemod, bool paused, Vector3 ballPos, Vector3 paddleVelocity, Vector3 paddleAccel, DifficultyEvaluation difficultyEvaluation)
	{
		continuousDatas[difficultyEvaluationIndex].datas.Add(new ContinuousData(degreesOfFreedom, time, bouncemod, paused, ballPos, paddleVelocity, paddleAccel));
	}

	public void recordDifficulty(float ballSpeed, bool targetLineActive, float targetLineHeightOffset, float targetLineWidth, float time, int difficulty)
	{
		difficultyDatas[difficultyEvaluationIndex].datas.Add(new DifficultyData(ballSpeed, targetLineActive, targetLineHeightOffset, targetLineWidth, time, difficulty));
	}

	public void recordHeaderInfo(Condition c, ExpCondition ec, Session s, int maxtime, float htime, float tradius)
	{
		headerData = new HeaderData(c, ec, s, maxtime, htime, tradius);
	}

	// record eye data
	public void recordEye(float time, bool paused, Vector3 gaze, Vector3 origin, Vector3 direction, int gazedObj)
	{
		eyeDatas[difficultyEvaluationIndex].datas.Add(new EyeData(time, paused, gaze, origin, direction, gazedObj));
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

		foreach (var trialData in trialDatas)
		{
			var evaluation = GetEvaluationsIteration(trialData.difficultyEvaluation);

			if (trialData.datas.Count <= 0) continue;

			//using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + trialData.difficultyEvaluation.ToString() + "_" + evaluation.ToString() + "_" + pid + "Trial.csv"))
			using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Trial.csv"))
			{
				Debug.Log("Writing trial data to file");

				// write session data
				WriteHeaderInfo(writer);

				// write header
				CsvRow header = new CsvRow();
				header.Add("Participant ID");
				header.Add("Time");
				header.Add("Time Between Trials");
				header.Add("Trial #");
				header.Add("# of Consecutive Bounces");
				header.Add("# of Accurate Bounces");

				writer.WriteRow(header);

				// write each line of data
				foreach (TrialData d in trialData.datas)
				{
					CsvRow row = new CsvRow();

					row.Add(pid);
					row.Add(d.time.ToString());
					row.Add(d.trialTime.ToString("0.00"));
					row.Add(d.trialNum.ToString());
					row.Add(d.numBounces.ToString());
					row.Add(d.numAccurateBounces.ToString());

					writer.WriteRow(row);
				}
			}
		}

		ResetEvaluationsIteration();
	}

	/// <summary>
	/// Writes the Bounce file to a CSV
	/// </summary>
	private void WriteBounceFile()
	{
		// Write all entries in data list to file
		string directory = "Data/" + pid;
		Directory.CreateDirectory(@directory);
		foreach (var bounceData in bounceDatas)
		{
			var evaluation = GetEvaluationsIteration(bounceData.difficultyEvaluation);
			if (bounceData.datas.Count <= 0) continue;

			//using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + bounceData.difficultyEvaluation.ToString() + "_" + evaluation.ToString() + "_" + pid + "Bounce.csv"))
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
				foreach (BounceData d in bounceData.datas)
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

		ResetEvaluationsIteration();
	}

	/// <summary>
	/// Writes the Trial File to a CSV
	/// </summary>
	private void WriteContinuousFile()
	{
		// Write all entries in data list to file
		string directory = "Data/" + pid;
		Directory.CreateDirectory(@directory);
		foreach (var continuousData in continuousDatas)
		{
			var evaluation = GetEvaluationsIteration(continuousData.difficultyEvaluation);
			if (continuousData.datas.Count <= 0) continue;


			//using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + continuousData.difficultyEvaluation.ToString() + "_" + evaluation.ToString() + "_" + pid + "Continuous.csv"))
			using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Continuous.csv"))
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
				foreach (ContinuousData d in continuousData.datas)
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
		ResetEvaluationsIteration();
	}

	/// <summary>
	/// Writes the Difficulty file to a CSV
	/// </summary>
	private void WriteDifficultyFile()
	{
		// Write all entries in data list to file
		string directory = "Data/" + pid;
		Directory.CreateDirectory(@directory);

		if (difficultyDatas.Count <= 0) return;

		//using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + "DifficultyData_" + pid + ".csv"))
		using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + ".csv"))
		{
			Debug.Log("Writing diffiuclty data to file");

			// write session data
			WriteHeaderInfo(writer);

			// write header
			CsvRow header = new CsvRow();
			header.Add("Difficulty Evaluation");
			header.Add("Difficulty");
			header.Add("Participant ID");
			header.Add("Timestamp");
			header.Add("Ball Speed");
			header.Add("Target Line Active");
			header.Add("Target Line Height Offset");
			header.Add("Target Line Width");

			writer.WriteRow(header);

			foreach (var difficultyData in difficultyDatas)
			{
				// var evaluation = GetEvaluationsIteration(difficultyData.difficultyEvaluation);

				// write each line of data
				foreach (DifficultyData d in difficultyData.datas)
				{
					CsvRow row = new CsvRow();

					row.Add(difficultyData.difficultyEvaluation.ToString());
					row.Add(d.difficulty.ToString());
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
		// ResetEvaluationsIteration();
	}

	// record eye data
	private void WriteEyeFile()
	{
		// Write all entries in data list to file
		string directory = "Data/" + pid;
		Directory.CreateDirectory(@directory);
		foreach (var eyeData in eyeDatas)
		{
			var evaluation = GetEvaluationsIteration(eyeData.difficultyEvaluation);
			if (eyeData.datas.Count <= 0) continue;


			//using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + eyeData.difficultyEvaluation.ToString() + "_" + evaluation.ToString() + "_" + pid + "Eye.csv"))
			using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + pid + "Eye.csv"))
			{
				Debug.Log("Writing continuous data to file");

				// write session data
				WriteHeaderInfo(writer);

				// write header
				CsvRow header = new CsvRow();
				header.Add("Participant ID");
				header.Add("Timestamp");
				header.Add("Paused?");
				header.Add("Gaze X");
				header.Add("Gaze Y");
				header.Add("Gaze Z");
				header.Add("Origin X");
				header.Add("Origin Y");
				header.Add("Origin Z");
				header.Add("Direction X");
				header.Add("Direction Y");
				header.Add("Direction Z");
				header.Add("GazedObj");

				writer.WriteRow(header);

				// write each line of data
				foreach (EyeData d in eyeData.datas)
				{
					CsvRow row = new CsvRow();

					row.Add(pid);
					row.Add(d.time.ToString());
					row.Add(d.paused ? "PAUSED" : "");
					row.Add(d.gaze.x.ToString());
					row.Add(d.gaze.y.ToString());
					row.Add(d.gaze.z.ToString());
					row.Add(d.origin.x.ToString());
					row.Add(d.origin.y.ToString());
					row.Add(d.origin.z.ToString());
					row.Add(d.direction.x.ToString());
					row.Add(d.direction.y.ToString());
					row.Add(d.direction.z.ToString());
					row.Add(d.gazedObj.ToString());

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
