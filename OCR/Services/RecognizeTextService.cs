using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;
using OCR.Models.Domain;
using OCR.Models.DTO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OCR.Services
{
    public class RecognizeTextService
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly ILogger<RecognizeTextService> _logger;

        public RecognizeTextService(IConfiguration configuration, ILogger<RecognizeTextService> logger)
        {
            _logger = logger;

            var endpoint = configuration["AzureLanguage:Endpoint"]
                ?? throw new InvalidOperationException("Azure Language endpoint not configured");
            var key = configuration["AzureLanguage:Key"]
                ?? throw new InvalidOperationException("Azure Language key not configured");

            _textAnalyticsClient = new TextAnalyticsClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));
        }

        public async Task<RecognizedTextResultDto> RecognizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Input text cannot be null or empty.", nameof(text));
            }

            var result = new RecognizedTextResultDto();

            var entityTask = ExtractEntitiesAsync(text, result);
            var healthcareTask = ExtractHealthcareEntitiesAsync(text, result);

            await Task.WhenAll(entityTask, healthcareTask);

            return result;
        }

        private async Task ExtractEntitiesAsync(string text, RecognizedTextResultDto result)
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
                (e.SubCategory?.Contains("Birth", StringComparison.OrdinalIgnoreCase) ?? false));

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


        private async Task ExtractHealthcareEntitiesAsync(string text, RecognizedTextResultDto result)
        {
            try
            {
                var batchInput = new List<string> { text };
                var healthOp = await _textAnalyticsClient.StartAnalyzeHealthcareEntitiesAsync(batchInput);
                await healthOp.WaitForCompletionAsync();

                var medications = new List<string>();
                var treatments = new List<string>();
                var examination = new List<string>();
                var allExtractedMedications = new List<(string Text, int Offset)>();

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

                                allExtractedMedications.Add((entity.Text, entity.Offset));
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
                }

                var contraindicatedMeds = ExtractContraindicatedMedicines(text, allExtractedMedications, result);

                if (contraindicatedMeds.Any())
                {
                    medications = medications.Where(med => !contraindicatedMeds.Contains(med)).ToList();
                    _logger.LogInformation($"Filtered out {contraindicatedMeds.Count} contraindicated medicines from regular medicines list");
                }

                if (medications.Any())
                    result.Medicine = string.Join(", ", medications);

                if (treatments.Any())
                    result.Treatment = string.Join(", ", treatments);

                if (examination.Any())
                    result.Examination = string.Join(", ", examination);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Healthcare extraction failed: {ex.Message}");
            }
        }


        private List<string> ExtractContraindicatedMedicines(string text, List<(string Text, int Offset)> allMedications, RecognizedTextResultDto result)
        {
            try
            {
                var contraindicationPatterns = new[]
                {
                    @"(?:протипоказан[оіаеи]*|не можна|не рекомендується|заборонен[оіаеи]*|не призначати)[\s:,-]*([^.\n]{1,200})",
                    @"(?:contraindicated|not recommended|forbidden|should not take|avoid)[\s:,-]*([^.\n]{1,200})",
                    @"(?:ПРОТИПОКАЗАННЯ|CONTRAINDICATIONS)[\s:]*([^\n]+(?:\n(?![А-ЯA-Z]{3,})[^\n]+)*)"
                };

                var contraindicatedMeds = new List<string>();
                var reasons = new List<string>();

                foreach (var pattern in contraindicationPatterns)
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(text, pattern, 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                        System.Text.RegularExpressions.RegexOptions.Multiline);

                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            var contraindicationSection = match.Groups[1].Value;
                            var sectionStart = match.Index;
                            var sectionEnd = sectionStart + contraindicationSection.Length;

                            _logger.LogInformation($"Found contraindication section: {contraindicationSection.Substring(0, Math.Min(50, contraindicationSection.Length))}...");

                            foreach (var med in allMedications)
                            {
                                if (med.Offset >= sectionStart && med.Offset <= sectionEnd)
                                {
                                    if (!contraindicatedMeds.Contains(med.Text))
                                    {
                                        contraindicatedMeds.Add(med.Text);
                                        _logger.LogInformation($"Identified contraindicated medicine: {med.Text}");
                                    }
                                }
                            }

                            var reasonPatterns = new[]
                            {
                                @"(?:через|due to|because of|caused by|у зв'язку з|причина протипоказань)[\s:]+([\w\s]{3,50})",
                                @"\(([\w\s]{3,50})\)"
                            };

                            foreach (var reasonPattern in reasonPatterns)
                            {
                                var reasonMatches = System.Text.RegularExpressions.Regex.Matches(contraindicationSection, reasonPattern,
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                foreach (System.Text.RegularExpressions.Match reasonMatch in reasonMatches)
                                {
                                    if (reasonMatch.Groups.Count > 1)
                                    {
                                        var reason = reasonMatch.Groups[1].Value.Trim();
                                        if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                                        {
                                            reasons.Add(reason);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var globalReasonPatterns = new[]
                {
                    @"(?:причина протипоказань|contraindication reason|причина|reason)[\s:]+([^\r\n]{3,100})",
                    @"(?:через|due to|because of|caused by|у зв'язку з)[\s:]+([^\r\n]{3,100})"
                };

                foreach (var reasonPattern in globalReasonPatterns)
                {
                    var reasonMatches = System.Text.RegularExpressions.Regex.Matches(text, reasonPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                        System.Text.RegularExpressions.RegexOptions.Multiline);

                    foreach (System.Text.RegularExpressions.Match reasonMatch in reasonMatches)
                    {
                        if (reasonMatch.Groups.Count > 1)
                        {
                            var reason = reasonMatch.Groups[1].Value.Trim();
                            if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                            {
                                reasons.Add(reason);
                                _logger.LogInformation($"Found contraindication reason: {reason}");
                            }
                        }
                    }
                }

                if (contraindicatedMeds.Any())
                {
                    result.ContraindicatedMedicine = string.Join(", ", contraindicatedMeds);
                    _logger.LogInformation($"Final contraindicated medicines: {result.ContraindicatedMedicine}");
                }

                if (reasons.Any())
                {
                    result.ContraindicatedReason = string.Join("; ", reasons);
                    _logger.LogInformation($"Contraindication reasons: {result.ContraindicatedReason}");
                }

                return contraindicatedMeds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract contraindicated medicines");
                return new List<string>();
            }
        }


    }
}
