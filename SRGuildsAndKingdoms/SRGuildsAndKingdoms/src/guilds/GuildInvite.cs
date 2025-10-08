using System;

namespace SRGuildsAndKingdoms.src.guilds
{
    public class GuildInvite
    {
        public string InviterUid { get; set; }
        public string InviteeUid { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
