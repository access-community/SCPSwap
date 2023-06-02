using System;
using System.Linq;
using CommandSystem;
using PluginAPI.Core;
using PlayerRoles;
using RemoteAdmin;
using SCPSwap_NWAPI.Models;

namespace SCPSwap_NWAPI.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ScpSwapParent : ParentCommand
    {
        public ScpSwapParent() => LoadGeneratedCommands();
        
        public override string Command => "scpswap";
        
        public override string[] Aliases { get; } = { "swap" };
        
        public override string Description => "Base command for ScpSwap.";
        
        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new Accept());
            RegisterCommand(new Cancel());
            RegisterCommand(new Decline());
            RegisterCommand(new List());
        }
        
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get((PlayerCommandSender)sender);
            if (player == null)
            {
                response = Plugin.Instance.Messages.ShouldRunFromGame.Message;
                return false;
            }

            if (!Round.IsRoundStarted)
            {
                response = Plugin.Instance.Messages.RoundHasNotStarted.Message;
                return false;
            }
            
            if (Round.Duration > TimeSpan.FromSeconds(Plugin.Instance.Config.SwapTimeout))
            {
                response = Plugin.Instance.Messages.SwapPeriodEnded.Message;
                return false;
            }

            if (arguments.IsEmpty())
            {
                response = $"Usage: .{Command} SCP";
                return false;
            }

            if (player.Team != Team.SCPs)
            {
                response = Plugin.Instance.Messages.MustBeAScp.Message;
                return false;
            }

            if (Plugin.Instance.Config.BlacklistedScps.Contains(player.Role))
            {
                response = Plugin.Instance.Messages.CannotSwapHasScp.Message;
                return false;
            }

            if (Swap.FromSender(player) != null)
            {
                response = Plugin.Instance.Messages.PendingRequest.Message;
                return false;
            }

            Player receiver = GetReceiver(arguments.At(0), out Action<Player> spawnMethod);
            if (player == receiver)
            {
                response = Plugin.Instance.Messages.CannotSwapSelf.Message;
                return false;
            }

            if (spawnMethod == null)
            {
                response = Plugin.Instance.Messages.RoleNotFound.Message;
                return false;
            }
            
            if (receiver != null)
            {
                if (Plugin.Instance.Config.BlacklistedScps.Contains(receiver.Role))
                {
                    response = Plugin.Instance.Messages.CannotSwap.Message;
                    return false;
                }
                Swap.Send(player, receiver);
                response = Plugin.Instance.Messages.RequestSent.Message;
                return true;
            }

            response = Plugin.Instance.Messages.CannotSwap.Message;
            return false;
        }

        private static Player GetReceiver(string request, out Action<Player> spawnMethod)
        {
            RoleTypeId roleSwap = ValidSwaps.Get(request);
            if (roleSwap != RoleTypeId.None)
            {
                spawnMethod = player => player.SetRole(roleSwap);
                return Player.GetPlayers().FirstOrDefault(player => player.Role == roleSwap);
            }

            spawnMethod = null;
            return null;
        }
    }
}