namespace WebApi.Infrastructure.Models.Storage;

public class VariantExercise : BaseEntity
{
    public int VariantId { get; set; }
    public Variant Variant { get; set; }
    
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
}