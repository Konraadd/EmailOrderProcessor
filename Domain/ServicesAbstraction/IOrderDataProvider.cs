using Domain.Entities;

namespace Domain.ServicesAbstraction
{
    public interface IOrderDataProvider
    {
        Task<List<OrderData>> GetOrderData();
    }
}
