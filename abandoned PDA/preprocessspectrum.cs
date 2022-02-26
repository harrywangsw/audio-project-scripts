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

        if (audioSource.clip != null)
        {
            herztperbin = 48000f / 4096 / (48000 / audioSource.clip.frequency);
            getFullSpectrumThreaded(new data());
        }
    }
    void Update()
    { 
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

    public void getFullSpectrumThreaded(data tobecorrected)
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
                analysespectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
            }

            

            Debug.Log("Spectrum Analysis done");
            Debug.Log("Background Thread Completed");

        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    void analysespectrum(float[] spectrum, float time)
    {
        int i, j;
        float loudest = 0f, frequency = 0f; 
        for (i = 0; i<spectrum.Length; i++)
        {
            if (spectrum[i] > loudest)
            {
                loudest = spectrum[i];
                frequency = i * herztperbin;
            }
        }
        note.Add(identifynote(frequency, false));
        Debug.Log(time+" "+note[note.Count - 1]);
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