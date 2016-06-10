using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot
{
    class ModLevelCommands
    {
        #region grant channel
        static Channel admins = YDCbot.AdminCh;
        public static User NewChannelOwner;
        static string NewChannelOwnerString = "";
        #endregion
        public static void ExecuteModCommands(MessageEventArgs e, string[] MessageArray)//called from YDCbot.cs
        {
            #region !clear
            if (MessageArray[0].ToLower() == "!clear") //checks if first phrase entered was !clear, because it passes through ToLower, !CLEAR would also enter this command. repeated throughout commands
            {
                //clears public chat of messages
                if (e.Message.Channel.IsPrivate == true) return; //if sent from a PM escapes the loop

                int value;
                string ReportedPlayer = "";
                IEnumerable<Message> Imsgs = null;
                IEnumerable<User> Iusers = null;

                if (MessageArray.Length > 1) //if the message is longer than 1 will expect the second value is a number and convert it to int
                {
                    int.TryParse(MessageArray[1], out value);
                    Imsgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(value); //gets x amount of messages in the channe

                }
                else
                {
                    Imsgs = e.Channel.Messages.OrderByDescending(x => x.Timestamp).Take(100); //defaults to 100 messages
                }
                if (MessageArray.Length > 2) //if the message is longer than 2 will expect third value is a user
                {
                    for (int i = 2; i < MessageArray.Length; i++) //forloop that puts together string sections, great if username has a space in it, but otherwise not used. shows up throughout code
                    {
                        ReportedPlayer = ReportedPlayer + MessageArray[i];
                    }
                    if (ReportedPlayer == "imgur")
                    {
                        Iusers = e.Server.FindUsers("YDC", true);
                        Imsgs = Imsgs.Where(x => Iusers.Contains(x.User));
                        var AsyncClass1 = new AsyncAwait();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncClass1.Deleteimgur(Imsgs, e, admins, ReportedPlayer);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                        Iusers = e.Server.FindUsers(ReportedPlayer, true);

                    if (Iusers == null)
                    {
                        e.User.SendMessage("No User Found by that name");
                        return;
                    }
                    Imsgs = Imsgs.Where(x => Iusers.Contains(x.User)); //returns messages from the user out of the amount of messages that was collected
                }
                if (Imsgs.Any()) //checks if any messages were found
                {
                    var AsyncClass = new AsyncAwait();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncClass.DeleteMessages(Imsgs, e, admins, ReportedPlayer);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    e.User.SendMessage("No Messages Found");
                }
            }
            #endregion
            #region !grantchannel
            if (MessageArray[0].ToLower() == "!grantchannel" && YDCbot.ChannelToGrant)
            {
                //creates a new channel and gives rights to the given user
                IEnumerable<User> Iowners;
                List<User> ListOwners = new List<User>();

                if (MessageArray.Length > 1)
                {
                    for (int i = 1; i < MessageArray.Length; i++)
                    {
                        if (i > 1)
                        {
                            NewChannelOwnerString = NewChannelOwnerString + " " + MessageArray[i];
                        }
                        else
                            NewChannelOwnerString = NewChannelOwnerString + MessageArray[i];
                    }

                    if (e.Server.FindUsers(NewChannelOwnerString, true) != null) //checks if entered name is a real user
                    {
                        Iowners = e.Server.FindUsers(NewChannelOwnerString);
                        ListOwners = Iowners.ToList(); //converts Ienumerable to List
                        if (ListOwners.Count > 1) //if more than one user found escape loop
                        {
                            e.User.SendMessage("More than one user found by that name");
                            return;
                        }
                        else
                        {
                            NewChannelOwner = ListOwners[0];
                            e.Server.CreateChannel(NewChannelOwnerString, ChannelType.Voice); //creates voice channel with the users name, they can change it later
                        }
                    }
                    else
                        e.User.SendMessage("No user by that name found!");
                }
                else
                    e.User.SendMessage("No username entered!");
            }
            #endregion
            #region !save
            if (MessageArray[0].ToLower() == "!save")
            {
                //saves information to file
                //currently only saves twitter info
                TwitterBot.SaveTwitter(); //saves twitter var
            }
            #endregion
            #region !exit
            if (MessageArray[0].ToLower() == "!exitapp")
            {
                Environment.Exit(1);
            }
            #endregion
        }
        public class AsyncAwait
        {
            //some methods used need "Await" to work, which was done through AsyncAwait.
            //Is there another way to do this? can it be done within a normal method?
            public async Task DeleteMessages(IEnumerable<Message> Imsgs, MessageEventArgs e, Channel admins, String ReportedPlayer)
            {
                int i = 0;
                foreach (var msg in Imsgs)
                {
                    i = i + 1;
                    await msg.Delete(); //deletes messages
                }
                //reports to admin channel about messages being deleted and who deleted them
                if (ReportedPlayer == "")
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    admins.SendMessage("`" + e.User.Name + " Cleaned out " + i + " messages" + "`");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                else
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    admins.SendMessage("`" + e.User.Name + " Cleaned out " + i + " of " + ReportedPlayer + " messages" + "`");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            public async Task Deleteimgur(IEnumerable<Message> Imsgs, MessageEventArgs e, Channel admins, String ReportedPlayer)
            {
                int i = 0;
                foreach (var msg in Imsgs)
                {
                    i = i + 1;
                    if(msg.Text.Contains("i.imgur.com"))
                        await msg.Delete(); //deletes messages
                }
            }
            public async Task NewChannel(string name, ChannelType type, MessageEventArgs e)
            {
                //creates channel
                await e.Server.CreateChannel(name, type);
            }
        }
    }
}
