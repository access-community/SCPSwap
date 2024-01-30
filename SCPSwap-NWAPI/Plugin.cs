﻿using System;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using SCPSwap_NWAPI.Models;

// ReSharper disable UnusedMember.Local
namespace SCPSwap_NWAPI
{
    public class Plugin
    {
        [PluginConfig] 
        public Config Config;
        [PluginConfig("messages.yml")] 
        public Messages Messages;
        public static Plugin Instance { get; private set; }
        
        [PluginEntryPoint("SCPSwap", "1.3.1", "Allows SCPs to swap with other SCPs at the start of the match.", "ThijsNameIsTaken, DentyTxR and BuildBoy12")]
        void OnEnabled()
        {
            if (!Config.IsEnabled) return;
            Instance = this;
            EventManager.RegisterEvents(this);
        }
        
        [PluginReload]
        void OnReload()
        {
            ValidSwaps.Refresh();
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        void OnPlayerChangeRole(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason reason)
        {
            if (player == null || reason == RoleChangeReason.Destroyed) return;
            if (oldRole.Team == Team.SCPs) return;
            if (newRole.GetTeam() == Team.SCPs && Round.Duration < TimeSpan.FromSeconds(Config.SwapTimeout) && !Config.BlacklistedScps.Contains(newRole))
                player.SendBroadcast(Messages.StartMessage.Message, Messages.StartMessage.Duration);
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        void OnRoundRestart()
        {
            Swap.Clear();
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void OnWaitingForPlayers()
        {
            ValidSwaps.Refresh();
        }
    }
}