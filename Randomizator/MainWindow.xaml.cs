using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Randomizator
{
    public partial class MainWindow : Window
    {
        List<ScriptEntry> enabledScripts = new List<ScriptEntry>();
        ScriptEntry selectedScript;

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
            scriptsLabel.Content = $"Список сценариев ({viewModel.Scripts.Count()})";
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
        }

        private void ScriptSettings(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.DataContext as ScriptEntry;
            selectedScript = item;
            scriptDescription.Text = item.Info.Description;
        }

        private void KeyboardInput(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
        #endregion

        #region Helpers
        private void CollectScripts(ScriptCollection viewModel)
        {
            viewModel.AddScript("Drop", "Выбросить оружие", "Выбрасывает оружие которое вы держите в руках.", true, Visibility.Visible);
            viewModel.AddScript("Jump", "Прыжок", "Нажимает кнопку \"Прыжка\".", true, Visibility.Visible);
            viewModel.AddScript("Inspect", "Осмотр оружия", "Запускает анимацию осмотра оружия.", false, Visibility.Visible);
            viewModel.AddScript("Reload", "Перезарядка", "Перезаряжает оружие которое вы держите в руках.", false, Visibility.Visible);
            viewModel.AddScript("Fire", "Огонь", "Нажимает кнопку \"Выстрела\".", false, Visibility.Visible);
            viewModel.AddScript("SFire", "Альтернативный огонь", "Нажимает кнопку \"Альтернативного огня\", обычно используется для прицеливания и переключения режима стрельбы.", false, Visibility.Visible);
            viewModel.AddScript("Voice", "Голосовой чат", "Включает ваш микрофон на N секунд.", false, Visibility.Visible);
            viewModel.AddScript("Crouch", "Приседание", "Зажимает кнопку \"Приседания\" на N секунд.", false, Visibility.Visible);
            viewModel.AddScript("Sneak", "Медленная ходьба", "Зажимает кнопку \"Медленной ходьбы\" на N секунд.", false, Visibility.Visible);
            viewModel.AddScript("WeaponSwitch", "Переключение оружия", "Переключает оружие в руках на случайное.", false, Visibility.Visible);
            viewModel.AddScript("WalkingSomewhere", "Случайная ходьба", "Задает направление ходьбы в случайное направление (вперёд, назад, вправо, влево).", false, Visibility.Visible);
            viewModel.AddScript("TurnAround", "Разворот", "Разворачивает персонажа на N градусов.", false, Visibility.Visible);
            viewModel.AddScript("Ping", "Метка", "Ставит внутри-игровую метку по направлению взгляда.", false, Visibility.Visible);
            viewModel.AddScript("DisablingTheKeyboard", "Отключение клавиатуры", "Полностью отключает ввод с клавиатуры на N секунд.", false, Visibility.Visible);
            viewModel.AddScript("DisablingTheMouse", "Отключение мыши", "Полностью отключает ввод с мыши на N секунд.", false, Visibility.Visible);
        }
        #endregion

        #region Async Tasks

        //private async Task ScriptRoulette()
        //{

        //}

        #endregion
    }
}
