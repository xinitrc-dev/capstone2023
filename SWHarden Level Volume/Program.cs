var waveIn = new NAudio.Wave.WaveInEvent
{
    DeviceNumber = MicPrompt(), // microphone index
    WaveFormat = MicSpecsPrompt(), // microphone bitrate, channels and bandwith
    BufferMilliseconds = 20
};

waveIn.DataAvailable += WaveIn_AdvancedDataAvailable;
//waveIn.DataAvailable += WaveIn_SimpleDataAvailable;
waveIn.StartRecording();

Console.WriteLine("C# Audio Level Meter");
Console.WriteLine("<Press any key to exit>\n\n");
Console.ReadKey();


static void WaveIn_AdvancedDataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
{
    int bytesPerSample = 2;
    int channelCount = 2;
    int sampleCount = e.Buffer.Length / bytesPerSample / channelCount;

    Int16[] valuesL = new short[sampleCount];
    Int16[] valuesR = new short[sampleCount];

    for (int i = 0; i < sampleCount; i++)
    {
        int position = i * bytesPerSample * channelCount;
        valuesL[i] = BitConverter.ToInt16(e.Buffer, position);
        valuesR[i] = BitConverter.ToInt16(e.Buffer, position + 2);
    }

    float maxPercentL = (float)valuesL.Max() / 32768;
    float maxPercentR = (float)valuesR.Max() / 32768;

    // print a level meter using the console
    string barL = new('#', (int)(maxPercentL * 40));
    string barR = new('#', (int)(maxPercentR * 40));

    string meterL = $"[{barL.PadRight(40, '-')}]";
    string meterR = $"[{barR.PadRight(40, '-')}]";

    //Console.CursorLeft = 0;

    //Console.Write(new string(' ', 50));


    Console.CursorLeft = 0;
    Console.CursorVisible = false;

    //Console.Write($"L: {maxPercentL:000.0}% R:{maxPercentR:000.0}%\n");
    Console.Write($"{meterL} {maxPercentL * 100:000.0}%     {meterR} {maxPercentR * 100:000.0}%");
}

static void WaveIn_SimpleDataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
{
    // copy buffer into an array of integers
    Int16[] values = new short[e.Buffer.Length / 2];
    Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

    // determine the highest value as a fraction of the maximum possible value
    float fraction = (float)values.Max() / 32768;

    // print a level meter using the console
    string bar = new('#', (int)(fraction * 60));
    string meter = $"[{bar.PadRight(60, '-')}]";

    Console.CursorLeft = 0;
    Console.CursorVisible = false;

    Console.Write($"{meter} {fraction * 100:00.0}%");
}

static int MicPrompt()
{
    Console.WriteLine("Please, select your microphone:");

    for (int i = -1; i < NAudio.Wave.WaveIn.DeviceCount; i++)
    {
        var caps = NAudio.Wave.WaveIn.GetCapabilities(i);
        Console.WriteLine($"[{i}]: {caps.ProductName}");
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

    return int.Parse(key.KeyChar.ToString());
}

static NAudio.Wave.WaveFormat MicSpecsPrompt()
{
    Console.WriteLine("Please, select your microphone preset:");

    Console.WriteLine("[1]: 2 channel, 16 bit, 48000 Hz");
    Console.WriteLine("[2]: 2 channel, 16 bit, 44100 Hz");

    bool isDigit;
    ConsoleKeyInfo key;
    int number = -1;

    do
    {
        key = Console.ReadKey();
        isDigit = char.IsDigit(key.KeyChar);

        if (isDigit)
        {
            number = int.Parse(key.KeyChar.ToString());
            if (number < 1 || number > 2)
                isDigit = false;
        }
    }
    while (!isDigit);

    Console.WriteLine("\n");

    switch (number)
    {
        case 1:
            return new NAudio.Wave.WaveFormat(rate: 48000, bits: 16, channels: 2);
        case 2:
            return new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 2);
        default:
            throw new Exception("Something went wrong! :(");
    }
}