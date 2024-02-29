﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Epic.OnlineServices.Auth;
using HarmonyLib;
using Landfall.Network;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace CitrusLib
{

    


    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "citrusbird.tabg.citruslib";
        public const string pluginName = "Citrus Lib";
        public const string pluginVersion = "0.3";

        public void Awake()
        {
            Citrus.log.Log(string.Format("{0} {1} Loading!", pluginName, pluginVersion));
            Harmony harmony = new Harmony(pluginGuid);


            harmony.PatchAll();


            Citrus.AddCommand("team", delegate (string[] prms, TABGPlayerServer player)
             {
                 switch (prms[0])
                 {
                     case "get":
                         if (prms.Length != 2)
                         {
                             Citrus.SelfParrot(player, "team get <name>");
                             return;
                         }
                         TABGPlayerServer find = null;
                         if (!Citrus.PlayerChatSearch(prms[1], out find))
                         {
                             if (find != null)
                             {
                                 Citrus.SelfParrot(player, "multiple results for: " + prms[1]);
                             }
                             else
                             {
                                 Citrus.SelfParrot(player, "no results for: " + prms[1]);
                             }
                             return;
                         }

                         Citrus.SelfParrot(player, "groupIndex for player " + find.PlayerName + " is:" + find.GroupIndex);

                         break;
                     case "set":
                         if (prms.Length != 3)
                         {
                             Citrus.SelfParrot(player, "team set <name> <index>");
                             return;
                         }
                         TABGPlayerServer setFind = null;
                         if (!Citrus.PlayerChatSearch(prms[1], out setFind))
                         {
                             if (setFind != null)
                             {
                                 Citrus.SelfParrot(player, "multiple results for: " + prms[1]);
                             }
                             else
                             {
                                 Citrus.SelfParrot(player, "no results for: " + prms[1]);
                             }
                             return;
                         }
                         byte ind;
                         if (!byte.TryParse(prms[2], out ind))
                         {
                             Citrus.SelfParrot(player, "could not parse group index: " + prms[2]);
                             return;
                         }

                         Citrus.SetTeam(setFind, ind);
                         Citrus.SelfParrot(player, "set player " + setFind.PlayerName + " to team " + prms[2]);
                         break;
                     default:
                         Citrus.SelfParrot(player, "unknown parameter: " + prms[0]);
                         break;
                 }
             }, true,
            pluginName, "Changes or queries a player's team", "<get|set> <player> [index](if setting)",2);

            Citrus.AddCommand("goto", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "goto <name>");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                Citrus.log.Log(string.Format("taking player {0} to {1}", player.PlayerName, find.PlayerName));
                Citrus.Teleport(player, find.PlayerPosition);

            }, true,
            pluginName, "Brings the command user to the specified player", "<player>",2);

            Citrus.AddCommand("bring", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "bring <name>");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                Citrus.log.Log(string.Format("taking player {0} to {1}", find.PlayerName, player.PlayerName));
                Citrus.Teleport(find, player.PlayerPosition);

            }, true,
            pluginName, "Brings a player to the command user", "<player>",2);

            Citrus.AddCommand("perm-get", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "perm-get <name>");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }

                if (Citrus.permList == null)
                {
                    Citrus.log.Log(string.Format("Perm list doesn't exist!"));
                    return;
                }
                PermList.PermPlayer ply = Citrus.permList.players.Find(p => p.epic == (string)find.EpicUserName);
                int perm = 1;
                bool admin = false;
                if (ply != null)
                {
                    perm = ply.permlevel;
                    admin = ply.admin;
                }

                string text = string.Format("{0} has perm level {1} and is {2}an admin", find.PlayerName, perm, admin ? "" : "NOT");

                Citrus.log.Log(text);
                Citrus.SelfParrot(player, text);
                //Citrus.Teleport(find, player.PlayerPosition);

            }, true,
           pluginName, "Gets the permission status of the player", "<player>", 1);


            Citrus.AddCommand("perm-set", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 2)
                {
                    Citrus.SelfParrot(player, "perm-set <name> [level]");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }

                if (Citrus.permList == null)
                {
                    Citrus.log.Log(string.Format("Perm list doesn't exist!"));
                    return;
                }
                int perm = 1;
                if (!int.TryParse(prms[2], out perm))
                {
                    Citrus.log.Log(string.Format("Invalid integer {0}", prms[2]));
                    return;
                }

                PermList.PermPlayer ply = Citrus.permList.players.Find(p => p.epic == (string)find.EpicUserName);

                if (ply != null)
                {
                    ply.permlevel = perm;
                }
                else
                {
                    Citrus.permList.players.Add(new PermList.PermPlayer
                    {
                        name = find.PlayerName,
                        epic = find.EpicUserName,
                        permlevel = perm,
                        admin = false
                    });

                }

                Citrus.WriteNewPermList();


                //Citrus.Teleport(find, player.PlayerPosition);

            }, true,
           pluginName, "SETS the permission status of the player!", "<player>", 4);

            Citrus.AddCommand("admin", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 2)
                {
                    Citrus.SelfParrot(player, "admin <name> [level]");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }

                if (Citrus.permList == null)
                {
                    Citrus.log.Log(string.Format("Perm list doesn't exist!"));
                    return;
                }
                bool adminn = false;
                if (!bool.TryParse(prms[2], out adminn))
                {
                    Citrus.log.Log(string.Format("Invalid bool {0}", prms[2]));
                    return;
                }

                PermList.PermPlayer ply = Citrus.permList.players.Find(p => p.epic == (string)find.EpicUserName);

                if (ply != null)
                {
                    ply.admin = adminn;
                }
                else
                {
                    Citrus.permList.players.Add(new PermList.PermPlayer
                    {
                        name = find.PlayerName,
                        epic = find.EpicUserName,
                        permlevel = 1,
                        admin = adminn
                    });

                }

                Citrus.WriteNewPermList();


                //Citrus.Teleport(find, player.PlayerPosition);

            }, true,
           pluginName, "SETS the admin status of the player!", "<player>", 4);

            Citrus.AddCommand("start", delegate (string[] prms, TABGPlayerServer player)
             {
                 float time = 30;
                 if (prms.Length > 0)
                 {
                     if (prms.Length != 1)
                     {
                         Citrus.SelfParrot(player, "start <time(optional>");
                         return;
                     }
                     if (!float.TryParse(prms[0], out time))
                     {
                         Citrus.SelfParrot(player, "invalid time: " + prms[0]);
                         return;
                     }


                 }
                 Citrus.World.GameRoomReference.StartCountDown(time);
             }, true,
            pluginName, "Starts the countdown timer", "[time]",1);

            Citrus.AddCommand("list", delegate (string[] prms, TABGPlayerServer player)
            {
                string result = "LIST INFO\n";



                string players = "";
                string playerRefs = "";
                string teams = "";
                string teamsLite = ""; //teams without empty teams listed

                players += "PLAYERS:";
                foreach (TABGPlayerServer pl in Citrus.World.GameRoomReference.Players)
                {
                    players += "\n" + pl.PlayerName + " ind:" + pl.PlayerIndex + " team:" + pl.GroupIndex + " (original team is " + (byte)(Citrus.players.Find(p => p.player.PlayerIndex == pl.PlayerIndex).data["originalTeam"]) + ") epic:" + pl.EpicUserName;
                }


                playerRefs += "PLAYERREFS:";
                foreach (PlayerRef pl in Citrus.players)
                {
                    playerRefs += "\n" + pl.player.PlayerName + " ind:" + pl.player.PlayerIndex + " team:" + pl.player.GroupIndex;
                }

                teams += "TEAM REFS:\n";
                foreach (PlayerTeam team in Citrus.teams)
                {

                    if (team.players.Count > 0)
                    {
                        teams += "Team " + team.groupIndex + " : ";
                        teamsLite += "Team " + team.groupIndex + " : ";
                        foreach (PlayerRef p in team.players)
                        {
                            teams += p.player.PlayerName + ", ";
                            teamsLite += p.player.PlayerName + ", ";
                        }
                        teams += "\n";
                        teamsLite += "\n";
                    }
                    else
                    {
                        teams += "Team " + team.groupIndex + " : (no players)\n";
                    }


                }

                string req = "all";
                if (prms.Length != 0)
                {
                    req = prms[0];
                }
                switch (req)
                {
                    case "teams":
                    case "team":
                    case "t":
                        req = "teams";
                        result += "\n" + teams;
                        Citrus.SelfParrot(player, teamsLite);
                        break;
                    case "players":
                    case "player":
                    case "p":
                        req = "players";
                        result += "\n" + players;
                        Citrus.SelfParrot(player, players);
                        break;
                    case "playerrefs":
                        req = "playerrefs";
                        result += "\n" + playerRefs;
                        Citrus.SelfParrot(player, playerRefs);
                        break;
                    default:
                        req = "all";
                        result += "\n" + players + "\n" + teams + "\n" + playerRefs;
                        Citrus.SelfParrot(player, "lists posted to console.");
                        break;
                }
                result = req.ToUpper() + " " + result;


                Citrus.log.Log(result);
            }, true,
            pluginName, "lists different things in the console.", "<teams|players|playerrefs|all>",2);

            Citrus.AddCommand("id", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "id <name>");
                    return;
                }
                if (!Citrus.PlayerWithName(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                string message = string.Format("id for {0} is {1}", find.PlayerName, find.PlayerIndex);

                Citrus.SelfParrot(player, message);
                Citrus.log.Log(message);



            }, true,
            pluginName, "Gets the ID of a player with the given name.", "<name>");

            Citrus.AddCommand("send", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                TABGPlayerServer find2 = null;
                if (prms.Length != 2)
                {
                    Citrus.SelfParrot(player, "send <name> <name>");
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                if (!Citrus.PlayerChatSearch(prms[1], out find2))
                {
                    if (find2 != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[1]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[1]);
                    }
                    return;
                }
                Citrus.log.Log(string.Format("taking player {0} to {1}", find.PlayerName, find2.PlayerName));
                Citrus.Teleport(find, find2.PlayerPosition);

            }, true,
            pluginName, "Sends the first player to the second player", "<player> <player>",2);

            Citrus.AddCommand("name", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "name <id>");
                    return;
                }
                byte ind;
                if (!byte.TryParse(prms[0], out ind))
                {
                    Citrus.SelfParrot(player, "name <id>");
                }

                if (!Citrus.PlayerWithIndex(ind, out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                string message = string.Format("name for {0} is {1}", find.PlayerIndex, find.PlayerName);

                Citrus.SelfParrot(player, message);
                Citrus.log.Log(message);



            }, true,
            pluginName, "Gets the NAME of a player with the given byte playerindex.", "[id]");

            Citrus.AddCommand("epic", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = null;
                if (prms.Length != 1)
                {
                    Citrus.SelfParrot(player, "name <id>");
                    return;
                }


                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }
                string message = string.Format("epicid for {0} is {1}", find.PlayerName, find.EpicUserName);

                Citrus.SelfParrot(player, message);
                Citrus.log.Log(message);



            }, true,
            pluginName, "Gets the epic id of a player with the given name or index", "<name>");

            
            Citrus.AddCommand("give", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = player;
                if (prms.Length < 1 | prms.Length > 2)
                {
                    Citrus.SelfParrot(player, "give <itemID> <amount>");
                    return;
                }

                int amt = 1;
                int typ;


                if (prms.Length == 2)
                {
                    if (!int.TryParse(prms[1], out amt))
                    {
                        Citrus.SelfParrot(player, "invalid amount");
                        return;
                    }
                }

                if (!int.TryParse(prms[0],out typ))
                {
                    Citrus.SelfParrot(player, "invalid item type");
                    return;
                }

                LootPack lp = new LootPack();
                lp.AddLoot(typ,amt);
                Citrus.GiveLoot(player,lp);

                //Citrus.SelfParrot(player, message);
                //Citrus.log.Log(message);



            }, true,
            pluginName, "gives the user an item with an optional amount", "[id] [amount(optional)]",2);

            Citrus.AddCommand("gift", delegate (string[] prms, TABGPlayerServer player)
            {
                TABGPlayerServer find = player;
                if (prms.Length < 1 | prms.Length > 3)
                {
                    Citrus.SelfParrot(player, "give <player> <itemID> <amount>");
                    return;
                }


                if (!Citrus.PlayerChatSearch(prms[0], out find))
                {
                    if (find != null)
                    {
                        Citrus.SelfParrot(player, "multiple results for: " + prms[0]);
                    }
                    else
                    {
                        Citrus.SelfParrot(player, "no results for: " + prms[0]);
                    }
                    return;
                }

                int amt = 1;
                int typ;


                if (prms.Length == 3)
                {
                    if (!int.TryParse(prms[2], out amt))
                    {
                        Citrus.SelfParrot(player, "invalid amount");
                        return;
                    }
                }

                if (!int.TryParse(prms[1], out typ))
                {
                    Citrus.SelfParrot(player, "invalid item type");
                    return;
                }

                LootPack lp = new LootPack();
                lp.AddLoot(typ, amt);
                Citrus.GiveLoot(player, lp);

                //Citrus.SelfParrot(player, message);
                //Citrus.log.Log(message);



            }, true,
            pluginName, "gives the user an item with an optional amount", "<player> [id] [amount(optional)]",2);



            Citrus.ExtraSettings.AddSetting(new GameSetting
            {
                name = "Suppress Landlog",
                value = false.ToString(),
                description = "Prevents most landfall debug messages from being written."
            });

            
            Citrus.ExtraSettings.AddSetting(new GameSetting
            {
                name = "AdminFileLocation",
                value = "",
                description = "filepath to a FOLDER where you want the PlayerPerms setting to be stored. Usefull when hosting multiple servers on the same computer leave blank to edit locally."
            });

        }


    }


    //does stuff early but AFTER other mods have registered their citlib chat commands
    [HarmonyPatch(typeof(GameRoom), "InitActions")]
    class StartPatch
    {
        static void Prefix()
        {
            Citrus.WriteAllCommands();
            Citrus.ExtraSettings.ReadSettings();
            GuestBook.LoadGuestBook();

            GameSetting supp;
            if (Citrus.ExtraSettings.TryGetSetting("Suppress Landlog", out supp))
            {
                bool sup = false;
                if (Boolean.TryParse(supp.value, out sup))
                {
                    Citrus.landLogSupressed = sup;
                }
            }
            else
            {
                Citrus.log.Log("Log supress setting is missing?");
            }


            GameSetting adminLoc;
            if (Citrus.ExtraSettings.TryGetSetting("AdminFileLocation", out adminLoc))
            {
                if (adminLoc.value != "")
                {
                    Citrus.log.Log("Loading player perms at " + adminLoc.value);
                }
                Citrus.LoadPermSettings(adminLoc.value + "/PlayerPerms");
            }

        }

    }


        //grabs the Network object when the game is hosted
    [HarmonyPatch(typeof(UnityTransportServer), "ActuallyHost")]
    class HostPatch
    {
        static void Prefix(ref NetworkSettings networkSettings, bool ___m_isHosting)
        {
            if (___m_isHosting)
            {
                
                //Citrus.WriteAllCommands();
                //not the best place to do this but WHO CARES

                
                //Citrus.log.Log("trying to increase buffer sizes tremendously!!");
                //networkSettings = networkSettings.WithBaselibNetworkInterfaceParameters(2048, 2048, 4000U);
            }
            
        }

        static void Postfix(ref NetworkDriver ___m_ServerHandler)
        {
            Citrus.Network = ___m_ServerHandler;
        }
    }




   // [HarmonyPatch(typeof(NetworkDriver), nameof(NetworkDriver.Create), typeof(NetworkSettings))]
    class NetworkPatch
    {
        static void Prefix(ref NetworkSettings settings)
        {
            //settings = settings.WithBaselibNetworkInterfaceParameters(2048, 2048, 4000U);
        }
    }

    //reduces multiple-recipient packages into seperate ones before trying to send to prevent package duplication
    //... and to make my bot mod work more seamlessly
    
    [HarmonyPatch(typeof(ServerClient), nameof(ServerClient.SendMessageToClients), new Type[] { typeof(EventCode), typeof(byte[]), typeof(byte[]), typeof(bool), typeof(bool) })]
    class MessagePatch
    {

        static bool Prefix(EventCode opCode, byte[] buffer, byte[] recipents, bool reliable, bool alsoSendToTeamates)
        {
            if(recipents.Length!=1)
            {
                foreach (byte b in recipents)
                {
                    Citrus.World.SendMessageToClients(opCode, buffer, b, reliable, false);//haha! saved the world!
                }
                return false;
            }
            else
            {
                if (recipents.Length == 0)
                {
                    Citrus.log.LogError("Sending message to no recipient???");
                    return false;
                }
                if (recipents[0] == byte.MaxValue)
                {
                    foreach (TABGPlayerServer p in Citrus.World.GameRoomReference.Players)
                    {
                        Citrus.World.SendMessageToClients(opCode, buffer, p.PlayerIndex, reliable, false);
                    }
                    return false;
                }

            }
            return true;
            
        }

    }


    /*
    [HarmonyPatch(typeof(UnityTransportServer),nameof(UnityTransportServer.Update))]
    class BufferedPackagePatch
    {
        static bool Prefix(UnityTransportServer __instance, ref Queue<UnityTransportServer.BufferedPackage> ___m_BufferedPackages)
        {
           
        }
    }*/


    [HarmonyPatch(typeof(UnityTransportServer), "Update")]
        class UpdatePatch
        {
            static void Prefix(ref BidirectionalDictionary<byte,NetworkConnection> ___m_playerIDToConnection, UnityTransportServer __instance, ref NativeList<NetworkConnection> ___m_connections)
            {
                //apparent naitivelists are special and cannot be compared to null?
                if (Citrus.kicklist != null & ___m_playerIDToConnection != null)
                {
                    while (Citrus.kicklist.Count != 0)
                    {
                        byte pb = Citrus.kicklist.Dequeue();
                        NetworkConnection nc;
                        if (!___m_playerIDToConnection.TryGetValue(pb, out nc))
                        {
                            Citrus.log.LogError(string.Format("Failed to find connection fo player ID: {0}", pb));
                            continue;
                        }
                        //please work please work please work
                        int j = ___m_connections.IndexOf(nc);
                        ___m_playerIDToConnection.Remove(___m_connections[j]);
                        Citrus.log.Log(string.Format("Client: {0} disconnected from server", ___m_connections[j].InternalId));
                        ___m_connections[j] = default(NetworkConnection);
                        TABGPlayerServer tabgplayerServer = Citrus.World.GameRoomReference.FindPlayer(pb);
                        if (tabgplayerServer != null)
                        {
                            Citrus.World.HandlePlayerLeave(tabgplayerServer);
                        }



                    }
                }
                /*
                Citrus.landLogSupressed = true;
                Citrus.FixQueue(ref ___m_BufferedPackages);
                int before = Citrus.queue.Count();
                if (before > 0)
                {
                    foreach (UnityTransportServer.BufferedPackage p in Citrus.queue)
                    {
                        Citrus.World.SendMessageToClients(p.EventCode, p.Data, p.RecipentIndicies, true);
                    }
                    int after = Citrus.queue.Count();
                    Citrus.queue = new List<UnityTransportServer.BufferedPackage>();
                    Citrus.FixQueue(ref ___m_BufferedPackages);
                    Citrus.log.Log(string.Format("Sent {0} buffered packages, {1} still remaining",(before - after),after));
                }
                Citrus.landLogSupressed = false;
                UnityTransportServer.BufferedPackage bufferedPackage;
                if (___m_BufferedPackages != null & ___m_BufferedPackages.Count > 0)
                {

                    //Citrus.World.GameRoomReference.CurrentGameSettings;
                    int i = ___m_BufferedPackages.Count;

                    int buff = i;

                    while (i > 0)
                    {
                        i--;
                        //might work, i've never used queues before

                        bufferedPackage = ___m_BufferedPackages.Peek();
                        //Citrus.log.Log("Buffer Package type: "+((EventCode)(bufferedPackage.EventCode)).ToString());

                        //Citrus.Network.GetPipelineBuffers(Citrus.Network.pi)



                        List<byte> recip = new List<byte>();
                        foreach(byte by in bufferedPackage.RecipentIndicies)
                        {
                            if (Citrus.World.GameRoomReference.FindPlayer(by) != null)
                            {
                                recip.Add(by);
                            }
                        }
                        if (recip.Count != 0)
                        {
                            __instance.SendMessageToClients(bufferedPackage.EventCode, bufferedPackage.Data, recip.ToArray(), true, false);
                        }



                    }
                    Citrus.log.Log(string.Format("tried going through {0} packages, there are now {1} remaining in the queue...", buff, ___m_BufferedPackages.Count));

                }*/

                //return true;

            }


            static void Postfix(ref BidirectionalDictionary<byte, NetworkConnection> ___m_playerIDToConnection, ref NativeList<NetworkConnection> ___m_connections)
            {
            }
        }

        

    //prevents players from setting their gear every time they spawn. allows opposing-clientsided gear changing
    [HarmonyPatch(typeof(GearChangeCommand),nameof(GearChangeCommand.Run))]
    class GearPatch
    {
        static bool Prefix(byte[] msgData, ServerClient world)
        {
            byte index;
            using (MemoryStream memoryStream = new MemoryStream(msgData))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    index = binaryReader.ReadByte();
                    /*
                    int num = binaryReader.ReadInt32();
                    array = new int[num];
                    for (int i = 0; i < num; i++)
                    {
                        array[i] = binaryReader.ReadInt32();
                    }*/
                }
            }
            TABGPlayerServer tabgplayerServer = world.GameRoomReference.FindPlayer(index);
            if (tabgplayerServer == null || tabgplayerServer.GearData.Length!=0)
            {
                return false;
            }
            return true;

        }
    }




    [HarmonyPatch(typeof(RoomInitRequestCommand), "OnVerifiesEpicToken")]
    class VerifyPatch
    {
        //[HarmonyAfter("citrusbird.tabg.modTools")]
        static void Postfix(ref VerifyIdTokenCallbackInfo data)
        {


            TABGPlayerServer tabgplayerServer = data.ClientData as TABGPlayerServer;

            if (tabgplayerServer != null & tabgplayerServer.EpicUserName!=null)
            {
                GuestBook.SignGuestBook(tabgplayerServer);
            }
        }
    }

    //creates player references and team references
    [HarmonyPatch(typeof(GameRoom), nameof(GameRoom.AddPlayer))]
    class AddPlayerPatch
    {
        static void Postfix(TABGPlayerServer p, bool wantsToBeAlone)
        {
            
            if(Citrus.players.Find(pr => pr.player.PlayerIndex == p.PlayerIndex)!=null)
            {
                return;
            }

            PlayerRef pRef = new PlayerRef(p);

            if(Citrus.players == null)
            {
                Citrus.log.Log("player list is null?");
                Citrus.players = new List<PlayerRef>();
            }

            Citrus.players.Add(pRef);

            PlayerTeam myTeam = Citrus.teams.Find(t => t.groupIndex == p.GroupIndex);

            if(myTeam == null)
            {
                myTeam = new PlayerTeam(p.GroupIndex);
                Citrus.teams.Add(myTeam);
            }
            myTeam.players.Add(pRef);

            //GuestBook.SignGuestBook(p);


            //im pretty confused at this point
            //Citruslib.CitrusLib.AllyTo(p, CitrusLib.World.GameRoomReference.CurrentGameStats.GetTeam(p.GroupIndex).PlayersInTeam[0]);

            //for now im not doing that as, also for now, ill just test with games starting as solos only so this issue shouldnt be present...

        }

    }

    //removes player (but not team reference)
    [HarmonyPatch(typeof(GameRoom), nameof(GameRoom.RemovePlayer))]
    class RemovePlayerPatch
    {
        static void Prefix(TABGPlayerServer p)
        {
            if (p == null)
            {
                return;
            }

            PlayerRef player = Citrus.players.Find(pl => pl.player == p);

            if (player == null) return;

            PlayerTeam team = Citrus.teams.Find(t => t.players.Contains(player));
            

            if (team != null)
            {
                team.players.Remove(player);
                return;
            }
            Citrus.players.Remove(player);
            
            
            


            //going a little bit insane

        }

    }

    //praying
    [HarmonyPatch(typeof(LandLog),nameof(LandLog.Log), new Type[] { typeof(string), typeof(object)})]
    class LogPatch
    {
        static bool Prefix(string logMessage, object context)
        {
            Citrus.landLog.Log(logMessage);
            return false;
        }
    }

    [HarmonyPatch(typeof(LandLog), nameof(LandLog.LogError))]
    class ErrPatch
    {
        static bool Prefix(string logMessage, object context)
        {
            Citrus.landLog.LogError(logMessage);
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatMessageCommand), nameof(ChatMessageCommand.Run))]
    class ChatPatch
    {
        static bool Prefix(byte[] msgData, ServerClient world, byte sender)
        {
            
            TABGPlayerServer player = Citrus.World.GameRoomReference.FindPlayer(sender);
            if(player == null)
            {
                return false;
            }
            

            using (MemoryStream memoryStream = new MemoryStream(msgData))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    byte index = binaryReader.ReadByte();
                    byte count = binaryReader.ReadByte();
                    string message = Encoding.Unicode.GetString(binaryReader.ReadBytes((int)count));

                    string[] prms = message.Split(' ');

                    int i = 0;
                    foreach(string s in prms)
                    {
                        prms[i] = s.ToLower();
                        i++;
                    }


                    if(prms[0].StartsWith("/"))
                    {
                        //runs command and doesnt say it in chat
                        List<string> prmsReal = prms.ToList();
                        prmsReal.RemoveAt(0);
                        Citrus.RunCommand(prms[0].Replace("/",""), prmsReal.ToArray(), player);
                        return true;
                    }
                }
            }
            return true;
        }
    }
}