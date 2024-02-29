# FireFetcher

# Categories
### speedrun.com
- Portal 2 NoSLA
- Portal 2 Amc
- Portal 2 Speedrun Mod
- Portal Stories: Mel
### board.portal2.sr
- Portal 2 Challenge Mode
### lp.nekz.me
- Portal 2 Least Portals

# How it works
### Leaderboards
The leaderboards part of the bot is an automatic way to fetch users pbs from speedrun.com and board.portal2.sr for the added users. To do this, the bot goes through the following stages.

1. Build url to make api requests to each website
2. Make an api request for each added user
3. Parse the request response
   - If the result is from speedrun.com, clean the time to "standard notation" and only save specified [categories](#categories)
4. Clean duplicate runs and only save best run from each player
5. Build the embed
6. Post / edit message

# Deployment
### Ubuntu
1. Using the command prompt, navigate to the folder containing the solution file (FireFetcher.sln)
2. Run ``dotnet publish -c release -r ubuntu.16.04-x64 --self-contained``
3. Copy the publish folder (../bin/Release/net6.0/ubuntu.16.04-x64/publish) to the Ubuntu machine
4. Open the Ubuntu machine terminal (CLI) and go to the project directory
5. Provide execute permissions: ``chmod +x ./FireFetcher``
6. Execute the application by running ``./FireFetcher``

# Architecture
### CategoryIndexes
| Index | Category   |
| ------| :--------: |
| 0     | NoSLA      |
| 1     | Amc        |
| 2     | Srm        |
| 3     | PS: Mel    |
| 4     | SP CM      |
| 5     | SP LP      |
