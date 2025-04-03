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
                Console.WriteLine("MasterNodeManager not found.");
                return;
            }

            // Retrieve the UmatiNodeManager by its namespace URI
            ushort namespaceIndex = (ushort)_server.CurrentInstance.NamespaceUris.GetIndex("http://ifw.uni-hannover.de/umatiConnectDMG/");
            var nodeManager = masterNodeManager.NodeManagers[namespaceIndex] as UmatiNodeManager;
            if (nodeManager == null)
            {
                Console.WriteLine("UmatiNodeManager not found.");
                return;
            }

            foreach (var mappedObject in mappedObjects)
            {
                string opcPath = "Objects/Machines/" + machine + "/" + mappedObject.OpcPath;
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