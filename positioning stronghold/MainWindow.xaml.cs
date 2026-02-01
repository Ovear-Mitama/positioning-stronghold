using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using positioning_stronghold.Models;
using positioning_stronghold.Pages;

namespace positioning_stronghold
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ClipboardMonitor _clipboardMonitor;
        private int _nextId = 1;
        private string _lastClipboardText = "";
        private Stack<UndoAction> _undoStack = new Stack<UndoAction>();
        private Stack<UndoAction> _redoStack = new Stack<UndoAction>();
        private SettingsWindow _settingsWindow;

        public ObservableCollection<MeasurementPoint> MeasurementPoints { get; set; }
        private string _resultX;
        public string ResultX
        {
            get => _resultX;
            set
            {
                _resultX = value;
                OnPropertyChanged(nameof(ResultX));
            }
        }
        private string _resultZ;
        public string ResultZ
        {
            get => _resultZ;
            set
            {
                _resultZ = value;
                OnPropertyChanged(nameof(ResultZ));
            }
        }
        private string _resultXY;
        public string ResultXY
        {
            get => _resultXY;
            set
            {
                _resultXY = value;
                OnPropertyChanged(nameof(ResultXY));
            }
        }
        private string _pointCount;
        public string PointCount
        {
            get => _pointCount;
            set
            {
                _pointCount = value;
                OnPropertyChanged(nameof(PointCount));
            }
        }
        private string _averageError;
        public string AverageError
        {
            get => _averageError;
            set
            {
                _averageError = value;
                OnPropertyChanged(nameof(AverageError));
            }
        }
        private string _distance;
        public string Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MeasurementPoints = new ObservableCollection<MeasurementPoint>();
            InitializeClipboardMonitor();
            LoadSettings();
            UpdateStatus();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Close();
            }
            SaveSettings();
        }

        private void InitializeClipboardMonitor()
        {
            Loaded += (s, e) =>
            {
                _clipboardMonitor = new ClipboardMonitor(this, OnClipboardChanged);
                LoadWindowPosition();
            };
            LocationChanged += (s, e) =>
            {
                SaveWindowPosition();
            };
        }

        private void LoadWindowPosition()
        {
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    var windowLeft = key.GetValue("WindowLeft");
                    var windowTop = key.GetValue("WindowTop");

                    if (windowLeft != null && double.TryParse(windowLeft.ToString(), out var left))
                    {
                        Left = left;
                    }

                    if (windowTop != null && double.TryParse(windowTop.ToString(), out var top))
                    {
                        Top = top;
                    }
                    key.Close();
                }
            }
            catch
            {
            }
        }

        private void SaveWindowPosition()
        {
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    key.SetValue("WindowLeft", Left);
                    key.SetValue("WindowTop", Top);
                    key.Close();
                }
            }
            catch
            {
            }
        }

        private void LoadSettings()
        {
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    var measurementPoints = key.GetValue("MeasurementPoints");
                    var nextId = key.GetValue("NextId");
                    var topmost = key.GetValue("Topmost");
                    var transparent = key.GetValue("Transparent");

                    if (topmost == null || topmost.ToString().ToLower() == "true" || topmost.ToString() == "True")
                    {
                        Topmost = true;
                    }
                    else
                    {
                        Topmost = false;
                    }

                    if (transparent != null && (transparent.ToString().ToLower() == "true" || transparent.ToString() == "True"))
                    {
                        Opacity = 0.8;
                    }
                    else
                    {
                        Opacity = 1.0;
                    }

                    if (measurementPoints != null && !string.IsNullOrWhiteSpace(measurementPoints.ToString()))
                    {
                        var pointsData = measurementPoints.ToString().Split(new[] { '|' });
                        foreach (var line in pointsData)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var parts = line.Split(',');
                                if (parts.Length >= 6)
                                {
                                    var point = new MeasurementPoint(
                                        int.Parse(parts[0]),
                                        double.Parse(parts[1]),
                                        double.Parse(parts[2]),
                                        double.Parse(parts[3]),
                                        double.Parse(parts[4]),
                                        double.Parse(parts[5]),
                                        parts[6]
                                    );
                                    MeasurementPoints.Add(point);
                                }
                            }
                        }
                    }
                    if (nextId != null && int.TryParse(nextId.ToString(), out var id))
                    {
                        _nextId = id;
                    }
                    key.Close();
                }
            }
            catch
            {
                Topmost = true;
                Opacity = 1.0;
            }
        }

        private void SaveSettings()
        {
            try
            {
                var pointsList = new List<string>();
                foreach (var p in MeasurementPoints)
                {
                    pointsList.Add($"{p.Id},{p.X},{p.Y},{p.Z},{p.Yaw},{p.Pitch},{p.Dimension}");
                }
                var key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    key.SetValue("WindowLeft", Left);
                    key.SetValue("WindowTop", Top);
                    key.SetValue("MeasurementPoints", string.Join("|", pointsList));
                    key.SetValue("NextId", _nextId);
                    key.SetValue("Topmost", Topmost);
                    key.SetValue("Transparent", Opacity < 1.0);
                    key.Close();
                }
            }
            catch
            {
            }
        }

        private void OnClipboardChanged(string text)
        {
            if (text == _lastClipboardText)
                return;

            _lastClipboardText = text;

            var point = CommandParser.ParseCommand(text);
            if (point != null)
            {
                AddMeasurementPoint(point);
            }
            else
            {
            }
        }

        private void AddMeasurementPoint(MeasurementPoint point)
        {
            if (MeasurementPoints.Count >= 10)
            {
                Status = "已达到最大测量点数量（10个）";
                return;
            }

            if (!CommandParser.IsValidAngle(point.Pitch))
            {
                Status = $"垂直角度 {point.Pitch:F1}° 无效，必须在 -90 到 0 度之间";
                return;
            }

            var undoAction = new UndoAction
            {
                Type = UndoAction.ActionType.Add,
                Points = new List<MeasurementPoint>(MeasurementPoints),
                NextId = _nextId,
                LastClipboardText = _lastClipboardText
            };
            _undoStack.Push(undoAction);
            _redoStack.Clear();
            point.Id = _nextId++;
            MeasurementPoints.Add(point);
            CalculatePosition();
            UpdateStatus();
            SaveSettings();
        }

        private void CalculatePosition()
        {
            if (MeasurementPoints.Count == 0)
            {
                ResultXY = "-";
                PointCount = "0";
                AverageError = "-";
                Distance = "-";
                Status = "等待添加测量点......";
                return;
            }

            var result = TriangulationCalculator.CalculatePosition(MeasurementPoints.ToList());
            if (result.Success)
            {
                int x = (int)Math.Round(result.X);
                int z = (int)Math.Round(result.Z);
                ResultXY = $"({x}, {z})";
                AverageError = result.Error.ToString("F2") + "°";

                double distance = 0;
                if (MeasurementPoints.Count > 0)
                {
                    var lastPoint = MeasurementPoints.Last();
                    double dx = result.X - lastPoint.X;
                    double dz = result.Z - lastPoint.Z;
                    distance = Math.Sqrt(dx * dx + dz * dz);
                }
                Distance = Math.Round(distance).ToString();
            }
            else
            {
                ResultXY = "-";
                AverageError = "-";
                Distance = "-";
                Status = result.Message;
            }
            PointCount = MeasurementPoints.Count.ToString();
        }

        private void UpdateStatus()
        {
            if (MeasurementPoints.Count == 0)
            {
                Status = "等待添加测量点......";
            }
            else if (MeasurementPoints.Count < 2)
            {
                Status = $"已添加 {MeasurementPoints.Count} 个测量点，至少需要 2 个才能计算";
            }
            else
            {
                bool allValidAngles = MeasurementPoints.All(p => CommandParser.IsValidAngle(p.Pitch));
                if (!allValidAngles)
                {
                    Status = $"已添加 {MeasurementPoints.Count} 个测量点，但垂直角度必须在 -90 到 0 度之间";
                }
                else
                {
                    Status = $"已添加 {MeasurementPoints.Count} 个测量点，计算完成";
                }
            }
        }

        private void DeletePoint_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                var point = MeasurementPoints.FirstOrDefault(p => p.Id == id);
                if (point != null)
                {
                    var undoAction = new UndoAction
                    {
                        Type = UndoAction.ActionType.Delete,
                        Points = new List<MeasurementPoint>(MeasurementPoints),
                        NextId = _nextId,
                        LastClipboardText = _lastClipboardText
                    };
                    _undoStack.Push(undoAction);
                    MeasurementPoints.Remove(point);
                    CalculatePosition();
                    UpdateStatus();
                    SaveSettings();
                }
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MeasurementPoints.Count > 0)
            {
                var undoAction = new UndoAction
                {
                    Type = UndoAction.ActionType.Reset,
                    Points = new List<MeasurementPoint>(MeasurementPoints),
                    NextId = _nextId,
                    LastClipboardText = _lastClipboardText
                };
                _undoStack.Push(undoAction);
                MeasurementPoints.Clear();
                _nextId = 1;
                _lastClipboardText = "";
                CalculatePosition();
                UpdateStatus();
                SaveSettings();
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                var undoAction = _undoStack.Pop();
                var redoAction = new UndoAction
                {
                    Type = undoAction.Type,
                    Points = new List<MeasurementPoint>(MeasurementPoints),
                    NextId = _nextId,
                    LastClipboardText = _lastClipboardText
                };
                _redoStack.Push(redoAction);
                if (undoAction.Type == UndoAction.ActionType.Delete)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in undoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = undoAction.NextId;
                    _lastClipboardText = undoAction.LastClipboardText;
                }
                else if (undoAction.Type == UndoAction.ActionType.Reset)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in undoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = undoAction.NextId;
                    _lastClipboardText = undoAction.LastClipboardText;
                }
                else if (undoAction.Type == UndoAction.ActionType.Add)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in undoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = undoAction.NextId;
                    _lastClipboardText = undoAction.LastClipboardText;
                }
                CalculatePosition();
                UpdateStatus();
                SaveSettings();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                var redoAction = _redoStack.Pop();
                var undoAction = new UndoAction
                {
                    Type = redoAction.Type,
                    Points = new List<MeasurementPoint>(MeasurementPoints),
                    NextId = _nextId,
                    LastClipboardText = _lastClipboardText
                };
                _undoStack.Push(undoAction);
                if (redoAction.Type == UndoAction.ActionType.Delete)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in redoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = redoAction.NextId;
                    _lastClipboardText = redoAction.LastClipboardText;
                }
                else if (redoAction.Type == UndoAction.ActionType.Reset)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in redoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = redoAction.NextId;
                    _lastClipboardText = redoAction.LastClipboardText;
                }
                else if (redoAction.Type == UndoAction.ActionType.Add)
                {
                    MeasurementPoints.Clear();
                    foreach (var point in redoAction.Points)
                    {
                        MeasurementPoints.Add(point);
                    }
                    _nextId = redoAction.NextId;
                    _lastClipboardText = redoAction.LastClipboardText;
                }
                CalculatePosition();
                UpdateStatus();
                SaveSettings();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MeasurementPoints.Count > 0)
            {
                var undoAction = new UndoAction
                {
                    Type = UndoAction.ActionType.Reset,
                    Points = new List<MeasurementPoint>(MeasurementPoints),
                    NextId = _nextId,
                    LastClipboardText = _lastClipboardText
                };
                _undoStack.Push(undoAction);
            }
            MeasurementPoints.Clear();
            _nextId = 1;
            _lastClipboardText = "";
            CalculatePosition();
            UpdateStatus();
            SaveSettings();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Close();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Left = Left + Width + 5;
            _settingsWindow.Top = Top + Height - 160;
            _settingsWindow.Show();
            _settingsWindow.Closed += (s, args) => _settingsWindow = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosed(EventArgs e)
        {
            _clipboardMonitor?.Dispose();
            base.OnClosed(e);
        }
    }

    public class UndoAction
    {
        public enum ActionType
        {
            Add,
            Delete,
            Reset
        }

        public ActionType Type { get; set; }
        public List<MeasurementPoint> Points { get; set; }
        public int NextId { get; set; }
        public string LastClipboardText { get; set; }
    }
}