﻿using System;
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
using System.Net.Http;

namespace ConcertRewind.Controllers
{
    public class SetlistController : Controller
    {
        //Setlist.fm JSON JObject to be shared by controller
        public static JObject setlistApi;

        //Build SQL database object for previous searches
        public static ConcertDBEntities db = new ConcertDBEntities();

        
        //Update SQL database of artist searches
        public static void addtoDB(string artistname)
        {
            artistname = artistname.ToLower();
            
            //Check if artist has already been searched before, iterate times searched by one
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
                //Add new artist name to database
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
            setlistApi = GenerateSetlistApi(artistName);

            //Update SQL database of artist searches
            addtoDB(artistName);
            ViewBag.artist = artistName;
            
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

            using (var client = new HttpClient())
            {
                string apiKey = APIKeys.setlistApiKey;
                string url = "https://api.setlist.fm/rest/1.0/search/setlists?artistName=" + artistName;

                //Headers for requesting JSON format and providing api key
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;

                    //Converts json into JObject
                    JObject setlistApi = JObject.Parse(json);

                    return setlistApi;
                }
                else
                {
                    Console.WriteLine("There was a problem communicating with the Setlist.fm API, or no data for this artist exists.");
                    return null;
                }
            }




            /*

            HttpWebRequest request =

            //Load setlist json for chosen artist from setlist.fm API
            WebRequest.CreateHttp("https://api.setlist.fm/rest/1.0/search/setlists?artistName=" + artistName);

            //Tells the user what browsers we're using
            request.UserAgent = @"User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36";

            //Request JSON format and give API key to server
            request.Accept = "application/json";
            request.Headers.Add("x-api-key", apiKey);

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException e)
            {
                //404 Error. Server is down or artist does not exist.
                if (((HttpWebResponse)(e.Response)).StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("There was a problem communicating with the Setlist.fm API, or no data for this artist exists.");
                    return null;
                }

                throw;
            }

            //gets a stream of text
            StreamReader rd = new StreamReader(response.GetResponseStream());

            //reads to the end of file
            string ApiText = rd.ReadToEnd();

            //Converts that text into JSON
            JObject setlistApi = JObject.Parse(ApiText);

            return setlistApi;*/
        }

        //Search youtube for song, return video ID of top result
        public static string SearchYoutube(string artistName, string songName)
        {
            JObject youtubeApi;

            List<string> videoIds = new List<string>();

            string apiKey = APIKeys.youtubeApiKey;

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

            //Go to error view if no concert data available
            if (recentConcerts == null)
            {
                ViewBag.errorMessage = "There is no concert data available for artist \"" + artistName + "\".";
                return View("error");
            }

            ViewBag.artist = artistName;
            foreach(concert c in recentConcerts)
            {
                ViewBag.songsPlayed = "";

                if (c.id == concertId)
                {
                    ViewBag.artist = c.artist;
                    ViewBag.date = c.date;
                    ViewBag.location = c.city + ", " + c.state;
                    ViewBag.tour = c.tour;

                    //Generate list of Youtube video IDs for playlist     
                    ViewBag.videoIds = "";

                    //Only search for video IDs on YouTube if there is song data for current concert
                    if(c.songsPlayed.Count() > 0)
                    {
                        for (int i = 0; i < c.songsPlayed.Count; i++)
                        {
                            string currentId = SearchYoutube(artistName, c.songsPlayed[i]);
                            //Only add video IDs that were found. If null is returned from search, song will be skipped.
                            if(currentId != null)
                            {
                                ViewBag.videoIds += currentId;

                                //Don't add comma after last id
                                if (i < (c.songsPlayed.Count - 1))
                                {
                                    ViewBag.videoIds += ",";
                                }
                            } 
                        }

                        foreach (string song in c.songsPlayed)
                        {
                            string artist = Replace(ViewBag.artist);
                            string songTwo = Replace(song);
                            string location = Replace(ViewBag.location);

                            ViewBag.songsPlayed += "<li> <a href=https://www.youtube.com/results?search_query=" + songTwo + "+" + location + "+" + artist + " " + "target =_blank" + ">" + song + "</a> </li>";
                        }
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
                totalConcerts = setlistApi["setlist"].Count();
            }
            catch (Exception)
            {
                Console.WriteLine("There is no concert info available for this artist.");
                return null;
            }

            if (totalConcerts == 0)
            {
                return null;
            }
            else if(totalConcerts < 10)
            {
                for(int i = 0; i < totalConcerts; i++)
                {
                    concert c = GetSetList(artistName, i);
                    if(c != null)
                    {
                        recentConcerts.Add(c);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    concert c = GetSetList(artistName, i);
                    if (c != null)
                    {
                        recentConcerts.Add(c);
                    }
                }
            }

            //Check if list of concerts was populated. Return null if not.
            if (recentConcerts.Count == 0)
            {
                return null;
            }
            else
            {
                return recentConcerts;
            }
        }

        //Generate concert object
        public static concert GetSetList(string artistName, int concertIndex)
        {
            //Get info (date, location, etc.) of concert

            //Concert must have artist name data to continue!
            string artist;
            try
            {
                artist = setlistApi["setlist"][concertIndex]["artist"]["name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No artist info available for this concert.");
                return null;
            }

            string date = null;
            try
            {
                date = setlistApi["setlist"][concertIndex]["eventDate"].ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("No date info available for this concert.");
            }

            string city = null;
            try
            {
                city = setlistApi["setlist"][concertIndex]["venue"]["city"]["name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No city info available for this concert.");
            }

            //Find state. If no state info available, try to find country instead.
            string state = null;
            try
            {
                state = setlistApi["setlist"][concertIndex]["venue"]["city"]["state"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No state info available for this concert.");
            }

            string country = null;
            try
            {
                country = setlistApi["setlist"][concertIndex]["venue"]["city"]["country"]["name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No country info available for this concert.");
            }

            string venue = null;
            try
            {
                venue = setlistApi["setlist"][concertIndex]["venue"]["name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No venue info available for this concert.");
            }

            string tour = null;
            try
            {
                tour = setlistApi["setlist"][concertIndex]["tour"]["name"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No tour info available for this concert.");
            }

            //Concert must have an id to continue!!!
            string id;
            try
            {
                id = setlistApi["setlist"][concertIndex]["id"].ToString();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("No ID assigned to this concert.");
                return null;
            }


            List<string> songsPlayed = new List<string>();

            int totalSets = 0;
            try
            {
                totalSets = setlistApi["setlist"][concertIndex]["sets"]["set"].Count();
            }
            catch (Exception)
            {
                Console.WriteLine("There is no set information for this concert.");
            }

            //Check through each set from concert

            for (int setIndex = 0; setIndex < setlistApi["setlist"][concertIndex]["sets"]["set"].Count(); setIndex++)
            {
                //Check through each song from set
                for (int songIndex = 0; songIndex < setlistApi["setlist"][concertIndex]["sets"]["set"][setIndex]["song"].Count(); songIndex++)
                {
                    //Add song to list
                    songsPlayed.Add((setlistApi["setlist"][concertIndex]["sets"]["set"][setIndex]["song"][songIndex]["name"]).ToString());
                }
            }

            //Build and return concert object
            concert concert = new Models.concert(artist, date, city, state, country, venue, tour, songsPlayed, id);

            return concert;
        }
    }
}