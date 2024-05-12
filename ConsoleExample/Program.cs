using MarsRoversCore;
using ConsoleExample;
using System.Text;

IPlateau? plateau = null;

int width = -1;
int height = -1;

string? input = null;

void ResetInput()
{
    input = "*";
}

do
{
    Console.WriteLine("Select from the following options:");
    Console.WriteLine("\tP+Enter to create a new plateau;");
    Console.WriteLine("\tH+Enter or ?+Enter to view help;");
    Console.WriteLine("\tEnter to exit.");

    input = Console.ReadLine();

    if (!string.IsNullOrEmpty(input))
    {
        var inputChar = input.ToUpper()[0];
        switch (inputChar)
        {
            case 'P':
                bool onPLateau = false;
                do
                {
                    onPLateau = ManagePlateau();
                    if (onPLateau)
                    {
                        bool stillPlaying = false;
                        do
                        {
                            stillPlaying = ManageRovers();
                        } while (stillPlaying);
                        
                    }
                    else
                        ResetInput();
                } while (onPLateau);

                break;
            case 'H':
            case '?':
                Console.WriteLine(string.Format("The first step is to create a plateau: it can only be a rectangle between {0} and {1} units wide ", Plateau.MIN_WIDTH, Plateau.MAX_WIDTH));
                Console.WriteLine(string.Format("and between {0} and {1} deep. The width is measured from West to East, and the depth, from South to North, so (0, 0)", 
                    Plateau.MIN_HEIGHT, Plateau.MAX_HEIGHT));
                Console.WriteLine("represents the southwestern-most corner, while (widht, height) represent the northeastern-most one.");
                Console.WriteLine("Once the plateau is created, the next step is to deploy some rovers to it.");
                Console.WriteLine("Each rover gests a humanly-readable name (in addition to an auto-assigned ID) to simplify the rover's selection for ");
                Console.WriteLine("future interaction. Rovers are deployed at the specified locations on the plateau, each facing in the specified ");
                Console.WriteLine("direction. These parameters are specified as a string in the following format: the X (West-East) cordinate, space, ");
                Console.WriteLine("the Y (South-North) coordinate, space, the orientation. The orinetation can be N for North, S for South, E for ");
                Console.WriteLine("East, and W for West. The orinetation is important because a rover can move only in the direction it is facing: ");
                Console.WriteLine("more on this below. Once the desired number of rovers are deployed, it becomes possible to select one by name ");
                Console.WriteLine("and check its location or send it a command string. A command string is a sting with no spaces, each letter ");
                Console.WriteLine("representing an atomic command. L makes the rover turn left - change its orientation from N (North) to W (West), ");
                Console.WriteLine("for example; R makes it turn right (from (N)orth to (E)ast); and M makes it move one unit in the direction it ");
                Console.WriteLine("is facing (this is how the orientation is important: it determnies the direction of the rover's movement). ");
                Console.WriteLine("Repeating each letter in the command string will make the rover repeat the corresponding action.");
                Console.WriteLine("On each stage, simply press Enter to move back and eventually to exit the program.");
                break;
            default:
                break;
        }
    }

} while (!string.IsNullOrEmpty(input));

IPlateau? CreatePlpateau(int width, int height)
{
    return new Plateau(width, height);
}

bool ManagePlateau()
{
    bool quitting = false;
    while (!quitting)
    {
        var txtNum = string.Empty;
        bool converted = false;
        do
        {
            Console.WriteLine("Enter the plateau width and press Enter (or just press Enter to go back):");
            txtNum = Console.ReadLine();
            converted = Int32.TryParse(txtNum, out width);
        } while (!string.IsNullOrEmpty(txtNum) && !converted);

        quitting = string.IsNullOrEmpty(txtNum); // We want to quit

        if (!quitting)
        {
            do
            {
                Console.WriteLine("Enter the plateau height and press Enter (or just press Enter go back):");
                txtNum = Console.ReadLine();
                converted = Int32.TryParse(txtNum, out height);
            } while (!string.IsNullOrEmpty(txtNum) && !converted);
        }

        quitting = string.IsNullOrEmpty(txtNum);

        if (!quitting)
        {
            plateau = CreatePlpateau(width, height);
            if (plateau != null)
            {
                Console.WriteLine("Created a plateau {0:D}: {1} units wide (West to East) and {2} units deep (South to Nort).",plateau.ID, plateau.Width, plateau.Height);
                break;
            }
        }
    }

    // If quitting has become true, we don't have a plateau and want out; if not, we want to continue.
    return !quitting;
}

bool ManageRovers()
{
    bool result = plateau != null;

    if (result)
    {
        List<NamedRover> rovers = new List<NamedRover>();

        string? roversInput = string.Empty;

        do
        {
            string prompt = rovers.Count <= 0 ? string.Format("No rovers on the plateau {0:D}", plateau?.ID) : string.Format("The following rovers are on the plateau {0:D}:", plateau?.ID);
            Console.WriteLine(prompt);

            if (rovers.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var rover in rovers)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(rover.Name);
                }
                Console.WriteLine(string.Format("\t{0}", sb.ToString()));
            }

            Console.WriteLine("Select from the following options:");
            Console.WriteLine("\tA+Enter to add a new rover;");
            Console.WriteLine("\tRover's name to interact with an existing rover;");
            Console.WriteLine("\tEnter to go back;");

            roversInput = Console.ReadLine();

            if (!string.IsNullOrEmpty(roversInput))
            {
                if (roversInput == "A" || roversInput == "a")
                {
                    //Append rover
                    Console.WriteLine("Enter the new rover's name (entering a blank name will cause the rover to be named by its ID):");
                    var name = Console.ReadLine();
                    Console.WriteLine("Enter the new rover's deploymnet coordinates, in the \"X Y [orientation]\" format (no double quotes),");
                    Console.WriteLine("where [orientation] can be N, S, E, or W for the rover to face towards the corresonding side of the compass:");
                    var initialInput = Console.ReadLine();
                    string errorMessage = string.Empty;
                    if (plateau != null && initialInput != null)
                    {
                        var rover = Rover.Deploy<NamedRover>(plateau, initialInput, out errorMessage) as NamedRover;
                        if (rover != null && name != null)
                        {
                            rover?.AssignName(name);
                            rovers.Add(rover!);
                            Console.WriteLine(string.Format("Added the rover {0} ({1:D})", rover?.Name, rover?.ID));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.WriteLine(string.Format("The following error has occurred: {0}", errorMessage));
                            }
                            else
                                Console.WriteLine("An unexpected error has occurred");
                        }
                    }
                    else
                        Console.WriteLine("An unexpected error has occurred");
                }
                else
                {
                    var rover = (from r in rovers where r.Name.ToLower() == roversInput.ToLower().Trim() select r).FirstOrDefault();
                    if (rover != null)
                    {
                        RoverInteraction(rover);
                    }
                }
            }
            else
            {
                result = false;
            }

        } while (!string.IsNullOrEmpty(roversInput));
    }

    return result;
}

void RoverInteraction(NamedRover? rover)
{
    if (rover != null)
    {
        string? interactionInput = string.Empty;

        do
        {
            Console.WriteLine("Select from the following options:");
            Console.WriteLine("\tL+Enter to check the rover's location;");
            Console.WriteLine("\tC+Enter to send the rover the string of commands");
            Console.WriteLine("\t\t(NOTE: the command string should not contain spaces, and may only include ");
            Console.WriteLine("\t\tthe letters L, R, and M, in any combinations. L makes the rover turn left ");
            Console.WriteLine("\t\t(change its orientation from N (North) to W (West), for example; R makes it ");
            Console.WriteLine("\t\tturn right (from N (North) to E (East); and M makes it move one unit in the ");
            Console.WriteLine("\t\tdirection it is facing. Repeating each letter makes the rover repeat the ");
            Console.WriteLine("\t\trelevant action (RMLMM will make it turn right, move one unit, then turn left, ");
            Console.WriteLine("\t\tthen move two units.");
            Console.WriteLine("\tEnter to go back.");

            interactionInput = Console.ReadLine();

            if (!string.IsNullOrEmpty(interactionInput))
            {
                var upper = interactionInput.ToUpper();
                switch (upper[0])
                {
                    case 'L':
                        Console.WriteLine(((IRover)rover).CurrentPosition);
                        break;
                    case 'C':
                        Console.WriteLine("Enter the command string:");
                        var command = Console.ReadLine();
                        Console.WriteLine(((IRover)rover).Command(command ?? string.Empty));
                        break;
                    default:
                        break;
                }
            }

        } while (!string.IsNullOrEmpty(interactionInput));
    }
}


