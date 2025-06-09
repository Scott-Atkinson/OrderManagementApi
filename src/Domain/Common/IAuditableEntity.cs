namespace Order_Management.Domain.Common;
public interface IAuditableEntity
{
    public int Id { get; set; }
    DateTimeOffset CreatedDate { get; set; }
    DateTimeOffset ModifiedDate { get; set; }
}
