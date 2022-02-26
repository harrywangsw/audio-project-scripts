using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class octave_correction : MonoBehaviour
{
    float[] spectrum = new float[4096];
    float herztperbin = 0f, time = 0f;
    int i, j;
    AudioSource playermic;

    void Start()
    {

        herztperbin = 24000f / 4096f;
    }
    void Update()
    {
        playermic = gameObject.GetComponent<AudioSource>();
        time = playermic.time;
        float loudestindex = 0f, loudest = 0f;
        int peakcount = 0, c = 0;
        float[,] peaks = new float[13, 2];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
        //WriteString(time.ToString() + '\n');
        for (i = 18; i < spectrum.Length; i++)
        {
            //WriteString((i * herztperbin).ToString() + ", " + spectrum[i].ToString() + '\n');
            if (spectrum[i] > loudest)
            {
                loudest = spectrum[i];
                loudestindex = i * herztperbin;
            }
            /*for (j = 1; j <= 18; j++)
            {
                if (spectrum[i + j] < spectrum[i] && spectrum[i - j] < spectrum[i]) c++;
                else break;
            }
            if (c >= 18)
            {
                peaks[peakcount, 0] = (float)i * herztperbin;
                //peaks[peakcount, 1] = 70f+20f*(float)Math.Log(spectrum[i], 10f);
                peaks[peakcount, 1] = spectrum[i];
                if (peaks[peakcount, 1] < 0f)
                {
                    peaks[peakcount, 1] = 0f;
                }
                Debug.Log(time + " peak #" + peakcount.ToString() + ": " + peaks[peakcount, 0].ToString() + " loudness " + peaks[peakcount, 1]);

                peakcount++;

                if (peakcount >= 13) break;
            }
            c = 0;*/
            Debug.Log(identifynote(loudestindex, false));
        }        
    }
    public double octavefromweightedaverage(float average, float basenote)
    {
        int power = Mathf.RoundToInt(Mathf.Log((basenote / 440.0f), 1.059463094359f));
        int i;
        double returnval = 0;
        float difference = 0f, mindifference = 8888f;
        for (i = 1; i <= 4; i++)
        {
            difference = Mathf.Abs(Mathf.Pow(1.059463094359f, power) * 440 * Mathf.Pow(2, i) - average);
            if (difference < mindifference)
            {
                mindifference = difference;
                returnval = 4 + Math.Ceiling((double)power / 12) + i;
            }
        }
        return returnval;
    }

    public string identifynote(float frequency, bool checktuning)
    {
        if (frequency == 0f)
        {
            Debug.Log("frequency is zero, bud");
            return null;
        }
        string note;
        double octave;
        int power = Mathf.RoundToInt(Mathf.Log((frequency / 440.0f), 1.059463094359f));


        octave = 4 + Math.Ceiling((double)power / 12);
        Debug.Log("power: " + power + " frequency: " + frequency + " time: " + time);

        if (power > 0)
        {

            switch (Mathf.Abs(power) % 12)
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
                    note = null;
                    return note;
                    break;
            }
        }
        else
        {
            switch (Mathf.Abs(power) % 12)
            {
                case 0:
                    note = "A natural";
                    break;
                case 11:
                    note = "B flat";
                    break;
                case 10:
                    note = "B natural";
                    break;
                case 9:
                    note = "C natural";
                    break;
                case 8:
                    note = "D flat";
                    break;
                case 7:
                    note = "D natural";
                    break;
                case 6:
                    note = "E flat";
                    break;
                case 5:
                    note = "E natural";
                    break;
                case 4:
                    note = "F natural";
                    break;
                case 3:
                    note = "G flat";
                    break;
                case 2:
                    note = "G natural";
                    break;
                case 1:
                    note = "A flat";
                    break;
                default:
                    note = null;
                    return note;
                    break;
            }
        }

        /*if (checktuning)
        {
            float tuning = Mathf.Abs(frequency - Mathf.Pow(1.059463094359f, power) * 440);
            if (tuning > maxallowedtuningerror)
            {
                setmistake(3);
                Debug.Log("out of tune");
            }
        */

        return note + "_" + octave.ToString();
    }

    void WriteString(string text)
    {
        string path = "Assets/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine(text);

        writer.Close();
    }
}
