﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;

namespace BASeCamp.Logging
{
    /// <summary>
    ///     Debug logging output class. Can automatically transform Debug output (Debug.Print) and save it to a properly dated
    ///     and organized
    ///     file in %APPDATA%\-CompanyName-\DebugLogs\AppName. Just make sure it get's initialized by calling it in some fashion-
    ///     setting EnableDebugging to True is usually good enough.
    ///     Note that Debug output will not appear for Release builds, if we ever use them.
    /// </summary>
    public class DebugLogger : TraceListener
    {
        public static bool EnableLogging = true;
        public static bool FullExceptionLogging = false;
        public static DebugLogger Log = new DebugLogger(Application.ProductName);
        private String _ActiveLogFile;
        private String _LoggerName;
        private StreamWriter LogStream;
        object logStreamLock = new object();


        bool writerecursion;

        public DebugLogger(String sName, String sLogFolder)
        {
            _LoggerName = sName;
            InitLog(sLogFolder);
            if (EnableLogging)
                Trace.Listeners.Add(this);
        }

        public DebugLogger(String sName)
        {
            _LoggerName = sName;
            InitLog();
            if (EnableLogging)
            {
                Trace.Listeners.Add(this);
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

        public String ActiveLogFile
        {
            get { return _ActiveLogFile; }
        }

        private void InitLog()
        {
            InitLog(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BASeCamp\\DebugLogs"));
        }

        private void InitLog(String sLogFolder)
        {
            String BasePath = Path.Combine(sLogFolder, _LoggerName);
            Directory.CreateDirectory(BasePath);
            String BaseName = Application.ProductName;
            String LogFileUse = Path.Combine
            (BasePath, BaseName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + "." +
                       (new Random().Next()).ToString("x8") + ".log");
            FileStream fs = new FileStream(LogFileUse, FileMode.CreateNew);
            try
            {
                LogStream = new StreamWriter(fs);
                _ActiveLogFile = LogFileUse;
            }
            catch
            {
                fs.Dispose();
                throw;
            }

            lock (logStreamLock)
            {
                LogStream.WriteLine("--Log Initialized--");
                WriteLogHeader();
            }

            //System.Windows.Forms.Application.ThreadException += Application_ThreadException;
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //for logging we'll trace unhandled exceptions.
            //catch-all exception in case anything goes wrong. 

            try
            {
                WriteLine(e.Exception.ToString());
            }
            catch
            {
                // Ignore it.. if we get an exception while logging an exception, attempting
                // to log it again is probably not going to be useful.
            }
        }

        private void WriteLogHeader()
        {
            LogStream.WriteLine("--" + DateTime.Now + "--");
            LogStream.WriteLine(Application.ProductName);
            LogStream.WriteLine(Application.ProductVersion);
            LogStream.WriteLine("Main executable file: " + Assembly.GetEntryAssembly().Location);
            LogStream.WriteLine("Command line: " + Environment.CommandLine);
            LogStream.WriteLine("======");
        }

        public override void Write(String LogMessage)
        {
            if (!EnableLogging) return;
            if (writerecursion) return;
            writerecursion = true;
            try
            {
                lock (logStreamLock)
                {
                    try
                    {
                        LogStream.Write(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ">>" + LogMessage);
                        LogStream.Flush();
                    }
                    catch
                    {
                        //unknown error
                    }
                }
            }
            finally
            {
                writerecursion = false;
            }
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (!FullExceptionLogging)
                return;

            try
            {
                WriteLine(e.Exception.ToString());
            }
            catch
            {
                //Application must be in a very bad state- or, the error was actually caused by DebugLogger.
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Log the Exception. Note that we don't want ANOTHER exception to occur, so we wrap it all in a try-catch.
            try
            {
                WriteLine(e.ExceptionObject.ToString());
            }
            catch
            {
                //Application must be in a very bad state- or, the error was actually caused by DebugLogger.
            }

            //ExceptionPrinter.FatalException((Exception)e.ExceptionObject);
        }
    }
}