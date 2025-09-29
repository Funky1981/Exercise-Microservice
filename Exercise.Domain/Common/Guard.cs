using System;

namespace Exercise.Domain.Common
{
    public static class Guard
    {
        public static void AgainstEmptyGuid(Guid value, string paramName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        public static void AgainstNullOrWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        }

        public static void AgainstNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        public static void AgainstNegativeOrZero(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero.", paramName);
        }

        public static void AgainstNegativeOrZero(decimal value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero.", paramName);
        }


        public static void AgainstNegative(TimeSpan value, string paramName)
        {
            if (value.TotalSeconds < 0)
                throw new ArgumentException($"{paramName} cannot be negative.", paramName);
        }
        public static void AgainstInvalidDateRange(DateTime start, DateTime? end, string paramName)
        {
            if (end.HasValue && end <= start)
                throw new ArgumentException($"{paramName} end date must be after start date.", paramName);
        }
    }
}