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

namespace mtc2umati.Services
{
public class XmlFetcher
{
    private static readonly HttpClient httpClient = new();

    public static async Task<XDocument?> FetchXmlAsync(string url, int port)
    {
        try
        {
            string fullUrl = $"{url}:{port}/current";
            var response = await httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.

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
    private readonly XmlNamespaceManager _namespaceManager;

    public XmlMapper(string mtcNamespace)
    {
        _namespaceManager = new XmlNamespaceManager(new NameTable());
        _namespaceManager.AddNamespace("mt", mtcNamespace);
        Console.WriteLine($"[INFO] Namespace manager initialized with namespace: {mtcNamespace}");
    }

    public List<MappedObject> MapXmlValues(XDocument xmlDoc, List<MappedObject> mappedObjects)
    {
        foreach (var mappedObject in mappedObjects)
        {
            // if mappedObject.MtcPath starts with # then use this entry as the mappedObject.Value and strip the # character
            if (mappedObject.MtcPath.StartsWith("#"))
            {
                mappedObject.Value = mappedObject.MtcPath[1..];
                Console.WriteLine($"[INFO] Special Value {mappedObject.OpcPath} will be explicitly set to: {mappedObject.Value}");
                continue;
            }

            var mtcPathParts = mappedObject.MtcPath.Split('/');
            if (mtcPathParts.Length != 3) // this assumption needs to be changed for special cases / or maybe add another special case and special logic for Stacklight etc.
            {
                //Console.WriteLine($"[ERROR] Invalid MTC Path: {mappedObject.MtcPath}");
                mappedObject.Value = "Placeholder";
                continue;
            }

            string componentType = mtcPathParts[0];
            string componentName = mtcPathParts[1];
            string dataItemName = mtcPathParts[2];
            string subType = mappedObject.MtcSubtype;

            var componentXPath = $"//mt:ComponentStream[@component='{componentType}' and @name='{componentName}']";
            var component = xmlDoc.XPathSelectElement(componentXPath, _namespaceManager);

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


    // Recursive method to search for DataItem without SubType
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
                return dataType.ToLower() switch
                {
                    "int" or "integer" => int.Parse(value),
                    "double" or "float" => double.Parse(value),
                    "bool" or "boolean" => bool.Parse(value),
                    _ => value,// String or unknown type, return as is
                };
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
    public static async Task RunXmlFetchLoopAsync(string url, int port, string mtcNamespace, List<MappedObject> mappedObjects)
    {
        var cancellationTask = Task.Run(() => Console.ReadKey(true));

        var xmlMapper = new XmlMapper(mtcNamespace);

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
                //mappedObjects.ShowMappedObjects();
            }


            // Intervall abwarten oder auf Tastendruck reagieren
            var delayTask = Task.Delay(3000); // 3 seconds delay
            var completedTask = await Task.WhenAny(delayTask, cancellationTask);

        }
    }

}
}