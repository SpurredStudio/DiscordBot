using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Discord.Audio;


namespace DiscordBot
{
    public class YDCbot
    {
        ///////////////////////////////////////////////////////////////////////
        ////// an opus.dll file is included but it is not in the correct //////
        ////// spot. after running the debug in VS include the opus.dll  //////
        ////// file under DiscordBot/bin/Debug. Once you have compiled   //////
        ////// the "final" version make sure you also place the opus.dll //////
        ////// with the other .dll files.                                ////// 
        ///////////////////////////////////////////////////////////////////////

        ////// Enter your information here ///////
        string BotToken = "";
        public static string ImgurClientID = "";
        public static string TwitterConsumerKey = "";
        public static string TwitterConsumerSecret = "";
        public static string Twittertoken = "";
        public static string TwittertokenSecret = "";
        string CurrentGame = "Testing"; //changes the bots current game
        //txt channels
        string AdminTxtCh = "admins"; //Change this to what ever you want to call the admin text channel. In the future this may not be an option
                                      //as Discord may add its own admin channel for moderation. Once there is a default admin channel I will push
                                      //all our admin posts to that channel.
        string TwitterFeedName = "twitter"; //where tweets will be posted
        public static string TwitterToFollow = ""; //name of the person you want to follow, don't include @ in the name.
        //voip channels
        string MusicChName = "Music Player"; //where the bot will go when music is to be played
        //roles
        string JailedRoleName = "@Jailed"; //we have a jail channel for users that just need a time out
        string ModRoleName = "@Mod"; //our mods role
        string MemberRoleName = "@Member"; //we dont allow random people to use commands so people we know get this role.

        ///////////////////////////////////////////////
        ////Edit after this point at your own risk ////
        /////////////////////////////////////////////// 

        #region bot var
        public static DiscordClient bot;
        Random rnd = new Random();
        Dictionary<User, DateTime> MsgDictionary = new Dictionary<User, DateTime>();
        List<string> UserCommands = new List<string>(); //must add command to this lists in order for them to be used by a member
        List<string> ModCommands = new List<string>(); //must add command to this list in order for an admin to use them
        #endregion
        #region Timers
        static Timer TwitterTimer = new Timer();
        static Timer MessageTimer = new Timer();
        static Timer BanTimer = new Timer();
        static Timer RoleTimer = new Timer();
        static Timer RaffleTimer = new Timer();
        #endregion
        #region Setup
        
        //Text Channels     
        public static Channel AdminCh = null;
        public static Channel TwitterFeedCh = null;
        public static Channel TwitterBlizzCH = null;
        public static Channel TwitterRustCH = null;
        public static bool HasTwitterBlizz = true;
        public static bool HasTwitterRust = true;
        //Roles
        public static Role Mod;
        public static Role Member;
        public static Role Everyone;
        public static Role Jailed;
        //Voice Channels
        public static bool ChannelToGrant = false;
        public static Channel TestChannel = null;
        public static Channel MusicCh = null;
        //misc
        public static ulong BotID = 0;
        
        ChannelPermissionOverrides OwnerPermOverrides;
        ChannelPermissionOverrides RolePermOverrides;
        bool firstpass = true;
        #endregion 
        public YDCbot()
        {
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Music\");
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Save\");
            //Timers Elasped Event
            TwitterTimer.Elapsed += TwitterTimer_Elapsed;
            MessageTimer.Elapsed += MessageTimer_Elapsed;
            BanTimer.Elapsed += BanTimer_Elapsed;
            RoleTimer.Elapsed += RoleTimer_Elapsed;
            RaffleTimer.Elapsed += RaffleTimer_Elapsed;

            //adding commands to lists to compare for easy sorting
            UserCommands.Add("!imgur");
            UserCommands.Add("!roll");
            UserCommands.Add("!raffle");
            UserCommands.Add("!8ball");
            UserCommands.Add("!flipcoin");
            UserCommands.Add("!help");
            UserCommands.Add("!ban");

            ModCommands.Add("!clear");
            ModCommands.Add("!save");
            ModCommands.Add("!grantchannel");
            UserCommands.Add("!youtube");
            ModCommands.Add("!exitApp");
            //load last tweets for twitter bot
            TwitterBot.LoadTwitter();

            //DiscordBot Setup including connecting to server and message/channel events
            bot = new DiscordClient();
            bot.MessageReceived += Bot_MessageReceived;
            bot.ChannelCreated += Bot_ChannelCreated;                 
            bot.Connect(BotToken);
            bot.AddService<AudioService>(new AudioService(new AudioServiceConfigBuilder()
            {
                Channels = 2,
                EnableEncryption = false,
                Bitrate = null,
            }));
            bot.Wait(); //stops the program from running just once
        }
        private void Bot_ChannelCreated(object sender, ChannelEventArgs e)
        {  
            //This event is triggered on channel creation command !grantchannel

            if (e.Channel.IsPrivate) return; //exits out of this method if 
            if (ModLevelCommands.NewChannelOwner.Name != null) //double checks if channel was created by command or not
            {
                //setting permission rules for new channel with
                e.Channel.AddPermissionsRule(ModLevelCommands.NewChannelOwner, OwnerPermOverrides); //gives full rights to given user
                e.Channel.AddPermissionsRule(Member,RolePermOverrides);
                e.Channel.AddPermissionsRule(Mod, RolePermOverrides);
                e.Channel.AddPermissionsRule(Jailed, RolePermOverrides);
                e.Channel.AddPermissionsRule(Everyone, RolePermOverrides);
                ModLevelCommands.NewChannelOwner = null; //resets so that if Owner creates channel without command, the program wont enter this if statement
            }
        }
        public void Bot_MessageReceived(object sender, MessageEventArgs e)
        {
            //this event is triggered when the bot sees a new message either private or in a server channel 
            if (e.Message.IsAuthor) return; //returns if the bot sent the message
            
            if (firstpass == true) //loads data from the server, at some point need to store objects as json!?! so that a file is loaded instead of information from server
                LoadFromServer(e);
            if (e.Message.ToString().ToLower().Contains("!")) //if message contains "!" we enter this statemnet
            {
                //setting up var for MessageEventArgs in other classes and the two second cd
                bool CanSendMessage;
                string[] MessageArray;
                MessageArray = e.Message.Text.Split(null); //splits text by spaces and puts in an array which is used by other classes

                if (MsgDictionary.ContainsKey(e.User)) //if user is in dictionary enter loop
                {
                    if ((DateTime.Now - MsgDictionary[e.User]).TotalSeconds >= 2) //enter loop if last message was greater than 2 seconds agao
                    {
                        CanSendMessage = true;
                        MsgDictionary.Remove(e.User);
                        MsgDictionary.Add(e.User, DateTime.Now);
                    }
                    else
                    {
                        e.Message.Delete(); //deletes message in chat, repeated throughout code
                        e.User.SendMessage("Commands have a two second cooldown please try again!");
                        CanSendMessage = false;
                    }
                }
                else //enters else if is not appart of dictionary
                {
                    MsgDictionary.Add(e.User, DateTime.Now);
                    CanSendMessage = true;
                }
                if (UserCommands.Contains(MessageArray[0].ToLower()) && CanSendMessage) //checks if user can send message, and if they entered a command  in the List of commands
                {
                    e.Message.Delete();
                    UserLevelCommands.ExecuteCommands(e, MessageArray); //enters UserLevelCommands Class
                }
                if (e.User.Roles.Contains(Mod) && ModCommands.Contains(MessageArray[0].ToLower()) && CanSendMessage) //Checks if user can send message, checks if user is a mod, checks if they entered a command in the List of Mod commands
                {
                    e.Message.Delete();
                    ModLevelCommands.ExecuteModCommands(e, MessageArray); //enters ModLevelCommands Class
                }
            }
        }
        #region Timers
        //Elapsed Event Methods for timers
        private void MessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //prevents dictionary from filling up by reseting every 5min
            int count =0;
            Console.WriteLine("Resting Cooldowns");

            if(MsgDictionary != null)
               count = MsgDictionary.Count();

            if (count > 0)
            {
                MsgDictionary.Clear();
            }
            MessageTimer.Interval = 5 * 60 * 1000;
            MessageTimer.Start();
        }
        private void TwitterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //updates twitter posts
            TwitterTimer.Interval = 1 * 60 * 1000;
            TwitterTimer.Start();
            TwitterBot.TwitterUpdate();
        }
        private void RoleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopRollTimer();
        }
        private void BanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BanTimer.Stop();
            UserLevelCommands.ResetBan();
        }
        private void RaffleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopRaffleTimer();
        }     
        #endregion
        public void LoadFromServer(MessageEventArgs e)
        {
                IEnumerable<Role> Roles;
                IEnumerable<Channel> TextChannels;
                IEnumerable<Channel> VOIPchannels;
                User BotUser = null;
                BotID = bot.CurrentUser.Id;
                bot.SetGame(CurrentGame);

                if (!e.Message.Channel.IsPrivate)
                {
                    IEnumerable<User> OnlineUsers = null;
                    OnlineUsers = e.Server.Users;
                    foreach (User user in OnlineUsers)
                    {
                        if (user.Id == BotID)
                            BotUser = user;
                    }

                    TextChannels = e.Server.TextChannels;
                    VOIPchannels = e.Server.VoiceChannels;
                    Roles = e.Server.Roles;
                    

                    foreach (Channel ch in TextChannels)
                    {
                        if (ch.ToString() == AdminTxtCh)
                            AdminCh = ch;
                        if (ch.ToString() == TwitterFeedName)
                            TwitterFeedCh = ch;
                        if (ch.ToString() == "twitter-blizz")
                            TwitterBlizzCH = ch;
                        if (ch.ToString() == "twitter-rust")
                            TwitterRustCH = ch;

                }
                    foreach (Role role in Roles)
                    {
                        if (role.Name == ModRoleName)
                            Mod = role;
                        if (role.Name == MemberRoleName)
                            Member = role;
                        if (role.Name == JailedRoleName)
                            Jailed = role;
                        if (role.Name == "@everyone")
                            Everyone = role;
                    }
                    foreach (Channel ch in VOIPchannels)
                    {
                        if (ch.Name == "GrantChannel")//this is the channel that the grantchannel command uses to assign permisions
                            TestChannel = ch;
                        if (ch.Name == MusicChName)//this is the channel that the Bot will join when music is played.
                            MusicCh = ch;
                    }

                    firstpass = false;

                    if (MusicCh == null)
                    {
                         e.Server.CreateChannel(MusicChName, ChannelType.Voice);
                         firstpass = true;
                    }
                    if (AdminCh == null)
                    {
                        e.Server.CreateChannel(AdminTxtCh, ChannelType.Text);
                        firstpass = true;
                    }
                    if (TwitterFeedCh == null && TwitterToFollow != "")
                    {
                        e.Server.CreateChannel(TwitterFeedName, ChannelType.Text);
                        firstpass = true;
                    }
                    if (TwitterRustCH == null)
                    {
                        HasTwitterRust = false;
                    }
                    if (TwitterBlizzCH == null)
                    {
                        HasTwitterBlizz = false;
                    }

                    if (TestChannel != null)
                    {
                        ChannelToGrant = true;
                        OwnerPermOverrides = TestChannel.GetPermissionsRule(BotUser);
                        RolePermOverrides = TestChannel.GetPermissionsRule(Everyone);
                    }
                    else
                    {
                        e.Server.CreateChannel("GrantChannel", ChannelType.Voice);
                    }             
                    TwitterTimer.Interval = 1 * 60 * 1000;
                    TwitterTimer.Start();
                    MessageTimer.Interval = 5 * 60 * 1000;
                    MessageTimer.Start();
                    TwitterBot.TwitterUpdate();            
            }          
        }
        //Methods to communicate between classes and timers
        public static void StartRaffleTimer(int timerV)
        {
            RaffleTimer.Interval = timerV * 60 * 1000;
            RaffleTimer.Start();
        }
        public static void StopRaffleTimer()
        {
            RaffleTimer.Stop();
            UserLevelCommands.RaffleEnded();
        }
        public static void StartRollTimer()
        {
            RoleTimer.Interval = 1 * 60 * 1000;
            RoleTimer.Start();
        }
        public static void StopRollTimer()
        {
            UserLevelCommands.RollEnded();
            RoleTimer.Stop();
        }
        public static void StartBanTimer()
        {
            BanTimer.Interval = 5 * 60 * 1000;
            BanTimer.Start();
        }
        public static void StopBanTimer()
        {
            BanTimer.Stop();
        }
        /*
        public void SaveFile()
        {
            //set up var
            List<Channel> ChannelsToSave = new List<Channel>();
            List<Role> RolesToSave = new List<Role>();
            var jsonSerialiser = new JavaScriptSerializer();
            Console.WriteLine("Saving File");

            //saves channels and roles to list
            ChannelsToSave.Add(AdminCh);
            ChannelsToSave.Add(TwitterBlizzCH);
            ChannelsToSave.Add(TwitterRustCH);
            ChannelsToSave.Add(MembersLobby);
            ChannelsToSave.Add(General);
            ChannelsToSave.Add(BloodyCh);
            ChannelsToSave.Add(AFKch);
            ChannelsToSave.Add(JailCh);
            ChannelsToSave.Add(TestChannel);
            RolesToSave.Add(Mod);
            RolesToSave.Add(Everyone);
            RolesToSave.Add(Member);
            RolesToSave.Add(Jailed);

            var CHjson = jsonSerialiser.Serialize(ChannelsToSave);
            TextWriter tw1 = new StreamWriter(@"C:\Users\1\desktop\DiscordBot\ChannelsSave.txt");
            tw1.WriteLine(CHjson);
            tw1.Close();

            var Rolesjson = jsonSerialiser.Serialize(RolesToSave);
            TextWriter tw2 = new StreamWriter(@"C:\Users\1\desktop\DiscordBot\RolesSave.txt");
            tw2.WriteLine(Rolesjson);
            tw2.Close();       
        } 
        */
    }
}
