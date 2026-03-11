using OCR.Domain.Exceptions;

namespace OCR.Domain.ValueObjects
{
    public record PatientFullName
    {
        public string FirstName { get; }
        public string? LastName { get; }

        public PatientFullName(string firstName, string? lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new DomainException("FirstName cannot be empty");
            }

            FirstName = firstName;
            LastName = lastName;
        }

    }
}
