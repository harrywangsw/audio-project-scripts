using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class check_mistake : MonoBehaviour
{
    GameObject timescale;
    public float maxallowederror = 0.1f, maxdynamicserror, samplelength = 0.2f, scheduledcheck;
    public float[,] mistakes = new float[888, 2];
    public float sectionlength = 1f, time, truetime = 0f;
    int changecount = 0, mistakecount = 0, i, j, previousindex = 0, seccount = 1, currentnoteindex = 0;
    AudioClip clip;
    public data loaded, recorded = new data(), sectionofloaded = new data();
    AudioSource audioSource;

    void Start()
    {
        timescale = GameObject.Find("timescale");
        audioSource = gameObject.GetComponent<AudioSource>();
        clip = audioSource.clip;
        scheduledcheck = clip.length;
        for(i = 0; i<mistakes.GetLength(0); i++)
        {
            if (mistakes[i, 0] == 0f && mistakes[i, 1] == 0f) mistakecount = i;
        }
    }

    void Update()
    {
        //time is the time of the microphone, truetime is the time of the frame, true time always lags behind microhphone's time
        time = audioSource.time;
        truetime += Time.deltaTime;
        float[] samples = new float[getIndexFromTime(2f*samplelength) * clip.channels];
        if (Camera.main.GetComponent<AudioSource>().clip == null) { justdisplaynote(); return; }
        if (changecount >= loaded.notes.Count)
        {
            Microphone.End(Microphone.devices[0]);
            organize_mistakes(mistakes);
            mistakedata mistake = new mistakedata();
            mistake.mistake_time_type = mistakes;
            saveanalysis.SaveMistake(mistake, Camera.main.gameObject.GetComponent<AudioSource>().clip.name);
        }

        // if time is "samplelength" seconds away from the current note change, do PDA and check for mistakes
        if (time >= loaded.time[changecount]+samplelength || time >= scheduledcheck+samplelength) 
        {
            clip.GetData(samples, getIndexFromTime(time-2f*samplelength));
            transform.parent.gameObject.GetComponent<PDA>().main(samples, recorded, false, time);
            for (i = 0; i < recorded.notes.Count; i++)if (i < recorded.notes.Count - 1) Debug.Log(((time - 2f * samplelength) + recorded.time[i]).ToString() + " recorded notes: " + recorded.notes[i]);
            for(i=0; i<loaded.time.Count; i++)
            {
                if (loaded.time[i] < loaded.time[changecount] + 0.5f && loaded.time[i] > loaded.time[changecount] - 0.5f)
                {
                    sectionofloaded.time.Add(loaded.time[i]);
                    sectionofloaded.notes.Add(loaded.notes[i]);
                }
                else if (loaded.time[i] > loaded.time[changecount] + 0.5f) break;
            }

            rhythm(loaded.time[changecount], loaded.notes[changecount]);
            if (loaded.notes[changecount + 1] == "rest") changecount += 2;
            else changecount++;
            //if the current note is very long, schedule a check halfway through for dynamics and tuning
            scheduledcheck = clip.length;
            if(loaded.time[changecount]-time > 2f)
            {
                scheduledcheck = time + (loaded.time[changecount] - time)/2;
            }
            recorded = new data();
        }      
    }

    //reads 1 second of samples starting from time-1 (read the latest second of the recording) and do PDA on the sample
    void justdisplaynote()
    {
        if (time <= (float)seccount) return;
        float[] samples = new float[getIndexFromTime(1)];
        clip.GetData(samples, getIndexFromTime(time - 1f));
        transform.parent.gameObject.GetComponent<PDA>().main(samples, recorded, false, time);
        for (i = 0; i < recorded.notes.Count-1; i++)
        {
            Debug.Log(((time) + recorded.time[i]).ToString() + " recorded notes: " + recorded.notes[i]);
            if (i > 0) { timescale.GetComponent<populate_grid>().displaynote(recorded.notes[i], time + recorded.time[i], recorded.time[i + 1] - recorded.time[i]);}
            //else if (recorded.notes.Count == 1) timescale.GetComponent<populate_grid>().displaynote(recorded.notes[i], (float)Math.Round(time), 1f);
            //else timescale.GetComponent<populate_grid>().displaynote(recorded.notes[i], truetime, recorded.time[i]);
        }
        recorded = new data();
        seccount++;
    }

    // if 2 mistakes has the same time and is the same type, 
    void organize_mistakes(float[,] mistakes)
    {
        float[,] organized_mistakes = new float[mistakes.GetLength(0), 3];
        int i, j, count = 0;
        for(i = 0; i<mistakes.GetLength(0) && mistakes[i, 0] != 0f; i++)
        {
            for(j=0; j<mistakes.Length && j != i; j++)
            {
                if (mistakes[i, 0] == mistakes[j, 0] && mistakes[i, 1] == mistakes[j, 1]) count++; mistakes[j, 0] = 0f; mistakes[j, 1] = 0f;
            }
            organized_mistakes[i, 0] = mistakes[i, 0];
            organized_mistakes[i, 1] = mistakes[i, 1];
            organized_mistakes[i, 2] = count;
            count = 0;
        }
    }

    public void rhythm(float changetime, string fstnote)
    {
        int i;
        
        //idea: get every sequence of every possible length from recorded.notes (which accounts for added and/or ommitted notes) and search section of loaded for that sequence
        int[] sequenceindex = new int[recorded.time.Count];
        for(i=0; i<recorded.time.Count; i++)    sequenceindex[i] = i;
        for (i = recorded.time.Count; i>0; i--) printCombination(sequenceindex, recorded.time.Count, i);

        //check if the current section includes the 2 notes
        for (i = 0; i < recorded.time.Count; i++)
        {
            if (recorded.notes[i] == fstnote)
            {
                Debug.Log(loaded.time[changecount] + " " + loaded.notes[changecount] + " should be a note");
                Color alpha = timescale.transform.GetChild(1).gameObject.transform.GetChild(changecount).gameObject.GetComponent<Image>().color;
                alpha.a = 1f;
                timescale.transform.GetChild(1).gameObject.transform.GetChild(changecount).gameObject.GetComponent<Image>().color = alpha;
                if (Camera.main.gameObject.GetComponent<PDA>().tuningerror[i]) setmistake(7, loaded.time[changecount]);
            }
            else if (Math.Abs(((time - 2f * samplelength) + recorded.time[i]) - loaded.time[changecount]) < maxallowederror && recorded.notes[i] != "rest")
            {
                Debug.Log(loaded.time[changecount] + " " + loaded.notes[changecount] + " correct time, wrong note");
                Color alpha = timescale.transform.GetChild(1).gameObject.transform.GetChild(changecount).gameObject.GetComponent<Image>().color;
                alpha.a = 1f;
                timescale.transform.GetChild(1).gameObject.transform.GetChild(changecount).gameObject.GetComponent<Image>().color = alpha;
            }
        }
    }

    public void setmistake(int type, float mistaketime)
    {
        mistakes[mistakecount, 1] = type;
        mistakes[mistakecount, 0] = mistaketime;
        mistakecount++;

        timescale.GetComponent<populate_grid>().displaymistake(type, changecount);
    }

    public int getIndexFromTime(float curTime)
    {
        float lengthPerSample = clip.length / (float)clip.samples;

        return Mathf.FloorToInt(curTime / lengthPerSample);
    }

    int searchforsequence(int[] sequence)
    {
        int i, j;
        for (i = 0; i < sequence.Length; i++)
        {
            int c = 0;
            for (j = 0; j < sectionofloaded.notes.Count-sequence.Length; j++) 
            {
                if (recorded.notes[sequence[i]] == sectionofloaded.notes[j + i]) c++;
                else break;
            }
            if (c >= sequence.Length) return j;
        }
        return -1;
    }

    List<List<int>> returnarray;
    public void combinationUtil(int[] arr, int n, int r, int index, int[] data, int i)
    {
        /* arr[] ---> Input Array
    data[] ---> Temporary array to store
    current combination start & end --->
    Staring and Ending indexes in arr[]
    index ---> Current index in data[]
    r ---> Size of a combination to be
    printed */

        // Current combination is ready to
        // be printed, print it
        if (index == r)
        {
            //for (int j = 0; j < r; j++) Debug.Log(data[j] + " ");
            int startingindexofmatch = searchforsequence(data);
            if (startingindexofmatch == -1)
            {
                //Debug.Log(time+" no match found");
                if (r == 1 && i == arr.Length) Debug.Log(loaded.time[changecount] + " "+loaded.notes[changecount]+" no sequence has match in section of loaded");
                return;
            }
            else if (startingindexofmatch > changecount) Debug.Log(loaded.time[changecount] + " " + loaded.notes[changecount]+" match is after current note's index, so you played the match note early");
            else if (startingindexofmatch < changecount) Debug.Log(loaded.time[changecount] + " " + loaded.notes[changecount]+" match is before current note's index, so u played the match note late");
            for(j=1; j<data.Length; j++)
            {
                if (data[j] > data[j - 1] + 1) Debug.Log(loaded.time[changecount] + " " + loaded.notes[changecount]+" you added a note or two. Here is the last note you added: " + recorded.notes[data[j] - 1]);
            }
            return;
        }

        // When no more elements are there
        // to put in data[]
        if (i >= n) return;

        // current is included, put next
        // at next location
        data[index] = arr[i];
        combinationUtil(arr, n, r, index + 1,
                                data, i + 1);

        // current is excluded, replace
        // it with next (Note that i+1
        // is passed, but index is not
        // changed)
        combinationUtil(arr, n, r, index,
                                data, i + 1);
    }

    // The main function that prints all
    // combinations of size r in arr[] of
    // size n. This function mainly uses
    // combinationUtil()
    public void printCombination(int[] arr, int n, int r)
    {

        // A temporary array to store all
        // combination one by one
        int[] data = new int[r];

        // Print all combination using
        // temporary array 'data[]'
        combinationUtil(arr, n, r, 0, data, 0);
    }
}