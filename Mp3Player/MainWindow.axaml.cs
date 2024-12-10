using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Classic.Avalonia.Theme;
using NAudio.Utils;
using NAudio.Wave;

namespace Mp3Player;

public partial class MainWindow : ClassicWindow
{
    private WaveOutEvent _waveOut = new();
    private AudioFileReader? _audioFileReader;
    private bool _isSliderControlled;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void PlayFile(string filename)
    {
        _audioFileReader = new AudioFileReader(filename);
        _waveOut.Init(_audioFileReader);
        _waveOut.Play();
    }
    private async void MiOpenMp3_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var mp3File = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions(){AllowMultiple = false, Title = "Select MP3 File", FileTypeFilter = [
            new FilePickerFileType("MP3 File")
            {
                Patterns = ["*.mp3"]
            }
        ]});
        
        PlayFile(mp3File[0].Path.AbsolutePath);
        SliderSongVolume.Value = 100;
        SliderSongPosition.Maximum = _audioFileReader!.TotalTime.TotalSeconds;
        SliderSongVolume.Value = _audioFileReader.Volume * 100;
        LabelTotalTime.Content = _audioFileReader.TotalTime.ToString(@"hh\:mm\:ss");
        BtnPlayMusic.IsEnabled = true;
        BtnPauseMusic.IsEnabled = true;
        BtnStopMusic.IsEnabled = true;
        
        await PlayMusic();
    }

    private async Task PlayMusic()
    {
        
        while (_waveOut.PlaybackState == PlaybackState.Playing && !_isSliderControlled)
        {
            await Task.Delay(1);
            LabelTimeElapsed.Content = _waveOut.GetPositionTimeSpan().ToString(@"hh\:mm\:ss");
            SliderSongPosition.Value = _waveOut.GetPositionTimeSpan().TotalSeconds;
        }
    }

    private void BtnStopMusic_OnClick(object? sender, RoutedEventArgs e)
    {
        _waveOut.Stop();
        _audioFileReader!.Position = 0;
    }

    private async void BtnPlayMusic_OnClick(object? sender, RoutedEventArgs e)
    {
        _waveOut.Play();
        await PlayMusic();
    }

    private void BtnPauseMusic_OnClick(object? sender, RoutedEventArgs e)
    {
        _waveOut.Pause();
    }

    private void SliderSongVolume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _waveOut.Volume = Convert.ToSingle(e.NewValue / 100);
        LabelMusicVolume.Text = MathF.Round(_waveOut.Volume * 100, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
    }

    private void SliderSongVolume_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var slider = sender as Slider;
        slider!.Value += e.Delta.Y * 4;

    }

    private void SliderSongDuration_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        
    }

    private void SliderSongDuration_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isSliderControlled = true;
    }

    private void SliderSongDuration_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isSliderControlled = false;
    }

    private void SliderSongPosition_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }
}