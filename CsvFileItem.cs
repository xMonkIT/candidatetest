using System;

namespace TestTaskTransneft
{
    /// <summary>
    /// Класс, содержащий данные каждой строки csv файла
    /// </summary>
    public class CsvFileItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        
        public CsvFileItem(string n)
        {
            string[] temp = n.Split(';');
            Name = temp[0];
            Type = temp[1];
        }
    }
    
}