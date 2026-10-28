using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LibVLCSharp.Shared;

namespace EchoDeck
{
    public partial class EqualizerWindow : Window
    {
        private readonly MediaPlayer _player;
        private Equalizer _equalizer;
        private Slider[] _sliders = Array.Empty<Slider>();
        private TextBlock[] _valueLabels = Array.Empty<TextBlock>();
        private bool _loading;

        public EqualizerWindow(MediaPlayer player, Equalizer? currentEq)
        {
            InitializeComponent();
            WindowHelper.AttachChrome(this);
            _player = player;
            _equalizer = currentEq ?? new Equalizer();
            BuildSliders();
            LoadPresets();
        }

        private void BuildSliders()
        {
            int bands = (int)_equalizer.BandCount;
            _sliders = new Slider[bands];
            _valueLabels = new TextBlock[bands];
            var grid = new UniformGrid { Columns = bands };
            
            for (int i = 0; i < bands; i++)
            {
                var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 8, 0) };
                
                var valLabel = new TextBlock { Text = "0 dB", FontSize = 11, Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
                _valueLabels[i] = valLabel;

                var slider = new Slider { Orientation = Orientation.Vertical, Minimum = -20, Maximum = 20, Value = 0, Height = 180, IsMoveToPointEnabled = true, Style = (Style)FindResource("FigmaSlider") };
                slider.ValueChanged += (s, e) => Slider_Changed(i, e.NewValue);
                _sliders[i] = slider;

                var freqLabel = new TextBlock { Text = FormatFreq(_equalizer.BandFrequency((uint)i)), FontSize = 11, Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) };

                panel.Children.Add(valLabel);
                panel.Children.Add(slider);
                panel.Children.Add(freqLabel);
                grid.Children.Add(panel);
            }
            SliderHost.Children.Add(grid);
        }

        private string FormatFreq(float hz) => hz >= 1000 ? $"{hz / 1000:0.#} kHz" : $"{hz} Hz";

        private void LoadPresets()
        {
            _loading = true;
            PresetCombo.Items.Add("Custom");
            for (uint i = 0; i < _equalizer.PresetCount; i++)
                PresetCombo.Items.Add(_equalizer.PresetName(i));
            
            PresetCombo.SelectedIndex = 0; 
            EqToggle.IsChecked = true; 
            _loading = false;
            
            ApplyPreset(0); 
        }

        private void PresetCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || PresetCombo.SelectedIndex <= 0) return;
            ApplyPreset((uint)(PresetCombo.SelectedIndex - 1));
        }

        private void ApplyPreset(uint presetIndex)
        {
            _loading = true;
            _equalizer = new Equalizer(presetIndex);
            _player.SetEqualizer(_equalizer);
            for (int i = 0; i < _sliders.Length; i++)
            {
                float amp = _equalizer.Amp((uint)i); // <-- FIXED: Amp instead of GetAmp
                _sliders[i].Value = amp;
                _valueLabels[i].Text = $"{(amp > 0 ? "+" : "")}{amp:0.0} dB";
            }
            _loading = false;
        }

        private void Slider_Changed(int band, double value)
        {
            if (_loading) return;
            
            if (EqToggle.IsChecked != true)
            {
                EqToggle.IsChecked = true;
                EqToggle_Click(this, new RoutedEventArgs());
            }

            _valueLabels[band].Text = $"{(value > 0 ? "+" : "")}{value:0.0} dB";
            PresetCombo.SelectedIndex = 0;

            // FIXED: SetAmp takes (amp, band) not (band, amp)
            _equalizer.SetAmp((float)value, (uint)band); 
            _player.SetEqualizer(_equalizer);

            if (LinkSlidersCheck.IsChecked == true)
            {
                for (int i = 1; i <= 2; i++)
                {
                    if (band - i >= 0) AdjustSlider(band - i, value, i);
                    if (band + i < _sliders.Length) AdjustSlider(band + i, value, i);
                }
            }
        }

        private void AdjustSlider(int index, double baseValue, int distance)
        {
            double target = baseValue * (1.0 - (distance * 0.3));
            if (target > 20) target = 20; 
            if (target < -20) target = -20;
            _loading = true;
            _sliders[index].Value = target;
            _valueLabels[index].Text = $"{(target > 0 ? "+" : "")}{target:0.0} dB";
            _equalizer.SetAmp((float)target, (uint)index); // <-- FIXED
            _loading = false;
        }

        private void EqToggle_Click(object sender, RoutedEventArgs e)
        {
            // If the toggle is on, apply the equalizer. If off, disable it.
            _player.SetEqualizer(EqToggle.IsChecked == true ? _equalizer : null);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}