/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

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
            Console.WriteLine($"Namespace index for '{namespaceUri}' is {namespaceIndex}.");

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
            }
        }
    }
}



