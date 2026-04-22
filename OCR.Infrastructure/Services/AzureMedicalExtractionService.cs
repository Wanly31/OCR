using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OCR.Application.Abstractions;
using OCR.Application.DTOs;
using System.Globalization;

namespace OCR.Infrastructure.Services
{
    public class AzureMedicalTextExtractionService : IMedicalExtractionService
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly ILogger<AzureMedicalTextExtractionService> _logger;

        public AzureMedicalTextExtractionService(IConfiguration configuration, ILogger<AzureMedicalTextExtractionService> logger)
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

        /// <summary>
        /// Extracts medical data (person name, date of birth, enities) from input text
        /// </summary>
        /// <param name="text">Raw OCR text from medical document</param>
        /// <returns>Structured medical data DTO</returns>
        /// <exception cref="ArgumentException">Thrown when input text is null or empty</exception>
        public async Task<RecognizedTextResultDto> ExtractMedicalDataAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Input text cannot be null or empty.", nameof(text));
            }

            var result = new RecognizedTextResultDto();

            await ExtractEntitiesAsync(text, result);
            await ExtractHealthcareEntitiesAsync(text, result);

            return result;
        }

        /// <summary>
        /// Extracts general entities such as person name and birth date from text.
        /// </summary>
        /// <param name="text">OCR extracted text</param>
        /// <param name="result">DTO where extracted data will be stored</param>
        private async Task ExtractEntitiesAsync(string text, RecognizedTextResultDto result)
        {
            try
            {
                var response = await _textAnalyticsClient.RecognizeEntitiesAsync(text);

                var personEntity = response.Value
                    .Where(e => e.Category == EntityCategory.Person && e.ConfidenceScore >= 0.70)
                    .OrderByDescending(e => e.ConfidenceScore)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(personEntity.Text))
                {

                    var (firstName, lastName) = SplitMedicalName(personEntity.Text);

                    result.FirstName = firstName;
                    result.LastName = lastName;

                    _logger.LogInformation("Extracted name: {FirstName} {LastName}", result.FirstName, result.LastName);
                }
                else
                {
                    _logger.LogInformation("No person entities found.");
                }

                var dobKeywords = new[] { "народження", "DOB", "born", "birth", "народився", "д.н." };
                var dateFormats = new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "d MMMM yyyy" };

                var dobEntity = response.Value
                    .Where(e => e.Category == EntityCategory.DateTime && !string.IsNullOrEmpty(e.Text))
                    .Where(e =>
                    {
                        int start = Math.Max(0, e.Offset - 40);
                        int length = Math.Min(60, text.Length - start);
                        var context = text.Substring(start, length);
                        return dobKeywords.Any(k => context.Contains(k, StringComparison.OrdinalIgnoreCase));
                    })
                    .OrderByDescending(e => e.ConfidenceScore)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(dobEntity.Text))
                {
                    _logger.LogInformation("No DateOfBirth entity found near DOB keywords.");
                    return;
                }

                if (DateTime.TryParseExact(dobEntity.Text, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    result.BirthDate = DateOnly.FromDateTime(parsedDate);
                    _logger.LogInformation("Extracted birth date (exact): {Date}", parsedDate);
                }
                else if (DateTime.TryParse(dobEntity.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallbackDate))
                {
                    result.BirthDate = DateOnly.FromDateTime(fallbackDate);
                    _logger.LogInformation("Extracted birth date (fallback): {Date}", fallbackDate);
                }
                else
                {
                    _logger.LogWarning("Found DateTime entity '{Text}' near DOB keyword but failed to parse.", dobEntity.Text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Entity extraction failed.");
            }
        }

        /// <summary>
        /// Extracts healthcare-related entities such as medications, treatments and examinations.
        /// Also filters contraindicated medicines.
        /// </summary>
        /// <param name="text">Raw OCR text</param>
        /// <param name="result">DTO with extracted medical data</param>
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

                var assertionContraindicatedMeds = new List<string>();
                var allExtractedMedications = new List<(string Text, int Offset)>();

                var medicationsWithoutAssertion = new List<(string Text, int Offset)>();

                await foreach (var docPage in healthOp.GetValuesAsync())
                {
                    foreach (var doc in docPage)
                    {
                        if (doc.HasError)
                        {
                            _logger.LogWarning("Healthcare entity recognition error: {Error}", doc.Error.Message);
                            continue;
                        }

                        // Step 1: Classify via Azure Assertion
                        foreach (var entity in doc.Entities)
                        {
                            if (entity.ConfidenceScore < 0.80)
                            {
                                _logger.LogDebug("Skipping low-confidence entity '{Text}' ({Score:P0})",
                                    entity.Text, entity.ConfidenceScore);
                                continue;
                            }

                            if (entity.Category == HealthcareEntityCategory.MedicationName)
                            {
                                allExtractedMedications.Add((entity.Text, entity.Offset));

                                if (IsContraindicatedByAssertion(entity))
                                {
                                    assertionContraindicatedMeds.Add(entity.Text);
                                    _logger.LogInformation(
                                        "[Assertion] Contraindicated: '{Text}' (Certainty={Certainty}, Association={Association})",
                                        entity.Text, entity.Assertion?.Certainty, entity.Assertion?.Association);
                                }
                                else
                                {
                                    medications.Add(entity.Text);
                                    _logger.LogInformation("Extracted medicine: '{Text}'", entity.Text);

                                    if (entity.Assertion == null)
                                        medicationsWithoutAssertion.Add((entity.Text, entity.Offset));
                                }
                            }
                            else if (entity.Category == HealthcareEntityCategory.TreatmentName)
                            {
                                treatments.Add(entity.Text);
                                _logger.LogInformation("Extracted treatment: '{Text}'", entity.Text);
                            }
                            else if (entity.Category == HealthcareEntityCategory.ExaminationName)
                            {
                                examination.Add(entity.Text);
                                _logger.LogInformation("Extracted examination: '{Text}'", entity.Text);
                            }
                        }

                        foreach (var relation in doc.EntityRelations)
                        {
                            var medicationRole = relation.Roles
                                .FirstOrDefault(r => r.Entity.Category == HealthcareEntityCategory.MedicationName);

                            if (medicationRole == null) continue;

                            var medText = medicationRole.Entity.Text;

                            bool relationIndicatesContraindication = relation.Roles
                                .Where(r => r.Entity.Category != HealthcareEntityCategory.MedicationName)
                                .Any(r =>
                                    r.Entity.Assertion?.Certainty == EntityCertainty.Negative ||
                                    r.Entity.Assertion?.Certainty == EntityCertainty.NegativePossible ||
                                    r.Entity.Assertion?.Association == EntityAssociation.Other);

                            if (relationIndicatesContraindication &&
                                !assertionContraindicatedMeds.Contains(medText, StringComparer.OrdinalIgnoreCase) &&
                                medications.Contains(medText, StringComparer.OrdinalIgnoreCase))
                            {
                                // RemoveAll щоб прибрати всі дублікати
                                medications.RemoveAll(m =>
                                    string.Equals(m, medText, StringComparison.OrdinalIgnoreCase));

                                assertionContraindicatedMeds.Add(medText);
                                _logger.LogInformation(
                                    "[Relation] Contraindicated via relation: '{Text}'", medText);
                            }
                        }
                    }
                }

                List<string> regexContraindicatedMeds = new();
                if (medicationsWithoutAssertion.Count > 0)
                {
                    regexContraindicatedMeds = ExtractContraindicatedMedicinesFallback(
                        text, medicationsWithoutAssertion, result);

                    _logger.LogInformation(
                        "[Regex Fallback] Found {Count} additional contraindicated medicines",
                        regexContraindicatedMeds.Count);
                }

                var allContraindicatedMeds = assertionContraindicatedMeds
                    .Union(regexContraindicatedMeds)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                medications = medications
                    .Where(m => !allContraindicatedMeds.Contains(m, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (medications.Any())
                    result.Medicine = string.Join(", ", medications);

                if (treatments.Any())
                    result.Treatment = string.Join(", ", treatments.Distinct(StringComparer.OrdinalIgnoreCase));

                if (examination.Any())
                    result.Examination = string.Join(", ", examination.Distinct(StringComparer.OrdinalIgnoreCase));

                if (allContraindicatedMeds.Any())
                {
                    result.ContraindicatedMedicine = string.Join(", ", allContraindicatedMeds);
                    _logger.LogInformation("Final contraindicated medicines: {Meds}", result.ContraindicatedMedicine);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Healthcare extraction failed.");
            }
        }

        /// <summary>
        /// Determines if a medication entity is contraindicated based on Azure's semantic assertion tags.
        /// </summary>
        private bool IsContraindicatedByAssertion(HealthcareEntity entity)
        {
            if (entity.Assertion == null)
                return false;

            bool negatedCertainty =
                entity.Assertion.Certainty == EntityCertainty.Negative ||
                entity.Assertion.Certainty == EntityCertainty.NegativePossible;

            bool notAssociated =
                entity.Assertion.Association == EntityAssociation.Other;

            return negatedCertainty || notAssociated;
        }



        /// <summary>
        /// Fallback: detects contraindicated medicines using regex section-matching.
        /// Called ONLY for medications that Azure assertions did NOT classify.
        /// Sets ContraindicatedReason on the result DTO.
        /// </summary>
        /// <param name="text">OCR text</param>
        /// <param name="unclassifiedMedications">Only medications not yet classified by Azure</param>
        /// <param name="result">DTO to store contraindication reason</param>
        /// <returns>List of contraindicated medicine names found via regex</returns>
        private List<string> ExtractContraindicatedMedicinesFallback(
            string text,
            List<(string Text, int Offset)> unclassifiedMedications,
            RecognizedTextResultDto result)
        {
            try
            {
                // Patterns that mark the START of a contraindication section in the document
                var sectionPatterns = new[]
                {
                    @"(?:протипоказан[оіаеи]*|не можна|не рекомендується|заборонен[оіаеи]*|не призначати)[\s:,-]*([^.\n]{1,200})",
                    @"(?:contraindicated|not recommended|forbidden|should not take|avoid)[\s:,-]*([^.\n]{1,200})",
                    @"(?:ПРОТИПОКАЗАННЯ|CONTRAINDICATIONS)[\s:]*([^\n]+(?:\n(?![А-ЯA-Z]{3,})[^\n]+)*)"
                };

                var contraindicatedMeds = new List<string>();
                var reasons = new List<string>();

                foreach (var pattern in sectionPatterns)
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(
                        text, pattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                        System.Text.RegularExpressions.RegexOptions.Multiline);

                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        if (match.Groups.Count <= 1) continue;

                        var sectionText = match.Groups[1].Value;
                        var sectionStart = match.Index;
                        var sectionEnd = sectionStart + sectionText.Length;

                        _logger.LogDebug($"[Regex Fallback] Contraindication section: '{sectionText[..Math.Min(60, sectionText.Length)]}...'");

                        // Match unclassified medications whose offset falls inside this section
                        foreach (var med in unclassifiedMedications)
                        {
                            if (med.Offset >= sectionStart && med.Offset <= sectionEnd &&
                                !contraindicatedMeds.Contains(med.Text))
                            {
                                contraindicatedMeds.Add(med.Text);
                                _logger.LogInformation($"[Regex Fallback] Contraindicated medicine: '{med.Text}'");
                            }
                        }

                        // Extract reason from inside the section
                        var reasonPatterns = new[]
                        {
                            @"(?:через|due to|because of|caused by|у зв'язку з|причина протипоказань)[\s:]+([\w\s]{3,50})",
                            @"\(([\w\s]{3,50})\)"
                        };

                        foreach (var rp in reasonPatterns)
                        {
                            foreach (System.Text.RegularExpressions.Match rm in
                                System.Text.RegularExpressions.Regex.Matches(sectionText, rp,
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                if (rm.Groups.Count > 1)
                                {
                                    var reason = rm.Groups[1].Value.Trim();
                                    if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                                        reasons.Add(reason);
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

                foreach (var rp in globalReasonPatterns)
                {
                    foreach (System.Text.RegularExpressions.Match rm in
                        System.Text.RegularExpressions.Regex.Matches(text, rp,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                            System.Text.RegularExpressions.RegexOptions.Multiline))
                    {
                        if (rm.Groups.Count > 1)
                        {
                            var reason = rm.Groups[1].Value.Trim();
                            if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                            {
                                reasons.Add(reason);
                                _logger.LogInformation($"[Regex Fallback] Contraindication reason: '{reason}'");
                            }
                        }
                    }
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
                _logger.LogError(ex, "[Regex Fallback] Failed to extract contraindicated medicines");
                return new List<string>();
            }


        }

        /// <summary>
        /// Розділяє повне ім'я на ім'я та прізвище, враховуючи специфіку медичних документів.
        /// </summary>
        private (string FirstName, string LastName) SplitMedicalName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (null, null);

            var cleanName = fullName.Trim().Replace(",", "").Replace(".", ". ");
            var parts = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return (null, null);
            if (parts.Length == 1) return (parts[0], null);

            string[] lastNameEndings = { "ов", "ва", "ко", "ий", "ін", "юк", "ак", "ич", "як", "ка" };

            bool isFirstWordLastName = lastNameEndings.Any(e =>
                parts[0].EndsWith(e, StringComparison.OrdinalIgnoreCase));

            if (isFirstWordLastName)
            {
                return (parts[1], parts[0]);
            }

            return (parts[0], string.Join(" ", parts.Skip(1)));
        }
    }
}
