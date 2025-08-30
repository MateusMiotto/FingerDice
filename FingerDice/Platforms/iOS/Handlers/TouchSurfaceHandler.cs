// Platforms/iOS/TouchSurfaceHandler.cs
using FingerDice.Utilities.Handlers;
using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

public class TouchSurfaceHandler : ViewHandler<TouchSurface, UIView>
{
    public TouchSurfaceHandler()
        : base(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper)
    {
    }

    protected override UIView CreatePlatformView()
    {
        var uiview = new UIView();
        uiview.MultipleTouchEnabled = true;
        return uiview;
    }

    protected override void ConnectHandler(UIView platformView)
    {
        platformView.AddGestureRecognizer(new TouchGestureRecognizer(VirtualView, platformView));
        base.ConnectHandler(platformView);
    }

    class TouchGestureRecognizer : UIGestureRecognizer
    {
        readonly TouchSurface _view;
        readonly UIView _ui;
        public TouchGestureRecognizer(TouchSurface view, UIView ui)
        {
            _view = view; _ui = ui; CancelsTouchesInView = false;
        }
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            foreach (UITouch t in touches)
            {
                var p = t.LocationInView(_ui);
                _view.OnPressed((long)(nint)t.Handle, new Point(p.X, p.Y));
            }
        }
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            foreach (UITouch t in touches)
            {
                var p = t.LocationInView(_ui);
                _view.OnMoved((long)(nint)t.Handle, new Point(p.X, p.Y));
            }
        }
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            foreach (UITouch t in touches)
                _view.OnReleased((long)(nint)t.Handle);
        }
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            foreach (UITouch t in touches)
                _view.OnReleased((long)(nint)t.Handle);
        }

    }
}
