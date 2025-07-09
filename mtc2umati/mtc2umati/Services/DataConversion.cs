/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/
using Opc.Ua;

namespace mtc2umati.Services
{
    public class DataConverter
    {
        public static object ConvertValue(MappedObject mappedObject)
        {
            #region DataType conversion
            if (mappedObject.Value?.ToString() == "UNAVAILABLE")
            {
                return mappedObject.Value;
            }
            switch (mappedObject.OpcDataType)
            {
                case "LocalizedText" when mappedObject.Value is string localizedTextVal:
                    mappedObject.Value = new LocalizedText("en", localizedTextVal);
                    break;
                
                case "Double":
                    TryConvert(value => Convert.ToDouble(value), mappedObject);
                    break;

                case "UInt32":
                    //TryConvert(value => Convert.ToDouble(value), mappedObject);
                    TryConvert(value => Convert.ToInt32(value), mappedObject);
                    break;

                case "UInt16":
                    TryConvert(value => Convert.ToUInt16(value), mappedObject);
                    break;

                case "DateTime":
                    TryConvert(value => Convert.ToDateTime(value), mappedObject);
                    break;

                case "Duration":
                    TryConvert(value => Convert.ToDouble(value), mappedObject);
                    break;
                
                case "Range":
                    // OPC UA Range = ExtensionObject
                    try
                    {
                        var parts = mappedObject.Value?.ToString()?.Split(',');
                        if (parts?.Length == 2 && double.TryParse(parts[0], out var low) && double.TryParse(parts[1], out var high))
                        {
                            mappedObject.Value = new ExtensionObject(new Opc.Ua.Range
                            {
                                Low = low,
                                High = high
                            });
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"[ERROR] Value '{mappedObject.Value}' of '{mappedObject.MtcName}' in '{mappedObject.OpcPath} could not be converted to {mappedObject.OpcDataType}.");
                    }
                    break;
                    
                case "EUInformation":
                    // OPC UA EUInformation = ExtensionObject
                    try
                    {
                        var parts = mappedObject.Value?.ToString()?.Split(',');
                        if (parts?.Length == 2)
                        {
                            mappedObject.Value = new ExtensionObject(new EUInformation
                            {
                                NamespaceUri = ConfigStore.VendorSettings.OPCNamespace,
                                UnitId = 0,
                                DisplayName = new LocalizedText("en", parts[0].ToString()),
                                Description = new LocalizedText("en", parts[1].ToString())
                            });
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"[ERROR] Value '{mappedObject.Value}' of '{mappedObject.MtcName}' in '{mappedObject.OpcPath} could not be converted to {mappedObject.OpcDataType}.");
                    }
                    break;
            }
            #endregion
            
            #region Specific conversion
            if (mappedObject.MtcName.StartsWith("LightState") && mappedObject.Value is string lightStateVal)
            {                
                mappedObject.Value = lightStateVal switch
                {
                    "UNAVAILABLE" => null,
                    "OFF" => 0,
                    "ON" => 1,
                    _ => lightStateVal
                };
            }

            else if (mappedObject.MtcName == "PowerOnTime")
            {
                if (double.TryParse(mappedObject.Value?.ToString(), out var val))
                {
                    mappedObject.Value = val / 3600; // * 1000.0;
                    mappedObject.Value = Math.Round((double)mappedObject.Value, 0);
                }
            }

            else if (mappedObject.MtcName == "ControllerMode" && mappedObject.Value is string controllerModeVal)
            {
                mappedObject.Value = controllerModeVal switch
                {
                    "AUTOMATIC" => 0,
                    "MANUAL" => 2,
                    _ => controllerModeVal
                };
            }
            else if (mappedObject.MtcName == "OperationMode" && mappedObject.Value is string operationModeVal)
            {
                mappedObject.Value = operationModeVal switch
                {
                    "AUTOMATIC" => 1,
                    "MANUAL" => 0,
                    "MANUAL_DATA_INPUT" => 2,
                    _ => operationModeVal
                };
            }
            else if (mappedObject.MtcName == "Execution" && mappedObject.Value is LocalizedText ExecutionVal)
            {
                mappedObject.Value = ExecutionVal switch
                {
                    { Text: "READY" } => new LocalizedText("en", "Initializing"),
                    { Text: "ACTIVE" } => new LocalizedText("en", "Running"),
                    { Text: "STOPPED" } => new LocalizedText("en", "Ended"),
                    _ => ExecutionVal
                };
            }
            #endregion

            return mappedObject.Value ?? throw new InvalidOperationException("Value cannot be null.");
        }

        #region Conversion helper
        private static void TryConvert(Func<object, object> convertFunc, MappedObject mappedObject)
        {
            if (mappedObject.Value != null)
            {
                try
                {
                    mappedObject.Value = convertFunc(mappedObject.Value);
                }
                catch (Exception)
                {
                    //Console.WriteLine($"[ERROR] Value '{mappedObject.Value}' of '{mappedObject.MtcName}' in '{mappedObject.OpcPath} could not be converted to {mappedObject.OpcDataType}.");
                }
            }
        }
        #endregion
    }
}


            

