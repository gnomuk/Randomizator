using Gma.System.MouseKeyHook;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WindowsInput;
using WindowsInput.Native;

namespace Randomizator
{
    public partial class MainWindow : Window
    {
        #region Declarations
        List<ScriptEntry> enabledScripts = new List<ScriptEntry>();
        Settings settings = new Settings();
        ScriptEntry selectedScript;
        ScriptCollection viewModel;

        //Эмуляция нажатий
        private InputSimulator inputSimulator;
        private IKeyboardMouseEvents _globalHook;

        // Рулетка сценариев
        private bool _rouletteStarted = false;
        private CancellationTokenSource _cts;
        private Task _runningTask;
        private bool _enabledScriptsShuffled = false;

        //Директории
        readonly string MYDIRECTORY = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator";
        readonly string MYSCRIPTCONFIG = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator/config.json";
        readonly string MYSETTINGSCONFIG = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator/settings.json";
        readonly string MYLOGSFOLDER = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator/Logs";

        // Словари
        readonly Dictionary<string, Keys> winFormsKeycodes = new Dictionary<string, Keys>
        {
            {"A", Keys.A},
            {"B", Keys.B},
            {"C", Keys.C},
            {"D", Keys.D},
            {"E", Keys.E},
            {"F", Keys.F},
            {"G", Keys.G},
            {"H", Keys.H},
            {"I", Keys.I},
            {"J", Keys.J},
            {"K", Keys.K},
            {"L", Keys.L},
            {"M", Keys.M},
            {"N", Keys.N},
            {"O", Keys.O},
            {"P", Keys.P},
            {"Q", Keys.Q},
            {"R", Keys.R},
            {"S", Keys.S},
            {"T", Keys.T},
            {"U", Keys.U},
            {"V", Keys.V},
            {"W", Keys.W},
            {"X", Keys.X},
            {"Y", Keys.Y},
            {"Z", Keys.Z},
            {"F1", Keys.F1},
            {"F2", Keys.F2},
            {"F3", Keys.F3},
            {"F4", Keys.F4},
            {"F5", Keys.F5},
            {"F6", Keys.F6},
            {"F7", Keys.F7},
            {"F8", Keys.F8},
            {"F9", Keys.F9},
            {"F10", Keys.F10},
            {"F11", Keys.F11},
            {"F12", Keys.F12},
            {"Space", Keys.Space},
            {"LeftCtrl", Keys.LControlKey},
            {"RightCtrl", Keys.RControlKey},
            {"LeftShift", Keys.LShiftKey},
            {"RightShift", Keys.RShiftKey},
            {"Enter", Keys.Enter},
            {"Escape", Keys.Escape},
            {"Backspace", Keys.Back},
            {"Tab", Keys.Tab},
            {"Left", Keys.Left},
            {"Right", Keys.Right},
            {"Up", Keys.Up},
            {"Down", Keys.Down},
            {"Home", Keys.Home},
            {"End", Keys.End},
            {"PageUp", Keys.PageUp},
            {"PageDown", Keys.PageDown},
            {"D1", Keys.D1},
            {"D2", Keys.D2},
            {"D3", Keys.D3},
            {"D4", Keys.D4},
            {"D5", Keys.D5},
            {"D6", Keys.D6},
            {"D7", Keys.D7},
            {"D8", Keys.D8},
            {"D9", Keys.D9},
            {"D0", Keys.D0}
        };

        readonly Dictionary<string, VirtualKeyCode> virtualKeycodes = new Dictionary<string, VirtualKeyCode>
        {
            {"D0", VirtualKeyCode.VK_0},
            {"D1", VirtualKeyCode.VK_1},
            {"D2", VirtualKeyCode.VK_2},
            {"D3", VirtualKeyCode.VK_3},
            {"D4", VirtualKeyCode.VK_4},
            {"D5", VirtualKeyCode.VK_5},
            {"D6", VirtualKeyCode.VK_6},
            {"D7", VirtualKeyCode.VK_7},
            {"D8", VirtualKeyCode.VK_8},
            {"D9", VirtualKeyCode.VK_9},
            {"A", VirtualKeyCode.VK_A},
            {"B", VirtualKeyCode.VK_B},
            {"C", VirtualKeyCode.VK_C},
            {"D", VirtualKeyCode.VK_D},
            {"E", VirtualKeyCode.VK_E},
            {"F", VirtualKeyCode.VK_F},
            {"G", VirtualKeyCode.VK_G},
            {"H", VirtualKeyCode.VK_H},
            {"I", VirtualKeyCode.VK_I},
            {"J", VirtualKeyCode.VK_J},
            {"K", VirtualKeyCode.VK_K},
            {"L", VirtualKeyCode.VK_L},
            {"M", VirtualKeyCode.VK_M},
            {"N", VirtualKeyCode.VK_N},
            {"O", VirtualKeyCode.VK_O},
            {"P", VirtualKeyCode.VK_P},
            {"Q", VirtualKeyCode.VK_Q},
            {"R", VirtualKeyCode.VK_R},
            {"S", VirtualKeyCode.VK_S},
            {"T", VirtualKeyCode.VK_T},
            {"U", VirtualKeyCode.VK_U},
            {"V", VirtualKeyCode.VK_V},
            {"W", VirtualKeyCode.VK_W},
            {"X", VirtualKeyCode.VK_X},
            {"Y", VirtualKeyCode.VK_Y},
            {"Z", VirtualKeyCode.VK_Z},
            {"Insert", VirtualKeyCode.INSERT},
            {"Backspace", VirtualKeyCode.BACK},
            {"Tab", VirtualKeyCode.TAB},
            {"Enter", VirtualKeyCode.RETURN},
            {"Escape", VirtualKeyCode.ESCAPE},
            {"Space", VirtualKeyCode.SPACE},
            {"Left", VirtualKeyCode.LEFT},
            {"Right", VirtualKeyCode.RIGHT},
            {"Up", VirtualKeyCode.UP},
            {"Down", VirtualKeyCode.DOWN},
            {"Home", VirtualKeyCode.HOME},
            {"End", VirtualKeyCode.END},
            {"PageUp", VirtualKeyCode.PRIOR},
            {"PageDown", VirtualKeyCode.NEXT},
            {"LeftCtrl", VirtualKeyCode.LCONTROL},
            {"RightCtrl", VirtualKeyCode.RCONTROL},
            {"LeftShift", VirtualKeyCode.LSHIFT},
            {"RightShift", VirtualKeyCode.RSHIFT}
        };

        readonly Dictionary<string, MouseButtons> mouseKeycodes = new Dictionary<string, MouseButtons>
        {
            {"Left", MouseButtons.Left},
            {"Right", MouseButtons.Right},
            {"Middle", MouseButtons.Middle},
            {"XButton1", MouseButtons.XButton1},
            {"XButton2", MouseButtons.XButton2}
        };

        readonly string[] isThisMouseButton = { "Left", "Right", "Middle", "Up", "Down", "XButton1", "XButton2" };

        readonly Dictionary<string, Func<CancellationToken, ScriptCollection, Task>> allFunctions;

        List<Func<CancellationToken, ScriptCollection, Task>> enabledFunctions = new List<Func<CancellationToken, ScriptCollection, Task>>();
        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;

            if (!Directory.Exists(MYDIRECTORY)) Directory.CreateDirectory(MYDIRECTORY);
            if (!Directory.Exists(MYLOGSFOLDER)) Directory.CreateDirectory(MYLOGSFOLDER);

            this.DataContext = new ScriptCollection();

            allFunctions = new Dictionary<string, Func<CancellationToken, ScriptCollection, Task>>
            {
                { "Drop", Drop_Script },
                { "Jump", Jump_Script },
                { "Inspect", Inspect_Script },
                { "Reload", Reload_Script },
                { "Interact", Interact_Script },
                { "Fire", Fire_Script },
                { "SFire", SFire_Script },
                { "Voice", Voice_Script },
                { "Crouch", Crouch_Script },
                { "Sneak", Sneak_Script },
                { "WeaponSwitch", WeaponSwitch_Script },
                { "WalkingSomewhere", WalkingSomewhere_Script },
                { "TurnAround", TurnAround_Script },
                { "Ping", Ping_Script },
                { "DisablingTheKeyboard", DisablingTheKeyboard_Script },
                { "DisablingTheMouse", DisablingTheMouse_Script },
                { "DisableShooting", DisableShooting_Script },
                { "DisableRotation", DisableRotation_Script },
                { "InvertControl", InvertControl_Script },
            };
        }

        #region Window Handlers
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = (ScriptCollection)this.DataContext;
            CreateScripts(viewModel);
            if (File.Exists(MYSETTINGSCONFIG)) LoadSettings(); else CreateSettings();
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { DragMove(); }

        private void CloseWindow(object sender, RoutedEventArgs e) { Close(); }

        private void MinimizeWindow(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
        
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            HideAllGrids();
            Settings_Label.Content = "Конфигурация рулетки";
            Settings_Grid.Visibility = Visibility.Visible;
            WindowSettings.Visibility = Visibility.Visible;
            
            if (File.Exists(MYSETTINGSCONFIG))
            {
                LoadSettings();
                return;
            }
            CreateSettings();
        }

        private void StartScriptRoulette(object sender, RoutedEventArgs e)
        {
            StartRoulette();
        }

        private void PinWindow(object sender, RoutedEventArgs e)
        {
            if (this.Topmost)
            {
                this.Topmost = false;
                PinButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/grayPin.png"));
                return;
            }
            this.Topmost = true;
            PinButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/bluePin.png"));
        }
        #endregion

        #region Other Handlers
        private void ApplyChanges(object sender, RoutedEventArgs e)
        {
            viewModel.SaveToJson(MYSCRIPTCONFIG);
            CollectFunctions(viewModel);
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
        }

        private void ScriptSettings(object sender, RoutedEventArgs e)
        {
            HideAllGrids();
            Settings_Grid.Visibility = Visibility.Visible;
            ScriptSettings_Grid.Visibility = Visibility.Visible;

            var button = sender as System.Windows.Controls.Button;
            var item = button?.DataContext as ScriptEntry;
            selectedScript = item;
            scriptDescription.Text = item.Info.Description;

            Settings_Label.Content = item.Info.Name;

            if (item.Info.Config.Int != 0) 
            {
                IntInput_StackPanel.Visibility = Visibility.Visible;
                Int_TextBox.Text = item.Info.Config.Int.ToString();
            }
            else IntInput_StackPanel.Visibility = Visibility.Collapsed;

            if (item.Info.Config.Binds.ContainsKey("Bind"))
            {
                KeybindInput_StackPanel.Visibility = Visibility.Visible;
                Keybind_TextBox.Text = item.Info.Config.Binds.Values.FirstOrDefault();
            }
            else KeybindInput_StackPanel.Visibility = Visibility.Collapsed;

            if (item.ShortName == "WalkingSomewhere")
            {
                WalkingSomewhere_StackPanel.Visibility = Visibility.Visible;
                Forward_TextBox.Text = item.Info.Config.Binds["Forward"];
                Left_TextBox.Text = item.Info.Config.Binds["Left"];
                Backward_TextBox.Text = item.Info.Config.Binds["Backward"];
                Right_TextBox.Text = item.Info.Config.Binds["Right"];
            }
            else WalkingSomewhere_StackPanel.Visibility = Visibility.Collapsed;
            
            if (item.ShortName == "WeaponSwitch")
            {
                WeaponSwitch_StackPanel.Visibility = Visibility.Visible;
                if (item.Info.Config.Binds.ContainsKey("Main")) Main_TextBox.Text = item.Info.Config.Binds["Main"];
                if (item.Info.Config.Binds.ContainsKey("Secondary")) Secondary_TextBox.Text = item.Info.Config.Binds["Secondary"];
                if (item.Info.Config.Binds.ContainsKey("Knife")) Knife_TextBox.Text = item.Info.Config.Binds["Knife"];
                if (item.Info.Config.Binds.ContainsKey("Grenades")) Grenades_TextBox.Text = item.Info.Config.Binds["Grenades"];
                if (item.Info.Config.Binds.ContainsKey("Bomb")) Bomb_TextBox.Text = item.Info.Config.Binds["Bomb"];

                
            }
            else WeaponSwitch_StackPanel.Visibility = Visibility.Collapsed;

        }

        private void KeyboardInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string key = e.Key.ToString();
            if (key == "Back") { textBox.Text = ""; return; }
            textBox.Text = key;
            e.Handled = true;
        }

        private void MouseInput(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string button = e.ChangedButton.ToString();
            textBox.Text = button;
            //e.Handled = true;
        }

        private void MouseWheelInput(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (e.Delta > 0) textBox.Text = "Up";
            else textBox.Text = "Down";
        }

        private void IntInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right) e.Handled = false;
            else e.Handled = true;
        }

        private void CloseScriptSettings(object sender, RoutedEventArgs e)
        {
            Settings_Grid.Visibility = Visibility.Collapsed;
        }

        private void SaveScriptSettings(object sender, RoutedEventArgs e)
        {
            if (ScriptSettings_Grid.Visibility == Visibility.Visible)
            {
                Dictionary<string, string> binds = new Dictionary<string, string>();

                switch (selectedScript.ShortName)
                {
                    case ("WalkingSomewhere"):
                        if (selectedScript.Info.Config.Int != 0) selectedScript.Info.Config.Int = Int32.Parse(Int_TextBox.Text);
                        binds.Add("Forward", Forward_TextBox.Text);
                        binds.Add("Left", Left_TextBox.Text);
                        binds.Add("Backward", Backward_TextBox.Text);
                        binds.Add("Right", Right_TextBox.Text);
                        break;

                    case ("WeaponSwitch"):
                        if (selectedScript.Info.Config.Int != 0) selectedScript.Info.Config.Int = Int32.Parse(Int_TextBox.Text);
                        if (Main_TextBox.Text != "") binds.Add("Main", Main_TextBox.Text);
                        if (Secondary_TextBox.Text != "") binds.Add("Secondary", Secondary_TextBox.Text);
                        if (Knife_TextBox.Text != "") binds.Add("Knife", Knife_TextBox.Text);
                        if (Grenades_TextBox.Text != "") binds.Add("Grenades", Grenades_TextBox.Text);
                        if (Bomb_TextBox.Text != "") binds.Add("Bomb", Bomb_TextBox.Text);
                        break;

                    default:
                        if (selectedScript.Info.Config.Int != 0) selectedScript.Info.Config.Int = Int32.Parse(Int_TextBox.Text);
                        if (selectedScript.Info.Config.Binds.ContainsKey("Bind")) binds.Add("Bind", Keybind_TextBox.Text);
                        break;
                }
                viewModel.EditScript(selectedScript, binds);
                viewModel.SaveToJson(MYSCRIPTCONFIG);
                return;
            }
            SaveSettings();
        }
        #endregion

        #region Helpers
        private void CreateScripts(ScriptCollection viewModel)
        {
            if (!File.Exists(MYSCRIPTCONFIG))
            {
                viewModel.AddScript("Drop", "Выбросить оружие", "Выбрасывает оружие которое вы держите в руках.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "G" } });
                viewModel.AddScript("Jump", "Прыжок", "Нажимает кнопку \"Прыжка\".", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Space" } });
                viewModel.AddScript("Inspect", "Осмотр оружия", "Запускает анимацию осмотра оружия.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "F" } });
                viewModel.AddScript("Reload", "Перезарядка", "Перезаряжает оружие которое вы держите в руках.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "R" } });
                viewModel.AddScript("Interact", "Взаимодействие", "Нажимает кнопку \"Взаимодействие\", может неожиданно прервать обезвреживание бомбы.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "E" } });
                viewModel.AddScript("Fire", "Огонь", "Нажимает кнопку \"Выстрела\".", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Left" } });
                viewModel.AddScript("SFire", "Альтернативный огонь", "Нажимает кнопку \"Альтернативного огня\", обычно используется для прицеливания и переключения режима стрельбы.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Right" } });
                viewModel.AddScript("Voice", "Голосовой чат", "Включает ваш микрофон на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "C" } }, 10);
                viewModel.AddScript("Crouch", "Приседание", "Зажимает кнопку \"Приседания\" на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "LeftCtrl" } }, 10);
                viewModel.AddScript("Sneak", "Медленная ходьба", "Зажимает кнопку \"Медленной ходьбы\" на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "LeftShift" } }, 10);
                viewModel.AddScript("WeaponSwitch", "Переключение оружия", "Переключает оружие в руках на случайное.", false, Visibility.Visible, new Dictionary<string, string> { { "Main", "D1" }, { "Secondary", "D2" }, { "Knife", "D3" }, { "Grenades", "D4" }, { "Bomb", "D5" } });
                viewModel.AddScript("WalkingSomewhere", "Случайная ходьба", "Задает направление ходьбы в случайное направление (вперёд, влево, назад, вправо) на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Forward", "W" }, { "Left", "A" }, { "Backward", "S" }, { "Right", "D" } }, 10);
                viewModel.AddScript("TurnAround", "Разворот", "Разворачивает персонажа в случайном направлении", false, Visibility.Visible, new Dictionary<string, string>());
                viewModel.AddScript("Ping", "Метка", "Ставит внутри-игровую метку по направлению взгляда.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Middle" } });
                viewModel.AddScript("DisablingTheKeyboard", "Отключение клавиатуры", "Полностью отключает ввод с клавиатуры на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.AddScript("DisablingTheMouse", "Отключение мыши", "Полностью отключает ввод с мыши на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.AddScript("DisableShooting", "Отключение стрельбы", "Отключает возможность стрелять на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.AddScript("DisableRotation", "Отключение вращения", "Отключает возможность вращать камеру на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.AddScript("InvertControl", "Инвертировать управление", "Меняет местами WASD на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.SaveToJson(MYSCRIPTCONFIG);
                return;
            }
            var loadedViewModel = ScriptCollection.LoadFromJson(MYSCRIPTCONFIG);

            foreach (var item in loadedViewModel.Scripts)
            {
                viewModel.AddScript(item.ShortName, item.Info.Name, item.Info.Description, item.Info.Enabled, item.Info.Show_Gear, item.Info.Config.Binds, item.Info.Config.Int);
            }
        }
        
        private void CreateSettings()
        {
            settings = new Settings
            {
                Roulette = new Settings.RouletteSettings
                {
                    From = 30,
                    To = 90
                },
                Other = new Settings.OtherSettings
                {
                    RouletteBind = "F6",
                    RandomSampling = false,
                    CombineScripts = false
                }
            };

            settings.SaveToFile(MYSETTINGSCONFIG);
            LoadSettings();
        }

        private void SaveSettings()
        {
            settings = new Settings
            {
                Roulette = new Settings.RouletteSettings
                {
                    From = int.TryParse(pauseFrom.Text, out int valueFrom) ? valueFrom : 30,
                    To = int.TryParse(pauseTo.Text, out int valueTo) ? valueTo : 90
                },
                Other = new Settings.OtherSettings
                {
                    RouletteBind = rouletteBind.Text,
                    RandomSampling = (bool)randomSampling.IsChecked,
                    CombineScripts = (bool)combineScripts.IsChecked
                }
            };

            settings.SaveToFile(MYSETTINGSCONFIG);
        }

        private void LoadSettings()
        {
            settings = Settings.LoadFromFile(MYSETTINGSCONFIG);
            pauseFrom.Text = settings.Roulette.From.ToString();
            pauseTo.Text = settings.Roulette.To.ToString();
            rouletteBind.Text = settings.Other.RouletteBind;
            randomSampling.IsChecked = settings.Other.RandomSampling;
            combineScripts.IsChecked = settings.Other.CombineScripts;
        }

        private void StartRoulette()
        {
            if (enabledScripts.Count() == 0) return;
            if (settings.Roulette.From > settings.Roulette.To)
            {
                System.Windows.MessageBox.Show("Параметр \"От\" не должен быть больше \"До\"."); 
                return;
            }

            var waveOut = new WaveOutEvent();
            var startSignal = new SignalGenerator()
            {
                Gain = 0.2, // Громкость
                Frequency = 440, // Частота (в Гц)
                Type = SignalGeneratorType.Sin // Тип сигнала (синусоидальный)
            }.Take(TimeSpan.FromSeconds(0.3));

            var stopSignal = new SignalGenerator()
            {
                Gain = 0.2, // Громкость
                Frequency = 840, // Частота (в Гц)
                Type = SignalGeneratorType.Sin // Тип сигнала (синусоидальный)
            }.Take(TimeSpan.FromSeconds(0.3));

            inputSimulator = new InputSimulator();

            ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/greenPin.png"));

            CollectFunctions(viewModel);

            if (_rouletteStarted)
            {
                _cts.Cancel();
                _rouletteStarted = false;
                ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/redPin.png"));
                waveOut.Init(stopSignal);
                waveOut.Play();
            }
            else
            {
                waveOut.Init(startSignal);
                waveOut.Play();

                _cts = new CancellationTokenSource();
                _rouletteStarted = true;
                var token = _cts.Token;

                _runningTask = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (settings.Other.RandomSampling && !_enabledScriptsShuffled) { CollectFunctions(viewModel); Shuffle(enabledFunctions); Debug.WriteLine("RS: true, Shuffle"); }
                        else if (!settings.Other.RandomSampling && _enabledScriptsShuffled) { _enabledScriptsShuffled = false; CollectFunctions(viewModel); Debug.WriteLine("RS: false, CollectFunctions"); }

                        var taskForStart = settings.Other.RandomSampling ? enabledFunctions.FirstOrDefault() : enabledFunctions[new Random().Next(enabledFunctions.Count)];
                        Debug.WriteLine($"enabledScripts Count: {enabledFunctions.Count()}, {taskForStart.Method.Name}");
                        try
                        {
                            await Task.Delay(new Random().Next(settings.Roulette.From, settings.Roulette.To + 1) * 1000, token);
                            await taskForStart(token, viewModel);
                            if (settings.Other.RandomSampling)
                            {
                                Debug.WriteLine("RemoveSelected");
                                enabledFunctions.Remove(taskForStart);

                                if (enabledFunctions.Count() == 0) { CollectFunctions(viewModel); Debug.WriteLine("if Count == 0 | CollectFunctions"); }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                    _rouletteStarted = false;
                }, token);
            }
        }

        private int EnabledScriptsCount(ScriptCollection viewModel)
        {
            enabledScripts = viewModel.Scripts.Where(scriptEntry => scriptEntry.Info.Enabled).ToList();
            return enabledScripts.Count();
        }

        private Keys GetWinFormsKeyCodeFromDictionary(string key) { return winFormsKeycodes[key]; }

        private VirtualKeyCode GetVirtualKeyCodeFromDictionary(string key) { return virtualKeycodes[key]; }

        private MouseButtons GetMouseButtonFromDictionary(string key) { return mouseKeycodes[key]; }

        private void CollectFunctions(ScriptCollection viewModel) 
        {
            enabledFunctions.Clear();
            foreach (var i in viewModel.Scripts.Where(x => x.Info.Enabled).Select(x => x.ShortName).ToList()) enabledFunctions.Add(allFunctions[i]);
            if (settings.Other.RandomSampling) Shuffle(enabledFunctions);
        }
        
        private void MouseClick(string key)
        {
            switch (key)
            {
                case ("Left"):
                    inputSimulator.Mouse.LeftButtonClick();
                    break;
                case ("Right"):
                    inputSimulator.Mouse.RightButtonClick();
                    break;
                case ("Middle"):
                    inputSimulator.Mouse.MiddleButtonClick();
                    break;
                case ("Up"):
                    inputSimulator.Mouse.VerticalScroll(1);
                    break;
                case ("Down"):
                    inputSimulator.Mouse.VerticalScroll(-1);
                    break;
            }
        }
        
        private async Task PressAndBlockKey(CancellationToken ct, VirtualKeyCode key, int intValue, List<VirtualKeyCode> blockedKeys)
        {
            using (var blocker = new BlockSpecificKeys())
            {
                await blocker.BlockKey(key, intValue * 1000, ct, blockedKeys);
            }
            return;
        }

        private void SendLog(string log)
        {
            string file = $"{MYLOGSFOLDER}/{DateTime.Now.ToString("dd.MM.yyyy")}.txt";
            File.AppendAllText(file, $"{DateTime.Now} | {log}\n");
        }
        
        private void HideAllGrids()
        {
            WindowSettings.Visibility = Visibility.Collapsed;
            ScriptSettings_Grid.Visibility = Visibility.Collapsed;
        }

        private void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            _enabledScriptsShuffled = true;
        }
        #endregion

        #region Scripts
        private async Task Drop_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Drop");
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Drop").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500, ct);
        }

        private async Task Jump_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Jump");
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Jump").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500, ct);
        }

        private async Task Inspect_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Inspect");
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Inspect").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500, ct);
        }

        private async Task Reload_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Reload");
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Reload").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500, ct);
        }

        private async Task Interact_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Interact");
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Interact").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500, ct);
        }

        private async Task Fire_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Fire");
            string key = viewModel.FindScriptByShortName("Fire").Info.Config.Binds.Values.FirstOrDefault();
            if (isThisMouseButton.Contains(key))
            {
                MouseClick(key);
                return;
            }

            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(key));
            await Task.Delay(500, ct);
        }

        private async Task SFire_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("SFire");
            string key = viewModel.FindScriptByShortName("SFire").Info.Config.Binds.Values.FirstOrDefault();
            if (isThisMouseButton.Contains(key))
            {
                MouseClick(key);
                return;
            }

            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(key));
            await Task.Delay(500, ct);
        }

        private async Task Voice_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Voice");
            VirtualKeyCode targetKey = GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Voice").Info.Config.Binds.Values.FirstOrDefault());
            int intValue = viewModel.FindScriptByShortName("Voice").Info.Config.Int;

            await PressAndBlockKey(ct, targetKey, intValue, new List<VirtualKeyCode> { targetKey });
        }

        private async Task Crouch_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Crouch");
            VirtualKeyCode targetKey = GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Crouch").Info.Config.Binds.Values.FirstOrDefault());
            int intValue = viewModel.FindScriptByShortName("Crouch").Info.Config.Int;

            await PressAndBlockKey(ct, targetKey, intValue, new List<VirtualKeyCode> { targetKey });
        }

        private async Task Sneak_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Sneak");
            VirtualKeyCode targetKey = GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Sneak").Info.Config.Binds.Values.FirstOrDefault());
            int intValue = viewModel.FindScriptByShortName("Sneak").Info.Config.Int;

            await PressAndBlockKey(ct, targetKey, intValue, new List<VirtualKeyCode> { targetKey });
        }

        private async Task WeaponSwitch_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("WeaponSwitch");
            Random random = new Random();
            var binds = viewModel.FindScriptByShortName("WeaponSwitch").Info.Config.Binds.Values.ToList();
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(binds[random.Next(binds.Count)]));
            await Task.Delay(500, ct);
        }

        private async Task WalkingSomewhere_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("WalkingSomewhere");
            Random rand = new Random();
            List<VirtualKeyCode> blockedKeys = new List<VirtualKeyCode>();
            foreach (var key in viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Binds.Values.ToList()) { blockedKeys.Add(GetVirtualKeyCodeFromDictionary(key)); }

            VirtualKeyCode targetKey = blockedKeys[rand.Next(blockedKeys.Count)];
            int intValue = viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Int;

            await PressAndBlockKey(ct, targetKey, intValue, blockedKeys);
        }
        
        private async Task TurnAround_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("TurnAround");
            Random rand = new Random();
            inputSimulator.Mouse.MoveMouseBy(rand.Next(-13650, 13650), rand.Next(-3500, 3500));
            await Task.Delay(500, ct);
        }
        
        private async Task Ping_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("Ping");
            string key = viewModel.FindScriptByShortName("Ping").Info.Config.Binds.Values.FirstOrDefault();
            if (isThisMouseButton.Contains(key))
            {
                MouseClick(key);
                return;
            }

            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(key));
            await Task.Delay(500, ct);
        }
        
        private async Task DisablingTheKeyboard_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            SendLog("DisablingKeyboard");
            int intValue = viewModel.FindScriptByShortName("DisablingTheKeyboard").Info.Config.Int;
            List<VirtualKeyCode> keysForRelease = new List<VirtualKeyCode> 
            {
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Binds["Forward"]) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Binds["Left"]) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Binds["Backward"]) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("WalkingSomewhere").Info.Config.Binds["Right"]) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Crouch").Info.Config.Binds.Values.FirstOrDefault()) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Sneak").Info.Config.Binds.Values.FirstOrDefault()) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Sneak").Info.Config.Binds.Values.FirstOrDefault()) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Inspect").Info.Config.Binds.Values.FirstOrDefault()) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Voice").Info.Config.Binds.Values.FirstOrDefault()) },
                { GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Interact").Info.Config.Binds.Values.FirstOrDefault()) }
            };
            using (var blocker = new BlockTheEntireKeyboard())
            {
                await blocker.BlockKeyboard(intValue * 1000, ct, keysForRelease);
            }
        }

        private async Task DisablingTheMouse_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            return;
        }
        
        private async Task DisableShooting_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            return;
        }
        
        private async Task DisableRotation_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            return;
        }
        
        private async Task InvertControl_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            return;
        }
        #endregion

        #region Hooks
        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(settings.Other.RouletteBind))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => { StartRoulette(); });
            }
        }
        #endregion

        #region Async Functions

        #endregion
    }
}