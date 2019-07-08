using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocketServer2
{
    class PrefixHandler
    {
        public Int32 HandlePrefix(SocketAsyncEventArgs e, DataHoldingUserToken receiveSendToken, Int32 remainingBytesToProcess)
        {            
            //receivedPrefixBytesDoneCount tells us how many prefix bytes were
            //processed during previous receive ops which contained data for 
            //this message. Usually there will NOT have been any previous 
            //receive ops here. So in that case,
            //receiveSendToken.receivedPrefixBytesDoneCount would equal 0.
            //Create a byte array to put the new prefix in, if we have not
            //already done it in a previous loop.
            if (receiveSendToken.receivedPrefixBytesDoneCount == 0)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("PrefixHandler, create prefix array " + receiveSendToken.TokenId);
                }
                receiveSendToken.byteArrayForPrefix = new Byte[receiveSendToken.receivePrefixLength];
            }

            // If this next if-statement is true, then we have received >=
            // enough bytes to have the prefix. So we can determine the 
            // length of the message that we are working on.
            if (remainingBytesToProcess >= receiveSendToken.receivePrefixLength - receiveSendToken.receivedPrefixBytesDoneCount)
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("PrefixHandler, enough for prefix " + receiveSendToken.TokenId + ". remainingBytesToProcess = " + remainingBytesToProcess);
                }
                //Now copy that many bytes to byteArrayForPrefix.
                //We can use the variable receiveMessageOffset as our main
                //index to show which index to get data from in the TCP
                //buffer.
                Buffer.BlockCopy(e.Buffer, receiveSendToken.receiveMessageOffset - receiveSendToken.receivePrefixLength + receiveSendToken.receivedPrefixBytesDoneCount, receiveSendToken.byteArrayForPrefix, receiveSendToken.receivedPrefixBytesDoneCount, receiveSendToken.receivePrefixLength - receiveSendToken.receivedPrefixBytesDoneCount);

                remainingBytesToProcess = remainingBytesToProcess - receiveSendToken.receivePrefixLength + receiveSendToken.receivedPrefixBytesDoneCount;

                receiveSendToken.recPrefixBytesDoneThisOp = receiveSendToken.receivePrefixLength - receiveSendToken.receivedPrefixBytesDoneCount;

                receiveSendToken.receivedPrefixBytesDoneCount = receiveSendToken.receivePrefixLength;

                receiveSendToken.lengthOfCurrentIncomingMessage = BitConverter.ToInt32(receiveSendToken.byteArrayForPrefix, 0);

                

                if (Program.watchData == true)
                {
                    //Now see what integer the prefix bytes represent, for the length.
                    StringBuilder sb = new StringBuilder(receiveSendToken.byteArrayForPrefix.Length);
                    sb.Append(" Token id " + receiveSendToken.TokenId + ". " + receiveSendToken.receivePrefixLength + " bytes in prefix:");
                    foreach (byte theByte in receiveSendToken.byteArrayForPrefix)
                    {
                        sb.Append(" " + theByte.ToString());
                    }
                    sb.Append(". Message length: " + receiveSendToken.lengthOfCurrentIncomingMessage);

                    Program.testWriter.WriteLine(sb.ToString());
                }
                
            }

            //This next else-statement deals with the situation 
            //where we have some bytes
            //of this prefix in this receive operation, but not all.
            else
            {
                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("PrefixHandler, NOT all of prefix " + receiveSendToken.TokenId + ". remainingBytesToProcess = " + remainingBytesToProcess);
                }
                //Write the bytes to the array where we are putting the
                //prefix data, to save for the next loop.
                Buffer.BlockCopy(e.Buffer, receiveSendToken.receiveMessageOffset - receiveSendToken.receivePrefixLength + receiveSendToken.receivedPrefixBytesDoneCount, receiveSendToken.byteArrayForPrefix, receiveSendToken.receivedPrefixBytesDoneCount, remainingBytesToProcess);

                receiveSendToken.recPrefixBytesDoneThisOp = remainingBytesToProcess;
                receiveSendToken.receivedPrefixBytesDoneCount += remainingBytesToProcess;
                remainingBytesToProcess = 0;
            }

            // This section is needed when we have received
            // an amount of data exactly equal to the amount needed for the prefix,
            // but no more. And also needed with the situation where we have received
            // less than the amount of data needed for prefix. 
            if (remainingBytesToProcess == 0)
            {   
                receiveSendToken.receiveMessageOffset = receiveSendToken.receiveMessageOffset - receiveSendToken.recPrefixBytesDoneThisOp;
                receiveSendToken.recPrefixBytesDoneThisOp = 0;
            }
            return remainingBytesToProcess;
        }
    }
}
