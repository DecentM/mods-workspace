using Discord;
using Discord.WebSocket;
using Discord.Commands;
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
        public static Emoji rightArrow = new Emoji("➡️");
        public static Emoji warning = new Emoji("⚠️");
        public static Emoji error = new Emoji("❗");
        public static Emoji tick = new Emoji("🏁");
    }

    public class Commands
    {
        public static SlashCommandBuilder HelpCommand = new SlashCommandBuilder().WithName("help").WithDescription("Information about how to use me");
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
                this.client.Ready += this.OnClientReady;
                this.client.SlashCommandExecuted += this.OnSlashCommand;

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

        private async Task OnSlashCommand(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "help":
                    await this.HelpCommand(command);
                    break;

                // We should only hit this default branch if we have a programming error as commands are created at startup
                default:
                    await command.RespondAsync($"I don't know a {command.Data.Name} command. This is an issue with me.");
                    break;
            }
        }

        private async Task HelpCommand(SocketSlashCommand command)
        {
            await command.RespondAsync(
                "Send me a subtitle file by pressing the + icon in your message field! Then download my response, and paste its contents into the subtitle input field in the `Cinema In a Space Bottle` world!\n" +
                "Tips:\n" +
                "- You can convert subtitles to .srt format here: <https://subtitletools.com/convert-to-srt-online>\n" +
                "- If the output I give you contains weird characters, make sure your subtitles are UTF-8 encoded: <https://subtitletools.com/convert-text-files-to-utf8-online>\n"
            );
        }

        public async Task OnClientReady()
        {
            try
            {
                await this.client.CreateGlobalApplicationCommandAsync(Commands.HelpCommand.Build());
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                Console.Error.WriteLine(ex.ToString());
            }
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
                await msg.AddReactionAsync(EmojiIcon.rightArrow);
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
                await msg.Channel.SendMessageAsync("Please send me a subtitle file! I'll parse it and send you what you need to paste in VRChat. Type `/help` for help.");
                typingState.Dispose();
                SentrySdk.CaptureTransaction(transaction);
                return;
            }

            // Reject messages with more than one attachment
            if (msg.Attachments.Count != 1)
            {
                await msg.AddReactionAsync(EmojiIcon.cross);
                await msg.Channel.SendMessageAsync("I can only accept one file at a time.");
                typingState.Dispose();
                SentrySdk.CaptureTransaction(transaction);
                return;
            }

            Discord.Attachment attachment = msg.Attachments.ElementAt(0);

            // Nearing this size, the time it takes to compile starts going over a few seconds, so we limit to 256kb to keep performance up
            // and prevent people from stalling the bot with huge files. Subtitle files are 99% of the time are small anyway.
            if (attachment.Size > 386000)
            {
                await msg.AddReactionAsync(EmojiIcon.cross);
                await msg.Channel.SendMessageAsync("This file is too large, please only send me files smaller than 386kb!");
                typingState.Dispose();
                SentrySdk.CaptureTransaction(transaction);
                return;
            }

            try
            {
                // Download the attachment from Discord servers
                string file = await this.http.GetStringAsync(attachment.Url);
                Compiler.CompilationResult result = this.compiler.Compile(file, Path.GetExtension(attachment.Filename));

                // In theory this will never happen as the output is an empty string by default
                if (result.output == null)
                {
                    throw new Exception("The compilation result is null. This is an issue with me not being able to process your file.");
                }

                // Let the user know about non-fatal errors
                if (result.errors.Count != 0)
                {
                    await msg.AddReactionAsync(EmojiIcon.warning);
                    await msg.Channel.SendMessageAsync($"I ran into {result.errors.Count} {(result.errors.Count == 1 ? "error" : "errors")} while processing your file. Some subtitles may be missing, or the whole file might be unusable.");

                    string errorLog = result.errors.Aggregate("", (current, error) =>
                    {
                        return $"{current}{(string)error.value.ReplaceLineEndings("\\n")}\n";
                    });

                    MemoryStream errorStream = new MemoryStream(Encoding.UTF8.GetBytes(errorLog));

                    await msg.Channel.SendFileAsync(errorStream, $"space-bottle-error-log_{attachment.Filename}.txt", "See this file for errors.");
                }

                // If the result is empty, the file probably has an extension that doesn't match its contents
                if (result.output.Length == 0)
                {
                    throw new Exception("Empty parsing result. Check the file format and its contents.");
                }

                // We passed all checks, send the result to the user
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(result.output));

                await msg.AddReactionAsync(EmojiIcon.tick);
                await msg.Channel.SendFileAsync(stream, $"{attachment.Filename}.txt", $"Download this, then paste the entire contents of this file into the input field!");
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                await msg.AddReactionAsync(EmojiIcon.error);
                await msg.Channel.SendMessageAsync($"I ran into an error while transforming your file that caused me to abort. Try again with a different one.\n> {ex.Message}");

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
