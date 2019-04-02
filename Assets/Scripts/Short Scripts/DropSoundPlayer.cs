using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSoundPlayer : MonoBehaviour {

    // The bounce sound effects. Pick randomly from this array
    public AudioSource dropSound;

    // Play high pitch sound when ball is about to drop.
    public void PlayDropSound()
    {
        // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
        dropSound.Stop();
        dropSound.PlayOneShot(dropSound.clip);  
    }
    
}
