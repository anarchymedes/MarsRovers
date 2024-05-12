using MarsRoversCore;

namespace CoreUnitTests
{
    [TestClass]
    public class PlateauTests
    {
        [TestMethod]
        public void TestPlateauCreation()
        {
            int testWidth = 5;
            int testHeight = 3;

            IPlateau? plateau = null;

            plateau = new Plateau(testWidth, testHeight);

            Assert.IsNotNull(plateau);
            Assert.AreEqual(testWidth, plateau.Width);
            Assert.AreEqual(plateau.Height, plateau.Height); 

            for (int i=0; i<testHeight; i++)
            {
                for (int j=0; j<testWidth; j++)
                {
                    Assert.IsNotNull(plateau.locations[i, j]);
                    Assert.IsInstanceOfType(plateau.locations[i, j], typeof(IPlateauLocation));
                }
            }
        }

        [TestMethod]
        public void TestPlateauDimensionLimits()
        {
            int validWidth = 7;
            int invalidWidth1 = 0;
            int invalidWidth2 = -3;
            int invalidWidth3 = 101;

            int validHeight = 5;
            int invalidHeight1 = 0;
            int invalidHeight2 = -2;
            int invalidHeight3 = 105;

            var plateau11 = new Plateau(validWidth, invalidHeight1);
            Assert.AreEqual(plateau11.Width, validWidth);
            Assert.AreEqual(plateau11.Height, Plateau.MIN_HEIGHT);

            var plateau12 = new Plateau(validWidth, invalidHeight2);
            Assert.AreEqual(plateau12.Width, validWidth);
            Assert.AreEqual(plateau12.Height, Plateau.MIN_HEIGHT);

            var plateau13 = new Plateau(validWidth, invalidHeight3);
            Assert.AreEqual(plateau13.Width, validWidth);
            Assert.AreEqual(plateau13.Height, Plateau.MAX_HEIGHT);

            var plateau21 = new Plateau(invalidWidth1, validHeight);
            Assert.AreEqual(plateau21.Height, validHeight);
            Assert.AreEqual(plateau21.Width, Plateau.MIN_WIDTH);

            var plateau22 = new Plateau(invalidWidth2, validHeight);
            Assert.AreEqual(plateau22.Height, validHeight);
            Assert.AreEqual(plateau22.Width, Plateau.MIN_WIDTH);

            var plateau23 = new Plateau(invalidWidth3, validHeight);
            Assert.AreEqual(plateau23.Height, validHeight);
            Assert.AreEqual(plateau23.Width, Plateau.MAX_WIDTH);

            var plateau33 = new Plateau(invalidWidth1, invalidHeight3);
            Assert.AreEqual(plateau33.Width, Plateau.MIN_WIDTH);
            Assert.AreEqual(plateau33.Height, Plateau.MAX_HEIGHT);
        }
    }
}