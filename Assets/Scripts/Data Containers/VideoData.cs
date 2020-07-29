using UnityEngine;
using System.Collections;
using UnityEngine.Video;

[System.Serializable]
public class VideoData
{
	public VideoClip videoClip;
	public AudioClip audioClip;
	public float audioClipStartOffset = 0f;
	public float postClipTime = 60f;
	public int difficulty;

	public VideoData (VideoClip videoClipVar, AudioClip audioClipVar, float audioClipStrartOffsetVar, float postClipTimeVar, int difficultyVar)
	{
		videoClip = videoClipVar;
		audioClip = audioClipVar;
		audioClipStartOffset = audioClipStrartOffsetVar;
		postClipTime = postClipTimeVar;
		difficulty = difficultyVar;
	}
}
