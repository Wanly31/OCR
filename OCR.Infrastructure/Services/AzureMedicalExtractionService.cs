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

                var personEntity = response.Value.FirstOrDefault(e => e.Category == EntityCategory.Person);

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

                
                var dobEntity = response.Value.FirstOrDefault(e => e.Category == EntityCategory.DateTime);
                var dateFormats = new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "d MMMM yyyy" };
                
                if (DateTime.TryParseExact(dobEntity.Text, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    result.BirthDate = DateOnly.FromDateTime(parsedDate);

                    _logger.LogInformation($"Extracted date: {parsedDate}");
                }
                else if (DateTime.TryParse(dobEntity.Text, out var fallbackDate))
                {
                    result.BirthDate = DateOnly.FromDateTime(fallbackDate);

                    _logger.LogInformation($"Extracted date: {fallbackDate}");
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

                // Medications detected as contraindicated via Azure assertions (primary method)
                var assertionContraindicatedMeds = new List<string>();

                // All detected medications with offsets - used for regex fallback
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

                        // --- Step 1: Classify all entities via Azure Assertion (primary detection) ---
                        foreach (var entity in doc.Entities)
                        {
                            // Skip low-confidence results
                            if (entity.ConfidenceScore < 0.70)
                            {
                                _logger.LogDebug($"Skipping low-confidence entity '{entity.Text}' ({entity.ConfidenceScore:P0})");
                                continue;
                            }

                            if (entity.Category == HealthcareEntityCategory.MedicationName)
                            {
                                allExtractedMedications.Add((entity.Text, entity.Offset));

                                // Primary: use Azure's semantic negation/association tags
                                bool isContraindicated = IsContraindicatedByAssertion(entity);

                                if (isContraindicated)
                                {
                                    assertionContraindicatedMeds.Add(entity.Text);
                                    _logger.LogInformation($"[Assertion] Contraindicated medicine detected: '{entity.Text}' (Certainty={entity.Assertion?.Certainty}, Association={entity.Assertion?.Association})");
                                }
                                else
                                {
                                    medications.Add(entity.Text);
                                    _logger.LogInformation($"Extracted medicine: '{entity.Text}'");
                                }
                            }
                            else if (entity.Category == HealthcareEntityCategory.TreatmentName)
                            {
                                treatments.Add(entity.Text);
                                _logger.LogInformation($"Extracted treatment: '{entity.Text}'");
                            }
                            else if (entity.Category == HealthcareEntityCategory.ExaminationName)
                            {
                                examination.Add(entity.Text);
                                _logger.LogInformation($"Extracted examination: '{entity.Text}'");
                            }
                        }

                        // --- Step 2: Use EntityRelations to catch contraindications via relations ---
                        // e.g. a medication linked to a negated condition or a contraindication context
                        foreach (var relation in doc.EntityRelations)
                        {
                            // Look for medication entities in any relation that involves a negated role
                            var medicationRole = relation.Roles
                                .FirstOrDefault(r => r.Entity.Category == HealthcareEntityCategory.MedicationName);

                            if (medicationRole == null) continue;

                            var medText = medicationRole.Entity.Text;

                            // If any other role in this relation is negated or "Other", upgrade the medication
                            bool relationIndicatesContraindication = relation.Roles
                                .Where(r => r.Entity.Category != HealthcareEntityCategory.MedicationName)
                                .Any(r =>
                                    r.Entity.Assertion?.Certainty == EntityCertainty.Negative ||
                                    r.Entity.Assertion?.Certainty == EntityCertainty.NegativePossible ||
                                    r.Entity.Assertion?.Association == EntityAssociation.Other);

                            if (relationIndicatesContraindication &&
                                !assertionContraindicatedMeds.Contains(medText) &&
                                medications.Contains(medText))
                            {
                                medications.Remove(medText);
                                assertionContraindicatedMeds.Add(medText);
                                _logger.LogInformation($"[Relation] Contraindicated medicine detected via entity relation: '{medText}'");
                            }
                        }
                    }
                }

                // --- Step 3: Regex fallback — only for medications not yet classified ---
                var unclassifiedMedications = allExtractedMedications
                    .Where(m => !assertionContraindicatedMeds.Contains(m.Text) && !medications.Contains(m.Text))
                    .ToList();

                List<string> regexContraindicatedMeds = new();
                if (unclassifiedMedications.Count > 0)
                {
                    regexContraindicatedMeds = ExtractContraindicatedMedicinesFallback(text, unclassifiedMedications, result);
                    _logger.LogInformation($"[Regex Fallback] Found {regexContraindicatedMeds.Count} additional contraindicated medicines");
                }

                // Merge all contraindicated sources
                var allContraindicatedMeds = assertionContraindicatedMeds
                    .Union(regexContraindicatedMeds)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Ensure no contraindicated med appears in regular medications
                medications = medications
                    .Where(m => !allContraindicatedMeds.Contains(m, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (medications.Any())
                    result.Medicine = string.Join(", ", medications.Distinct(StringComparer.OrdinalIgnoreCase));

                if (treatments.Any())
                    result.Treatment = string.Join(", ", treatments.Distinct(StringComparer.OrdinalIgnoreCase));

                if (examination.Any())
                    result.Examination = string.Join(", ", examination.Distinct(StringComparer.OrdinalIgnoreCase));

                if (allContraindicatedMeds.Any())
                {
                    result.ContraindicatedMedicine = string.Join(", ", allContraindicatedMeds);
                    _logger.LogInformation($"Final contraindicated medicines: {result.ContraindicatedMedicine}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Healthcare extraction failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines if a medication entity is contraindicated based on Azure's semantic assertion tags.
        /// This is the primary (most reliable) detection method.
        /// </summary>
        private bool IsContraindicatedByAssertion(HealthcareEntity entity)
        {
            if (entity.Assertion == null)
                return false;

            // Negated certainty: "do NOT take Aspirin" → Negative or NegativePossible
            bool negatedCertainty =
                entity.Assertion.Certainty == EntityCertainty.Negative ||
                entity.Assertion.Certainty == EntityCertainty.NegativePossible;

            // "Other" association: medication mentioned but NOT associated with THIS patient
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

                // Global reason scan (e.g. "Причина: алергія на пеніцилін")
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

    }
}
