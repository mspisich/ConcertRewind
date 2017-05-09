using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ConcertRewind.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return GetSetList("bruce%20springsteen");
        }

        public ActionResult GetSetList(string artistName)
        {

            HttpWebRequest request =

            //Load setlist json for chosen artist
            WebRequest.CreateHttp("http://api.setlist.fm/rest/0.1/search/setlists.json?artistName=" + artistName);

            //Tells the user what browsers we're using
            request.UserAgent = @"User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36";

            //actually grabs the request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //gets a stream of text
            StreamReader rd = new StreamReader(response.GetResponseStream());

            //reads to the end of file
            string ApiText = rd.ReadToEnd();

            //Converts that text into JSON
            JObject setlist = JObject.Parse(ApiText);


            List<string> songsPlayed = new List<string>();

            //Check through each set from concert
            for(int setIndex = 0; setIndex < setlist["setlists"]["setlist"][0]["sets"]["set"].Count(); setIndex++)
            {
                //Check through each song from set
                for (int songIndex = 0; songIndex < setlist["setlists"]["setlist"][0]["sets"]["set"][setIndex]["song"].Count(); songIndex++)
                {
                    //Add song to list
                    songsPlayed.Add((setlist["setlists"]["setlist"][0]["sets"]["set"][setIndex]["song"][songIndex]["@name"]).ToString());
                }
            }

            foreach(string song in songsPlayed)
            {
                ViewBag.Text += song;
            }
            

            //Go to results view
            return View("Index");
        }
    }
}
