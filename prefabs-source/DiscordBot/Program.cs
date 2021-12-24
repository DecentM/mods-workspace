using Discord;
using Discord.WebSocket;
using DecentM.Subtitles;
using System.Text;
using System.IO;
using Sentry;
using Config.Net;
using System;

namespace DecentM.Subtitles.DiscordBot
{
    public class EmojiIcon
    {
        public static Emoji clock2 = new Emoji("🕑");
        public static Emoji cross = new Emoji("❌");
        public static Emoji info = new Emoji("➡️");
        public static Emoji warning = new Emoji("⚠️");
    }

    public class Program
    {
        private BotConfig botConfig = new ConfigurationBuilder<BotConfig>()
            .UseEnvironmentVariables()
            .Build();

        public static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient client = new DiscordSocketClient();
        private Compiler compiler = new Compiler();
        private HttpClient http = new HttpClient();

        public async Task MainAsync()
        {
            using (SentrySdk.Init(o =>
            {
                o.Dsn = this.botConfig.SentryDsn;
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = false;
                // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
            }))
            {
                this.client.Log += this.Log;
                this.client.MessageReceived += this.OnMessage;

                await this.client.LoginAsync(TokenType.Bot, this.botConfig.DiscordToken);
                await this.client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }

        private async Task OnMessage(SocketMessage msg)
        {
            // We actually don't care about other bots, but this will also prevent us
            // from getting into an infinite loop
            if (msg.Author.IsBot)
            {
                return;
            }

            // Reject messages that mention us but aren't DMs
            if (msg.MentionedUsers.Any(u => u.Id == this.client.CurrentUser.Id) && msg.Channel.GetType() != typeof(Discord.WebSocket.SocketDMChannel))
            {
                await msg.AddReactionAsync(EmojiIcon.info);
                await msg.Channel.SendMessageAsync("Please DM me to prevent clogging this channel!");
                return;
            }

            // Ignore messages that don't mention us and aren't a DM
            if (msg.Channel.GetType() != typeof(Discord.WebSocket.SocketDMChannel))
            {
                return;
            }

            Transaction transaction = new Transaction("parse", "parse");

            transaction.SetTag("DiscordDiscriminator", msg.Author.Discriminator);
            transaction.SetTag("DiscordChannel", msg.Channel.Name);
            transaction.SetTag("DiscordTimestamp", msg.Timestamp.UtcDateTime.ToString());

            // Let the user know we're processing their message
            IDisposable typingState = msg.Channel.EnterTypingState();

            // Reject messages with no attachments
            if (msg.Attachments.Count == 0)
            {
                await msg.AddReactionAsync(EmojiIcon.cross);
                await msg.Channel.SendMessageAsync("Please send me a subtitle file! I'll parse it and send you what you need to paste in VRChat.");
                typingState.Dispose();
                SentrySdk.CaptureTransaction(transaction);
                return;
            }

            // Reject messages with more than one attachment
            if (msg.Attachments.Count != 1)
            {
                await msg.AddReactionAsync(EmojiIcon.cross);
                await msg.Channel.SendMessageAsync("Only one file is accepted at a time.");
                typingState.Dispose();
                SentrySdk.CaptureTransaction(transaction);
                return;
            }

            Discord.Attachment srt = msg.Attachments.ElementAt(0);

            string file = await this.http.GetStringAsync(srt.Url);

            try
            {
                string output = this.compiler.Compile(file, Path.GetExtension(srt.Filename));

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(output));

                await msg.Channel.SendFileAsync(stream, $"{srt.Filename}.txt", $"Download this, then paste the entire contents of this file into the input field!");
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                await msg.AddReactionAsync(EmojiIcon.warning);
                await msg.Channel.SendMessageAsync($"I ran into an error while transforming your file. Try again with a different one.\n> {ex.Message}");

                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                SentrySdk.CaptureTransaction(transaction);

                // let the user know we're done
                typingState.Dispose();

                await Task.CompletedTask;
            }
        }
    }
}
