using System.Globalization;
using Lacuna.Space.Domain.Abstractions;
using Lacuna.Space.Domain.Enums;

namespace Lacuna.Space.Domain.Probes;

public class Probe
{
    public string Id { get; }
    public string Name { get; }
    public TimestampEncoding Encoding { get; }
    public long TimeOffset { get; private set; }
    public long RoundTrip { get; private set; }
    public bool IsSynchronized { get; private set; }
    
    public Probe(string id, string name, TimestampEncoding encoding)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Encoding = encoding;
        TimeOffset = 0;
        RoundTrip = 0;
        IsSynchronized = false;
    }

    public void UpdateSynchronization(long timeOffset, long roundTrip)
    {
        TimeOffset += timeOffset;
        RoundTrip = roundTrip;

        // Considera sincronizado se o último offset for menor que 5ms (50000 ticks)
        IsSynchronized = Math.Abs(timeOffset) < 50000; // 5ms em ticks
    }

    public long GetSynchronizedTime()
    {
        return DateTimeOffset.UtcNow.Ticks + TimeOffset;
    }

    public string GetEncodedTimestamp(long ticks)
    {
        return Encoding switch
        {
            TimestampEncoding.Iso8601 => new DateTimeOffset(ticks, TimeSpan.Zero).ToString(
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"),
            TimestampEncoding.Ticks => ticks.ToString(),
            TimestampEncoding.TicksBinary => Convert.ToBase64String(BitConverter.GetBytes(ticks)),
            TimestampEncoding.TicksBinaryBigEndian => Convert.ToBase64String(BitConverter.GetBytes(ticks).Reverse()
                .ToArray()),
            _ => throw new NotSupportedException($"Codificação {Encoding} não suportada")
        };
    }

    public static long DecodeTimestamp(string encodedTimestamp, TimestampEncoding encoding)
    {
        if (string.IsNullOrEmpty(encodedTimestamp))
            throw new ArgumentException("Timestamp codificado não pode ser nulo ou vazio", nameof(encodedTimestamp));

        return encoding switch
        {
            TimestampEncoding.Iso8601 => DateTimeOffset.Parse(encodedTimestamp, null, DateTimeStyles.RoundtripKind).Ticks,
            TimestampEncoding.Ticks => long.Parse(encodedTimestamp),
            TimestampEncoding.TicksBinary => BitConverter.ToInt64(Convert.FromBase64String(encodedTimestamp), 0),
            TimestampEncoding.TicksBinaryBigEndian => BitConverter.ToInt64(
                Convert.FromBase64String(encodedTimestamp).Reverse().ToArray(), 0),
            _ => throw new NotSupportedException($"Codificação {encoding} não suportada")
        };
    }
    // Equality members
    public override bool Equals(object? obj)
    {
        return obj is Probe other && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"Sonda({Id}, {Name})";
}