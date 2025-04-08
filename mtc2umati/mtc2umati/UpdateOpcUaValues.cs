/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Threading.Tasks;

namespace umatiConnect
{
    public class UmatiWriter
    {
        private readonly UmatiServer _server;

        public UmatiWriter(UmatiServer server)
        {
            _server = server;
        }

        public async Task UpdateNodesAsync(List<MappedObject> mappedObjects, string machine)
        {
            var masterNodeManager = _server.CurrentInstance.NodeManager;
            if (masterNodeManager == null)
            {
                Console.WriteLine("[ERROR] MasterNodeManager not found.");
                return;
            }

            string namespaceUri = "http://ifw.uni-hannover.de/umatiConnectDMG/";
            ushort namespaceIndex = (ushort)_server.CurrentInstance.NamespaceUris.GetIndex(namespaceUri);
            Console.WriteLine($"Namespace index for '{namespaceUri}' is {namespaceIndex}.");

            var nodeManager = masterNodeManager.NodeManagers.OfType<UmatiNodeManager>().FirstOrDefault();

            Console.WriteLine($"NodeManager index: {nodeManager.NamespaceIndex}");
            foreach (var node in nodeManager.GetPredefinedNodes())
            {
                // if brosename = assedID then print the node
                if (node.BrowseName.Name == "AssetId")
                {
                    int value = 1234;

                    PropertyState variableNode = node as PropertyState;
                    if (variableNode != null)
                    {
                        variableNode.Value = value;
                        variableNode.Timestamp = DateTime.UtcNow;
                        variableNode.StatusCode = StatusCodes.Good;
                        variableNode.ClearChangeMasks(nodeManager.SystemContext, true);
                    }
                    else
                    {
                        Console.WriteLine($"Node '{node.BrowseName.Name}' is not a PropertyState.");
                    }
                    Console.WriteLine($"NodeId: {node.NodeId}, BrowseName: {node.BrowseName}");
                }

            }



            return;

            if (nodeManager == null)
            {
                Console.WriteLine("[ERROR] UmatiNodeManager not found.");
                return;
            }

            foreach (var mappedObject in mappedObjects)
            {
                string opcPath = "Objects/Machines/" + machine + "/" + mappedObject.OpcPath;
                Console.WriteLine($"Updating node '{opcPath}' with value '{mappedObject.Value}'.");
                NodeState parentNode = nodeManager.FindPredefinedNode(new NodeId("Objects/Machines/" + machine, namespaceIndex), typeof(NodeState));

                if (parentNode == null)
                {
                    Console.WriteLine($"Machine node '{machine}' not found.");
                    continue;
                }

                string[] opcPathParts = opcPath.Split('/');
                bool nodeFound = true;

                // Traverse the path except the last part
                for (int i = 0; i < opcPathParts.Length - 1; i++)
                {
                    parentNode = parentNode.FindChild(nodeManager.SystemContext, new QualifiedName(opcPathParts[i], namespaceIndex)) as NodeState;

                    if (parentNode == null)
                    {
                        Console.WriteLine($"Node '{opcPathParts[i]}' not found in path '{opcPath}'.");
                        nodeFound = false;
                        break;
                    }
                }

                if (!nodeFound)
                {
                    continue;
                }

                // Get the leaf node
                string variableName = opcPathParts.Last();
                var variableNode = parentNode.FindChild(nodeManager.SystemContext, new QualifiedName(variableName, namespaceIndex)) as BaseDataVariableState;

                if (variableNode != null)
                {
                    // Read current value from OPC UA
                    var currentValue = variableNode.Value;

                    // Compare old with new value and update if necessary
                    if (!Equals(currentValue, mappedObject.Value))
                    {
                        variableNode.Value = mappedObject.Value;
                        variableNode.Timestamp = DateTime.UtcNow;
                        variableNode.StatusCode = StatusCodes.Good;
                        variableNode.ClearChangeMasks(nodeManager.SystemContext, true);

                        Console.WriteLine($"Node '{variableName}' changed from '{currentValue}' to '{mappedObject.Value}'.");
                    }
                }
                else
                {
                    Console.WriteLine($"Leaf node '{variableName}' not found in path '{opcPath}'.");
                }
            }

            await Task.CompletedTask;
        }
    }
}