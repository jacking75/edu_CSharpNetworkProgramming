using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public static class ExceptionHelper
    {
        public static string ExtractException(this Exception ex, int indent = 2)
        {
            var indentStr = new String(' ', indent);
            var traceLog = new System.Text.StringBuilder();
            var trace = new System.Diagnostics.StackTrace(ex, true);
            foreach (var frame in trace.GetFrames())
            {
                traceLog.AppendLine($"{indentStr}File Name : {frame.GetFileName()}");
                traceLog.AppendLine($"{indentStr}Class Name : {frame.GetMethod().ReflectedType.Name}");
                traceLog.AppendLine($"{indentStr}Method Name : {frame.GetMethod()}");
                traceLog.AppendLine($"{indentStr}Line Number : {frame.GetFileLineNumber()}");
                traceLog.AppendLine($"=======================================================");
            }

            return traceLog.ToString();
        }
    }
}
