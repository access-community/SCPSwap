using System;
using CommandSystem;
using PluginAPI.Core;
using SCPSwap_NWAPI.Models;

namespace SCPSwap_NWAPI.Commands
{
    public class Decline : ICommand
    {
        public string Command { get; set; } = "decline";
        
        public string[] Aliases { get; set; } = { "no", "d" };
        
        public string Description { get; set; } = "Rejects an active swap request.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if (playerSender == null)
            {
                response = Plugin.Instance.Messages.ShouldRunFromGame.Message;
                return false;
            }

            Swap swap = Swap.FromReceiver(playerSender);
            if (swap == null)
            {
                response = Plugin.Instance.Messages.NoSwapRequest.Message;
                return false;
            }

            swap.Decline();
            response = Plugin.Instance.Messages.SwapCancelled.Message;
            return true;
        }
    }
}