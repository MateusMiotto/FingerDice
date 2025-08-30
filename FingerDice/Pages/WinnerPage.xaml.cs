using Microsoft.Maui.Layouts;
using System.Net.NetworkInformation;

namespace FingerDice.Pages
{
    public partial class WinnerPage : ContentPage
    {
        readonly List<GraphicsView> _pins = new();
        CancellationTokenSource? _cts;
        bool _running;
        bool _finalized; // indica que a rodada terminou
        int _groupCount = 2; // padrão
        int _paletteIdx = 0;

        // controle das animações por círculo
        readonly Dictionary<GraphicsView, string> _animNames = new();


        readonly Dictionary<long, GraphicsView> _touchPins = new();
        public WinnerPage()
        {
            InitializeComponent();

            TouchLayer.Pressed += (id, p) =>
            {
                if (_finalized)
                {
                    ClearPins();
                    _finalized = false;
                }

                var pin = NewPin();
                AbsoluteLayout.SetLayoutBounds(pin, new Rect(p.X - 40, p.Y - 40, 80, 80));
                AbsoluteLayout.SetLayoutFlags(pin, AbsoluteLayoutFlags.None);
                MainLayout.Children.Add(pin);
                _pins.Add(pin);
                _touchPins[id] = pin;

                // Só inicia o contador se não estiver rodando e não estiver finalizado
                if (!_running && !_finalized && _pins.Count >= 2)
                {
                    _ = StartCountdownAsync(5);
                }
            };

            TouchLayer.Moved += (id, p) =>
            {
                if (_touchPins.TryGetValue(id, out var v))
                    AbsoluteLayout.SetLayoutBounds(v, new Rect(p.X - 40, p.Y - 40, 80, 80));
            };

            TouchLayer.Released += id =>
            {
                if (_finalized)
                {
                    _touchPins.Remove(id); // Apenas remove o vínculo do toque
                    return;
                }

                if (_touchPins.TryGetValue(id, out var v))
                {
                    if (_animNames.TryGetValue(v, out var anim)) this.AbortAnimation(anim);
                    MainLayout.Children.Remove(v);
                    _pins.Remove(v);
                    _touchPins.Remove(id);
                }

                if (_pins.Count <= 1 && _running && !_finalized)
                {
                    _cts?.Cancel();
                    _running = false;
                    CountdownLabel.IsVisible = false;
                    CountdownLabel.Text = "";
                }
            };
        }

        private void OnLayoutTapped(object sender, TappedEventArgs e)
        {
            // se a rodada anterior terminou, limpar tudo e começar outra
            if (_finalized)
            {
                ClearPins();
                _finalized = false;
            }

            var p = e.GetPosition(MainLayout);
            if (p is null) return;

            var circle = NewPin();
            AbsoluteLayout.SetLayoutBounds(circle, new Rect(p.Value.X - 30, p.Value.Y - 30, 60, 60));
            AbsoluteLayout.SetLayoutFlags(circle, AbsoluteLayoutFlags.None);
            MainLayout.Children.Add(circle);
            _pins.Add(circle);

            if (!_running)
                _ = StartCountdownAsync(5);
        }

        GraphicsView NewPin()
        {
            // Gera gradientes mais distintos usando espaçamento maior no hue
            double hueStep = 1.0 / Math.Max(6, _groupCount * 2); // Mais espaçado
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



        async Task StartCountdownAsync(int seconds)
        {
            _running = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            CountdownLabel.IsVisible = true;

            try
            {
                for (int s = seconds; s >= 1; s--)
                {
                    CountdownLabel.IsVisible = true;
                    CountdownLabel.Text = s.ToString();
                    await Task.Delay(1000, ct);
                }


                CountdownLabel.Text = "";
#if DEBUG
                CountdownLabel.Text = "Ok";
#endif
                SelectWinner();
                _finalized = true;
            }
            catch (TaskCanceledException) { }
            finally
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                });
                _running = false;
                _cts = null;
            }
        }
        void SelectWinner()
        {
            if (_pins.Count == 0) return;

            // Embaralha a lista algumas vezes para aumentar a aleatoriedade
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var shuffled = _pins.OrderBy(_ => rnd.Next()).ToList();
            shuffled = shuffled.OrderBy(_ => rnd.Next()).ToList();

            var winnerIndex = rnd.Next(shuffled.Count);
            var winner = shuffled[winnerIndex];

            // Esconde os outros círculos com fade-out
            foreach (var pin in _pins)
            {
                if (pin != winner)
                {
                    pin.FadeTo(0, 200, Easing.CubicIn).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() => pin.IsVisible = false);
                    });
                }
            }

            // Para animação anterior, se houver
            if (_animNames.TryGetValue(winner, out var animName))
            {
                this.AbortAnimation(animName);
                _animNames.Remove(winner);
            }

            // Troca a cor para verde e faz animação de crescimento e brilho
            winner.Drawable = new SolidCircleDrawable(Colors.LimeGreen);
            winner.Invalidate();
            winner.InputTransparent = true;

            // Animação: cresce, brilha e volta ao normal
            winner.Scale = 1;
            winner.Opacity = 1;
            var grow = winner.ScaleTo(1.5, 300, Easing.CubicOut);
            var glow = winner.FadeTo(1, 300, Easing.CubicOut);
            Task.WhenAll(grow, glow).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await winner.ScaleTo(1, 300, Easing.CubicIn);
                });
            });

            _finalized = true;
        }


        void ClearPins()
        {
            CountdownLabel.Text = "";
            foreach (var v in _pins)
            {
                if (_animNames.TryGetValue(v, out var animName))
                {
                    this.AbortAnimation(animName);
                }
                MainLayout.Children.Remove(v);
            }
            _animNames.Clear();
            _pins.Clear();
            _paletteIdx = 0;
        }
    }
}