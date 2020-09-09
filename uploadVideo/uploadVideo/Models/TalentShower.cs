using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uploadVideo.Models
{
    public class TalentShower
    {
        [BsonId]//gives us flexibility to name the index as we want
        [BsonRepresentation(BsonType.ObjectId)]//lets u juggle between mongo and internal .net model
        public string userId { get; set; }
        
        [Display(Name = "User Name")]
        public string Name { get; set; }

        [Display(Name = "Profession")]
        public string Profession { get; set; }
       
       
    }
}
