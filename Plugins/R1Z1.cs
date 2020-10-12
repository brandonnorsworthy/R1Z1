using System;
using UnityEngine;
using Rust;
using Oxide.Core.Plugins;
using Oxide.Core.Configuration;
using Oxide.Game.Rust.Cui;
using GameTips;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("R1Z1", "Cog", "0.0.1")]
    class R1Z1 : RustPlugin
    {
        #region Loadup

        [PluginReference]
        Plugin Clans;

        //BR
        static R1Z1 _instance;
        static List<ulong> playerList = new List<ulong>();
        static List<ulong> playerAliveList = new List<ulong>();

        void Loaded()
        {
            _instance = this;
            LoadVariables();
            permission.RegisterPermission("r1z1.use", this);
            permission.RegisterPermission("r1z1.admin", this);
        }

        #endregion

        #region Configuration
        List<int> zoneNumbers = new List<int>() { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
        List<char> zoneLetters = new List<char>() { 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y' };
        
        float countDownTimer = 60f;
        float zoneCountDownTimer = 180f;
        float restartTimer = 1200f;
        bool shouldStopFromStarting = true;

        //zone variables
        int zoneXIndex = 0;
        int zoneYIndex = 0;
        int currentZoneNumber = 0;

        void LoadVariables()
        {
            LoadConfigVariables();
            SaveConfig();
        }

        void LoadDefaultConfig()
        {
            Puts("Creating a new config file");
            Config.Clear();
            LoadVariables();
        }

        void LoadConfigVariables()
        {
            CheckCfgFloat("Countdown timer to start the Battle Royale:", ref countDownTimer);
            CheckCfgFloat("Time between each safe zone being declared:", ref zoneCountDownTimer);
            CheckCfgFloat("Time for the Battle Royale to Restart:", ref restartTimer);
        }

        void CheckCfgFloat(string Key, ref float var)
        {

            if (Config[Key] != null)
                var = System.Convert.ToSingle(Config[Key]);
            else
                Config[Key] = var;
        }
        #endregion

        #region R1Z1
        #region Commands
        [ConsoleCommand("startbr")]
        void cmdConsoleStartR1Z1(ConsoleSystem.Arg arg)
        {
            var player = arg.Player() ?? null;
            if (player != null)
            {
                if (permission.UserHasPermission(player.UserIDString, "r1z1.admin"))
                {
                    StartR1Z1();
                }
            }
            if (player == null)
            {
                StartR1Z1();
            }
        }

        [ConsoleCommand("togglebr")]
        void cmdConsoleToggleR1Z1(ConsoleSystem.Arg arg)
        {
            var player = arg.Player() ?? null;
            if (player != null)
            {
                if (permission.UserHasPermission(player.UserIDString, "r1z1.admin"))
                {
                    toggleR1Z1();
                }
            }
            if (player == null)
            {
                toggleR1Z1();
            }
        }
        #endregion

        #region BR Controllers
        void StartR1Z1()
        {
            if (shouldStopFromStarting == true)
            {
                PrintWarning("R1Z1-BR: Battle Royale is disabled, please ~togglebr to start...");
            }
            else
            {
                PrintWarning("R1Z1-BR: Battle Royale has been activated...");
                ConVar.Chat.Broadcast("R1Z1 is starting in: " + countDownTimer.ToString("F0") + " seconds", "R1Z1", "#FF0000");
                timer.Once(countDownTimer, () =>
                {
                    doR1Z1();
                });
            }
        }

        void toggleR1Z1()
        {
            if(shouldStopFromStarting == true)
            {
                PrintWarning("R1Z1-BR: Battle Royale has been enabled...");
                shouldStopFromStarting = false;
            }
            else
            {
                PrintWarning("R1Z1-BR: Battle Royale has been disabled...");
                shouldStopFromStarting = true;
            }
        }

        void doR1Z1()
        {
            startNewZone();
            ZoneController();
        }

        void ZoneController()
        {
            ConVar.Chat.Broadcast("R1Z1 is starting in: " + zoneCountDownTimer.ToString("F0") + " seconds", "R1Z1", "#FF0000");
            timer.Once(zoneCountDownTimer * 0.25f, () => ConVar.Chat.Broadcast("New zone in " + (zoneCountDownTimer * 0.75f).ToString("F0") + " seconds", "R1Z1", "#FF0000"));
            timer.Once(zoneCountDownTimer * 0.50f, () => ConVar.Chat.Broadcast("New zone in " + (zoneCountDownTimer * 0.50f).ToString("F0") + " seconds", "R1Z1", "#FF0000"));
            timer.Once(zoneCountDownTimer * 0.75f, () => ConVar.Chat.Broadcast("New zone in " + (zoneCountDownTimer * 0.25f).ToString("F0") + " seconds", "R1Z1", "#FF0000"));
            timer.Once(zoneCountDownTimer, () =>
            {
                PrintWarning("R1Z1: Choosing a new zone...");
                startNewZone();
                startNewZone();
            });
        }

        void startNewZone()
        {
            //8 - 4 - 1
            int max = 19;
            int zoneSize1 = 8;
            int zoneSize2 = 4;
            int zoneSize3 = 1;

            switch (currentZoneNumber)
            {
                default:
                    PrintWarning("R1Z1: zoneNumber out of bounds no zone declared...");
                    break;
                case 0:
                    zoneYIndex = UnityEngine.Random.Range(0, max - zoneSize1);
                    zoneXIndex = UnityEngine.Random.Range(0, max - zoneSize1);
                    ConVar.Chat.Broadcast("Starting zone: " + zoneLetters[zoneXIndex] + zoneNumbers[zoneYIndex] + " - " + zoneLetters[zoneXIndex + zoneSize1] + zoneNumbers[zoneYIndex + zoneSize1], "R1Z1", "#FF0000");
                    break;
                case 1:
                    //x random(index, index+redsize-bluesize-1)
                    zoneXIndex = UnityEngine.Random.Range(zoneXIndex, zoneXIndex + zoneSize1 - (zoneSize2 - 1));
                    zoneYIndex = UnityEngine.Random.Range(zoneYIndex, zoneYIndex + zoneSize1 - (zoneSize2 - 1));
                    ConVar.Chat.Broadcast("New zone: " + zoneLetters[zoneXIndex] + zoneNumbers[zoneYIndex] + " - " + zoneLetters[zoneXIndex + zoneSize2] + zoneNumbers[zoneYIndex + zoneSize2], "R1Z1", "#FF0000");
                    break;
                case 2:
                    zoneXIndex = UnityEngine.Random.Range(zoneXIndex, zoneXIndex + zoneSize2 - (zoneSize3 - 1));
                    zoneYIndex = UnityEngine.Random.Range(zoneYIndex, zoneYIndex + zoneSize2 - (zoneSize3 - 1));
                    ConVar.Chat.Broadcast("New zone: " + zoneLetters[zoneXIndex] + zoneNumbers[zoneYIndex] + " - " + zoneLetters[zoneXIndex + zoneSize3] + zoneNumbers[zoneYIndex + zoneSize3], "R1Z1", "#FF0000");
                    break;
            }
            currentZoneNumber++;

        }
        #endregion
        #endregion

        #region Skyfall
        #endregion
    }
}