using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Numerics;
using DSPLib;
using System;
using System.IO;

public class preprocessspectrum : MonoBehaviour
{
    float[] multiChannelSamples;
    int numChannels, numTotalSamples;
    float clipLength, sampleRate;
    float previousdip = 0f, previousdiptime, previoustime, herztperbin, F0canidate, foundamental, harmonics, highestpeak, time, currentloudness, previousloudness, loudness, loudness_slope, loudnessaverage, maxloudnessratio, starttime;
    public List<string> note;
    string[] previousnote = new string[2], currentnote, trdnotedown = new string[2];
    public float[] dynamics, previousnotechange = new float[18], loudness_time = new float[29];
    public float[,] duration;
    List<string> noteset = new List<string>();
    int notecount = 1, dynamicscount = 1, mid;
    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        dynamics = new float[888];
        duration = new float[888, 2];
        note = new List<string>();
        mid = (loudness_time.Length - 1) / 2;

    }
    void Update()
    {
        if (gameObject.GetComponent<AudioSource>().clip != null)
        {
            Debug.Log("clip setted");
            herztperbin = 24000f / 8192 / (48000 / audioSource.clip.frequency);
            getFullSpectrumThreaded();
        }
    }
    public int getIndexFromTime(float curTime)
    {
        float lengthPerSample = this.clipLength / (float)this.numTotalSamples;

        return Mathf.FloorToInt(curTime / lengthPerSample);
    }

    public float getTimeFromIndex(int index)
    {
        return ((1f / (float)this.sampleRate) * index);
    }



    public void getFullSpectrumThreaded()
    {
        

        // Need all audio samples.If in stereo, samples will return with left and right channels interweaved
        // [L,R,L,R,L,R]
        multiChannelSamples = new float[audioSource.clip.samples * audioSource.clip.channels];
        numChannels = audioSource.clip.channels;
        numTotalSamples = audioSource.clip.samples;
        clipLength = audioSource.clip.length;

        // We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
        sampleRate = audioSource.clip.frequency;

        audioSource.clip.GetData(multiChannelSamples, 0);

        Debug.Log("channels: " + numChannels);
        try
        {
            // We only need to retain the samples for combined channels over the time domain
            float[] preProcessedSamples = new float[this.numTotalSamples];

            int numProcessed = 0;
            float combinedChannelAverage = 0f;
            for (int i = 0; i < multiChannelSamples.Length; i++)
            {
                combinedChannelAverage += multiChannelSamples[i];

                // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                if ((i + 1) % this.numChannels == 0)
                {
                    preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
                    numProcessed++;
                    combinedChannelAverage = 0f;
                }
            }

            Debug.Log("Combine Channels done");
            Debug.Log(preProcessedSamples.Length);

            // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
            int spectrumSampleSize = 4096;
            int iterations = preProcessedSamples.Length / spectrumSampleSize;

            FFT fft = new FFT();
            fft.Initialize((UInt32)spectrumSampleSize);

            Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
            double[] sampleChunk = new double[spectrumSampleSize];

            for (int i = 0; i < iterations; i++)
            {
                // Grab the current 1024 chunk of audio sample data
                Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                // Apply our chosen FFT Window
                double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                // Perform the FFT and convert output (complex numbers) to Magnitude
                Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
                float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

                // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                analysespectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, i);               
            }

            data preprocessed = new data();

            preprocessed.duration = duration;
            preprocessed.notes = note;
            preprocessed.correctdynamics = dynamics;

            saveanalysis.SavePlayer(preprocessed, audioSource.clip.name);
            Debug.Log("Spectrum Analysis done");
            Debug.Log("Background Thread Completed");
            Camera.main.GetComponent<AudioSource>().clip = audioSource.clip;
            audioSource.clip = null;
            Camera.main.GetComponent<AudioSource>().Play();
            Camera.main.gameObject.GetComponent<spectrumanalysis>().generatesonglist();

        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    public void analysespectrum(float[] spectrum, float time, int iterationnum)
    {
        int i, j, k;
        if (time == previoustime)
        {
            return;
        }
        previoustime = time;
        int peakcount = 0, c = 0;
        float loudest = 0f, loudestindex = 0f;
        float[,] peaks = new float[18, 2];
        for (i = 2; i * herztperbin < 2000f; i++)
        {
            if (spectrum[i] > loudest)
            {
                loudest = spectrum[i];
                loudestindex = i * herztperbin;
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
                peaks[peakcount, 0] = (float)i * herztperbin;
                peaks[peakcount, 1] = spectrum[i];
                //Debug.Log(time + " peak #" + peakcount.ToString() + ": " + peaks[peakcount, 0].ToString() + " loudness " + peaks[peakcount, 1]);

                peakcount++;

                if (peakcount >= 18) break;
            }
            c = 0;
        }
        /*if (time >= 13.2f&&time<=13.4f)
        {
            WriteString();
        }*/


        float[,] refinedpeaks = peaks.OrderBy(p => p[1]);
        float max = 0f;
        int maxindex = 0;
        for (i = 10; i < refinedpeaks.GetLength(0); i++)
        {
            //Debug.Log(time + " refined peaks #" + i + ": " + refinedpeaks[i, 0] + " loudness " + refinedpeaks[i, 1]);
            loudness += refinedpeaks[i, 1];
        }
        loudness /= 8;
        loudness_slope = (loudness - previousloudness) / 0.0106803f;
        previousloudness = loudness;
        checkloudness();

        float bin = 0f;
        /*WriteString(time.ToString() + "\n");
        for (i = 0; i < spectrum.Length; i++)
        {
            bin = i * herztperbin;
            WriteString(bin.ToString() + "," + spectrum[i].ToString("F8") + "\n");
        }
        WriteString("\n" + "\n");*/
        //writeloudness();
        //Debug.Log(time + " loudest: " + loudestindex + " maxindex: " + maxindex + " max ratio: " + max);
        max = 0f;


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
            Debug.Log(time + " Foudamental canidate: " + identifynote(F0canidate, false) + "number of harmonics: " + count + " max harmonics: " + c + "weighted average: " + weightedaverage[i]);
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


        Debug.Log(time + " base note before refine: " + identifynote((foundamental), false) + " " + foundamental);
        currentnote = identifynote((foundamental), false).Split('_');
        if (currentnote[0] != previousnote[0])
        {
            //Debug.Log(time+" note swicthed");
            previousnotechange[notecount] = time;
            noteset.Add(currentnote[0]);

            checkchanging();
            if (notecount < previousnotechange.Length - 1) notecount++;
        }
        trdnotedown = previousnote;
        previousnote = currentnote;

    }

    void checkchanging()
    {
        if (notecount > 0)
        {
            //Debug.Log(time + " previous note-switch: " + previousnotechange[notecount - 1]);
            if (time - previousnotechange[notecount - 1] < 0.08f && currentnote[0] == trdnotedown[0])
            {
                // Debug.Log(time + " a note switch was discredited");
                previousnotechange[notecount] = 0f;
                previousnotechange[notecount - 1] = 0f;
                noteset.RemoveAt(notecount);
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

        loudness_time[loudness_time.Length - 1] = loudness;
        for (i = 1; i <= mid; i++)
        {
            if (loudness_time[mid + i] > loudness_time[mid])
            {
                right++;
                if ((loudness_time[mid + i] - loudness_time[mid + i - 1]) > 0f)
                {
                    rslope += (loudness_time[mid + i] - loudness_time[mid + i - 1]) / 0.0106803f;
                }
            }
            else
            {
                break;
            }
        }
        for (i = 1; i <= mid; i++)
        {
            if (loudness_time[mid - i] > loudness_time[mid])
            {
                left++;
                if ((loudness_time[mid - i] - loudness_time[mid - i + 1]) < 0f)
                {
                    lslope += (loudness_time[mid - i] - loudness_time[mid - i + 1]) / 0.0106803f;
                }
            }
            else
            {
                break;
            }
        }

        if (loudness_time.Max() - loudness_time[mid] > 0.001f && Math.Max(right, left) == mid && Math.Min(right, left) > 4 && Math.Max(lslope, rslope) >= 0.3f)
        {
            //Debug.Log(time + " dip");
            //Debug.Log((time - 0.0106803f * mid) + " slope average: " + Math.Max(lslope, rslope));
            previousdip = loudness_time.Max() - loudness_time[mid];
            for (i = 0; i < previousnotechange.Length; i++)
            {
                //Debug.Log(time + " previous note swicth: " + previousnotechange[i]+" previous dip time: "+ (time - 0.0106803f * mid));
                if (Math.Abs((time - 0.0106803f * mid) - previousnotechange[i]) < 0.05f)
                {
                    Debug.Log((time - 0.0106803f * mid) + " note change!"+" the mode note is: "+ getmodenote(noteset.ToArray()));
                    previousnotechange = new float[previousnotechange.Length];
                    notecount = 0;
                }
                //writeloudness();
            }
        }

    }
    public string getmodenote(string[] notes)
    {
        var groups = notes.GroupBy(v => v);
        int maxCount = groups.Max(g => g.Count());
        string mode = groups.First(g => g.Count() == maxCount).Key;
        return mode;
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
    float weirdweighing(int harmonics)
    {
        return ((float)Mathf.Log((harmonics * (float)Math.Sqrt((harmonics + 1) / (double)harmonics)), 1.2599210498948731647672106072782f) - (float)Mathf.Log(((harmonics - 1) * (float)Math.Sqrt((double)harmonics / (harmonics - 1))), 1.2599210498948731647672106072782f));
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
    /*[MenuItem("Tools/Write file")]
    void writeloudness()
    {
        string path = "Assets/test/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine((time - 0.1495242f).ToString() + "," + loudness);
        writer.Close();
    }
    void WriteString(string text)
    {
        string path = "Assets/test/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine(text);
        
        writer.Close();
    }*/
}