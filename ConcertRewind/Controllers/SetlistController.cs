using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConcertRewind.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ConcertRewind.Controllers
{
    public class SetlistController : Controller
    {
        public static JObject setlistApi;

        public string Replace(string info)
        {
            string output = info.Replace(" ", "+");


            return output;
        }

        public ActionResult Results(string artistName)
        {
            setlistApi = GenerateSetlistApi(artistName);

            //Generate artist object
            //artist artist = new Models.artist(artistName);

            //Get list of concert objects
            List<concert> recentConcerts = GetConcerts(artistName);
            /*
            foreach (concert concert in recentConcerts)
            {
                string concertInfo = concert.city + ", " + concert.state + ": " + concert.date;
                //ViewBag.recentConcerts += "<li>" + concertInfo + " " + "<input type=\"button\" value=\"Go to Concert\" onclick=\"location.href = '<%: Url.Action(\"Concert\", \"Setlist\", new { parameter1 = \"concert\" }) %>\" />"</li>";

                ViewBag.recentConcerts += "<li>@Html.ActionLink(\"" + concertInfo + "\", \"Concert\", \"Setlist\", new {parameter1=\"concert\"})</li>";


            }*/

            return View(recentConcerts);
        }


        //Get Setlist API JSON object
        private static JObject GenerateSetlistApi(string artistName)
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
            JObject setlistApi = JObject.Parse(ApiText);

            return setlistApi;
        }


        //Return Concert view for concert matching id on setlist.fm API
        public ActionResult Concert(string artistName, string concertId)
        {
            //Generate artist object
            //artist a = new Models.artist(artistName);

            //Get list of concert objects
            List<concert> recentConcerts = GetConcerts(artistName);
            
            foreach(concert c in recentConcerts)
            {
                if(c.id == concertId)
                {
                    ViewBag.artist = c.artist;
                    ViewBag.date = c.date;
                    ViewBag.location = c.city + ", " + c.state;
                    ViewBag.tour = c.tour;

                    foreach (string song in c.songsPlayed)
                    {
                        string artist = Replace(ViewBag.artist);
                        string songTwo = Replace(song);
                        string location = Replace(ViewBag.location);

                        ViewBag.songsPlayed += "<li> <a href=https://www.youtube.com/results?search_query=" + songTwo + "+" + location + artist + " " + "target =_blank" + ">" + song + "</a> </li>";
                    }
                }
            }
            return View();
        }

        //Get a list of recent concerts by an artist (up to 10)
        public static List<concert> GetConcerts(string artistName)
        {
            //Get name and JObject for artist
            //string artistName = a.name;
            //JObject setlist = a.setlistApi;

            List<concert> recentConcerts = new List<concert>();

            int totalConcerts = setlistApi["setlists"]["setlist"].Count();

            if(totalConcerts < 10)
            {
                for(int i = 0; i < totalConcerts; i++)
                {
                    recentConcerts.Add(GetSetList(artistName, i));
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    recentConcerts.Add(GetSetList(artistName, i));
                }
            }

            return recentConcerts;
        }

        //Generate concert object
        public static concert GetSetList(string artistName, int concertIndex)
        {
            //Get name and JObject for artist
            //string artistName = a.name;
            //JObject setlist = a.setlistApi;

            //Get date and location of previous concert
            string artist = setlistApi["setlists"]["setlist"][concertIndex]["artist"]["@name"].ToString();

            string date = "Date info not available.";
            try
            {
                date = setlistApi["setlists"]["setlist"][concertIndex]["@eventDate"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No date info available for this concert.");
            }

            string city = "City info not available.";
            try
            {
                city = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["city"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No city info available for this concert.");
            }

            string state = "State/country info not available.";
            try
            {
                state = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["city"]["country"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No state/country info available for this concert.");
            }

            //string state = setlist["setlists"]["setlist"][concertIndex]["venue"]["city"]["@state"].ToString();

            string venue = "Venue info not available.";
            try
            {
                venue = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No venue info available for this concert.");
            }

            string tour = "n/a";
            try
            {
                tour = setlistApi["setlists"]["setlist"][concertIndex]["@tour"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No tour info available for this concert.");
            }

            string id = setlistApi["setlists"]["setlist"][concertIndex]["@id"].ToString();

            List<string> songsPlayed = new List<string>();

            int totalSets = setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"].Count();

            //Check through each set from concert

            //Case of no songs
            if (totalSets <= 0)
            {
                //***********Add exception here!************
            }
            //Case of only one set at concert
            else if (totalSets == 1)
            {
                //Check through each song from set
                for (int songIndex = 0; songIndex < setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"].Count(); songIndex++)
                {
                    //Add song to list
                    songsPlayed.Add((setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"][songIndex]["@name"]).ToString());
                }
            }
            //Case of multiple sets at concert
            else
            {
                for (int setIndex = 0; setIndex < setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"].Count(); setIndex++)
                {
                    //Check through each song from set
                    for (int songIndex = 0; songIndex < setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"][setIndex]["song"].Count(); songIndex++)
                    {
                        //Add song to list
                        songsPlayed.Add((setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"][setIndex]["song"][songIndex]["@name"]).ToString());
                    }
                }
            }

            concert concert = new Models.concert(artist, date, city, state, venue, tour, songsPlayed, id);

            //Go to results view
            return concert;
        }
    }
}