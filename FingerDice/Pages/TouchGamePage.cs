using FingerDice.Utilities;
using FingerDice.Utilities.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace FingerDice.Pages
{
    public abstract class TouchGamePage : ContentPage
    {
        protected readonly List<GraphicsView> _pins = new();
        protected readonly Dictionary<GraphicsView, string> _animNames = new();
        protected readonly Dictionary<long, GraphicsView> _touchPins = new();

        CancellationTokenSource? _cts;
        protected bool _running;
        protected bool _finalized;
        protected int _paletteIdx;

        AbsoluteLayout _mainLayout = null!;
        TouchSurface _touchLayer = null!;
        Label _countdownLabel = null!;

        protected void Initialize(AbsoluteLayout layout, TouchSurface touch, Label countdown)
        {
            _mainLayout = layout;
            _touchLayer = touch;
            _countdownLabel = countdown;

            _touchLayer.Pressed += OnPressed;
            _touchLayer.Moved += OnMoved;
            _touchLayer.Released += OnReleased;
        }

        void OnPressed(long id, Point p)
        {
            if (_finalized)
            {
                ClearPins();
                _finalized = false;
            }

            var pin = NewPin();
            AbsoluteLayout.SetLayoutBounds(pin, new Rect(p.X - 40, p.Y - 40, 80, 80));
            AbsoluteLayout.SetLayoutFlags(pin, AbsoluteLayoutFlags.None);
            _mainLayout.Children.Add(pin);
            _pins.Add(pin);
            _touchPins[id] = pin;

            if (!_running && !_finalized)
                _ = StartCountdownAsync(GetSeconds());
        }

        void OnMoved(long id, Point p)
        {
            if (_touchPins.TryGetValue(id, out var v))
                AbsoluteLayout.SetLayoutBounds(v, new Rect(p.X - 40, p.Y - 40, 80, 80));
        }

        void OnReleased(long id)
        {
            if (_finalized)
            {
                _touchPins.Remove(id);
                return;
            }

            if (_touchPins.TryGetValue(id, out var v))
            {
                if (_animNames.TryGetValue(v, out var anim)) this.AbortAnimation(anim);
                _mainLayout.Children.Remove(v);
                _pins.Remove(v);
                _touchPins.Remove(id);
            }

            if (_pins.Count == 0 && _running && !_finalized)
            {
                _cts?.Cancel();
                _running = false;
                _countdownLabel.IsVisible = false;
                _countdownLabel.Text = string.Empty;
            }
        }

        protected GraphicsView NewPin()
        {
            double hueStep = 1.0 / Math.Max(6, (_pins.Count + 1) * 2);
            double hue = (_paletteIdx * hueStep) % 1.0;
            var c1 = Color.FromHsla(hue, 0.9, 0.5);
            var c2 = Color.FromHsla((hue + 0.15) % 1.0, 0.9, 0.5);

            _paletteIdx++;

            var drawable = new GradientCircleDrawable(c1, c2);

            var gv = new GraphicsView
            {
                Drawable = drawable,
                WidthRequest = 140,
                HeightRequest = 140,
                InputTransparent = true,
                BackgroundColor = Colors.Transparent,
                ZIndex = 2
            };

            var anim = new Animation(v => { drawable.Phase = v; gv.Invalidate(); }, 0, 1);
            var name = $"pin_anim_{Guid.NewGuid()}";
            _animNames[gv] = name;
            anim.Commit(this, name, rate: 16, length: 1500, easing: Easing.Linear, repeat: () => true);
            return gv;
        }

        protected async Task StartCountdownAsync(int seconds)
        {
            _running = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            _countdownLabel.IsVisible = true;

            try
            {
                for (int s = seconds; s >= 1; s--)
                {
                    _countdownLabel.IsVisible = true;
                    _countdownLabel.Text = s.ToString();
                    await Task.Delay(1000, ct);
                }

                _countdownLabel.Text = string.Empty;
#if DEBUG
                _countdownLabel.Text = "Ok";
#endif
                FinishRound();
                _finalized = true;
            }
            catch (TaskCanceledException) { }
            finally
            {
                _ = Task.Run(async () => { await Task.Delay(1000); });
                _running = false;
                _cts = null;
            }
        }

        protected abstract void FinishRound();

        protected virtual int GetSeconds() => 5;

        protected void ClearPins()
        {
            foreach (var v in _pins)
            {
                if (_animNames.TryGetValue(v, out var animName))
                {
                    this.AbortAnimation(animName);
                }
                _mainLayout.Children.Remove(v);
            }
            _animNames.Clear();
            _pins.Clear();
            _paletteIdx = 0;
        }
    }
}
