using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly EmailDbContext _dbContext;

        public EmailRepository(EmailDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task SaveEmails(List<EmailMessage> emails)
        {
            foreach (var email in emails)
            {
                var exists = await _dbContext.EmailMessages
                    .AsNoTracking()
                    .AnyAsync(e => e.Id == email.Id);

                if (!exists)
                {
                    _dbContext.EmailMessages.Add(email);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
