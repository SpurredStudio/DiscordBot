using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Audio;
using YoutubeExtractor;
using System.IO;
using NAudio.Wave;
using System.Text;

namespace DiscordBot
{
    class MusicPlayer
    {
        public static bool IsPlaying = false;
        static public DiscordClient bot = YDCbot.bot;
        static Channel MusicCh = YDCbot.MusicCh;
        static Role Mod = YDCbot.Mod;
        public static string MusicFileTitle;
        public static bool ExitLoop;
        public static List<string> Playlist = new List<string>();

        public static async void PlayYouTube(string link, MessageEventArgs e)
        {
            IsPlaying = true;
            var vbot = await MusicCh.JoinAudio();
            string BaseFilePath = System.AppDomain.CurrentDomain.BaseDirectory+@"Music\";
            try
            {
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
                VideoInfo video = videoInfos
                    .Where(info => info.CanExtractAudio)
                    .OrderByDescending(info => info.AudioBitrate)
                    .First();
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                Console.WriteLine(video.AudioType.ToString());
                MusicFileTitle = RemoveSpecialCharacters(video.Title) + video.AudioExtension;

                if (!File.Exists(BaseFilePath + MusicFileTitle))
                {
                    var audioDownloader = new AudioDownloader(video, Path.Combine(BaseFilePath, MusicFileTitle));
                    if (audioDownloader.BytesToDownload > 2000000 && !e.User.HasRole(Mod))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        e.User.SendMessage("The audio file is to large to download, try a shorter youtube video.");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        return;
                    }
                    try
                    {
                        audioDownloader.Execute();
                        Console.WriteLine("Download Complete");
                    }
                    catch
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        e.User.SendMessage("could not find file location");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Console.WriteLine("Error file location");
                    }             
                }
            }
            catch(YoutubeParseException w)
            {
                await MusicCh.LeaveAudio();
                await vbot.Disconnect();
                IsPlaying = false;
                Console.WriteLine(w.Message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                e.User.SendMessage(w.Message);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            string filePath = BaseFilePath + MusicFileTitle;
            var OutFormat = new WaveFormat(48000, 16, 2); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.

            if (!File.Exists(filePath))
            {
                await MusicCh.LeaveAudio();               
                await vbot.Disconnect();
                IsPlaying = false;
                return;
            }
            Playlist.Add(filePath);
            var MP3Reader = new Mp3FileReader(filePath);
            MP3Reader.Seek(0, SeekOrigin.Begin);
            var resampler = new MediaFoundationResampler(MP3Reader, OutFormat);// Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            Console.WriteLine("Converted MP3 to Opus");
            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
            int blockSize = OutFormat.AverageBytesPerSecond / 10; // Establish the size of our AudioBuffer
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && !ExitLoop) // Read audio into our buffer, and keep a loop open while data is present
            {
                if (byteCount < blockSize)
                {
                    // Incomplete Frame
                    for (int i = byteCount; i < blockSize; i++)
                        buffer[i] = 0;
                }

                vbot.Send(buffer, 0, blockSize); // Send the buffer to Discord
            }
            ExitLoop = false;
            resampler.Dispose();
            vbot.Clear();
            if (Playlist.Count() > 10)
            {
                DeletePlaylist();
            }
            await vbot.Disconnect();
            await MusicCh.LeaveAudio();
            IsPlaying = false;
       
        }
        public static void DeletePlaylist()
        {
            foreach (string song in Playlist)
            {
                if (File.Exists(song))
                {
                    File.Delete(song);
                }
                Playlist.Remove(song);
            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
