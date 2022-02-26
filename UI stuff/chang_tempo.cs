using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class chang_tempo : MonoBehaviour
{
    Slider temposlider;
    public float previousvalue = 1f;
    int i;
    public bool settingtempo = false, usingseconds = true;
    public AudioMixer pitchBendGroup;
    Transform timescale;
    GameObject start;
    public int beats, tempo;
    void Start()
    {
        beats = 4;
        tempo = 240;
        start = GameObject.Find("start");
        timescale = GameObject.Find("timescale").transform;
        temposlider = GameObject.Find("Slider").GetComponent<Slider>();
        temposlider.onValueChanged.AddListener(delegate { changetempo(); });
        GameObject.Find("settempo").GetComponent<Button>().onClick.AddListener(delegate { settempo(); });
        GameObject.Find("tempo").GetComponent<InputField>().onEndEdit.AddListener(delegate { tempo = (int)Decimal.Parse(GameObject.Find("tempo").transform.GetChild(2).gameObject.GetComponent<Text>().text); start.GetComponent<start_practice_session>().loadedsong.tempo = tempo;saveanalysis.SavePlayer(start.GetComponent<start_practice_session>().loadedsong, Camera.main.gameObject.GetComponent<AudioSource>().clip.name);});
    }

    void Update()
    {
        if (settingtempo && GameObject.Find("drag_cursor_end").GetComponent<mouse_drag>().endtime!=0f)
        {
            tempo = Mathf.RoundToInt(beats*60f/(GameObject.Find("drag_cursor_end").GetComponent<mouse_drag>().endtime - GameObject.Find("drag_cursor_end").GetComponent<mouse_drag>().starttime));
            start.GetComponent<start_practice_session>().loadedsong.tempo = tempo;
            saveanalysis.SavePlayer(start.GetComponent<start_practice_session>().loadedsong, Camera.main.gameObject.GetComponent<AudioSource>().clip.name);
            transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = "current tempo: "+(tempo).ToString();
            settingtempo = false;
        }
    }

    public void changetempo()
    {
        for (i = 0; i < start.GetComponent<start_practice_session>().loadedsong.notes.Count; i++)
        {
            if (i > 0)
            {
                start.GetComponent<start_practice_session>().loadedsong.time[i] *= temposlider.value / previousvalue;
                timescale.GetChild(1).GetChild(i).localPosition = new Vector2(start.GetComponent<start_practice_session>().loadedsong.time[i]*timescale.gameObject.GetComponent<populate_grid>().secondsperunit, 1);
            }
            timescale.GetChild(0).GetChild(i).gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(temposlider.value/previousvalue, 1f);            
        }

        transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "current tempo: "+(Mathf.Round(temposlider.value *tempo)).ToString();
        Camera.main.gameObject.GetComponent<AudioSource>().pitch = temposlider.value;
        pitchBendGroup.SetFloat("pitch_bend", 1f / temposlider.value);
        previousvalue = temposlider.value;
    }

    public void editempo()
    {

    }

    public void changescale()
    {
        int childCount = 0, i;
        timescale.gameObject.GetComponent<populate_grid>().secondsperunit = 0f;
        if (usingseconds) { usingseconds = false; GameObject.Find("zoom_out").GetComponent<Button>().interactable = false; GameObject.Find("zoom_in").GetComponent<Button>().interactable = false;}
        else usingseconds = true;

        childCount = timescale.childCount-1;
        for(i = 1; i<timescale.childCount; i++)
        {
            Destroy(timescale.GetChild(i).gameObject);
        }
        //Debug.Log(((float)childCount / 600f) * (float)tempo+" "+childCount+" "+tempo);
        timescale.gameObject.GetComponent<HorizontalLayoutGroup>().spacing *= 10f / (float)beats;
        timescale.gameObject.GetComponent<populate_grid>().displaytimescale(((float)childCount/600f) * (float)tempo/(float)beats, beats);
    }

    public void settempo()
    {
        GameObject.Find("drag_cursor_end").GetComponent<SpriteRenderer>().color = Color.green;
        GameObject.Find("drag_cursor").GetComponent<SpriteRenderer>().color = Color.green;
        settingtempo = true;
    }
}
