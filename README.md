# FireFetcher

# Categories
### Speedrun.com
- Portal 2 NoSLA
- Portal 2 Amc
- Portal 2 Speedrun Mod
- Portal Stories: Mel
### board.portal2.sr
- Portal 2 Challenge Mode

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
