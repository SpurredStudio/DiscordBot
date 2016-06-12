using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot
{
    class UserLevelCommands
    {
        static Random rnd = new Random();
        static ulong BotID = YDCbot.BotID;
        static Role Mod = YDCbot.Mod;
        static Channel admins = YDCbot.AdminCh;
        static Role Member = YDCbot.Member;
        #region bot text response
        static string helptext = "Hello i am a Discord bot created to help manage the Your Daily Clutch server." + "\n" +
                "Commands:" + "\n" + "   !roll int, rolls between 1 and that int, default is 100." + "\n"+ "   !roll start int, will start a roll that others can participate in!" + "\n" + 
                "   !flipcoin, returns heads or tails" + "\n" + "   !8ball question, user must ask a question" + "\n" + "   !imgur subreddit int, returns a picture from the imgur subreddit entered, if no int is entered then a random picture is returned." + "\n" +
                "   !raffle type, starts a raffle by group, signup, or online. If group is chosen then user must also enter the group/role. If signup is chosen participants must type !raffle to be entered into the raffle. If online is chosen then all online users will be placed into the raffle "
                +"\n"  +"\n" + "Mod Commands" + "\n" + "   !cleanup '#' 'username' # has a max of 100" + "\n" + "   !save, saves current variables to file so that the bot can safely be restarted" + "\n" +"   !grantchannel username, creates a channel that only the user has control over";
        static string EightBall = " Signs point to yes^ Yes^ Reply hazy, try again^ Without a doubt^ My sources say no^ As I see it, yes" +
                "^ You may rely on it^ Concentrate and ask again^ Outlook not so good^ It is decidedly so^ Better not tell you now^ " +
                "Very doubtful^ Yes - definitely^ It is certain^ Cannot predict now^ Most likely^ Ask again later^ My reply is no^ " +
                "Outlook good^ Don't count on it^ Yes, in due time^ Definitely not^ You will have to wait^ I have my doubts^ " +
                "Outlook so so^ Looks good to me!^ Who knows?^ Looking good!^ Probably^ Are you Kidding?^ Go for it!^ Don't bet on it^ " +
                "Forget about it";
        static string[] EightBallArray;
        #endregion
        #region Ban Users
        static List<string> ListofReporters = new List<string>();
        static List<Discord.User> ListofOnlineUsers = new List<Discord.User>();
        static string lastplayerReported = "";      
        #endregion
        #region Rolls       
        static bool isRoleTimerActive = false;
        static int Rolevalue;
        static List<string> RolePlayers = new List<string>();
        static List<int> RoleScores = new List<int>();
        static Channel RoleCh;
        static User RoleStarter;
        #endregion
        #region Raffle
        static List<string> raffleList = new List<string>();
        static User RaffleStarter;
        static bool isRaffleTimerActive = false;
        static Channel RaffleCh;
        #endregion
        public static void ExecuteCommands(MessageEventArgs e, string[] MessageArray)
        {
            if (EightBallArray == null) //checks if load text has ran yet
                loadText();

            #region !flipcoin
            if (MessageArray[0].ToString().ToLower() == "!flipcoin")
            {
                //simple coin flip
                int flip = rnd.Next(0, 2);
                if (flip == 0)
                {
                    e.Channel.SendMessage(e.User.Mention + " ` " + " Flipped a coin and it landed Heads up!" + " `");
                }
                if (flip == 1)
                {
                    e.Channel.SendMessage(e.User.Mention + " ` " + " Flipped a coin and it landed Tails up!" + " `");
                }
            }
            #endregion
            #region !ban
            if (MessageArray[0].ToLower() == "!ban")
            {
                //setup for starting a vote ban
                string ReportedPlayer = "";
                string PlayerReporting = "";
                IEnumerable<User> ReportedUsers;
                IEnumerable<User> OnlineUsers;
                bool matchedplayer = false;
                PlayerReporting = e.User.Name;
                OnlineUsers = e.Server.Users;
                int countUsers = 0;

                if (MessageArray.Length > 1)
                {
                    for (int i = 1; i < MessageArray.Length; i++)
                    {
                        if (i > 1)
                        {
                            ReportedPlayer = ReportedPlayer + " " + MessageArray[i];
                        }
                        else
                            ReportedPlayer = ReportedPlayer + MessageArray[i];
                    }
                    foreach (User user in OnlineUsers)
                    {
                        if (user.Status == UserStatus.Online && user.Id != BotID)
                        {
                            countUsers = countUsers + 1;
                        }
                    }

                    if (e.Server.FindUsers(ReportedPlayer, true) != null)
                    {
                        ReportedUsers = e.Server.FindUsers(ReportedPlayer, true);
                        if (e.User.Roles.Contains(Mod) && false)
                        {
                            foreach (User user in ReportedUsers)
                            {
                                if (!user.HasRole(Mod))
                                {
                                    var AsyncClass = new AsyncAwait();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    AsyncClass.BanUser(user, e);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    
                                    admins.SendMessage("`" + "The user " + ReportedPlayer + " was banned by " + e.User.Name + "`");
                                    matchedplayer = false;
                                }
                            }
                        }
                        else if (countUsers > 3)
                        {
                            if (lastplayerReported == "")
                            {
                                foreach (User user in ReportedUsers)
                                {
                                    if (user.Name == ReportedPlayer && !user.HasRole(Mod))
                                        matchedplayer = true;
                                }
                                if (matchedplayer == true)
                                {                                     
                                    YDCbot.StartBanTimer();
                                    e.User.SendMessage("New Ban starting now! Vote ends in 5 minutes.");
                                    lastplayerReported = ReportedPlayer;
                                    ListofReporters.Add(PlayerReporting);
                                }
                                else
                                    e.User.SendMessage("No user found by that name!");
                            }
                            else if (lastplayerReported == ReportedPlayer)
                            {
                                if (ListofReporters.Contains(PlayerReporting))
                                {
                                    e.User.SendMessage("Can only Report Once");
                                }
                                else
                                {
                                    ListofReporters.Add(PlayerReporting);
                                    int countReports = ListofReporters.Count;
                                    e.User.SendMessage("Petition added total count " + countReports + " out of " + countUsers / 2 + " votes needed.");
                                    if (countReports >= countUsers / 2)
                                    {
                                        foreach (User user in ReportedUsers)
                                        {
                                            var AsyncClass = new AsyncAwait();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                            AsyncClass.BanUser(user, e);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                            YDCbot.StopBanTimer();
                                        }
                                        foreach (string reporter in ListofReporters)
                                        {
                                            admins.SendMessage("`" + "The user " + ReportedPlayer + " was banned by " + reporter + "`");
                                        }
                                        ListofReporters.Clear();
                                        lastplayerReported = "";
                                    }
                                }
                            }
                            else
                                e.User.SendMessage("There is already a vote to ban on someone else!");
                        }
                        else
                            e.User.SendMessage("There must be more than 4 players online to ban someone");
                    }
                }
                else
                    e.User.SendMessage("You must enter a name to report a player");
            }
            #endregion
            #region !8ball
            if (MessageArray[0].ToLower() == "!8ball")
            {
                //asks the 8ball a question, then sends the response back into the chat
                int value;
                if (MessageArray.Length > 1)
                {
                    value = rnd.Next(0, EightBallArray.Length);
                    e.Channel.SendMessage(e.User.Mention+"` asked "+ e.Message.Text + "--The Eightball:" + EightBallArray[value] + " `");
                }
                else
                {
                    e.User.SendMessage("You must ask the 8ball something");
                }             
            }
            #endregion
            #region !roll
            if (e.Message.Text.ToLower().Contains("!roll"))
            {
                int value;
                if (MessageArray[0].ToLower() == "!roll")
                {
                    if (isRoleTimerActive && !e.Message.Channel.IsPrivate)
                    {
                        if (MessageArray.Length > 1)
                        {
                            if (MessageArray[1].ToLower() == "end")
                            {
                                if (e.User == RoleStarter || e.User.HasRole(Mod))
                                {
                                    YDCbot.StopRollTimer();
                                }
                            }
                        }
                        else if (!RolePlayers.Contains(e.User.Name))
                        {
                            int roll = rnd.Next(1, Rolevalue + 1);
                            e.Channel.SendMessage(e.User.Mention + " ` " + "Rolled a " + roll + " out of " + Rolevalue + "`");
                            RolePlayers.Add(e.User.Name);
                            RoleScores.Add(roll);
                        }

                        return;
                    }
                    else if (MessageArray.Length > 1 && !isRoleTimerActive)
                    {
                        if (MessageArray[1].ToLower() == "start" && !isRoleTimerActive && !e.Message.Channel.IsPrivate)
                        {
                            YDCbot.StartRollTimer();
                            isRoleTimerActive = true;
                            RoleCh = e.Channel;
                            RoleStarter = e.User;

                            if (MessageArray.Length > 2)
                            {
                                bool isSuccess = int.TryParse(MessageArray[2], out value);
                                if (isSuccess == true)
                                {
                                    value = Math.Abs(value);
                                    Rolevalue = value;
                                    if (value <= 1000000)
                                    {
                                        int roll = rnd.Next(1, value + 1);
                                        e.Channel.SendMessage(e.User.Mention + " ` " + "Started a roll for the next 60 seconds and then rolled " + roll + " out of " + value + "`");
                                        RolePlayers.Add(e.User.Name);
                                        RoleScores.Add(roll);
                                    }
                                    else
                                    {
                                        int roll = rnd.Next(1, 1000001);
                                        e.Channel.SendMessage(e.User.Mention + " ` " + "Started a roll for the next 60 seconds and then rolled " + roll + " out of " + 1000000 + "`");
                                        RolePlayers.Add(e.User.Name);
                                        RoleScores.Add(roll);
                                    }
                                }
                            }
                            else
                            {
                                Rolevalue = 100;
                                int roll = rnd.Next(1, 101);
                                e.Channel.SendMessage(e.User.Mention + " ` " + "Started for the next 60 seconds a roll and then rolled " + roll + " out of " + 100 + "`");
                                RolePlayers.Add(e.User.Name);
                                RoleScores.Add(roll);
                            }
                        }
                        else
                        {
                            bool isSuccess = int.TryParse(MessageArray[1], out value);
                            if (isSuccess == true)
                            {
                                value = Math.Abs(value);
                                if (value <= 1000000)
                                {
                                    int roll = rnd.Next(1, value);
                                    e.Channel.SendMessage(e.User.Mention + " ` " + "Rolled a " + roll + " out of " + value + "`");
                                }
                                else
                                {
                                    int roll = rnd.Next(1, 1000001);
                                    e.Channel.SendMessage(e.User.Mention + " ` " + "Rolled a " + roll + " out of " + 1000000 + "`");
                                }
                            }
                        }
                    }
                    else if (!isRoleTimerActive)
                    {
                        int roll = rnd.Next(1, 101);
                        e.Channel.SendMessage(e.User.Mention + " ` " + "Rolled a " + roll + " out of 100" + "`");
                    }
                }
            }
            #endregion
            #region !raffle
            if (MessageArray[0].ToLower() == "!raffle" && !e.Channel.IsPrivate)
            {
                int value;
                IEnumerable<User> OnlineUsers;
                OnlineUsers = e.Server.Users;
                int timerV =1;
                int v;
                int length = MessageArray.Length;
                if (length > 1)
                {
                    if (MessageArray[1].ToLower() == "end" && isRaffleTimerActive && e.User == RaffleStarter)
                    {
                        YDCbot.StopRaffleTimer();
                    }
                    else if (MessageArray[1].ToLower() == "online" && !isRaffleTimerActive)
                    {
                        foreach (User user in OnlineUsers)
                        {
                            if (user.Status == UserStatus.Online && user.Id != BotID)
                            {
                                raffleList.Add(user.Name);
                            }
                        }
                        value = raffleList.Count();
                        int ind = rnd.Next(0, value);
                        e.Channel.SendMessage(" ` " + " The winner of the raffle was " + raffleList[ind] + "`");
                        raffleList.Clear();
                    }
                    else if (MessageArray[1].ToLower() == "signup" && !isRaffleTimerActive)
                    {
                        if (length > 2)
                        {
                            bool isSuccess = int.TryParse(MessageArray[3], out v);
                            timerV = v;
                        }
                        else
                            timerV = 5;

                        e.Channel.SendMessage(e.User.Name + " ` " + " Has started a raffle for the next " + timerV + " minutes. If you would like to enter type !raffle" + "`");
                        isRaffleTimerActive = true;
                        RaffleStarter = e.User;
                        RaffleCh = e.Channel;
                        YDCbot.StartRaffleTimer(timerV);

                    }
                    else if (MessageArray[1].ToLower() == "group" && length > 2 && !isRaffleTimerActive)
                    {
                        if (MessageArray[2].ToLower() == "member")
                        {
                            foreach (User user in OnlineUsers)
                            {
                                if (user.HasRole(Member) && user.Status == UserStatus.Online)
                                {
                                    raffleList.Add(user.Name);
                                }
                            }
                            value = raffleList.Count();
                            int ind = rnd.Next(0, value);
                            e.Channel.SendMessage(Member.Mention + " ` " + " The winner of the raffle was " + raffleList[ind] + "`");
                            raffleList.Clear();
                        }
                        if (MessageArray[2].ToLower() == "mod")
                        {
                            foreach (User user in OnlineUsers)
                            {
                                if (user.HasRole(Mod) && user.Status == UserStatus.Online)
                                {
                                    raffleList.Add(user.Name);
                                }
                            }
                            value = raffleList.Count();
                            int ind = rnd.Next(0, value);
                            e.Channel.SendMessage(Mod.Mention + " ` " + " The winner of the raffle was " + raffleList[ind] + "`");
                            raffleList.Clear();
                        }
                        if (MessageArray[2].ToLower() == "hasrole")
                        {
                            foreach (User user in OnlineUsers)
                            {
                                if (user.Status == UserStatus.Online)
                                {
                                    if (user.HasRole(Mod) || user.HasRole(Member))
                                    {
                                        raffleList.Add(user.Name);
                                    }
                                }
                            }
                            value = raffleList.Count();
                            int ind = rnd.Next(0, value);
                            e.Channel.SendMessage(Mod.Mention + Member.Mention + " ` " + " The winner of the raffle was " + raffleList[ind] + "`");
                            raffleList.Clear();
                        }
                    }
                    else
                        e.User.SendMessage("There is already an active Raffle!");
                }
                else if (isRaffleTimerActive)
                {
                    if (!raffleList.Contains(e.User.Name))
                    {
                        raffleList.Add(e.User.Name);
                    }
                    else
                        e.User.SendMessage("You are already entered into the raffle!");
                }
                else
                    e.User.SendMessage("Currently there is no active raffle.");               
            }
            #endregion
            #region !Imgur
            if (MessageArray[0].ToLower() == "!imgur")
            {
                string Reddit;
                int number = 0;
                if (MessageArray.Length > 3)
                {
                    e.User.SendMessage("Format is /imgur Subreddit ImageNumber");
                }
                else if (MessageArray.Length > 2)
                {
                    Reddit = MessageArray[1].ToString();
                    bool isSuccess = int.TryParse(MessageArray[2], out number);
                    if (!e.Channel.IsPrivate)
                        admins.SendMessage("`" + e.User.Name.ToString() + " posted an image from r/" + Reddit + " in the " + e.Channel.Name.ToString() + " Channel" + "`");
                    ImgurBot.ImgurUpdate(Reddit, number, e);
                }
                else if (MessageArray.Length > 1)
                {
                    Reddit = MessageArray[1].ToString();
                    if (!e.Channel.IsPrivate)
                        admins.SendMessage("`" + e.User.Name.ToString() + " posted an image from r/" + Reddit + " in the " + e.Channel.Name.ToString() + " Channel" + "`");
                    ImgurBot.ImgurUpdate(Reddit, number, e);
                }
                else
                    e.User.SendMessage("Please enter a subreddit to Imgur");               
            }
            #endregion
            #region !youtube
            if (MessageArray[0].ToLower() == "!youtube" && !e.Channel.IsPrivate)
            {
                if (MessageArray[1].ToLower() == "stop" && e.User.HasRole(Mod))
                {
                    e.User.SendMessage("Music Stopping!");
                    MusicPlayer.ExitLoop = true;
                }
                else
                {
                    if (!MusicPlayer.IsPlaying)
                    {
                        MusicPlayer.ExitLoop = false;
                        MusicPlayer.PlayYouTube(MessageArray[1], e);
                    }
                    else
                        e.User.SendMessage("Music is already playing!");
                }
            }
            #endregion
            #region !help
            if (e.Message.Text.ToLower().Contains("!help"))
            {
                e.Message.Delete();
                e.User.SendMessage("```"+helptext+"```");
            }
            #endregion
        }
        public class AsyncAwait
        {
            public async Task BanUser(User ReportedUser, MessageEventArgs e)
            {
                await e.Server.Ban(ReportedUser);
            }
        }
        public static void loadText()
        {   //turns long string into arrays
            EightBallArray = EightBall.Split('^');
            helptext = helptext.Replace("\n", System.Environment.NewLine);
        }
        //methods used to get information between this script and YDCbot.cs
        public static void ResetBan()
        {
            lastplayerReported = "";
            ListofReporters.Clear();          
        }
        public static void RaffleEnded()
        {
            isRaffleTimerActive = false;
            

            int rafflecount = raffleList.Count();
            int index = rnd.Next(0, rafflecount);

            RaffleCh.SendMessage(" The winner of the raffle started by " + RaffleStarter.Name + " was " + raffleList[index] + "!");
            raffleList.Clear();
        }
        public static void RollEnded()
        {
            isRoleTimerActive = false;

            int indexMax
                = !RoleScores.Any() ? -1 :
                RoleScores
                .Select((value, index) => new { Value = value, Index = index })
                .Aggregate((a, b) => (a.Value > b.Value) ? a : b)
                .Index;

            RoleCh.SendMessage(" " + RoleCh.Mention + " " + RolePlayers[indexMax] + " Won the rolls with a " + RoleScores[indexMax]);
            Rolevalue = 100;
            RolePlayers.Clear();
            RoleScores.Clear();
        }
    }
}
