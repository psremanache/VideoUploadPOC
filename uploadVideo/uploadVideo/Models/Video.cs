using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uploadVideo.Models
{
    public class Video
    {
        [BsonId]//gives us flexibility to name the index as we want
        [BsonRepresentation(BsonType.ObjectId)]//lets u juggle between mongo and internal .net model
        public string videoDetailsId { get; set; } //This id is generated when files details are stored

        [Display(Name = "Video Name")]
        public string videoName { get; set; }

        [Display(Name = "Type of Video")]
        public string ContentType { get; set; }

        public string videoFileId { get; set; }//this id we are saving when video is saved
       
    }
}
