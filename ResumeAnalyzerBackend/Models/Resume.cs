using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ResumeAnalyzerBackend.Models
{
    public class Resume
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  // Nullable

        [BsonElement("email")]
        public string? Email { get; set; }  // Nullable

        [BsonElement("fileName")]
        public string? FileName { get; set; }  // Nullable

        [BsonElement("fileData")]
        public byte[]? FileData { get; set; }  // Nullable

        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [BsonElement("atsScore")]
        public double AtsScore { get; set; }  // No need to make this nullable
    }
}
