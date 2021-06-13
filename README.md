# Chia Harvester Image
Create a visual graph of Chia network harvesting activity.

![screenshot](https://github.com/gustrix/ChiaHarvesterImage/blob/master/screenshot.png)

This program requires .NET Core 5.0.
It has been tested on Windows 10 and Debian Linux.

# Concept

Your harvester should get challenge messages at a rate of 64 per 10 minutes. This program scans a log file and determines if your harvester is matching that expected rate.

# Installation

1. Install .NET Core 5.0 SDK for your platform.

2. Build the solution

3. Run with a log file.

You can use an optional HTML template to present the information. An example is included.


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
