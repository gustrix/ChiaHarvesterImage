# Chia Harvester Image
Create a visual representation of Chia network harvesting activity.

![screenshot](https://github.com/gustrix/ChiaHarvesterImage/blob/master/screenshot.png)

This program requires .NET Core 5.0.
It has been tested on Windows 10 and Debian Linux.

# Concept

Your harvester should get challenge messages at a rate of 64 per 10 minutes. This program scans a log file and determines if your harvester is matching that expected rate.

It generates a 640x360 16:9 image with a resolution of 80x60, using 8x6 rectangles. Each rectangle represents one minute, so each column is one hour.

The data is presented with the oldest in the upper-left and the newest on the lower-right.

# Installation

1. Install .NET Core 5.0 SDK for your platform.

2. Build the solution

3. Run with a log file.

```
Usage:

SpiceHarvester <input debug.log> <output image.png> [optional template.html]
    Normally, stats will be sent to stdout but they can also be used with a template.
    If a template is specified, then these placeholders will be replaced by values:
        %img%         Your image filename
        %best%        Best response time (ms)
        %worst%       Worst response time (ms)
        %avg%         The average response time (ms)
        %efficiency%  How much harvesting was achieved
        %proofs%      Number of proofs found
```

An example HTML template is included.

# Example for Debian Linux

You need the .NET Core 5.0 SDK. Instructions:

https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian

Building:

`dotnet publish -c release -r debian.10-x64 --self-contained`

# Notes

It's best if you filter your logs to isolate the harvesting messages.

This can be done very simply. For example, in Linux:

`grep -h eligible debug.log.1 debug.log > output.log`

Isolating the messages will make it faster to parse. Keeping the harvesting in a separate log also lets you manage that more efficiently.
