using nl.flukeyfiddler.bt.Utils.Logger;
using System;
using System.Reflection;

namespace nl.flukeyfiddler.bt.TurnDirectorDebug.Utils
{
    public static class Logger
    {
        private static LogFilePath logFilePath;
        
        public static void SetLogFilePath(LogFilePath logFilePath)
        {
            Logger.logFilePath = logFilePath;
        }

        public static void Error(Exception ex, MethodBase caller = null)
        {
            LoggerUtil.Error(logFilePath, ex, caller);
        }

        public static void Line(string line, MethodBase caller = null)
        {
            LoggerUtil.Line(logFilePath, line, caller);
        }

        public static void InfoLine(MethodBase caller = null)
        {
            LoggerUtil.InfoLine(logFilePath, caller);
        }

        public static void EndLine()
        {
            LoggerUtil.EndLine(logFilePath);
        }

        public static void Minimal(string line)
        {
            LoggerUtil.Minimal(logFilePath, line);
        }

        public static void Block(string[] lines, MethodBase caller = null)
        {
            LoggerUtil.Block(logFilePath, lines, caller);
        }

        public static void GameStarted()
        {
            LoggerUtil.GameStarted(logFilePath);
        }
    }
}
