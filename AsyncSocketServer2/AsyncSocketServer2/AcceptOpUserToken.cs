using System;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocketServer2
{
    class AcceptOpUserToken
    {
        //The only reason to use this UserToken in our app is to give it an identifier,
        //so that you can see it in the program flow. Otherwise, you would not need it.

        
        private Int32 id; //for testing only
        internal Int32 socketHandleNumber; //for testing only

        public AcceptOpUserToken(Int32 identifier)
        {
            id = identifier;
            

            //if (Program.watchProgramFlow == true)   //for testing
            //{
            //    Program.testWriter.WriteLine("AcceptOpUserToken constructor, idOfThisObject " + id);
            //}
        }

        public Int32 TokenId
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
    }
}
