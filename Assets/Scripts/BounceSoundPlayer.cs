using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceSoundPlayer : MonoBehaviour {

    // The bounce sound effects. Pick randomly from this array
    public AudioSource[] bounceSounds;

    // Play a random bounce sound from the array
    public void PlayBounceSound()
    {
        foreach (AudioSource audioPlaying in bounceSounds)
        {
            // Stop the bounce sounds that are currently playing. We dont want to overload the audio.
            audioPlaying.Stop();
        }
        AudioSource sound = bounceSounds[Random.Range(0, bounceSounds.Length)];
        sound.PlayOneShot(sound.clip);
    }
}
