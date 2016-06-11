using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;
using System.IO;

namespace DiscordBot
{
    class TwitterBot
    {       
        static int TweetAmount = 1;
        static string ConsumerKey = YDCbot.TwitterConsumerKey;
        static string ConsumerSecret = YDCbot.TwitterConsumerSecret;
        static string token = YDCbot.Twittertoken;
        static string tokenSecret = YDCbot.TwittertokenSecret;
        static List<TwitterObject> TwitterAccounts = new List<TwitterObject>();
        static TwitterObject Rust = new TwitterObject();
        static TwitterObject Overwatch = new TwitterObject();
        static TwitterObject Starcraft = new TwitterObject();
        static TwitterObject Warcraft = new TwitterObject();
        static TwitterObject Heroes = new TwitterObject();
        static TwitterObject Diablo = new TwitterObject();
        static TwitterObject Hearthstone = new TwitterObject();
        static TwitterObject FollowedTwitter = new TwitterObject();
        static bool hardreset = false;
        static int resetTime = 0;

        public static void TwitterUpdate()
        {
            Rust.TwitterAccount = "RustUpdates";
            Warcraft.TwitterAccount = "Warcraft";
            Overwatch.TwitterAccount = "PlayOverwatch";
            Starcraft.TwitterAccount = "Starcraft";
            Diablo.TwitterAccount = "Diablo";
            Hearthstone.TwitterAccount = "PlayHearthstone";
            Heroes.TwitterAccount = "BlizzHeroes";

            if (hardreset)
            {
                if (resetTime >= 15)
                {
                    hardreset = false;
                    resetTime = 0;
                }    
                else
                {
                    resetTime++;
                    return;
                }
            }

            
            //gets channels to send tweets to
            Channel TwitterRustCH = YDCbot.TwitterRustCH;
            Channel TwitterBlizzCH = YDCbot.TwitterBlizzCH;
            Channel TwitterFeedCh = YDCbot.TwitterFeedCh;

            //starts a service with twitter to GET tweets
            var service = new TwitterService(ConsumerKey, ConsumerSecret);
            service.AuthenticateWith(token, tokenSecret);
            //sets up a GET status request for each user, with given name and tweet amounts
            //then posts those tweets in their correct channel   

            
            if (YDCbot.TwitterToFollow != "" && TwitterFeedCh != null)
            {
                List<TwitterStatus> TwitterStatuses = new List<TwitterStatus>();
                var Tweetoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Tweetoptions.ScreenName = YDCbot.TwitterToFollow;
                if (FollowedTwitter.LastTweet != 0)
                {
                    Tweetoptions.SinceId = FollowedTwitter.LastTweet;
                }
                else
                    Tweetoptions.Count = TweetAmount;
                Tweetoptions.ExcludeReplies = true;
                TwitterStatuses = service.ListTweetsOnUserTimeline(Tweetoptions).ToList();
                if (TwitterStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in TwitterStatuses)
                    {
                        if (tweet.Id > FollowedTwitter.LastTweet)
                            FollowedTwitter.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + YDCbot.TwitterToFollow + "/status/" + tweet.Id + " ```";

                        TwitterFeedCh.SendMessage(TweetToDiscord);
                    }
                }
            }


            //used as examples, will only work if user has correct text channels in their server
            //YDCbot.cs has the void that sets everything up, check for correct channel names
            #region RustUpdates           
            if (YDCbot.HasTwitterRust)
            {
                List<TwitterStatus> RustStatuses = new List<TwitterStatus>();
                var Rustoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Rustoptions.ScreenName = Rust.TwitterAccount;
                if (Rust.LastTweet != 0)
                {
                    Rustoptions.SinceId = Rust.LastTweet;
                }
                else
                    Rustoptions.Count = TweetAmount;
                Rustoptions.ExcludeReplies = true;
                RustStatuses = service.ListTweetsOnUserTimeline(Rustoptions).ToList();
                if (RustStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in RustStatuses)
                    {
                        if (tweet.Id > Rust.LastTweet)
                            Rust.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Rust.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterRustCH.SendMessage(TweetToDiscord);
                    }
                }
            }
            #endregion
            if (YDCbot.HasTwitterBlizz)
            {
                #region Diablo
                List<TwitterStatus> DiabloStatuses = new List<TwitterStatus>();
                var Diablooptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Diablooptions.ScreenName = Diablo.TwitterAccount;
                if (Diablo.LastTweet != 0)
                {
                    Diablooptions.SinceId = Diablo.LastTweet;
                }               
                else
                    Diablooptions.Count = TweetAmount;
                Diablooptions.ExcludeReplies = true;
                DiabloStatuses = service.ListTweetsOnUserTimeline(Diablooptions).ToList();
                if (DiabloStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in DiabloStatuses)
                    {
                        if (tweet.Id > Diablo.LastTweet)
                            Diablo.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Diablo.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterBlizzCH.SendMessage(TweetToDiscord);
                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }

                    }
                }
                #endregion
                #region Warcraft
                List<TwitterStatus> WarcraftStatuses = new List<TwitterStatus>();
                var Warcraftoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Warcraftoptions.ScreenName = Warcraft.TwitterAccount;
                if (Warcraft.LastTweet != 0)
                {
                    Warcraftoptions.SinceId = Warcraft.LastTweet;
                }
                else
                    Warcraftoptions.Count = TweetAmount;
                Warcraftoptions.ExcludeReplies = true;
                WarcraftStatuses = service.ListTweetsOnUserTimeline(Warcraftoptions).ToList();
                if (WarcraftStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in WarcraftStatuses)
                    {
                        if (tweet.Id > Warcraft.LastTweet)
                            Warcraft.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Warcraft.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterBlizzCH.SendMessage(TweetToDiscord);
                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }

                    }
                }
                #endregion
                #region Starcraft
                List<TwitterStatus> StarcraftStatuses = new List<TwitterStatus>();
                var Starcraftoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Starcraftoptions.ScreenName = Starcraft.TwitterAccount;
                if (Starcraft.LastTweet != 0)
                {
                    Starcraftoptions.SinceId = Starcraft.LastTweet;
                }
                else
                    Starcraftoptions.Count = TweetAmount;
                Starcraftoptions.ExcludeReplies = true;
                StarcraftStatuses = service.ListTweetsOnUserTimeline(Starcraftoptions).ToList();
                if (StarcraftStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in StarcraftStatuses)
                    {
                        if (tweet.Id > Starcraft.LastTweet)
                            Starcraft.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Starcraft.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterBlizzCH.SendMessage(TweetToDiscord);
                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }

                    }
                }
                #endregion
                #region Heroes
                List<TwitterStatus> HeroesStatuses = new List<TwitterStatus>();
                var Heroesoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Heroesoptions.ScreenName = Heroes.TwitterAccount;
                if (Heroes.LastTweet != 0)
                {
                    Heroesoptions.SinceId = Heroes.LastTweet;
                }
                else
                    Heroesoptions.Count = TweetAmount;
                Heroesoptions.ExcludeReplies = true;
                HeroesStatuses = service.ListTweetsOnUserTimeline(Heroesoptions).ToList();
                if (HeroesStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in HeroesStatuses)
                    {
                        if (tweet.Id > Heroes.LastTweet)
                            Heroes.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Heroes.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterBlizzCH.SendMessage(TweetToDiscord);
                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }

                    }
                }
                #endregion
                #region Hearthstone
                List<TwitterStatus> HearthstoneStatuses = new List<TwitterStatus>();
                var Hearthoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Hearthoptions.ScreenName = Hearthstone.TwitterAccount;
                if (Hearthstone.LastTweet != 0)
                {
                    Hearthoptions.SinceId = Hearthstone.LastTweet;
                }
                else
                    Hearthoptions.Count = TweetAmount;
                Hearthoptions.ExcludeReplies = true;
                HearthstoneStatuses = service.ListTweetsOnUserTimeline(Hearthoptions).ToList();
                if (HearthstoneStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in HearthstoneStatuses)
                    {
                        if (tweet.Id > Hearthstone.LastTweet)
                            Hearthstone.LastTweet = tweet.Id;
                        string TweetToDiscord = "``` " + "https://twitter.com/" + Hearthstone.TwitterAccount + "/status/" + tweet.Id + " ```";

                        TwitterBlizzCH.SendMessage(TweetToDiscord);
                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }

                    }
                }
                #endregion
                #region Overwatch
                List<TwitterStatus> OverwatchStatuses = new List<TwitterStatus>();
                var Overwatchoptions = new TweetSharp.ListTweetsOnUserTimelineOptions();
                Overwatchoptions.ScreenName = Overwatch.TwitterAccount;
                if (Overwatch.LastTweet != 0)
                {
                    Overwatchoptions.SinceId = Overwatch.LastTweet;
                }
                else
                    Overwatchoptions.Count = TweetAmount;
                Overwatchoptions.ExcludeReplies = true;
                OverwatchStatuses = service.ListTweetsOnUserTimeline(Overwatchoptions).ToList();
                if (OverwatchStatuses.Count > 0)
                {
                    foreach (TwitterStatus tweet in OverwatchStatuses)
                    {
                        if (tweet.Id > Overwatch.LastTweet)
                            Overwatch.LastTweet = tweet.Id;

                        string TweetToDiscord = "``` " + "https://twitter.com/" + Overwatch.TwitterAccount + "/status/" + tweet.Id + " ```";
                        TwitterBlizzCH.SendMessage(TweetToDiscord);

                        if (tweet.Entities.Media.Count > 0)
                        {
                            foreach (var link in tweet.Entities.Media)
                            {
                                TwitterBlizzCH.SendMessage(link.MediaUrlHttps);
                            }
                        }



                    }
                }
                #endregion
            }
            TwitterRateLimitStatus rate = service.Response.RateLimitStatus; //checking rate limits to prevent twitter bot from being ip banned
            Console.WriteLine("You have " + rate.RemainingHits + " remaining out of the " + rate.HourlyLimit + " limit. @" + DateTime.Now.ToString("h:mm:ss tt"));
            if (rate.RemainingHits < 25)
            {
                hardreset = true;
                return;
            }
            SaveTwitter();
            
        }
        public static void SaveTwitter()
        {
            //saves last tweet from each twitter account
            Console.WriteLine("Saving Twitter");
            TweetAmount = 1;
            TextWriter tw = new StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory+@"\Save\TwitterSave.txt");
            tw.WriteLine(Heroes.LastTweet);
            tw.WriteLine(Warcraft.LastTweet);
            tw.WriteLine(Heroes.LastTweet);
            tw.WriteLine(Diablo.LastTweet);
            tw.WriteLine(Starcraft.LastTweet);
            tw.WriteLine(Overwatch.LastTweet);
            tw.WriteLine(Rust.LastTweet);
            tw.WriteLine(TweetAmount);
            tw.Close();

        }
        public static void LoadTwitter()
        {
            
            //ran at the first of the program, loads last tweet from each user
            Console.WriteLine("Loading Twitter");

            if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\Save\TwitterSave.txt"))
            {
                return;
            }

            TextReader tr = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory+@"\Save\TwitterSave.txt");

            string LastHearthstoneTweetstring = tr.ReadLine();
            string LastWarcraftTweetstring = tr.ReadLine();
            string LastHeroesTweetstring = tr.ReadLine();
            string LastDiabloTweetstring = tr.ReadLine();
            string LastStarcraftTweetstring = tr.ReadLine();
            string LastOverwatchTweetstring = tr.ReadLine();
            string LastRustTweetstring = tr.ReadLine();
            string TweetAmountstring = tr.ReadLine();
            tr.Close();



            TweetAmount = Convert.ToInt32(TweetAmountstring);
            Hearthstone.LastTweet = Convert.ToInt64(LastHearthstoneTweetstring);
            Warcraft.LastTweet = Convert.ToInt64(LastWarcraftTweetstring);
            Heroes.LastTweet = Convert.ToInt64(LastHeroesTweetstring);
            Diablo.LastTweet = Convert.ToInt64(LastDiabloTweetstring);
            Starcraft.LastTweet = Convert.ToInt64(LastStarcraftTweetstring);
            Overwatch.LastTweet = Convert.ToInt64(LastOverwatchTweetstring);
            Rust.LastTweet = Convert.ToInt64(LastRustTweetstring);          
            TwitterAccounts.Add(Rust);
            TwitterAccounts.Add(Overwatch);
            TwitterAccounts.Add(Starcraft);
            TwitterAccounts.Add(Warcraft);
            TwitterAccounts.Add(Heroes);
            TwitterAccounts.Add(Hearthstone);
            TwitterAccounts.Add(Diablo);

        }
        public class TwitterObject
        {
            public string TwitterAccount { get; set; }
            public long LastTweet { get; set; }
        }
    }
}