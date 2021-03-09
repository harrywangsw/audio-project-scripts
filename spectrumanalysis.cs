using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

[RequireComponent(typeof(AudioSource))]


public class spectrumanalysis : MonoBehaviour
{

    public float[,] duration = new float[888, 2];
    public string[] notes;
    int bin;
    float max, loudest, secondloud = 0.0f;
    AudioSource flutescale;
    int count = 1;

    void Start()
    {
        notes = new string[888];
        string[] x;
        for(int i=0; i<888; i++)
        {
            notes[i] = "hi";
        }
        flutescale = GameObject.Find("flutescale").GetComponent<AudioSource>();

        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info)
        {
            x = f.Name.Split('.');
            GameObject b = GameObject.Find("Saved Songs ("+count.ToString()+")");
            b.transform.GetChild(0).gameObject.GetComponent<Text>().text = x[0];
            count++;
        }
    }



    void Update()
    {
        float[] spectrum = new float[4096];

        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        GameObject.Find("flutescale").GetComponent<preprocessspectrum>().analysespectrum(spectrum, flutescale.time, 1.0f);
    }


    

    public string identifynote(float frequency)
    {
        int power = Mathf.RoundToInt(Mathf.Log((frequency / 440.0f), 1.059463094359f));
        string note;
        int octave;
        switch(power % 12)
        {
            case 0:
                note = "A natural";
                break;
            case 1:
                note = "B flat";
                break;

            case 2:
                note = "B natural";
                break;
            case 3:
                note = "C natural";
                break;
            case 4:
                note = "D flat";
                break;
            case 5:
                note = "D natural";
                break;
            case 6:
                note = "E flat";
                break;
            case 7:
                note = "E natural";
                break;
            case 8:
                note = "F natural";
                break;
            case 9:
                note = "G flat";
                break;
            case 10:
                note = "G natural";
                break;
            case 11:
                note = "A flat";
                break;
            default:
                note = "???? no frakking idea";
                return note;
                break;
        }
        octave = 4 + (power / 12);
        return note + "_" + octave.ToString();
    }
}
