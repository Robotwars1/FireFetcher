﻿using static Program;

namespace FireFetcher
{
    public class UserHandler
    {
        const string UsersFilePath = "Data/Users.json";

        private Program FireFetcher;
        private readonly JsonInterface JsonInterface = new();

        public List<Username> Users = new();

        public void Setup(Program fireFetcher)
        {
            FireFetcher = fireFetcher;

            Users = (List<Username>)JsonInterface.ReadJson(UsersFilePath, "Users");
            // If Users is returned as a null list, re-create it to avoid program crashing
            Users ??= new();
        }

        public async void LinkAccount(ulong UserID, string Username, string SrcUsername, string BoardProfileID, string SteamUsername)
        {
            bool AlreadyInUsers = false;
            int UserIndex = 0;

            // Check if User is already added to Users list
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].DiscordID == UserID)
                {
                    AlreadyInUsers = true;
                    UserIndex = i;
                    break;
                }
            }

            if (AlreadyInUsers)
            {
                Users[UserIndex].SpeedrunCom = SrcUsername;
                Users[UserIndex].BoardProfileID = BoardProfileID;
                Users[UserIndex].Steam = SteamUsername;
            }
            else
            {
                Users.Add(new Username() { DiscordID = UserID, DiscordName = Username, SpeedrunCom = SrcUsername, BoardProfileID = BoardProfileID, Steam = SteamUsername });
            }

            // Write changes to Users file
            JsonInterface JsonInterface = new();
            JsonInterface.WriteToJson(Users, UsersFilePath);

            // Update leaderboards when anything regarding users has been changed
            await FireFetcher.CreateLeaderboard();
        }

        public async Task<bool> SetNickname(ulong UserID, string Nickname)
        {
            int UserIndex = -1;

            // Get index of user
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].DiscordID == UserID)
                {
                    UserIndex = i;
                    break;
                }
            }

            // If user has account linked
            if (UserIndex >= 0)
            {
                Users[UserIndex].Nickname = Nickname;

                // Write to Users file
                JsonInterface JsonInterface = new();
                JsonInterface.WriteToJson(Users, UsersFilePath);

                // Update leaderboards when anything regarding users has been changed
                await FireFetcher.CreateLeaderboard();
            }

            return UserIndex >= 0;
        }

        public async Task<bool> UnlinkAccount(ulong UserID)
        {
            int UserIndex = -1;

            // Get index of user
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].DiscordID == UserID)
                {
                    UserIndex = i;
                    break;
                }
            }

            // If user has account linked
            if (UserIndex >= 0)
            {
                Users.RemoveAt(UserIndex);

                // Write to Users file
                JsonInterface JsonInterface = new();
                JsonInterface.WriteToJson(Users, UsersFilePath);

                // Update leaderboards when anything regarding users has been changed
                await FireFetcher.CreateLeaderboard();
            }

            return UserIndex >= 0;
        }

        public string[] GetDiscordUsernames()
        {
            string[] ReturnData = new string[Users.Count];
            
            for (int i = 0; i < Users.Count; i++)
            {
                ReturnData[i] = Users[i].DiscordName;
            }

            return ReturnData;
        }
    }
}
