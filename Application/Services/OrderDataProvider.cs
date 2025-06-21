using Domain.Entities;
using Domain.Repositories;
using Domain.ServicesAbstraction;

namespace Application.Services
{
    public class OrderDataProvider : IOrderDataProvider
    {
        private readonly IEmailService _emailService;
        private readonly IEmailRepository _emailRepository;
        private readonly IEmailOrderParser _parser;

        public OrderDataProvider(
            IEmailService emailService,
            IEmailOrderParser parser,
            IEmailRepository emailRepository)
        {
            _emailService = emailService;
            _parser = parser;
            _emailRepository = emailRepository;
        }

        public async Task<List<OrderData>> GetOrderData()
        {
            var emails = await _emailService.DownloadEmails();
            var saveEmailTask = _emailRepository.SaveEmails(emails);
            var parseEmailTask = _parser.ParseEmailOrders(emails);

            await Task.WhenAll(parseEmailTask);

            return parseEmailTask.Result;
        }

    }
}
