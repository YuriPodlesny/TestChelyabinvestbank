using System.IO.Compression;
using System.Xml.Serialization;

namespace TestChelyabinvestbank;

internal class Program
{
    static async Task Main(string[] args)
    {
        if (File.Exists(Constants.ZIP_PATH))
        {
            Console.WriteLine("Файл существует.");
        }
        else
        {
            Directory.CreateDirectory(Constants.PATH_DIRECTORY_ZIP);
            await GetАrchiveGar(Constants.URL, Constants.ZIP_PATH);
        }

        Directory.CreateDirectory(Constants.PATH_DIRECTORY_FILES);
        bool isNotEmptyFolderExtract = Directory.GetFiles(Constants.EXTRACT_PATH).Length > 0 || Directory.GetDirectories(Constants.EXTRACT_PATH).Length > 0;
        if (!isNotEmptyFolderExtract)
        {
            GetFiles(Constants.ZIP_PATH, Constants.EXTRACT_PATH);
        }
        ConvertToCSV(Constants.FOLDER_PATH, Constants.CSV_PATH);
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
    public static void ConvertToCSV(string folderPath, string csvPath)
    {
        // Получаем массив путей к файлам
        string[] files = Directory.GetFiles(folderPath, $"*{Constants.SEARCH_WORD}*");

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
