using NAudio.CoreAudioApi;
using NAudio.Wave;

// TEST FILE (GOD BLESS YOUR EARS): https://www.youtube.com/watch?v=qNf9nzvnd1k

WasapiCapture AudioDevice;
double[] FftValues;
double[] AudioValues; 

SelectAudioDevice();

WaveFormat fmt = AudioDevice.WaveFormat;
AudioValues = new double[fmt.SampleRate / 10];
double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
double[] fftMag = FftSharp.Transform.FFTpower(paddedAudio);
FftValues = new double[fftMag.Length];
double fftPeriod = FftSharp.Transform.FFTfreqPeriod(fmt.SampleRate, fftMag.Length);

Console.WriteLine($"{fmt.Encoding} ({fmt.BitsPerSample}-bit) {fmt.SampleRate} KHz");

AudioDevice.DataAvailable += WaveIn_DataAvailable;
AudioDevice.StartRecording();

Console.WriteLine("<Press any key to exit>\n\n");
Console.ReadKey();

FftSignalClosed();

void SelectAudioDevice()
{
    MMDevice[] AudioDevices = new MMDeviceEnumerator()
    .EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
    .ToArray();

    Console.WriteLine("Please, select your microphone:");

    int i = 0;
    foreach (MMDevice device in AudioDevices)
    {
        string deviceType = device.DataFlow == DataFlow.Capture ? "INPUT" : "OUTPUT";
        string deviceLabel = $"{deviceType}: {device.FriendlyName}";
        Console.WriteLine($"[{i}] " + deviceLabel);
        i++;
    }

    bool isDigit;
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey();
        isDigit = char.IsDigit(key.KeyChar);
    }
    while (!isDigit);

    Console.WriteLine("\n");

    int deviceID = int.Parse(key.KeyChar.ToString());

    MMDevice selectedDevice = AudioDevices[deviceID];
    WasapiCapture audioDevice = selectedDevice.DataFlow == DataFlow.Render
        ? new WasapiLoopbackCapture(selectedDevice)
        : new WasapiCapture(selectedDevice, true, 10);

    AudioDevice = audioDevice;
}

void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
{
    int bytesPerSamplePerChannel = AudioDevice.WaveFormat.BitsPerSample / 8;
    int bytesPerSample = bytesPerSamplePerChannel * AudioDevice.WaveFormat.Channels;
    int bufferSampleCount = e.Buffer.Length / bytesPerSample;

    if (bufferSampleCount >= AudioValues.Length)
    {
        bufferSampleCount = AudioValues.Length;
    }

    if (bytesPerSamplePerChannel == 2 && AudioDevice.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
    {
        for (int i = 0; i < bufferSampleCount; i++)
            AudioValues[i] = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
    }
    else if (bytesPerSamplePerChannel == 4 && AudioDevice.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
    {
        for (int i = 0; i < bufferSampleCount; i++)
            AudioValues[i] = BitConverter.ToInt32(e.Buffer, i * bytesPerSample);
    }
    else if (bytesPerSamplePerChannel == 4 && AudioDevice.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
    {
        for (int i = 0; i < bufferSampleCount; i++)
            AudioValues[i] = BitConverter.ToSingle(e.Buffer, i * bytesPerSample);
    }
    else
    {
        throw new NotSupportedException(AudioDevice.WaveFormat.ToString());
    }

    DisplayRawData(GetPeakFrequency());
    //DisplayBandedData(GetPeakFrequency());
}

void DisplayRawData(double freq)
{
    Console.CursorLeft = 0;
    Console.CursorVisible = false;

    Console.Write($"Peak Frequency: {freq,-6:N0} Hz");
}

void DisplayBandedData(double freq)
{
    int BAND1 = 100;
    int BAND2 = 500;
    int BAND3 = 1000;

    Console.CursorLeft = 0;
    Console.CursorVisible = false;

    if ( freq < BAND1 )
    {
        Console.Write($"Peak Frequency: < {BAND1, -6}");
    } else if ( freq < BAND2 )
    {
        Console.Write($"Peak Frequency: < {BAND2,-6}");
    } else if (freq < BAND3)
    {
        Console.Write($"Peak Frequency: < {BAND3,-6}");
    } else
    {
        Console.Write($"Peak Frequency: > {BAND3,-6}");
    }
}

double GetPeakFrequency()
{
    double[] paddedAudio = FftSharp.Pad.ZeroPad(AudioValues);
    double[] fftMag = FftSharp.Transform.FFTmagnitude(paddedAudio);
    Array.Copy(fftMag, FftValues, fftMag.Length);

    // find the frequency peak
    int peakIndex = 0;
    for (int i = 0; i < fftMag.Length; i++)
    {
        if (fftMag[i] > fftMag[peakIndex])
            peakIndex = i;
    }
    double fftPeriod = FftSharp.Transform.FFTfreqPeriod(AudioDevice.WaveFormat.SampleRate, fftMag.Length);
    double peakFrequency = fftPeriod * peakIndex;

    return peakFrequency;
}

void FftSignalClosed()
{
    AudioDevice.StopRecording();
    AudioDevice.Dispose();
}
