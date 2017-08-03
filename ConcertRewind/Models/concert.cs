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
        public string country;
        public string venue;
        public string tour;
        public List<string> songsPlayed;
        public string id;

        //public concert(string a, DateTime? d, string c, string s, string v, string t, List<string> sp, string i)
        public concert(string a, string d, string ci, string s, string co, string v, string t, List<string> sp, string i)
        {
            artist = a;
            date = d;
            city = ci;
            state = s;
            country = co;
            venue = v;
            tour = t;
            songsPlayed = sp;
            id = i;
        }
    }
}