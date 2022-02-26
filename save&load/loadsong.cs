using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class loadsong : MonoBehaviour
{
    public GameObject line, timescale, start;
    public data datatobeloaded;
    public mistakedata mistakestobeloaded;
    float secondsperunit;
    int i, j;
    public string songname;
    public Transform[] children;
    bool zoom = false;
    


    void Start()
    {
        timescale = GameObject.Find("timescale");
        children = gameObject.transform.parent.gameObject.GetComponentsInChildren<Transform>();
        start = GameObject.Find("start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void load()
    {
        songname = gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text;
        datatobeloaded = saveanalysis.LoadPlayer(songname);
        mistakestobeloaded = new mistakedata();
        mistakestobeloaded = saveanalysis.loadmistake(songname);

        if (datatobeloaded.time.Count == 0)
        {
            Debug.Log("there's no data in this");
            return;
        }
        foreach (Transform button in children)
        {
            if (gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text != songname) { Destroy(button.gameObject); }
        }

        //draw the time scale
        GameObject.Find("timescale").GetComponent<populate_grid>().displaytimescale(datatobeloaded.time[datatobeloaded.notes.Count - 1] + 1f, 10);

        //fill the time scale with notes after destroying the previous notes
        foreach (Transform child in timescale.transform.GetChild(0))
        {
            if(child.gameObject.name!= "drag_cursor") Destroy(child.gameObject);
        }

        Camera.main.gameObject.GetComponent<PDA>().lengthofindex = datatobeloaded.time[datatobeloaded.time.Count - 1] / datatobeloaded.dynamics.Count;
        for (i = 0; i < datatobeloaded.notes.Count; i++)
        {
            string notesymbol = null;
            switch (datatobeloaded.notes[i][2])
            {
                case 'f':
                    notesymbol = Char.ToLower(datatobeloaded.notes[i][0]).ToString() + '\u0048'+ datatobeloaded.notes[i][datatobeloaded.notes[i].Length - 1].ToString();
                    break;
                case 'n':
                    notesymbol = Char.ToLower(datatobeloaded.notes[i][0]).ToString() + '\u004A'+ datatobeloaded.notes[i][datatobeloaded.notes[i].Length-1].ToString();
                    break;
                default:
                    notesymbol = datatobeloaded.notes[i];
                    break;
            }

            if (i < datatobeloaded.notes.Count - 1)
            {
                timescale.GetComponent<populate_grid>().displaynote(notesymbol, datatobeloaded.time[i], datatobeloaded.time[i + 1] - datatobeloaded.time[i]);
                //if (datatobeloaded.time[i + 1] - datatobeloaded.time[i] < 0.3f) zoom = true;
            }
            //if i is the last note, display it for 3 seconds
            else timescale.GetComponent<populate_grid>().displaynote(notesymbol, datatobeloaded.time[i], 3f);
            Debug.Log(datatobeloaded.dynamics.Count);
            Camera.main.gameObject.GetComponent<PDA>().dynamics(i, datatobeloaded);
            Debug.Log(datatobeloaded.time[i] + " notes loaded: " + datatobeloaded.notes[i]);
        }
        if (zoom)
        {
            if(!timescale.GetComponent<populate_grid>().zoomed) timescale.GetComponent<populate_grid>().zoom();
            zoom = false;
        }

        if(!File.Exists(Application.persistentDataPath + "/" + "audio_files" + "/" + songname+".wav")) GameObject.Find("pop-up").GetComponent<pop_up>().editor("I can't find the audio file for this song in whereever the audio files are saved. Would you like to point me to a new location for the song?", "Yes (open file browser)", "No, (You won't be able to listen to the song)", null, GameObject.Find("add_song").GetComponent<CanvasSampleOpenFileText>().openbrowser, GameObject.Find("pop-up").GetComponent<pop_up>().closepopup);
        else StartCoroutine(GameObject.Find("add_song").GetComponent<CanvasSampleOpenFileText>().OutputRoutine(Application.persistentDataPath + "/" + "audio_files" + "/" + songname+".wav"));

        if (mistakestobeloaded != null) loadmistakes();
        else GameObject.Find("block the default selection").transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "you have no mistake records for this song";


        start.GetComponent<start_practice_session>().loadedsong = datatobeloaded;
        start.GetComponent<start_practice_session>().miclooplength = (int)Math.Ceiling(datatobeloaded.time[datatobeloaded.time.Count-1]);
        start.GetComponent<Button>().interactable = true;
        start.GetComponent<Button>().onClick.AddListener(delegate { start.GetComponent<start_practice_session>().startpractice(); });
        start.transform.GetChild(1).gameObject.GetComponent<Text>().color = Color.blue;
        start.transform.GetChild(1).gameObject.GetComponent<Text>().text = "start practicing!";
        GameObject.Find("Slider").GetComponent<Slider>().interactable = true;
        if (datatobeloaded.tempo != 0)
        {
            GameObject.Find("Slider").transform.GetChild(3).GetChild(1).gameObject.GetComponent<Text>().text = "Current tempo: " + datatobeloaded.tempo.ToString();
            GameObject.Find("Slider").GetComponent<chang_tempo>().tempo = datatobeloaded.tempo;
            GameObject.Find("change time scale").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("Slider").transform.GetChild(4).GetChild(0).gameObject.GetComponent<Text>().text = "Current tempo: unknown";
            GameObject.Find("pop-up").GetComponent<pop_up>().editor("You havn't set a tempo for this song yet. You can do so by drag-selecting a bar in the note scroll or type the bpm number in the inputfield beside the tempo slider", "ok", "never show this messages again", null, GameObject.Find("pop-up").GetComponent<pop_up>().closepopup, GameObject.Find("pop-up").GetComponent<pop_up>().closepopup);
            GameObject.Find("change time scale").GetComponent<Button>().interactable = false;
        }
        Debug.Log("song loaded!!!");
    }

    public void loadmistakes()
    {
        for (i = 0; i < mistakestobeloaded.mistake_time_type.GetLength(0); i++)
        {
             for (j = 0; j < datatobeloaded.time.Count; j++) if (mistakestobeloaded.mistake_time_type[i, 0] == datatobeloaded.time[j]) timescale.GetComponent<populate_grid>().displaymistake((int)mistakestobeloaded.mistake_time_type[i, 1], j); ;
        }
        start.GetComponent<start_practice_session>().loadedmistakes = mistakestobeloaded;
    }
}
