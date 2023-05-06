using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;

namespace GameOfLife
{
    // An enum representing the state of a cell in terms of whether it is alive or dead.
    public enum EAliveState
    {
        alive,
        dead
    }

    // An enum representing the infection state of a cell in terms of whether it is infected, vaccinated, or organic.
    public enum EInfectedState
    {
        infected,
        vaccinated,
        organic
    }

    // An enum representing the direction of the neighboring cells relative to a given cell.
    public enum EDirection
    {
        right,
        down,
        left,
        up,
        topleft,
        topright,
        bottomleft,
        bottomright
    }

    // This is a struct that defines the state of a single cell in the game
    public struct StructCellState
    {
        // The current state of the cell - either alive or dead
        private EAliveState eAliveState;

        
    // Public property to get and set the current alive state of the cell
    public EAliveState PEAliveState
        {
            get
            {
                return eAliveState;
            }

            set
            {
                // Set the current alive state of the cell to the given value
                eAliveState = value;

                // If the cell is dead, set the infected state to organic
                if (eAliveState == EAliveState.dead)
                {
                    eInfectedState = EInfectedState.organic;
                }
            }
        }

        // The current infected state of the cell - either infected, vaccinated, or organic
        public EInfectedState eInfectedState;
    }

    // This class represents the Game of Life simulation.
    static public class Game
    {
        // Flag used to indicate if the simulation should be terminated.
        public static bool bExit = false;
        // Random number generator used for various purposes throughout the simulation.
        public static Random rand = new Random();

        // Maximum number of rows in the grid.
        public static int MAX_ROWS = 30;

        // Maximum number of columns in the grid.
        public static int MAX_COLS = 80;

        // Two-dimensional array that represents the cells in the grid.
        public static Cell[,] organism;

        // This class represents an individual cell in the grid.
        public class Cell
        {
            const int MAX_VIRUSES = 50;
            const int MAX_VACCINES = 50;
            // Maximum number of neighboring cells that a cell can have.
            public static int MAX_NEIGHBORS = Enum.GetNames(typeof(EDirection)).Length;

            // Array of neighboring cells.
            public Cell[] neighbor;

            // The next cell in the simulation.
            public Cell nextCell;

            // The previous state of the cell.
            public StructCellState prevCellState;

            // The current state of the cell.
            public StructCellState currentCellState;

            // The next state of the cell.
            public StructCellState nextCellState;

            // The number of viruses in the simulation.
            public static int nViruses;

            // The number of vaccines in the simulation.
            public static int nVaccines;

            // The GameObject associated with this cell.
            public object gameObject;

            // Constructor for the Cell class.
            public Cell(int maxCells, int probability)
            {
                // Initialize the neighbor array.
                neighbor = new Cell[MAX_NEIGHBORS];

                // Initialize the cell's state to be "organic" and "dead".
                currentCellState.eInfectedState = EInfectedState.organic;
                currentCellState.PEAliveState = EAliveState.dead;

                // If the random number generated is less than the probability, the cell should be alive.
                if (rand.Next(0, probability) == 0)
                {
                    currentCellState.PEAliveState = EAliveState.alive;

                    // If there are still viruses to be infected, there is a chance that this cell becomes infected.
                    if (nViruses < MAX_VIRUSES)
                    {
                        if (rand.Next(0, maxCells) < maxCells / MAX_VIRUSES)
                        {
                            currentCellState.eInfectedState = EInfectedState.infected;
                            ++nViruses;
                        }
                    }

                    // If the cell is not infected and there are still vaccines to be administered, there is a chance that this cell becomes vaccinated.
                    if (currentCellState.eInfectedState == EInfectedState.organic && nVaccines < MAX_VACCINES)
                    {
                        if (rand.Next(0, maxCells) < maxCells / MAX_VACCINES)
                        {
                            currentCellState.eInfectedState = EInfectedState.vaccinated;
                            ++nVaccines;
                        }
                    }
                }
            }




            // This method is responsible for setting the next state of the current cell based on its neighbors' states.
            // It counts the number of alive, infected, and vaccinated neighbors.
            // The for loop iterates through the neighboring cells of the current cell.
            // For each neighbor, it checks if it's not null and updates the count of alive, infected, and vaccinated cells accordingly.
            // The counts are used to set the next state of the current cell in the main simulation loop.
            public void SetNextState()
            {
                int nAlive = 0;
                int nInfected = 0;
                int nVaccinated = 0;

                for (int nCntr = 0; nCntr < MAX_NEIGHBORS; ++nCntr)
                {
                    Cell neighborCell = neighbor[nCntr];

                    if (neighborCell != null)
                    {
                        if (neighborCell.currentCellState.eInfectedState == EInfectedState.infected)
                        {
                            ++nInfected;
                        }

                        if (neighborCell.currentCellState.eInfectedState == EInfectedState.vaccinated)
                        {
                            ++nVaccinated;
                        }

                        if (neighborCell.currentCellState.PEAliveState == EAliveState.alive)
                        {
                            ++nAlive;
                        }
                    }
                }

                // default to current live and infected state
                nextCellState.eInfectedState = currentCellState.eInfectedState;
                nextCellState.PEAliveState = currentCellState.PEAliveState;

                // if less than 2 or more than 3 contiguous live cells
                if (nAlive < 2 || nAlive > 3)
                {
                    nextCellState.PEAliveState = EAliveState.dead;
                }
                else
                {
                    if (nAlive == 3)
                    {
                        nextCellState.PEAliveState = EAliveState.alive;
                    }
                }

                if (currentCellState.PEAliveState == EAliveState.alive &&
                    nextCellState.PEAliveState == EAliveState.alive)
                {
                    if (nInfected > 0 && nInfected >= nVaccinated &&
                        currentCellState.eInfectedState != EInfectedState.vaccinated)
                    {
                        nextCellState.eInfectedState = EInfectedState.infected;
                    }
                    else if (nVaccinated > 0)
                    {
                        nextCellState.eInfectedState = EInfectedState.vaccinated;
                    }
                }
            }
        }

        static void Main(string[] args)
        //public static void CreateOrganism( int probability = 6)
        {
            organism = new Cell[MAX_ROWS, MAX_COLS];

            string[] sInitialState = new string[MAX_ROWS];

            Console.WriteLine("Test");

            int nIniRows = 0;

            //Beacon
            sInitialState[0] = "11";
            sInitialState[1] = "1";
            sInitialState[2] = "   1";
            sInitialState[3] = "  11";
            nIniRows = 4;

            /*
            //Glider
            sInitialState[0] = "  1";
            sInitialState[1] = "1 1";
            sInitialState[2] = " 11";
            nIniRows = 3;
            */

            int probability;

            /*for (int row = 0; row < MAX_ROWS; ++row)
            {
                for (int col = 0; col < MAX_COLS; ++col)
                {
                    organism[row, col] = new Cell(MAX_ROWS * MAX_COLS, 6);
                }
            }
            */

            for (int row = 0; row < MAX_ROWS; ++row)
            {
                for (int col = 0; col < MAX_COLS; ++col)
                {
                    probability = 6;

                    if (row < nIniRows)
                    {
                        if (col < sInitialState[row].Length)
                        {
                            if (sInitialState[row][col] == '1')
                            {
                                probability = 0;
                                Console.WriteLine("1");
                            }
                            else
                            {
                                probability = 1;
                            }
                        }
                        else
                        {
                            probability = 1;
                        }
                    }
                    else
                    {
                        if (nIniRows > 0)
                        {
                            probability = 1;
                        }
                    }
                    organism[row, col] = new Cell(MAX_ROWS * MAX_COLS, probability);
                }
            }

            for (int row = 0; row < MAX_ROWS; ++row)
            {
                for (int col = 0; col < MAX_COLS; ++col)
                {
                    Cell nextCell = null;

                    for (int nCntr = 0; nCntr < Cell.MAX_NEIGHBORS; ++nCntr)
                    {
                        Cell neighborCell = null;

                        switch (nCntr)
                        {
                            case (int)EDirection.right:
                                // right neighbor
                                if (col < MAX_COLS - 1)
                                {
                                    neighborCell = organism[row, col + 1];
                                    nextCell = organism[row, col + 1];
                                }
                                else if (row < MAX_ROWS - 1)
                                {
                                    nextCell = organism[row + 1, 0];
                                }

                                break;

                            case (int)EDirection.down:
                                // bottom neighbor
                                if (row < MAX_ROWS - 1)
                                {
                                    neighborCell = organism[row + 1, col];
                                }
                                break;

                            case (int)EDirection.left:
                                // left neighbor
                                if (col > 0)
                                {
                                    neighborCell = organism[row, col - 1];
                                }
                                break;

                            case (int)EDirection.up:
                                // top neighbor
                                if (row > 0)
                                {
                                    neighborCell = organism[row - 1, col];
                                }
                                break;

                            case (int)EDirection.topleft:
                                // top-left neighbor
                                if (row > 0 && col > 0)
                                {
                                    neighborCell = organism[row - 1, col - 1];
                                }
                                break;

                            case (int)EDirection.topright:
                                // top-right neighbor
                                if (row > 0 && col < MAX_COLS - 1)
                                {
                                    neighborCell = organism[row - 1, col + 1];
                                }
                                break;

                            case (int)EDirection.bottomleft:
                                // bottom-left neighbor
                                if (row < MAX_ROWS - 1 && col > 0)
                                {
                                    neighborCell = organism[row + 1, col - 1];
                                }
                                break;

                            case (int)EDirection.bottomright:
                                // bottom-right neighbor
                                if (row < MAX_ROWS - 1 && col < MAX_COLS - 1)
                                {
                                    neighborCell = organism[row + 1, col + 1];
                                }
                                break;
                        }

                        organism[row, col].neighbor[nCntr] = neighborCell;

                    }

                    organism[row, col].nextCell = nextCell;
                }
            }

            // Set the console output encoding to Unicode so that it can display certain characters correctly
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Add a console event handler for the cancel key (CTRL+C)
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleCancel);

            // Loop while the game is not set to exit
            while (!bExit)
            {
                // Print the current state of the organism grid to the console
                PrintOrganism(organism, MAX_ROWS, MAX_COLS);

                
            // Calculate the next generation of cells in the organism grid
CalculateNextGeneration(organism[0, 0]);

                // Pause the thread for 100 milliseconds before continuing to the next iteration of the loop
                Thread.Sleep(100);
            }

            // Once the game is set to exit, print a message to the console and wait for the user to press Enter
            Console.WriteLine("\nPress Enter.");
            Console.ReadLine();
        }

        public static void ConsoleCancel(object sender, ConsoleCancelEventArgs e)
        {
            bExit = true; // Sets bExit to true to stop the main loop
        }

        public static void CalculateNextGeneration(Cell thisCell)
        {
            if (thisCell != null) // Check if current cell exists
            {
                thisCell.SetNextState(); // Calculate next state of the cell
                CalculateNextGeneration(thisCell.nextCell); // Recursive call on the next cell
                thisCell.currentCellState = thisCell.nextCellState; // Set current state to the next state
            }
        }


        // This method prints the current state of the organism on the console
        // It takes the 2D array of cells 'organism', the maximum number of rows 'maxRows', and the maximum number of columns 'maxCols' as parameters
        public static void PrintOrganism(Cell[,] organism, int maxRows, int maxCols)
        {
            // Initialize a string 'output' with a separator at the top
            string output = "----------------------------------------------------------------------------------\n";

           
        // Loop through all rows
for (int row = 0; row < maxRows; ++row)
            {
                // Add a separator at the beginning of each row
                output += "|";

                // Loop through all columns in the current row
                for (int col = 0; col < maxCols; ++col)
                {
                    // Get the current cell
                    Cell thisCell = organism[row, col];

                    // If the cell is alive, print its state based on its 'eInfectedState'
                    if (thisCell.currentCellState.PEAliveState == EAliveState.alive)
                    {
                        switch ((int)thisCell.currentCellState.eInfectedState)
                        {
                            case (int)EInfectedState.organic:
                                output += "\x25cb"; // print a white circle if the cell is organic
                                break;

                            case (int)EInfectedState.vaccinated:
                                output += "\x25ca"; // print a small white circle if the cell is vaccinated
                                break;

                            case (int)EInfectedState.infected:
                                output += "\x25cf"; // print a black circle if the cell is infected
                                break;
                        }
                    }
                    else
                    {
                        output += " "; // print an empty space if the cell is dead
                    }
                }

                // Add a separator at the end of each row
                output += "|\n";
            }

            // Add a separator at the bottom
            output += "----------------------------------------------------------------------------------";

            // Set the console background color to black, hide the cursor, and print the output string at the top left corner of the console
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Write(output);
        }
    }
}