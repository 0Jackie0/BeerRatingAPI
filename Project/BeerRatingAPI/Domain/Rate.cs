using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerRatingAPI.Domain
{
    public class Rate
    {
        //the beer id for the rate
        public int beerId { get; set; }

        //the user that give this rate
        public String userName { get; set; }

        //rate value for the beer
        public int rating { get; set; }

        //rate comment
        public String comment { get; set; }
    }
}
