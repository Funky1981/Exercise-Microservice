using System;
using System.Collections.Generic;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class Analytics
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string AnalyticsType { get; private set; }
        public DateTime PeriodStart { get; private set; }
        public DateTime PeriodEnd { get; private set; }
        public Dictionary<string, object> Data { get; private set; } = new();
        public DateTime GeneratedAt { get; private set; }

        private Analytics() { } // For EF Core

        public Analytics(Guid id, Guid userId, string analyticsType, DateTime periodStart, DateTime periodEnd)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(userId, nameof(userId));
            Guard.AgainstNullOrWhiteSpace(analyticsType, nameof(analyticsType));
            Guard.AgainstInvalidDateRange(periodStart, periodEnd, nameof(periodEnd));

            Id = id;
            UserId = userId;
            AnalyticsType = analyticsType;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            GeneratedAt = DateTime.UtcNow;
        }

        public void AddDataPoint(string key, object value)
        {
            Guard.AgainstNullOrWhiteSpace(key, nameof(key));
            Guard.AgainstNull(value, nameof(value));

            Data[key] = value;
        }

        public T? GetDataPoint<T>(string key)
        {
            if (Data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }
    }
}
