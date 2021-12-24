using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.Net;

namespace DecentM.Subtitles.DiscordBot
{
    public interface BotConfig
    {
        [Option(Alias = "DISCORD_TOKEN", DefaultValue = "")]
        string DiscordToken { get; }

        [Option(Alias = "SENTRY_DSN", DefaultValue = "")]
        string SentryDsn { get; }
    }
}
