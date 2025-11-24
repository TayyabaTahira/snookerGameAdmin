using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SnookerGameManagementSystem.Data
{
    /// <summary>
    /// Converts between Guid and String for database storage.
    /// This handles cases where MySQL stores char(36) as GUID instead of string.
    /// </summary>
    public class GuidToStringConverter : ValueConverter<string, Guid>
    {
        public GuidToStringConverter() 
            : base(
                v => Guid.Parse(v),           // Convert string to Guid for database
                v => v.ToString())            // Convert Guid to string for C#
        {
        }
    }
}
