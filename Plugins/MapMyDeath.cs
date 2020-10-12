using Oxide.Core;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Linq;  //tolist
using UnityEngine;  //txtanchor
using Convert = System.Convert;
using System;   //String.format

namespace Oxide.Plugins
{
    [Info("Map My Death", "BuzZ[PHOQUE]", "0.0.3")]
    [Description("Displays a mapmarker and a popup with distance on player death")]

/*======================================================================================================================= 
*
*   THANKS TO THE OXIDE/UMOD TEAM for coding quality, ideas, and time spent for the community
*
*   0.0.2   20190201    NRE code
*   0.0.3   20190517    networkto conflict vanishes
*
*=======================================================================================================================*/

    public class MapMyDeath : RustPlugin
    {
        //NO CHAT MESSAGES ON THIS VERSION
        //string Prefix = "[MMD] ";                       // CHAT PLUGIN PREFIX
        //ulong SteamIDIcon = 76561197991909498;          // SteamID FOR PLUGIN ICON

    class StoredData
    {
        public Dictionary<ulong, DeathInfo> playerdeath = new Dictionary<ulong, DeathInfo>();    
        
        public StoredData()
        {
        }
    }

    class DeathInfo
    {
        public float playerx;
        public float playery;
        public float playerz;
// timeofdeath
// lastdamage
    }

    private StoredData storedData;

    public Dictionary<ulong, MapMarkerGenericRadius> MapMarker = new Dictionary<ulong, MapMarkerGenericRadius>(); 

    bool debug = false;
    float markertime;
    bool ConfigChanged;
    string markertimeint = "2";
    int meters;
    float daradius = 3;

    void Init()
    {
        LoadVariables();
        storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
    }

#region MESSAGES

    protected override void LoadDefaultMessages()
    {
        lang.RegisterMessages(new Dictionary<string, string>
        {
            {"RespawnMsg", "A black marker shows on map where your corpse was last seen, at {0}m. away"},
        }, this, "en");

        lang.RegisterMessages(new Dictionary<string, string>
        {
            {"RespawnMsg", "Un marqueur noir indique sur votre carte le lieu de votre dernière mort, à {0}m. d'ici"},
        }, this, "fr");
    }


#endregion
#region CONFIG

    protected override void LoadDefaultConfig()
        {
            LoadVariables();
        }

        private void LoadVariables()
        {

            markertimeint = Convert.ToString(GetConfig("Map Marker will show", "Value in minutes", "2"));       
            daradius = Convert.ToSingle(GetConfig("Marker Settings", "Radius", "3"));

            if (!ConfigChanged) return;
            SaveConfig();
            ConfigChanged = false;
        }

        private object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                ConfigChanged = true;
            }
            object value;
            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
                ConfigChanged = true;
            }
            return value;
        }

#endregion
#region UNLOAD = KILL MARKERS

    void Unload()
    {
        if (MapMarker != null)
        {
            foreach (var markerz in MapMarker)
            {
                if (markerz.Value != null)
                {
                    markerz.Value.Kill();
                    markerz.Value.SendUpdate();
                    if (debug) Puts($"-> UNLOAD DEATH MARKER");
                }
            }
        }                   
        Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);
    }
#endregion
#region ON DEATH
        
    void OnPlayerDie(BasePlayer player, HitInfo info)
    {
        if (player == null)
        {
            if (debug) Puts($"-> ERROR on VOID OnPlayerDie -> BasePlayer is Null");
            return;
        }
        if (MapMarker.ContainsKey(player.userID))
        {
            MapMarkerGenericRadius tokill = new MapMarkerGenericRadius();
            MapMarker.TryGetValue(player.userID, out tokill);
            if (tokill != null)
            {
                tokill.Kill();
                tokill.SendUpdate();
                if (debug) Puts($"-> UNLOAD DEATH MARKER ON NEW DEATH");
            }
        }   
        foreach(BasePlayer playerzonlinez in BasePlayer.activePlayerList.ToList())
        {
            if (player == playerzonlinez)
            {
                            
            //future
            //player.UserIDString; 
            //string playername;
            //string shortplayername;
            //shortplayername = playername.Substring(0,8);

            UpdatePos(player);

            Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

                if (debug) Puts($"-> PLAYER DEATH recorded for {player.UserIDString}");
            }
        }
    }

#endregion
#region NETWORK FILTER

        object CanNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (entity is MapMarkerGenericRadius)
            {
                MapMarkerGenericRadius entitymarker = entity.GetComponent<MapMarkerGenericRadius>();
                if (entitymarker == null || target == null) return null;
                if (MapMarker.ContainsValue(entitymarker))
                {
                    foreach (var deadmarkers in MapMarker)
                    {
                        if (deadmarkers.Value == entitymarker)
                        {
                            if (deadmarkers.Key == target.userID) return null;
                            else return false;
                        }
                    }
                }
            }
            return null;
        }

#endregion
#region UPDATE POSITION

    void UpdatePos(BasePlayer player)
    {
        float posx;
        posx = player.transform.position.x;
        float posy;
        posy = player.transform.position.y;
        float posz;
        posz = player.transform.position.z;
        float check;

        if (posx == null || posy == null || posz == null)
        {           
            if (debug) Puts($"-> DEATH Position update ERROR for {player.UserIDString}");
            return;
        }

        if (storedData.playerdeath.ContainsKey(player.userID)) storedData.playerdeath.Remove(player.userID);
        DeathInfo deadnow = new DeathInfo();
        deadnow.playerx = posx;
        deadnow.playery = posy;
        deadnow.playerz = posz;
        storedData.playerdeath.Remove(player.userID);        
        storedData.playerdeath.Add(player.userID, deadnow);
        if (debug) Puts($"-> DEATH Position updated for {player.UserIDString}");
    }
#endregion
#region GET PLAYER POSITION AND MARKER

    void GetPos(BasePlayer player)
    {
        if (storedData.playerdeath.ContainsKey(player.userID) == false)
        {
            Puts($"-> DEATH Position unknown for {player.UserIDString}");
            return;
        }  
        float posx = storedData.playerdeath[player.userID].playerx;
        float posy = storedData.playerdeath[player.userID].playery;
        float posz = storedData.playerdeath[player.userID].playerz;
        Vector3 deadly = new Vector3(posx,posy,posz);
        meters = (int)Vector3.Distance(deadly, player.transform.position);
        MapMarkerGenericRadius deathmarker = new MapMarkerGenericRadius();
        deathmarker = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", deadly) as MapMarkerGenericRadius;
        if (deathmarker == null)
        {
            if (debug) Puts("ERROR - NULL MARKER CREATION");
            return;
        }
        deathmarker.alpha = 1f;
        deathmarker.color1 = Color.black;
        deathmarker.color2 = Color.white;
        deathmarker.radius = daradius;
        MapMarker.Remove(player.userID);        
        MapMarker.Add(player.userID, deathmarker);
        if (debug) Puts($"-> DEATH MARKER prefab for {player.UserIDString} {posx} {posy} {posz}");
    }
#endregion
#region ON RESPAWN - SLEEP ENDED

    private void OnPlayerSleepEnded(BasePlayer player)
    {
        if (storedData.playerdeath.ContainsKey(player.userID))
        {
            GetPos(player);
            if (MapMarker.ContainsKey(player.userID))
            {                       
                MapMarkerGenericRadius deathmarker = new MapMarkerGenericRadius();
                MapMarker.TryGetValue(player.userID, out deathmarker);
                if (deathmarker != null)
                {
                    deathmarker.Spawn();
                    deathmarker.SendUpdate();
                    if (debug) Puts($"-> DEATH MARKER SPAWN for {player.UserIDString}");
                }
            
// CUI POPUP - INFO MESSAGE

                timer.Once(10f, () =>
                {
                    var CuiElement = new CuiElementContainer();
                    var DeadBanner = CuiElement.Add(new CuiPanel{Image ={Color = "0.5 0.5 0.5 0.75"},
                        RectTransform ={AnchorMin = "0.20 0.85",AnchorMax = "0.80 0.90"},
                        CursorEnabled = false});
                    var closeButton = new CuiButton{Button ={Close = DeadBanner,Color = "0.0 0.0 0.0 0.6"},
                        RectTransform ={AnchorMin = "0.90 0.01",AnchorMax = "0.99 0.99"},
                        Text ={Text = "X",FontSize = 20,Align = TextAnchor.MiddleCenter}
                        };
                    CuiElement.Add(closeButton, DeadBanner);
                    CuiElement.Add(new CuiLabel{Text ={Text = String.Format(lang.GetMessage("RespawnMsg", this, player.UserIDString), meters),FontSize = 18,
                        Align = TextAnchor.MiddleCenter,Color = "1.0 1.0 1.0 1"},
                        RectTransform ={AnchorMin = "0.10 0.10",   AnchorMax = "0.90 0.90"}
                        }, DeadBanner);
                        CuiHelper.AddUi(player, CuiElement);
                    timer.Once(25f, () =>
                    {
                            CuiHelper.DestroyUi(player, DeadBanner);
                    });
                });                

// KILL MARKER AFTER 2 MINUTES
                if (debug) Puts($"-> TIME IN CONF : {markertimeint}");
                markertime = Convert.ToSingle(markertimeint);
                if (debug) Puts($"-> MARKER TIME TO FLOAT FROM CONF : {markertime}");
                markertime = markertime * 60;
                if (debug) Puts($"-> MARKER TIME IN SECONDS {markertime}");
                if (markertime <= 0)
                {
                    markertime = 120;
                    if (debug) Puts($"-> ERROR :/ wrong entry value in config file.");           
                    if (debug) Puts($"-> MARKER TIME IN SECONDS {markertime}");
                }
                timer.Once(markertime, () =>
                {
                    if (deathmarker != null)
                    {
                        deathmarker.Kill();
                        deathmarker.SendUpdate();
                        storedData.playerdeath.Remove(player.userID);
                    }
                });
            }
        }
    }

#endregion
  
    }
}
