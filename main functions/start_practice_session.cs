using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class start_practice_session : MonoBehaviour
{
    string[] microphones;
    public data loadedsong = new data();
    public mistakedata loadedmistakes = new mistakedata();
    public int miclooplength;
    public Sprite record, pause_icon;
    UnityAction playopause;
    void Start()
    {
        microphones = Microphone.devices;
        playopause = () => { justplay(); };
        gameObject.GetComponent<Button>().onClick.AddListener(playopause);
    }

    void Update()
    {
        //Debug.Log(Microphone.IsRecording(microphones[0]));
    }

    public void startpractice()
    {
        if (loadedsong.notes != null)
        {
            AudioSource audioSource = Camera.main.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
            audioSource.clip = Microphone.Start(microphones[0], true, miclooplength, 44100);
            Camera.main.gameObject.GetComponent<PDA>().clipforPDA = audioSource.clip;
            while (!(Microphone.GetPosition(microphones[0]) > 0))
            {
                audioSource.Play();
            }
            GameObject.Find("drag_cursor").transform.localPosition -= new Vector3(GameObject.Find("drag_cursor").transform.localPosition.x, 0, 0);
            Camera.main.gameObject.GetComponent<PDA>().in_practice = true;
            GameObject.Find("micophone").AddComponent<check_mistake>();
            Debug.Log("added check mistake: ");
            Camera.main.gameObject.transform.GetChild(0).gameObject.GetComponent<check_mistake>().loaded = loadedsong;
            Camera.main.gameObject.transform.GetChild(0).gameObject.GetComponent<check_mistake>().mistakes = loadedmistakes.mistake_time_type;
            GameObject.Find("Slider").GetComponent<Slider>().interactable = false;
            Debug.Log(loadedsong.time[0]);
        }
    }

    public void justplay()
    {
        AudioSource audioSource = Camera.main.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        if (!Camera.main.gameObject.GetComponent<PDA>().in_practice)
        {
            audioSource.clip = Microphone.Start(microphones[0], true, miclooplength, 44100);
            Camera.main.gameObject.GetComponent<PDA>().clipforPDA = audioSource.clip;
            Camera.main.gameObject.GetComponent<AudioSource>().clip = null;
            while (!(Microphone.GetPosition(microphones[0]) > 0))
            {
                audioSource.Play();
            }
            GameObject.Find("drag_cursor").transform.localPosition -= new Vector3(GameObject.Find("drag_cursor").transform.localPosition.x, 0, 0);
            Camera.main.gameObject.GetComponent<PDA>().in_practice = true;
            Camera.main.gameObject.transform.GetChild(0).gameObject.AddComponent<check_mistake>();
            Debug.Log("added check_mistake: ");
            GameObject.Find("timescale").GetComponent<populate_grid>().displaytimescale(80f, 10);
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = pause_icon;
        }

        else
        {
            DateTime localDate = DateTime.Now;
            SavWav.save("practice"+localDate.ToString().Replace('/', '_').Replace(':', '_'), SavWav.TrimSilence(audioSource.clip, 0.00001f));
            Microphone.End(Microphone.devices[0]);
            Camera.main.gameObject.transform.GetChild(0).gameObject.GetComponent<AudioSource>().clip = null;
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = record;
            Microphone.End(microphones[0]);
            Camera.main.gameObject.GetComponent<PDA>().in_practice = false;
        }
    }
}
