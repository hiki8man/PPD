using PPDEditor.Models;
using System.Windows;

namespace PPDEditor.Forms
{
    public sealed partial class ModeSelectWindow : Window
    {
        public EditorScriptMode SelectedMode { get; private set; }

        public bool RememberChoice { get; private set; }

        public ModeSelectWindow(EditorScriptMode initialMode, bool rememberChoice)
        {
            InitializeComponent();
            SelectedMode = initialMode;
            RememberChoice = rememberChoice;
            switch (initialMode)
            {
                case EditorScriptMode.DivaScript:
                    divascriptRadio.IsChecked = true;
                    break;
                case EditorScriptMode.Vet08:
                    vet08Radio.IsChecked = true;
                    break;
                default:
                    vet08hRadio.IsChecked = true;
                    break;
            }
            rememberCheckBox.IsChecked = rememberChoice;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (divascriptRadio.IsChecked == true)
            {
                SelectedMode = EditorScriptMode.DivaScript;
            }
            else if (vet08Radio.IsChecked == true)
            {
                SelectedMode = EditorScriptMode.Vet08;
            }
            else
            {
                SelectedMode = EditorScriptMode.Vet08H;
            }
            RememberChoice = rememberCheckBox.IsChecked == true;
            DialogResult = true;
        }
    }
}
