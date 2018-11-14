using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace Invincible_Overlord
{
    public class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        #region Vars
        Random rdm = new Random();

        public static DiscordSocketClient client;
        public static CommandService commands;

        public static Timer timer;
        public static List<IMessage> pMsg = new List<IMessage>();
        public static IWebDriver Chrome;

        public const ulong PARTY_CHANNEL = 511979873720467491;
        #endregion

        public async Task Run()
        {
            Start:
            try
            {
                DiscordSocketConfig config = new DiscordSocketConfig() { MessageCacheSize = 1000 };
                Console.WriteLine("Welcome, Brady. Initializing Invincible Overlord...");
                client = new DiscordSocketClient();
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                string token = File.ReadAllLines(@"Files\BotToken")[0];
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                await client.LoginAsync(TokenType.Bot, token);
                Console.WriteLine("Successfully logged in!");
                // Connect the client to Discord's gateway
                await client.StartAsync();
                Console.WriteLine("Invincible Overlord successfully intialized");
                // Block this task until the program is exited.

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("headless");
                Chrome = new ChromeDriver(chromeOptions);
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);

                Again:

                Console.WriteLine("Would you like to try reconnecting? [Y/N]");
                var input = Console.Read();

                if (input == 121) { Console.Clear(); goto Start; }
                else if (input == 110) Environment.Exit(0);

                Console.WriteLine("Invalid input.");
                goto Again;
            }
        }

        public async Task InstallCommands()
        {
            timer = new Timer(new TimerCallback(timerCallback), null, 1000 * 10, 1000 * 60);
            
            client.MessageReceived += HandleCommand;
            client.Ready += HandleReady;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        async void timerCallback(object o)
        {
            foreach(IMessage msg in pMsg)
            {
                await msg.DeleteAsync();
            }

            await Commands.SendParty(PARTY_CHANNEL,true);
        }

        //Code that runs when bot recieves a message
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            if (message.HasCharPrefix('>', ref argPos))
            {

                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
            else return;
        }

        public async Task HandleReady()
        {
            await Commands.SendParty(PARTY_CHANNEL,true);
        }
    }


}
