using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection; //for assembly name


namespace AsyncSocketServer2
{
    public class TestFileWriter
    {
        private object lockerForLog = new object();
        internal string saveFile;
        StreamWriter tw;

        public TestFileWriter()
        {
            //We create a new log file every time we run the app.
            this.saveFile = GetSaveFileName();
            // create a writer and open the file
            tw = new StreamWriter(this.saveFile);
        }

        private string GetSaveFileName()
        {
            string saveDirectory = @"c:\LogForSaeaTest\";            

            try
            {
                if (Directory.Exists(saveDirectory) == false)
                {
                    Directory.CreateDirectory(saveDirectory);
                }
            }
            catch
            {
                Console.WriteLine("Could not create save directory for log. See TestFileWriter.cs."); Console.ReadLine();
            }

            string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
            Int32 index = assemblyFullName.IndexOf(',');
            string saveFile = assemblyFullName.Substring(0, index);
            string dt = DateTime.Now.ToString("yyMMddHHmmss");
            //Save directory is created in ConfigFileHandler
            saveFile = saveDirectory + saveFile + "-" + dt + ".txt";
            return saveFile;
        }

        internal void WriteLine(string lineToWrite)
        {
            if (Program.consoleWatch == true)
            {
                Console.WriteLine(lineToWrite);
            }

            lock (this.lockerForLog)
            {
                tw.WriteLine(lineToWrite);
            }
        }

        internal void Close()
        {
            tw.Close();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This session was logged to " + saveFile);
            Console.WriteLine();
            Console.WriteLine();
        }

    }
}


