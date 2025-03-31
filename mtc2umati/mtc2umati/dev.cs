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