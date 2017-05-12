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
        public string Replace(string info)
        {
            string output = info.Replace(" ", "+");


            return output;
        }
        public ActionResult Results(string artistName)
        {
            //Generate artist object
            artist artist = new Models.artist(artistName);

            //Get list of concert objects
            List<concert> recentConcerts = GetConcerts(artist);

            foreach (concert concert in recentConcerts)
            {
                string concertInfo = concert.city + ", " + concert.state + ": " + concert.date;
                ViewBag.recentConcerts += "<li>" + concertInfo + " " + "<input type=\"button\" value=\"Go to Concert\" onclick=\"location.href = '<%: Url.Action(\"Concert\", \"Setlist\", new { parameter1 = \"concert\" }) %>\" />"</li>";

                /*
                 <input type="button" value="Go to Concert" onclick="location.href='<%: Url.Action("Concert", "Setlist", new { parameter1 = " + concert + " }) %>'" />
*/



                /*
                <Form action="Concert">
                    <input type="submit"/>
                </form>

                <script>
                    function
                </script>

    */
            }

            return View();
        }

        

        //Return Concert view
        public ActionResult Concert(concert c)
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

            return View();
        }

        //Get a list of recent concerts by an artist (up to 10)
        public static List<concert> GetConcerts(artist a)
        {
            //Get name and JObject for artist
            string artistName = a.name;
            JObject setlist = a.setlistApi;

            List<concert> recentConcerts = new List<concert>();

            int totalConcerts = setlist["setlists"]["setlist"].Count();

            if(totalConcerts < 10)
            {
                for(int i = 0; i < totalConcerts; i++)
                {
                    recentConcerts.Add(GetSetList(a, i));
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    recentConcerts.Add(GetSetList(a, i));
                }
            }

            return recentConcerts;
        }

        //Generate concert object
        public static concert GetSetList(artist a, int concertIndex)
        {
            //Get name and JObject for artist
            string artistName = a.name;
            JObject setlist = a.setlistApi;

            //Get date and location of previous concert
            string artist = setlist["setlists"]["setlist"][concertIndex]["artist"]["@name"].ToString();
            string date = setlist["setlists"]["setlist"][concertIndex]["@eventDate"].ToString();
            string city = setlist["setlists"]["setlist"][concertIndex]["venue"]["city"]["@name"].ToString();
            string state = setlist["setlists"]["setlist"][concertIndex]["venue"]["city"]["country"]["@name"].ToString();
            //string state = setlist["setlists"]["setlist"][concertIndex]["venue"]["city"]["@state"].ToString();
            string venue = setlist["setlists"]["setlist"][concertIndex]["venue"]["@name"].ToString();

            string tour;
            try
            {
                tour = setlist["setlists"]["setlist"][concertIndex]["@tour"].ToString();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("No tour info available for current song.");
                tour = "n/a";
            }
            
            
            List<string> songsPlayed = new List<string>();

            int totalSets = setlist["setlists"]["setlist"][concertIndex]["sets"]["set"].Count();

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
                for (int songIndex = 0; songIndex < setlist["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"].Count(); songIndex++)
                {
                    //Add song to list
                    songsPlayed.Add((setlist["setlists"]["setlist"][concertIndex]["sets"]["set"]["song"][songIndex]["@name"]).ToString());
                }
            }
            //Case of multiple sets at concert
            else
            {
                for (int setIndex = 0; setIndex < setlist["setlists"]["setlist"][concertIndex]["sets"]["set"].Count(); setIndex++)
                {
                    //Check through each song from set
                    for (int songIndex = 0; songIndex < setlist["setlists"]["setlist"][concertIndex]["sets"]["set"][setIndex]["song"].Count(); songIndex++)
                    {
                        //Add song to list
                        songsPlayed.Add((setlist["setlists"]["setlist"][concertIndex]["sets"]["set"][setIndex]["song"][songIndex]["@name"]).ToString());
                    }
                }
            }

            concert concert = new Models.concert(artist, date, city, state, venue, tour, songsPlayed);

            //Go to results view
            return concert;
        }
    }
}