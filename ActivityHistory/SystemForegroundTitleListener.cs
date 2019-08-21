using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ActivityHistory
{
    public class SystemForegroundTitleListener : IDisposable
    {
        private SafeHandle hFocusedApplicationEvent;
        private SafeHandle hWindowTitleChangeEvent;
        private IntPtr previousHwnd;

        public event Action ForegroundTitleProbablyChanged;

        private Api.WinEventDelegate _HandleSystemForegroundEvent;
        private Api.WinEventDelegate _HandleObjectNamechangeEvent;

        public SystemForegroundTitleListener()
        {
            _HandleSystemForegroundEvent = HandleSystemForegroundEvent;
            _HandleObjectNamechangeEvent = HandleObjectNamechangeEvent;
            Refs.Set(ref hFocusedApplicationEvent,
                SetWinEventHook(Api.EVENT_SYSTEM_FOREGROUND, _HandleSystemForegroundEvent));
        }

        private void HandleSystemForegroundEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == previousHwnd)
                return;
            var threadId = Api.GetWindowThreadProcessId(hwnd, out var processId);
            Refs.Set(ref hWindowTitleChangeEvent,
                SetWinEventHook(Api.EVENT_OBJECT_NAMECHANGE, processId, threadId, _HandleObjectNamechangeEvent));
            previousHwnd = hwnd;
            FireForegroundTitleProbablyChanged();
        }
        private void HandleObjectNamechangeEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            FireForegroundTitleProbablyChanged();
        }

        private void FireForegroundTitleProbablyChanged()
        {
            Refs.Fire(ForegroundTitleProbablyChanged);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Refs.Dispose(hFocusedApplicationEvent);
            Refs.Dispose(hWindowTitleChangeEvent);
            // TODO: we must find a way to keep those delegate references alive. Ts there really no way in .NET to really keep these delegates from being collected?
            // Putting GC.KeepAlive will at least prevent the compiler from optimizing out the field references, but it is still possible that the delegate is collected earlier
            // during a gc run
            GC.KeepAlive(_HandleSystemForegroundEvent);
            GC.KeepAlive(_HandleObjectNamechangeEvent);
        }
        ~SystemForegroundTitleListener()
        {
            Dispose(false);
        }

        internal static SafeHandle SetWinEventHook(uint eventId, Api.WinEventDelegate winEventDelegate)
        {
            return SetWinEventHook(eventId, 0, 0, winEventDelegate);
        }
        internal static SafeHandle SetWinEventHook(uint eventId, uint processId, uint threadId, Api.WinEventDelegate winEventDelegate)
        {
            return Api.SetWinEventHook(eventId, eventId, IntPtr.Zero, winEventDelegate, processId, threadId, Api.WINEVENT_OUTOFCONTEXT);
        }

        internal static class Api
        {
            [DllImport("user32.dll")]
            public static extern WinEventHookHandle SetWinEventHook(
                uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags
                );
            [DllImport("user32.dll")]
            public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


            public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
            public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
            public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
            public const uint EVENT_OBJECT_NAMECHANGE = 0x800C;


            public delegate void WinEventDelegate(
                IntPtr hWinEventHook, uint eventType,
                IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime
                );


            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public class WinEventHookHandle : SafeHandleZeroOrMinusOneIsInvalid
            {
                private WinEventHookHandle() : base(true) { }
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                override protected bool ReleaseHandle()
                {
                    return UnhookWinEvent(handle);
                }
            }
        }
    }
}
