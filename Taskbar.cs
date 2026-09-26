using System.Runtime.InteropServices;

namespace MyTextureConverter
{
    internal enum TaskbarState
    {
        None = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    internal static class Taskbar
    {
        [ComImport, Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);
            void SetProgressValue(IntPtr hwnd, ulong completed, ulong total);
            void SetProgressState(IntPtr hwnd, TaskbarState state);
        }

        [ComImport, Guid("56FDF344-FD6D-11d0-958A-006097C9A090"), ClassInterface(ClassInterfaceType.None)]
        private class TaskbarList
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO info);

        private const uint FlashAll = 0x3;
        private const uint FlashUntilForeground = 0xC;

        private static readonly Lazy<ITaskbarList3?> Instance = new(() =>
        {
            try
            {
                var taskbar = (ITaskbarList3)new TaskbarList();
                taskbar.HrInit();
                return taskbar;
            }
            catch (Exception ex) when (ex is COMException or InvalidCastException)
            {
                return null;
            }
        });

        public static void SetState(Form form, TaskbarState state)
        {
            try
            {
                Instance.Value?.SetProgressState(form.Handle, state);
            }
            catch (COMException)
            {
            }
        }

        public static void SetValue(Form form, int done, int total)
        {
            try
            {
                Instance.Value?.SetProgressValue(form.Handle, (ulong)done, (ulong)Math.Max(total, 1));
            }
            catch (COMException)
            {
            }
        }

        public static void Flash(Form form)
        {
            var info = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
                hwnd = form.Handle,
                dwFlags = FlashAll | FlashUntilForeground
            };
            FlashWindowEx(ref info);
        }
    }
}
