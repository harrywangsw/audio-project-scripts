using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadsong : MonoBehaviour
{
    data datatobeloaded;
    int i;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void load()
    {
        string songname = gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text;
        datatobeloaded = saveanalysis.LoadPlayer(songname);
        /*Camera.main.gameObject.GetComponent<spectrumanalysis>().notes = datatobeloaded.notes;
        Camera.main.gameObject.GetComponent<spectrumanalysis>().duration = datatobeloaded.duration;
        Camera.main.gameObject.GetComponent<spectrumanalysis>().songname = songname;
        Camera.main.gameObject.GetComponent<spectrumanalysis>().correctdynamics = datatobeloaded.correctdynamics;
        GameObject.Find("start").transform.localScale = new Vector3(1, 1, 1);*/
        for(i=0; i<datatobeloaded.notes.Count; i++)
        {
            GameObject.Find("timescale").GetComponent<populate_grid>().displaynote(datatobeloaded.notes[i], datatobeloaded.duration[i, 1]);
        }

        Debug.Log("song loaded!!!");
    }
}
