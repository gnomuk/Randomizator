using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

public sealed class BlockSpecificKeys : IDisposable
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private static IntPtr _hookID = IntPtr.Zero;
    private static ManualResetEvent _exitEvent = new ManualResetEvent(false);
    private LowLevelKeyboardProc _hookProc;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;

    private VirtualKeyCode _currentKey;
    private List<VirtualKeyCode> _blockedKeys;
    private volatile bool _isDisposed = false;
    private Thread _hookThread;

    public async Task BlockKey(VirtualKeyCode key, int durationMs, CancellationToken token, List<VirtualKeyCode> blockedKeys)
    {
        _blockedKeys = blockedKeys;
        if (_isDisposed) throw new ObjectDisposedException(nameof(BlockSpecificKeys));

        _currentKey = key;

        _hookProc = HookCallback;
        _hookThread = new Thread(() =>
        {
            _hookID = SetHook(_hookProc);
            Application.Run();
        })
        {
            IsBackground = true
        };

        ReleaseKeys(_blockedKeys);

        PressAndReleaseKey(true);
        _hookThread.Start();
        await Task.Delay(durationMs, token);
        Cleanup();
        PressAndReleaseKey(false);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (_blockedKeys.Contains((VirtualKeyCode)vkCode))
            {
                return (IntPtr)1;
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private void PressAndReleaseKey(bool down)
    {
        InputSimulator inputSimulator = new InputSimulator();
        if (down) inputSimulator.Keyboard.KeyDown(_currentKey);
        else inputSimulator.Keyboard.KeyUp(_currentKey);
    }

    private void ReleaseKeys(List<VirtualKeyCode> blockedKeys)
    {
        InputSimulator inputSimulator = new InputSimulator();
        foreach (VirtualKeyCode key in blockedKeys)
        {
            inputSimulator.Keyboard.KeyUp(key);
        }
    }

    private void Cleanup()
    {
        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }

        if (_hookThread != null && _hookThread.IsAlive)
        {
            Application.ExitThread();
            _hookThread.Join(1000);
            if (_hookThread.IsAlive) _hookThread.Abort();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Cleanup();
        _exitEvent.Dispose();
    }
}
