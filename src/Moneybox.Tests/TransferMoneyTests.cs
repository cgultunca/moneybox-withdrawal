using System;
using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.Tests
{
    public class TransferMoneyTests
    {
        private readonly Guid fromAccountId = Guid.NewGuid();
        private readonly Guid toAccountId = Guid.NewGuid();
        private const string FromUserEmail = "from@moneybox.com";
        private const string ToUserEmail = "to@moneybox.com";
        private Account from;
        private Account to;
        private Mock<IAccountRepository> _accountRepository;
        private Mock<INotificationService> _notificationService;
        private TransferMoney _transferMoney;
        [SetUp]
        public void SetUp()
        {
            from = new Account()
            {
                Id = fromAccountId,
                Balance = 6000,
                User = new User
                {
                    Email = FromUserEmail
                }
            };

            to = new Account()
            {
                Id = toAccountId,
                Balance = 2000,
                User = new User
                {
                    Email = ToUserEmail
                }
            };

            _accountRepository = new Mock<IAccountRepository>();
            _accountRepository.Setup(a => a.GetAccountById(fromAccountId)).Returns(from);
            _accountRepository.Setup(a => a.GetAccountById(toAccountId)).Returns(to);

            _notificationService = new Mock<INotificationService>();
            _transferMoney = new TransferMoney(_accountRepository.Object, _notificationService.Object);
        }

        [Test]
        public void TransferThrowsError_InsufficientFundsToMakeTransfer()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _transferMoney.Execute(fromAccountId, toAccountId, 6500);
            });
        }

        [Test]
        public void TransferThrowsError_AccountPayInLimitReached()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _transferMoney.Execute(fromAccountId, toAccountId, 4500);
            });
        }

        [Test]
        public void TransferRequestSuccesfullyEnded()
        {
            _transferMoney.Execute(fromAccountId, toAccountId, 2500);
            Assert.AreEqual(from.Balance, 3500);
            Assert.AreEqual(from.Withdrawn, -2500);
            Assert.AreEqual(to.Balance, 4500);
            Assert.AreEqual(to.PaidIn, 2500);

            _notificationService.Verify(n => n.NotifyFundsLow(FromUserEmail), Times.Never, "Expected no notify funds low message");
            _notificationService.Verify(n => n.NotifyApproachingPayInLimit(ToUserEmail), Times.Never, "Expected no notfy account pay in limit reached message");
        }

        [Test]
        public void TransferRequestSuccesfully_ButNotifyFundsLowToFromAccount()
        {
            from.Balance = 2750;
            _transferMoney.Execute(fromAccountId, toAccountId, 2500);
            Assert.AreEqual(from.Balance, 250);
            Assert.AreEqual(from.Withdrawn, -2500);
            Assert.AreEqual(to.Balance, 4500);
            Assert.AreEqual(to.PaidIn, 2500);

            _notificationService.Verify(n => n.NotifyFundsLow(FromUserEmail), Times.Once, "Expected once notify funds low message");
            _notificationService.Verify(n => n.NotifyApproachingPayInLimit(ToUserEmail), Times.Never, "Expected no notfy account pay in limit reached message");
        }

        [Test]
        public void TransferRequestSuccesfully_ButNotifyApproachingPayInLimitToAccount()
        {
            _transferMoney.Execute(fromAccountId, toAccountId, 3750);
            Assert.AreEqual(from.Balance, 2250);
            Assert.AreEqual(from.Withdrawn, -3750);
            Assert.AreEqual(to.Balance, 5750);
            Assert.AreEqual(to.PaidIn, 3750);

            _notificationService.Verify(n => n.NotifyFundsLow(FromUserEmail), Times.Never, "Expected no notify funds low message");
            _notificationService.Verify(n => n.NotifyApproachingPayInLimit(ToUserEmail), Times.Once, "Expected once notfy account pay in limit reached message");
        }
        [Test]
        public void TransferRequestSuccesfully_ButNotifyFundsLowAndNotifyApproachingPayInLimit()
        {
            from.Balance = 4000;
            _transferMoney.Execute(fromAccountId, toAccountId, 3750);
            Assert.AreEqual(from.Balance, 250);
            Assert.AreEqual(from.Withdrawn, -3750);
            Assert.AreEqual(to.Balance, 5750);
            Assert.AreEqual(to.PaidIn, 3750);

            _notificationService.Verify(n => n.NotifyFundsLow(FromUserEmail), Times.Once, "Expected once notify funds low message");
            _notificationService.Verify(n => n.NotifyApproachingPayInLimit(ToUserEmail), Times.Once, "Expected no notfy account pay in limit reached message");
        }


    }
}
