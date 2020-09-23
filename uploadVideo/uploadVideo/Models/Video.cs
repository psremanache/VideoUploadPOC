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
    [BsonIgnoreExtraElements]
    public class Video
    {       
        public string videoUploaderId { get; set; } //This id is uploader ID 
        [Display(Name = "Video Name")]
        public string videoName { get; set; }

        [Display(Name = "Type of Video")]
        public string ContentType { get; set; }

        public string videoFileId { get; set; }//this id we are saving when video is saved
       
    }
}
