using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using OCR.Models.Domain;
using System.Globalization;

namespace OCR.Services
{
    public class RecognizeTextService
    {
        private readonly ILogger<RecognizeTextService> _logger;
        private readonly TextAnalyticsClient _textAnalyticsClient;

        public RecognizeTextService(ILogger<RecognizeTextService> logger, IConfiguration configuration)
        {
            _logger = logger;

            var endpoint = configuration["AzureLanguage:Endpoint"]!;
            var key = configuration["AzureLanguage:Key"]!;

            _textAnalyticsClient = new TextAnalyticsClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));
        }

        public async Task<RecognizeText> RecognizeText(string text)
        {
            var result = new RecognizeText();

            var entityTask = ExtractEntitiesAsync(text, result);
            var healthcareTask = ExtractHealthcareEntitiesAsync(text, result);

            await Task.WhenAll(entityTask, healthcareTask);

            return result;
        }

        private async Task ExtractEntitiesAsync(string text, RecognizeText result)
        {
            try
            {
                var response = await _textAnalyticsClient.RecognizePiiEntitiesAsync(text);

                var personEntity = response.Value.FirstOrDefault(e => e.Category == PiiEntityCategory.Person);
                if (!string.IsNullOrEmpty(personEntity.Text))
                {
                    var nameParts = personEntity.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    result.FirstName = nameParts.FirstOrDefault();
                    result.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : null;

                    _logger.LogInformation($"Extracted name: {result.FirstName} {result.LastName}");
                }
                else
                {
                    _logger.LogInformation("No person entities found.");
                }

                var dobEntity = response.Value.FirstOrDefault(e =>
                    e.Category == PiiEntityCategory.Date &&
                    e.SubCategory == "DateOfBirth");

                if (!string.IsNullOrEmpty(dobEntity.Text))
                {
                    if (DateTime.TryParseExact(dobEntity.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        result.BirthDate = DateOnly.FromDateTime(parsedDate);
                        _logger.LogInformation($"Extracted birth date: {result.BirthDate}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to parse birth date: {dobEntity.Text}");
                    }
                }
                else
                {
                    _logger.LogInformation("No DateOfBirth entities found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Entity extraction failed: {ex.Message}");
            }
        }

        private async Task ExtractHealthcareEntitiesAsync(string text, RecognizeText result)
        {
            try
            {
                var batchInput = new List<string> { text };
                var healthOp = await _textAnalyticsClient.StartAnalyzeHealthcareEntitiesAsync(batchInput);
                await healthOp.WaitForCompletionAsync();

                var medications = new List<string>();
                var treatments = new List<string>();
                var examination = new List<string>();

                await foreach (var docPage in healthOp.GetValuesAsync())
                {
                    foreach (var doc in docPage)
                    {
                        if (doc.HasError)
                        {
                            _logger.LogWarning($"Healthcare entity recognition error: {doc.Error.Message}");
                            continue;
                        }

                        foreach (var entity in doc.Entities)
                        {
                            if (entity.Category == HealthcareEntityCategory.MedicationName)
                            {
                                medications.Add(entity.Text);
                                _logger.LogInformation($"Extracted medicine: {entity.Text}");
                            }
                            else if (entity.Category == HealthcareEntityCategory.TreatmentName)
                            {
                                treatments.Add(entity.Text);
                                _logger.LogInformation($"Extracted treatment: {entity.Text}");
                            }
                            else if (entity.Category == HealthcareEntityCategory.ExaminationName)
                            {
                                examination.Add(entity.Text);
                                _logger.LogInformation($"Extracted examination: {entity.Text}");
                            }
                        }
                    }

                    if (medications.Any())
                        result.Medicine = string.Join(", ", medications);

                    if (treatments.Any())
                        result.Treatment = string.Join(", ", treatments);

                    if (examination.Any())
                        result.Examination = string.Join(", ", examination);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Healthcare extraction failed: {ex.Message}");
            }
        }


    }
}
