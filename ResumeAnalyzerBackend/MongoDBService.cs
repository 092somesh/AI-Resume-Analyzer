using MongoDB.Driver;
using ResumeAnalyzerBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ResumeAnalyzerBackend.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Resume> _resumeCollection;
        private readonly IMongoCollection<User> _userCollection;

        public MongoDBService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:DatabaseName"]);

            _resumeCollection = database.GetCollection<Resume>("Resumes");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task AddResume(Resume resume) => await _resumeCollection.InsertOneAsync(resume);
        public async Task<List<Resume>> GetSortedResumes() =>
            await _resumeCollection.Find(_ => true).SortByDescending(r => r.AtsScore).ToListAsync();

        public async Task AddUser(User user) => await _userCollection.InsertOneAsync(user);
        public async Task<User> GetUserByEmail(string email) =>
            await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }
}
