using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace FingerDice.Pages
{
    public partial class GroupPage : ContentPage
    {
        readonly List<GraphicsView> _pins = new();
        CancellationTokenSource? _cts;
        bool _running;
        bool _finalized; // novo: indica que a rodada terminou
        int _groupCount = 2; // padrão

        // paletas alternadas para cada círculo
        readonly (Color a, Color b)[] _palettes = new (Color, Color)[]
        {
            (Colors.HotPink, Colors.OrangeRed),
            (Colors.DodgerBlue, Colors.MediumPurple),
            (Colors.LimeGreen, Colors.Teal),
            (Colors.Gold, Colors.Orange),
            (Colors.DeepSkyBlue, Colors.Turquoise),
        };
        int _paletteIdx = 0;

        // controle das animações por círculo
        readonly Dictionary<GraphicsView, string> _animNames = new();


        readonly Dictionary<long, GraphicsView> _touchPins = new();

        public GroupPage()
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
                if (!_running && !_finalized)
                    _ = StartCountdownAsync(5);
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

                if (_pins.Count == 0 && _running && !_finalized)
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
                _ = StartCountdownAsync(GetSeconds());
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
                SplitIntoGroups();
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
        void SplitIntoGroups()
        {
            if (_pins.Count == 0) return;

            Color[] colors;
            if (_groupCount == 2)
            {
                // Verde de confirmação e vermelho de atenção
                colors = new[] { Colors.LimeGreen, Colors.Red };
            }
            else
            {
                // Paleta dinâmica para mais grupos
                colors = Enumerable.Range(0, _groupCount)
                    .Select(i => Color.FromHsla(i / (double)_groupCount, 0.75, 0.5))
                    .ToArray();
            }

            var rnd = new Random();
            var shuffled = _pins.OrderBy(_ => rnd.Next()).ToList();

            // distribuição balanceada por round-robin
            for (int i = 0; i < shuffled.Count; i++)
            {
                var v = shuffled[i];
                var groupIdx = i % _groupCount;
                var finalColor = colors[groupIdx];

                if (_animNames.TryGetValue(v, out var animName))
                {
                    this.AbortAnimation(animName);
                    _animNames.Remove(v);
                }

                v.Drawable = new SolidCircleDrawable(finalColor);
                v.Invalidate();
            }
            _finalized = true;
        }

        void ClearPins()
        {
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

        async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            var input = await DisplayPromptAsync("Configurar grupos", "Quantidade de grupos:",
                accept: "OK", cancel: "Cancelar",
                initialValue: _groupCount.ToString(), keyboard: Keyboard.Numeric);

            if (int.TryParse(input, out var n) && n >= 2 && n <= 12)
            {
                _groupCount = n;
                // feedback opcional
                Title = $"Grupos ({_groupCount})";
                // se quiser reiniciar imediatamente:
                // ClearPins(); _finalized = false;
            }
            else if (input is not null)
            {
                await DisplayAlert("Inválido", "Informe um número entre 2 e 12.", "OK");
            }
        }

        int GetSeconds()
        {
            if (_groupCount >= 4)
            {
                return 5;
            }
            else
            {
                return 10;
            }
        }

    }

    public class CircleDrawable : IDrawable
    {
        public Color Color { get; set; }
        public CircleDrawable(Color color) => Color = color;

        public void Draw(ICanvas canvas, RectF r)
        {
            canvas.Antialias = true;
            canvas.StrokeColor = Color;
            canvas.StrokeSize = 6;
            canvas.DrawCircle(r.Center.X, r.Center.Y, r.Width / 2 - 3);
        }
    }
}
