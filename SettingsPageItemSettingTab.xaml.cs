using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Invoice
{
    /// <summary>
    /// SettingsPageItemSettingTab.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsPageItemSettingTab : TabItem
    {
        private bool isEditing = false;
        
        private SettingsViewModel vm;

        public SettingsPageItemSettingTab()
        {
            InitializeComponent();
            var mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;
            vm = mainWindowViewModel.SettingsVM;
            Loaded += SettingsPageItemSettingTab_Loaded;
        }

        private void SettingsPageItemSettingTab_Loaded(object sender, RoutedEventArgs e)
        {
            var p = this.Parent as TabControl;
            var mainWindowViewModel = p.DataContext as MainWindowViewModel;
            DataContext = mainWindowViewModel;
            vm = mainWindowViewModel.SettingsVM;
        }

        private void ShowDatailPane()
        {
            //ItemPageContentsGrid.IsEnabled = false;
            //var renderTransform = ItemDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            //var slideUpAnimation = new DoubleAnimation
            //{
            //    From = 300,
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(300),
            //    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            //};
            //renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);
        }


    }
}
