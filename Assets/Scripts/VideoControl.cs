using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer player;
    public AudioSource audioSource;
    public float postVideoDelay = 3f;
    public GameObject renderTarget;
    public bool editorTesting = false;
    public PaddleGame paddleGame;
    public List<VideoPracticeData> practiceDatas = new List<VideoPracticeData>();
    public List<VideoData> videoDatas = new List<VideoData>();
    

    // VideoClip video;
    // Coroutine playbackFinished;
    GlobalControl globalControl;
    GlobalPauseHandler globalPauseHandler;
    float playedTime = 0f;

    void Start()
    {
        globalControl = GlobalControl.Instance;

        // TODO populate with real times and pause times. 
        //practiceDatas = new List<VideoPracticeData>()
        //{
        //    new VideoPracticeData(5f, 5f, new UnityAction(() => 
        //    { 
        //        globalControl.timescale = .5f; 

        //    }))
        //};

        if (GlobalControl.Instance.playVideo)
		{
            globalPauseHandler = GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>();
            globalPauseHandler.Pause();
            globalPauseHandler.SetIndicatorVisibility(false);


#if UNITY_EDITOR
            if (editorTesting)
		    {
    //            foreach(VideoData videoData in videoDatas)
				//{
    //                videoData.
				//}
    //            float appStartDelay = (float)video.length + postVideoDelay;
    //            appStartDelay = 10f;
    //            // playbackFinished = 
    //            StartCoroutine(PlaybackFinished(appStartDelay));
		    }
#else
        editorTesting = false;
#endif

            //         float total = 0;
            //         for (int i = 0; i < practiceDatas.Count; i++)
            //{
            //             total += practiceDatas[i].playbackDuration;
            //             StartCoroutine(PracticeTime(total, practiceDatas[i].practiceDuration, practiceDatas[i].practiceChanges));
            //             total += practiceDatas[i].practiceDuration;
            //}

            //         if (!editorTesting)
            //{
            //             playbackFinished = StartCoroutine(PlaybackFinished(total));
            //}

            float total = 0;
            for (int i = 0; i < videoDatas.Count; i++)
            {
                StartCoroutine(PracticeTime(total, videoDatas[i]));
                // total += (float)videoDatas[i].videoClip.length;
                float duration = (float)videoDatas[i].videoClip.length + videoDatas[i].postClipTime; 
                total += duration;
            }

            if (!editorTesting)
            {
                // playbackFinished = 
                StartCoroutine(PlaybackFinished(total + .2f));
            }

            // player.Play();
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
            StopAllCoroutines();
            // StopCoroutine(playbackFinished);
            StartCoroutine(PlaybackFinished(0f));
		}
	}

    IEnumerator PlaybackFinished(float delaySeconds)
	{
        yield return new WaitForSecondsRealtime(delaySeconds);
        Debug.Log("playback finished");
        renderTarget.gameObject.SetActive(false);
        player.Stop();
        audioSource.Stop();
        // paddleGame.SetDifficulty(1);
        globalControl.playVideo = false;
        globalPauseHandler.Pause();
        if (globalControl.session != Session.SHOWCASE)
        {
            globalControl.recordingData = true;
            paddleGame.StartRecording();
        }
        paddleGame.Initialize();
        globalPauseHandler.pauseIndicator.visibleOverride = false;
	}

    IEnumerator PracticeTime(float start, VideoData videoData)
	{
        yield return new WaitForSecondsRealtime(start);
        paddleGame.SetDifficulty(videoData.difficulty);
        player.clip = videoData.videoClip;
        player.Play();
        audioSource.PlayOneShot(videoData.audioClip);
        Debug.Log("playing video " + player.clip.name);
        yield return new WaitForSecondsRealtime((float)videoData.videoClip.length);
        player.Pause();
        globalPauseHandler.Resume();
        yield return new WaitForSecondsRealtime(videoData.postClipTime);
        // player.Play();
        globalPauseHandler.Pause();
    }
}
