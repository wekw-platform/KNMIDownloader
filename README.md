# KNMIDownloader-NoSave

## What it is

KNMIDownloader-NoSave is a .NET program that automatically posts the weather maps present on the website of the Royal Netherlands Meteorological Institute (KNMI) to a Discord server of your liking.

## What it isn't

KNMIDownloader-NoSave is not a program that saves raw data like the output of instruments. It only downloads and posts existing GIFs released every five minutes on knmi.nl. They are deleted from your storage after posting.

## What it can be used for

KNMIDownloader-NoSave can be useful for those who want to automatically post radar gifs to a Discord server.

## How to use it

Choose a period to record weather maps of. Start the KNMIDownloader executable on time and stop it at any time.
You can also run KNMIDownloader forever, and it will continue saving... until you're out of storage!

## The Discord bot...

The KNMIDownloader executable has an optional Discord bot built in. You can configure it so that it sends the newest weather map GIFs and images to Discord channels of your choice that you own. To use this:

- Log in with your Discord account on the Developer portal and create a bot.
- Find your bot's token on the Bot page of your application within the Discord Developer portal.
- Start KNMIDownloader with the "dodiscord" argument. It will create the folders and files that it needs.
- Now, you will be launched into Setup.
- Follow the steps.
- After completing Setup, KNMIDownloader will start the Discord Bot. You now have fully set up the KNMIDownloader Discord Bot!

## Building the code yourself...

KNMIDownloader was written in C# for .NET 8.
To build it yourself, just download the src folder and open KNMIDownloader.sln. It might load. If it doesn't, cope.

## License

Please see the LICENSE file to learn what you can and cannot do with our code.