/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;

namespace umatiConnect
{
    public class UmatiNodeManager : CustomNodeManager2
    {
        public UmatiNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, "http://ifw.uni-hannover.de/umatiConnect")
        {
            SystemContext.NodeIdFactory = this;
        }

        // <summary>
        /// Creates the NodeId for the specified node.
        /// </summary>
        public override NodeId New(ISystemContext context, NodeState node)
        {
            BaseInstanceState? instance = node as BaseInstanceState;

            if (instance != null && instance.Parent != null)
            {
                // string id = instance.Parent.NodeId.Identifier as string;

                // if (id != null)
                // {
                //     return new NodeId(id + "_" + instance.SymbolicName, instance.Parent.NodeId.NamespaceIndex);
                // }
            }
            return node.NodeId;
        }


        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                IList<IReference> references = new List<IReference>();

                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                }
            
            Console.WriteLine("Importing NodeSet2 XML...");

            try
            {
                SystemContext.NamespaceUris = Server.NamespaceUris;
                SystemContext.TypeTable = Server.TypeTree;

                #region Import NodeSet2 XML
                string path = Path.Combine("models", "umaticonnectdmg.xml");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"NodeSet2 file not found: {path}");
                    return;
                }

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var nodeSet = UANodeSet.Read(stream);
                    var importedNodes = new NodeStateCollection();
                    nodeSet.Import(SystemContext, importedNodes);
                    Console.WriteLine("NodeSet2 XML imported successfully.");

                    // there should be a method for automatically adding the nodes correctly. 
                    // look into modelcompiler for this

                    // basic logic for adding nodes to the server with private methods
                    foreach (var node in importedNodes)
                    {
                        //Console.WriteLine($"Importing node: {node}");
                        QualifiedName browseName = node.BrowseName;
                        NodeId referenceTypeId = ReferenceTypeIds.Organizes;
                        NodeId parentId = ObjectIds.ObjectsFolder; // Default parent is ObjectsFolder

                        if (node is BaseObjectState baseObject)
                        {
                            FolderState folder = CreateFolder(parent: null, path: browseName.Name, name: browseName.Name);
                            folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
                            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, folder.NodeId));
                            AddPredefinedNode(SystemContext, folder);
                        }
                        else
                        {
                            BaseDataVariableState variable = CreateVariableState(parent: null, path: browseName.Name, name: browseName.Name, dataType: 0, defaultValue: string.Empty);
                            variable.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
                            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, variable.NodeId));
                            AddPredefinedNode(SystemContext, variable);
                        }
                }
                
            }
            }

                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing NodeSet2 XML: {ex.Message}");
                }
            }
        }
        #endregion

        #region Folder and variable creation
        private FolderState CreateFolder(NodeState? parent, string path, string name)
        {
            FolderState folder = new FolderState(parent) {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new Opc.Ua.LocalizedText("en", name),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            if (parent != null)
            {
                parent.AddChild(folder);
            }
            return folder;
        }

        private BaseDataVariableState CreateVariableState(NodeState? parent, string path, string name, uint dataType, object defaultValue)
        {
            BaseDataVariableState variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId(path, NamespaceIndex),
                BrowseName = new QualifiedName(path, NamespaceIndex),
                DisplayName = new Opc.Ua.LocalizedText("en", name),
                WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
                DataType = dataType,
                Value = defaultValue,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow
            };

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }
        #endregion

}
}
