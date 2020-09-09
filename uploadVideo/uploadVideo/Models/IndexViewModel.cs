using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uploadVideo.Models
{
    public class IndexViewModel
    {
        public IEnumerable<Video> Video { get; set; }
        public IEnumerable<TalentShower> talentShower { get; set; }
    }
}
