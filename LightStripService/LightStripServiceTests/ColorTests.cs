using ChristmasLightServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChristmasLightServerTests
{
    [TestClass]
    public class ColorTests
    {
        [TestMethod]
        public void FromHSVTests()
        {
            var test = Color.FromHSV(0, 0, 0);
            Assert.IsTrue(test == new Color(0, 0, 0));

            test = Color.FromHSV(0, 0, 100);
            Assert.IsTrue(test == new Color(255, 255, 255));

            test = Color.FromHSV(120, 77, 55);
            Assert.IsTrue(test == new Color(32, 140, 32));

            test = Color.FromHSV(1, 1, 1);
            Assert.IsTrue(test == new Color(3, 3, 3));
        }

    }
}
