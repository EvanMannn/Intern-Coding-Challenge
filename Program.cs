using System.Globalization;
using CsvHelper;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

class Program {
    static void Main(string[] args) {
        using (var reader = new StreamReader("./SensorData1.csv")) 
        using (var csv_input = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var data = csv_input.GetRecords<csvData>();
            csvData.printCsv(data);
        }
    }

}

public class csvData {
    public int id {get; set; }
    public float latitude {get; set; }
    public float longitude {get; set; }

    public static void printCsv(IEnumerable<csvData> data) {
        foreach (var entry in data) {
            Console.WriteLine($"ID: {entry.id}, LATITUDE: {entry.latitude}, LONGITUDE: {entry.longitude}");
        }
    }
}