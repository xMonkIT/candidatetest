using System.Collections.Generic;
using System.Windows;

namespace TestTaskTransneft
{
    /// <summary>
    /// Логика взаимодействия для dgSelectionForm.xaml
    /// </summary>
    public partial class SelectionForm : Window
    {
        #region Поля
        
        List<CsvFileItem> _list = new List<CsvFileItem>();
        
        #endregion

        #region Конструктор

        public SelectionForm()
        {
            InitializeComponent();
            
            Apply.Click += Apply_OnClick;
            Cancel.Click += Cancel_OnClick;
        }

        #endregion

        #region События

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <  MainWindow.csvFile.Count; i++)
            {
                _list.Add(MainWindow.csvFile[i]);
            }
            
            dgSelectionForm.ItemsSource = _list;
        }

        public void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.csvFile.Clear();
            for (int i = 0; i < _list.Count; i++)
            {
                MainWindow.csvFile.Add(_list[i]);
            }
            Apply.Click -= Apply_OnClick;
            Cancel.Click -= Cancel_OnClick;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Apply.Click -= Apply_OnClick;
            Cancel.Click -= Cancel_OnClick;
            Close();
        }

        #endregion
    }
}