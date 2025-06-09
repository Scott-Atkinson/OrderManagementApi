namespace Order_Management.Application.Common.Interfaces;
public interface IOrderNumberService
{
    Task<string> GenerateOrderNumberAsync();
}
