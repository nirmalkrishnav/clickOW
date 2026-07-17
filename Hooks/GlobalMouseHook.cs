using System.Runtime.InteropServices;
using ClickOW.Interop;

namespace ClickOW.Hooks;

public enum MouseButtonKind
{
    Left,
    Right,
    Middle
}

public sealed class MouseHookEventArgs : EventArgs
{
    public MouseButtonKind Button { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed class MouseMoveEventArgs : EventArgs
{
    public int X { get; init; }
    public int Y { get; init; }
}

/// <summary>
/// System-wide low-level mouse hook. Raises typed events with screen coordinates.
/// The hook callback is click-through friendly: it never swallows events.
/// </summary>
public sealed class GlobalMouseHook : IDisposable
{
    private readonly NativeMethods.LowLevelProc _proc;
    private nint _hookId = nint.Zero;
    private bool _disposed;

    public event EventHandler<MouseHookEventArgs>? ButtonDown;
    public event EventHandler<MouseHookEventArgs>? ButtonUp;
    public event EventHandler<MouseMoveEventArgs>? MouseMove;

    public GlobalMouseHook()
    {
        // Keep the delegate referenced so it is not garbage collected.
        _proc = HookCallback;
    }

    public void Start()
    {
        if (_hookId != nint.Zero)
        {
            return;
        }

        nint hModule = NativeMethods.GetModuleHandle(null);
        _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _proc, hModule, 0);
        if (_hookId == nint.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to install low-level mouse hook. Error {Marshal.GetLastWin32Error()}.");
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
            var data = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            int msg = (int)wParam;

            switch (msg)
            {
                case NativeMethods.WM_LBUTTONDOWN:
                    RaiseButton(ButtonDown, MouseButtonKind.Left, data.pt);
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    RaiseButton(ButtonUp, MouseButtonKind.Left, data.pt);
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    RaiseButton(ButtonDown, MouseButtonKind.Right, data.pt);
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    RaiseButton(ButtonUp, MouseButtonKind.Right, data.pt);
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    RaiseButton(ButtonDown, MouseButtonKind.Middle, data.pt);
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    RaiseButton(ButtonUp, MouseButtonKind.Middle, data.pt);
                    break;
                case NativeMethods.WM_MOUSEMOVE:
                    MouseMove?.Invoke(this, new MouseMoveEventArgs { X = data.pt.x, Y = data.pt.y });
                    break;
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void RaiseButton(EventHandler<MouseHookEventArgs>? handler, MouseButtonKind button, NativeMethods.POINT pt)
    {
        handler?.Invoke(this, new MouseHookEventArgs { Button = button, X = pt.x, Y = pt.y });
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
