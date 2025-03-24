using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
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
        ScriptEntry selectedScript;

        //Перехват и эмуляция нажатий
        private InputSimulator inputSimulator;
        private IKeyboardMouseEvents _globalHook;

        private VirtualKeyCode _targetKey;
        private bool _isSimulatingKey;


        // Рулетка сценариев
        private bool rouletteStarted = false;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        //Директории
        readonly string MYDIRECTORY = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator";
        readonly string MYCONFIG = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator/config.json";

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

        private readonly Dictionary<string, Func<CancellationToken, ScriptCollection, Task>> allFunctions;

        List<Func<CancellationToken, ScriptCollection, Task>> enabledFunctions = new List<Func<CancellationToken, ScriptCollection, Task>>();

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ScriptCollection();

            allFunctions = new Dictionary<string, Func<CancellationToken, ScriptCollection, Task>>
            {
                { "Drop", Drop_Script },
                { "Jump", Jump_Script },
                { "Inspect", Inspect_Script },
                { "Reload", Reload_Script },
                { "Fire", Fire_Script },
                { "SFire", SFire_Script },
                { "Voice", Voice_Script }
            };
        }

        #region Window Handlers
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ScriptCollection)this.DataContext;
            CollectScripts(viewModel);
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
            if (!Directory.Exists(MYDIRECTORY)) Directory.CreateDirectory(MYDIRECTORY);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { DragMove(); }

        private void CloseWindow(object sender, RoutedEventArgs e) { Close(); }

        private void MinimizeWindow(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void StartScriptRoulette(object sender, RoutedEventArgs e)
        {
            if (enabledScripts.Count() == 0) return;

            ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/greenPin.png"));

            var viewModel = (ScriptCollection)this.DataContext;
            inputSimulator = new InputSimulator();

            CollectFunctions(viewModel);

            if (rouletteStarted)
            {
                _cts.Cancel();
                rouletteStarted = false;
                ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/redPin.png"));
            }
            else
            {
                _cts = new CancellationTokenSource();
                rouletteStarted = true;

                var token = _cts.Token;

                _runningTask = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            if (enabledFunctions.Count() == 0) { await Task.Delay(1000, token); return; }
                            await Task.Delay(new Random().Next(5, 6) * 1000, token);
                            await enabledFunctions[new Random().Next(enabledFunctions.Count)](token, viewModel);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }

                    rouletteStarted = false;
                }, token);
            }
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
            var viewModel = (ScriptCollection)this.DataContext;
            viewModel.SaveToJson(MYCONFIG);
            CollectFunctions(viewModel);
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
        }

        private void ScriptSettings(object sender, RoutedEventArgs e)
        {
            ScriptSettings_Grid.Visibility = Visibility.Visible;

            var button = sender as System.Windows.Controls.Button;
            var item = button?.DataContext as ScriptEntry;
            selectedScript = item;
            scriptDescription.Text = item.Info.Description;

            scriptName.Content = item.Info.Name;

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
                Main_TextBox.Text = item.Info.Config.Binds["Main"];
                Secondary_TextBox.Text = item.Info.Config.Binds["Secondary"];
                Knife_TextBox.Text = item.Info.Config.Binds["Knife"];
                Grenades_TextBox.Text = item.Info.Config.Binds["Grenades"];
                Bomb_TextBox.Text = item.Info.Config.Binds["Bomb"];
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
            ScriptSettings_Grid.Visibility = Visibility.Collapsed;
        }

        private void SaveScriptSettings(object sender, RoutedEventArgs e)
        {
            var viewModel = (ScriptCollection)this.DataContext;
            Dictionary<string, string> binds = new Dictionary<string, string>();

            switch (selectedScript.ShortName)
            {
                case ("WalkingSomewhere"):
                    binds.Add("Forward", Forward_TextBox.Text);
                    binds.Add("Left", Left_TextBox.Text);
                    binds.Add("Backward", Backward_TextBox.Text);
                    binds.Add("Right", Right_TextBox.Text);
                    break;

                case ("WeaponSwitch"):
                    binds.Add("Main", Main_TextBox.Text);
                    binds.Add("Secondary", Secondary_TextBox.Text);
                    binds.Add("Knife", Knife_TextBox.Text);
                    binds.Add("Grenades", Grenades_TextBox.Text);
                    binds.Add("Bomb", Bomb_TextBox.Text);
                    break;

                default:
                    if (selectedScript.Info.Config.Int != 0) selectedScript.Info.Config.Int = Int32.Parse(Int_TextBox.Text);
                    if (selectedScript.Info.Config.Binds.ContainsKey("Bind")) binds.Add("Bind", Keybind_TextBox.Text);
                    break;
            }
            viewModel.EditScript(selectedScript, binds);
            viewModel.SaveToJson(MYCONFIG);
        }
        #endregion

        #region Helpers
        private void CollectScripts(ScriptCollection viewModel)
        {
            if (!File.Exists(MYCONFIG))
            {
                viewModel.AddScript("Drop", "Выбросить оружие", "Выбрасывает оружие которое вы держите в руках.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "G" } });
                viewModel.AddScript("Jump", "Прыжок", "Нажимает кнопку \"Прыжка\".", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Space" } });
                viewModel.AddScript("Inspect", "Осмотр оружия", "Запускает анимацию осмотра оружия.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "F" } });
                viewModel.AddScript("Reload", "Перезарядка", "Перезаряжает оружие которое вы держите в руках.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "R" } });
                viewModel.AddScript("Fire", "Огонь", "Нажимает кнопку \"Выстрела\".", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Left" } });
                viewModel.AddScript("SFire", "Альтернативный огонь", "Нажимает кнопку \"Альтернативного огня\", обычно используется для прицеливания и переключения режима стрельбы.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Right" } });
                viewModel.AddScript("Voice", "Голосовой чат", "Включает ваш микрофон на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "C" } }, 10);
                viewModel.AddScript("Crouch", "Приседание", "Зажимает кнопку \"Приседания\" на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "LeftCtrl" } }, 10);
                viewModel.AddScript("Sneak", "Медленная ходьба", "Зажимает кнопку \"Медленной ходьбы\" на N секунд.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "LeftShift" } }, 10);
                viewModel.AddScript("WeaponSwitch", "Переключение оружия", "Переключает оружие в руках на случайное.", false, Visibility.Visible, new Dictionary<string, string> { { "Main", "D1" }, { "Secondary", "D2" }, { "Knife", "D3" }, { "Grenades", "D4" }, { "Bomb", "D5" } });
                viewModel.AddScript("WalkingSomewhere", "Случайная ходьба", "Задает направление ходьбы в случайное направление (вперёд, влево, назад, вправо).", false, Visibility.Visible, new Dictionary<string, string> { { "Forward", "W" }, { "Left", "A" }, { "Backward", "S" }, { "Right", "D" } });
                viewModel.AddScript("TurnAround", "Разворот", "Разворачивает персонажа на N градусов.", false, Visibility.Visible, new Dictionary<string, string>(), 180);
                viewModel.AddScript("Ping", "Метка", "Ставит внутри-игровую метку по направлению взгляда.", false, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Middle" } });
                viewModel.AddScript("DisablingTheKeyboard", "Отключение клавиатуры", "Полностью отключает ввод с клавиатуры на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.AddScript("DisablingTheMouse", "Отключение мыши", "Полностью отключает ввод с мыши на N секунд.", false, Visibility.Visible, new Dictionary<string, string>(), 10);
                viewModel.SaveToJson(MYCONFIG);
                return;
            }
            var loadedViewModel = ScriptCollection.LoadFromJson(MYCONFIG);

            foreach (var item in loadedViewModel.Scripts)
            {
                viewModel.AddScript(item.ShortName, item.Info.Name, item.Info.Description, item.Info.Enabled, item.Info.Show_Gear, item.Info.Config.Binds, item.Info.Config.Int);
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
        }
        
        private void SendKey(string key, int @int)
        {
            
        }
        #endregion

        #region Scripts
        private async Task Drop_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Drop").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task Jump_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Jump").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task Inspect_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Inspect").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task Reload_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Reload").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task Fire_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Fire").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task SFire_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyPress(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("SFire").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(500);
        }

        private async Task Voice_Script(CancellationToken ct, ScriptCollection viewModel)
        {
            inputSimulator.Keyboard.KeyDown(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Voice").Info.Config.Binds.Values.FirstOrDefault()));
            await Task.Delay(viewModel.FindScriptByShortName("Voice").Info.Config.Int * 1000, ct);
            inputSimulator.Keyboard.KeyUp(GetVirtualKeyCodeFromDictionary(viewModel.FindScriptByShortName("Voice").Info.Config.Binds.Values.FirstOrDefault()));
        }
        #endregion
    }
}