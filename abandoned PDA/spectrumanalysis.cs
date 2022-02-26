using System;
using System.Linq;
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
    public float[,] mistakes, frequencytotal = new float[88, 2];
    public float[] correctdynamics, dynamics;
    public string[] notes;
    int secondloud, thrdloud, fourthloud, loudestindex = 0;
    float loudest = 0.0f;
    AudioSource playermic;
    int count, notecount = 0;
    public float mistake_time, starttime, maxallowederror, maxallowedtuningerror, maxdynamicserror, basefrequency = 0f;
    // 1 is rythem, 2 is dynamics, 3 is tuning, 4 is wrong note
    public int noteindex, totalnotes, songlength;
    public string mistake_detail, correctnote, previouscorrectnote, nextcorrectnote, previousnote, curnote, songname, basenote, mode;
    static float herztperbin = 24000f / 8192f;
    public bool playerstarted, notechanged, mistake;
    public float[] spectrum = new float[4096];
    double slope, rSquared, intercept;
    double[] yValues, xValues;

    void Start()
    {
        count = 1;
        notes = new string[888];
        playermic = gameObject.GetComponent<AudioSource>();
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
            GameObject b = GameObject.Find("Saved Songs (" + i.ToString() + ")");
            b.transform.position = new Vector3(b.transform.position.x, b.transform.position.y - i * 28.0f, b.transform.position.z);
            b.transform.GetChild(0).gameObject.GetComponent<Text>().text = x[0];

            i++;
        }
    }
    void Update()
    {
        int i = 1, j = 1;
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        //getbasenotefrompeaks(spectrum);       
        if (notes[1]!=null)
        {
            if (curnote == notes[1] && !playerstarted)
            {
                //assuming player plays the first note correctly
                playerstarted = true;
                starttime = playermic.time;
            }
        }
        //mistaketype(loudest);       
        previousnote = curnote;
        loudestindex = 0;
        loudest = 0f;
        secondloud = 0;
        thrdloud = 0;
        fourthloud = 0;
        notechanged = false;
    }

    /*public void getbasenotefrompeaks(float[] spectrum)
    {
        int i = 0, j = 0, c = 0, peakcount= 0, count = 0, most = 0, harmonics;
        float sum = 0f, loudnessweighing = 0f, average = 0f, min = 8888f;
        float[,] peaks = new float[29, 2];
        string[] differencesnotes = new string[19];
        int[] mostfrequentdifferences = new int[19];
        bool stop = false;
        for (i = 18; i * herztperbin / (48000 / playermic.clip.frequency) < 3000; i++)
        {

            if (spectrum[i] > loudest)
            {
                loudest = spectrum[i];
                loudestindex = i * herztperbin / (48000 / playermic.clip.frequency);
            }
            for (j = 1; j <= 2; j++)
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
            if (c >= 2)
            {
                peaks[peakcount, 0] = i * herztperbin / (48000 / playermic.clip.frequency);
                peaks[peakcount, 1] = spectrum[i];
                sum += peaks[peakcount, 0] * spectrum[i];
                loudnessweighing += spectrum[i];
                
                
                peakcount++;
                c = 0;    
                if (peakcount >= 29) break;
            }
        }
        
        Debug.Log("weighted average: " + sum / loudnessweighing+" at time: "+playermic.time+ " loudness average "+loudnessweighing/peakcount);

        float[] refinedpeaks = new float[peaks.Length/2];
        float[] refinedloudness = new float[peaks.Length / 2];
        float medium = 
        for(i=0; i < peakcount; i++)
        {
            if (peaks[i, 1] > medium)
            {
                refinedpeaks[c] = peaks[i, 0];
                refinedloudness[c] = peaks[i, 1];
                Debug.Log(playermic.time + " peak #" + c.ToString() + ": " + refinedpeaks[c].ToString() + " loudness " + peaks[i, 1]);
                if (c > 0)
                {
                    differencesnotes[c] = identifynote(refinedpeaks[c] - refinedpeaks[c - 1], false);
                }
                c++;
            }
        }

        refinedpeaks = refinedpeaks.Where(p => p != 0f).ToArray();
        refinedloudness = refinedloudness.Where(p => p != 0f).ToArray();
        yValues = new double[refinedpeaks.Length];
        xValues = new double[refinedpeaks.Length];
        for (i = 0; i < refinedpeaks.Length; i++) 
        {
            yValues[i] = i;
            xValues[i] = (double)Mathf.Pow(1.059463094359f, Mathf.RoundToInt(Mathf.Log((refinedpeaks[i] / 440.0f), 1.059463094359f))) * 440;
        }
        var groups = differencesnotes.GroupBy(v => v);
        foreach(var group in groups)
        {
            if (group.Key != null)
            {
                if (group.Count() > most)
                {
                    most = group.Count();
                    mode = group.Key;
                }
                //Debug.Log("the key: " + group.Key + "appears: " + group.Count() + "at time: " + playermic.time);
            }
        }
        LinearRegression(yValues, xValues, out rSquared, out intercept, out slope);

        float weightedaverage = 0f;
        for(i=1; i<=Mathf.RoundToInt(loudestindex/55); i++)
        {
            stop = false;
            basefrequency = loudestindex/i;
            for (j=0; j<refinedpeaks.Length; j++)
            {
                if (Mathf.Abs((refinedpeaks[j] / basefrequency) - (float)Mathf.RoundToInt(refinedpeaks[j] / basefrequency)) < 0.1)
                {
                    
                    harmonics = Mathf.RoundToInt(refinedpeaks[j] / basefrequency)
                    if (harmonics <= 4)
                    {
                        weightedaverage += refinedloudness;
                    }
                    else
                    {
                        weightedaverage += refinedloudness*Mathf.Log((harmonics*sqrt((harmonics+1)/harmonics), 1.2599210498948731647672106072782)
                    }
                    
                }
                else
                {
                    stop = true;
                    break;
                }
            }

            if (stop) continue;
            else
            {
                min = deviation;
                basenote = identifynote(basefrequency, false);
                Debug.Log(playermic.time + " basenote: " + basenote + " basefrequency " + basefrequency + " deviation: " + deviation);
            }
        }
        Debug.Log("basenote: "+basenote+" intercept: "+identifynote((float)intercept, false)+" slope: "+identifynote((float)slope, false)+" mode: " + most+ mode + " The base note is : " + identifynote(Mathf.RoundToInt(refinedpeaks[0]), false) + " the octave is: " + octavefromweightedaverage(sum / loudnessweighing, refinedpeaks[0]) + " at time: " + playermic.time);
        
    }*/




        
    public double octavefromweightedaverage(float average, float basenote)
    {
        int power = Mathf.RoundToInt(Mathf.Log((basenote / 440.0f), 1.059463094359f));
        int i;
        double returnval = 0;
        float difference = 0f, mindifference = 8888f;
        for(i=1; i<=4; i++)
        {
            difference = Mathf.Abs(Mathf.Pow(1.059463094359f, power)*440 * Mathf.Pow(2, i) - average);
            if (difference < mindifference)
            {
                mindifference = difference;
                returnval = 4+Math.Ceiling((double)power / 12)+i;
            }
        }
        return returnval;
    }

    public void mistaketype(float loudest)
    {
        int i;
        if (curnote != null && curnote != previousnote) notechanged = true;
        else notechanged = false;

        if (notechanged) {
            Debug.Log("note changed!!!!!!");
            if ((playermic.time - starttime) >= duration[notecount, 0] && (playermic.time - starttime) <= duration[notecount, 1])
            {
                correctnote = notes[notecount];

                if (Mathf.Abs(loudest - correctdynamics[(int)(playermic.time - starttime) / 2048]) > maxdynamicserror)
                {
                    setmistake(2);
                    Debug.Log("dynamics wrong");
                }

                //check if current time is within the allowed time span
                if ((playermic.time - starttime) + maxallowederror > duration[notecount+1, 0])
                {
                    if (curnote != correctnote)
                    {
                        setmistake(4);
                        Debug.Log("wrong note, buddy");
                    }
                }
                else
                {
                    setmistake(1);
                    Debug.Log("early");
                    if (curnote != correctnote)
                    {
                        setmistake(4);
                        Debug.Log("wrong note, buddy");
                    }
                        
                }

                if ((playermic.time - starttime) - maxallowederror < duration[notecount, 0])
                {
                    if (curnote != correctnote)
                    {
                        setmistake(4);
                        Debug.Log("wrong note, buddy");
                    }
                }
                else
                {
                    setmistake(1);
                    Debug.Log("late");
                    if (curnote != correctnote)
                    {
                        setmistake(4);
                    }
                        
                }
                notecount++;
            }
        }
    }

    void setmistake(int type)
    {
        mistakes[count, 1] = type;
        mistakes[count, 0] = playermic.time;
        count++;
        //do UI stuff here
        //saveanalysis.SaveMistake(this, songname);
    }

    public string identifynote(float frequency, bool checktuning)
    {
        if(frequency == 0f)
        {
            Debug.Log("frequency is zero, bud");
            return null;
        }
        string note;
        double octave;
        int power = Mathf.RoundToInt(Mathf.Log((frequency / 440.0f), 1.059463094359f));
        

        octave = 4 + Math.Ceiling((double)power / 12);
        //Debug.Log("power: " + power + " frequency: " + frequency + " time: " + playermic.time);

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
       
        if (checktuning)
        {
            float tuning = Mathf.Abs(frequency - Mathf.Pow(1.059463094359f, power) * 440);
            if (tuning > maxallowedtuningerror)
            {
                setmistake(3);
                Debug.Log("out of tune");
            }
        }
        
        return note + "_" + octave.ToString();
    }
    

    /// <summary>
    /// Fits a line to a collection of (x,y) points.
    /// </summary>
    /// <param name="xVals">The x-axis values.</param>
    /// <param name="yVals">The y-axis values.</param>
    /// <param name="rSquared">The r^2 value of the line.</param>
    /// <param name="yIntercept">The y-intercept value of the line (i.e. y = ax + b, yIntercept is b).</param>
    /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
    public static void LinearRegression(
        double[] xVals,
        double[] yVals,
        out double rSquared,
        out double yIntercept,
        out double slope)
    {
        if (xVals.Length != yVals.Length)
        {
            throw new Exception("Input values should be with the same length.");
        }

        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double sumCodeviates = 0;

        for (var i = 0; i < xVals.Length; i++)
        {
            var x = xVals[i];
            var y = yVals[i];
            sumCodeviates += x * y;
            sumOfX += x;
            sumOfY += y;
            sumOfXSq += x * x;
            sumOfYSq += y * y;
        }

        var count = xVals.Length;
        var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
        var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

        var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        var meanX = sumOfX / count;
        var meanY = sumOfY / count;
        var dblR = rNumerator / Math.Sqrt(rDenom);

        rSquared = dblR * dblR;
        yIntercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
    }
}
