/* ========================================================================
 * Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
 * =======================================================================*/

using System;
using Opc.Ua;
using Opc.Ua.Server;
using System.Collections.Generic;

namespace mtc2umati.Services
{
    public class UmatiServer : StandardServer
    {
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            Console.WriteLine("Creating custom node manager...");
            return new MasterNodeManager(server, configuration, null,
            [
                new UmatiNodeManager(server, configuration)
            ]);
        }
    }
}
