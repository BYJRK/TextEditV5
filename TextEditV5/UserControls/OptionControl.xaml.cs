using System;
using System.Windows;
using System.Windows.Controls;

namespace TextEditV5.UserControls
{
    /// <summary>
    /// OptionControl.xaml 的交互逻辑
    /// </summary>
    public partial class OptionControl : UserControl
    {
        public OptionControl()
        {
            InitializeComponent();
        }

        public MainWindow mainForm;
        private void SetTheme(object sender, RoutedEventArgs e)
        {
            if (mainForm != null)
                mainForm.ChangeTheme((sender as RadioButton).Name);
        }
        public void UpdatePanel()
        {
            if (mainForm.color == "LightTheme") LightTheme.IsChecked = true;
            else if (mainForm.color == "DarkTheme") DarkTheme.IsChecked = true;
            alwaysShowingBox2.IsChecked = mainForm.alwaysShowingBox2;
            autoReset.IsChecked = mainForm.autoReset;
            detailedCountInfo.IsChecked = mainForm.detailedCountInfo;
            autoCache.IsChecked = mainForm.autoCache;
            cutButton.IsChecked = mainForm.cutButton;
            showRunTime.IsChecked = mainForm.showRunTime;
            autoCheckUpdate.IsChecked = mainForm.autoCheckUpdate;
            maxHistory.Text = mainForm.maxHistory.ToString();
            pairFolder.Text = mainForm.pairFolder;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString() == "确认")
            {
                mainForm.alwaysShowingBox2 = alwaysShowingBox2.IsChecked.Value;
                mainForm.autoReset = autoReset.IsChecked.Value;
                mainForm.detailedCountInfo = detailedCountInfo.IsChecked.Value;
                mainForm.autoCache = autoCache.IsChecked.Value;
                mainForm.cutButton = cutButton.IsChecked.Value;
                mainForm.showRunTime = showRunTime.IsChecked.Value;
                mainForm.autoCheckUpdate = autoCheckUpdate.IsChecked.Value;
                mainForm.maxHistory = Convert.ToInt32(maxHistory.Text);
                mainForm.pairFolder = pairFolder.Text;
                if (LightTheme.IsChecked.Value) mainForm.ChangeTheme("LightTheme");
                else if (DarkTheme.IsChecked.Value) mainForm.ChangeTheme("DarkTheme");
                mainForm.ApplyConfig();

                mainForm.ShowLeftMenu(true);
                mainForm.ShowRightContent(RightContent.Editor);
            }
            else if (((Button)sender).Content.ToString() == "关闭")
            {
                mainForm.ShowLeftMenu(true);
                mainForm.ShowRightContent(RightContent.Editor);
            }
        }
    }
}