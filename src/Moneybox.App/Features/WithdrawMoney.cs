using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);

            //witdhdrawn Check
            bool IsFromAccountAvailableForWithdrawn = from.CheckWithdrawnAvailability(amount);

            if (IsFromAccountAvailableForWithdrawn)
            {
                from.WithdrawnMoney(amount,notificationService);
                this.accountRepository.Update(from);
            }

        }
    }
}
