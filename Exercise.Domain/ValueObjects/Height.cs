using System;

namespace Exercise.Domain.ValueObjects
{
    public class Height
    {
        // The value is always stored in centimeters for consistency.
        public decimal Centimeters { get; private set; }

        // Private constructor to enforce creation via factory methods.
        private Height(decimal centimeters)
        {
            if (centimeters <= 0)
            {
                throw new ArgumentException("Height must be a positive value.", nameof(centimeters));
            }
            Centimeters = centimeters;
        }

        // Factory method to create a Height object from centimeters.
        public static Height FromCentimeters(decimal centimeters)
        {
            return new Height(centimeters);
        }

        // Factory method to create a Height object from feet and inches.
        public static Height FromFeetAndInches(int feet, decimal inches)
        {
            if (feet < 0) throw new ArgumentException("Feet cannot be negative.", nameof(feet));
            if (inches < 0) throw new ArgumentException("Inches cannot be negative.", nameof(inches));

            var totalInches = (feet * 12) + inches;
            var centimeters = totalInches * 2.54m; // 1 inch = 2.54 cm
            return new Height(centimeters);
        }

        // Factory method to create a Height object from meters.
        public static Height FromMeters(decimal meters)
        {
            if (meters <= 0) throw new ArgumentException("Meters must be positive.", nameof(meters));
            return new Height(meters * 100); // 1 meter = 100 cm
        }

        // Properties to get the value in other units.
        public decimal TotalInches => Centimeters / 2.54m;
        public decimal Meters => Centimeters / 100m;
        public int Feet => (int)(TotalInches / 12);
        public decimal RemainingInches => TotalInches % 12;

        public override string ToString()
        {
            return $"{Centimeters:F2} cm";
        }

        // Equality methods for value object
        public override bool Equals(object? obj)
        {
            return obj is Height other && Centimeters == other.Centimeters;
        }

        public override int GetHashCode()
        {
            return Centimeters.GetHashCode();
        }
    }
}