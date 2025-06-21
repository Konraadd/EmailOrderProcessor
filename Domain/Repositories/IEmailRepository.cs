using Domain.Entities;

namespace Domain.Repositories
{
    public interface IEmailRepository
    {
        Task SaveEmails(List<EmailMessage> emails);

    }
}
