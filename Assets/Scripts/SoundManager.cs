using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource source;
    public AudioClip clip1, clip2, clip3;
    
    public void playJumpSound()
    {
        source.PlayOneShot(clip1);
    }

    public void playFireThrowSound()
    {
        source.PlayOneShot(clip2);
    }

    public void playFireExplodeSound()
    {
        source.PlayOneShot(clip3);
    }
}
