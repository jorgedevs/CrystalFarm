using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CrystalApp;

public class MainViewModel : INotifyPropertyChanged
{
    private TcpClient client;
    private NetworkStream stream;

    // Connectivity

    private string ipAddress;
    public string IpAddress
    {
        get => ipAddress;
        set { ipAddress = value; OnPropertyChanged(); }
    }

    private int portNumber;
    public int PortNumber
    {
        get => portNumber;
        set { portNumber = value; OnPropertyChanged(); }
    }

    private bool isConnected;
    public bool IsConnected
    {
        get => isConnected;
        set { isConnected = value; OnPropertyChanged(); }
    }

    private string connectionStatus;
    public string ConnectionStatus
    {
        get => connectionStatus;
        set { connectionStatus = value; OnPropertyChanged(); }
    }

    public ICommand ToggleConnectionCmd { get; set; }

    // Graphs

    private ObservableCollection<ChartEntry> entries;

    private Chart sensorChart;
    public Chart SensorChart
    {
        get => sensorChart;
        set { sensorChart = value; OnPropertyChanged(); }
    }

    // Settings

    private bool isAutoMode;
    public bool IsAutoMode
    {
        get => isAutoMode;
        set { isAutoMode = value; OnPropertyChanged(); }
    }

    private float temperatureSetPoint;
    public float TemperatureSetPoint
    {
        get => temperatureSetPoint;
        set { temperatureSetPoint = value; OnPropertyChanged(); }
    }

    private float temperatureValue;
    public float TemperatureValue
    {
        get => temperatureValue;
        set { temperatureValue = value; OnPropertyChanged(); }
    }

    private float manualControlOutput;
    public float ManualControlOutput
    {
        get => manualControlOutput;
        set { manualControlOutput = value; OnPropertyChanged(); }
    }

    private string settingsStatus;
    public string SettingsStatus
    {
        get => settingsStatus;
        set { settingsStatus = value; OnPropertyChanged(); }
    }

    public ICommand SendSettingsCmd { get; set; }

    public MainViewModel()
    {
        client = new TcpClient();
        IpAddress = Preferences.Get("IPAddress", "192.168.1.1");
        PortNumber = Preferences.Get("PortNumber", 0);
        ConnectionStatus = string.Empty;

        entries = new ObservableCollection<ChartEntry>();
        TemperatureValue = 0;

        IsAutoMode = Preferences.Get("OperationMode", false);
        TemperatureSetPoint = Preferences.Get("TemperatureSetPoint", 0f);
        SettingsStatus = string.Empty;

        SendSettingsCmd = new Command(SendSettings);
        ToggleConnectionCmd = new Command(ToggleConnection);

        _ = PollSensor();
    }

    private async void SendSettings()
    {
        Preferences.Set("OperationMode", IsAutoMode);
        Preferences.Set("TemperatureSetPoint", TemperatureSetPoint);

        //string command = "" +
        //    (IsAutoMode ? "1.0," : "0.0,") +
        //    $"{TemperatureSetPoint}," +
        //    $"{ManualControlOutput}";
        //byte[] commandBytes = Encoding.ASCII.GetBytes(command);
        //await stream.WriteAsync(commandBytes, 0, commandBytes.Length);
        //Console.WriteLine($"Sent: {command}");

        SettingsStatus = "Settings Sent!";
        await Task.Delay(3000);
        SettingsStatus = string.Empty;
    }

    private async void ToggleConnection()
    {
        try
        {
            if (!IsConnected)
            {
                Preferences.Set("IPAddress", IpAddress);
                Preferences.Set("PortNumber", PortNumber);

                //await client.ConnectAsync(IpAddress, PortNumber);
                //stream = client.GetStream();

                IsConnected = true;
                ConnectionStatus = "Connected!";
                await Task.Delay(3000);
                ConnectionStatus = string.Empty;
            }
            else
            {
                //stream.Close();
                entries.Clear();

                IsConnected = false;
                ConnectionStatus = "Disconnected!";
                await Task.Delay(3000);
                ConnectionStatus = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            IsConnected = false;
        }
    }

    private async Task PollSensor()
    {
        Random random = new Random();

        while (true)
        {
            if (IsConnected)
            {
                //byte[] buffer = new byte[1024];
                //int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                //string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                //Console.WriteLine($"Received: {response}");

                //var values = response.Split(",");
                //IsAutoMode = float.Parse(values[0]) == 1;
                //TemperatureSetPoint = float.Parse(values[1]);
                //ManualControlOutput = float.Parse(values[2]);
                //TemperatureValue = float.Parse(values[3]);
                //UpdateGraph(TemperatureValue);

                UpdateGraph(random.Next(20, 40));
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private void UpdateGraph(float temperatureValue)
    {
        TemperatureValue = temperatureValue;

        if (entries.Count > 120)
        {
            entries.RemoveAt(0);
        }

        entries.Add(new ChartEntry(temperatureValue)
        {
            Color = temperatureValue switch
            {
                >= 30 => SKColor.Parse("#FC0103"),
                >= 25 and < 30 => SKColor.Parse("#B10130"),
                >= 20 and < 25 => SKColor.Parse("#81014E"),
                >= 15 and < 20 => SKColor.Parse("#4F016C"),
                < 15 => SKColor.Parse("#090197"),
            }
        });

        if (entries.Count > 0)
        {
            var yAxisPaint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var yAxisTextPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 24,
                IsAntialias = true
            };

            SensorChart = new LineChart
            {
                Entries = entries,
                BackgroundColor = SKColor.FromHsl(240.0f, 28.57f, 16.47f),
                LineMode = LineMode.Spline,
                LineSize = 8,
                LabelTextSize = 36,
                YAxisTextPaint = yAxisTextPaint,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                YAxisLinesPaint = yAxisPaint,
                ShowYAxisLines = true,
                ShowYAxisText = true,
                SerieLabelTextSize = 20,
                IsAnimated = false
            };
        }
    }

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}