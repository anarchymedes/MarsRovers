
namespace MarsRoversCore
{
    /// <summary>
    /// The abstract rectangular plateau to deploy the rovers to.
    /// Its height is from 0 to Height (the SN direction), and its
    /// width is from 0 to Width (the WE direction); each point 
    /// represented by x, y (col, row) coordinates is an
    /// instance of IPlateauLocation type that may or may not be occupied by an IRover instance.
    /// </summary>
    public interface IPlateau
    {
        int Width { get; }
        int Height { get; }
        // The plateau ID is for the possible expansion: multiple plateaus on the same planet, multiple planets, etc.:
        // this way, those won't be breaking changes - or at least they'll break less :-)
        Guid ID { get; }
        IPlateauLocation[,] locations { get; } //Row-first: [y, x]
    }

    /// <summary>
    /// The basic implementation of the IPlateau interface
    /// </summary>
    public class Plateau: IPlateau
    {
        // These are chosen arbitrarily: while the minimum of 2 makes sense,
        // the maximum is only limited by the availabe memory and within that limit,
        // can be anything.
        public const int MIN_WIDTH = 2;
        public const int MIN_HEIGHT = 2;
        public const int MAX_WIDTH = 100;
        public const int MAX_HEIGHT = 100;

        private int width = MIN_WIDTH;
        private int height = MIN_HEIGHT;

        private int LimitWidth(int value)
        {
            int width = MIN_WIDTH;

            if (value > MIN_WIDTH)
            {
                if (value < MAX_WIDTH)
                    width = value;
                else
                    width = MAX_WIDTH;
            }
            else
                width = MIN_WIDTH;

            return width;
        }

        private int LimitHeight(int value)
        {
            int height = MIN_HEIGHT;

            if (value > MIN_HEIGHT)
            {
                if (value < MAX_HEIGHT)
                    height = value;
                else
                    height = MAX_HEIGHT;
            }
            else
                height = MIN_HEIGHT;

            return height;
        }

        public int Width { get { return width; } private set 
            { 
                width = LimitWidth(value);
            } 
        }

        public int Height { get { return height; } private set 
            { 
                height = LimitHeight(value);
            } 
        }

        public IPlateauLocation[,] locations { get; private set; } 

        public Guid ID { get; private set; } 

        public Plateau(int width, int height)
        {
            Width = width;
            Height = height;

            ID = Guid.NewGuid();

            locations = new IPlateauLocation[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    locations[i, j] = (IPlateauLocation) new PlateauLocation(ID, j, i);
                }
            }
        }
    }
}
