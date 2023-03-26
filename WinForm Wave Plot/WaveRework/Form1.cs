using ScottPlot;
using System;
using System.Windows.Forms;
using NAudio.Wave;

namespace WaveRework
{
    public partial class Form1 : Form
    {
        private WaveIn waveIn;
        private Plot plot;

        // Define the intensity interval thresholds
        private const double QuietThreshold = 0.1;
        private const double LoudThreshold = 0.9;

        public Form1()
        {
            InitializeComponent();
            InitializePlot();
        }

        private void InitializePlot()
        {
         
            plot = new Plot(800, 400);
            formsPlot1.Plot.YLabel("Amplitude");
            formsPlot1.Plot.XLabel("Time (s)");
            formsPlot1.Refresh();
            formsPlot1.Render();
        }

        private void StartCapturingAudio()
        {
            int blockSize = 1024;
            int sampleRate = 44100; // or the sample rate of your audio input device
            double[] samples = new double[blockSize];

            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = blockSize * 1000 / sampleRate;
            waveIn.WaveFormat = new WaveFormat(sampleRate, 16, 1);
            waveIn.DataAvailable += (sender, e) =>
            {
                byte[] buffer = e.Buffer;
                int bytesPerSample = waveIn.WaveFormat.BitsPerSample / 8;
                int sampleCount = e.BytesRecorded / bytesPerSample;

                double sumOfSquares = 0;
                for (int i = 0; i < sampleCount; i++)
                {
                    double sampleValue = BitConverter.ToInt16(buffer, i * bytesPerSample) / 32768.0; // convert to a double value between -1 and 1
                    sumOfSquares += sampleValue * sampleValue;
                    samples[i] = sampleValue;
                }

                double rmsAmplitude = Math.Sqrt(sumOfSquares / sampleCount);
                string intensity;
                if (rmsAmplitude < QuietThreshold)
                {
                    intensity = "Quiet";
                }
                else if (rmsAmplitude > LoudThreshold)
                {
                    intensity = "Loud";
                }
                else
                {
                    intensity = "Normal";
                }

                formsPlot1.Plot.Clear();
                formsPlot1.Plot.PlotSignal(samples, sampleRate);
                formsPlot1.Plot.Title($"Intensity: {intensity} (RMS Amplitude: {rmsAmplitude:F3})");
                formsPlot1.Render();
            };
            waveIn.StartRecording();
        }

        private void StopCapturingAudio()
        {
            waveIn.StopRecording();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartCapturingAudio();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopCapturingAudio();
        }
    }
}
