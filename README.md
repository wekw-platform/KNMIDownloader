# KNMIDownloader

## What it is:

KNMIDownloader is a .NET program that automatically saves the weather maps present on the website of the Royal Netherlands Meteorological Institute (KNMI).

## What it isn't:

KNMIDownloader is not a program that saves raw data like the output of instruments. It only downloads existing GIFs released every five minutes on knmi.nl.

## What it can be used for:

KNMIDownloader can be useful for those who want to make radar timelapses of weather events that happen in the Netherlands.

## How to use it:

Choose a period to record weather maps of. Start the KNMIDownloader executable on time and stop it at any time.
You can also run KNMIDownloader forever, and it will continue saving... until you're out of storage!

## The Discord bot...

The KNMIDownloader executable has an optional Discord bot built in. You can configure it so that it sends the newest weather map GIFs to a Discord channel of your choice that you own. To use this:

- Log in with your Discord account on the Developer portal and create a bot.
- Start KNMIDownloader with the "dodiscord" argument. It will create the folders and files that it needs.
- Exit the program.
- Find your bot's token on the Bot page of your application within the Discord Developer portal.
- Copy and paste this token into discord-token.txt, located in the "sys" folder.
- Find your server's ID and the ID of the channel you want KNMIDownloader to post system messages to.
- Find the IDs of the posting channels. Specify one for neerslag-bliksem-temp, neerslag, neerslag-bliksem, neerslag-temp, neerslag-wind_ms, neerslag-wind_bft and the warning maps.
- Paste these IDs in the /sys/ids.txt file like this:  
  SystemServerID:SystemChannelID#neerslag-bliksem-tempID:neerslagID:neerslag-bliksemID:neerslag-tempID:neerslag-wind_msID:neerslag-wind_bftID:warningmapsID. I am aware this is insanely dirty and weird, and it will be an easier process in later versions of KNMIDownloader.
- You now have fully set up KNMIDownloader-Bot.

## Building the code yourself...

KNMIDownloader was written in C# for .NET 8.
To build it yourself, just download the src folder and open KNMIDownloader.sln. It might load. If it doesn't, cope.

## License

Please see the LICENSE file to learn what you can and cannot do with our code.