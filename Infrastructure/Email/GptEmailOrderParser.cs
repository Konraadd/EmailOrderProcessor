using Domain.ConfigurationOptions;
using Domain.Entities;
using Domain.ServicesAbstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Email
{
    public class GptEmailOrderParser : IEmailOrderParser
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GptEmailOrderParser> _logger;

        public GptEmailOrderParser(HttpClient httpClient, IOptions<OpenAIOptions> options, ILogger<GptEmailOrderParser> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            if (string.IsNullOrEmpty(options.Value.ApiKey))
                throw new InvalidOperationException("OpenAI API key is missing in configuration.");
            _apiKey = options.Value.ApiKey;
        }

        public async Task<List<OrderData>> ParseEmailOrders(List<EmailMessage> emails)
        {
            List<OrderData> allOrders = new List<OrderData>();

            foreach (var email in emails)
            {
                string emailBody = ExtractEmailBody(email);

                var prompt = BuildPrompt(emailBody);

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                new { role = "user", content = prompt }
            },
                    max_tokens = 500,
                    temperature = 0
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();

                var gptResponse = JsonDocument.Parse(responseJson);

                string? content = null;
                try
                {
                    content = gptResponse.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse GPT response JSON structure");
                    return new List<OrderData>();
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("GPT response content is empty");
                    return new List<OrderData>();
                }

                allOrders.AddRange(ExtractParsedData(content));
            }

            return allOrders;
        }

        private string BuildPrompt(string emailBody)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Extract the list of items from this email.");
            sb.AppendLine("For each item, provide the item name, amount ordered, and price.");
            sb.AppendLine("Output the result in JSON array format, use properties ItemName, AmountOrdered, Price");
            sb.AppendLine("From price remove the zł appendix and format it into decimal");
            sb.AppendLine($"Email body: {emailBody}");
            return sb.ToString();
        }

        private string ExtractEmailBody(EmailMessage email)
        {
            if (email.EmlContent == null || email.EmlContent.Length == 0)
                return string.Empty;

            try
            {
                using var stream = new MemoryStream(email.EmlContent);
                var message = MimeMessage.Load(stream);

                string text = message.TextBody ?? message.HtmlBody ?? string.Empty;

                if (!string.IsNullOrEmpty(text) && message.HtmlBody != null)
                {
                    text = HtmlToPlainText(text);
                }

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract email body");
                return string.Empty;
            }
        }

        private string HtmlToPlainText(string html)
        {
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", String.Empty);
        }

        private List<OrderData> ExtractParsedData(string gptContent)
        {
            if (gptContent.StartsWith("```"))
            {
                var startIndex = gptContent.IndexOf('\n') + 1;
                var endIndex = gptContent.LastIndexOf("```");

                if (startIndex > 0 && endIndex > startIndex)
                {
                    gptContent = gptContent[startIndex..endIndex].Trim();
                }
            }
            try
            {
                var items = JsonSerializer.Deserialize<List<OrderData>>(gptContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return items ?? new List<OrderData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize GPT content to OrderData list");
                return new List<OrderData>();
            }
        }
    }
}