using JailTime2.Core;
using JailTime2.Core.Manager;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Net.Sockets;
using UnityEngine;

namespace JailTime2
{
    public class JailTimePlugin : RocketPlugin<Configuration>
    {
        public static JailTimePlugin Instance;

        public Prison Prison;

        #region Load/Unload
        protected override void Load()
        {
            Instance = this;
            Prison = new Prison();

            UnturnedPlayerEvents.OnPlayerDeath += onPlayerDeath;
            U.Events.OnPlayerConnected += onPlayerConnected;
        }
        

        protected override void Unload()
        {
            Instance = null;

            UnturnedPlayerEvents.OnPlayerDeath -= onPlayerDeath;
            U.Events.OnPlayerConnected -= onPlayerConnected;
        }
        #endregion

        #region Translate
        public override TranslationList DefaultTranslations => new TranslationList()
         {
             {"jail.syntax", "Try this: /jail [playername] [imprisonment time]"},
             {"jail.player.not.found", "Player {0} not found, please try again." },
             {"jail.player.self", "You can't land yourself." },
             {"jail.player.immune", "Player {0} is immune, you cannot jail him." },
             {"jail.player.arrested", "Player {0} has already been arrested, he has {1} c left." }, // {1} how much time is left in the jail
             {"jail.player.is.arrested", "You cannot arrest as you are arrested." },
             {"jail.successful.arrested", "You have successfully arrested player {0} for {1} seconds." },
             {"jail.player.arrested.message", "You were arrested by player {0} for {1} seconds." },



             {"unjail.player.syntax", "Try this: /unjail [playername]"},
             {"unjail.player.not.found", "Player {0} not found." },
             {"unjail.self", "You can't get yourself out of jail." },
             {"unjail.player.is.not.arrested", "Player {0} has not been arrested." },
             {"unjail.successful.unnarrested", "Player {0} has been successfully released from prison." },
             {"unjail.successful.player.unnarrested", "Player {0} freed you from prison." },



             {"addcell.syntax", "Try this: /addcell | /addcell [id] [X] [Y] [Z]"},
             {"addcell.created.auto", "A new cell was created in the place where you are standing. {0}"}, // {1} POSITION WHERE THE CAGE IS DELIVERED
             {"addcell.created", "You have successfully created a new cell. (X ({0}), Y ({1}), Z ({2}))"},
             {"addcell.contains", "Cell {0} already exists." },



             {"addcell.info", "/addcell (adding at the current position) | / addcell [X] [Y] [Z] - adding a cell"},
             {"removecell.info", "/removecell [id] - remove a cell by number"},
             {"jail.info", "/jail [player name] [imprisonment time] - jail the player for a time"},
             {"unjail.info", "/unjail [playername] - exclude a player from jail"},
             {"currentposition.info", "/position - shows the current position (can help when creating a cell)"},
             {"cells.info", "/cells - shows all created cells"},
             {"handcuffs.info", "/handcuff (where the player is looking) | /handcuff [player's name] - put handcuffs on the player"},
             {"unhandcuffs.info", "/unhandcuff (where the player is looking) | /handcuff [player's name] - remove the handcuffs from the player"},
             {"jailtime.info", "/jailtime (will work on itself) | /jailtime [player name] - see how much time is left to sit"},



             {"removecell.syntax", "/removecell [id]"},
             {"removecell.contains", "Cell number {0} does not exist." },
             {"removecell.removed", "Cell number {0} has been successfully removed." },



             
             {"cells", "Cell number: {0}, position: (X: {1}, Y: {2}, Z: {3})"},



             {"handcuff.ray.player.not.found", "You are not looking at the player, please try again." },
             {"handcuff.player.not.found", "Player {0} not found." },
             {"handcuff.self", "You cannot handcuff yourself." },
             {"handcuff.immune", "You cannot handcuff this player, he is immune." },
             {"handcuff.successful", "You have successfully handcuffed player {0}." },
             {"handcuff.successful.to.player", "Player {0} has handcuffed you." },



             {"unhandcuff.ray.player.not.found", "You are not looking at the player, please try again." },
             {"unhandcuff.player.not.found", "Player {0} not found." },
             {"unhandcuff.self", "You cannot remove the handcuffs yourself." },
             {"unhandcuff.successful", "You have successfully uncuffed the player you are looking at." }, // {0} player name
             {"unhandcuff.successful.to.player", "Player {0} has removed your handcuffs." },



             {"jailtime.not.jailed", "You are not in jail."},
             {"jailtime.not.jailed.to.player", "Player {0} is not in jail."},
             {"jailtime.successful", "You have {0} seconds left."},
             {"jailtime.successful.to.player", "Player {0} has {1} seconds left."},
             {"jailtime.player.not.found", "Player {0} not found."},



             {"arrested.suicide", "You're still under arrest, you can't get out of jail, so we brought you back." },
         };
        #endregion

        #region Events
        private void onPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (Prison.IsPlayerContains(player.CSteamID))
            {
                Core.Player prisoner = Prison.GetPlayerBySteamId(player.CSteamID);
                Cell cell = Prison.GetCellById(prisoner.CellId);

                player.Player.teleportToLocation(cell.Position.GetVector3(), player.Rotation);

                UnturnedChat.Say(player, $"{Translate("arrested.suicide")}", Color.red);
            }
        }
        private void onPlayerConnected(UnturnedPlayer player)
        {
            if (Instance.Configuration.Instance.BanArrestedOnReconnect)
            {
                if (Prison.IsPlayerContains(player.CSteamID))
                {
                    player.Ban($"{Instance.Configuration.Instance.BanArrestedReasonOnReconnect}", Instance.Configuration.Instance.BanDurationOnReconnect);
                }
            }
        }
        #endregion

        #region FixedUpdate
        public void FixedUpdate()
        {
            foreach (Core.Player prisoner in Instance.Prison.GetPrisoners())
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(prisoner.SteamId);
                if (Vector3.Distance(player.Position, Prison.GetCellPositionById(prisoner.CellId).GetVector3()) > Instance.Configuration.Instance.WalkDistance)
                {
                    player.Player.teleportToLocation(Prison.GetCellPositionById(prisoner.CellId).GetVector3(), player.Rotation);
                } // Система возвращения игрока обратно если он ушел далеко от место тюрьмы

                if ((DateTime.Now - prisoner.JailTime).TotalSeconds >= prisoner.ArrestDuration)
                {
                    Instance.Prison.UnArrestPlayer(prisoner.SteamId);
                    Instance.Prison.TakeOffHandcuffsFromPlayer(prisoner.SteamId);
                } // Система освобождения игрока из тюрьмы!
            }
        }
        #endregion

    }
}
