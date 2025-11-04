using System.IO.Compression;
using System.Xml.Serialization;

namespace TestChelyabinvestbank;

internal class Program
{
    static async Task Main(string[] args)
    {
        string url = "https://fias.nalog.ru/Public/Downloads/Actual/gar_delta_xml.zip";
        string zipPath = @"C:\Gap\gar_delta_xml.zip";
        string extractPath = @"C:\Gap\Extracted";

        if (File.Exists(zipPath))
        {
            Console.WriteLine("Файл существует.");
        }
        else
        {
            string pathDirectoryZip = @"C:\Gap";
            Directory.CreateDirectory(pathDirectoryZip);

            await GetАrchiveGar(url, zipPath);
        }

        string pathDirectoryFiles = @"C:\Gap\Extracted";
        Directory.CreateDirectory(pathDirectoryFiles);

        bool isNotEmptyFolderExtract = Directory.GetFiles(extractPath).Length > 0 || Directory.GetDirectories(extractPath).Length > 0;
        if (!isNotEmptyFolderExtract)
        {
            GetFiles(zipPath, extractPath);
        }
        ConvertToCSV();
    }

    /// <summary>
    /// Метод получения архива.
    /// </summary>
    public static async Task GetАrchiveGar(string url, string savePath)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                using (Stream stream = await client.GetStreamAsync(url))
                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                    Console.WriteLine("Файл успешно скачан по пути: " + savePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при скачивании файла: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Извлечение файлов из архива 
    /// </summary>
    public static void GetFiles(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Console.WriteLine("Архив успешно распакован в: " + extractPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при распаковке: " + ex.Message);
        }
    }

    /// <summary>
    /// Конвертация в формат csv.
    /// </summary>
    public static void ConvertToCSV()
    {
        string folderPath = @"C:\Gap\Extracted\01";
        string searchWord = "AS_APARTMENTS";

        string csvPath = @"C:\Gap\output.csv";

        // Получаем массив путей к файлам
        string[] files = Directory.GetFiles(folderPath, $"*{searchWord}*");

        try
        {
            // Десериализация XML в объекты
            XmlSerializer serializer = new XmlSerializer(typeof(APARTMENTS));
            APARTMENTS apartments;

            using (FileStream fs = new FileStream(files[0], FileMode.Open))
            {
                apartments = (APARTMENTS)serializer.Deserialize(fs);
            }

            // Запись в CSV
            WriteCsv(apartments, csvPath);
            Console.WriteLine("Конвертация завершена: " + csvPath);
        }
        catch (Exception)
        {
            Console.WriteLine("Ошибка при конвертации");
        }

    }

    static void WriteCsv(APARTMENTS apartments, string path)
    {
        using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
        {
            // Заголовок CSV
            sw.WriteLine("ID;OBJECTID;OBJECTGUID;CHANGEID;NUMBER;APARTTYPE;OPERTYPEID;PREVID;PREVIDSpecified;NEXTID;NEXTIDSpecified;UPDATEDATE;STARTDATE;ENDDATE;ISACTUAL;ISACTIVE");

            foreach (var apartment in apartments.APARTMENT)
            {
                sw.WriteLine($"{apartment.ID};{apartment.OBJECTID};{apartment.OBJECTGUID};{apartment.CHANGEID};{apartment.NUMBER};{apartment.APARTTYPE};{apartment.OPERTYPEID};{apartment.PREVID};{apartment.PREVIDSpecified};{apartment.NEXTID};{apartment.NEXTIDSpecified};{apartment.UPDATEDATE},{apartment.STARTDATE};{apartment.ENDDATE};{apartment.ISACTUAL};{apartment.ISACTIVE}");
            }
        }
    }
}
