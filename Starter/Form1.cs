﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starter
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteFile(IntPtr handle, 
            double[] lpBuffer,
            uint numBytesToWrite, 
            out uint numBytesWritten, 
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteFile(IntPtr handle,
            int lpBuffer,
            uint numBytesToWrite,
            out uint numBytesWritten,
            IntPtr lpOverlapped);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);



        //PIPE
        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);



        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName, 
            string lpCommandLine, 
            IntPtr lpProcessAttributes, 
            IntPtr lpThreadAttributes,
            bool bInheritHandles, 
            uint dwCreationFlags, 
            IntPtr lpEnvironment,
            string lpCurrentDirectory, 
            ref STARTUPINFO lpStartupInfo, 
            out PROCESS_INFORMATION lpProcessInformation);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateNamedPipe(string lpName, 
            uint dwOpenMode,
            uint dwPipeMode, 
            uint nMaxInstances, 
            uint nOutBufferSize, 
            uint nInBufferSize,
            uint nDefaultTimeOut, 
            IntPtr lpSecurityAttributes);




        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, 
            IntPtr hTargetProcessHandle, 
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, 
            uint dwOptions);


        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        private STARTUPINFO startupInfo;
        private PROCESS_INFORMATION processInfo;

        private int syncWay;
        private int N = 0;
        private double[] arrays;

        private uint bytesWritten;
        private uint bytesWritten1;
        private uint bytesRead;

        public Form1()
        {
            InitializeComponent();
            startupInfo = new STARTUPINFO();
            processInfo= new PROCESS_INFORMATION();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                syncWay = 1; // semaphore
            else if (radioButton2.Checked)
                syncWay = 2; // mutex
            else
                syncWay = 3; // event


            // todo write exception
            N = Int32.Parse(textBox1.Text);

            Generator gen = new Generator();

            arrays = gen.generate(N, 1, 3);

            arrays[2*N] = syncWay;

            //label3.Text = arrays[2 * N].ToString();

            // create pipe
            //IntPtr pipe = CreateNamedPipe("\\\\.\\pipe\\MyPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            //IntPtr pipe1 = CreateNamedPipe("\\\\.\\pipe\\MyPipe1", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1,
              //  512, 512, 5000, IntPtr.Zero);
            // PIPE_ACCESS_DUPLEX = 0x00000003
            // PIPE_TYPE_MESSAGE = 0x00000004
            //PIPE_READMODE_MESSAGE = 0x00000002
            //PIPE_WAIT = 0x00000000
            //PIPE_NOWAIT = 0x00000001
            IntPtr file = CreateFile("\\\\.\\pipe\\MyPipe", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
            IntPtr file1 = CreateFile("\\\\.\\pipe\\MyPipe1", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.OpenOrCreate, 0, IntPtr.Zero);
            gen.SortArrays(syncWay);
            WriteFile(file, arrays, 16, out bytesWritten, IntPtr.Zero);
            WriteFile(file1,N, 16, out bytesWritten1, IntPtr.Zero);
            // todo change location file
            //CreateProcessHelper.CreateProcess("C:\\Users\\veryoldbarny\\Documents\\WindowsFormsApplication7.exe", String.Empty);
            String path = "C:\\Users\\veryoldbarny\\WindowsFormsApplication7.exe";

            CreateProcess(path, null, IntPtr.Zero,
                 IntPtr.Zero, true, 0, IntPtr.Zero, null, ref startupInfo, out processInfo);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
