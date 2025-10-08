using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.guilds
{
    public class Guild
    {
        public string Name { get; set; }
        public Dictionary<string, GuildMember> Members { get; set; } = new();
        public List<GuildInvite> PendingInvites { get; set; } = new();
        public Dictionary<string, string> Roles { get; set; } = new(); // role name -> description
        //public List<Claim> Claims { get; set; } = new();
    }
}
