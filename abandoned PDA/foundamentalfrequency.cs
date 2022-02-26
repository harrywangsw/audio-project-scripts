using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
[RequireComponent(typeof(AudioSource))]

public class foundamentalfrequency : MonoBehaviour
{
    float[] spectrum = new float[4096];
    float[] loudness_time = new float[18], loudnessduration = new float[29];
    float[,] mistakes = new float[88, 2];
    float dipduration, loudestindex = 0f, previousdip = 0f, previousdiptime, previoustime, herztperbin, F0canidate, foundamental, harmonics, highestpeak, time, currentloudness, previousloudness, loudness, loudness_slope, loudnessaverage, maxloudnessratio, starttime;
    int mid, i, j, k, averagecount = 1, notecount = 1, changecount = 0, mistakecount = 0;
    string[] previousnote = new string[2], currentnote = new string[2], trdnotedown;
    float[] slope_peak = new float[2];
    bool playingstarted = false;
    Vector3 lastpoint = Vector3.zero;
    List<string> noteset = new List<string>();
    public List<string> notes = new List<string>();
    List<float> previousnotechange = new List<float>();
    public float maxallowederror;
    public float[,] duration = new float[888, 2];
    AudioSource playermic;

    void Start()
    {      
        playermic = gameObject.GetComponent<AudioSource>();
        herztperbin = 24000f / 2048f;
        starttime = 0f;
        mid = (loudness_time.Length - 1) / 2;
        generatesonglist();
    }

    public void generatesonglist()
    {
        string[] x;
        int i = 1;
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] info = dir.GetFiles("*.sa");
        foreach (FileInfo f in info)
        {
            x = f.Name.Split('.');
            GameObject b = GameObject.Instantiate(GameObject.Find("Saved Songs"), GameObject.Find("Content").transform);
            b.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            b.transform.GetChild(0).gameObject.GetComponent<Text>().text = x[0];
        }
    }

    void Update()
    {
        time = playermic.time;
        if (time == previoustime)
        {
            return;
        }
        var oldArray = loudnessduration;
        Array.Copy(oldArray, 1, loudnessduration, 0, oldArray.Length - 1);
        loudnessduration[loudnessduration.Length-1] = time-previoustime;
        //Debug.Log(time + " loudness duration: " + loudnessduration[loudnessduration.Length - 1]);

        int peakcount = 0, c = 0;
        float loudest = 0f;
        float[,] peaks = new float[18, 2];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
        for (i = 18; i<spectrum.Length; i++)
        {
            if (spectrum[i] > loudest)
            {
                loudest = spectrum[i];
                loudestindex = i * herztperbin;
            }
            for (j = 1; j <= 18; j++)
            {
                if (spectrum[i + j] < spectrum[i] && spectrum[i - j] < spectrum[i])
                {
                    c++;
                }
                else
                {
                    break;
                }
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

                if (peakcount >= 18) break;
            }
            c = 0;
        }
        /*if (time >= 0f&&time<=13.6f)
        {
            WriteString();
        }*/


        float[,] refinedpeaks = peaks.OrderBy(p => p[1]);
        float max = 0f;
        loudness = 0f;
        int maxindex = 0;
        int firstnpartial = 8;
        for (i = (peaks.GetLength(0)-firstnpartial); i < peaks.GetLength(0); i++)
        {
            //Debug.Log(time + " refined peaks #" + i + ": " + refinedpeaks[i, 0] + " loudness " + refinedpeaks[i, 1]);
            loudness += refinedpeaks[i, 1];
        }
        //alternatepda(refinedpeaks);
        loudness /= firstnpartial;
        checkloudness();
        //writeloudness();
        if (loudness < 0.01f)
        {
            return;
        }
        Debug.Log(time + " loudness: "+loudness);
        //Debug.Log(time + " loudest: " + loudestindex + " maxindex: " + maxindex + " max ratio: " + max);
        max = 0f;
        //C:\Users\harry\Downloads\

        float[] weightedaverage = new float[Mathf.RoundToInt(loudestindex / 27.5f) + 1];
        float weighing = 0f, weightedsum = 0f, peaksum = 0f, loudnessweighing = 0f;
        int maxharmonics = 1;
        int count = 0;
        //
        //
        //
        //
        //
        //
        //
        //Another idea for fundamental frequency: do the below steps for the loudest and second loudest partials and compare the F0canidate they
        //each produced
        for (i = 1; i <= Mathf.RoundToInt(loudestindex / 27.5f); i++)
        {
            F0canidate = loudestindex / i;
            for (j = 2; j < refinedpeaks.GetLength(0); j++)
            {
                harmonics = F0canidate * j;
                for (k = maxindex; k < refinedpeaks.GetLength(0); k++)
                {
                    peaksum += refinedpeaks[k, 0] * refinedpeaks[k, 1];
                    loudnessweighing += refinedpeaks[k, 1];
                    if (Mathf.Abs((refinedpeaks[k, 0] / harmonics) - 1) < 0.08f)
                    {
                        c = j;
                        count++;
                        //Debug.Log(time + " the " + j + "th harmonics of " + identifynote(F0canidate, false) + " is a peak!");
                        if (j <= 4)
                        {
                            weightedsum += refinedpeaks[k, 1];
                        }
                        else
                        {
                            weightedsum += refinedpeaks[k, 1] * weirdweighing(j);
                        }
                        break;

                    }
                }
            }

            for (j = 1; j <= c; j++)
            {
                if (j <= 4)
                {
                    weighing += 1;
                }
                else
                {
                    weighing += weirdweighing(j);
                }
            }
            weightedaverage[i] = weightedsum / weighing;
            if (weightedaverage[i] > max)
            {
                max = weightedaverage[i];
                foundamental = F0canidate;
            }
            if (count > 0)
            {
                //Debug.Log(time + " Foudamental canidate: " + identifynote(F0canidate, false) + "number of harmonics: " + count + " max harmonics: " + c + " weighted average: " + weightedaverage[i]);
            }
            weightedsum = 0f;
            count = 0;
            c = 0;
            weighing = 0f;
            maxharmonics = 1;
        }

        for (j = 1; j < refinedpeaks.GetLength(0); j++)
        {
            harmonics = foundamental * j;
            for (k = 0; k < refinedpeaks.GetLength(0); k++)
            {
                if (Mathf.Abs((refinedpeaks[k, 0] / harmonics) - 1) < 0.08f)
                {
                    c = j;
                    //Debug.Log(time + " while refining, the " + j + "th harmonics of " + identifynote(foundamental, false) + " is a peak!");
                    if (j <= 4)
                    {
                        weightedsum += refinedpeaks[k, 1] * refinedpeaks[k, 0] / j;
                    }
                    else
                    {
                        weightedsum += weirdweighing(j) * refinedpeaks[k, 1] * refinedpeaks[k, 0] / j;
                    }
                    break;
                }
            }
        }
        //Debug.Log(time + " while refining, the Foudamental canidate: " + identifynote(foundamental, false) + " harmonics count: " + c + "weightedsum: " + weightedsum);
        for (j = 1; j <= c; j++)
        {
            harmonics = foundamental * j;
            for (k = 0; k < refinedpeaks.GetLength(0); k++)
            {
                if (Mathf.Abs((refinedpeaks[k, 0] / harmonics) - 1) < 0.08f)
                {
                    if (j <= 4)
                    {
                        weighing += refinedpeaks[k, 1];
                    }
                    else
                    {
                        weighing += refinedpeaks[k, 1] * weirdweighing(j);
                    }
                }
            }

        }
        //foundamental = weightedsum / weighing;

        Debug.Log(time + " base note before refine: " + identifynote((foundamental), false) + " " + foundamental);

        currentnote = identifynote((foundamental), false).Split('_');
        if (currentnote[0] != previousnote[0])
        {
            previousnotechange.Add((previoustime+time)/2f);
            noteset.Add(currentnote[0]);
            validatechanging();
            notecount++;
        }
        trdnotedown = previousnote;
        previousnote = currentnote;
        previoustime = time;

        if (!playermic.isPlaying)
        {
            duration[0, 0] = 0f;
            data songdata = new data();
            songdata.notes = notes;
            //songdata.duration = duration;
            saveanalysis.SavePlayer(songdata, playermic.clip.name);
        }
    }

    void alternatepda(float[,] refinedpeaks)
    {
        Debug.Log("loudest index: " + loudestindex);
        float[] c = new float[Mathf.RoundToInt(loudestindex / 27.5f)], X = new float[Mathf.RoundToInt(loudestindex / 27.5f)];
        float[,] H = new float[c.Length, 18];
        int[] maxj = new int[c.Length];
        int i, j, k;
        float weighing = 0f, max = 0f, foundamental = 0f;
        for(i = 1; i < Mathf.RoundToInt(loudestindex / 27.5f); i++)
        {
            c[i] = loudestindex / i;
        }
        for(i = 1; i < c.Length; i++)
        {
            for (j = 1; j < 18; j++) {
                for(k = 0; k<refinedpeaks.GetLength(0); k++)
                {
                    if (refinedpeaks[k, 0] / (j * c[i]) > 0.9f && refinedpeaks[k, 0] / (j * c[i]) < 1.1f)
                    {
                        maxj[i] = j;
                        H[i, j] = refinedpeaks[k, 1];
                    }
                    else
                    {
                        H[i, j] = 0f;
                    }
                }
            }
        }
        for (i = 0; i < c.Length; i++)
        {
            for (j = 1; j <= maxj[i]; j++)
            {
                X[i] += H[i, j] * weirdweighing(j);
                weighing += weirdweighing(j);
            }
            X[i] /= weighing;
            Debug.Log(time + " the canidate: " + identifynote(c[i], false) + " has weighted average: " + X[i]+" and maxj is: "+maxj[i]);
            if (X[i] > max)
            {
                foundamental = c[i];
                max = X[i];
            }
        }
        Debug.Log(time + " please let this be right: " + identifynote(foundamental, false));
    }

    void validatechanging()
    {
        if (notecount > 0)
        {
            //Debug.Log(time + " previous note-switch: " + previousnotechange[notecount - 1]);
            if (time - previousnotechange[notecount - 1] < 0.08f && currentnote[0] == trdnotedown[0])
            {
                //Debug.Log(time + " a note switch was discredited");
                previousnotechange[notecount] = 0f;
                previousnotechange[notecount - 1] = 0f;
                noteset.RemoveAt(noteset.Count - 1);
                notecount--;
            }
        }
    }

    void checkloudness()
    {
        int i;
        var oldArray = loudness_time;
        int left = 0, right = 0;
        float rslope = 0f, lslope = 0f;
        Array.Copy(oldArray, 1, loudness_time, 0, oldArray.Length - 1);

        loudness_time[loudness_time.Length-1] = loudness;
        for (i = 1; i <= mid; i++)
        {
            if (loudness_time[mid - i] > loudness_time[mid])
            {
                left++;
                if ((loudness_time[mid - i] > loudness_time[mid - i + 1]))
                {
                    lslope += (loudness_time[mid - i] - loudness_time[mid - i + 1]) / loudnessduration[mid - i + 1];
                }
            }
            else
            {
                break;
            }
        }
        for (i = 1; i <= mid; i++) 
        {
            dipduration += loudnessduration[mid + i];
            if (loudness_time[mid + i]>loudness_time[mid])
            {
                right++;
                if ((loudness_time[mid + i] > loudness_time[mid + i - 1]))
                {
                    rslope += (loudness_time[mid + i] - loudness_time[mid + i - 1]) / loudnessduration[mid+i];
                }
            }
        }
        //Debug.Log(time + " aaaaaa "+loudness);

        Debug.Log(time + " at the dip time: " + (time - dipduration) + " the max slope: " + Math.Max(lslope, rslope)+" The right dip length: "+ right + " The left dip length: " + left+" loudness at the dip: "+loudness_time[mid]);
        if (Math.Max(right, left) == mid && Math.Min(right, left) > 1 && Math.Max(lslope, rslope) >= 0.5f)
        {
            for (i = 0; i < previousnotechange.Count; i++)
            {
                Debug.Log(time + " previous note swicth: " + previousnotechange[i]+" corresponding dip time: "+ (time - dipduration));
                if (Math.Abs((time - dipduration) - previousnotechange[i]) < 0.05f)
                {
                    Debug.Log((time - dipduration) + " note change!");

                    UI();

                    mistaketype(time-dipduration);
                    changecount++;
                    notecount = 0;
                }
                //writeloudness();
            }
        }
        dipduration = 0f;
    }

    void mistaketype(float changetime)
    {
        int i;
        string correctnote = notes[changecount];
        //check if current time is within the allowed time span
        if (duration[changecount, 0]-maxallowederror<changetime && duration[changecount, 0] + maxallowederror>changetime)
        {

        }
        else
        {
            setmistake(1);
            Debug.Log(time + " not on time!"+"changetime: "+changetime+" correct: "+duration[changecount, 0]);
        }
        if(currentnote[0] != correctnote)
        {
            setmistake(2);
            Debug.Log(time + "wrong note!");
        }

        return;
    }

    void setmistake(int type)
    {
        mistakes[mistakecount, 1] = type;
        mistakes[mistakecount, 0] = playermic.time;
        mistakecount++;
        //do UI stuff here
        mistakedata mistake = new mistakedata();
        mistake.mistake_time_type = mistakes;
        saveanalysis.SaveMistake(mistake, GameObject.Find("Saved Songs").transform.GetChild(0).gameObject.GetComponent<Text>().text);
    }

    void UI()
    {
        int i;

        var groups = noteset.GroupBy(v => v);
        int maxCount = groups.Max(g => g.Count());
        string mode = groups.First(g => g.Count() == maxCount).Key;
        Debug.Log(time+" asda: " + noteset[0]);

        notes.Add(mode);
        //GameObject.Find("timescale").GetComponent<populate_grid>().displaynote(mode, (time - dipduration));
        noteset = new List<string>();


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
        //Debug.Log("power: " + power + " frequency: " + frequency + " time: " + time);

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
    float weirdweighing(int harmonics)
    {
        if (harmonics <= 4)
        {
            return 1;
        }
        else
        {
            return ((float)Mathf.Log((harmonics * (float)Math.Sqrt((harmonics + 1) / (double)harmonics)), 1.2599210498948731647672106072782f) - (float)Mathf.Log(((harmonics - 1) * (float)Math.Sqrt((double)harmonics / (harmonics - 1))), 1.2599210498948731647672106072782f));
        }
    }

    /*[MenuItem("Tools/Write file")]
    void WriteString()
    {
        string path = "Assets/test/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
       
        writer.WriteLine(time.ToString() + "\n");
        float bin = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            bin = i * herztperbin;
            writer.WriteLine(bin.ToString()+","+spectrum[i].ToString("F8")+"\n");
        }
        writer.WriteLine("\n" + "\n");
        writer.Close();
    }
    void writeloudness()
    {
        string path = "Assets/test/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine((time).ToString() + "," + (loudness).ToString());
        writer.Close();
    }*/

}

public static class MultiDimensionalArrayExtensions
{
    public static T[,] OrderBy<T>(this T[,] source, Func<T[], T> keySelector)
    {
        return source.ConvertToSingleDimension().OrderBy(keySelector).ConvertToMultiDimensional();
    }
    private static IEnumerable<T[]> ConvertToSingleDimension<T>(this T[,] source)
    {
        T[] arRow;

        for (int row = 0; row < source.GetLength(0); ++row)
        {
            arRow = new T[source.GetLength(1)];

            for (int col = 0; col < source.GetLength(1); ++col)
                arRow[col] = source[row, col];

            yield return arRow;
        }
    }
    private static T[,] ConvertToMultiDimensional<T>(this IEnumerable<T[]> source)
    {
        T[,] twoDimensional;
        T[][] arrayOfArray;
        int numberofColumns;

        arrayOfArray = source.ToArray();
        numberofColumns = (arrayOfArray.Length > 0) ? arrayOfArray[0].Length : 0;
        twoDimensional = new T[arrayOfArray.Length, numberofColumns];

        for (int row = 0; row < arrayOfArray.GetLength(0); ++row)
            for (int col = 0; col < numberofColumns; ++col)
                twoDimensional[row, col] = arrayOfArray[row][col];

        return twoDimensional;
    }
}
