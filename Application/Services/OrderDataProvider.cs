using Domain.Entities;
using Domain.ServicesAbstraction;

namespace Application.Services
{
    public class OrderDataProvider : IOrderDataProvider
    {
        private readonly IEmailService _emailService;
        private readonly IEmailOrderParser _parser;

        public OrderDataProvider(
            IEmailService emailService,
            IEmailOrderParser parser)
        {
            _emailService = emailService;
            _parser = parser;
        }

        public async Task<List<OrderData>> GetOrderData()
        {
            var emails = await _emailService.DownloadEmails();
            //var saveEmailTask = _emailService.SaveEmails(emails);
            var parseEmailTask = _parser.ParseEmailOrders(emails);

            await Task.WhenAll(parseEmailTask);

            return parseEmailTask.Result;
        }

    }
}
