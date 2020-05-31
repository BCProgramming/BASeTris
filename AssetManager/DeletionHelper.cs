using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace BASeTris.AssetManager
{
    public class DeletionHelperAPI : IDisposable
    {
        private const int FILE_FLAG_DELETE_ON_CLOSE = 0x4000000;
        private const int FILE_SHARE_READ = 0x1;
        private const int FILE_SHARE_WRITE = 0x2;
        private const int ACCESS_NONE = 0;
        private const int OPEN_EXISTING = 3;
        private const int FILE_SHARE_DELETE = 0x4;

        private List<DeletionHelperAPI> Subdeletors = new List<DeletionHelperAPI>();

        [DllImport
        ("kernel32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        SafeFileHandle FileHandle = null;

        public DeletionHelperAPI(String FileOrFolder)
        {
            //if it's a folder, create subobjects.
            if (Directory.Exists(FileOrFolder))
            {
                //create a subdeletor for each File/folder in that file-folder.
                DirectoryInfo di = new DirectoryInfo(FileOrFolder);
                foreach (FileSystemInfo fsi in di.GetFileSystemInfos())
                {
                    Subdeletors.Add(new DeletionHelperAPI(fsi.FullName));
                }
            }
            else
            {
                FileHandle = CreateFile
                (FileOrFolder, ACCESS_NONE, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, IntPtr.Zero,
                    OPEN_EXISTING, FILE_FLAG_DELETE_ON_CLOSE, IntPtr.Zero);
            }
        }

        public void Dispose()
        {
            if (Subdeletors != null)
            {
                foreach (var iterate in Subdeletors)
                {
                    iterate.Dispose();
                }
            }

            if (FileHandle != null)
            {
                FileHandle.Dispose();
            }
        }
    }


    /// <summary>
    ///     Rudimentary class that is instantiated with the name of a folder, and deletes that folder when the class instance is disposed.
    ///     Currently it is used to defer deletion of the folder used to store the contents of dynamically extracted ZIP files (which is a mechanism that in and of itself
    ///     only half-works anyway)
    /// </summary>
    public class DeletionHelper : IDisposable
    {
        //helper native methods.
        private static readonly Queue<DeletionHelper> QueuedDeletions = new Queue<DeletionHelper>();
        private readonly String mDeleteThis = "";

        public DeletionHelper(String deletefolder)
        {
            mDeleteThis = deletefolder;
        }

        public String DeleteThis
        {
            get { return mDeleteThis; }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

        public static void QueueDeletion(String FileOrDir)
        {
            var dh = new DeletionHelper(FileOrDir);
            QueuedDeletions.Enqueue(dh);
        }

        ~DeletionHelper()
        {
            Dispose();
        }

        #region IDisposable Members

        private int delaycount;

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(mDeleteThis))
                {
                    Debug.Print("Deleting folder:" + mDeleteThis);
                    Directory.Delete(mDeleteThis, true);
                }
                else if (File.Exists(mDeleteThis))
                {
                    int AttemptCount = 0;
                    try
                    {
                        AttemptCount++;
                        Debug.Print("Deleting File:" + mDeleteThis);
                        File.Delete(mDeleteThis);
                    }
                    catch (IOException e)
                    {
                        if (AttemptCount == 10)
                        {
                            return; //give up!
                        }

                        Thread.Sleep(250);
                    }
                }
            }
            catch (IOException ioe)
            {
                delaycount++;
                if (delaycount > 5)
                {
                    //schedule for reboot deletion.
                    ScheduleRebootDeletion();
                    return;
                }

                DelayCall(new TimeSpan(0, 0, 0, delaycount), Dispose);
            }
        }

        private static void DelayInvokeThread(Object parameters)
        {
            var acquireparam = (Object[]) parameters;

            var useaction = acquireparam[1] as Action;
            var waittime = (TimeSpan) acquireparam[2];
            var startdelay = (DateTime) acquireparam[3];
            while (DateTime.Now - startdelay < waittime)
            {
                Thread.Sleep(0);
            }

            if (useaction == null) return;
            useaction();
        }

        private static void DelayCall(TimeSpan waittime, Action routine)
        {
            var usethread = new Thread(DelayInvokeThread);
            usethread.Start(new Object[] {usethread, routine, waittime, DateTime.Now});
        }

        private void ScheduleRebootDeletion()
        {
            try
            {
                if (!MoveFileEx(mDeleteThis, null, MoveFileFlags.DelayUntilReboot))
                {
                }
            }
            catch (DllNotFoundException dlex)
            {
            }
        }

        #endregion

        [Flags]
        internal enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }
    }
}