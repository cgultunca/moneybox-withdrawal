using System;
using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.Tests
{
    public class WithdrawMoneyTests
    {
        private readonly Guid fromAccountId = Guid.NewGuid();
        private const string FromUserEmail = "from@moneybox.com";
        private Account from;
        private Mock<IAccountRepository> _accountRepository;
        private Mock<INotificationService> _notificationService;
        private WithdrawMoney _withdrawMoney;

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
            _accountRepository = new Mock<IAccountRepository>();
            _accountRepository.Setup(a => a.GetAccountById(fromAccountId)).Returns(from);

            _notificationService = new Mock<INotificationService>();
            _withdrawMoney = new WithdrawMoney(_accountRepository.Object, _notificationService.Object);
        }


        [Test]
        public void WithdrawMoneyExecuteThrowsError_InsufficientFundsToMakeTransfer()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _withdrawMoney.Execute(fromAccountId, 6500);
            });
        }

        [Test]
        public void TransferRequestSuccesfullyEnded()
        {
            _withdrawMoney.Execute(fromAccountId, 2500);
            Assert.AreEqual(from.Balance, 3500);
            Assert.AreEqual(from.Withdrawn, -2500);
        }

        [Test]
        public void TransferRequestSuccesfully_ButNotifyFundsLowToFromAccount()
        {
            from.Balance = 2750;
            _withdrawMoney.Execute(fromAccountId, 2500);
            Assert.AreEqual(from.Balance, 250);
            Assert.AreEqual(from.Withdrawn, -2500);

            _notificationService.Verify(n => n.NotifyFundsLow(FromUserEmail), Times.Once, "Expected once notify funds low message");
        }

    }
}
