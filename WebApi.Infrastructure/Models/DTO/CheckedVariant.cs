namespace WebApi.Infrastructure.Models.DTO;

public class CheckedVariant
{
    public List<WrongExercise> WrongExercises { get; set; }
    public string Score { get; set; }
    public string SecondaryScore { get; set; }
}