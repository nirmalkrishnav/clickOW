using System.Runtime.InteropServices;
using System.Windows.Input;
using ClickOW.Interop;

namespace ClickOW.Hooks;

/// <summary>
/// System-wide low-level keyboard hook that fires named events for the
/// application's global shortcuts (Ctrl+Alt+O, Ctrl+Alt+L).
/// </summary>
public sealed class GlobalKeyboardHook : IDisposable
{
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12; // Alt

    private readonly NativeMethods.LowLevelProc _proc;
    private nint _hookId = nint.Zero;
    private bool _disposed;

    /// <summary>Raised when Ctrl+Alt+O is pressed (toggle ClickOW on/off).</summary>
    public event EventHandler? ToggleEnabled;

    /// <summary>Raised when Ctrl+Alt+L is pressed (toggle laser pointer mode).</summary>
    public event EventHandler? ToggleLaser;

    public GlobalKeyboardHook()
    {
        _proc = HookCallback;
    }

    public void Start()
    {
        if (_hookId != nint.Zero)
        {
            return;
        }

        nint hModule = NativeMethods.GetModuleHandle(null);
        _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _proc, hModule, 0);
        if (_hookId == nint.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to install low-level keyboard hook. Error {Marshal.GetLastWin32Error()}.");
        }
    }

    public void Stop()
    {
        if (_hookId != nint.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = nint.Zero;
        }
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;
            if (msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN)
            {
                var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                var key = KeyInterop.KeyFromVirtualKey((int)data.vkCode);

                bool ctrl = (NativeMethods.GetKeyState(VK_CONTROL) & 0x8000) != 0;
                bool alt = (NativeMethods.GetKeyState(VK_MENU) & 0x8000) != 0;

                if (ctrl && alt)
                {
                    if (key == Key.O)
                    {
                        ToggleEnabled?.Invoke(this, EventArgs.Empty);
                    }
                    else if (key == Key.L)
                    {
                        ToggleLaser?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
    }
}
