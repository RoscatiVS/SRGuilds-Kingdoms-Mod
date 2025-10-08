using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.IO;
using System.Text.Json;

namespace SRGuildsAndKingdoms.src.guilds
{
    public class GuildManager
    {
        private Dictionary<string, Guild> guilds = new(); // guild name -> Guild
        private ICoreServerAPI sapi;
        private string savePath => Path.Combine(sapi.GetOrCreateDataPath("SRGuildsAndKingdoms"), "guilds.json");

        public GuildManager(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public void OnSaveGameLoading()
        {
            LoadGuilds();
        }

        public void OnSaveGameSaving()
        {
            SaveGuilds();
        }

        public bool CreateGuild(string name, string creatorUid)
        {
            if (guilds.ContainsKey(name)) return false;
            if (GetGuildByMember(creatorUid) != null) return false; // User already in a guild
            var guild = new Guild { Name = name };
            guild.Members[creatorUid] = new GuildMember { PlayerUid = creatorUid, Role = "Leader" };
            guilds[name] = guild;
            return true;
        }

        public Guild GetGuildByMember(string playerUid)
        {
            return guilds.Values.FirstOrDefault(g => g.Members.ContainsKey(playerUid));
        }

        public Guild GetGuild(string name)
        {
            guilds.TryGetValue(name, out var guild);
            return guild;
        }

        public bool InviteToGuild(string guildName, string inviterUid, string inviteeUid)
        {
            var guild = GetGuild(guildName);
            if (guild == null || !guild.Members.ContainsKey(inviterUid)) return false;
            if (guild.Members.ContainsKey(inviteeUid)) return false;
            if (guild.PendingInvites.Any(i => i.InviteeUid == inviteeUid)) return false;
            guild.PendingInvites.Add(new GuildInvite { InviterUid = inviterUid, InviteeUid = inviteeUid, Timestamp = System.DateTime.UtcNow });
            return true;
        }

        public bool AcceptInvite(string playerUid)
        {
            var invite = guilds.Values.SelectMany(g => g.PendingInvites).FirstOrDefault(i => i.InviteeUid == playerUid);
            if (invite == null) return false;
            var guild = guilds.Values.First(g => g.PendingInvites.Contains(invite));
            guild.Members[playerUid] = new GuildMember { PlayerUid = playerUid, Role = "Member" };
            guild.PendingInvites.Remove(invite);
            return true;
        }

        public bool RemoveMember(string guildName, string removerUid, string targetUid)
        {
            var guild = GetGuild(guildName);
            if (guild == null || !guild.Members.ContainsKey(removerUid)) return false;
            if (!guild.Members.ContainsKey(targetUid)) return false;
            guild.Members.Remove(targetUid);
            return true;
        }

        public List<GuildMember> ListMembers(string guildName)
        {
            var guild = GetGuild(guildName);
            return guild?.Members.Values.ToList() ?? new List<GuildMember>();
        }

        public void SaveGuilds()
        {
            var json = JsonSerializer.Serialize(guilds);
            File.WriteAllText(savePath, json);
        }

        public void LoadGuilds()
        {
            if (!File.Exists(savePath)) return;
            var json = File.ReadAllText(savePath);
            guilds = JsonSerializer.Deserialize<Dictionary<string, Guild>>(json) ?? new();
        }
    }
}
