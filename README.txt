	
	This solution is for the PROBLEM TWO: MARS ROVERS. 

	First, let me re-state the problem as I understood it.
	There is at least one plateau, which has the dimensions of X by Y 
units. A single "square unit" can hold zero or one rover at a time, 
which means that the plateau itself can hold no more than X by Y 
rovers.
	And here is the first assumption that I made: I assumed that 
it is important for the rovers to be able to move; to achieve that, 
I have limited the maximum number of rovers on the plateau to X*Y-1.
This way, at least one rover can move, and then its neighbour can 
move to the place it has vacated, an so on.
	Now a rover can be deployed on a plateau (on A plateau, not 
THE plateau, because I prefer to design with expansion in mind) at 
any location between (0,0) and (X-1,Y-1), and it can be initially facing 
in any of the four directions, North, West, South, or East. Once deployed, 
the rover can report its location (as, for example, "12 14 E", meaning 
"I am at 12 units X, 14 units Y, facing East"), and it can receive and 
execute commands. The commands come as uninterrupted (no spaces) 
strings that can contain only the following symbols:
	L tells the rover to turn left, relative to the direction it is 
	facing;
	R tells the rover to turn right, again relative to wherever it 
	is facing at the moment;
	M tells the rover to move one unit in the direction it is facing.
All turns are 90 degrees: if the rover faces North, L will make it 
turn to face West, and R, East. Obviously, turning South will require 
either LL or RR.
	Let us suppose that the rover's current location is "12 14 E", as 
above. Now suppose it receives a command LMMMLMRRMMMMR. So, first it 
is going to turn North, then move 3 units Northward, then turn West, 
move one unit Westward, turn 180 degrees right (which will leave it
facing East again), the move 4 units Eastward, and then turn right 
(South), while remaining where it was.
	In order to calculate the rover's new location we agree that on our 
plateau, positive X means East, and positive Y means North, so the point 
(0,0) is at the southwestern corner, and (X-1,Y-1), at the north-eastern 
one. So, once again, the rover turns North and moves 3 units, making its 
Y coordinate 14 + 3 = 17 so, the new location will look like "12 17 N" 
(the rover is not going to report it as it is still in the middle of 
action). Now, it turns left again, to face West, and moves one unit. This
makes its X coordinate 12 - 1 = 11, and the new interim location,
"11 17 W". Then it turns 180 degrees (it does not matter right or left),
so that now it faces East; after moving 4 units Eastward, its X 
coordinate becomes 11 + 4 = 15. Finally, staying at the same place, it 
turns right, to face South. This means, its final location is "15 17 S".
This location will appear aster the rover finishes executing the 
command.
	Here are some other assumptions that I made: first, a rover cannot 
move off the plateau. Since it executes the movement commands one at 
a time, it can easily estimate that the next move will be disastrous, 
stop before making it, and report an error. Next, if the unit in front 
of the moving rover is already occupied by another rover, the moving 
rover also stops next to it and report a "collision alert". It may be 
important to remember that both situations will leave the rover not 
where it was and not where it was going, but at an unpredictable 
location.
	This outlines my understanding of the problem that I attempted to
solve.

	Now for the abstractions. The two obvious ones are a rover and a 
plateau. As I already mentioned, treating a plateau as an abstraction 
makes it possible for the solution to adapt to a situation where we 
have, for example, multiple plateaus on the planet, then multiple 
planets, and so on. However, the question is, who "knows" about whom? 
Obviously, rover are deployed on a plateau, so they can "know" about 
this plateau (hold a reference to it); does the plateau "know" about 
the rovers deployed on it? Should it? I did not think so and as a 
result, came  up with a third abstraction: a plateau location that 
represents a "square unit" of a plateau which may or may not contain 
a rover.
	
	And this brings us to the solution, as in Visual Studio 2022 
solution.
	There are four projects: MarsRoverCore, a class library that 
exposes the three interfaces, IPLateau, IPLateauLocation, and 
IRover (I believe the names are self-explanatory by now), as well 
as the simplest implementations of these interfaces; CoreUnitTests 
that contains a comprehensive set of unit tests for the functionality
exposed by MarsRoverCore; ConsoleExample, a console app that lets the 
user create a plateau of the desired dimensions, deploy a number of 
rovers on it, and then choose a rover, send it a command, and 
monitor its execution; and finally, WpfExample that does the same 
thing as ConsoleExample, only adds a basic GUI to it.
	All projects are based on .Net Core 6 (LTS): I did not believe 
running them was worth upgrading, to someone who perhaps had good 
reasons not to rush it. However, when I tried to move the project to
a different PC, I had to download the .NET 6.0 Runtime (to run the 
unit tests) and the .NET 6.0 Desktop Runtime (to run the WPF example),
eben though that PC too had .NET 8. Just in case, here are the links:
.NET 6.0 Runtime
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.29-windows-x64-installer
.NET 6.0 Desktop Runtime
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.29-windows-x64-installer?cid=getdotnetcore
	I ran the unit tests from Visual Studio's Test Explorer: simply 
right-clicked at the top of the test tree it shows and selected Run. 
The unit tests do not output anything (apart from the standard 
pass/fail Visual Studio GUI), and it may be necessary to look at 
their code to ascertain whether they are as comprehensive as 
expected.
	The console app attempts to guide the user through the process 
with rather wordy prompts: first, it offers to create a plateau; 
next, to add some rovers to it, while also displaying the list of 
the existing rover which the user can select and then send a 
command to it and see the response. It also has a "help" option.
	And the WPF app actually displays the generated plateau as a grid
of plus ("+") signs. Clicking on any such sign brings up a rover 
deployment dialog that lets the user select the direction the new 
rover will face and then deploys it where the user has clicked. 
The rovers appear as arrows pointing where each rover is facing. 
Clicking on a rover selects it for communication: the user can 
then clear the text box below, type in a command, and click the
Send button; the rover's response will appear in the same text box,
and the rover's icon will move to a new location/orientation.
	Unfortunately, multithreading simply did not get into the picture
with this solution: it is entirely single-threaded and may require 
some re-work to handle, for example, multiple users "talking" to 
the same rovers on the same plateau from different network locations, 
at unpredictable times.

		