using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Rest.Azure;
using OCR.Models.Domain;
using System.Reflection.Metadata;

namespace OCR.Services
{
    public class RecognizeTextService
    {
        private readonly ILogger<RecognizeTextService> _logger;
        private readonly TextAnalyticsClient textAnalyticsClient; 

        public RecognizeTextService(ILogger<RecognizeTextService> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _logger = logger;

            var endpoint = configuration["AzureLanguage:Endpoint"]!;
            var key = configuration["AzureLanguage:Key"]!;

            textAnalyticsClient = new TextAnalyticsClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));
        }

        public async Task<RecognizeText> RecognizeText(string text)
        {
            var result = new RecognizeText();

            var tasks = new List<Task>
            {
                ExtractDateAsync(text, result),
                ExtractNameAndMedicineAsync(text,  result)
            };

            await Task.WhenAll(tasks);
            return result;
        }



        //Використовуємо Microsoft Recognizers Text дати
        private async Task ExtractDateAsync(string text, RecognizeText result)
        {
            try
            {
                var dateResults = DateTimeRecognizer.RecognizeDateTime(text, Culture.English);

                if (dateResults.Any())
                {
                    var dateResult = dateResults.FirstOrDefault();
                    var resolution = dateResult?.Resolution;

                    if (resolution?.ContainsKey("values") == true)
                    {
                        var values = resolution["values"] as List<Dictionary<string, string>>;
                        var dateValue = values?.FirstOrDefault()?["value"];

                        if (!string.IsNullOrEmpty(dateValue) && DateTime.TryParse(dateValue, out var parsedDate))
                        {
                            result.DateDocument = DateOnly.FromDateTime(parsedDate);
                            _logger.LogInformation($"Successfully extracted date: {result.DateDocument}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No date found in the text");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Date extraction failed: {ex.Message}");
            }
        }


        //Використовуємо Azure Text Analytics для витягання імен
        private async Task ExtractNameAndMedicineAsync(string text, RecognizeText result)
        {
            try
            {
                try
                {
                    var response = await textAnalyticsClient.RecognizeEntitiesAsync(text);
                    var personEntities = response.Value.Where(e => e.Category == EntityCategory.Person).ToList();

                    if (personEntities.Any())
                    {
                        var fullName = personEntities.First().Text;
                        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        result.FirstName = nameParts.FirstOrDefault();
                        result.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : null;

                        _logger.LogInformation($"Successfully extracted name: {result.FirstName} {result.LastName}");
                    }
                    else
                    {
                        _logger.LogInformation("No person entities found in the text");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Name extraction failed: {ex.Message}");
                }

                try
                {
                    List<string> batchInput = new List<string>() { text };

                    AnalyzeHealthcareEntitiesOperation healthOperation =
                        await textAnalyticsClient.StartAnalyzeHealthcareEntitiesAsync(batchInput);

                    await healthOperation.WaitForCompletionAsync();

                    var medications = new List<string>();
                    var treatment = new List<string>();

                    await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthOperation.GetValuesAsync())
                    {
                        foreach (AnalyzeHealthcareEntitiesResult document in documentsInPage)
                        {
                            if (document.HasError)
                            {
                                _logger.LogWarning($"Error in healthcare entity recognition: {document.Error.Message}");
                                continue;
                            }

                            foreach (var entity in document.Entities)
                            {
                                if (entity.Category == HealthcareEntityCategory.MedicationName)
                                {
                                    medications.Add(entity.Text);
                                    _logger.LogInformation($"Successfully extracted medicine: {entity.Text}");
                                }
                            }

                            foreach (var entity in document.Entities)
                            {
                                if (entity.Category == HealthcareEntityCategory.TreatmentName)
                                {
                                    treatment.Add(entity.Text);
                                    _logger.LogInformation($"Successfully extracted treatment: {entity.Text}");
                                }
                            }
                        }

                        if (medications.Any())
                        {
                            result.Medicine = string.Join(", ", medications);
                        }
                        if (treatment.Any())
                        {
                            result.Treatment = string.Join(", ", treatment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Medicine extraction failed: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Combined extraction failed: {ex.Message}");
                throw;
            }
        }



    }



}
