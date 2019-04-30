using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSoundPlayer : MonoBehaviour {
    
    public AudioSource dropSound;
    public AudioSource successSound;

    // Play high pitch sound when ball is about to drop.
    public void PlayDropSound()
    {
        // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
        dropSound.Stop();
        dropSound.PlayOneShot(dropSound.clip);  
    }

    // Play success sound when ball apex is within target window
    public IEnumerator PlaySuccessSound()
    {
        yield return new WaitForSeconds(0.1f);
        successSound.Stop();
        successSound.PlayOneShot(successSound.clip);
    }
    
}
