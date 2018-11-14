using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using ImageProcessor;
using System.IO;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;

namespace Invincible_Overlord
{
    public class Commands : ModuleBase
    {
        static Random rdm = new Random();

        [Command("help"), Summary("Displays commands and descriptions.")]
        public async Task Help()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "Commands";
            emb.ColorStripe = new Color(rdm.Next(256), rdm.Next(256), rdm.Next(256));

            foreach (CommandInfo command in Bot.commands.Commands)
            {
                emb.Fields.Add(new JEmbedField(x =>
                {
                    string header = "+" + command.Name;
                    foreach (ParameterInfo parameter in command.Parameters)
                    {
                        header += " [" + parameter.Name + "]";
                    }
                    x.Header = header;
                    x.Text = command.Summary;
                }));
            }
            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        [Command("roll"), Summary("Roll some dice!")]
        public async Task Roll(string dice)
        {
            int roll = 0;
            bool wm = true;
            string[] inputs;
            int amount = 1;
            bool big = false;
            List<int> bigRolls = new List<int>();

            if (!dice.StartsWith("d") && dice.Contains("d"))
            {
                inputs = dice.Split('d');
                amount = Convert.ToInt32(inputs[0]);
                dice = "d" + inputs[1];
            }

            if (amount > 10) { big = true; wm = false; bigRolls.Clear(); }

            for (int z = 0; z < amount; z++)
            {

                if (dice == "d4") roll = rdm.Next(4) + 1;
                else if (dice == "d6") roll = rdm.Next(6) + 1;
                else if (dice == "d8") roll = rdm.Next(8) + 1;
                else if (dice == "d10") roll = rdm.Next(10) + 1;
                else if (dice == "d12") roll = rdm.Next(12) + 1;
                else if (dice == "d20") roll = rdm.Next(20) + 1;
                else if (dice == "stats")
                {
                    try
                    {
                        int[] rolls = new int[4];
                        for (int i = 0; i < 4; i++)
                        {
                            rolls[i] = rdm.Next(6) + 1;
                        }

                        int lowest = rolls[0];
                        int id = 0;

                        foreach (int val in rolls)
                        {
                            if (val < lowest) { lowest = val; }
                            else id++;
                        }

                        List<int> newRolls = new List<int>();
                        bool removed = false;

                        for (int i = 0; i < 4; i++)
                        {
                            if (!removed)
                            {
                                if (rolls[i] == lowest) removed = true;
                                else newRolls.Add(rolls[i]);
                            }
                            else newRolls.Add(rolls[i]);

                        }

                        string message = "Rolls: ";
                        foreach (int item in newRolls) message += item + " ";
                        message += $"~~{lowest}~~ ({newRolls[0] + newRolls[1] + newRolls[2]})";
                        wm = false;
                        await Context.Channel.SendMessageAsync(message);
                    }
                    catch (Exception a)
                    {
                        Console.WriteLine(a.Message);
                    }
                }
                else
                {
                    roll = rdm.Next(Convert.ToInt32(dice)) + 1;
                    await Context.Channel.SendMessageAsync(Convert.ToString(roll));
                    wm = false;
                }

                if (wm) await watermark(roll, Context.Channel, dice);

                if (big) bigRolls.Add(roll);
            }

            if (big)
            {
                string message = "Rolls: ";
                int total = 0;
                bool nope = false;
                foreach (int item in bigRolls)
                {
                    message += " " + item;
                    if (total + item > Int32.MaxValue) { nope = true; break; }
                    total += item;
                }
                if (!nope)
                {
                    message += $" ***Total: ({total})***";
                    if (message.Length > 2000) message = $"***Total: ({total})***";
                }
                else message = "Yeah I don't think so buddy.";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        [Command("party"), Summary("How's the party?")]
        public async Task Party()
        {
            await SendParty(Context.Channel.Id);
        }
        
        public static async Task SendParty(ulong channelID, bool auto = false)
        {
            string[] charIDs = { "3969285", "4678486", "4528604", "5517451", "5625203", "3898644", "5690800", "6492102", "5650645", "5704102" };
            if (auto) Bot.pMsg.Clear();

            foreach (string id in charIDs)
            {
                Bot.Chrome.Navigate().GoToUrl("https://www.dndbeyond.com/characters/" + id);

                var wait = new WebDriverWait(Bot.Chrome, new TimeSpan(0, 0, 30));
                wait.Until(ExpectedConditions.ElementExists(By.ClassName("ct-character-tidbits__avatar")));

                var imgElem = Bot.Chrome.FindElement(By.ClassName("ct-character-tidbits__avatar"));
                var imgUrl = imgElem.GetCssValue("background-image").Replace("url(\"", "").Replace("\")", "");

                string pageSource = Bot.Chrome.FindElement(By.TagName("body")).Text;

                var charData = pageSource.Split('\n');
                var name = charData[3].Trim('\r');
                var health = charData[9].Trim('\r');
                var GenderRaceLevel = charData[4].Trim('\r');
                var exp = charData[7].Trim('\r');
                var cLevel = charData[5].Trim('\r');
                var nLevel = charData[6].Trim('\r');

                JEmbed emb = new JEmbed();
                emb.Title = name;
                emb.ThumbnailUrl = imgUrl;
                var grl = Regex.Replace(GenderRaceLevel, "[A-Z]", " $&").Trim();
                emb.Description = grl;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = ":heart: Health";
                    x.Text = health;
                    x.Inline = true;
                }));
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = ":arrow_up: EXP";
                    x.Text = cLevel + " -> " + exp + " -> " + nLevel;
                    x.Inline = true;
                }));
                emb.ColorStripe = new Color(rdm.Next(256), rdm.Next(256), rdm.Next(256));
                if (auto) Bot.pMsg.Add(await (Bot.client.GetChannel(channelID) as IMessageChannel).SendMessageAsync("", embed: emb.Build()));
            }
        }
        private async Task watermark(int num,  IMessageChannel chan, string die)
        {
            try
            {
                byte[] imageBytes = null;

                int x = 0, y = 0, fontsize = 0;

                #region die vars
                if (die == "d4")
                {
                    imageBytes = File.ReadAllBytes("Die Images/D4.png");
                    x = 54;
                    y = 55;
                    fontsize = 40;
                }
                else if (die == "d6")
                {
                    imageBytes = File.ReadAllBytes("Die Images/D6.png");
                    x = 50;
                    y = 45;
                    fontsize = 50;
                }
                else if (die == "d8")
                {
                    imageBytes = File.ReadAllBytes("Die Images/D8.png");
                    x = 53;
                    y = 45;
                    fontsize = 45;
                }
                else if (die == "d10")
                {
                    if (num == 10) num = 0;
                    imageBytes = File.ReadAllBytes("Die Images/D10.png");
                    x = 53;
                    y = 55;
                    fontsize = 45;
                }
                else if (die == "d12")
                {
                    imageBytes = File.ReadAllBytes("Die Images/D12.png");
                    x = 55;
                    if (Convert.ToString(num).Length == 2) x = 40;
                    else x = 55;
                    y = 10;
                    fontsize = 40;
                }
                else if (die == "d20")
                {
                    imageBytes = File.ReadAllBytes("Die Images/D20.png");
                    y = 67;
                    if (Convert.ToString(num).Length == 2) x = 55;
                    else x = 60;
                    fontsize = 18;
                }
                #endregion
                using (MemoryStream inStream = new MemoryStream(imageBytes))
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        imageFactory.Load(inStream)
                            .Watermark(new ImageProcessor.Imaging.TextLayer()
                            {
                                Text = Convert.ToString(num),
                                FontColor = System.Drawing.Color.White,
                                FontFamily = new System.Drawing.FontFamily("Arial"),
                                Position = new System.Drawing.Point(x, y),
                                FontSize = fontsize
                            })
                            .Save("Die Images/TempDie.png");
                    }
                }
                await chan.SendFileAsync("Die Images/TempDie.png");

                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

    }
}
