using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text.RegularExpressions;
            
namespace ResumeAnalyzerBackend.Services
{
    public class ResumeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ResumeService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["GoogleAI:ApiKey"] ?? throw new ArgumentNullException("API key is missing from configuration.");
        }

        public async Task<(double atsScore, string tips)> AnalyzeResumeAsync(byte[] fileData, string jobTitle)
        {
            string resumeText = ExtractTextFromPdf(fileData);
            if (string.IsNullOrWhiteSpace(resumeText))
                throw new Exception("Failed to extract text from the resume.");

            // var requestBody = new
            // {
            //     contents = new[]
            //     {
            //         new
            //         {
            //             parts = new[] { new { text = $"Analyze the resume for '{jobTitle}' and provide an ATS score + improvement tips.\n\n{resumeText}" } }

            //         }
            //     }
            // };
            var requestBody = new
{
    contents = new[]
    {
        new
        {
            parts = new[]
            {
                new
                {
                    text = $"Analyze the whole resume's tech stacks according to the job title '{jobTitle}' and calculate the ATS score based on the following criteria:\n\n" +
                           $"Evaluate the ATS score of the given resume for '{jobTitle}' based on the provided job description. Analyze keyword relevance and frequency, formatting clarity, and overall match with job requirements. " +
                           $"Consider factors like skills, experience, and education. Provide a score between 1-100 and offer improvement suggestions to optimize ATS compatibility.\n\n" +
                           $"Improvement Tips:\nIdentify mistakes such as missing keywords, poor formatting, unclear section headings, or irrelevant content. " +
                           $"Highlight areas where skills, experience, or education do not align with job requirements. Provide specific suggestions to optimize the resume for ATS compatibility and increase its score.\n\n" +
                           $"Resume Content:\n{resumeText}"
                }
            }
        }
    }
};


            string jsonBody = JsonSerializer.Serialize(requestBody);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            Console.WriteLine("Sending request to Google API...");
            Console.WriteLine("Request Body: " + jsonBody);

            HttpResponseMessage response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}",
                content
            );

            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Google AI API Response: " + result);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to call Google AI API: {response.StatusCode} - {result}");

            var responseObject = JsonSerializer.Deserialize<GoogleAIResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            string responseText = responseObject?.GetText() ?? "";

            if (string.IsNullOrWhiteSpace(responseText))
                throw new Exception("AI analysis failed. No result returned.");

            double atsScore = ExtractAtsScore(responseText);
            return (atsScore, responseText);
        }

        private string ExtractTextFromPdf(byte[] fileData)
        {
            try
            {
                using var memoryStream = new MemoryStream(fileData);
                using var pdf = PdfDocument.Open(memoryStream);
                StringBuilder text = new StringBuilder();

                foreach (Page page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting text from PDF: " + ex.Message);
                return string.Empty;
            }
        }

        private double ExtractAtsScore(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText)) return 0.0;

            Match match = Regex.Match(responseText, @"ATS Score:\s*(\d{1,3})");
            return match.Success && double.TryParse(match.Groups[1].Value, out double extractedScore) ? extractedScore : 0.0;
        }

        private class GoogleAIResponse
        {
            public Candidate[] Candidates { get; set; } = Array.Empty<Candidate>();

            public string GetText()
            {
                return Candidates.Length > 0 && Candidates[0].Content?.Parts?.Length > 0 
                    ? Candidates[0].Content.Parts[0].Text 
                    : string.Empty;
            }
        }

        private class Candidate
        {
            public Content Content { get; set; } = new Content();
        }

        private class Content
        {
            public Part[] Parts { get; set; } = Array.Empty<Part>();
        }

        private class Part
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}
