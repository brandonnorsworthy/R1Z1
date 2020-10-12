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

        static R1Z1 _instance;
        static List<ulong> skyfallplayerlist = new List<ulong>();
        static List<ulong> isParachuting = new List<ulong>();
        static List<ulong> cooldownlist = new List<ulong>();


        void Loaded()
        {
            _instance = this;
            LoadVariables();
            LoadMessages();
            permission.RegisterPermission("skyfall.use", this);
            permission.RegisterPermission("skyfall.localrespawn", this);
            permission.RegisterPermission("skyfall.admin", this);
            layerMask = (1 << 29);
            layerMask |= (1 << 18);
            layerMask = ~layerMask;
        }

        #endregion

        #region Configuration

        float countDownTimer = 60f;
        float zoneCountDownTimer = 60f;
        float restartTimer = 300f;

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
            CheckCfgFloat("Time between each safe zone being declared:", ref countDownTimer);
            CheckCfgFloat("Time for the Battle Royale to Restart:", ref countDownTimer);
        }

        void CheckCfgFloat(string Key, ref float var)
        {

            if (Config[Key] != null)
                var = System.Convert.ToSingle(Config[Key]);
            else
                Config[Key] = var;
        }
        #endregion

        #region Commands
        [ConsoleCommand("startR1Z1")]
        void cmdConsoleStartR1Z1(ConsoleSystem.Arg arg)
        {
            var player = arg.Player() ?? null;
            if (player != null)
            {
                if (permission.UserHasPermission(player.UserIDString, "skyfall.admin"))
                {
                    ActivateChaosDrop();
                }
            }
            if (player == null)
            {
                ActivateChaosDrop();
            }
        }
        #endregion

        #region BR Controllers
        void StartR1Z1()
        {

        }

        void ZoneController()
        {

        }
        #endregion
    }
}