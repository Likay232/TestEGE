namespace WebApi.Infrastructure.Models.DTO;

public class ExerciseSolutions
{
    public int Id { get; set; }
    public int Year { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public int EgeNumber { get; set; }

    public List<StudentSolution> StudentSolutions { get; set; }
}

public class StudentSolution
{
    public int Id { get; set; }
    
    public string StudentFirstName { get; set; }
    public string StudentLastName { get; set; }

    public string Answer { get; set; }
    
    public string? StudentSolutionPath { get; set; }
}