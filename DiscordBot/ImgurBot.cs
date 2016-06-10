using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;

namespace DiscordBot
{
    class ImgurBot
    {
        static Uri BaseURL = new Uri("https://api.imgur.com/3/gallery/r/");
        static Random rnd = new Random();
        static string ImgurClientID = YDCbot.ImgurClientID;

        public static void ImgurUpdate(string ImgurSub, int ImageNumber, MessageEventArgs e)
        {         
            ImgurGallery oRootObject = new ImgurGallery();
            JavaScriptSerializer oJS = new JavaScriptSerializer();
            List<ImgurDatum> GalleryList = new List<ImgurDatum>();
            HttpWebRequest webRequest;       
            
            int Max = 200;
            int ind = ImageNumber;
            string ImageUrl = "";
            Uri ImgurURL = new Uri(BaseURL, ImgurSub);            
            Stream response = null;
            StreamReader reader = null;
            string responseFromServer = "";

            try //trys to get response from web, if fails it returns an exception
            {
                webRequest = (HttpWebRequest)WebRequest.Create(ImgurURL);
                webRequest.Timeout = (System.Int32)TimeSpan.FromSeconds(1.75f).TotalMilliseconds;
                webRequest.Headers.Add("Authorization", "Client-ID " + ImgurClientID);
                response = webRequest.GetResponse().GetResponseStream();
                reader = new StreamReader(response);
                responseFromServer= reader.ReadToEnd();
            }
            catch (WebException E)
            {
                Console.WriteLine(E.Status.ToString());
                e.User.SendMessage("Error: "+E.Status.ToString());
            }

            //closes reader and response
            if (reader != null)
                reader.Close();
            if (response != null)
                response.Close();

            try //for some reason json wasn't the only response that was returning, something filled with ///// would also return making Deserialize fail
            {
                oRootObject = oJS.Deserialize<ImgurGallery>(responseFromServer);
            }
            catch
            {

            }       
                
            if (oRootObject !=null)
            {
                try
                {
                    GalleryList = oRootObject.data.OfType<ImgurDatum>().ToList();
                }
                catch
                {

                }
                if (GalleryList.Count == 0)
                {
                    return;
                }

                if (GalleryList.Count < Max)
                {
                    Max = GalleryList.Count;
                }
                if (ImageNumber == 0)
                {
                    ind = rnd.Next(0, Max);
                }
                else if (ImageNumber >= Max)
                    ind = Max - 1;

                //sends correct image type to Discord chat
                if (oRootObject.data[ind].gifv != null)
                {
                    ImageUrl = "http://i.imgur.com/" + oRootObject.data[ind].id + ".gifv";
                    GalleryList.Clear();
                }
                else
                {
                    ImageUrl = "http://i.imgur.com/" + oRootObject.data[ind].id + ".jpg";
                    GalleryList.Clear();
                }

                e.Channel.SendMessage(ImageUrl);
            }
        }
    }
    public class ImgurDatum //sets up image object
    {
        public string id { get; set; }
        public string gifv { get; set; }
    }
    public class ImgurGallery //sets up gallery object
    {
        public List<ImgurDatum> data { get; set; } //gallery is just a list of image objects
        public bool success { get; set; }
        public int status { get; set; }
    }
}
