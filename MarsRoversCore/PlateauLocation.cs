
namespace MarsRoversCore
{
    /// <summary>
    /// An abstract location on an instance of IPlateau that may be 
    /// occupied (contain an instance of IRover) or not at any given moment.
    /// Normally, every instance of IPlateauLocation is attached to the 
    /// containing IPlateau: they are not meant to be instantiated independently.
    /// </summary>
    public interface IPlateauLocation
    {
        int X { get; }
        int Y { get; }
        Guid PlatoId { get; }

        bool IsAvailable { get { return Rover == null; } }
        IRover? Rover { get; }
        bool Enter(IRover rover, IPlateauLocation? origin);
        void Vacate();
    }

    /// <summary>
    /// The basic implementation of the IPlateauLocation interface
    /// </summary>
    public class PlateauLocation: IPlateauLocation
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Guid PlatoId { get; private set; }

        public IRover? Rover { get; private set; }

        /// <summary>
        /// Creates the new instance for the plateau plateauId,at the cooridnates x, y (the row Y, the column x)
        /// </summary>
        /// <param name="plateauId">The ID of the plateauthis location will belong to.</param>
        /// <param name="x">The column to contain this location. WARNING: there is no check whether it is within the plateau's dimensions.</param>
        /// <param name="y">The row to contain this location. WARNING: there is no check whether it is within the plateau's dimensions.</param>
        public PlateauLocation(Guid plateauId, int x, int y)
        {
            this.PlatoId = plateauId;
            this.X = x;
            this.Y = y;
            Rover = null;
        }

        public bool Enter(IRover rover, IPlateauLocation? origin)
        {
            bool result = false;
            if (origin != null)
                origin.Vacate();
            this.Rover = rover;
            return result;
        }

        public void Vacate()
        {
            Rover = null;
        }
    }
}
