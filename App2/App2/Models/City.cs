using System;
using System.Collections.Generic;
using System.Text;

namespace App2.Models
{
    //public class City
    //{
    //    public string CityName { get; set; }
    //    public string CountryName { get; set; }
    //    public string FlagId { get; set; }
    //    public string SecondCityName { get; set; }
    //    public string SecondFlagId { get; set; }
    //}

    public class Chat
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string FlagId { get; set; }
        public string Status { get; set; }

        public Chat(Chat city)
        {
            this.CityName = city.CityName;
            this.CountryName = city.CountryName;
            this.FlagId = city.FlagId;
        }

        public Chat()
        {

        }
    }
}
