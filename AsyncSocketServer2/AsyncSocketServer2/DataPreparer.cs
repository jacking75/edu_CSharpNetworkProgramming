using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AsyncSocketServer2
{
    class DataPreparer
    {
        private DataHolder theDataHolder;

        public DataPreparer(DataHolder incomingDataHolder)
        {
            theDataHolder = incomingDataHolder;
            theDataHolder.receivedTransMissionId = TransMissionIdGetter();
        }

        public Int32 TransMissionIdGetter()
        {
            Int32 receivedTransMissionId;
            lock (Program.lockerForTid)
            {
                receivedTransMissionId = Program.mainTransMissionId;
                Program.mainTransMissionId++;
            }            
            return receivedTransMissionId;
        }

        public void SendBufferSetter(SocketAsyncEventArgs e)
        {
            //Let's send back the receivedTransMissionId, followed by the
            //message that the client sent to the server. And we have to
            //prefix it with the length of the message. So we have to put 3 
            //things into the array.
            // 1) first prefix that tells the length
            // 2) then the receivedTransMissionId
            // 3) and last the message that we received from the client, which
            // we have stored in our trusty DataHolder until we needed it.
            //That is our data protocol.

            //Convert the receivedTransMissionId to byte array.
            Byte[] idByteArray = BitConverter.GetBytes(theDataHolder.receivedTransMissionId);

            //Determine the length of all the data that we will send back.
            Int32 lengthOfArrayToSend = Program.receivePrefixLength + idByteArray.Length + theDataHolder.dataMessageReceived.Length;

            //So, now we convert the integer which tells the length into a byte array.
            //Aren't byte arrays wonderful? Maybe you'll dream about byte arrays tonight
            Byte[] arrayOfBytesInPrefix = BitConverter.GetBytes(lengthOfArrayToSend - Program.receivePrefixLength);

            //Create the byte array to send.
            Byte[] arrayOfBytesToSend = new Byte[lengthOfArrayToSend];

            //Now copy the 3 things to the arrayOfBytesToSend.
            Buffer.BlockCopy(arrayOfBytesInPrefix, 0, arrayOfBytesToSend, 0, Program.receivePrefixLength);
            Buffer.BlockCopy(idByteArray, 0, arrayOfBytesToSend, Program.receivePrefixLength, idByteArray.Length);
            //The message that the client sent is already in a byte array, in DataHolder.
            Buffer.BlockCopy(theDataHolder.dataMessageReceived, 0, arrayOfBytesToSend, Program.receivePrefixLength + idByteArray.Length, theDataHolder.dataMessageReceived.Length);

            // Great! Now tell SocketAsyncEventArgs object to send this byte array.
            //Your client will have to know the data protocol, in order to separate
            //the receivedTransMissionId from the message that we are echoing back,
            //because the message is ASCII but the receivedTransMissionId is binary.

            DataHoldingUserToken theUserToken = (DataHoldingUserToken)e.UserToken;
            
            Console.WriteLine("offset for send in DP = " + theUserToken.sendBufferOffset);

            Buffer.BlockCopy(arrayOfBytesToSend, 0, e.Buffer, theUserToken.sendBufferOffset, lengthOfArrayToSend);

            e.SetBuffer(e.Buffer, theUserToken.sendBufferOffset, lengthOfArrayToSend);
        }

//        public DataHolder CreateNewDataHolder(Int32 id)
        //public DataHolder CreateNewDataHolder()
        //{
        //    //id used for testing only
        //    return new DataHolder();
        //}
    }
}
