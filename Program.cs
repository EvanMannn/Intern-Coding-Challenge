using System.Globalization;
using CsvHelper;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program {
    static void Main(string[] args) {
        Sensor jsonSensor = new("./SensorData2.json");
        Sensor csvSensor = new("./SensorData1.csv");

        Console.WriteLine("~~~~~~JSON DATA~~~~~~");
        jsonSensor.printData();

        Console.WriteLine("~~~~~~CSV DATA~~~~~~");
        csvSensor.printData();
    }
}

public class Sensor {
    private List<SensorData> data { get; } = new List<SensorData>();
    
    public Sensor(string filePath) {
        if (filePath.Contains(".json")) {
            data = LoadJsonData(filePath);
        }
        else if (filePath.Contains(".csv")) {
            data = LoadCsvData(filePath);
        }
    }

    private List<SensorData> LoadCsvData(string filePath) {
        //Loading CSV data
        using (var csv_reader = new StreamReader("./SensorData1.csv")) 
        using (var csv_input = new CsvReader(csv_reader, CultureInfo.InvariantCulture))
        {
            return [.. csv_input.GetRecords<SensorData>()];
        }
    }

    private List<SensorData> LoadJsonData(string filePath) {
        //Loading JSON data
        string json_input = File.ReadAllText("./SensorData2.json");
        return JsonConvert.DeserializeObject<List<SensorData>>(json_input) ?? new List<SensorData>();
    }

    public void printData() {
        foreach (var datum in data) {
            Console.WriteLine($"ID: {datum.id} LATITUDE: {datum.latitude} LONGITUDE: {datum.longitude}");
        }
    }
}

public class SensorData {
    public int id {get; set; }
    public float latitude {get; set; }
    public float longitude {get; set; }
}
