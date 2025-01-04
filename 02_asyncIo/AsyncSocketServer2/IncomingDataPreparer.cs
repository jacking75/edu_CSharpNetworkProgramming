using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace AsyncSocketServer2;

class IncomingDataPreparer
{
    //object that will be used to lock the listOfDataHolders
    private object lockerForList = new object();
    private DataHolder theDataHolder;
    private SocketAsyncEventArgs theSaeaObject;

    public IncomingDataPreparer(SocketAsyncEventArgs e)
    {
        
        this.theSaeaObject = e;
    }
            
    private Int32 ReceivedTransMissionIdGetter()
    {
        Int32 receivedTransMissionId = Interlocked.Increment(ref Program.mainTransMissionId);
        return receivedTransMissionId;
    }

    private EndPoint GetRemoteEndpoint()
    {   
        return this.theSaeaObject.AcceptSocket.RemoteEndPoint;
    }

    internal DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEventArgs theSaeaObject)
    {
        DataHoldingUserToken receiveToken = (DataHoldingUserToken)theSaeaObject.UserToken;
        if (Program.watchProgramFlow == true)   //for testing
        {
            Program.testWriter.WriteLine("HandleReceivedData() " + receiveToken.TokenId);
        }
        theDataHolder = incomingDataHolder;
        theDataHolder.sessionId = receiveToken.SessionId;
        theDataHolder.receivedTransMissionId = this.ReceivedTransMissionIdGetter();            
        theDataHolder.remoteEndpoint = this.GetRemoteEndpoint();
        if ((Program.watchData == true) & (Program.runLongTest == false))
        {
            this.AddDataHolder();
        }
        
        return theDataHolder;
    }

    //You can use this method when testing for a short time.  Its contents will 
    //display at the end of the program if watchData == true.
    //If you are doing a long
    //test, then don't use this. Otherwise, List<DataHolder> will get too big, and
    //you'll probably run out of memory.
    //You can control this with the runLongTest variable in Program.
    private void AddDataHolder()
    {
        lock (this.lockerForList)
        {
            Program.listOfDataHolders.Add(theDataHolder);
        }
    }        
}
