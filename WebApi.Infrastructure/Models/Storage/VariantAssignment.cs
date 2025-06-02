namespace WebApi.Infrastructure.Models.Storage;

public class VariantAssignment : BaseEntity
{
    public int VariantId { get; set; }
    public Variant Variant { get; set; }
    
    public int TeacherId { get; set; }
    public User Teacher { get; set; }
    
    public int StudentId { get; set; }
    public User Student { get; set; }
}