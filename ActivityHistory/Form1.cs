using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ActivityHistory
{
    public partial class ActivityMonitorMainForm : Form
    {
        private FocusInfo LastFocus = null;
        private IList<FocusChange> FocusChanges = new List<FocusChange>();
        private CsvWriter csvWriter;
        public ActivityMonitorMainForm()
        {
            InitializeComponent();
            csvWriter = new CsvWriter(
                new StreamWriter(File.OpenWrite("activity.log.csv")), 
                new Configuration()
                {
                    Delimiter = ";"
                }
            );
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateCurrentApplication();
        }

        private void UpdateCurrentApplication()
        {
            var currentFocus = GetCurrentFocus();
            if (FocusInfo.Equality.Equals(currentFocus, LastFocus))
            {
                return;
            }
            label1.Text = currentFocus.WindowTitle;
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
            while (dataGridView1.Rows.Count > 100)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count);
            }
            csvWriter.WriteRecord(focusChange);
            csvWriter.NextRecord();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public FocusInfo GetCurrentFocus()
        {
            try
            {
                const int nChars = 1024;
                IntPtr handle = GetForegroundWindow();
                FocusInfo focusInfo = new FocusInfo()
                {
                    HWND = (long)handle,
                };

                uint pid;
                GetWindowThreadProcessId(handle, out pid);
                focusInfo.PID = pid;
                var process = Process.GetProcessById((int)pid);
                StringBuilder buff = new StringBuilder(nChars);
                if (GetWindowText(handle, buff, nChars) > 0)
                {
                    focusInfo.WindowTitle = buff.ToString();
                }
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
    }
}
