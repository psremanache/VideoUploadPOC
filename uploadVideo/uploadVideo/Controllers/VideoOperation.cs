using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using uploadVideo.Models;


namespace uploadVideo.Controllers
{
    public class VideoOperation : Controller
    {
       
        IGridFSBucket gridFS;   //file storage
        IMongoCollection<Video> video; // collection in video database
        IMongoCollection<TalentShower> talentShower;//collection talentshower

        public VideoOperation()
        {
            //connection string
            string connectionString = "mongodb://localhost:27017/VideoDB";
            var connection = new MongoUrlBuilder(connectionString);
            // get a client to interact with the database
            MongoClient client = new MongoClient(connectionString);
            // we get access to the database itself
            IMongoDatabase database = client.GetDatabase(connection.DatabaseName);
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
        public async Task<TalentShower> GetUser(string id)
        {
            return await talentShower.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }

        public async Task<Video> GetVideoFileId(string id)
        {
            var filter = Builders<Video>.Filter.Eq("videoUploaderId", id);
            return await video.Find(filter).FirstOrDefaultAsync();          
        }
        public async Task<IActionResult> Delete(string id)
        {
            TalentShower ts = await GetUser(id);         
            if (ts.hasFile)
            {
                var localVideo = await GetVideoFileId(id);//Finding video file ID            
                await gridFS.DeleteAsync(new ObjectId(localVideo.videoFileId));//deleting video
                var filter = Builders<Video>.Filter.Eq("videoUploaderId", id);
                await video.DeleteOneAsync(filter);
            }          
            await talentShower.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Upload(string id)
        {
            TalentShower ts = await GetUser(id);           
            var tuple = new Tuple<TalentShower, Video>(ts, new Video());
            return View(tuple);
        }
        [HttpPost]
        public async Task<IActionResult> Upload([Bind(Prefix = "Item2")] Video p, [Bind(Prefix = "Item1")] TalentShower ts, IFormFile uploadedFile)
        {         
            
            if (ModelState.IsValid)
            {
                await video.InsertOneAsync(p);
                ObjectId fileId = new ObjectId();
                fileId = await gridFS.UploadFromStreamAsync(uploadedFile.FileName, uploadedFile.OpenReadStream());
                p.videoFileId = fileId.ToString();
                ts.hasFile = true;
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<ActionResult> GetFile(string id)
        {
            var file = await gridFS.DownloadAsBytesAsync(new ObjectId(id));
            if (file == null)
            {
                return NotFound();
            }
            return File(file, "video/mp4");
        }
    }
}
