using MarsRoversCore;

namespace CoreUnitTests
{
    [TestClass]
    public class RoverTests
    {
        const int testPlateauWidth = 16;
        const int testPlateauHeight = 10;

        void testRoverDeployment(IPlateau testPlateau, string initialInput)
        {
            string errorMessage = string.Empty;

            var parts = initialInput.Split(' ');
            var x = Int32.Parse(parts[0]);
            var y = Int32.Parse(parts[1]);

            var rover = Rover.Deploy<Rover>(testPlateau, initialInput, out errorMessage);
            Assert.IsNotNull(rover);                                        // The rover has been deployed
            Assert.AreEqual(rover.location, testPlateau.locations[y, x]);   // The location of the new rover is correct
            Assert.AreEqual(rover.location?.IsAvailable, false);            // The new location correctly set up
            Assert.AreEqual(rover.location?.Rover, rover);                  // As above: the rover-location relationship
            Assert.AreEqual(rover.CurrentPosition, initialInput.ToUpper()); // The rover's response (always in the upper case)
            Assert.AreEqual(errorMessage, string.Empty);                    // No other issues
        }

        [TestMethod]
        public void SimpleDeploymentTest()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);

            testRoverDeployment(testPlateau, "6 8 N");
            testRoverDeployment(testPlateau, "12 5 e"); // Input string should be case-insensitive
            testRoverDeployment(testPlateau, "5 2 S");
            testRoverDeployment(testPlateau, "2 7 w");  // Input string should be case-insensitive

            // Extra insurance on the plateau integrity
            var busy = (from IPlateauLocation l in testPlateau.locations where !l.IsAvailable select l).Count();
            var taken = (from IPlateauLocation l in testPlateau.locations where l.Rover != null select l).Count();

            Assert.AreEqual(busy, 4);
            Assert.AreEqual(busy, taken);
        }

        [TestMethod]
        public void DeploymentWithIinvalidInputTest()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            var rover = Rover.Deploy<Rover>(testPlateau, string.Empty, out errorMessage);
            Assert.IsNull(rover);
            Assert.AreEqual(errorMessage, "Invalid input");

            rover = Rover.Deploy<Rover>(testPlateau, "abrakadabra", out errorMessage);
            Assert.IsNull(rover);
            Assert.AreEqual(errorMessage, "Invalid parameters");

            // Wrong orientation
            rover = Rover.Deploy<Rover>(testPlateau, "6 5 F", out errorMessage);
            Assert.IsNull(rover);
            Assert.AreEqual(errorMessage, "Invalid parameters");

            // Wrong coordinates
            rover = Rover.Deploy<Rover>(testPlateau, "6 -5 S", out errorMessage);
            Assert.IsNull(rover);
            Assert.AreEqual(errorMessage, "Invalid parameters");

            // Outside of the plateau
            rover = Rover.Deploy<Rover>(testPlateau, string.Format("{0} {1} N", testPlateauWidth + 1, testPlateauHeight + 1), out errorMessage);
            Assert.IsNull(rover);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
            Assert.IsTrue(errorMessage.Contains("does not exist on plateau"));
        }

        [TestMethod]
        public void DeploymentOnOccupiedLocation()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            // The 'lawful owner': should be deployed successfully
            var rover = Rover.Deploy<Rover>(testPlateau, "6 5 N", out errorMessage);
            Assert.IsNotNull(rover);
            Assert.AreEqual(errorMessage, string.Empty);

            var invader = Rover.Deploy<Rover>(testPlateau, "6 5 S", out errorMessage);
            Assert.IsNull(invader);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
            Assert.IsTrue(errorMessage.Contains("is already occupied by rover"));

            // Make sure the 'lawful owner' hasn't been damaged
            Assert.IsNotNull(rover);
            Assert.AreEqual(rover.CurrentPosition, "6 5 N");
        }

        [TestMethod]
        public void DeployToOvercrowding()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            for (int i = 0; i< testPlateauHeight; i++)
            {
                for (int j = 0; j< testPlateauWidth; j++)
                {
                    var rover = Rover.Deploy<Rover>(testPlateau, string.Format("{0} {1} N", j, i), out errorMessage);
                    if (rover == null && !string.IsNullOrEmpty(errorMessage))
                    {
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(errorMessage))
                    break;
            }

            Assert.IsTrue(errorMessage.Contains("cannot hold any more rovers"));
            Assert.IsTrue(errorMessage.Contains(string.Format("{0:D}", testPlateau.ID)));
        }

        [TestMethod]
        public void SimpleCommandTest()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            // We've already tested this above but - just in case
            var rover = Rover.Deploy<Rover>(testPlateau, "6 5 N", out errorMessage);
            Assert.IsNotNull(rover);
            Assert.AreEqual(errorMessage, string.Empty);

            var output = rover.Command("LMLMM"); // Turn left (W), move Westward (x - 1), left again (S), Southward, twice (y - 2)
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.AreEqual(output, "5 3 S");
            Assert.AreEqual(output, rover.CurrentPosition);

            // Now turn left (E), Eastward, three times (x + 3), right, three times (N), Nothward, three times (y + 3), left (W), Westward (x - 1)
            output = rover.Command("LMmMRrRMmmlM"); // Commands are case-insensitive
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.AreEqual(output, "7 6 W");
            Assert.AreEqual(output, rover.CurrentPosition);

            var before = rover.CurrentPosition;
            output = rover.Command(string.Empty); // We're not moving anywhere
            Assert.AreEqual(output, before);
            Assert.AreEqual(output, rover.CurrentPosition);

            //No trails
            var busy = (from IPlateauLocation l in testPlateau.locations where !l.IsAvailable select l).Count();
            var taken = (from IPlateauLocation l in testPlateau.locations where l.Rover != null select l).Count();

            Assert.AreEqual(busy, 1);
            Assert.AreEqual(busy, taken);
        }

        [TestMethod]
        public void InvalidCommandTest()
        {
            // We've already tested that part in PLateauTests
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            // We've already tested this above, but - just in case
            var rover = Rover.Deploy<Rover>(testPlateau, "6 5 N", out errorMessage);
            Assert.IsNotNull(rover);
            Assert.AreEqual(errorMessage, string.Empty);

            var before = rover.CurrentPosition;

            var output = rover.Command("wtf");
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("Invalid command"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover.ID)));
            // A more sublte case...
            output = rover.Command("LMMRN");
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("Invalid command"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover.ID)));
            // We're not moving anywhere
            Assert.AreEqual(before, rover.CurrentPosition);
        }

        [TestMethod]
        public void MovingOffPlateauTest()
        {
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            var rover1 = Rover.Deploy<Rover>(testPlateau, "6 8 N", out errorMessage);
            Assert.IsNotNull(rover1);
            Assert.AreEqual(errorMessage, string.Empty);

            var rover2 = Rover.Deploy<Rover>(testPlateau, "2 5 W", out errorMessage);
            Assert.IsNotNull(rover2);
            Assert.AreEqual(errorMessage, string.Empty);

            var rover3 = Rover.Deploy<Rover>(testPlateau, "14 5 E", out errorMessage);
            Assert.IsNotNull(rover3);
            Assert.AreEqual(errorMessage, string.Empty);

            var rover4 = Rover.Deploy<Rover>(testPlateau, "6 2 S", out errorMessage);
            Assert.IsNotNull(rover4);
            Assert.AreEqual(errorMessage, string.Empty);

            var command = "MMM";

            var output = rover1.Command(command); // Off the Nothern edge
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("nowhere to go in the North direction"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover1.ID)));

            output = rover2.Command(command); // Off the Western edge
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("nowhere to go in the West direction"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover2.ID)));

            output = rover3.Command(command); // Off the Eastern edge
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("nowhere to go in the East direction"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover3.ID)));

            output = rover4.Command(command); // Off the Eastern edge
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("nowhere to go in the South direction"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover4.ID)));

            // No trails
            var busy = (from IPlateauLocation l in testPlateau.locations where !l.IsAvailable select l).Count();
            var taken = (from IPlateauLocation l in testPlateau.locations where l.Rover != null select l).Count();

            Assert.AreEqual(busy, 4);
            Assert.AreEqual(busy, taken);
        }

        [TestMethod]
        public void CollisionTest()
        {
            var testPlateau = new Plateau(testPlateauWidth, testPlateauHeight);
            string errorMessage = string.Empty;

            var rover1 = Rover.Deploy<Rover>(testPlateau, "6 8 N", out errorMessage);
            Assert.IsNotNull(rover1);
            Assert.AreEqual(errorMessage, string.Empty);

            var rover2 = Rover.Deploy<Rover>(testPlateau, "2 5 W", out errorMessage);
            Assert.IsNotNull(rover2);
            Assert.AreEqual(errorMessage, string.Empty);

            var rover3 = Rover.Deploy<Rover>(testPlateau, "14 5 E", out errorMessage);
            Assert.IsNotNull(rover3);
            Assert.AreEqual(errorMessage, string.Empty);

            var output = rover1.Command("LLMMMRMMMM"); // Left, twice (S), Southward, 3 times (y - 3), right (W), Westward, 4 times (x - 4)
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("is already occupied"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover2.ID)));

            output = rover2.Command("RRmmmmmmmmmmmmmm"); // Right, twice (E), Eastward, 15 times (x + 15)
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("is already occupied"));
            // We tried to reach the rover 3, but rover 1 in the way, so...
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover1.ID)));

            //...get it out of the way - and double-check the movement, while we're at it
            output = rover1.Command("LM"); // Left, twice (S), Southward (y - 1)
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.AreEqual(output, rover1.CurrentPosition);
            //Now try again
            output = rover2.Command("mmmmmmmmmmmmmm"); // Eastward, 15 times (x + 15) - rover 2 should already be facing East
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("is already occupied"));
            Assert.IsTrue(output.Contains(string.Format("{0:D}", rover3.ID)));
        }
    }
}
