using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class start_practice_session : MonoBehaviour
{
    string[] microphones;
    void Start()
    {
        microphones = Microphone.devices;
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Microphone.IsRecording(microphones[0]));
    }

    public void startpractice()
    {
        //AudioSource audioSource = Camera.main.gameObject.GetComponent<AudioSource>();
        //audioSource.clip = Microphone.Start(microphones[0], true, 180, 48000);
        //while (!(Microphone.GetPosition(microphones[0]) > 0)) 
        //{

            //audioSource.Play();
        //}
    }
}
