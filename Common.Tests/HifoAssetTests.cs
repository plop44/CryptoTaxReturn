using System;
using System.Linq;
using Common.Models;
using NUnit.Framework;

namespace Common.Tests
{
    public class HifoAssetTests
    {
        private Asset _objectUnderTest;

        [SetUp]
        public void Setup()
        {
            _objectUnderTest= new HifoAsset(new AssetName("Plop"));
        }

        [Test]
        public void WhenWeGetSellSide()
        {
            var source = new object();

            var assetCost = new AssetCost(DateTime.Now.ToUnixDateTime(),10,2.2m, source);
            _objectUnderTest.IncrementAmount(assetCost);

            var sellSideFor = _objectUnderTest.DecrementAmount(10, null).ToArray();

            CollectionAssert.AreEquivalent(new[] { assetCost }, sellSideFor);
            CollectionAssert.IsEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideWithZero()
        {
            var source = new object();
            
            var assetCost = new AssetCost(DateTime.Now.ToUnixDateTime(), 10, 2.2m, source);
            _objectUnderTest.IncrementAmount(assetCost);

            var sellSideFor = _objectUnderTest.DecrementAmount(0, null).ToArray();

            CollectionAssert.IsEmpty(sellSideFor);
            CollectionAssert.IsNotEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndThereIsNothing()
        {
            var sellSideFor = _objectUnderTest.DecrementAmount(10,null).ToArray();

            CollectionAssert.IsEmpty(sellSideFor);
            CollectionAssert.IsEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(1, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndThisIsNegative()
        {
            Assert.Throws<Exception>(()=> _objectUnderTest.DecrementAmount(-10, null).ToArray());
        }

        [Test]
        public void WhenWeGetSellSideAndThereIsNotEnough()
        {
            var source = new object();


            var assetCost = new AssetCost(DateTime.Now.ToUnixDateTime(), 10, 2.2m, source);
            _objectUnderTest.IncrementAmount(assetCost);

            var sellSideFor = _objectUnderTest.DecrementAmount(12, null).ToArray();

            CollectionAssert.AreEqual(new[] {assetCost}, sellSideFor);
            CollectionAssert.IsEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(1, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndWeNeedMultipleValues()
        {
            var source1 = new object();
            var source2 = new object();

            var assetCost1 = new AssetCost(DateTime.Now.ToUnixDateTime(), 8, 2.2m, source1);
            var assetCost2 = new AssetCost(DateTime.Now.ToUnixDateTime(), 2, 3.3m, source2);
            _objectUnderTest.IncrementAmount(assetCost1);
            _objectUnderTest.IncrementAmount(assetCost2);

            var sellSideFor = _objectUnderTest.DecrementAmount(10, null).ToArray();

            CollectionAssert.AreEqual(new[] { assetCost2 , assetCost1 }, sellSideFor);
            CollectionAssert.IsEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndWeNeedMultipleValues2()
        {
            var source1 = new object();
            var source2 = new object();

            var assetCost1 = new AssetCost(DateTime.Now.ToUnixDateTime(), 8, 2.2m, source1);
            var assetCost2 = new AssetCost(DateTime.Now.ToUnixDateTime(), 3, 3.3m, source2);
            _objectUnderTest.IncrementAmount(assetCost1);
            _objectUnderTest.IncrementAmount(assetCost2);

            var sellSideFor = _objectUnderTest.DecrementAmount(10, null).ToArray();

            Assert.AreEqual(2, sellSideFor.Length);
            Assert.AreEqual(3, sellSideFor[0].RemainingQuantity);
            Assert.AreEqual(7, sellSideFor[1].RemainingQuantity);
            Assert.AreEqual(3.3, sellSideFor[0].CostPerUnit);
            Assert.AreEqual(2.2, sellSideFor[1].CostPerUnit);
            Assert.AreEqual(source2, sellSideFor[0].Item);
            Assert.AreEqual(source1, sellSideFor[1].Item);

            Assert.AreEqual(1, _objectUnderTest.AssetCosts.Single().RemainingQuantity);
            Assert.AreEqual(2.2m, _objectUnderTest.AssetCosts.Single().CostPerUnit);
            Assert.AreEqual(source1, _objectUnderTest.AssetCosts.Single().Item);

            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndWeNeedMultipleValues3()
        {
            var source1 = new object();
            var source2 = new object();

            var assetCost1 = new AssetCost(DateTime.Now.ToUnixDateTime(), 8, 4.4m, source1);
            var assetCost2 = new AssetCost(DateTime.Now.ToUnixDateTime(), 2, 3.3m, source2);
            _objectUnderTest.IncrementAmount(assetCost1);
            _objectUnderTest.IncrementAmount(assetCost2);

            var sellSideFor = _objectUnderTest.DecrementAmount(10, null).ToArray();

            CollectionAssert.AreEqual(new[] { assetCost1 , assetCost2 }, sellSideFor);
            CollectionAssert.IsEmpty(_objectUnderTest.AssetCosts);
            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }

        [Test]
        public void WhenWeGetSellSideAndWeNeedMultipleValues4()
        {
            var source1 = new object();
            var source2 = new object();

            var assetCost1 = new AssetCost(DateTime.Now.ToUnixDateTime(), 8, 4.4m, source1);
            var assetCost2 = new AssetCost(DateTime.Now.ToUnixDateTime(), 3, 3.3m, source2);
            _objectUnderTest.IncrementAmount(assetCost1);
            _objectUnderTest.IncrementAmount(assetCost2);

            var sellSideFor = _objectUnderTest.DecrementAmount(10, null).ToArray();

            Assert.AreEqual(2, sellSideFor.Length);
            Assert.AreEqual(8, sellSideFor[0].RemainingQuantity);
            Assert.AreEqual(2, sellSideFor[1].RemainingQuantity);
            Assert.AreEqual(4.4, sellSideFor[0].CostPerUnit);
            Assert.AreEqual(3.3, sellSideFor[1].CostPerUnit);
            Assert.AreEqual(source1, sellSideFor[0].Item);
            Assert.AreEqual(source2, sellSideFor[1].Item);

            Assert.AreEqual(1, _objectUnderTest.AssetCosts.Single().RemainingQuantity);
            Assert.AreEqual(3.3m, _objectUnderTest.AssetCosts.Single().CostPerUnit);
            Assert.AreEqual(source2, _objectUnderTest.AssetCosts.Single().Item);

            Assert.AreEqual(0, _objectUnderTest.SellSideBreaches);
        }
    }
}