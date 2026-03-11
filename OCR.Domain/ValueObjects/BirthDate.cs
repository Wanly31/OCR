using OCR.Domain.Exceptions;

namespace OCR.Domain.ValueObjects
{
    public record BirthDate
    {
        public DateOnly Value { get; }

        public BirthDate(DateOnly value)
        {
            if (value > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new DomainException("BirthDate cannot be in the future");
            }

            Value = value;
        }
    }
}
