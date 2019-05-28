using System;
using Moneybox.App.Domain.Services;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }


        public bool CheckWithdrawnAvailability(decimal amount)
        {
            var fromBalance = Balance - amount;
            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

           
            return true;
        }

        public bool CheckPayInAvailability(decimal amount)
        {
            var paidIn = PaidIn + amount;
            if (paidIn > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

           
            return true;

        }

        public void WithdrawnMoney(decimal amount, INotificationService notificationService)
        {
            Balance -= amount;
            Withdrawn -= amount;

            if (Balance < 500m)
            {
                notificationService.NotifyFundsLow(User.Email);
            }

        }


        public void PayInMoney(decimal amount, INotificationService notificationService)
        {
            Balance += amount;
            PaidIn += amount;

            if (PayInLimit - PaidIn < 500m)
            {
                notificationService.NotifyApproachingPayInLimit(User.Email);
            }


        }


    }
}
