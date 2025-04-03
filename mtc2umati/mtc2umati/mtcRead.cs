/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace umatiConnect
{


// Fetch XML from a given URL and port
public class XmlFetcher
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<XDocument?> FetchXmlAsync(string url, int port)
    {
        try
        {
            string fullUrl = $"{url}:{port}/current";  // URL für die XML-Abfrage
            var response = await httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();  // Prüfen, ob die Anfrage erfolgreich war

            string xmlContent = await response.Content.ReadAsStringAsync();
            XDocument xmlDoc = XDocument.Parse(xmlContent);

            return xmlDoc;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to fetch XML: {ex.Message}");
            return null;
        }
    }
}

public class XmlMapper
{
    private static readonly XmlNamespaceManager NamespaceManager;

    static XmlMapper()
    {
        NamespaceManager = new XmlNamespaceManager(new System.Xml.NameTable());
        //NamespaceManager.AddNamespace("mt", "urn:mtconnect.org:MTConnectStreams:2.0"); // for Mazak
        NamespaceManager.AddNamespace("mt", "urn:mtconnect.org:MTConnectStreams:1.3"); // for DMG --> add to config file
    }

    public List<MappedObject> MapXmlValues(XDocument xmlDoc, List<MappedObject> mappedObjects)
    {
        foreach (var mappedObject in mappedObjects)
        {
            var mtcPathParts = mappedObject.MtcPath.Split('/');
            if (mtcPathParts.Length != 3)
            {
                Console.WriteLine($"[ERROR] Invalid MTC Path: {mappedObject.MtcPath}");
                continue;
            }

            string componentType = mtcPathParts[0];
            string componentName = mtcPathParts[1];
            string dataItemName = mtcPathParts[2];
            string subType = mappedObject.MtcSubtype;

            var componentXPath = $"//mt:ComponentStream[@component='{componentType}' and @name='{componentName}']";
            var component = xmlDoc.XPathSelectElement(componentXPath, NamespaceManager);

            if (component != null)
            {
                var valueElement = subType != ""
                    ? FindDataItemSubtypeRecursive(component, dataItemName, subType)
                    : FindDataItemRecursive(component, dataItemName);

                if (valueElement?.Value is not null && mappedObject?.MtcDataType is not null)
                {
                    var value = valueElement.Value;
                    var dataType = mappedObject.MtcDataType;
                    var convertedValue = ConvertValue(value, dataType);

                    if (convertedValue is not null)
                    {
                        mappedObject.Value = convertedValue;
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Conversion returned null for value '{value}' and data type '{dataType}'");
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR] Value '{dataItemName}'{(string.IsNullOrEmpty(subType) ? "" : $" with subtype '{subType}'")} not found"
                        + (component is null ? $" within component: {(mappedObject != null ? mappedObject.MtcPath : "unknown")}" : ""));
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] Component '{componentType}' with name '{componentName}' not found in XML.");
            }
        }
        return mappedObjects;
    }



    // Rekursive Methode zum Durchsuchen aller Unterelemente ohne Subtype
    private static XElement? FindDataItemRecursive(XElement parentElement, string dataItemName)
    {
        if (parentElement.Name.LocalName.Equals(dataItemName, StringComparison.OrdinalIgnoreCase))
        {
            return parentElement;
        }

        foreach (var child in parentElement.Elements())
        {
            var result = FindDataItemRecursive(child, dataItemName);
            if (result != null)
                return result;
        }

        return null;
    }


    // Recursive method to search for DataItem with SubType
    private static XElement? FindDataItemSubtypeRecursive(XElement parentElement, string dataItemName, string subType)
    {
        if (parentElement.Name.LocalName.Equals(dataItemName, StringComparison.OrdinalIgnoreCase))
        {
            var subTypeAttribute = parentElement.Attribute("subType");
            if (subTypeAttribute != null && subTypeAttribute.Value.Equals(subType, StringComparison.OrdinalIgnoreCase))
            {
                return parentElement;
            }
        }

        foreach (var child in parentElement.Elements())
        {
            var result = FindDataItemSubtypeRecursive(child, dataItemName, subType);
            if (result != null)
                return result;
        }

        return null;
    }

    private static object? ConvertValue(string value, string dataType)
    {
        try
        {
            switch (dataType.ToLower())
            {
                case "int":
                case "integer":
                    return int.Parse(value);
                case "double":
                case "float":
                    return double.Parse(value);
                case "bool":
                case "boolean":
                    return bool.Parse(value);
                default:
                    return value;  // String or unknown type, return as is
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to convert value '{value}' to type '{dataType}': {ex.Message}");
            return null;
        }
    }
}

// Cyclic XML fetch logic
public static class XmlFetchLoopRunner
{
    public static async Task RunXmlFetchLoopAsync(string url, int port, List<MappedObject> mappedObjects, int intervalInMilliseconds = 2000)
    {
        Console.WriteLine("Starting XML fetch loop...");

        var cancellationTask = Task.Run(() => Console.ReadKey(true));

        var xmlMapper = new XmlMapper();

        while (true)
        {
            // Fetch XML data
            XDocument? xmlDoc = await XmlFetcher.FetchXmlAsync(url, port);
            if (xmlDoc == null)
            {
                Console.WriteLine("[WARN] Failed to fetch XML, skipping iteration.");
                continue;
            }

            else
            {
                mappedObjects = xmlMapper.MapXmlValues(xmlDoc, mappedObjects);

                // +++++++++++++++++ Print the mapped objects after fetching XML data +++++++++++++++++++++
                mappedObjects.ShowMappedObjects();
            }


            // Intervall abwarten oder auf Tastendruck reagieren
            var delayTask = Task.Delay(intervalInMilliseconds);
            var completedTask = await Task.WhenAny(delayTask, cancellationTask);

        }
    }

}
}