/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;

namespace umatiConnect
{
    public class UmatiNodeManager : CustomNodeManager2
    {
        public UmatiNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, "http://ifw.uni-hannover.de/umatiConnectDMG/")
        {
            SystemContext.NodeIdFactory = this;
        }

        public override NodeId New(ISystemContext context, NodeState node)
        {

        uint id = Utils.IncrementIdentifier(ref m_lastUsedId);
        return new NodeId(id, m_namespaceIndex);
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

            string resourcePathDI = "./Nodesets/Opc.Ua.Di.NodeSet2.xml";
            string resourcePathIA = "./Nodesets/Opc.Ua.IA.NodeSet2.xml";
            string resourcePathMachinery = "./Nodesets/Opc.Ua.Machinery.NodeSet2.xml";
            string resourcePathJobControl = "./Nodesets/opc.ua.isa95-jobcontrol.nodeset2.xml";
            string resourcePathMachineryJobs = "./Nodesets/Opc.Ua.Machinery.Jobs.Nodeset2.xml";
            string resourcePathMachineTool = "./Nodesets/Opc.Ua.MachineTool.NodeSet2.xml";
            string resourcePathCNC = "./Nodesets/Opc.Ua.CNC.NodeSet.xml";
            string resourcePathUmatiConnect = "./Nodesets/umaticonnectdmg.xml";

            ImportXml(externalReferences, resourcePathDI);
            ImportXml(externalReferences, resourcePathIA);
            ImportXml(externalReferences, resourcePathMachinery);
            ImportXml(externalReferences, resourcePathJobControl);
            ImportXml(externalReferences, resourcePathMachineryJobs);
            ImportXml(externalReferences, resourcePathMachineTool);
            ImportXml(externalReferences, resourcePathCNC);
            ImportXml(externalReferences, resourcePathUmatiConnect);

            Console.WriteLine("NodeSet2 XML imported successfully.");
            }
        }

        private void ImportXml(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcePath)
        {
            NodeStateCollection predefinedNodes = [];

            using (Stream stream = File.OpenRead(resourcePath))
            {
                var nodeSet = UANodeSet.Read(stream);

                foreach (var uri in nodeSet.NamespaceUris)
                {
                    // if namespace not in namespaceUris, add it
                    if (SystemContext.NamespaceUris.GetIndex(uri) != -1)
                    {
                        m_namespaceIndex = (ushort)SystemContext.NamespaceUris.GetIndex(uri);
                    }
                    else
                    {
                        m_namespaceIndex = (ushort)SystemContext.NamespaceUris.Count;
                        SystemContext.NamespaceUris.Append(uri);
                    }
                }

                nodeSet.Import(SystemContext, predefinedNodes);
                Console.WriteLine(predefinedNodes.Count + " nodes imported from " + resourcePath);

                NodeState topLevelNode = null;

                foreach (var node in predefinedNodes)
                {
                    // Console.WriteLine($"Adding node: {node.BrowseName} [{node.NodeId}]");
                    AddPredefinedNode(SystemContext, node);

                    // Capture the top-level node to be added to the ObjectsFolder, without this the nodes are not visible in the server, because they are not referenced by the ObjectsFolder
                    if (node.BrowseName.Name == "DMGMilltap700")
                    // if (node.NodeId.Identifier is uint id && id == 1002)
                    {
                        Console.WriteLine($"Found top-level node: {node.BrowseName} [{node.NodeId}]");
                        topLevelNode = node;
                    }
                }

        #region TopLevelNode Reference
        if (topLevelNode != null)
        {
            Console.WriteLine("Hooking top-level node to ObjectsFolder...");

            // Make sure ObjectsFolder has a reference to this node
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
            {
                references = new List<IReference>();
                externalReferences[ObjectIds.ObjectsFolder] = references;
            }

            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, topLevelNode.NodeId));

            // And the reverse reference
            topLevelNode.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
        }
        // else
        // {
        //     Console.WriteLine("Top-level node not found.");
        // }
                // ensure the reverse references exist.
                AddReverseReferences(externalReferences);
            }
        }
        #endregion


        #region Private Fields
        private long m_lastUsedId;
        private ushort m_namespaceIndex;
        #endregion
    }
}

