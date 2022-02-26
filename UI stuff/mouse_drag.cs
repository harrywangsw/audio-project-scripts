using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mouse_drag : MonoBehaviour
{
    Vector3 startpos;
    public float starttime = 0f, endtime = 0f, filledtime; 
    public float distance_to_screen, secondsperunit;
    AudioSource play_section, microphone;
    GameObject timescale;
    Text currenttime;

    void Start()
    {
        distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        play_section = Camera.main.GetComponent<AudioSource>();
        microphone = GameObject.Find("microphone").GetComponent<AudioSource>();
        timescale = GameObject.Find("timescale");
        currenttime = GameObject.Find("current_time").GetComponent<Text>();
    }

    void Update()
    {
        secondsperunit = timescale.GetComponent<populate_grid>().secondsperunit;
        // if user drag-selected a section of the audio file to play, then endtime != 0f
        if (endtime != 0f && play_section.time >= endtime)
        {
            play_section.Stop();
        }
        starttime = transform.parent.transform.localPosition.x/ secondsperunit;
        if (Camera.main.gameObject.GetComponent<PDA>().in_practice == true) 
        { 
            transform.parent.transform.localPosition += new Vector3((microphone.time-filledtime) * secondsperunit, 0, 0); 
            GameObject.Find("generic").GetComponent<Text>().text = (GameObject.Find("microphone").GetComponent<check_mistake>().truetime).ToString();
            filledtime = microphone.time;
        }
        
        currenttime.text = (microphone.time).ToString();
    }

    void OnMouseDrag()
    {
        Vector3 pos_move = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        transform.position = new Vector3(pos_move.x, transform.position.y, pos_move.z);
    }
    void OnMouseUp()
    {
        if (starttime != 0f)
        {
            endtime = (transform.parent.transform.localPosition.x+transform.localPosition.x) / secondsperunit;
            play_section.time = starttime;
            play_section.Play();
            GameObject.Find("settempo").GetComponent<Button>().interactable = true;
        }
    }

}
