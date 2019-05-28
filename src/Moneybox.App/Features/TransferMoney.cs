using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            //Withdrawn Availability Check

            bool IsFromAccountAvailableForWithdrawn = from.CheckWithdrawnAvailability(amount);

            //PayIn Availability Check

            bool IsFromAccountAvailableForPayIn = from.CheckPayInAvailability(amount);

            if (IsFromAccountAvailableForPayIn && IsFromAccountAvailableForWithdrawn)
            {
                from.WithdrawnMoney(amount, notificationService);
                to.PayInMoney(amount, notificationService);
                accountRepository.Update(from);
                accountRepository.Update(to);
            }


        }
    }
}
