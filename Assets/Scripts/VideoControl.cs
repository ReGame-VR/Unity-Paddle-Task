using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer player;
    public float postVideoDelay = 3f;
    public GameObject renderTarget;
    public bool editorTesting = false;
    public PaddleGame paddleGame;
    public List<VideoPracticeData> practiceDatas = new List<VideoPracticeData>();
    
    VideoClip video;
    Coroutine playbackFinished;
    GlobalControl globalControl;
    float playedTime = 0f;

    void Start()
    {
        globalControl = GlobalControl.Instance;

        // TODO populate with real times and pause times. 
        practiceDatas = new List<VideoPracticeData>()
        {
            new VideoPracticeData(5f, 5f, new UnityAction(() => 
            { 
                globalControl.timescale = .5f; 

            }))
        };

        video = player.clip; 
        if (GlobalControl.Instance.playVideo)
		{
            GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>().TogglePause();

#if UNITY_EDITOR
            if (editorTesting)
		    {
                float appStartDelay = (float)video.length + postVideoDelay;
                appStartDelay = 10f;
                playbackFinished = StartCoroutine(PlaybackFinished(appStartDelay));
		    }
#else
        editorTesting = false;
#endif

            float total = 0;
            for (int i = 0; i < practiceDatas.Count; i++)
			{
                total += practiceDatas[i].playbackDuration;
                StartCoroutine(PracticeTime(total, practiceDatas[i].practiceDuration, practiceDatas[i].practiceChanges));
                total += practiceDatas[i].practiceDuration;
			}

            if (!editorTesting)
			{
                playbackFinished = StartCoroutine(PlaybackFinished(total));
			}

            player.Play();
		}
		else
		{
            renderTarget.gameObject.SetActive(false);
        }
    }

	void Update()
	{
        if (Input.GetKeyDown(KeyCode.V))
		{
            StopCoroutine(playbackFinished);
            StartCoroutine(PlaybackFinished(0));
		}
	}

    IEnumerator PlaybackFinished(float delaySeconds)
	{
        yield return new WaitForSeconds(delaySeconds);
        renderTarget.gameObject.SetActive(false);
        player.Stop();
        GlobalControl.Instance.recordingData = true;
        paddleGame.StartRecording();
	}

    IEnumerator PracticeTime(float start, float duration, UnityAction practiceChanges)
	{
        yield return new WaitForSeconds(start);
        practiceChanges?.Invoke();
        player.Pause();
        yield return new WaitForSeconds(duration);
        player.Play();
	}
}
