using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using DSPLib;
using System;

public class preprocessspectrum : MonoBehaviour
{
    float[] multiChannelSamples;
    int numChannels, numTotalSamples;
    float clipLength, sampleRate, loudest, secondloud;
    public string[] note;
    public float[,] duration;
    static float herztperbin = 24000f / 4097f;
    int count = 0;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        // Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
        // [L,R,L,R,L,R]
        multiChannelSamples = new float[audioSource.clip.samples * audioSource.clip.channels];
        numChannels = audioSource.clip.channels;
        numTotalSamples = audioSource.clip.samples;
        clipLength = audioSource.clip.length;

        // We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
        sampleRate = audioSource.clip.frequency;

        audioSource.clip.GetData(multiChannelSamples, 0);
        getFullSpectrumThreaded();
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
        note = new string[888];
        duration = new float[888, 2];
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
            int spectrumSampleSize = 8192;
            int iterations = preProcessedSamples.Length / spectrumSampleSize;

            FFT fft = new FFT();
            fft.Initialize((UInt32)spectrumSampleSize);

            Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
            double[] sampleChunk = new double[spectrumSampleSize];
            for (int i = 0; i < iterations; i++)
            {
                // Grab the current 8192 chunk of audio sample data
                Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                // Apply our chosen FFT Window
                double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                // Perform the FFT and convert output (complex numbers) to Magnitude
                Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                // These 8192 magnitude values correspond (roughly) to a single point in the audio timeline
                float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

                // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                analysespectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 1.087f);
                loudest = 0f;
                secondloud = 0f;
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
    
    public void analysespectrum(float[] spectrum, float time, float adjustmentvalue)
    {
        //Debug.Log("length:   "+ spectrum.Length);
        float loudestindex = 0f;
        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            if (spectrum[i] > 0.03f)
            {
                if (spectrum[i] > loudest)
                {
                    secondloud = loudest;
                    loudest = spectrum[i];
                    loudestindex = i * herztperbin/adjustmentvalue;
                }
            }
        }



        //Debug.Log("loudest herzt: " + loudestindex+"    time: "+time.ToString());
        string curnote = GameObject.Find("Main Camera").GetComponent<spectrumanalysis>().identifynote(loudestindex);
        if (note[count] != curnote && curnote != "???? no frakking idea")
        {
            note[count + 1] = curnote;

            duration[count + 1, 0] = time;
            duration[count, 1] = time;
            Debug.Log("current note: " + note[count + 1]);
            //saveanalysis.SavePlayer(this, "test");
            if (count > 887) count += 1;

        }
        loudest = 0f;
        secondloud = 0f;
    }
    
}