# KNMIDownloader-NoSave

## What it is

KNMIDownloader-NoSave is a .NET program that automatically posts the weather maps on the website of the Royal Netherlands Meteorological Institute (KNMI) to a Discord server of your liking.

## What it isn't

KNMIDownloader-NoSave is not a program that saves raw data like the output of instruments. It only downloads and posts existing GIFs released every five minutes on knmi.nl. They are deleted from your storage device after posting.

## What it can be used for

KNMIDownloader-NoSave can be useful for those who want to automatically post radar gifs to a Discord server.

## How to use it

Configuring system.json
- The file is located in src/knmidownloader/sys.
- Place your Discord bot token in the 'Token' field. Replace the text "YourBotTokenHere".
- Place the IDs of your Discord channels in their slots.
- Start your Docker container. The build process will copy the system.json file to the 'sys' folder located in the output directory.

Choose a period to record weather maps of.
You can also run KNMIDownloader forever, and it will continue posting!

## Building the code yourself...

KNMIDownloader was written in C# for .NET 8.
To build it yourself, just download the src folder and open KNMIDownloader.sln. It might load. If it doesn't, cope.

## License

Please see the LICENSE file to learn what you can and cannot do with our code.