using MarsRoversCore;

namespace ConsoleExample
{
    public class NamedRover : Rover
    {
        public string Name { get; private set; }

        // For compatibility with Rover.Deploy<T>(...) method
        protected NamedRover(int x, int y, char dir, IPlateau plateau) : base(x, y, dir, plateau)
        {
            Name = string.Format("{0:D}", ID);
        }

        /// <summary>
        /// To be called once, after the new rover has been  created, to give it a name.
        /// This is the compensation for the limitation on the constructor
        /// </summary>
        /// <param name="name">The name of the rover</param>
        public void AssignName(string name)
        {
            if (Name == string.Format("{0:D}", ID))
                Name = name;
        }
    }
}
