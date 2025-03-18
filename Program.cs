using System.Globalization;

//Dependancies
using CsvHelper;
using Newtonsoft.Json;

//Main program file that acts as an entrance for the main function
class Program {
    static void Main() {
        //Load the data for each of the files into new sensor classes
        Sensor csvSensor = new("./SensorData1.csv");
        Sensor jsonSensor = new("./SensorData2.json");

        //Compare the data and output the results into the JSON
        Sensor.compareSensors(csvSensor, jsonSensor);
    }
}

public class Sensor {
    //Container for the data gathered by the sensor
    private List<SensorData> data { get; } = new List<SensorData>();
    
    //Constructor that reads the filepath
    public Sensor(string filePath) {
        if (filePath.EndsWith(".json")) {
            data = LoadJsonData(filePath);
        }
        else if (filePath.EndsWith(".csv")) {
            data = LoadCsvData(filePath);
        }
        else {
            throw new ArgumentException($"{filePath} has a invalid file ending, valid endings include .csv and .json");
        }
    }

    //CSV data loader helper function
    private List<SensorData> LoadCsvData(string filePath) {
        //File ending check
        if (filePath.EndsWith(".csv")) {
            using (var csv_reader = new StreamReader(filePath)) 
            using (var csv_input = new CsvReader(csv_reader, CultureInfo.InvariantCulture))
            {
                return [.. csv_input.GetRecords<SensorData>()];
            }
        }
        //Return emtpy list if invalid
        else {
            Console.Error.WriteLine($"{filePath} has a invalid file ending, must be .csv. Returning empty list");
            return new List<SensorData>();
        }
    }

    //JSON data loader helper function
    private List<SensorData> LoadJsonData(string filePath) {
        //File ending check
        if (filePath.EndsWith(".json")) {
            string json_input = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<SensorData>>(json_input) ?? new List<SensorData>();
        }
        //Return empty list if invlaid
        else {
            Console.Error.WriteLine($"{filePath} has a invalid file ending, must be .json. Returning empty list");
            return new List<SensorData>();
        }
    }

    //Static method to comapre Sensors 
    public static void compareSensors(Sensor sensor1, Sensor sensor2) {
        //Input checking 
        if (sensor1 == null || sensor2 == null) {
            throw new ArgumentException("Invalid input, sensors cannot be null");
        }
        
        //Instantiate the output dictionary 
        Dictionary<string, int> correlatedSensorReadings = new Dictionary<string, int>();

        //Check each data point against each other datapoint with the distance function
        foreach (var sen1Data in sensor1.data) {
            foreach (var sen2Data in sensor2.data) {
                float distance = lambertEllipsodialDistance(sen1Data, sen2Data);
                if (distance < 100) {
                    //If the returned distance is < 100 m then add the correlation to the output dictionary 
                    correlatedSensorReadings[sen1Data.id.ToString()] = sen2Data.id;
                }
            }
        }

        //Convert the dictionary to JSON
        string outputJSON = JsonConvert.SerializeObject(correlatedSensorReadings, Formatting.Indented);
        File.WriteAllText("SensorDataComparisonOutput.json", outputJSON);
    }

    private static float lambertEllipsodialDistance(SensorData sd1, SensorData sd2) {
        const float EQUATORIAL_RADIUS = 6378137.0f;     //Earth's equatorial radius
        const float POLAR_RADIUS = 6356752.0f;          //Earth's polar radius

        //A coefficient to represent how flattened the Earth is.
        const float EARTH_FLATTENING = (EQUATORIAL_RADIUS - POLAR_RADIUS) / EQUATORIAL_RADIUS;

        //Angles in Radians. These conversions save having to convert to radians later on
        float lat1Rad = sd1.latitude * MathF.PI/180.0f;
        float long1Rad = sd1.longitude * MathF.PI/180.0f;
        float lat2Rad = sd2.latitude * MathF.PI/180.0f;
        float long2Rad = sd2.longitude * MathF.PI/180.0f;

        //Convert all angles to positive values
        if (lat1Rad < 0) lat1Rad += 2.0f * MathF.PI;
        if (long1Rad < 0) long1Rad += 2.0f * MathF.PI;
        if (lat2Rad < 0) lat2Rad += 2.0f * MathF.PI;
        if (long2Rad < 0) long2Rad += 2.0f * MathF.PI;

        //Latitudes reduced based on the flattening of the Earth
        float reducedLat1 = MathF.Atan((1.0f - EARTH_FLATTENING) * MathF.Tan(lat1Rad));
        float reducedLat2 = MathF.Atan((1.0f - EARTH_FLATTENING) * MathF.Tan(lat2Rad));
        
        float P = (reducedLat1 + reducedLat2) / 2.0f;
        float Q = (reducedLat2 - reducedLat1) / 2.0f;

        //The last element needed for the final calculation is the central angle between the two points
        //This is obtained using the haversine equation
        float havInternal = MathF.Sqrt(
                                MathF.Pow(MathF.Sin((lat2Rad - lat1Rad) / 2.0f), 2.0f) + 
                                MathF.Cos(lat1Rad) * 
                                MathF.Cos(lat2Rad) *
                                MathF.Pow(MathF.Sin((long2Rad - long1Rad) / 2.0f), 2.0f)
                            );
        float centralAngle = 2.0f * MathF.Asin(havInternal);

        //X and Y make up intermediate steps for the Lambert formula 
        float X =   (centralAngle - MathF.Sin(centralAngle)) *
                    MathF.Pow(MathF.Sin(P), 2.0f) *
                    MathF.Pow(MathF.Cos(Q), 2.0f) /
                    MathF.Pow(MathF.Cos(centralAngle / 2.0f), 2.0f);

        float Y =   (centralAngle + MathF.Sin(centralAngle)) *
                    MathF.Pow(MathF.Cos(P), 2.0f) *
                    MathF.Pow(MathF.Sin(Q), 2.0f) /
                    MathF.Pow(MathF.Sin(centralAngle / 2.0f), 2.0f);

        //Final distance
        return EQUATORIAL_RADIUS * (centralAngle - (EARTH_FLATTENING / 2.0f * (X + Y)));
    }

    //Helper function to print the data stored in a Sensor, used particularly in testing
    public void printData() {
        foreach (var datum in data) {
            Console.WriteLine($"ID: {datum.id} LATITUDE: {datum.latitude} LONGITUDE: {datum.longitude}");
        }
    }
}

//Sensor data container
public class SensorData {
    public int id {get; set; }
    public float latitude {get; set; }
    public float longitude {get; set; }
}

