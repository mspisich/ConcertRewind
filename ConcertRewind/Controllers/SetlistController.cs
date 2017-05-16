using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConcertRewind.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data.Entity;
using System.Data;
using System.Data.SqlClient;
using System.Xml;


namespace ConcertRewind.Controllers
{
    public class SetlistController : Controller
    {
        //Setlist.fm JSON JObject to be shared by controller
        public static JObject setlistApi;
        public static ConcertDBEntities db = new ConcertDBEntities();
        

        public static void addtoDB(string artistname)
        {
            artistname = artistname.ToLower();
            
            if (db.ConcertDBs.Any(u => u.Artist_Name == artistname))
            {
                var query =
               from u in db.ConcertDBs
               where u.Artist_Name == artistname
               select u;
                List < ConcertDB > List = new List<ConcertDB>();
                List = query.ToList();

               foreach (var u in List)
                {
                    u.Times_Searched = u.Times_Searched + 1;
                    db.SaveChanges();
                }



            }
            else
            {
                var concert = new ConcertDB();
                concert.Artist_Name = artistname;
                concert.Times_Searched = 1;
                db.ConcertDBs.Add(concert);
                db.SaveChanges();



            }




        }


        //Replace spaces with '+' for YouTube search queries
        public static string Replace(string info)
        {
            string output = info.Replace(" ", "+");

            return output;
        }

        public ActionResult Results(string artistName)
        {
            //Generate JObject for JSON from Setlist.fm API
            bool test;
            setlistApi = GenerateSetlistApi(artistName);

            addtoDB(artistName);
            
            //Go to error view if Setlist API didn't load properly
            if (setlistApi == null)
            {
                ViewBag.errorMessage = "There is no data available for this artist, or there was a problem connecting to the Setlist.fm server. Please try again.";
                return View("error");
            }

            

            //Generate list of concert objects
            List<concert> recentConcerts = GetConcerts(artistName);

            //Go to error view if no concert data available
            if(recentConcerts == null)
            {
                ViewBag.errorMessage = "There is no concert data available for artist \"" + artistName + "\".";
                return View("error");
            }

            return View(recentConcerts);
        }


        //Generate Setlist API JSON JObject
        private static JObject GenerateSetlistApi(string artistName)
        {
            string apiKey = "d62d3a2f-a8c2-45a7-aa2e-405dc018fb62";
            
            try
            {
                HttpWebRequest request =

                //Load setlist json for chosen artist from setlist.fm API
                WebRequest.CreateHttp("http://api.setlist.fm/rest/0.1/search/setlists.json?artistName=" + artistName + "&?key=" + apiKey);

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
            }
            catch(System.Net.WebException)
            {
                Console.WriteLine("Unable to communicate with Setlist.fm API.");
                setlistApi = null;
            }


            /*
            //Backup JSON if Setlist.fm is not working during demo! Type "Rihanna" at index.
            StreamReader rd = new StreamReader(@"C:\Users\Mike Spisich\Documents\Visual Studio 2015\Projects\ConcertRewind\ConcertRewind\setlistapibackup");
            //StreamReader rd = new StreamReader(@"C:\Users\Mike Spisich\Documents\Visual Studio 2015\Projects\ConcertRewind\ConcertRewind\jimmyeatworldjson");
            string ApiText = rd.ReadToEnd();

            //Converts that text into JSON
            JObject setlistApi = JObject.Parse(ApiText);
            */


            return setlistApi;
        }

        //Search youtube for song, return video ID of top result
        public static string SearchYoutube(string artistName, string songName)
        {
            JObject youtubeApi;

            List<string> videoIds = new List<string>();

            string apiKey = "AIzaSyAfk5WMcg_rX2fZ-0mI9Id6aDXRaE_etcc";

            //Search for and get YouTube video ID for each song played during concert
            string searchTerm = Replace(artistName) + "+" + Replace(songName);

            try
            {
                HttpWebRequest request =

                //Load setlist json for chosen artist from setlist.fm API
                WebRequest.CreateHttp("https://www.googleapis.com/youtube/v3/search?part=id&q=" + searchTerm + "&type=video&key=" + apiKey);

                //Tells the user what browsers we're using
                request.UserAgent = @"User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36";

                //actually grabs the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //gets a stream of text
                StreamReader rd = new StreamReader(response.GetResponseStream());

                //reads to the end of file
                string ApiText = rd.ReadToEnd();

                //Converts that text into JSON
                youtubeApi = JObject.Parse(ApiText);
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Unable to communicate with YouTube API.");
                youtubeApi = null;
            }

            string videoId = null;

            try
            {
                videoId = youtubeApi["items"][0]["id"]["videoId"].ToString();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Youtube search results unavailable for song \"" + songName + "\".");
            }

            return videoId;
        }

        //Return Concert view for concert matching id on setlist.fm API
        public ActionResult Concert(string artistName, string concertId)
        {
            //Generate list of concert objects
            List<concert> recentConcerts = GetConcerts(artistName);
            
            foreach(concert c in recentConcerts)
            {
                if(c.id == concertId)
                {
                    ViewBag.artist = c.artist;
                    ViewBag.date = c.date;
                    ViewBag.location = c.city + ", " + c.state;
                    ViewBag.tour = c.tour;
                    ViewBag.songsPlayed = "";

                    //Generate list of Youtube video IDs for playlist     
                    ViewBag.videoIds = "";

                    for(int i = 0; i < c.songsPlayed.Count; i++)
                    {
                        ViewBag.videoIds += SearchYoutube(artistName, c.songsPlayed[i]);

                        //Don't add comma after last id
                        if(i < (c.songsPlayed.Count - 1))
                        {
                            ViewBag.videoIds += ",";
                        }
                    }

                    foreach (string song in c.songsPlayed)
                    {
                        string artist = Replace(ViewBag.artist);
                        string songTwo = Replace(song);
                        string location = Replace(ViewBag.location);

                        ViewBag.songsPlayed += "<li> <a href=https://www.youtube.com/results?search_query=" + songTwo + "+" + location + "+" + artist + " " + "target =_blank" + ">" + song + "</a> </li>";
                    }
                    //End loop once matching concert is found
                    break;
                }
            }
            return View();
        }

        //Get a list of recent concerts by an artist (up to 10)
        public static List<concert> GetConcerts(string artistName)
        {
            List<concert> recentConcerts = new List<concert>();

            int totalConcerts = 0;
            try
            {
                totalConcerts = setlistApi["setlists"]["setlist"].Count();
            }
            catch (Exception)
            {
                Console.WriteLine("There is no concert info available for this artist.");
                return null;
            }

            if (totalConcerts == 0)
            {

            }
            else if(totalConcerts < 10)
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
            //Get info (date, location, etc.) of concert
            string artist;
            try
            {
                artist = setlistApi["setlists"]["setlist"][concertIndex]["artist"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                artist = artistName;
            }

            string date = null;
            try
            {
                date = setlistApi["setlists"]["setlist"][concertIndex]["@eventDate"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No date info available for this concert.");
            }

            string city = null;
            try
            {
                city = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["city"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No city info available for this concert.");
            }

            //Find state. If no state info available, try to find country instead.
            string state = null;
            try
            {
                state = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["city"]["@state"].ToString();
            }
            catch (System.NullReferenceException)
            {
                try
                {
                    state = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["city"]["country"]["@name"].ToString();
                }
                catch (System.NullReferenceException)
                {
                    Console.WriteLine("No state/country info available for this concert.");
                }
            }

            string venue = null;
            try
            {
                venue = setlistApi["setlists"]["setlist"][concertIndex]["venue"]["@name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No venue info available for this concert.");
            }

            string tour = null;
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

            int totalSets = 0;
            try
            {
                totalSets = setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"].Count();
            }
            catch (Exception)
            {
                Console.WriteLine("There is no set information for this concert.");
            }

            //Check through each set from concert

            //Case of only one set at concert
            if (totalSets == 1)
            {
                //Check through each song from set
                for (int songIndex = 0; songIndex < setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"].Count(); songIndex++)
                {
                    //Add song to list
                    songsPlayed.Add((setlistApi["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"][songIndex]["@name"]).ToString());
                }
            }
            //Case of multiple sets at concert
            else if (totalSets > 1)
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

            //Build and return concert object
            concert concert = new Models.concert(artistName, date, city, state, venue, tour, songsPlayed, id);

            return concert;
        }
    }
}