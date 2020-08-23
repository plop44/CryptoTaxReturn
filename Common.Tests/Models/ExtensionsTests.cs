using Common.Models;
using NUnit.Framework;

namespace Common.Tests.Models
{
    public class ExtensionsTests
    {
        [Test]
        public void WhenWeCleanValuesTo8DecimalsPlace()
        {
            var value = 0.1234567891011m;
            var result = value.CleanValueTo8Decimals();

            Assert.AreEqual(0.12345678m, result);
            Assert.AreEqual("0.12345678", result.ToString());
        }
        [Test]
        public void WhenWeCleanValuesTo8DecimalsPlaceOnlyZeros()
        {
            var value = 1.000000000000000000001m;
            var result = value.CleanValueTo8Decimals();

            Assert.AreEqual(1m, result);
            Assert.AreEqual("1", result.ToString());
        }
    }
}