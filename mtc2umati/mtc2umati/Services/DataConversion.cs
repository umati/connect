/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/


namespace mtc2umati.Services
{
    public class DataConverter
    {
        public static object ConvertValue(MappedObject mappedObject)
        {
            if (mappedObject.MtcName.StartsWith("LightState"))
            {
                if (mappedObject.Value is string strVal)
                {
                    mappedObject.Value = strVal switch
                    {
                        "UNAVAILABLE" => null,
                        "OFF" => 0,
                        "ON" => 1,
                        _ => strVal
                    };
                }
            }
            else if (mappedObject.MtcName == "PowerOnTime")
            {
                //Console.WriteLine($"Converting {mappedObject.Value} to");
                if (double.TryParse(mappedObject.Value?.ToString(), out var val))
                {
                    //mappedObject.Value = val * 1000.0;
                    //Console.WriteLine($"{mappedObject.Value} successfully.");
                }
                else
                {
                    throw new InvalidOperationException("PowerOnTime must be a number.");
                }
            }
            else if (mappedObject.MtcName == "ControllerMode")
            {
                if (mappedObject.Value is string strVal)
                {
                    //Console.WriteLine($"Converting {mappedObject.Value} to");
                    mappedObject.Value = strVal switch
                    {
                        "AUTOMATIC" => 0,
                        "MANUAL" => 1,
                        _ => strVal
                    };
                    //Console.WriteLine($"{mappedObject.Value} successfully");
                }
            }
            return mappedObject.Value ?? throw new InvalidOperationException("Value cannot be null.");
        }

    }
}


            

