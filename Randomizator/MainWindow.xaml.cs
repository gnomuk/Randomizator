using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Randomizator
{
    public partial class MainWindow : Window
    {
        List<ScriptEntry> enabledScripts = new List<ScriptEntry>();
        ScriptEntry selectedScript;

        readonly string MYDIRECTORY = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator";
        readonly string MYCONFIG = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Randomizator/config.json";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ScriptCollection();
        }

        #region Window Handlers
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ScriptCollection)this.DataContext;
            CollectScripts(viewModel);
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
            if (!Directory.Exists(MYDIRECTORY)) Directory.CreateDirectory(MYDIRECTORY);
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) { DragMove(); }

        private void CloseWindow(object sender, RoutedEventArgs e) { Close(); }

        private void MinimizeWindow(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void StartScriptRoulette(object sender, RoutedEventArgs e)
        {
            //await Task.Run(ScriptRoulette);
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
            enabledScripts = viewModel.Scripts.Where(scriptEntry => scriptEntry.Info.Enabled).ToList();
            viewModel.SaveToJson(MYCONFIG);
            scriptsLabel.Content = $"Список сценариев ({EnabledScriptsCount(viewModel)})";
        }

        private void ScriptSettings(object sender, RoutedEventArgs e)
        {
            ScriptSettings_Grid.Visibility = Visibility.Visible;

            var button = sender as Button;
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

        private void KeyboardInput(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string key = e.Key.ToString();
            if (key == "Back") { textBox.Text = ""; return; }
            textBox.Text = key;
            e.Handled = true;
        }

        private void MouseInput(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string button = e.ChangedButton.ToString();
            textBox.Text = button;
            //e.Handled = true;
        }

        private void MouseWheelInput(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Delta > 0) textBox.Text = "Up";
            else textBox.Text = "Down";
        }

        private void IntInput(object sender, KeyEventArgs e)
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
                    MessageBox.Show(selectedScript.Info.Config.Int.ToString());
                    MessageBox.Show(selectedScript.Info.Config.Binds.Keys.FirstOrDefault());
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
                viewModel.AddScript("Drop", "Выбросить оружие", "Выбрасывает оружие которое вы держите в руках.", true, Visibility.Visible, new Dictionary<string, string> { { "Bind", "G" } });
                viewModel.AddScript("Jump", "Прыжок", "Нажимает кнопку \"Прыжка\".", true, Visibility.Visible, new Dictionary<string, string> { { "Bind", "Space" } });
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
            return viewModel.Scripts.Count(script => script.Info.Enabled);
        }
        #endregion

        #region Async Tasks

        //private async Task ScriptRoulette()
        //{

        //}

        #endregion
    }
}