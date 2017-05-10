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
        public string location;
        public List<string> songsPlayed;

        public concert(string a, string d, string l, List<string> s)
        {
            artist = a;
            date = d;
            location = l;
            songsPlayed = s;
        }
    }
}