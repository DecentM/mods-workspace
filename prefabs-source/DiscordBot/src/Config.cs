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
