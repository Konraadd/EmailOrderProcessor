using Domain.Entities;

namespace Domain.ServicesAbstraction;
public interface IEmailService
{
    Task<List<EmailMessage>> DownloadEmails();

}