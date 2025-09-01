using CommandSystem;
using Exiled.API.Features;
using SCP956Plugin.SCP956;
using System;

namespace SCP956FIX.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class @goto : ICommand
    {
        public string Command => "gotoscp956";

        public string[] Aliases => null;

        public string Description => "teleport player to room from scp956";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player != null)
            {
                if (SCP956AI.Instance.Spawned)
                {
                    player.Position = SCP956AI.Instance._spawnPos;
                    response = "You have teleported successfully";
                    return true;
                }

                response = "SCP956 hasn't spawned yet";
                return false;
            }

            response = "you arent a human";
            return false;
        }
    }
}
