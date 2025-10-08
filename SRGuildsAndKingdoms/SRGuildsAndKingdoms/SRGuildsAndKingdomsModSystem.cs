using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using System.Linq;
using SRGuildsAndKingdoms.src.guilds;

namespace SRGuildsAndKingdoms
{
    public class SRGuildsAndKingdomsModSystem : ModSystem
    {
        private GuildManager guildManager;
        private ICoreServerAPI serverApi;

        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("srguildsandkingdoms:hello"));
            guildManager = new GuildManager(api);
            serverApi = api;

            api.ChatCommands.Create("guild")
                .WithDescription("Manage or join guilds")
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .WithArgs(
                    api.ChatCommands.Parsers.Word("action", new string[] { "create", "list", "info", "invite", "accept", "decline", "remove" }),
                    api.ChatCommands.Parsers.OptionalWord("arg1"),
                    api.ChatCommands.Parsers.OptionalWord("arg2")
                )
                .HandleWith(OnGuildCommand);

            api.Event.SaveGameLoaded += OnSaveGameLoaded;
            api.Event.GameWorldSave += OnSaveGameSaving;
        }

        private void OnSaveGameLoaded()
        {
            guildManager.OnSaveGameLoading();
        }

        private void OnSaveGameSaving()
        {
            guildManager.OnSaveGameSaving();
        }

        private TextCommandResult OnGuildCommand(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player;
            var action = args.ArgCount > 0 ? args[0] as string : null;
            var arg1 = args.ArgCount > 1 ? args[1] as string : null;
            var arg2 = args.ArgCount > 2 ? args[2] as string : null;
            var playerUid = player?.PlayerUID;

            switch (action)
            {
                case "create":
                    if (string.IsNullOrEmpty(arg1)) return TextCommandResult.Success("Usage: /guild create <name>");
                    if (guildManager.CreateGuild(arg1, playerUid))
                        return TextCommandResult.Success($"Guild '{arg1}' created.");
                    else
                        return TextCommandResult.Success($"Guild '{arg1}' already exists or you are already in a guild.");
                case "invite":
                    if (string.IsNullOrEmpty(arg1)) return TextCommandResult.Success("Usage: /guild invite <player>");
                    var invitee = serverApi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerName.Equals(arg1, System.StringComparison.OrdinalIgnoreCase));
                    if (invitee == null) return TextCommandResult.Success($"Player '{arg1}' not found.");
                    var guild = guildManager.GetGuildByMember(playerUid);
                    if (guild == null) return TextCommandResult.Success("You are not in a guild.");
                    if (guildManager.InviteToGuild(guild.Name, playerUid, invitee.PlayerUID))
                        return TextCommandResult.Success($"Invited {arg1} to guild.");
                    else
                        return TextCommandResult.Success($"Could not invite {arg1}.");
                case "accept":
                    if (guildManager.AcceptInvite(playerUid))
                        return TextCommandResult.Success("You have joined the guild.");
                    else
                        return TextCommandResult.Success("No pending guild invite found.");
                case "list":
                    var g = guildManager.GetGuildByMember(playerUid);
                    if (g == null) return TextCommandResult.Success("You are not in a guild.");
                    var members = string.Join(", ", g.Members.Values.Select(m => serverApi.World.PlayerByUid(m.PlayerUid)?.PlayerName ?? m.PlayerUid));
                    return TextCommandResult.Success($"Guild members: {members}");
                case "remove":
                    if (string.IsNullOrEmpty(arg1)) return TextCommandResult.Success("Usage: /guild remove <player>");
                    var target = serverApi.World.AllOnlinePlayers.FirstOrDefault(p => p.PlayerName.Equals(arg1, System.StringComparison.OrdinalIgnoreCase));
                    if (target == null) return TextCommandResult.Success($"Player '{arg1}' not found.");
                    var guildToRemove = guildManager.GetGuildByMember(playerUid);
                    if (guildToRemove == null) return TextCommandResult.Success("You are not in a guild.");
                    if (guildManager.RemoveMember(guildToRemove.Name, playerUid, target.PlayerUID))
                        return TextCommandResult.Success($"Removed {arg1} from guild.");
                    else
                        return TextCommandResult.Success($"Could not remove {arg1}.");
                case "info":
                    var infoGuild = guildManager.GetGuildByMember(playerUid);
                    if (infoGuild == null) return TextCommandResult.Success("You are not in a guild.");
                    return TextCommandResult.Success($"Guild: {infoGuild.Name}, Members: {infoGuild.Members.Count}");
                case "decline":
                    // Not implemented yet
                    return TextCommandResult.Success("Decline not implemented yet.");
                default:
                    return TextCommandResult.Success("Unknown subcommand. Usage: /guild <create|invite|accept|list|remove|info|decline> ...");
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("srguildsandkingdoms:hello"));
        }
    }
}
