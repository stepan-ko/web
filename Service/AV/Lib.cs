using System.Diagnostics;


namespace Track
{
    public static class Lib
    {

        // Дополнительные методы
        // public static void LogTrack(LicensePlateData p, string dirsave, bool RU, int id, Logger logTrack, Logger logSystem)
        public static void LogTrack(LicensePlateData p, string dirsave, bool RU, int id)
        {
            if (ValidNumber(p.Number))
            {
                string outNumber = RU ? numberToRU(p.Number) : p.Number;

                if (p.Status == TrakcStatus.DETECTED) Debug.WriteLine($"{id};1;{outNumber}");

                if (p.Status == TrakcStatus.LOST)
                {
                    Debug.WriteLine($"{id};2;{outNumber}");
                    string dt = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string fileName = $"Input-{id}_{dt}__{p.Number}.jpg";
                    Console.WriteLine("fileName = " + fileName);

                    Directory.CreateDirectory(dirsave);      // создать каталог если его нет
                    p.BestImg?.Save(Path.Combine(dirsave, fileName)); // Сохранение лучшего кадра   
                }
            }
            Debug.WriteLine($"Input = {id}, TRACK = {p.Number}, [{p.Position}], Weight = {p.Weight:F4}, Status = {p.Status}");
        }

        public static bool ValidNumber(string number)
        {
            if (number.Length >= 8 && isLetterChar(number[0]) && char.IsNumber(number[1]) && char.IsNumber(number[2]) && char.IsNumber(number[3]) && isLetterChar(number[4]) && isLetterChar(number[5]) && char.IsNumber(number[6]) && char.IsNumber(number[7]))
            {
                if (number.Length == 8) return true;
                if (number.Length == 9 && char.IsNumber(number[8])) return true;
            }

            return false;
        }

        public static bool isLetterChar(char ch)
        {
            if (ch == 'A' || ch == 'B' || ch == 'E' || ch == 'K' || ch == 'M' || ch == 'H' ||
                ch == 'O' || ch == 'P' || ch == 'C' || ch == 'T' || ch == 'X' || ch == 'Y')
            {
                return true;
            }
            return false;
        }

        public static string numberToRU(string number)
        {
            string[] numberRU = new string[12];

            numberRU[0] = number.Replace('A', 'А');
            numberRU[1] = numberRU[0].Replace('B', 'В');
            numberRU[2] = numberRU[1].Replace('E', 'Е');
            numberRU[3] = numberRU[2].Replace('K', 'К');
            numberRU[4] = numberRU[3].Replace('M', 'М');
            numberRU[5] = numberRU[4].Replace('H', 'Н');
            numberRU[6] = numberRU[5].Replace('O', 'О');
            numberRU[7] = numberRU[6].Replace('P', 'Р');
            numberRU[8] = numberRU[7].Replace('C', 'С');
            numberRU[9] = numberRU[8].Replace('T', 'Т');
            numberRU[10] = numberRU[9].Replace('X', 'Х');
            numberRU[11] = numberRU[10].Replace('Y', 'У');
            return numberRU[11];
        }


        public static void PrintOptions(Options opt)
        {
            Console.WriteLine("== Options == ");
            Console.WriteLine(opt.MinWidth);
            Console.WriteLine(opt.MaxWidth);
            Console.WriteLine(opt.MinWeight);
            Console.WriteLine(opt.Tracking);
            Console.WriteLine(opt.NumberFrameForLose);
            Console.WriteLine(opt.Type);
            Console.WriteLine(opt.Area.ToString());
            Console.WriteLine("_ END Options _");
        }


        public static Config CheckJson(string[] args)
        {
            Config conf = new Config();

            if (args.Length == 0)
            {
                string msg = "Не указан путь к config.json \nПример: \nTrackDetected.exe config.json";
                ExitConsole(msg);
                return conf;
            }

            string configPath = args[0];
            if (!File.Exists(configPath))
            {
                ExitConsole($"Ошибка: файл 'config.json' не найден: {configPath}");
                return conf;
            }

            if (Path.GetExtension(configPath).ToLower() != ".json")
            {
                ExitConsole("Ошибка: требуется JSON файл");
                return conf;
            }

            Console.WriteLine($"Загрузка конфигурации: {configPath}");

            try
            {
                string json = File.ReadAllText(configPath);
                // conf = JsonConvert.DeserializeObject<Config>(json);
                if (conf == null)
                {
                    ExitConsole("Ошибка: не удалось распарсить config.json");
                    return conf;
                }
                else
                {
                    return conf;
                }
            }
            catch (Exception ex)
            {
                ExitConsole("Ошибка файла config.json: " + ex.ToString());
                return conf;
            }
        }

        public static void ExitConsole(string msg)
        {
            Console.WriteLine(msg);
            Console.ReadLine();
            Environment.Exit(1);
        }

    }
}
