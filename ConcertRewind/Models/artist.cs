using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace ConcertRewind.Models
{
    public class artist
    {
        public string name;
        public JObject setlistApi;

        public artist(string n)
        {
            name = n;
            setlistApi = GenerateSetlistApi(n);
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
    }
}