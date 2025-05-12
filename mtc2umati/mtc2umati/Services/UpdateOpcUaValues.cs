/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using System.Reflection;
using Opc.Ua;

namespace mtc2umati.Services
{
    public class UmatiWriter(UmatiServer server)
    {
        private readonly UmatiServer _server = server;

        public async Task UpdateNodesAsync(List<MappedObject> mappedObjects)
        {
            var masterNodeManager = _server.CurrentInstance.NodeManager;
            if (masterNodeManager == null)
            {
                Console.WriteLine("[ERROR] MasterNodeManager not found.");
                return;
            }

            string namespaceUri = ConfigStore.VendorSettings.OPCNamespace!;
            ushort namespaceIndex = (ushort)_server.CurrentInstance.NamespaceUris.GetIndex(namespaceUri);
            Console.WriteLine($"[INFO] Namespace index for '{namespaceUri}' is {namespaceIndex}.");

            var nodeManager = masterNodeManager.NodeManagers.OfType<UmatiNodeManager>().FirstOrDefault();

            // Find the parent node using the machine name defined in the config.json file and namespace index
            NodeState? parentNode = nodeManager?.GetPredefinedNodes()?.FirstOrDefault(n =>
                    n?.BrowseName?.Name != null &&
                    n.BrowseName.Name.Equals(ConfigStore.VendorSettings.Machine_Name, StringComparison.OrdinalIgnoreCase) &&
                    n.NodeId?.NamespaceIndex == namespaceIndex);

            Console.WriteLine($"MachineNodeId: {parentNode?.NodeId}, BrowseName: {parentNode?.BrowseName}");
            while (true)
            {
                WriteValuesToNodes(mappedObjects, nodeManager, parentNode);
                await Task.Delay(1000); // Wait before updating again
            }
        }

        public static void WriteValuesToNodes(List<MappedObject> mappedObjects, UmatiNodeManager? nodeManager, NodeState? parentNode)
        {
            if (nodeManager == null || parentNode == null)
            {
                Console.WriteLine("[ERROR] NodeManager or ParentNode is null.");
                return;
            }

            // Update the DisplayName and BrowseName of the predefined machine node with the name that was acutally found in the XML file
            // This is usefull, since the machine name in the config.json file and information model can be generic
            parentNode.DisplayName = new LocalizedText("en", ConfigStore.VendorSettings.ActualModelName!);
            parentNode.BrowseName = new QualifiedName(ConfigStore.VendorSettings.ActualModelName!, parentNode.NodeId.NamespaceIndex);


            foreach (var mappedObject in mappedObjects)
            {
                string opcPath = mappedObject.OpcPath;

                string[] opcPathParts = opcPath.Split('/');

                NodeState? currentNode = parentNode;

                // Using Child-Parent-References would be easier, but is currently not working, since the Child-Parent-References are not set up when importing the XML NodeSet
                // var childReferences = new List<BaseInstanceState>();
                // node.GetChildren(nodeManager.SystemContext, childReferences);
                // Console.WriteLine($"Children found: {childReferences.Count}");

                for (int i = 0; i < opcPathParts.Length; i++)
                {
                    var targetBrowseName = opcPathParts[i].Trim();

                    var references = new List<IReference>();
                    currentNode?.GetReferences(nodeManager?.SystemContext, references);

                    foreach (var reference in references)
                    {
                        var targetNodeId = reference.TargetId; // get the node id from the reference

                        // Look for the node by its NodeId
                        var node = nodeManager?.GetPredefinedNodes()
                                    .FirstOrDefault(n => n.NodeId.Equals(targetNodeId));

                        // If the node matches the targetBrowseName, the correct node was found
                        if (node != null && node.BrowseName?.Name == targetBrowseName)
                        {
                            currentNode = node;
                        }
                    }
                }
                #region Data conversion
                if (mappedObject.Value is not null and not (object)"UNAVAILABLE")
                {
                    mappedObject.Value = DataConverter.ConvertValue(mappedObject);
                }
                #endregion

                #region Mode handling
                // [MODE 1] When the adapter mode is set to 1 in the config, the value of the new nodes is set to null.
                if ((ConfigStore.VendorSettings.Mode == 1 && mappedObject.ModellingRule == "New") ||
                    mappedObject.ModellingRule == "DMG specific")
                {
                    mappedObject.Value = null;
                }

                // [MODE 2] Default: When the adapter mode is set to 2 in the config, the new nodes are inside the Server tree with normal values.

                // [MODE 3] When the adapter mode is set to 3 in the config, add the reference to the MTConnect folder for newly added nodes.
                if ((ConfigStore.VendorSettings.Mode == 3 && mappedObject.ModellingRule == "New") ||
                    mappedObject.ModellingRule == "DMG specific")
                {
                    if (currentNode != null && nodeManager != null)
                    {
                        AddHasAddInReference(currentNode, nodeManager, mappedObject);
                    }
                }
                #endregion
                #region Value updating
                if (currentNode is BaseVariableState variableNode)
                {
                    // Only update the value if it changed
                    if (variableNode.Value?.ToString() != mappedObject.Value?.ToString())
                    {
                        variableNode.Value = mappedObject.Value;
                        variableNode.Timestamp = DateTime.UtcNow;
                        variableNode.StatusCode = StatusCodes.Good;
                        variableNode.ClearChangeMasks(nodeManager?.SystemContext, true);
                        Console.WriteLine($"Updated node '{mappedObject.OpcPath}' to value '{mappedObject.Value}'.");
                    }
                }
                else
                {
                    Console.WriteLine($"Node '{currentNode?.BrowseName?.Name}' in '{mappedObject.MtcPath}' --> '{mappedObject.OpcPath}' is not a variable (PropertyState or BaseDataVariableState).");
                }
                #endregion
            }
        }

        public static void AddHasAddInReference(NodeState node, UmatiNodeManager nodeManager, MappedObject mappedObject)
        {
            var folder = ConfigStore.VendorSettings.MTConnect_FolderState;
            if (folder != null && node != null)
            {
                var existingReferences = new List<IReference>();
                folder.GetReferences(nodeManager.SystemContext, existingReferences);

                bool referenceExists = existingReferences.Any(r =>
                    r.ReferenceTypeId == ReferenceTypeIds.HasAddIn &&
                    r.TargetId.Equals(node.NodeId) &&
                    !r.IsInverse);

                if (!referenceExists)
                {
                    folder.AddReference(ReferenceTypeIds.HasAddIn, false, node.NodeId);
                    node.AddReference(ReferenceTypeIds.HasAddIn, true, folder.NodeId);

                    folder.ClearChangeMasks(nodeManager.SystemContext, false);
                    node.ClearChangeMasks(nodeManager.SystemContext, false);

                    Console.WriteLine($"Added HasAddIn reference to MTConnect folder and node '{mappedObject.OpcPath}'.");
                }
            }
        }
    }
}



