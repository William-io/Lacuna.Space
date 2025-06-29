using Lacuna.Space.Domain.Enums;

namespace Lacuna.Space.Domain.Jobs;

public class Job
{
    public string Id { get; }
    public string ProbeName { get; }
    public JobStatus Status { get; private set; }

    public Job(string id, string probeName)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ProbeName = probeName ?? throw new ArgumentNullException(nameof(probeName));
        Status = JobStatus.Pending;
    }

    public void MarkAsCompleted() => Status = JobStatus.Completed;
    public void MarkAsFailed() => Status = JobStatus.Failed;

    public bool IsCompleted => Status == JobStatus.Completed;
    public bool IsPending => Status == JobStatus.Pending;
    public bool IsFailed => Status == JobStatus.Failed;

    // Equality members
    public override bool Equals(object? obj)
    {
        return obj is Job other && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"Job({Id}, {ProbeName})";
}