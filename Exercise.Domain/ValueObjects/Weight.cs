using System;

namespace Exercise.Domain.ValueObjects
{
    public class Weight
    {
        // The value is always stored in kilograms for consistency.
        public decimal Kilograms { get; private set; }
        
        // Private constructor to enforce creation via factory methods.
        private Weight(decimal kilograms)
        {
            if (kilograms <= 0)
            {
                throw new ArgumentException("Weight must be a positive value.", nameof(kilograms));
            }
            Kilograms = kilograms;
        }
        
        // Factory method to create a Weight object from kilograms.
        public static Weight FromKilograms(decimal kilograms)
        {
            return new Weight(kilograms);
        }
        
        // Factory method to create a Weight object from pounds.
        public static Weight FromPounds(decimal pounds)
        {
            if (pounds < 0) throw new ArgumentException("Pounds cannot be negative.", nameof(pounds));
            var kilograms = pounds * 0.453592m; // 1 pound = 0.453592 kg
            return new Weight(kilograms);
        }

        // Factory method to create a Weight object from stones and pounds.
        public static Weight FromStonesAndPounds(int stones, decimal pounds)
        {
            if (stones < 0) throw new ArgumentException("Stones cannot be negative.", nameof(stones));
            if (pounds < 0) throw new ArgumentException("Pounds cannot be negative.", nameof(pounds));
            
            var totalPounds = (stones * 14) + pounds; // 1 stone = 14 pounds
            return FromPounds(totalPounds);
        }

        // Properties to get the value in other units.
        public decimal Pounds => Kilograms / 0.453592m;
        public decimal Stones => Pounds / 14m;

        public override string ToString()
        {
            return $"{Kilograms:F2} kg";
        }

        // Equality methods for value object
        public override bool Equals(object? obj)
        {
            return obj is Weight other && Kilograms == other.Kilograms;
        }

        public override int GetHashCode()
        {
            return Kilograms.GetHashCode();
        }
    }
}