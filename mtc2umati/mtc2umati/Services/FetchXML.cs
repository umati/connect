// SPDX-License-Identifier: Apache-2.0
// Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mtc2umati.Services
{
    #region Fetch XML from MTC
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
                Console.WriteLine($"[ERROR] Failed to fetch XML: {ex.Message}, waiting 10 seconds before retrying...");
                await Task.Delay(10000);
                return null;
            }
        }
    }
    #endregion

    #region Read & convert data
    public class XmlMapper
    {
        private readonly XmlNamespaceManager _namespaceManager;
        public string _modelName = string.Empty; // Stores the model name from the DeviceStream

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
                if (mappedObject.MtcPath is not null)
                // There are three cases for the MtcPath to handle:
                // 1. The MTC value is static and set in the mapping.xlsx --> MtcPath starts with #, use this entry as the mappedObject.Value and strip the # character.
                // 2. Model name and AssedID/uuid are read in a specific way from the MTC DeviceStream --> MtcPath starts with <Device.
                // 3. [Default] The MTC value is the value of a <ComponentStream> element --> MtcPath consists of 3 elements split by "/". An element can be also be the <Device> placeholder.  
                {
                    if (mappedObject.MtcPath.StartsWith('#')) // Case 1
                    {
                        mappedObject.Value = mappedObject.MtcPath[1..];
                    }

                    else if (mappedObject.MtcPath.StartsWith("<Device")) // Case 2
                    {
                        var deviceStreamPaths = mappedObject.MtcPath.Split('/');
                        string variableName = deviceStreamPaths[1];
                        if (variableName == "name")
                        {
                            var deviceStream = xmlDoc.XPathSelectElement($"//mt:DeviceStream[@name!='Agent']", _namespaceManager);
                            _modelName = deviceStream?.Attribute("name")?.Value ?? string.Empty;
                            mappedObject.Value = _modelName;
                            ConfigStore.VendorSettings.ActualModelName = _modelName; // Save the model name in the ConfigStore to write it to the machine node later
                        }
                        else if (variableName == "uuid")
                        {
                            var deviceStream = xmlDoc.XPathSelectElement($"//mt:DeviceStream[@name!='Agent']", _namespaceManager);
                            string uuid = deviceStream?.Attribute("uuid")?.Value ?? string.Empty;
                            mappedObject.Value = uuid;
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Unknown variable name '{variableName}' in DeviceStream path.");
                        }
                    }

                    else if (mappedObject.MtcPath?.Split('/').Length == 3) // Case 3
                    {
                        var mtcPathParts = mappedObject.MtcPath.Split('/');
                        string componentType = mtcPathParts[0];
                        string componentName = mtcPathParts[1];
                        if (componentName == "{Machine}")
                        {
                            componentName = _modelName; // use the model name from the DeviceStream
                        }
                        string dataItemName = mtcPathParts[2].Trim();
                        string subType = mappedObject.MtcSubtype;

                        var componentXPath = $"//mt:ComponentStream[@component='{componentType}' and @name='{componentName}']";
                        var component = xmlDoc.XPathSelectElement(componentXPath, _namespaceManager);

                        if (component != null)
                        {
                            var valueElement = subType != ""
                                ? FindDataItemSubtypeRecursive(component, dataItemName, subType)
                                : FindDataItemRecursive(component, dataItemName);

                            if (valueElement != null)
                            {
                                mappedObject.Value = valueElement.Value;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (mappedObject?.Value is not null)
                    {
                        mappedObject.ConvertedValue = DataConverter.ConvertValue(mappedObject);
                    }
                }
            }
            return mappedObjects;
        }
        #endregion

        #region Helper methods
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
        #endregion
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
                    Console.WriteLine("[ERROR] Failed to fetch XML, skipping iteration.");
                    continue;
                }
                else
                {
                    mappedObjects = xmlMapper.MapXmlValues(xmlDoc, mappedObjects);
                    // Log the mapped objects for debugging
                    //mappedObjects.ShowMappedObjects();
                }
                var delayTask = Task.Delay(1000); // Time in milliseconds to wait between fetches
                var completedTask = await Task.WhenAny(delayTask, cancellationTask);
            }
        }
    }
}