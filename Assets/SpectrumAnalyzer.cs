using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lomont;
using System;

public class SpectrumAnalyzer : MonoBehaviour {

    private AudioSource source;
    const int precision = 1024;
    //float[] spectrum = new float[precision];
    //float step = 44100 / 2 / precision;


    const int THRESHOLD_WINDOW_SIZE = 15;
    const double MULTIPLIER = 1.35f;
    //float[] samples = new float[1024];
    
    double[] spectrum = new double[precision / 2 + 1];
    double[] lastSpectrum = new double[precision / 2 + 1];

    //double[] spectrum = new double[precision];
    //double[] lastSpectrum = new double[precision];
    List<double> spectralFlux = new List<double>();
    List<double> threshold = new List<double>();
    List<double> prunnedSpectralFlux = new List<double>();

    [HideInInspector]
    public List<double> peaks = new List<double>();

    LomontFFT fft = new LomontFFT();
    float[] audioSamples;
    float[] audioAllChannelsSamples;
    int samplingRate;

    // Use this for initialization
    void Awake () {
        source = GetComponents<AudioSource>()[0];
        audioAllChannelsSamples = new float[source.clip.samples * source.clip.channels];
        audioSamples = new float[source.clip.samples];
        source.clip.GetData(audioAllChannelsSamples, 0);

        samplingRate = source.clip.frequency;

        Debug.Log("number samples: " + source.clip.samples + " allsampleslength: " + audioAllChannelsSamples.Length);

        //into mono
        for(int i = 0; i < audioSamples.Length; i++)
        {
            float sum = 0f;
            for(int j = 0; j < source.clip.channels; j++)
            {
                sum += audioAllChannelsSamples[i * source.clip.channels + j];
            }
            audioSamples[i] = sum / source.clip.channels;
        }


        AudioSettings.outputSampleRate = source.clip.frequency;

        fftThat();


        source.Play();
        /*

        float[] samples = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(samples, 0);
        int i = 0;
        while (i < samples.Length)
        {
            samples[i] = samples[i] * 0.5F;
            ++i;
        }

        */
        //
    }


    double[] currentSamples;// = new double[precision];
    void moveForwardSamples(int index)
    {
        currentSamples = new double[precision];
        int length = (audioSamples.Length - index >= precision) ? precision : audioSamples.Length - index;
        Array.Copy(audioSamples, index, currentSamples, 0, length);
    }

    public double[] Hamming(double[] iwv)
    {
        int N = iwv.Length;

        // iwv[i] = real number (raw wave data) 
        for (int n = 0; n < N; n++)
            iwv[n] *= 0.54f - 0.46f * (float)Math.Cos((2 * Math.PI * n) / (N - 1));

        return iwv;
    }

    public double[] Hann(double[] iwv)
    {
        int N = iwv.Length;

        //EDIT: See blog comments below on this method.
        for (int n = 0; n < N; n++)
            iwv[n] *= 0.5f * (float)Math.Cos((2 * Math.PI * n) / (N - 1));

        return iwv;
    }


    public double[] BlackmanHarris(double[] iwv)
    {
        int N = iwv.Length;

        //EDIT: See blog comments below on this method.
        for (int n = 0; n < N; n++)
            iwv[n] *= 0.35875 - (0.48829 * Math.Cos(1.0 * n / N)) + (0.14128 * Math.Cos(2.0 * n / N)) - (0.01168 * Math.Cos(3.0 * n / N));

        return iwv;
    }

    


    private void Update()
    {
        /*
        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            Debug.DrawLine(new Vector3(i - 1, (float)spectrum[i] + 10, 0), new Vector3(i, (float)spectrum[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log((float)spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log((float)spectrum[i]) + 10, 2), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), (float)spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), (float)spectrum[i] - 10, 1), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log((float)spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log((float)spectrum[i]), 3), Color.blue);
        }
        */
    }

    void fftThat()
    {
        /*
        while (decoder.readSamples(samples) > 0)
        {
            fft.forward(samples);
            fft.FFT

            source.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);


            spectrum.CopyTo(lastSpectrum, 0);
            samples.CopyTo(spectrum, 0);

            float flux = 0;
            for (int i = 0; i < spectrum.Length; i++)
            {
                float value = (spectrum[i] - lastSpectrum[i]);
                flux += value < 0 ? 0 : value;
            }
            spectralFlux.Add(flux);
        }
        */



        
        int currentIndex = 0;//index sample
        Debug.Log("audio length: " + audioSamples.Length);

        while (currentIndex < audioSamples.Length)
        {
            moveForwardSamples(currentIndex);
            currentIndex += precision;

            currentSamples = Hamming(currentSamples);
            
            fft.FFT(currentSamples, true);

            spectrum.CopyTo(lastSpectrum, 0);
            Array.Copy(currentSamples, 0, spectrum, 0, spectrum.Length);


            float flux = 0;
            for (int i = 0; i < spectrum.Length; i++)
            {
                //Debug.Log("spectrum " + spectrum[i]);
                float value = (float)(spectrum[i] - lastSpectrum[i]);
                flux += value < 0 ? 0 : value;
            }
            spectralFlux.Add(flux);
            //Debug.Log("flux " + flux);

            /*
            for (int i = 1; i < spectrum.Length - 1; i++)
            {
                Debug.DrawLine(new Vector3(i - 1, (float) spectrum[i] + 10, 0), new Vector3(i, (float)spectrum[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log((float)spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log((float)spectrum[i]) + 10, 2), Color.cyan);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), (float)spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), (float)spectrum[i] - 10, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log((float)spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log((float)spectrum[i]), 3), Color.blue);
            }
            */

        }

        for (int i = 0; i < spectralFlux.Count; i++)
        {
            int start = Mathf.Max(0, i - THRESHOLD_WINDOW_SIZE);
            int end = Mathf.Min(spectralFlux.Count - 1, i + THRESHOLD_WINDOW_SIZE);
            double mean = 0;
            for (int j = start; j <= end; j++)
                mean += spectralFlux[j];
            mean /= (end - start);
            threshold.Add(mean * MULTIPLIER);
        }

        for (int i = 0; i < threshold.Count; i++)
        {
            if (threshold[i] <= spectralFlux[i])
                prunnedSpectralFlux.Add(spectralFlux[i] - threshold[i]);
            else
                prunnedSpectralFlux.Add((float)0);
        }

        for (int i = 0; i < prunnedSpectralFlux.Count - 1; i++)
        {
            if (prunnedSpectralFlux[i] > prunnedSpectralFlux[i + 1])
                peaks.Add(prunnedSpectralFlux[i]);
            else
                peaks.Add((float)0);
        }

        string showMe = "";
        for (int i = 0; i < peaks.Count; i++)
        {
            float time;
            if (peaks[i] > 0)
            {
                time = i * ((float) precision / samplingRate);
                showMe += time + "/" + peaks[i] + " - ";
            }
            
        }
        Debug.Log(showMe);


        showMe = "Prunned : ";
        for (int i = 0; i < prunnedSpectralFlux.Count; i++)
        {
            showMe += prunnedSpectralFlux[i] + " - ";
        }
        Debug.Log(showMe);

        showMe = "threshold : ";
        for (int i = 0; i < threshold.Count; i++)
        {
            showMe += threshold[i] + " - ";
        }
        Debug.Log(showMe);

    }

    public List<Onset> getListOnset()
    {
        List<Onset> list = new List<Onset>();
        for (int i = 0; i < peaks.Count; i++)
        {
            if (peaks[i] > 0)
            {
                Onset onset = new Onset();
                onset.time = i * ((float)precision / samplingRate);
                onset.energy = (float) peaks[i];
                list.Add(onset);
            }
        }
        return list;
    }

 //   bool once = false;
 //   List<float> timeTime = new List<float>();
 //   // Update is called once per frame
 //   int lastSample = 0;
 //   int iii = 0;

	//void Update () {
 //       //int index = (int) (source.time / (1024 * 44100));
 //       //Debug.Log(peaks[index]);

 //       float[] getSpectrum = new float[2048];

 //       float[] ancientSpectrum = new float[1024];
 //       float[] newSpectrum = new float[1024];

 //       source.timeSamples = iii * 1024;
 //       iii++;
 //       //Debug.Log("sample difference per frame: " + (source.timeSamples - lastSample));
 //       lastSample = source.timeSamples;

 //       //spectrum.CopyTo(lastSpectrum, 0);
 //       source.GetSpectrumData(getSpectrum, 0, FFTWindow.BlackmanHarris);

 //       /*
 //       for (int i = 1; i < spectrum.Length - 1; i++)
 //       {
 //           Debug.DrawLine(new Vector3(i - 1, getSpectrum[i] + 10, 0), new Vector3(i, getSpectrum[i + 1] + 10, 0), Color.red);
 //           Debug.DrawLine(new Vector3(i - 1, Mathf.Log(getSpectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(getSpectrum[i]) + 10, 2), Color.cyan);
 //           Debug.DrawLine(new Vector3(Mathf.Log(i - 1), getSpectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), getSpectrum[i] - 10, 1), Color.green);
 //           Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(getSpectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(getSpectrum[i]), 3), Color.blue);
 //       }
 //       */



 //       //getSpectrum.CopyTo(spectrum, 0);
 //       Array.Copy(newSpectrum, 0, ancientSpectrum, 0, ancientSpectrum.Length);
 //       Array.Copy(getSpectrum, 0, newSpectrum, 0, newSpectrum.Length);
        
 //       //Array.Copy(getSpectrum, 0, spectrum, 0, spectrum.Length);

 //       float flux = 0;
 //       for (int i = 0; i < spectrum.Length; i++)
 //       {
 //           float value = (float)(newSpectrum[i] - ancientSpectrum[i]);
 //           flux += value < 0 ? 0 : value;
            
 //       }
        
        
 //       if(!source.isPlaying && !once)
 //       {
 //           once = true;
 //           processPeak();
 //       }
 //       if(!once)
 //       {
 //           spectralFlux.Add(flux);
 //           timeTime.Add(source.time);
 //           Debug.Log("flux " + flux);
 //       }
 //   }

 //   void processPeak()
 //   {
 //       for (int i = 0; i < spectralFlux.Count; i++)
 //       {
 //           int start = Mathf.Max(0, i - THRESHOLD_WINDOW_SIZE);
 //           int end = Mathf.Min(spectralFlux.Count - 1, i + THRESHOLD_WINDOW_SIZE);
 //           float mean = 0;
 //           for (int j = start; j <= end; j++)
 //               mean += spectralFlux[j];
 //           mean /= (end - start);
 //           threshold.Add(mean * MULTIPLIER);
 //       }

 //       for (int i = 0; i < threshold.Count; i++)
 //       {
 //           if (threshold[i] <= spectralFlux[i])
 //               prunnedSpectralFlux.Add(spectralFlux[i] - threshold[i]);
 //           else
 //               prunnedSpectralFlux.Add((float)0);
 //       }

 //       for (int i = 0; i < prunnedSpectralFlux.Count - 1; i++)
 //       {
 //           if (prunnedSpectralFlux[i] > prunnedSpectralFlux[i + 1])
 //               peaks.Add(prunnedSpectralFlux[i]);
 //           else
 //               peaks.Add((float)0);
 //       }

 //       string showMe = "";
 //       for (int i = 0; i < peaks.Count; i++)
 //       {
 //           float time;
 //           if (peaks[i] > 0.0f)
 //           {
 //               time = i * ((float)precision / samplingRate);
 //               showMe += timeTime[i] + "/" + peaks[i] + " - ";
 //           }

 //       }
 //       Debug.Log(showMe);
 //   }
}
