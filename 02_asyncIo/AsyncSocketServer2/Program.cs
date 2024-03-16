using System;
using System.Net;
using System.Collections.Generic;
using System.Text; //for testing

namespace AsyncSocketServer2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            AsycServer();
        }


        //If this is true, then info about which method the program is in
        //will print to log.               
        public static bool watchProgramFlow = true;

        //If you make this true, then connect/disconnect info will print to log.
        public static readonly bool watchConnectAndDisconnect = true;

        //If you make this true, then data will print to log.        
        public static readonly bool watchData = true;

        //If you make this true, then the IncomingDataPreparer will not write to
        // a List<T>, and you will not see the printout of the data at the end
        //of the log.
        public static readonly bool runLongTest = false;

        //If you make this true, then info about threads will print to log.
        public static readonly bool watchThreads = false;

        //If you make this true, then the above "watch-" variables will print to
        //both Console and log, instead of just to log. I suggest only using this if
        //you are having a problem with an application that is crashing.
        public static readonly bool consoleWatch = false;

        //This variable determines the number of 
        //SocketAsyncEventArg objects put in the pool of objects for receive/send.
        //The value of this variable also affects the Semaphore.
        //This app uses a Semaphore to ensure that the max # of connections
        //value does not get exceeded.
        //Max # of connections to a socket can be limited by the Windows Operating System
        //also.
        public const Int32 maxNumberOfConnections = 3000;

        //If this port # will not work for you, it's okay to change it.
        public const Int32 port = 4444;

        //You would want a buffer size larger than 25 probably, unless you know the
        //data will almost always be less than 25. It is just 25 in our test app.
        public const Int32 testBufferSize = 25;

        //This is the maximum number of asynchronous accept operations that can be 
        //posted simultaneously. This determines the size of the pool of 
        //SocketAsyncEventArgs objects that do accept operations. Note that this
        //is NOT the same as the maximum # of connections.
        public const Int32 maxSimultaneousAcceptOps = 10;

        //The size of the queue of incoming connections for the listen socket.
        public const Int32 backlog = 100;

        //For the BufferManager
        public const Int32 opsToPreAlloc = 2;    // 1 for receive, 1 for send

        //allows excess SAEA objects in pool.
        public const Int32 excessSaeaObjectsInPool = 1;

        //This number must be the same as the value on the client.
        //Tells what size the message prefix will be. Don't change this unless
        //you change the code, because 4 is the length of 32 bit integer, which
        //is what we are using as prefix.
        public const Int32 receivePrefixLength = 4;
        public const Int32 sendPrefixLength = 4;

        public static Int32 mainTransMissionId = 10000;
        public static Int32 startingTid; //
        public static Int32 mainSessionId = 1000000000;

        public static List<DataHolder> listOfDataHolders;

        //If you make this a positive value, it will simulate some delay on the
        //receive/send SAEA object after doing a receive operation.
        //That would be where you would do some work on the received data, 
        //before responding to the client.
        //This is in milliseconds. So a value of 1000 = 1 second delay.
        public static readonly Int32 msDelayAfterGettingMessage = -1;

        //This is for logging during testing.        
        //You can change the path in the TestFileWriter class if you need to.
        public static TestFileWriter testWriter;

        // To keep a record of maximum number of simultaneous connections
        // that occur while the server is running. This can be limited by operating
        // system and hardware. It will not be higher than the value that you set
        // for maxNumberOfConnections.
        public static Int32 maxSimultaneousClientsThatWereConnected = 0;

        //These strings are just for console interaction.
        public const string checkString = "C";
        public const string closeString = "Z";
        public const string wpf = "T";
        public const string wpfNo = "F";
        public static string wpfTrueString = "";
        public static string wpfFalseString = "";

        static void AsycServer()
        {
            // Just used to calculate # of received transmissions at the end.
            startingTid = mainTransMissionId;

            // Before the app starts, let's build strings for you to use the console.
            BuildStringsForServerConsole();

            // Create List<T> to hold data, unless we are running a long test, which
            // would create too much data to store in a list.
            if (runLongTest == false)
            {
                listOfDataHolders = new List<DataHolder>();
            }

            //Create a log file writer, so you can see the flow easily.
            //It can be printed. Makes it easier to figure out complex program flow.
            //The log StreamWriter uses a buffer. So it will only work right if you close
            //the server console properly at the end of the test.
            testWriter = new TestFileWriter();

            try
            {
                // Get endpoint for the listener.                
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

                WriteInfoToConsole(localEndPoint);

                //This object holds a lot of settings that we pass from Main method
                //to the SocketListener. In a real app, you might want to read
                //these settings from a database or windows registry settings that
                //you would create.
                SocketListenerSettings theSocketListenerSettings = new SocketListenerSettings
        (maxNumberOfConnections, excessSaeaObjectsInPool, backlog, maxSimultaneousAcceptOps, receivePrefixLength, testBufferSize, sendPrefixLength, opsToPreAlloc, localEndPoint);

                //instantiate the SocketListener.
                SocketListener socketListener = new SocketListener(theSocketListenerSettings);

                ManageClosing(socketListener);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // close the stream for test file writing
                try
                {
                    testWriter.Close();
                }
                catch
                {
                    Console.WriteLine("Could not close log properly.");
                }
            }
        }

        //____________________________________________________________________________
        static void BuildStringsForServerConsole()
        {
            StringBuilder sb = new StringBuilder();

            // Make the string to write.
            sb.Append("\r\n");
            sb.Append("\r\n");
            sb.Append("To take any of the following actions type the \r\ncorresponding letter below and press Enter.\r\n");
            sb.Append(closeString);
            sb.Append(")  to close the program\r\n");
            sb.Append(checkString);
            sb.Append(")  to check current status\r\n");
            string tempString = sb.ToString();
            sb.Length = 0;

            // string when watchProgramFlow == true 
            sb.Append(wpfNo);
            sb.Append(")  to quit writing program flow. (ProgramFlow is being logged now.)\r\n");
            wpfTrueString = tempString + sb.ToString();
            sb.Length = 0;

            // string when watchProgramFlow == false
            sb.Append(wpf);
            sb.Append(")  to start writing program flow. (ProgramFlow is NOT being logged.)\r\n");
            wpfFalseString = tempString + sb.ToString();
        }


        //____________________________________________________________________________
        public static void WriteInfoToConsole(IPEndPoint localEndPoint)
        {
            Console.WriteLine("The following options can be changed in Program.cs file.");
            Console.WriteLine("server buffer size = " + testBufferSize);
            Console.WriteLine("max connections = " + maxNumberOfConnections);
            Console.WriteLine("backlog variable value = " + backlog);
            Console.WriteLine("watchProgramFlow = " + watchProgramFlow);
            Console.WriteLine("watchConnectAndDisconnect = " + watchConnectAndDisconnect);
            Console.WriteLine("watchThreads = " + watchThreads);
            Console.WriteLine("msDelayAfterGettingMessage = " + msDelayAfterGettingMessage);
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("local endpoint = " + IPAddress.Parse(((IPEndPoint)localEndPoint).Address.ToString()) + ": " + ((IPEndPoint)localEndPoint).Port.ToString());
            Console.WriteLine("server machine name = " + Environment.MachineName);

            Console.WriteLine();
            Console.WriteLine("Client and server should be on separate machines for best results.");
            Console.WriteLine("And your firewalls on both client and server will need to allow the connection.");
            Console.WriteLine();
        }


        //____________________________________________________________________________
        static void ManageClosing(SocketListener socketListener)
        {
            string stringToCompare = "";
            string theEntry = "";

            while (stringToCompare != closeString)
            {
                if (watchProgramFlow == true)
                {
                    Console.WriteLine(wpfTrueString);
                }
                else
                {
                    Console.WriteLine(wpfFalseString);
                }

                theEntry = Console.ReadLine().ToUpper();

                switch (theEntry)
                {
                    case checkString:
                        Console.WriteLine("Number of current accepted connections = " + socketListener.numberOfAcceptedSockets);
                        break;
                    case wpf:
                        if (Program.watchProgramFlow == false)
                        {
                            Program.watchProgramFlow = true;
                            Console.WriteLine("Changed watchProgramFlow to true.");
                            Program.testWriter.WriteLine("\r\nStart logging program flow.\r\n");
                        }
                        else
                        {
                            Console.WriteLine("Program flow was already being logged.");
                        }

                        break;
                    case wpfNo:
                        if (Program.watchProgramFlow == true)
                        {
                            Program.watchProgramFlow = false;
                            Console.WriteLine("Changed watchProgramFlow to false.");
                            Program.testWriter.WriteLine("\r\nStopped logging program flow.\r\n");
                        }
                        else
                        {
                            Console.WriteLine("Program flow was already NOT being logged.");
                        }
                        break;
                    case closeString:
                        stringToCompare = closeString;
                        break;
                    default:
                        Console.WriteLine("Unrecognized entry");
                        break;
                }
            }
            WriteData();
        }

        //____________________________________________________________________________
        public static void WriteData()
        {
            if ((watchData == true) & (runLongTest == false))
            {
                DataHolder theDataHolder;
                Program.testWriter.WriteLine("\r\n\r\nData from DataHolders in listOfDataHolders follows:\r\n");
                int listCount = listOfDataHolders.Count;
                for (int i = 0; i < listCount; i++)
                {
                    theDataHolder = listOfDataHolders[i];
                    Program.testWriter.WriteLine(IPAddress.Parse(((IPEndPoint)theDataHolder.remoteEndpoint).Address.ToString()) + ": " + ((IPEndPoint)theDataHolder.remoteEndpoint).Port.ToString() + ", " + theDataHolder.receivedTransMissionId + ", " + Encoding.ASCII.GetString(theDataHolder.dataMessageReceived));
                }
            }
            testWriter.WriteLine("\r\nHighest # of simultaneous connections was " + maxSimultaneousClientsThatWereConnected);
            testWriter.WriteLine("# of transmissions received was " + (mainTransMissionId - startingTid));
        }
    }
}
