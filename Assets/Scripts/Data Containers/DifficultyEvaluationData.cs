using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DifficultyEvaluationData<T> 
{
	public DifficultyEvaluation difficultyEvaluation;
	public List<T> datas;

	public DifficultyEvaluationData(DifficultyEvaluation difficultyEvaluation, List<T> datas)
	{
		this.difficultyEvaluation = difficultyEvaluation;
		this.datas = datas;
	}

}
