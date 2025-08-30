// Shared: TouchSurface.cs
namespace FingerDice.Utilities.Handlers;
public class TouchSurface : ContentView
{
    public event Action<long, Point>? Pressed;
    public event Action<long, Point>? Moved;
    public event Action<long>? Released;
    public void OnPressed(long id, Point p) => Pressed?.Invoke(id, p);
    public void OnMoved(long id, Point p) => Moved?.Invoke(id, p);
    public void OnReleased(long id) => Released?.Invoke(id);
}
