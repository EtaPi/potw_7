// mp7 is a C# solution for the maze problem of the week, 
// possibly WORST CODE EVER! D/K 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mp7
{
    class Program
    {
        static void Main( string[] arguments )
        {
            var args        = ParseCommandLine( arguments );
            var innerWalls  = 0;

            if ( args.Rows == 1 )
            {
                innerWalls = args.Columns - 1;
            }
            else
            {
                innerWalls = ( 2 * args.Columns * args.Rows ) - args.Columns - args.Rows;
            }

            if ( innerWalls > 63 )
            {
                Console.WriteLine( "The requested maze would contain {0} inner walls, the data types for this crusty approach will overflow at >63 inner walls... but you shouldn't care as the mp only required a 4x4 block maze ;)", innerWalls );
                Console.WriteLine();

                Environment.Exit( -2 );
            }

            var combinations = (ulong)Math.Pow( 2, innerWalls );

            if ( combinations < 60 )
            {
                args.ShowProgressBar = false;
            }

            var maze = CreateMaze( args );

            if ( args.DisplayMode )
            {
                if ( args.MazeId > ( combinations - 1 ) )
                {
                    Console.WriteLine( "Invalid maze id specified for number of rows / columns." );
                    Console.WriteLine();

                    Environment.Exit( -1 );
                }

                BuildWalls( maze, args, args.MazeId );

                PrintMaze( maze, args );
            }
            else
            {
                var validIds = new List<ulong>();

                if ( args.ShowProgressBar )
                {
                    Console.WriteLine();
                    Console.Write( "[                                                            ]" );

                    Console.CursorLeft = 1;
                }

                var steps = combinations / 60;
                var bar = steps;

                for ( var i = 0UL; i < combinations; i++ )
                {
                    BuildWalls( maze, args, i );

                    if ( ValidateMaze( maze, args ) )
                    {
                        validIds.Add( i );
                    }

                    maze.Rollback();

                    if ( args.ShowProgressBar )
                    {
                        if ( i > bar )
                        {
                            bar += steps;
                            Console.Write( '.' );
                        }
                    }
                }

                if ( args.ShowProgressBar )
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }

                if ( args.ListMode )
                {
                    foreach ( var mazeId in validIds )
                    {
                        Console.WriteLine( mazeId );
                    }
                }
                else
                {
                    Console.WriteLine( "{0} valid wall patterns for a [{1},{2}] maze with {3} combinations.", validIds.Count, args.Rows, args.Columns, combinations );
                }
            }
        }

        private static bool ValidateMaze( MazeSegment[,] maze, CommandLineArgs args )
        {
            var visits = 0;
            var queue = new Queue<MazeSegment>();

            queue.Enqueue( maze[0, 0].Mark( -1 ) );

            while ( true )
            {
                if ( queue.Count == 0 )
                {
                    break;
                }

                var segment = queue.Dequeue();

                visits += 1;

                if ( segment.IsClosed )
                {
                    break;
                }

                if ( !segment.Top && maze[segment.X, segment.Y - 1].ID != segment.ParentID )
                {
                    if ( maze[segment.X, segment.Y - 1].Visited )
                    {
                        break;
                    }

                    queue.Enqueue( maze[segment.X, segment.Y - 1].Mark( segment.ID ) );
                }

                if ( !segment.Right && maze[segment.X + 1, segment.Y].ID != segment.ParentID )
                {
                    if ( maze[segment.X + 1, segment.Y].Visited )
                    {
                        break;
                    }

                    queue.Enqueue( maze[segment.X + 1, segment.Y].Mark( segment.ID ) );
                }

                if ( !segment.Bottom && maze[segment.X, segment.Y + 1].ID != segment.ParentID )
                {
                    if ( maze[segment.X, segment.Y + 1].Visited )
                    {
                        break;
                    }
                       
                    queue.Enqueue( maze[segment.X, segment.Y + 1].Mark( segment.ID ) );
                }

                if ( !segment.Left && maze[segment.X - 1, segment.Y].ID != segment.ParentID )
                {
                    if ( maze[segment.X - 1, segment.Y].Visited )
                    {
                        break;
                    }

                    queue.Enqueue( maze[segment.X - 1, segment.Y].Mark( segment.ID ) );
                }
            }

            return visits == ( args.Columns * args.Rows );
        }

        private static void PrintMaze( MazeSegment[,] maze, CommandLineArgs args )
        {
            Console.WriteLine();

            Console.Write( "   " );

            for ( var x = 0; x < args.Columns; x++ )
            {
                Console.Write( " _" );
            }

            Console.WriteLine();

            for ( var y = 0; y < args.Rows; y++ )
            {
                Console.Write( "   " );

                for ( var x = 0; x < args.Columns; x++ )
                {
                    var segment = maze[x, y];

                    if ( x == 0 )
                    {
                        Console.Write( '|' );
                    }

                    Console.Write( ( segment.Bottom ) ? '_' : ' ' );
                    Console.Write( ( segment.Right ) ? '|' : ' ' );
                }

                Console.WriteLine();
            }
        }

        private static void BuildWalls( MazeSegment[,] maze, CommandLineArgs args, ulong mazeId )
        {
            for ( var y = 0; y < args.Rows; y++ )
            {
                for ( var x = 0; x < args.Columns; x++ )
                {
                    var segment = maze[x, y];

                    for ( var i = 0; i < 4; i++ )
                    {
                        if ( segment.Factors[i] > 0 )
                        {
                            segment.Values[i] = ( mazeId & segment.Factors[i] ) == segment.Factors[i];
                        }
                        else
                        {
                            segment.Values[i] = true;
                        }
                    }
                }
            }
        }

        private static MazeSegment[,] CreateMaze( CommandLineArgs args )
        {
            var results = new MazeSegment[args.Columns, args.Rows];
            var id = 0;
            var factor = 1UL;

            for ( var y = 0; y < args.Rows; y++ )
            {
                for ( var x = 0; x < args.Columns; x++ )
                {
                    results[x, y] = new MazeSegment( ++id, x, y );

                    //Left
                    if ( x > 0 )
                    {
                        results[x, y].LeftFactor = results[x - 1, y].RightFactor;
                    }

                    //Top
                    if ( y > 0 )
                    {
                        results[x, y].TopFactor = results[x, y - 1].BottomFactor;
                    }

                    //Bottom
                    if ( y < args.Rows - 1 )
                    {
                        results[x, y].BottomFactor = factor;

                        factor *= 2;
                    }

                    //Right
                    if ( x < args.Columns - 1 )
                    {
                        results[x, y].RightFactor = factor;

                        factor *= 2;
                    }

                    results[x, y].Commit();
                }
            }

            return results;
        }

        private static CommandLineArgs ParseCommandLine( string[] args )
        {
            var results = new CommandLineArgs
            {
                Rows = 4,
                Columns = 4,
            };

            for ( var i = 0; i < args.Length; i++ )
            {
                switch ( args[i].ToLower() )
                {
                    case "-r":
                    case "-rows":
                        results.Rows = ProcessIntCommandLineArg( args, ++i );
                        break;

                    case "-c":
                    case "-cols":
                    case "-columns":
                        results.Columns = ProcessIntCommandLineArg( args, ++i );
                        break;

                    case "-d":
                    case "-disp":
                    case "-display":

                        results.DisplayMode = true;
                        results.MazeId = ProcessULongCommandLineArg( args, ++i );

                        break;

                    case "-p":
                    case "-prog":
                    case "-progress":
                    case "-progressbar":
                        results.ShowProgressBar = true;
                        break;

                    case "-l":
                    case "-list":
                        results.ListMode = true;
                        break;

                    default:
                        ExitShowUsage();
                        break;
                }
            }

            return results;
        }

        private static int ProcessIntCommandLineArg( string[] args, int i )
        {
            if ( i >= args.Length )
            {
                ExitShowUsage();
            }

            int results;

            if ( !int.TryParse( args[i], out results ) )
            {
                ExitShowUsage();
            }

            if ( results <= 0 )
            {
                ExitShowUsage();
            }

            return results;
        }

        private static ulong ProcessULongCommandLineArg( string[] args, int i )
        {
            if ( i >= args.Length )
            {
                ExitShowUsage();
            }

            ulong results;

            if ( !ulong.TryParse( args[i], out results ) )
            {
                ExitShowUsage();
            }

            if ( results < 0 )
            {
                ExitShowUsage();
            }

            return results;
        }

        private static void ExitShowUsage()
        {
            Console.WriteLine( "usage: mp7.exe [options]" );
            Console.WriteLine();
            Console.WriteLine( "  -r, -rows   Number of rows in maze, >=1 (default: 4)" );
            Console.WriteLine( "  -c, -cols   Number of columns in the maze, >=1 (default: 4 )" );
            Console.WriteLine( "  -d, -disp   Display the specified maze id, >=0 (default: 0 )" );
            Console.WriteLine( "  -p, -prog   Display a progress bar" );
            Console.WriteLine( "  -l, -list   List valid maze ids" );
            Console.WriteLine();

            Environment.Exit( -1 );
        }

        class CommandLineArgs
        {
            public int Rows
            { get; set; }

            public int Columns
            { get; set; }

            public ulong MazeId
            { get; set; }

            public bool DisplayMode
            { get; set; }

            public bool ShowProgressBar
            { get; set; }

            public bool ListMode
            { get; set; }
        }
    }

    [System.Diagnostics.DebuggerDisplay( "[{X},{Y}] - T:{Top}, R:{Right}, B:{Bottom}, L:{Left}]" )]
    public class MazeSegment
    {
        private struct Constants
        {
            public const int Top    = 0;
            public const int Right  = 1;
            public const int Bottom = 2;
            public const int Left   = 3;
        }

        public MazeSegment( int id, int x, int y )
        {
            CommittedFactors = new ulong[4];
            CommittedValues = new bool[4];
            Factors = new ulong[4];
            Values = new bool[4];

            X = x;
            Y = y;

            ID = id;
        }

        private ulong[] CommittedFactors
        { get; set; }

        private bool[] CommittedValues
        { get; set; }

        public ulong[] Factors
        { get; private set; }

        public bool[] Values
        { get; private set; }

        public ulong TopFactor
        {
            get { return Factors[Constants.Top]; }
            set { Factors[Constants.Top] = value; }
        }

        public bool Top
        {
            get { return Values[Constants.Top]; }
            set { Values[Constants.Top] = value; }
        }

        public ulong RightFactor
        {
            get { return Factors[Constants.Right]; }
            set { Factors[Constants.Right] = value; }
        }

        public bool Right
        {
            get { return Values[Constants.Right]; }
            set { Values[Constants.Right] = value; }
        }

        public ulong BottomFactor
        {
            get { return Factors[Constants.Bottom]; }
            set { Factors[Constants.Bottom] = value; }
        }

        public bool Bottom
        {
            get { return Values[Constants.Bottom]; }
            set { Values[Constants.Bottom] = value; }
        }

        public ulong LeftFactor
        {
            get { return Factors[Constants.Left]; }
            set { Factors[Constants.Left] = value; }
        }

        public bool Left
        {
            get { return Values[Constants.Left]; }
            set { Values[Constants.Left] = value; }
        }

        public bool IsClosed
        {
            get { return Top && Left && Right && Bottom; }
        }

        private bool CommittedVisited
        { get; set; }

        public bool Visited
        { get; set; }

        private int CommittedX
        { get; set; }

        private int CommittedY
        { get; set; }

        public int X
        { get; set; }

        public int Y
        { get; set; }

        public int ID
        { get; set; }

        public int ParentID
        { get; set; }

        private int CommittedID
        { get; set; }

        private int CommittedParentID
        { get; set; }

        public void Commit()
        {
            for ( var i = 0; i < 4; i++ )
            {
                CommittedFactors[i] = Factors[i];
                CommittedValues[i] = Values[i];
            }

            CommittedVisited = Visited;
            CommittedX = X;
            CommittedY = Y;
            CommittedID = ID;
            CommittedParentID = ParentID;
        }

        public void Rollback()
        {
            for ( var i = 0; i < 4; i++ )
            {
                Factors[i] = CommittedFactors[i];
                Values[i] = CommittedValues[i];
            }

            Visited = CommittedVisited;
            X = CommittedX;
            Y = CommittedY;
            ID = CommittedID;
            ParentID = CommittedParentID;
        }

        public void Clear()
        {
            for ( var i = 0; i < 4; i++ )
            {
                CommittedFactors[i] = 0;
                Factors[i] = 0;
                CommittedValues[i] = false;
                Values[i] = false;
            }

            Visited = CommittedVisited = false;
            X = CommittedX = -1;
            Y = CommittedY = -1;
            ID = CommittedID = -1;
            ParentID = CommittedParentID = -1;
        }

        public MazeSegment Mark(int parentID)
        {
            Visited     = true;
            ParentID    = parentID;

            return this;
        }
    }

    public static class MazeExtensions
    {
        public static void Commit( this MazeSegment[,] maze )
        {
            var rows = maze.GetLength( 1 );
            var cols = maze.GetLength( 0 );

            for ( var y = 0; y < rows; y++ )
            {
                for ( var x = 0; x < cols; x++ )
                {
                    maze[x, y].Commit();
                }
            }
        }

        public static void Rollback( this MazeSegment[,] maze )
        {
            var rows = maze.GetLength( 1 );
            var cols = maze.GetLength( 0 );

            for ( var y = 0; y < rows; y++ )
            {
                for ( var x = 0; x < cols; x++ )
                {
                    maze[x, y].Rollback();
                }
            }
        }
    }
}
