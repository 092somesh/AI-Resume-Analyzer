using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzerBackend.Models;
using ResumeAnalyzerBackend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ResumeAnalyzerBackend.Controllers
{
    [ApiController]
    [Route("api/resume")]
    public class ResumeController : ControllerBase
    {
        private readonly MongoDBService _mongoService;
        private readonly ResumeService _resumeService;

        public ResumeController(MongoDBService mongoService, ResumeService resumeService)
        {
            _mongoService = mongoService;
            _resumeService = resumeService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadResume([FromForm] IFormFile file, [FromForm] string email, [FromForm] string jobTitle)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                return BadRequest(new { error = "Only PDF files are allowed." });

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileData = memoryStream.ToArray();

            string resumeText = ExtractTextFromPdf(fileData);
            if (string.IsNullOrWhiteSpace(resumeText))
                return BadRequest(new { error = "Failed to extract text from the PDF. Ensure it's a valid text-based PDF." });

            try
            {
                var analysisResult = await _resumeService.AnalyzeResumeAsync(fileData, jobTitle);

                if (analysisResult == default || string.IsNullOrWhiteSpace(analysisResult.tips))
                {
                    return StatusCode(500, new { error = "AI analysis failed. No result returned." });
                }

                var resume = new Resume
                {
                    Email = email,
                    FileName = file.FileName,
                    FileData = fileData,
                    UploadDate = DateTime.UtcNow,
                    AtsScore = analysisResult.atsScore
                };

                await _mongoService.AddResume(resume);

                return Ok(new { message = "Resume uploaded successfully!", resume.AtsScore, analysisResult.tips });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = "Failed to analyze resume.", details = ex.Message });
            }
        }

        [HttpPost("hr/analyze-resumes")]
        public async Task<IActionResult> AnalyzeResumes([FromForm] IFormFile jobDescription, [FromForm] List<IFormFile> resumes)
        {
            if (jobDescription == null || jobDescription.Length == 0)
                return BadRequest(new { error = "Job description PDF is required." });

            if (resumes == null || resumes.Count == 0)
                return BadRequest(new { error = "At least one resume PDF is required." });

            using var jdStream = new MemoryStream();
            await jobDescription.CopyToAsync(jdStream);
            byte[] jdData = jdStream.ToArray();
            string jobDescriptionText = ExtractTextFromPdf(jdData);

            if (string.IsNullOrWhiteSpace(jobDescriptionText))
                return BadRequest(new { error = "Failed to extract text from the job description PDF." });

            List<Resume> processedResumes = new List<Resume>();
            foreach (var file in resumes)
            {
                using var resumeStream = new MemoryStream();
                await file.CopyToAsync(resumeStream);
                byte[] resumeData = resumeStream.ToArray();
                string resumeText = ExtractTextFromPdf(resumeData);

                if (string.IsNullOrWhiteSpace(resumeText)) continue;

                var analysisResult = await _resumeService.AnalyzeResumeAsync(resumeData, jobDescriptionText);

                string extractedEmail = ExtractEmailFromText(resumeText) ?? "unknown@example.com";

                processedResumes.Add(new Resume
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = extractedEmail,
                    FileName = file.FileName,
                    AtsScore = analysisResult.atsScore
                });
            }

            processedResumes.Sort((a, b) => b.AtsScore.CompareTo(a.AtsScore));
            var topCandidates = processedResumes.Take(5)
                .Select(r => new { r.Id, r.Email, r.FileName })
                .ToList();

            return Ok(new { top_candidates = topCandidates });
        }

        // Utility to extract email from text
        private string? ExtractEmailFromText(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : null;
        }


        private string ExtractTextFromPdf(byte[] fileData)
        {
            try
            {
                using var pdfStream = new MemoryStream(fileData);
                using var pdfDocument = PdfDocument.Open(pdfStream);
                StringBuilder text = new StringBuilder();

                foreach (var page in pdfDocument.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return text.Length > 0 ? text.ToString().Trim() : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
