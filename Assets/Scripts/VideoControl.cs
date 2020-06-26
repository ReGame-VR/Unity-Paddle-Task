using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer player;
    public float postVideoDelay = 3f;
    public GameObject renderTarget;
    public bool editorTesting = false;
    public PaddleGame paddleGame;
    
    VideoClip video;
    Coroutine playbackFinished;

    void Start()
    {

        video = player.clip; 
        if (GlobalControl.Instance.playVideo)
		{
            GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>().TogglePause();
            float appStartDelay = (float)video.length + postVideoDelay;

#if UNITY_EDITOR
            if (editorTesting)
		    {
                appStartDelay = 10f;
		    }
#endif
            playbackFinished = StartCoroutine(PlaybackFinished(appStartDelay));

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

}
