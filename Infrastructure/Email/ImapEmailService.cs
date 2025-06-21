using Domain.ConfigurationOptions;
using Domain.Entities;
using Domain.ServicesAbstraction;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email
{
    public class ImapEmailService : IEmailService
    {
        private readonly EmailOptions _emailSettings;

        public ImapEmailService(IOptions<EmailOptions> options)
        {
            _emailSettings = options.Value;
        }

        public async Task<List<EmailMessage>> DownloadEmails()
        {
            string host = _emailSettings.Host;
            int port = _emailSettings.Port;
            bool useSsl = _emailSettings.UseSSL;
            string username = _emailSettings.Username;
            string password = _emailSettings.Password;

            using var client = new ImapClient();
            await client.ConnectAsync(host, port, useSsl);
            await client.AuthenticateAsync(username, password);

            var emails = new List<EmailMessage>();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            for (int i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);

                await using var emlStream = new MemoryStream();
                await message.WriteToAsync(emlStream);

                var email = new EmailMessage
                {
                    Id = Guid.NewGuid(),
                    Subject = message.Subject ?? "(no subject)",
                    From = message.From.ToString(),
                    EmlContent = emlStream.ToArray(),
                };


                emails.Add(email);
            }

            await client.DisconnectAsync(true);

            return emails;
        }
    }
}
