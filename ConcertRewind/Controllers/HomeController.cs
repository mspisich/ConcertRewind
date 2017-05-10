using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ConcertRewind.Models;

namespace ConcertRewind.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Results(string artistName)
        {
            //Get concert info from setlist API
            concert c = GetSetList(artistName);

            ViewBag.artist = c.artist;
            ViewBag.date = c.date;
            ViewBag.location = c.location;

            foreach (string song in c.songsPlayed)
            {
                ViewBag.songsPlayed += "<li>" + song + "</li>";
            }

            return View();
        }

        public static concert GetSetList(string artistName)
        {

            HttpWebRequest request =

            //Load setlist json for chosen artist from setlist.fm API
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

            //Get date and location of previous concert
            string artist = setlist["setlists"]["setlist"][0]["artist"]["@name"].ToString();
            string date = setlist["setlists"]["setlist"][0]["@eventDate"].ToString();
            string location = setlist["setlists"]["setlist"][0]["venue"]["city"]["@name"].ToString() + ", " + setlist["setlists"]["setlist"][0]["venue"]["city"]["@state"].ToString();

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

            concert concert = new Models.concert(artist, date, location, songsPlayed);
            
            //Go to results view
            return concert;
        }
    }
}
