using OCR.Domain.Enums;

namespace OCR.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string? FileDescription { get; set; }
        public string FileExtension { get; set; }
        public long FileSizeInBytes { get; set; }
        public string FilePath { get; set; }

        public Recognize? RecognizedText { get; set; }
        public RecordStatus Status { get; set; }
    }
}
