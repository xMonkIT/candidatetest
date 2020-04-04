using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.Xml;

namespace TestTaskTransneft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static List<CsvFileItem> csvFile = new List<CsvFileItem>();
        
        private JsonFileItem _jsonFile = new JsonFileItem();
        private List<XmlFileItem> _xmlFile = new List<XmlFileItem>();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region События
        
        /// <summary>
        /// Вызов диалогового окна для выбора файла .csv и его загрузки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchCsv_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".csv files|*.csv|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames) 
                    csvPath.Text = Path.GetFullPath(filename);
            }

            try
            {
                ReadCsv(csvPath.Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Неправильная структура файла (Должно быть: Tag;Type;Address)", "Ошибка чтения .csv");
                throw;
            }
            
        }
        
        /// <summary>
        /// Вызов диалогового окна для выбора файла .json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".json files|*.json|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                    jsonPath.Text = Path.GetFullPath(filename);
            }

            try
            {
                ReadJson();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Неправильная структура файла (Должно быть {\"TypeInfos\":[{\"TypeName\": \"ZRP\",\"Propertys\": {\"flowkD\": " +
                                "\"double\"}},{\"TypeName\": \"AI\",\"Propertys\": {\"Cmd\": \"double\"}}]}", "Ошибка чтения .json");
                throw;
            }
        }
        
        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartProgram_Click(object sender, RoutedEventArgs e)
        {
            if (csvPath.Text != string.Empty && jsonPath.Text!=string.Empty)
            {
                try
                {                
                    MergeringCsvAndJson();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Неправильная структура файлов", "Ошибка");
                    throw;
                }
                SaveXML(PathToXml(), _xmlFile);
                Close();
            }
            else
            {
                MessageBox.Show("Файлы не выбраны","Ошибка");
            }
        }
        
        /// <summary>
        /// Вызов формы для удаления ненужных строк из .csv файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallSelectionForm_Click(object sender, RoutedEventArgs e)
        {
            if (csvPath.Text!=string.Empty)
            {
                SelectionForm selectionForm = new SelectionForm();
                selectionForm.Show();
            }
        }
        
        #endregion

        #region Методы

        /// <summary>
        /// Чтение содержимого файла .csv
        /// </summary>
        public void ReadCsv(string path)
        {
            StreamReader streamReader = new StreamReader(path);
            while (!streamReader.EndOfStream)
            {
                csvFile.Add(new CsvFileItem(streamReader.ReadLine()));
            }
            streamReader.Close();
            csvFile.Remove(csvFile[0]);
        }

        /// <summary>
        /// Чтение содержимого файла .json
        /// </summary>
        private void ReadJson()
        {
            string jsonString = File.ReadAllText("E:\\Гугл.Диск\\Программирование\\Test task (Транснефть)\\TypeInfos.json");
            _jsonFile = JsonSerializer.Deserialize<JsonFileItem>(jsonString);
        }
        
        /// <summary>
        /// Метод компанует строки из файла .csv c файлом .json и осуществляет преобразования
        /// </summary>
        private void MergeringCsvAndJson()
        {
            for (int i = 0; i < csvFile.Count; i++)
            {
                for (int j = 0; j < _jsonFile.TypeInfos.Count; j++)
                {
                    if (csvFile[i].Type == _jsonFile.TypeInfos[j].TypeName)
                    {
                        string[] tempKeys = new string[_jsonFile.TypeInfos[j].Propertys.Keys.Count];
                        string[] tempValue = new string[_jsonFile.TypeInfos[j].Propertys.Keys.Count];
                    
                        _jsonFile.TypeInfos[j].Propertys.Keys.CopyTo(tempKeys,0);
                        _jsonFile.TypeInfos[j].Propertys.Values.CopyTo(tempValue,0);
                        
                        int[] value = new int[tempValue.Length];
                        
                        ReformStringToByte(tempValue,ref value);
                        CalcAmount(ref value);
                        
                        XmlFileItem xmlFileItem = new XmlFileItem();
                        
                        for (int k = 0; k < tempValue.Length; k++)
                        {
                            xmlFileItem.Dictionary.Add(csvFile[i].Name+"."+tempKeys[k],value[k]);
                        }
                        _xmlFile.Add(xmlFileItem);
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Метод преобразует строковое наименование типа данных, в байтовый размер
        /// </summary>
        /// <param name="tempValueStrings">Массив с наименованиями типов данных</param>
        /// <param name="value">Массив с байтовыми размерами соответсвующего типа данных</param>
        private void ReformStringToByte(string[] tempValueStrings,ref int[] value)
        {
            for (int i = 0; i < tempValueStrings.Length; i++)
            {
                if (tempValueStrings[i].Equals("int")) value[i] = 4;
                if (tempValueStrings[i].Equals("double")) value[i] = 8;
                if (tempValueStrings[i].Equals("bool")) value[i] = 1;
            }
        }
        
        /// <summary>
        /// Метод реализует смещение байтового размера каждой строки на байтовый размер предыдущей строки 
        /// </summary>
        /// <param name="value">Массив с байтовыми размерами соответсвующего типа данных</param>
        private void CalcAmount(ref int[] value)
        {
            int[] temp = new int[value.Length];
            value.CopyTo(temp,0);

            for (int i = 1; i < temp.Length; i++)
            {
                temp[i] += temp[i - 1];
            }
            
            for (int i = 1; i < value.Length; i++)
            {
                value[i] = temp[i-1];
            }
            value[0] = 0;
        }
        
        /// <summary>
        /// Сохранение в файл .xml по шаблону
        /// </summary>
        /// <param name="path">Путь до файла</param>
        /// <param name="list">Лист всех словарей содержащих конечные значения для вывода</param>
        private void SaveXML(string path, List<XmlFileItem> list)
        {
            var doc = new XmlDocument();

            //var XmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            
            var item = doc.CreateElement("item");
            var attribute = doc.CreateAttribute("Binding");
            attribute.Value = "Introduced";
            item.Attributes.Append(attribute);
            doc.AppendChild(item);
            for (int i = 0; i < list.Count; i++)
            {
                string[] tempKeys = new string[list[i].Dictionary.Count];
                int[] tempValues = new int[list[i].Dictionary.Count];
                    
                list[i].Dictionary.Keys.CopyTo(tempKeys,0);
                list[i].Dictionary.Values.CopyTo(tempValues,0);
                    
                for (int k = 0; k < tempKeys.Length; k++)
                {
                    var node = doc.CreateElement("node-path");
                    var address = doc.CreateElement("address");
                    node.InnerText = $"{{{{ {tempKeys[k]} }}}}";
                    address.InnerText = $"{{{{ {tempValues[k]} }}}}";
                        
                    item.AppendChild(node);
                    item.AppendChild(address);
                }
            }
            doc.Save(path);
        }

        /// <summary>
        /// Выбор пути для сохранения файла .xml
        /// </summary>
        /// <returns>Путь к месту сохранения</returns>
        private string PathToXml()
        {
            string path = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.FileName = "Output";
            saveFileDialog.DefaultExt = ".xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                foreach (string filename in saveFileDialog.FileNames)
                {
                    path = Path.GetFullPath(filename);
                }
            }
            if (path != string.Empty) MessageBox.Show($"Файл сохранен по пути {path}", "Успешно");
            return path;
            
        }

        #endregion
    }
}