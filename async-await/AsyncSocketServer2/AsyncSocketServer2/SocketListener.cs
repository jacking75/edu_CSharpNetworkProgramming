using System;
using System.IO;
using System.Collections.Generic; //for testing
using System.Net.Sockets;
using System.Threading; //for Semaphore and Interlocked
using System.Net;
using System.Text; //for testing
using System.Diagnostics; //for testing



namespace AsyncSocketServer2
{   //____________________________________________________________________________
    // Implements the logic for the socket server.  
   
    class SocketListener
    {
        //__variables for testing ____________________________________________

        //total clients connected to the server, excluding backlog
        internal Int32 numberOfAcceptedSockets;    
                
        //****for testing threads
        Process theProcess; //for testing only
        ProcessThreadCollection arrayOfLiveThreadsInThisProcess;   //for testing
        HashSet<int> managedThreadIds = new HashSet<int>();  //for testing
        HashSet<Thread> managedThreads = new HashSet<Thread>();  //for testing        
        //object that will be used to lock the HashSet of thread references 
        //that we use for testing.
        private object lockerForThreadHashSet = new object();
        //****end variables for displaying what's happening with threads        
        //__END variables for testing ____________________________________________

        //__variables that might be used in a  real app__________________________________
        
        //Buffers for sockets are unmanaged by .NET. 
        //So memory used for buffers gets "pinned", which makes the
        //.NET garbage collector work around it, fragmenting the memory. 
        //Circumvent this problem by putting all buffers together 
        //in one block in memory. Then we will assign a part of that space 
        //to each SocketAsyncEventArgs object, and
        //reuse that buffer space each time we reuse the SocketAsyncEventArgs object.
        //Create a large reusable set of buffers for all socket operations.
        BufferManager theBufferManager;

        // the socket used to listen for incoming connection requests
        Socket listenSocket; 

        //A Semaphore has two parameters, the initial number of available slots
        // and the maximum number of slots. We'll make them the same. 
        //This Semaphore is used to keep from going over max connection #. (It is not about 
        //controlling threading really here.)   
        Semaphore theMaxConnectionsEnforcer;
                
        SocketListenerSettings socketListenerSettings;

        PrefixHandler prefixHandler;
        MessageHandler messageHandler;
        
        // pool of reusable SocketAsyncEventArgs objects for accept operations
        SocketAsyncEventArgsPool poolOfAcceptEventArgs;
        // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations
        SocketAsyncEventArgsPool poolOfRecSendEventArgs;
        //__END variables for real app____________________________________________
        
        //_______________________________________________________________________________
        // Constructor.
        public SocketListener(SocketListenerSettings theSocketListenerSettings)        
        {
            
            if (Program.watchProgramFlow == true)   //for testing
            {                
                Program.testWriter.WriteLine("SocketListener constructor");
            }            
            if (Program.watchThreads == true)   //for testing
            {                
                theProcess = Process.GetCurrentProcess(); //for testing only             
                DealWithThreadsForTesting("constructor");
            }
            
                        
            this.numberOfAcceptedSockets = 0; //for testing
            this.socketListenerSettings = theSocketListenerSettings;
            this.prefixHandler = new PrefixHandler();
            this.messageHandler = new MessageHandler();
                        
            //Allocate memory for buffers. We are using a separate buffer space for
            //receive and send, instead of sharing the buffer space, like the Microsoft
            //example does.            
            this.theBufferManager = new BufferManager(this.socketListenerSettings.BufferSize * this.socketListenerSettings.NumberOfSaeaForRecSend * this.socketListenerSettings.OpsToPreAllocate,
            this.socketListenerSettings.BufferSize * this.socketListenerSettings.OpsToPreAllocate);

            this.poolOfRecSendEventArgs = new SocketAsyncEventArgsPool(this.socketListenerSettings.NumberOfSaeaForRecSend);
            this.poolOfAcceptEventArgs = new SocketAsyncEventArgsPool(this.socketListenerSettings.MaxAcceptOps);     
            
            // Create connections count enforcer
            this.theMaxConnectionsEnforcer = new Semaphore(this.socketListenerSettings.MaxConnections, this.socketListenerSettings.MaxConnections);

            //Microsoft's example called these from Main method, which you 
            //can easily do if you wish.
            Init();
            StartListen();
        }

        //____________________________________________________________________________
        // initializes the server by preallocating reusable buffers and 
        // context objects (SocketAsyncEventArgs objects).  
        //It is NOT mandatory that you preallocate them or reuse them. But, but it is 
        //done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        
        internal void Init()
        {
            if (Program.watchProgramFlow == true)   //for testing
            {                
                Program.testWriter.WriteLine("Init method");
            }            
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("Init()");                
            }            
            
            // Allocate one large byte buffer block, which all I/O operations will 
            //use a piece of. This gaurds against memory fragmentation.
            this.theBufferManager.InitBuffer();

            if (Program.watchProgramFlow == true)   //for testing
            {                
                Program.testWriter.WriteLine("Starting creation of accept SocketAsyncEventArgs pool:");
            }

            // preallocate pool of SocketAsyncEventArgs objects for accept operations           
            for (Int32 i = 0; i < this.socketListenerSettings.MaxAcceptOps; i++)
            {
                // add SocketAsyncEventArg to the pool
                this.poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept(poolOfAcceptEventArgs));
            }           

            //The pool that we built ABOVE is for SocketAsyncEventArgs objects that do
            // accept operations. 
            //Now we will build a separate pool for SAEAs objects 
            //that do receive/send operations. One reason to separate them is that accept
            //operations do NOT need a buffer, but receive/send operations do. 
            //ReceiveAsync and SendAsync require
            //a parameter for buffer size in SocketAsyncEventArgs.Buffer.
            // So, create pool of SAEA objects for receive/send operations.
            SocketAsyncEventArgs eventArgObjectForPool;

            if (Program.watchProgramFlow == true)   //for testing
            {                
                Program.testWriter.WriteLine("Starting creation of receive/send SocketAsyncEventArgs pool");
            }

            Int32 tokenId;

            for (Int32 i = 0; i < this.socketListenerSettings.NumberOfSaeaForRecSend; i++)
            {
                //Allocate the SocketAsyncEventArgs object for this loop, 
                //to go in its place in the stack which will be the pool
                //for receive/send operation context objects.
                eventArgObjectForPool = new SocketAsyncEventArgs();

                // assign a byte buffer from the buffer block to 
                //this particular SocketAsyncEventArg object
                this.theBufferManager.SetBuffer(eventArgObjectForPool);

                tokenId = poolOfRecSendEventArgs.AssignTokenId() + 1000000;

                //Attach the SocketAsyncEventArgs object
                //to its event handler. Since this SocketAsyncEventArgs object is 
                //used for both receive and send operations, whenever either of those 
                //completes, the IO_Completed method will be called.
                eventArgObjectForPool.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                
                //We can store data in the UserToken property of SAEA object.
                DataHoldingUserToken theTempReceiveSendUserToken = new DataHoldingUserToken(eventArgObjectForPool, eventArgObjectForPool.Offset, eventArgObjectForPool.Offset + this.socketListenerSettings.BufferSize, this.socketListenerSettings.ReceivePrefixLength, this.socketListenerSettings.SendPrefixLength, tokenId);

                //We'll have an object that we call DataHolder, that we can remove from
                //the UserToken when we are finished with it. So, we can hang on to the
                //DataHolder, pass it to an app, serialize it, or whatever.
                theTempReceiveSendUserToken.CreateNewDataHolder();
                                
                eventArgObjectForPool.UserToken = theTempReceiveSendUserToken;

                // add this SocketAsyncEventArg object to the pool.
                this.poolOfRecSendEventArgs.Push(eventArgObjectForPool);
            }
        }

        //____________________________________________________________________________
        // This method is called when we need to create a new SAEA object to do
        //accept operations. The reason to put it in a separate method is so that
        //we can easily add more objects to the pool if we need to.
        //You can do that if you do NOT use a buffer in the SAEA object that does
        //the accept operations.
        internal SocketAsyncEventArgs CreateNewSaeaForAccept(SocketAsyncEventArgsPool pool)
        {
            //Allocate the SocketAsyncEventArgs object. 
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();

            //SocketAsyncEventArgs.Completed is an event, (the only event,) 
            //declared in the SocketAsyncEventArgs class.
            //See http://msdn.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.completed.aspx.
            //An event handler should be attached to the event within 
            //a SocketAsyncEventArgs instance when an asynchronous socket 
            //operation is initiated, otherwise the application will not be able 
            //to determine when the operation completes.
            //Attach the event handler, which causes the calling of the 
            //AcceptEventArg_Completed object when the accept op completes.
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

            AcceptOpUserToken theAcceptOpToken = new AcceptOpUserToken(pool.AssignTokenId() + 10000);
            acceptEventArg.UserToken = theAcceptOpToken;

            return acceptEventArg;

            // accept operations do NOT need a buffer.                
            //You can see that is true by looking at the
            //methods in the .NET Socket class on the Microsoft website. AcceptAsync does
            //not take require a parameter for buffer size.
        }

        //____________________________________________________________________________
        // This method starts the socket server such that it is listening for 
        // incoming connection requests.            
        internal void StartListen()
        {
            if (Program.watchProgramFlow == true)   //for testing
            {                   
                Program.testWriter.WriteLine("StartListen method. Before Listen operation is started.");
            }            
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("StartListen()");
            }
            
            // create the socket which listens for incoming connections
            listenSocket = new Socket(this.socketListenerSettings.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //bind it to the port
            listenSocket.Bind(this.socketListenerSettings.LocalEndPoint);

            // Start the listener with a backlog of however many connections.
            //"backlog" means pending connections. 
            //The backlog number is the number of clients that can wait for a
            //SocketAsyncEventArg object that will do an accept operation.
            //The listening socket keeps the backlog as a queue. The backlog allows 
            //for a certain # of excess clients waiting to be connected.
            //If the backlog is maxed out, then the client will receive an error when
            //trying to connect.
            //max # for backlog can be limited by the operating system.
            listenSocket.Listen(this.socketListenerSettings.Backlog);
            
            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("StartListen method Listen operation was just started.");
            }
            Console.WriteLine("\r\n\r\n*************************\r\n** Server is listening **\r\n*************************\r\n\r\nAfter you are finished, type 'Z' and press\r\nEnter key to terminate the server process.\r\nIf you terminate it by clicking X on the Console,\r\nthen the log will NOT write correctly.\r\n");

            // Calls the method which will post accepts on the listening socket.            
            // This call just occurs one time from this StartListen method. 
            // After that the StartAccept method will be called in a loop.
            StartAccept();
        }

        //____________________________________________________________________________
        // Begins an operation to accept a connection request from the client         
        internal void StartAccept()        
        {
            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("StartAccept method");
            }
            SocketAsyncEventArgs acceptEventArg;
                        
            //Get a SocketAsyncEventArgs object to accept the connection.                        
            //Get it from the pool if there is more than one in the pool.
            //We could use zero as bottom, but one is a little safer.            
            if (this.poolOfAcceptEventArgs.Count > 1)
            {
                try
                {
                    acceptEventArg = this.poolOfAcceptEventArgs.Pop();
                }
                //or make a new one.
                catch
                {
                    acceptEventArg = CreateNewSaeaForAccept(poolOfAcceptEventArgs);
                }
            }
            //or make a new one.
            else
            {
                acceptEventArg = CreateNewSaeaForAccept(poolOfAcceptEventArgs);
            }
        
            

            if (Program.watchThreads == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArg.UserToken;
                DealWithThreadsForTesting("StartAccept()", theAcceptOpToken);
            }
            if (Program.watchProgramFlow == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArg.UserToken;
                Program.testWriter.WriteLine("still in StartAccept, id = " + theAcceptOpToken.TokenId);
            }

            //Semaphore class is used to control access to a resource or pool of 
            //resources. Enter the semaphore by calling the WaitOne method, which is 
            //inherited from the WaitHandle class, and release the semaphore 
            //by calling the Release method. This is a mechanism to prevent exceeding
            // the max # of connections we specified. We'll do this before
            // doing AcceptAsync. If maxConnections value has been reached,
            //then the application will pause here until the Semaphore gets released,
            //which happens in the CloseClientSocket method.            
            this.theMaxConnectionsEnforcer.WaitOne();
            
            //Socket.AcceptAsync begins asynchronous operation to accept the connection.
            //Note the listening socket will pass info to the SocketAsyncEventArgs
            //object that has the Socket that does the accept operation.
            //If you do not create a Socket object and put it in the SAEA object
            //before calling AcceptAsync and use the AcceptSocket property to get it,
            //then a new Socket object will be created for you by .NET.            
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            //Socket.AcceptAsync returns true if the I/O operation is pending, i.e. is 
            //working asynchronously. The 
            //SocketAsyncEventArgs.Completed event on the acceptEventArg parameter 
            //will be raised upon completion of accept op.
            //AcceptAsync will call the AcceptEventArg_Completed
            //method when it completes, because when we created this SocketAsyncEventArgs
            //object before putting it in the pool, we set the event handler to do it.
            //AcceptAsync returns false if the I/O operation completed synchronously.            
            //The SocketAsyncEventArgs.Completed event on the acceptEventArg 
            //parameter will NOT be raised when AcceptAsync returns false.
            if (!willRaiseEvent)
            {                
                if (Program.watchProgramFlow == true)   //for testing
                {
                    AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArg.UserToken;
                
                    Program.testWriter.WriteLine("StartAccept in if (!willRaiseEvent), accept token id " + theAcceptOpToken.TokenId);
                }
                
                //The code in this if (!willRaiseEvent) statement only runs 
                //when the operation was completed synchronously. It is needed because 
                //when Socket.AcceptAsync returns false, 
                //it does NOT raise the SocketAsyncEventArgs.Completed event.
                //And we need to call ProcessAccept and pass it the SAEA object.
                //This is only when a new connection is being accepted.
                // Probably only relevant in the case of a socket error.
                ProcessAccept(acceptEventArg);
            }                        
        }
        
        //____________________________________________________________________________
        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an async accept operation completes.
        // This is only when a new connection is being accepted.
        // Notice that Socket.AcceptAsync is returning a value of true, and
        // raising the Completed event when the AcceptAsync method completes.
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Any code that you put in this method will NOT be called if
            //the operation completes synchronously, which will probably happen when
            //there is some kind of socket error. It might be better to put the code
            //in the ProcessAccept method.
            
            if (Program.watchProgramFlow == true)   //for testing
            {

                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)e.UserToken;
                Program.testWriter.WriteLine("AcceptEventArg_Completed, id " + theAcceptOpToken.TokenId);
            }
            
            if (Program.watchThreads == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)e.UserToken;
                DealWithThreadsForTesting("AcceptEventArg_Completed()", theAcceptOpToken);
            }
            
            ProcessAccept(e);
        }

        //____________________________________________________________________________       
        //The e parameter passed from the AcceptEventArg_Completed method
        //represents the SocketAsyncEventArgs object that did
        //the accept operation. in this method we'll do the handoff from it to the 
        //SocketAsyncEventArgs object that will do receive/send.
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            // This is when there was an error with the accept op. That should NOT
            // be happening often. It could indicate that there is a problem with
            // that socket. If there is a problem, then we would have an infinite
            // loop here, if we tried to reuse that same socket.
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                // Loop back to post another accept op. Notice that we are NOT
                // passing the SAEA object here.
                LoopToStartAccept();
                
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                Program.testWriter.WriteLine("SocketError, accept id " + theAcceptOpToken.TokenId);

                //Let's destroy this socket, since it could be bad.
                HandleBadAccept(acceptEventArgs);
                
                //Jump out of the method.
                return;
            }
            
            Int32 max = Program.maxSimultaneousClientsThatWereConnected;
            Int32 numberOfConnectedSockets = Interlocked.Increment(ref this.numberOfAcceptedSockets);
            if (numberOfConnectedSockets > max)
            {
                Interlocked.Increment(ref Program.maxSimultaneousClientsThatWereConnected);
            }

            if (Program.watchProgramFlow == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                Program.testWriter.WriteLine("ProcessAccept, accept id " + theAcceptOpToken.TokenId);
            }
            
            

            //Now that the accept operation completed, we can start another
            //accept operation, which will do the same. Notice that we are NOT
            //passing the SAEA object here.
            LoopToStartAccept();

            // Get a SocketAsyncEventArgs object from the pool of receive/send op 
            //SocketAsyncEventArgs objects
            SocketAsyncEventArgs receiveSendEventArgs = this.poolOfRecSendEventArgs.Pop();

            //Create sessionId in UserToken.
            ((DataHoldingUserToken)receiveSendEventArgs.UserToken).CreateSessionId();
                        
            //A new socket was created by the AcceptAsync method. The 
            //SocketAsyncEventArgs object which did the accept operation has that 
            //socket info in its AcceptSocket property. Now we will give
            //a reference for that socket to the SocketAsyncEventArgs 
            //object which will do receive/send.
            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                        
            if ((Program.watchProgramFlow == true) || (Program.watchConnectAndDisconnect == true))
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                Program.testWriter.WriteLine("Accept id " + theAcceptOpToken.TokenId + ". RecSend id " + ((DataHoldingUserToken)receiveSendEventArgs.UserToken).TokenId + ".  Remote endpoint = " + IPAddress.Parse(((IPEndPoint)receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString()) + ": " + ((IPEndPoint)receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Port.ToString() + ". client(s) connected = " + this.numberOfAcceptedSockets);
            }
            if (Program.watchThreads == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                theAcceptOpToken.socketHandleNumber = (Int32)acceptEventArgs.AcceptSocket.Handle;
                DealWithThreadsForTesting("ProcessAccept()", theAcceptOpToken);
                ((DataHoldingUserToken)receiveSendEventArgs.UserToken).socketHandleNumber = (Int32)receiveSendEventArgs.AcceptSocket.Handle;
            }
            
            //We have handed off the connection info from the
            //accepting socket to the receiving socket. So, now we can
            //put the SocketAsyncEventArgs object that did the accept operation 
            //back in the pool for them. But first we will clear 
            //the socket info from that object, so it will be 
            //ready for a new socket when it comes out of the pool.
            acceptEventArgs.AcceptSocket = null;
            this.poolOfAcceptEventArgs.Push(acceptEventArgs);            

            if (Program.watchProgramFlow == true)   //for testing
            {
                AcceptOpUserToken theAcceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                Program.testWriter.WriteLine("back to poolOfAcceptEventArgs goes accept id " + theAcceptOpToken.TokenId);
            }

            StartReceive(receiveSendEventArgs);
        }

        //____________________________________________________________________________
        //LoopToStartAccept method just sends us back to the beginning of the 
        //StartAccept method, to start the next accept operation on the next 
        //connection request that this listening socket will pass of to an 
        //accepting socket. We do NOT actually need this method. You could
        //just call StartAccept() in ProcessAccept() where we called LoopToStartAccept().
        //This method is just here to help you visualize the program flow.
        private void LoopToStartAccept()
        {
            if (Program.watchProgramFlow == true)   //for testing
            {                                
                Program.testWriter.WriteLine("LoopToStartAccept");
            }
                        
            StartAccept();
        }
                

        //____________________________________________________________________________
        // Set the receive buffer and post a receive op.
        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            if (Program.watchProgramFlow == true)   //for testing
            {                
                Program.testWriter.WriteLine("StartReceive(), receiveSendToken id " + receiveSendToken.TokenId);
            }

            //Set the buffer for the receive operation.
            receiveSendEventArgs.SetBuffer(receiveSendToken.bufferOffsetReceive, this.socketListenerSettings.BufferSize);                    

            // Post async receive operation on the socket.
            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

            //Socket.ReceiveAsync returns true if the I/O operation is pending. The 
            //SocketAsyncEventArgs.Completed event on the e parameter will be raised 
            //upon completion of the operation. So, true will cause the IO_Completed
            //method to be called when the receive operation completes. 
            //That's because of the event handler we created when building
            //the pool of SocketAsyncEventArgs objects that perform receive/send.
            //It was the line that said
            //eventArgObjectForPool.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            //Socket.ReceiveAsync returns false if I/O operation completed synchronously. 
            //In that case, the SocketAsyncEventArgs.Completed event on the e parameter 
            //will not be raised and the e object passed as a parameter may be 
            //examined immediately after the method call 
            //returns to retrieve the result of the operation.
            // It may be false in the case of a socket error.
            if (!willRaiseEvent)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {                    
                    Program.testWriter.WriteLine("StartReceive in if (!willRaiseEvent), receiveSendToken id " + receiveSendToken.TokenId);
                }
                //If the op completed synchronously, we need to call ProcessReceive 
                //method directly. This will probably be used rarely, as you will 
                //see in testing.
                ProcessReceive(receiveSendEventArgs);                
            }            
        }

        //____________________________________________________________________________
        // This method is called whenever a receive or send operation completes.
        // Here "e" represents the SocketAsyncEventArgs object associated 
        //with the completed receive or send operation
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Any code that you put in this method will NOT be called if
            //the operation completes synchronously, which will probably happen when
            //there is some kind of socket error.

            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)e.UserToken;
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("IO_Completed()", receiveSendToken);                
            }
            
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (Program.watchProgramFlow == true)   //for testing
                    {
                        Program.testWriter.WriteLine("IO_Completed method in Receive, receiveSendToken id " + receiveSendToken.TokenId);
                    }                    
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    if (Program.watchProgramFlow == true)   //for testing
                    {
                        Program.testWriter.WriteLine("IO_Completed method in Send, id " + receiveSendToken.TokenId);
                   }

                    ProcessSend(e);
                    break;

                default:
                    //This exception will occur if you code the Completed event of some
                    //operation to come to this method, by mistake.
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        //____________________________________________________________________________
        // This method is invoked by the IO_Completed method
        // when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.
        // Otherwise, we process the received data. And if a complete message was
        // received, then we do some additional processing, to 
        // respond to the client.
        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            // If there was a socket error, close the connection. This is NOT a normal
            // situation, if you get an error here.
            // In the Microsoft example code they had this error situation handled
            // at the end of ProcessReceive. Putting it here improves readability
            // by reducing nesting some.
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("ProcessReceive ERROR, receiveSendToken id " + receiveSendToken.TokenId);
                }

                receiveSendToken.Reset();
                CloseClientSocket(receiveSendEventArgs);

                //Jump out of the ProcessReceive method.
                return;
            }

            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("ProcessReceive NO DATA, receiveSendToken id " + 
    receiveSendToken.TokenId);
                }

                receiveSendToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return;
            }

            //The BytesTransferred property tells us how many bytes 
            //we need to process.
            Int32 remainingBytesToProcess = receiveSendEventArgs.BytesTransferred;

            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("ProcessReceive " + receiveSendToken.TokenId + ". remainingBytesToProcess = " + remainingBytesToProcess);
            }
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("ProcessReceive()", receiveSendToken);
            }            

            //If we have not got all of the prefix already, 
            //then we need to work on it here.                                
            if (receiveSendToken.receivedPrefixBytesDoneCount < this.socketListenerSettings.ReceivePrefixLength)
            {
                remainingBytesToProcess = prefixHandler.HandlePrefix(receiveSendEventArgs, receiveSendToken, remainingBytesToProcess);

                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("ProcessReceive, after prefix work " + receiveSendToken.TokenId + ". remainingBytesToProcess = " + remainingBytesToProcess);
                }
                                
                if (remainingBytesToProcess == 0)
                {                    
                    // We need to do another receive op, since we do not have
                    // the message yet, but remainingBytesToProcess == 0.
                    StartReceive(receiveSendEventArgs);
                    //Jump out of the method.
                    return;
                }
            }

            // If we have processed the prefix, we can work on the message now.
            // We'll arrive here when we have received enough bytes to read
            // the first byte after the prefix.
            bool incomingTcpMessageIsReady = messageHandler.HandleMessage(receiveSendEventArgs, receiveSendToken, remainingBytesToProcess);
            
            if (incomingTcpMessageIsReady == true)
            {
                if (Program.watchData == true)
                {
                    Program.testWriter.WriteLine(receiveSendToken.TokenId + ", Message in DataHolder = " + Encoding.ASCII.GetString(receiveSendToken.theDataHolder.dataMessageReceived) + "\r\n");
                }
                //for testing only
                if (Program.msDelayAfterGettingMessage > -1)
                {
                    //A Thread.Sleep here can be used to simulate delaying the 
                    //return of the SocketAsyncEventArgs object for receive/send
                    //to the pool. Simulates doing some work here.
                    if (Program.watchData == true)
                    {
                        Program.testWriter.WriteLine(receiveSendToken.TokenId + " waiting after read.\r\n");
                    }
                    Thread.Sleep(Program.msDelayAfterGettingMessage);
                }

                // Pass the DataHolder object to the Mediator here. The data in
                // this DataHolder can be used for all kinds of things that an
                // intelligent and creative person like you might think of.                        
                receiveSendToken.theMediator.HandleData(receiveSendToken.theDataHolder);

                // Create a new DataHolder for next message.
                receiveSendToken.CreateNewDataHolder();
                
                //Reset the variables in the UserToken, to be ready for the
                //next message that will be received on the socket in this
                //SAEA object.
                receiveSendToken.Reset();

                receiveSendToken.theMediator.PrepareOutgoingData();
                StartSend(receiveSendToken.theMediator.GiveBack());
            }
            else
            {
                // Since we have NOT gotten enough bytes for the whole message,
                // we need to do another receive op. Reset some variables first.

                // All of the data that we receive in the next receive op will be
                // message. None of it will be prefix. So, we need to move the 
                // receiveSendToken.receiveMessageOffset to the beginning of the 
                // receive buffer space for this SAEA.
                receiveSendToken.receiveMessageOffset = receiveSendToken.bufferOffsetReceive;

                // Do NOT reset receiveSendToken.receivedPrefixBytesDoneCount here.
                // Just reset recPrefixBytesDoneThisOp.
                receiveSendToken.recPrefixBytesDoneThisOp = 0;
                                
                StartReceive(receiveSendEventArgs);
            }            
        }

        //____________________________________________________________________________
        //Post a send.    
        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;

            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("StartSend, id " + receiveSendToken.TokenId);
            }
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("StartSend()", receiveSendToken);
            }

            //Set the buffer. You can see on Microsoft's page at 
            //http://msdn.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.setbuffer.aspx
            //that there are two overloads. One of the overloads has 3 parameters.
            //When setting the buffer, you need 3 parameters the first time you set it,
            //which we did in the Init method. The first of the three parameters
            //tells what byte array to use as the buffer. After we tell what byte array
            //to use we do not need to use the overload with 3 parameters any more.
            //(That is the whole reason for using the buffer block. You keep the same
            //byte array as buffer always, and keep it all in one block.)
            //Now we use the overload with two parameters. We tell 
            // (1) the offset and
            // (2) the number of bytes to use, starting at the offset.
                        
            //The number of bytes to send depends on whether the message is larger than
            //the buffer or not. If it is larger than the buffer, then we will have
            //to post more than one send operation. If it is less than or equal to the
            //size of the send buffer, then we can accomplish it in one send op.
            if (receiveSendToken.sendBytesRemainingCount <= this.socketListenerSettings.BufferSize)
            {
                receiveSendEventArgs.SetBuffer(receiveSendToken.bufferOffsetSend, receiveSendToken.sendBytesRemainingCount);
                //Copy the bytes to the buffer associated with this SAEA object.
                Buffer.BlockCopy(receiveSendToken.dataToSend, receiveSendToken.bytesSentAlreadyCount, receiveSendEventArgs.Buffer, receiveSendToken.bufferOffsetSend, receiveSendToken.sendBytesRemainingCount);
            }
            else
            {
                //We cannot try to set the buffer any larger than its size.
                //So since receiveSendToken.sendBytesRemainingCount > BufferSize, we just
                //set it to the maximum size, to send the most data possible.
                receiveSendEventArgs.SetBuffer(receiveSendToken.bufferOffsetSend, this.socketListenerSettings.BufferSize);
                //Copy the bytes to the buffer associated with this SAEA object.
                Buffer.BlockCopy(receiveSendToken.dataToSend, receiveSendToken.bytesSentAlreadyCount, receiveSendEventArgs.Buffer, receiveSendToken.bufferOffsetSend, this.socketListenerSettings.BufferSize);

                //We'll change the value of sendUserToken.sendBytesRemainingCount
                //in the ProcessSend method.
            }

            //post asynchronous send operation
            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.SendAsync(receiveSendEventArgs);
            
            if (!willRaiseEvent)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("StartSend in if (!willRaiseEvent), receiveSendToken id " + receiveSendToken.TokenId);
                }
                
                ProcessSend(receiveSendEventArgs);
            }            
        }

        //____________________________________________________________________________
        // This method is called by I/O Completed() when an asynchronous send completes.  
        // If all of the data has been sent, then this method calls StartReceive
        //to start another receive op on the socket to read any additional 
        // data sent from the client. If all of the data has NOT been sent, then it 
        //calls StartSend to send more data.        
        private void ProcessSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("ProcessSend, id " + receiveSendToken.TokenId);
            }
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("ProcessSend()", receiveSendToken);
            }                      

            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("ProcessSend, if Success, id " + receiveSendToken.TokenId);
                }

                receiveSendToken.sendBytesRemainingCount = receiveSendToken.sendBytesRemainingCount - receiveSendEventArgs.BytesTransferred;                 

                if (receiveSendToken.sendBytesRemainingCount == 0)
                {
                    // If we are within this if-statement, then all the bytes in
                    // the message have been sent. 
                    StartReceive(receiveSendEventArgs);
                }
                else
                {
                    // If some of the bytes in the message have NOT been sent,
                    // then we will need to post another send operation, after we store
                    // a count of how many bytes that we sent in this send op.                    
                    receiveSendToken.bytesSentAlreadyCount += receiveSendEventArgs.BytesTransferred;
                    // So let's loop back to StartSend().
                    StartSend(receiveSendEventArgs);
                }
            }
            else
            {
                //If we are in this else-statement, there was a socket error.
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("ProcessSend ERROR, id " + receiveSendToken.TokenId + "\r\n");
                }

                // We'll just close the socket if there was a
                // socket error when receiving data from the client.
                receiveSendToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
            }            
        }


        //____________________________________________________________________________
        // Does the normal destroying of sockets after 
        // we finish receiving and sending on a connection.        
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var receiveSendToken = (e.UserToken as DataHoldingUserToken);

            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("CloseClientSocket, id " + receiveSendToken.TokenId);
            }
            if (Program.watchThreads == true)   //for testing
            {
                DealWithThreadsForTesting("CloseClientSocket()", receiveSendToken);
            }

            // do a shutdown before you close the socket
            try
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("CloseClientSocket, Shutdown try, id " + receiveSendToken.TokenId + "\r\n");

                }
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            // throws if socket was already closed
            catch (Exception)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("CloseClientSocket, Shutdown catch, id " + receiveSendToken.TokenId + "\r\n");
                }
            }

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            e.AcceptSocket.Close();

            //Make sure the new DataHolder has been created for the next connection.
            //If it has, then dataMessageReceived should be null.
            if (receiveSendToken.theDataHolder.dataMessageReceived != null)
            {
                receiveSendToken.CreateNewDataHolder();
            }

            // Put the SocketAsyncEventArg back into the pool,
            // to be used by another client. This 
            this.poolOfRecSendEventArgs.Push(e);

            // decrement the counter keeping track of the total number of clients 
            //connected to the server, for testing
            Interlocked.Decrement(ref this.numberOfAcceptedSockets);

            if (Program.watchConnectAndDisconnect == true)   //for testing
            {
                Program.testWriter.WriteLine(receiveSendToken.TokenId + " disconnected. " + this.numberOfAcceptedSockets + " client(s) connected.");
            }

            //Release Semaphore so that its connection counter will be decremented.
            //This must be done AFTER putting the SocketAsyncEventArg back into the pool,
            //or you can run into problems.
            this.theMaxConnectionsEnforcer.Release();
        }

        //____________________________________________________________________________
        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            var acceptOpToken = (acceptEventArgs.UserToken as AcceptOpUserToken);
            Program.testWriter.WriteLine("Closing socket of accept id " + acceptOpToken.TokenId);
            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.           
            acceptEventArgs.AcceptSocket.Close();

            //Put the SAEA back in the pool.
            poolOfAcceptEventArgs.Push(acceptEventArgs);
        }

        //____________________________________________________________________________
        internal void CleanUpOnExit()
        {
            DisposeAllSaeaObjects();
        }

        //____________________________________________________________________________
        private void DisposeAllSaeaObjects()
        {
            SocketAsyncEventArgs eventArgs;
            while (this.poolOfAcceptEventArgs.Count > 0)
            {
                eventArgs = poolOfAcceptEventArgs.Pop();
                eventArgs.Dispose();
            }
            while (this.poolOfRecSendEventArgs.Count > 0)
            {
                eventArgs = poolOfRecSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }


        //____________________________________________________________________________
        //Display thread info.
        //Note that there is NOT a 1:1 ratio between managed threads 
        //and system (native) threads.
        //
        //Overloaded.
        //Use this one after the DataHoldingUserToken is available.
        //
        private void DealWithThreadsForTesting(string methodName, DataHoldingUserToken receiveSendToken)
        {            
            StringBuilder sb = new StringBuilder();
            sb.Append(" In " + methodName + ", receiveSendToken id " + receiveSendToken.TokenId + ". Thread id " + Thread.CurrentThread.ManagedThreadId + ". Socket handle " + receiveSendToken.socketHandleNumber + ".");
            sb.Append(DealWithNewThreads());

            Program.testWriter.WriteLine(sb.ToString());            
        }

        //Use this for testing, when there is NOT a UserToken yet. Use in SocketListener
        //method or Init().
        private void DealWithThreadsForTesting(string methodName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" In " + methodName + ", no usertoken yet. Thread id " + Thread.CurrentThread.ManagedThreadId);
            sb.Append(DealWithNewThreads());
            Program.testWriter.WriteLine(sb.ToString());
        }

        //____________________________________________________________________________
        //Display thread info.
        //Overloaded.
        //Use this one in method where AcceptOpUserToken is available.
        //
        private void DealWithThreadsForTesting(string methodName, AcceptOpUserToken theAcceptOpToken)
        {
            StringBuilder sb = new StringBuilder();
            string hString = hString = ". Socket handle " + theAcceptOpToken.socketHandleNumber;
            sb.Append(" In " + methodName + ", acceptToken id " + theAcceptOpToken.TokenId + ". Thread id " + Thread.CurrentThread.ManagedThreadId + hString + ".");
            sb.Append(DealWithNewThreads());
            Program.testWriter.WriteLine(sb.ToString());            
        }

        //____________________________________________________________________________
        //Display thread info.
        //called by DealWithThreadsForTesting
        private string DealWithNewThreads()
        {
            
            StringBuilder sb = new StringBuilder();
            bool newThreadChecker = false;
            lock (this.lockerForThreadHashSet)
            {
                if (managedThreadIds.Add(Thread.CurrentThread.ManagedThreadId) == true)
                {
                    managedThreads.Add(Thread.CurrentThread);
                    newThreadChecker = true;
                }
            }
            if (newThreadChecker == true)
            {
                
                //Display system threads
                //Note that there is NOT a 1:1 ratio between managed threads 
                //and system (native) threads.
                sb.Append("\r\n**** New managed thread.  Threading info:\r\nSystem thread numbers: ");
                arrayOfLiveThreadsInThisProcess = theProcess.Threads; //for testing only
                
                foreach (ProcessThread theNativeThread in arrayOfLiveThreadsInThisProcess)
                {
                    sb.Append(theNativeThread.Id.ToString() + ", ");
                }
                //Display managed threads
                //Note that there is NOT a 1:1 ratio between managed threads 
                //and system (native) threads.
                sb.Append("\r\nManaged threads that have been used: ");               
                foreach (Int32 theManagedThreadId in managedThreadIds)
                {
                    sb.Append(theManagedThreadId.ToString() + ", ");                    
                }

                //Managed threads above were/are being used.
                //Managed threads below are still being used now.
                sb.Append("\r\nManagedthread.IsAlive true: ");                
                foreach (Thread theManagedThread in managedThreads)
                {
                    if (theManagedThread.IsAlive == true)
                    {
                        sb.Append(theManagedThread.ManagedThreadId.ToString() + ", ");
                    }
                }                
                sb.Append("\r\nEnd thread info.");
            }
            return sb.ToString();
        }
    }    
}
