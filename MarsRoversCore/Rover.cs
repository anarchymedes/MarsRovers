using System.Text.RegularExpressions;
using System.Reflection;

namespace MarsRoversCore
{
    /// <summary>
    /// An abstract rover that can be deployed to an IPlateau instance, at a 
    /// certain IPlateauLocation. Each rover has a current position and orientation; 
    /// and it can take commands that make it rotate left (L), or right (R), or 
    /// move (M) one location in the direction of its current orientation.
    /// </summary>
    public interface IRover
    {
        Guid ID { get; }
        IPlateau? Plateau { get; }
        string InitialPosition { get; }
        char Direction { get; set; }

        IPlateauLocation? location
        {
            get
            {
                if (Plateau == null)
                    return null;
                else
                    return (from IPlateauLocation l in Plateau.locations where l.Rover?.ID == ID select l).FirstOrDefault();
            }
        }

        string CurrentPosition { get { return string.Format("{0} {1} {2}", location?.X ?? -1, location?.Y ?? -1, Direction); } }

        string Command(string command)
        {
            var response = String.Empty;

            if (!string.IsNullOrEmpty(command))
            {
                var upperCmd = command.ToUpper();

                string errorMessage = string.Empty;

                // Here we will use regex, to avoid starting a pointless loop
                Regex rg = new("^[LRM]+$");
                if (rg.IsMatch(upperCmd))
                {
                    for (int i = 0; i < upperCmd.Length; i++)
                    {
                        switch (upperCmd[i])
                        {
                            case 'L':
                                TurnLeft();
                                break;
                            case 'R':
                                TurnRight();
                                break;
                            case 'M':
                                MoveForward(out errorMessage);
                                break;
                            default:
                                response = string.Format("Rover ID {0:D}: Invalid command", ID);
                                break;
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response = errorMessage;
                            break; // No point continuing with the loop: we're not moving anywhere
                        }
                    }
                }
                else
                {
                    response = string.Format("Rover ID {0:D}: Invalid command", ID);
                }
            }

            if (string.IsNullOrEmpty(response)) // Meaning, nothing went boom: and if the command string was empty, we just stay where we are
            {
                response = CurrentPosition;
            }

            return response;
        }

        void TurnLeft()
        {
            char newDir = Direction;

            switch (Direction)
            {
                case 'N':
                    newDir = 'W';
                    break;
                case 'W':
                    newDir = 'S';
                    break;
                case 'S':
                    newDir = 'E';
                    break;
                case 'E':
                    newDir = 'N';
                    break;
                default:
                    ;
                    break;
            }
            Direction = newDir;
        }

        void TurnRight()
        {
            char newDir = Direction;

            switch (Direction)
            {
                case 'N':
                    newDir = 'E';
                    break;
                case 'E':
                    newDir = 'S';
                    break;
                case 'S':
                    newDir = 'W';
                    break;
                case 'W':
                    newDir = 'N';
                    break;
                default:
                    ;
                    break;
            }
            Direction = newDir;
        }

        bool MoveForward(out string errorMessage)
        {
            bool result = false;
            errorMessage = string.Empty;

            if (Plateau != null && location != null)
            {
                string setNotAvailable(IPlateauLocation newLocation)
                {
                    return string.Format("The location x: {0}; y: {1} on plateau {2:D} is already occupied by rover {3:D}",
                                                    newLocation.X, newLocation.Y, Plateau.ID, newLocation.Rover?.ID);
                }

                // To remind: plateau coordinates are column (y) first
                switch (Direction)
                {
                    case 'N':
                        if (location.Y >= Plateau.Height - 1)
                        {
                            errorMessage = string.Format("The rover {0:D} has nowhere to go in the North direction.", ID);
                        }
                        else
                        {
                            var newLocation = Plateau.locations[location.Y + 1, location.X];
                            if (newLocation.IsAvailable)
                            {
                                newLocation.Enter(this, location);
                            }
                            else
                                errorMessage = setNotAvailable(newLocation);
                        }
                        break;
                    case 'E':
                        if (location.X >= Plateau.Width - 1)
                        {
                            errorMessage = string.Format("The rover {0:D} has nowhere to go in the East direction.", ID);
                        }
                        else
                        {
                            var newLocation = Plateau.locations[location.Y, location.X + 1];
                            if (newLocation.IsAvailable)
                            {
                                newLocation.Enter(this,     location);
                            }
                            else
                                errorMessage = setNotAvailable(newLocation);
                        }
                        break;
                    case 'S':
                        if (location.Y <= 0)
                        {
                            errorMessage = string.Format("The rover {0:D} has nowhere to go in the South direction.", ID);
                        }
                        else
                        {
                            var newLocation = Plateau.locations[location.Y - 1, location.X];
                            if (newLocation.IsAvailable)
                            {
                                newLocation.Enter(this, location);
                            }
                            else
                                errorMessage = setNotAvailable(newLocation);
                        }
                        break;
                    case 'W':
                        if (location.X <= 0)
                        {
                            errorMessage = string.Format("The rover {0:D} has nowhere to go in the West direction.", ID);
                        }
                        else
                        {
                            var newLocation = Plateau.locations[location.Y, location.X - 1];
                            if (newLocation.IsAvailable)
                            {
                                newLocation.Enter(this, location);
                            }
                            else
                                errorMessage = setNotAvailable(newLocation);
                        }
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// The basic implementation of the IRover interface
    /// </summary>
    public class Rover : IRover
    {
        public Guid ID { get; private set; }
        public IPlateau? Plateau { get; private set; } = null;

        char IRover.Direction { get; set; } = 'N';

        public string InitialPosition { get; private set; }
        protected Rover(int x, int y, char dir, IPlateau plateau)
        {
            ((IRover)this).Direction = dir;
            InitialPosition = string.Format("{0} {1} {2}", x, y, dir);
            ID = Guid.NewGuid();
            Plateau = plateau;
        }

        /// <summary>
        /// A factory method that deploys (adds) a new rover (IRover) to a given plateau (IPlateau) 
        /// at a location, and with the orientation, given by the initial input string, This method is 
        /// generic over the rover type: they type must be Rover or its decendant, featuring a 
        /// protected constructor with the same set of parameters.
        /// </summary>
        /// <param name="plateau">The IPlateau instance to deploy the new rover to.</param>
        /// <param name="initialInput">A string containing the initial coordinates and orientation of the new rover: for example, "1 2 N". 
        /// The coordinates are given as "X Y" to deploy the rover at the row Y, the column X; 
        /// the orientation N (North) means the rower is facing upwards (the increasing row value).</param>
        /// <param name="errorMessage">If there was a problem deploying the new rover, this is where the problem's description can be found.</param>
        /// <returns>IRover instance if the deployment was successful; null otherwise. Even when it's not null, it's recommended to also check the errorMessage.</returns>
        public static IRover? Deploy<T>(IPlateau plateau, string initialInput, out string errorMessage) where T: Rover
        {
            errorMessage = string.Empty;

            IRover? rover = null;

            if (string.IsNullOrEmpty(initialInput))
            {
                errorMessage = "Invalid input";
                // By right, we should've thrown an exception here, but let's keep it simple:
                // if (string.IsNullOrEmpty(errorMessage)) {/* we're happy... */}
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                // Parse the input string
                int x = -1;
                int y = -1;
                char dir = 'Z';

                var parts = initialInput.Trim().ToUpper().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    if (!Int32.TryParse(parts[0].Trim(), out x))
                        x = -1; // Back to what it was: redundant, but easier to debug (step through) this way
                }

                if (parts.Length > 1)
                {
                    if (!Int32.TryParse(parts[1].Trim(), out y))
                        y = -1;
                }

                if (parts.Length > 2)
                {
                    dir = parts[2][0];
                }

                if (plateau == null || x < 0 || y < 0 || "NSEW".IndexOf(dir) < 0) // Too lazy to use the regex here...
                {
                    errorMessage = "Invalid parameters";
                    // Once again, should've been an exception :-)
                }
                else
                {
                    //Make sure the plateau is not so overcrowded that no rover can move (at least one location must be free)
                    var taken = (from IPlateauLocation l in plateau.locations where l.IsAvailable select l).Count();
                    if (taken <= 1)
                    {
                        errorMessage = string.Format("The plateau {0:D} cannot hold any more rovers", plateau.ID);
                    }
                    else
                    {
                        if (x < plateau.Width && y < plateau.Height && x >= 0 && y >= 0)
                        {
                            if (plateau.locations[y, x].IsAvailable)
                            {
                                Type type = typeof(T);
                                Type[] takes = new Type[4] { typeof(int), typeof(int), typeof(char), typeof(IPlateau) };
                                ConstructorInfo? constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, takes, null);
                                if (constructor != null)
                                    rover = (T)constructor.Invoke(new object[] { x, y, dir, plateau });
                                else
                                    throw new NullReferenceException(string.Format("Could not locate the expected constructor for the type {0}", type.Name));
                                plateau.locations[y, x].Enter(rover, null);
                            }
                            else
                            {
                                errorMessage = string.Format("The location x: {0}; y: {1} on plateau {2:D} is already occupied by rover {3:D}",
                                    x, y, plateau.ID, plateau.locations[y, x].Rover?.ID);
                            }
                        }
                        else
                        {
                            errorMessage = string.Format("The location x: {0}; y: {1} does not exist on plateau {2:D}", x, y, plateau.ID);
                        }
                    }
                }
            }

            return rover; // Should only be non-null when the rover has been deployed successfully (errorMessage == string.Empty)
        }
    }
}
