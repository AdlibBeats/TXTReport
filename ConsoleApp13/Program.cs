using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TXTReport
{
    class Entry
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
    }

    class Program
    {
        static void Main()
        {
            Console.Write("Введите путь к файлу: ");
            string fileDirect = Console.ReadLine(); //@"C:\Users\avasilyev\Desktop\Отчет 13-19.08.txt"
            Console.Write("Введите имя файла: ");
            string fileName = Console.ReadLine();
            Console.WriteLine();

            StreamReader streamReader = new StreamReader(fileDirect + @"\" + fileName, Encoding.GetEncoding(1251));

            string streamReaderLine = "";
            List<Entry> entries = new List<Entry>();

            bool isFirstLine = true;
            while (streamReaderLine != null)
            {
                streamReaderLine = streamReader.ReadLine();
                if (streamReaderLine != null && !isFirstLine)
                {
                    streamReaderLine = streamReaderLine.Replace("Нормальный вход по ключу", "");
                    streamReaderLine = streamReaderLine.Replace("Нормальный выход по ключу", "");
                    streamReaderLine = streamReaderLine.Replace("Главный вход", "");
                    streamReaderLine = streamReaderLine.Replace("Главный выход", "");
                    streamReaderLine = streamReaderLine.Replace("Авторизированный вход", "");
                    streamReaderLine = streamReaderLine.Replace("Авторизированный выход", "");
                    streamReaderLine = streamReaderLine.Replace("MobileDimension", "");

                    streamReaderLine = Regex.Replace(streamReaderLine, @"\s+", "*");

                    string[] blocks = streamReaderLine.Split(new char[] { '*' });
                    if (blocks.Length < 4) continue;

                    string[] dateBlock = blocks[0].Split(new char[] { '.' });
                    string[] timeBlock = blocks[1].Split(new char[] { ':' });


                    int year, month, day, hour, minute, second;

                    bool isCheckedYear = int.TryParse(dateBlock[2], out year);
                    bool isCheckedMonth = int.TryParse(dateBlock[1], out month);
                    bool isCheckedDay = int.TryParse(dateBlock[0], out day);
                    bool isCheckedHour = int.TryParse(timeBlock[0], out hour);
                    bool isCheckedMinute = int.TryParse(timeBlock[1], out minute);
                    bool isCheckedSecond = int.TryParse(timeBlock[2], out second);

                    Entry entry = new Entry
                    {
                        Date = dateBlock.Length > 2 ? new DateTime(
                            isCheckedYear ? year : 0,
                            isCheckedMonth ? month : 0,
                            isCheckedDay ? day : 0) : default(DateTime),
                        Time = timeBlock.Length > 2 ? new TimeSpan(
                            isCheckedHour ? hour : 0,
                            isCheckedMinute ? minute : 0,
                            isCheckedSecond ? second : 0) : default(TimeSpan),
                        Name = blocks[2] + " " + blocks[3]
                    };
                    entries.Add(entry);
                }
                isFirstLine = false;
            }
            streamReader.Close();

            StringBuilder finalText = new StringBuilder();
            entries = entries.OrderBy(i => i.Name).ToList();

            if (entries.Any())
            {
                var entriesDateGroups = entries.GroupBy(i => i.Date);
                foreach (var entriesDateGroup in entriesDateGroups)
                {
                    var entriesNameGroups = entriesDateGroup.GroupBy(i => i.Name);
                    foreach (var entriesNameGroup in entriesNameGroups)
                    {
                        string firstLine = string.Format("{0} {1} {2}",
                            entriesNameGroup.First().Date.ToString("d"),
                            entriesNameGroup.First().Time,
                            entriesNameGroup.First().Name);

                        string secondLine = string.Format("{0} {1} {2}",
                            entriesNameGroup.Last().Date.ToString("d"),
                            entriesNameGroup.Last().Time,
                            entriesNameGroup.Last().Name);

                        string thirdLine = string.Format("Общее время - {0}", entriesNameGroup.Last().Time - entriesNameGroup.First().Time);

                        Console.WriteLine(firstLine);
                        Console.WriteLine(secondLine);
                        Console.WriteLine(thirdLine);
                        Console.WriteLine();

                        finalText.AppendLine(firstLine);
                        finalText.AppendLine(secondLine);
                        finalText.AppendLine(thirdLine);
                        finalText.AppendLine("");
                    }
                }
                Console.WriteLine("Запись началась, ожидайте завершения.");
                try
                {
                    using (StreamWriter sw = new StreamWriter(fileDirect + @"\" + "(Report)" + fileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine(finalText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Console.WriteLine("Запись завершена.");
                }
            }
            Console.Read();
        }
    }
}
