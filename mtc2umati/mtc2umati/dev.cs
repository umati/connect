using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Export;


            // try
            // {
            //     SystemContext.NamespaceUris = Server.NamespaceUris;
            //     SystemContext.TypeTable = Server.TypeTree;

            //     #region Import NodeSet2 XML
            //     string path = Path.Combine("models", "umaticonnectdmg.xml");
            //     if (!File.Exists(path))
            //     {
            //         Console.WriteLine($"NodeSet2 file not found: {path}");
            //         return;
            //     }

            //     using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            //     {
            //         var nodeSet = UANodeSet.Read(stream);
            //         var importedNodes = new NodeStateCollection();
            //         nodeSet.Import(SystemContext, importedNodes);
            //         Console.WriteLine("NodeSet2 XML imported successfully.");

            //         // there should be a method for automatically adding the nodes correctly.
            //         // look into modelcompiler for this

            //         // basic logic for adding nodes to the server with private methods
            //         foreach (var node in importedNodes)
            //         {
            //             //Console.WriteLine($"Importing node: {node}");
            //             QualifiedName browseName = node.BrowseName;
            //             NodeId referenceTypeId = ReferenceTypeIds.Organizes;
            //             NodeId parentId = ObjectIds.ObjectsFolder; // Default parent is ObjectsFolder

            //             if (node is BaseObjectState baseObject)
            //             {
            //                 FolderState folder = CreateFolder(parent: null, path: browseName.Name, name: browseName.Name);
            //                 folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            //                 references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, folder.NodeId));
            //                 AddPredefinedNode(SystemContext, folder);
            //             }
            //             else
            //             {
            //                 BaseDataVariableState variable = CreateVariableState(parent: null, path: browseName.Name, name: browseName.Name, dataType: 0, defaultValue: string.Empty);
            //                 variable.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            //                 references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, variable.NodeId));
            //                 AddPredefinedNode(SystemContext, variable);
            //             }
            //     }

            // }
        //     // }

        //         catch (Exception ex)
        //         {
        //             Console.WriteLine($"Error importing NodeSet2 XML: {ex.Message}");
        //         }
        //     }
        // }
        // #endregion

#region Folder and variable creation

//         #region Folder and variable creation
//         private FolderState CreateFolder(NodeState? parent, string path, string name)
//         {
//             FolderState folder = new FolderState(parent) {
//                 SymbolicName = name,
//                 ReferenceTypeId = ReferenceTypes.Organizes,
//                 TypeDefinitionId = ObjectTypeIds.FolderType,
//                 NodeId = new NodeId(path, NamespaceIndex),
//                 BrowseName = new QualifiedName(path, NamespaceIndex),
//                 DisplayName = new Opc.Ua.LocalizedText("en", name),
//                 WriteMask = AttributeWriteMask.None,
//                 UserWriteMask = AttributeWriteMask.None,
//                 EventNotifier = EventNotifiers.None
//             };

//             if (parent != null)
//             {
//                 parent.AddChild(folder);
//             }
//             return folder;
//         }

//         private BaseDataVariableState CreateVariableState(NodeState? parent, string path, string name, uint dataType, object defaultValue)
//         {
//             BaseDataVariableState variable = new BaseDataVariableState(parent)
//             {
//                 SymbolicName = name,
//                 ReferenceTypeId = ReferenceTypes.Organizes,
//                 TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
//                 NodeId = new NodeId(path, NamespaceIndex),
//                 BrowseName = new QualifiedName(path, NamespaceIndex),
//                 DisplayName = new Opc.Ua.LocalizedText("en", name),
//                 WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
//                 UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
//                 DataType = dataType,
//                 Value = defaultValue,
//                 ValueRank = ValueRanks.Scalar,
//                 AccessLevel = AccessLevels.CurrentReadOrWrite,
//                 UserAccessLevel = AccessLevels.CurrentReadOrWrite,
//                 Historizing = false,
//                 StatusCode = StatusCodes.Good,
//                 Timestamp = DateTime.UtcNow
//             };

//             if (parent != null)
//             {
//                 parent.AddChild(variable);
//             }

//             return variable;
//         }
//         #endregion

// }
// }


// namespace umatiConnect
// {


// public UmatiNodeManager(IServerInternal server, ApplicationConfiguration configuration, string instanceNamespace, JobSettings jobSettings, ILogger<WireHarnessNodeManager> log)
//   : base(server,
//     configuration,
//     WireHarnessDeviceNamespaces.DI,
//     WireHarnessDeviceNamespaces.Machinery,
//     WireHarnessDeviceNamespaces.Machinery_Result,
//     WireHarnessDeviceNamespaces.ISA_95_JobControl,
//     WireHarnessDeviceNamespaces.Machinery_Jobs,
//     WireHarnessDeviceNamespaces.Wireharness,
//     instanceNamespace)

// }
#endregion




#region changing values


// using Opc.Ua;
// using Opc.Ua.Server;
// using System;
// using System.Threading.Tasks;

// namespace umatiConnect
// {
//     public class UmatiWriter
//     {
//         private readonly UmatiServer _server;

//         public UmatiWriter(UmatiServer server)
//         {
//             _server = server;
//         }

//         public async Task WriteValueAsync()
//         {
//             string nodeIdString = "ns=2;i=6033"; // Example NodeId string
//             string newValue = "TestValue"; // Example value to write

//             // Parse the NodeId
//             NodeId nodeId = new NodeId(nodeIdString);

//             // Access the MasterNodeManager
//             var masterNodeManager = _server.CurrentInstance.NodeManager;
//             if (masterNodeManager == null)
//             {
//                 Console.WriteLine("MasterNodeManager not found.");
//                 return;
//             }

//             // Retrieve the UmatiNodeManager by its namespace URI
//             ushort namespaceIndex = (ushort)_server.CurrentInstance.NamespaceUris.GetIndex("http://ifw.uni-hannover.de/umatiConnectDMG/");
//             var nodeManager = masterNodeManager.NodeManagers[namespaceIndex] as UmatiNodeManager;
//             if (nodeManager == null)
//             {
//                 Console.WriteLine("UmatiNodeManager not found.");
//                 return;
//             }

//             // Find the node
//             BaseDataVariableState variableNode = nodeManager.FindNode(nodeId);

//             if (variableNode != null)
//             {
//                 // Update the value of the node
//                 variableNode.Value = newValue;
//                 variableNode.Timestamp = DateTime.UtcNow;
//                 variableNode.StatusCode = StatusCodes.Good;

//                 // Notify the server about the value change
//                 variableNode.ClearChangeMasks(nodeManager.SystemContext, true);

//                 Console.WriteLine("Node value updated successfully.");
//             }
//             else
//             {
//                 Console.WriteLine("Node not found.");
//             }
//         }

//     }
// }
#endregion