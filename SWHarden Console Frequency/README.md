#Frequency display program#
This program will prompt you for your audio source (either input or output) and and will display it's current frequency.

#### Swap from current frequency to frequency bands display ####
Go to line **99-100** and chage
```csharp  
DisplayRawData(GetPeakFrequency());
//DisplayBandedData(GetPeakFrequency());
```
to
```csharp  
//DisplayRawData(GetPeakFrequency());
DisplayBandedData(GetPeakFrequency());
```

#### To change bands and their amount ####
Go to line **111** and chage the `DisplayBandedData` function
```csharp  
void DisplayBandedData(double freq)
{
    int BAND1 = 100;       // change these values for upper band limits
    int BAND2 = 500;       // change these values for upper band limits
    int BAND3 = 1000;      // change these values for upper band limits

    Console.CursorLeft = 0;
    Console.CursorVisible = false;

    if ( freq < BAND1 )    // change this code if you want more/less bands
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
```
