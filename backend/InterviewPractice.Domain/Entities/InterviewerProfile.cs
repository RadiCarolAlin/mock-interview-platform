namespace InterviewPractice.Domain.Entities;

public class InterviewerProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Interview> Interviews { get; set; } = [];
}