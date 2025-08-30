// Shared: TouchSurface.cs
using Android.Content;
using Android.Views;
using Microsoft.Maui.Handlers;

namespace FingerDice.Utilities.Handlers;
public partial class TouchSurfaceHandler : ViewHandler<TouchSurface, Android.Views.View>
{
    public TouchSurfaceHandler()
    : base(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper)
    {
    }
    protected override Android.Views.View CreatePlatformView() =>
        new Android.Views.View(Context) { Clickable = true, Focusable = true };

    protected override void ConnectHandler(Android.Views.View platformView)
    {
        platformView.Touch += OnTouch;
        base.ConnectHandler(platformView);
    }
    protected override void DisconnectHandler(Android.Views.View platformView)
    {
        platformView.Touch -= OnTouch;
        base.DisconnectHandler(platformView);
    }

    void OnTouch(object? s, Android.Views.View.TouchEventArgs e)
    {
        var v = PlatformView;
        var view = VirtualView;
        var ev = e.Event!;
        var action = ev.ActionMasked;

        int index = ev.ActionIndex;
        switch (action)
        {
            case MotionEventActions.Down:
            case MotionEventActions.PointerDown:
                {
                    var id = ev.GetPointerId(index);
                    var p = new Point(ev.GetX(index), ev.GetY(index));
                    view.OnPressed(id, p);
                    break;
                }
            case MotionEventActions.Move:
                {
                    for (int i = 0; i < ev.PointerCount; i++)
                    {
                        var id = ev.GetPointerId(i);
                        var p = new Point(ev.GetX(i), ev.GetY(i));
                        view.OnMoved(id, p);
                    }
                    break;
                }
            case MotionEventActions.Up:
            case MotionEventActions.PointerUp:
            case MotionEventActions.Cancel:
                {
                    var id = ev.GetPointerId(index);
                    view.OnReleased(id);
                    break;
                }
        }
        e.Handled = true;
    }
}
