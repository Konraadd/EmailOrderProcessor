using Domain.Entities;

namespace Domain.ServicesAbstraction;

public interface IEmailOrderParser
{
    Task<List<OrderData>> ParseEmailOrders(List<EmailMessage> email);
}

