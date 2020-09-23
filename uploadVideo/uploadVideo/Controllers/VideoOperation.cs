using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using uploadVideo.Models;


namespace uploadVideo.Controllers
{
    
    public class VideoOperation : Controller
    {
        IConfiguration configuration;//Used for connection string
        IGridFSBucket gridFS;   //file storage
        IMongoCollection<Video> video; // collection in video database
        IMongoCollection<TalentShower> talentShower;//collection talentshower

        public VideoOperation(IConfiguration _configuration)
        {
            configuration = _configuration;
            //connection string
            string connectionString = configuration.GetSection("MongoDB").GetSection("ConnectionString").Value;
            //var connection = new MongoUrlBuilder(connectionString);
            // get a client to interact with the database
            var client = new MongoClient(connectionString);
            // we get access to the database itself
            IMongoDatabase database = client.GetDatabase("VideoDB");
            // get access to file storage
            gridFS = new GridFSBucket(database);
            video = database.GetCollection<Video>("Videos");
            talentShower = database.GetCollection<TalentShower>("Users");
        }

        public async Task RegisterInMongoDB(TalentShower ts)
        {
            await talentShower.InsertOneAsync(ts);
        }
        public async Task<IEnumerable<TalentShower>> GetRegisteredUsers()
        {
            // filter builder
            var builder = new FilterDefinitionBuilder<TalentShower>();
            var filter = builder.Empty; //filter for selection of all documents
                                        // filter by name
            return await talentShower.Find(filter).ToListAsync();
        }
        public async Task<IEnumerable<Video>> GetUploadedVideo(string userId)
        {
            // filter of all documents with the same Id
            var filter = Builders<Video>.Filter.Eq(x => x.videoUploaderId, userId);                                                                   // filter by name
            return await video.Find(filter).ToListAsync();
        }
        public async Task<TalentShower> GetUser(string id)
        {
            return await talentShower.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }

        public async Task<Video> GetVideoFileId(string id)
        {
            var filter = Builders<Video>.Filter.Eq(x=>x.videoUploaderId, id);
            return await video.Find(filter).FirstOrDefaultAsync();          
        }
        public async Task<IActionResult> DeleteOneVideoFile(string videoID)
        {
            await gridFS.DeleteAsync(new ObjectId(videoID));//deleting video
            var filter = Builders<Video>.Filter.Eq(x => x.videoFileId, videoID);
            var videoDetail= await video.Find(filter).FirstOrDefaultAsync();
            await video.DeleteOneAsync(filter);
            return RedirectToAction("Login", "Home",videoDetail.videoUploaderId);
        }
        
        public async Task<IActionResult> Delete(string userId)
        {          
            var Videos = await GetUploadedVideo(userId);
            var model = new IndexViewModel { Video = Videos };
            if (model.Video != null)
            {
                foreach (var vid in model.Video)
                {
                    //  var localVideo = await GetVideoFileId(id);//Finding video file ID            
                    await gridFS.DeleteAsync(new ObjectId(vid.videoFileId));//deleting video
                    var filterVideo = Builders<Video>.Filter.Eq(x => x.videoFileId, vid.videoFileId);
                    await video.DeleteOneAsync(filterVideo);
                }
            }
            var filter = Builders<TalentShower>.Filter.Eq(x => x.userId, userId);
            await talentShower.DeleteOneAsync(filter);           
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Upload(string id)
        {
            TalentShower ts = await GetUser(id);           
            var tuple = new Tuple<TalentShower, Video>(ts, new Video());
            return View(tuple);
        }
        
        [HttpPost]
        public async Task<IActionResult> Upload([Bind(Prefix = "Item2")] Video p, IFormFile uploadedFile)
        {         
            
            if (ModelState.IsValid)
            {        
                ObjectId fileId = new ObjectId();
                fileId = await gridFS.UploadFromStreamAsync(uploadedFile.FileName, uploadedFile.OpenReadStream());
                p.videoFileId = fileId.ToString();
                await video.InsertOneAsync(p);
                //for updating
                var filter = Builders<TalentShower>.Filter.Eq(x=>x.userId,p.videoUploaderId);
                var update = Builders<TalentShower>.Update.Set(x=>x.hasFile, true);
                await talentShower.UpdateOneAsync(filter, update);
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<ActionResult> GetFile(string videoFileId)
        {
            var file = await gridFS.DownloadAsBytesAsync(new ObjectId(videoFileId));
            if (file == null)
            {
                return NotFound();
            }
            return File(file, "video/mp4");
        }
    }
}
