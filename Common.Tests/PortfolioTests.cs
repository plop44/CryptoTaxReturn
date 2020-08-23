using System;
using System.Linq;
using Common.Conversions;
using Common.Models;
using Common.TradeAbstractions;
using Moq;
using NUnit.Framework;

namespace Common.Tests
{
    public class PortfolioTests
    {
        private Portfolio _objectUnderTests;
        private Mock<IFiatConversion> _fiatConversion;

        [SetUp]
        public void BeforeEveryTest()
        {
            _fiatConversion = new Mock<IFiatConversion>();
            _objectUnderTests = new Portfolio(_fiatConversion.Object, t => new FifoAsset(t));
        }

        [Test]
        public void WhenWeBuyBtc()
        {
            var unixDateTime = new DateTime(2017, 07, 01).ToUnixDateTime();

            var trade = new Mock<ITrade>();
            trade.Setup(t => t.IsBuyer).Returns(true);
            trade.Setup(t => t.Quantity).Returns(1m);
            trade.Setup(t => t.Base).Returns("BTC");
            trade.Setup(t => t.Quote).Returns("USDT");
            trade.Setup(t => t.CommissionAsset).Returns("BNB");
            trade.Setup(t => t.Commission).Returns(0.1m);
            trade.Setup(t => t.Price).Returns(10000);
            trade.Setup(t => t.Time).Returns(unixDateTime);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", unixDateTime))
                .Returns(10000);

            // ACT
            _objectUnderTests.Process(trade.Object);
            var snapshot = _objectUnderTests.GetAssets().ToDictionary(t => t.Name);

            // ASSERT
            Assert.AreEqual(3, snapshot.Count);
            Assert.AreEqual(-0.1m, snapshot["BNB"].Amount);
            Assert.AreEqual(-10000, snapshot["USDT"].Amount);
            Assert.AreEqual(1, snapshot["BTC"].Amount);
        }

        [Test]
        public void WhenWeSellBtc()
        {
            var trade = new Mock<ITrade>();
            trade.Setup(t => t.IsBuyer).Returns(false);
            trade.Setup(t => t.Quantity).Returns(1m);
            trade.Setup(t => t.Base).Returns("BTC");
            trade.Setup(t => t.Quote).Returns("USDT");
            trade.Setup(t => t.CommissionAsset).Returns("BNB");
            trade.Setup(t => t.Commission).Returns(0.1m);
            trade.Setup(t => t.Price).Returns(10000);
            trade.Setup(t => t.Time).Returns(new DateTime(2017, 07, 01).ToUnixDateTime());

            // ACT
            _objectUnderTests.Process(trade.Object);
            var snapshot = _objectUnderTests.GetAssets().ToDictionary(t => t.Name);

            // ASSERT
            Assert.AreEqual(3, snapshot.Count);
            Assert.AreEqual(-0.1m, snapshot["BNB"].Amount);
            Assert.AreEqual(10000, snapshot["USDT"].Amount);
            Assert.AreEqual(-1, snapshot["BTC"].Amount);
        }

        // https://koinly.io/guides/crypto-tax-australia/
        //Craig purchases 0.1 Bitcoin in July 2017 for AU$1000 and sells it in November 2017 for AU$2000.
        //His total capital gain is thus AU$1000.
        [Test]
        public void WhenWeBuyThenSellBtc()
        {
            var date1 = new DateTime(2017, 07, 01).ToUnixDateTime();
            var trade1 = new Mock<ITrade>();
            trade1.Setup(t => t.IsBuyer).Returns(true);
            trade1.Setup(t => t.Quantity).Returns(0.1m);
            trade1.Setup(t => t.Base).Returns("BTC");
            trade1.Setup(t => t.Quote).Returns("USDT");
            trade1.Setup(t => t.Price).Returns(10000);
            trade1.Setup(t => t.CommissionAsset).Returns("BTC");
            trade1.Setup(t => t.Commission).Returns(0m);
            trade1.Setup(t => t.Time).Returns(date1);

            var date2 = new DateTime(2017, 11, 01).ToUnixDateTime();
            var trade2 = new Mock<ITrade>();
            trade2.Setup(t => t.IsBuyer).Returns(false);
            trade2.Setup(t => t.Quantity).Returns(0.1m);
            trade2.Setup(t => t.Base).Returns("BTC");
            trade2.Setup(t => t.Quote).Returns("USDT");
            trade2.Setup(t => t.Price).Returns(20000);
            trade2.Setup(t => t.CommissionAsset).Returns("BNB");
            trade2.Setup(t => t.Commission).Returns(0m);
            trade2.Setup(t => t.Time).Returns(date2);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date1))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date2))
                .Returns(1m);

            // ACT
            _objectUnderTests.Process(trade1.Object);
            var result = _objectUnderTests.Process(trade2.Object).Single();

            Assert.AreEqual("BTC", result.Asset);
            Assert.AreEqual(0.1, result.Quantity);
            Assert.AreEqual(1000, result.Cost);
            Assert.AreEqual(2000, result.Proceed);
            Assert.AreEqual(1000, result.Gain);
            Assert.AreEqual(new DateTime(2017, 07, 01), result.BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result.SoldTimeReadable);
        }

        [Test]
        public void WhenWeBuyThenSellBtcWithFees()
        {
            var buyBnbTrade = new Mock<ITrade>();
            buyBnbTrade.Setup(t => t.IsBuyer).Returns(true);
            buyBnbTrade.Setup(t => t.Quantity).Returns(2m);
            buyBnbTrade.Setup(t => t.Base).Returns("BNB");
            buyBnbTrade.Setup(t => t.Quote).Returns("USDT");
            buyBnbTrade.Setup(t => t.CommissionAsset).Returns("BNB");
            buyBnbTrade.Setup(t => t.Commission).Returns(0m);
            buyBnbTrade.Setup(t => t.Price).Returns(15);
            var date1 = new DateTime(2017, 06, 01).ToUnixDateTime();
            buyBnbTrade.Setup(t => t.Time).Returns(date1);

            var trade1 = new Mock<ITrade>();
            trade1.Setup(t => t.IsBuyer).Returns(true);
            trade1.Setup(t => t.Quantity).Returns(0.1m);
            trade1.Setup(t => t.Base).Returns("BTC");
            trade1.Setup(t => t.Quote).Returns("USDT");
            trade1.Setup(t => t.CommissionAsset).Returns("BNB");
            trade1.Setup(t => t.Commission).Returns(1m);
            trade1.Setup(t => t.Price).Returns(10000);
            var date2 = new DateTime(2017, 07, 01).ToUnixDateTime();
            trade1.Setup(t => t.Time).Returns(date2);

            var trade2 = new Mock<ITrade>();
            trade2.Setup(t => t.IsBuyer).Returns(false);
            trade2.Setup(t => t.Quantity).Returns(0.1m);
            trade2.Setup(t => t.Base).Returns("BTC");
            trade2.Setup(t => t.Quote).Returns("USDT");
            trade2.Setup(t => t.CommissionAsset).Returns("BNB");
            trade2.Setup(t => t.Commission).Returns(1m);
            trade2.Setup(t => t.Price).Returns(20000);
            var date3 = new DateTime(2017, 11, 01).ToUnixDateTime();
            trade2.Setup(t => t.Time).Returns(date3);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date1))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date2))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date3))
                .Returns(1m);

            // ACT
            _objectUnderTests.Process(buyBnbTrade.Object);
            _objectUnderTests.Process(trade1.Object);
            var result = _objectUnderTests.Process(trade2.Object);

            Assert.AreEqual(2,result.Length);

            Assert.AreEqual("BTC", result[0].Asset);
            Assert.AreEqual(0.1m, result[0].Quantity);
            Assert.AreEqual(1000m, result[0].Cost);
            Assert.AreEqual(2000m, result[0].Proceed);
            Assert.AreEqual(1000m, result[0].Gain);
            Assert.AreEqual(new DateTime(2017, 07, 01), result[0].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[0].SoldTimeReadable);

            Assert.AreEqual("BNB", result[1].Asset);
            Assert.AreEqual(1m, result[1].Quantity);
            Assert.AreEqual(15, result[1].Cost);
            Assert.AreEqual(0, result[1].Proceed);
            Assert.AreEqual(-15, result[1].Gain);
            Assert.AreEqual("Trade Fee", result[1].BoughtTag);
            Assert.AreEqual(new DateTime(2017, 06, 01), result[1].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[1].SoldTimeReadable);
        }

        [Test]
        public void WhenWeBuyInTwiceThenSellBtc()
        {
            var buyTrade1 = new Mock<ITrade>();
            buyTrade1.Setup(t => t.IsBuyer).Returns(true);
            buyTrade1.Setup(t => t.Quantity).Returns(0.06m);
            buyTrade1.Setup(t => t.Base).Returns("BTC");
            buyTrade1.Setup(t => t.Quote).Returns("USDT");
            buyTrade1.Setup(t => t.CommissionAsset).Returns("BTC");
            buyTrade1.Setup(t => t.Commission).Returns(0m);
            buyTrade1.Setup(t => t.Price).Returns(10000);
            var date1 = new DateTime(2017, 07, 01).ToUnixDateTime();
            buyTrade1.Setup(t => t.Time).Returns(date1);

            var buyTrade2 = new Mock<ITrade>();
            buyTrade2.Setup(t => t.IsBuyer).Returns(true);
            buyTrade2.Setup(t => t.Quantity).Returns(0.04m);
            buyTrade2.Setup(t => t.Base).Returns("BTC");
            buyTrade2.Setup(t => t.Quote).Returns("USDT");
            buyTrade2.Setup(t => t.CommissionAsset).Returns("BTC");
            buyTrade2.Setup(t => t.Commission).Returns(0m);
            buyTrade2.Setup(t => t.Price).Returns(15000);
            var date2 = new DateTime(2017, 08, 01).ToUnixDateTime();
            buyTrade2.Setup(t => t.Time).Returns(date2);

            var sellTrade = new Mock<ITrade>();
            sellTrade.Setup(t => t.IsBuyer).Returns(false);
            sellTrade.Setup(t => t.Quantity).Returns(0.1m);
            sellTrade.Setup(t => t.Base).Returns("BTC");
            sellTrade.Setup(t => t.Quote).Returns("USDT");
            sellTrade.Setup(t => t.CommissionAsset).Returns("BTC");
            sellTrade.Setup(t => t.Commission).Returns(0m);
            sellTrade.Setup(t => t.Price).Returns(20000);
            var date3 = new DateTime(2017, 11, 01).ToUnixDateTime();
            sellTrade.Setup(t => t.Time).Returns(date3);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date1))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date2))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date3))
                .Returns(1m);

            // ACT
            _objectUnderTests.Process(buyTrade1.Object);
            _objectUnderTests.Process(buyTrade2.Object);
            var result = _objectUnderTests.Process(sellTrade.Object);

            Assert.AreEqual(2, result.Length);

            Assert.AreEqual("BTC", result[0].Asset);
            Assert.AreEqual(0.06m, result[0].Quantity);
            Assert.AreEqual(0.06m * 10000, result[0].Cost);
            Assert.AreEqual(0.1m * 20000m * 0.06m / 0.1m, result[0].Proceed);
            Assert.AreEqual(600m, result[0].Gain);
            Assert.AreEqual(new DateTime(2017, 07, 01), result[0].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[0].SoldTimeReadable);

            Assert.AreEqual("BTC", result[1].Asset);
            Assert.AreEqual(0.04m, result[1].Quantity);
            Assert.AreEqual(0.04m * 15000, result[1].Cost);
            Assert.AreEqual(0.1m * 20000m * 0.04m / 0.1m, result[1].Proceed);
            Assert.AreEqual(200m, result[1].Gain);
            Assert.AreEqual(new DateTime(2017, 08, 01), result[1].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[1].SoldTimeReadable);
        }

        [Test]
        public void WhenWeBuyInTwiceThenSellBtcWithFees()
        {
            var buyBnbTrade = new Mock<ITrade>();
            buyBnbTrade.Setup(t => t.IsBuyer).Returns(true);
            buyBnbTrade.Setup(t => t.Quantity).Returns(2m);
            buyBnbTrade.Setup(t => t.Base).Returns("BNB");
            buyBnbTrade.Setup(t => t.Quote).Returns("USDT");
            buyBnbTrade.Setup(t => t.CommissionAsset).Returns("BNB");
            buyBnbTrade.Setup(t => t.Commission).Returns(0m);
            buyBnbTrade.Setup(t => t.Price).Returns(15);
            var date1 = new DateTime(2017, 06, 01).ToUnixDateTime();
            buyBnbTrade.Setup(t => t.Time).Returns(date1);

            var buyTrade1 = new Mock<ITrade>();
            buyTrade1.Setup(t => t.IsBuyer).Returns(true);
            buyTrade1.Setup(t => t.Quantity).Returns(0.06m);
            buyTrade1.Setup(t => t.Base).Returns("BTC");
            buyTrade1.Setup(t => t.Quote).Returns("USDT");
            buyTrade1.Setup(t => t.CommissionAsset).Returns("BTC");
            buyTrade1.Setup(t => t.Commission).Returns(0m);
            buyTrade1.Setup(t => t.Price).Returns(10000);
            var date2 = new DateTime(2017, 07, 01).ToUnixDateTime();
            buyTrade1.Setup(t => t.Time).Returns(date2);

            var buyTrade2 = new Mock<ITrade>();
            buyTrade2.Setup(t => t.IsBuyer).Returns(true);
            buyTrade2.Setup(t => t.Quantity).Returns(0.04m);
            buyTrade2.Setup(t => t.Base).Returns("BTC");
            buyTrade2.Setup(t => t.Quote).Returns("USDT");
            buyTrade2.Setup(t => t.CommissionAsset).Returns("BTC");
            buyTrade2.Setup(t => t.Commission).Returns(0m);
            buyTrade2.Setup(t => t.Price).Returns(15000);
            var date3 = new DateTime(2017, 08, 01).ToUnixDateTime();
            buyTrade2.Setup(t => t.Time).Returns(date3);

            var sellTrade = new Mock<ITrade>();
            sellTrade.Setup(t => t.IsBuyer).Returns(false);
            sellTrade.Setup(t => t.Quantity).Returns(0.1m);
            sellTrade.Setup(t => t.Base).Returns("BTC");
            sellTrade.Setup(t => t.Quote).Returns("USDT");
            sellTrade.Setup(t => t.CommissionAsset).Returns("BNB");
            sellTrade.Setup(t => t.Commission).Returns(1m);
            sellTrade.Setup(t => t.Price).Returns(20000);
            var date4 = new DateTime(2017, 11, 01).ToUnixDateTime();
            sellTrade.Setup(t => t.Time).Returns(date4);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date1))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date2))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date3))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date4))
                .Returns(1m);

            // ACT
            _objectUnderTests.Process(buyBnbTrade.Object);
            _objectUnderTests.Process(buyTrade1.Object);
            _objectUnderTests.Process(buyTrade2.Object);
            var result = _objectUnderTests.Process(sellTrade.Object);

            Assert.AreEqual(3, result.Length);

            Assert.AreEqual("BTC", result[0].Asset);
            Assert.AreEqual(0.06m, result[0].Quantity);
            Assert.AreEqual(0.06m * 10000, result[0].Cost);
            Assert.AreEqual(0.1m * 20000m * 0.06m / 0.1m, result[0].Proceed);
            Assert.AreEqual(600m, result[0].Gain);
            Assert.AreEqual(new DateTime(2017, 07, 01), result[0].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[0].SoldTimeReadable);

            Assert.AreEqual("BTC", result[1].Asset);
            Assert.AreEqual(0.04m, result[1].Quantity);
            Assert.AreEqual(0.04m * 15000, result[1].Cost);
            Assert.AreEqual(0.1m * 20000m * 0.04m / 0.1m, result[1].Proceed);
            Assert.AreEqual(200m, result[1].Gain);
            Assert.AreEqual(new DateTime(2017, 08, 01), result[1].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[1].SoldTimeReadable);

            Assert.AreEqual("BNB", result[2].Asset);
            Assert.AreEqual(1m, result[2].Quantity);
            Assert.AreEqual(15m, result[2].Cost);
            Assert.AreEqual(0m, result[2].Proceed);
            Assert.AreEqual(-15m, result[2].Gain);
            Assert.AreEqual(new DateTime(2017, 06, 01), result[2].BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result[2].SoldTimeReadable);
        }

        // https://koinly.io/guides/crypto-tax-australia/
        //Let's say you purchased 1 Bitcoin for AU$1000 in July 2017.
        //In November 2017, you exchanged 0.5 Bitcoin for 3 Ether.
        //    At this time, the market value of 3 ether was around AU$2000.
        //This means your capital proceeds come to AU$2000 and the cost of acquisition is AU$500.
        //In other words, your capital gains would be $1500.
        [Test]
        public void WhenWeBuyThenSellBtcForEth()
        {
            var trade1 = new Mock<ITrade>();
            trade1.Setup(t => t.IsBuyer).Returns(true);
            trade1.Setup(t => t.Quantity).Returns(1m);
            trade1.Setup(t => t.Base).Returns("BTC");
            trade1.Setup(t => t.Quote).Returns("USDT");
            trade1.Setup(t => t.Price).Returns(1000);
            trade1.Setup(t => t.CommissionAsset).Returns("BTC");
            trade1.Setup(t => t.Commission).Returns(0m);
            var date1 = new DateTime(2017, 07, 01).ToUnixDateTime();
            trade1.Setup(t => t.Time).Returns(date1);

            var trade2 = new Mock<ITrade>();
            trade2.Setup(t => t.IsBuyer).Returns(true);
            trade2.Setup(t => t.Quantity).Returns(3m);
            trade2.Setup(t => t.Base).Returns("ETH");
            trade2.Setup(t => t.Quote).Returns("BTC");
            trade2.Setup(t => t.Price).Returns(0.5m/3m);
            trade2.Setup(t => t.CommissionAsset).Returns("BNB");
            trade2.Setup(t => t.Commission).Returns(0m);
            var date2 = new DateTime(2017, 11, 01).ToUnixDateTime();
            trade2.Setup(t => t.Time).Returns(date2);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date1))
                .Returns(1m);

            _fiatConversion.Setup(t => t.GetExactPriceInFiat("USDT", date2))
                .Returns(1m);

            _fiatConversion.Setup(t => t.TryGetExactPriceInFiat("ETH", date2))
                .Returns(2000m / 3m);

            // ACT
            _objectUnderTests.Process(trade1.Object);
            var result = _objectUnderTests.Process(trade2.Object).Single();

            Assert.AreEqual("BTC", result.Asset);
            Assert.AreEqual(0.5, result.Quantity);
            Assert.AreEqual(500m, result.Cost);
            Assert.AreEqual(2000, result.Proceed);
            Assert.AreEqual(1500, result.Gain);
            Assert.AreEqual(new DateTime(2017, 07, 01), result.BoughtTimeReadable);
            Assert.AreEqual(new DateTime(2017, 11, 01), result.SoldTimeReadable);
        }
    }
}