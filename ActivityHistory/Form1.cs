using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace ActivityHistory
{
    public partial class ActivityMonitorMainForm : Form
    {
        public const int MaxHistoryRowCount = 200;
        private FocusInfo LastFocus = null;
        private IList<FocusChange> FocusChanges = new List<FocusChange>();
        private CsvWriter csvWriter;
        private string logFileName = "activity.log.csv";
        private int updateCounter = 0;
        SystemForegroundTitleListener systemFocusTitleListener = new SystemForegroundTitleListener();
        public ActivityMonitorMainForm()
        {
            InitializeComponent();
            csvWriter = new CsvWriter(
                new StreamWriter(logFileName, true, Encoding.UTF8),
                new Configuration() { Delimiter = ";" }
            );
            systemFocusTitleListener.ForegroundTitleProbablyChanged += UpdateCurrentApplication;
        }

        private void DisposeNonDesigner(bool disposing)
        {
            Refs.Dispose(systemFocusTitleListener);
        }

        private void UpdateCurrentApplication()
        {
            updateCounter++;
            var currentFocus = GetCurrentFocus();
            if (FocusInfo.Equality.Equals(currentFocus, LastFocus))
            {
                return;
            }
            textBox2.Text = $"{currentFocus.WindowTitle}, ({updateCounter})";
            LastFocus = currentFocus;
            RegisterFocusChange(new FocusChange
            {
                Timestamp = DateTime.Now.ToString(FocusChange.DateFormat),
                FocusInfo = currentFocus,
            });
        }


        private void RegisterFocusChange(FocusChange focusChange)
        {
            FocusChanges.Add(focusChange);
            new DataGridViewRow();
            dataGridView1.Rows.Insert(
                0,
                focusChange.Timestamp,
                focusChange.FocusInfo.WindowTitle,
                focusChange.FocusInfo.ExecutableName
            );
            while (dataGridView1.Rows.Count > MaxHistoryRowCount)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
            }
            csvWriter.WriteRecord(focusChange);
            csvWriter.NextRecord();
        }

        static class WinApi {

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
            public static string GetWindowText(IntPtr hWnd) {
                for (int nChars = 1024; nChars < 512 * 1024; nChars *= 4)
                {
                    StringBuilder buff = new StringBuilder(nChars);
                    if (GetWindowText(hWnd, buff, nChars) == 0)
                    {
                        return null;
                    }
                    if (buff.Length < nChars)
                    {
                        return buff.ToString();
                    }
                }
                throw new Exception("Could not retrieve window title");
            }


            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

            public static uint GetWindowProcessId(IntPtr hWnd)
            {
                GetWindowThreadProcessId(hWnd, out var result);
                return result;
            }
        }

        public FocusInfo GetCurrentFocus()
        {
            try
            {
                IntPtr handle = WinApi.GetForegroundWindow();
                FocusInfo focusInfo = new FocusInfo()
                {
                    HWND = (long)handle,
                };

                var pid = WinApi.GetWindowProcessId(handle);
                focusInfo.PID = pid;
                var process = Process.GetProcessById((int)pid);
                focusInfo.WindowTitle = WinApi.GetWindowText(handle);
                focusInfo.ExecutableFile = process.MainModule.FileName;
                focusInfo.ExecutableName = process.MainModule.ModuleName;
                return focusInfo;
            }
            catch (Exception ex)
            {
                return new FocusInfo() { Error = ex.ToString() };
            }
        }

        private void ActivityMonitorMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            csvWriter.Dispose();
        }

        private void ActivityMonitorMainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void ActivityMonitorMainForm_Shown(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("--minimized"))
            {
                WindowState = FormWindowState.Minimized;
                Visible = false;
                Hide();
            }
        }
    }
}
