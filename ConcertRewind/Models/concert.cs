using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConcertRewind.Models
{
    public class concert
    {
        //Concert info
        public string artist;
        public string date;
        public string city;
        public string state; //can be state or country
        public string venue;
        public string tour;
        public List<string> songsPlayed;

        public concert(string a, string d, string c, string s, string v, string t, List<string> sp)
        {
            artist = a;
            date = d;
            city = c;
            state = s;
            venue = v;
            tour = t;
            songsPlayed = sp;
        }
    }
}