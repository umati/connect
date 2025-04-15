/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;

namespace mtc2umati.Services
{
    public class UmatiNodeManager : CustomNodeManager2
    {

        public UmatiNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration,
            "http://opcfoundation.org/UA/DI/",
            "http://opcfoundation.org/UA/Machinery/",
            "http://opcfoundation.org/UA/ISA95-JOBCONTROL_V2/",
            "http://opcfoundation.org/UA/Machinery/Jobs/",
            "http://opcfoundation.org/UA/MachineTool/",
            "http://opcfoundation.org/UA/IA/",
            "http://opcfoundation.org/UA/CNC",
            ConfigStore.VendorSettings.OPCNamespace)
        {
            SystemContext.NodeIdFactory = this;
        }

        public IEnumerable<NodeState> GetPredefinedNodes()
        {
            return PredefinedNodes.Values;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                IList<IReference> references = [];

                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references!))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = [];
                }

                Console.WriteLine("Importing NodeSet2 XML...");

                string resourcePathDI = "./Nodesets/Opc.Ua.Di.NodeSet2.xml";
                string resourcePathIA = "./Nodesets/Opc.Ua.IA.NodeSet2.xml";
                string resourcePathMachinery = "./Nodesets/Opc.Ua.Machinery.NodeSet2.xml";
                string resourcePathJobControl = "./Nodesets/Opc.Ua.ISA95-JobControl.NodeSet2.xml";
                string resourcePathMachineryJobs = "./Nodesets/Opc.Ua.Machinery.Jobs.Nodeset2.xml";
                string resourcePathMachineTool = "./Nodesets/Opc.Ua.MachineTool.NodeSet2.xml";
                string resourcePathCNC = "./Nodesets/Opc.Ua.CNC.NodeSet.xml";
                string resourcePathUmatiConnect = $"./Nodesets/{ConfigStore.VendorSettings.Information_model}";

                ImportXml(externalReferences, resourcePathDI);
                ImportXml(externalReferences, resourcePathIA);
                ImportXml(externalReferences, resourcePathMachinery);
                ImportXml(externalReferences, resourcePathJobControl);
                ImportXml(externalReferences, resourcePathMachineryJobs);
                ImportXml(externalReferences, resourcePathMachineTool);
                ImportXml(externalReferences, resourcePathCNC);
                ImportXml(externalReferences, resourcePathUmatiConnect);
            }
        }

        private void ImportXml(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcePath)
        {
            NodeStateCollection predefinedNodes = [];

            Stream stream = new FileStream(resourcePath, FileMode.Open);

            var nodeSet = new UANodeSet();
            try
            {
                nodeSet = UANodeSet.Read(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading NodeSet XML: {e.Message}");
                return;
            }


            // nodeSet.NamespaceUris is an ordered list — use it directly
            foreach (var uri in nodeSet.NamespaceUris)
            {
                if (SystemContext.NamespaceUris.GetIndex(uri) == -1)
                {
                    SystemContext.NamespaceUris.Append(uri);
                }
            }

            m_namespaceIndex = (ushort)SystemContext.NamespaceUris.GetIndex(nodeSet.NamespaceUris[0]);
            Console.WriteLine($"Namespace index for '{nodeSet.NamespaceUris[0]}' is {m_namespaceIndex}.");

            nodeSet.Import(SystemContext, predefinedNodes);

            Console.WriteLine(predefinedNodes.Count + " nodes imported from " + resourcePath);

            foreach (var node in predefinedNodes)
            {
                AddPredefinedNode(SystemContext, node);
                //if namespaceindex is 9 print info:
                if (node.NodeId.NamespaceIndex == 9)
                {
                    Console.WriteLine($"NodeId: {node.NodeId}, BrowseName: {node.BrowseName}, DisplayName: {node.DisplayName}");
                }
            }


            // ensure the reverse references exist.
            AddReverseReferences(externalReferences);
        }

        #region Private Fields
        private ushort m_namespaceIndex;
        #endregion
    }
}

