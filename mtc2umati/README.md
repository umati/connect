# MTC2UMATI - MTConnect to OPC UA umati Bridge

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

## Overview

MTC2UMATI is a .NET 9.0 application that serves as a bridge between MTConnect data sources and OPC UA umati (universal machine tool interface) servers. It enables real-time data conversion and mapping from MTConnect XML streams to OPC UA nodes, facilitating Industry 4.0 connectivity for machine tools and manufacturing equipment.

### Key Features

- **Real-time MTConnect Data Fetching**: Continuously polls MTConnect agents for current machine data
- **OPC UA Server**: Implements a full OPC UA server with umati-compliant information models
- **Flexible Mapping System**: Excel-based configuration for mapping MTConnect data items to OPC UA nodes
- **Multi-Vendor Support**: Pre-configured support for DMG and Mazak machine tools
- **Docker Support**: Containerized deployment for easy integration
- **Certificate Management**: Automatic certificate handling for secure OPC UA communication

## Architecture

### Core Components

```
mtc2umati/
├── Program.cs                 # Main application entry point
├── ConfigStore.cs            # Configuration management
├── Services/
│   ├── umatiServer.cs        # OPC UA server implementation
│   ├── umatiNodeManager.cs   # Node management and address space
│   ├── FetchXML.cs           # MTConnect XML fetching and parsing
│   ├── DataConversion.cs     # Data type conversion utilities
│   ├── CreateMapping.cs      # Excel mapping file processing
│   └── UpdateOpcUaValues.cs  # OPC UA node value updates
├── Nodesets/                 # OPC UA information models
└── config.json              # Vendor-specific configurations
```

### Data Flow

1. **Configuration Loading**: Application loads vendor-specific settings from `config.json`
2. **Mapping Initialization**: Excel mapping file is processed to create data mappings
3. **OPC UA Server Startup**: Server initializes with umati information models
4. **MTConnect Polling**: Continuous XML fetching from MTConnect agents
5. **Data Conversion**: MTConnect values are converted to appropriate OPC UA data types
6. **Node Updates**: Converted values are written to OPC UA nodes in real-time

## Prerequisites

- **.NET 9.0 SDK** or **.NET 9.0 Runtime**
- **Docker** (optional, for containerized deployment)
- **MTConnect Agent** running on target machine tools
- **Excel** (for editing mapping files)

## Installation

### Local Development

1. **Clone the repository**:

   ```bash
   git clone <repository-url>
   cd mtc2umati
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore mtc2umati.sln
   ```

3. **Build the application**:
   ```bash
   dotnet build mtc2umati.sln
   ```

### Docker Deployment

1. **Build the Docker image**:

   ```bash
   docker build -t mtc2umati:latest .
   ```

2. **Run with Docker Compose**:
   ```bash
   docker-compose up -d
   ```

## Configuration

### Environment Variables

- `VENDOR_CONFIG`: Specifies which vendor configuration to use (default: "mazak")
  - Available options: `dmg`, `mazak`, `mazak_sampleserver`

### Configuration File (`config.json`)

The application supports multiple vendor configurations:

```json
{
  "dmg": {
    "MTConnectServerIP": "http://localhost",
    "MTConnectServerPort": 8100,
    "MTConnectNamespace": "urn:mtconnect.org:MTConnectStreams:1.3",
    "Mapping_file": "./Mapping.xlsx",
    "Mapping_sheet": "DMG",
    "Information_model": "umaticonnectdmg.xml",
    "OPCNamespace": "http://ifw.uni-hannover.de/umatiConnectDMG/",
    "Machine_Name": "DMGReference",
    "Mode": 3
  },
  "mazak": {
    "MTConnectServerIP": "http://10.1.1.17",
    "MTConnectServerPort": 5000,
    "MTConnectNamespace": "urn:mtconnect.org:MTConnectStreams:1.3",
    "Mapping_file": "./Mapping.xlsx",
    "Mapping_sheet": "Mazak",
    "Information_model": "umaticonnectmazak.xml",
    "OPCNamespace": "http://ifw.uni-hannover.de/umatiConnectMazak/",
    "Machine_Name": "MazakReference",
    "Mode": 2
  }
}
```

### Configuration Parameters

| Parameter             | Description                      | Example                                       |
| --------------------- | -------------------------------- | --------------------------------------------- |
| `MTConnectServerIP`   | IP address of MTConnect agent    | `http://localhost`                            |
| `MTConnectServerPort` | Port of MTConnect agent          | `8100`                                        |
| `MTConnectNamespace`  | XML namespace for MTConnect data | `urn:mtconnect.org:MTConnectStreams:1.3`      |
| `Mapping_file`        | Path to Excel mapping file       | `./Mapping.xlsx`                              |
| `Mapping_sheet`       | Excel worksheet name             | `DMG`                                         |
| `Information_model`   | OPC UA information model file    | `umaticonnectdmg.xml`                         |
| `OPCNamespace`        | OPC UA namespace URI             | `http://ifw.uni-hannover.de/umatiConnectDMG/` |
| `Machine_Name`        | Machine identifier in OPC UA     | `DMGReference`                                |
| `Mode`                | Adapter operation mode           | `1`, `2`, or `3`                              |

### Operation Modes

- **Mode 1**: New nodes have null values (for testing)
- **Mode 2**: Default mode - new nodes have actual values in model tree
- **Mode 3**: New nodes appear in MTConnect folder under main machine folder

## Mapping Configuration

### Excel Mapping File (`Mapping.xlsx`)

The mapping file defines how MTConnect data items are mapped to OPC UA nodes:

| Column         | Description              | Example                        |
| -------------- | ------------------------ | ------------------------------ |
| Modelling Rule | Node creation rule       | `Mandatory`, `Optional`, `New` |
| OPC Path       | Path to OPC UA node      | `Machine/Controller/Status`    |
| Data Type      | OPC UA data type         | `String`, `Double`, `Boolean`  |
| MTC Name       | MTConnect data item name | `Execution`                    |
| MTC Path       | Path to MTConnect data   | `Controller/Execution`         |
| MTC Data Type  | MTConnect data type      | `EVENT`                        |
| subType        | MTConnect subtype        | `ACTIVE`, `READY`              |

### MTConnect Path Syntax

The application supports three path syntax patterns:

1. **Static Values**: Path starting with `#` sets a static value

   ```
   #ON
   ```

2. **Device Stream Attributes**: Path starting with `<Device` for device metadata

   ```
   <Device/name
   <Device/uuid
   ```

3. **Component Stream Data**: Standard three-part path
   ```
   ComponentType/ComponentName/DataItemName
   ```

## Usage

### Running the Application

1. **Set vendor configuration**:

   ```bash
   # Windows
   set VENDOR_CONFIG=dmg

   # Linux/macOS
   export VENDOR_CONFIG=dmg
   ```

2. **Run the application**:

   ```bash
   dotnet run --project mtc2umati/mtc2umati.csproj
   ```

3. **Access OPC UA server**:
   - **Endpoint**: `opc.tcp://localhost:5440`
   - **Application URI**: `urn:UmatiConnect:Server`

### Docker Usage

```bash
# Run with specific vendor configuration
docker run -e VENDOR_CONFIG=dmg -p 5440:5440 mtc2umati:latest

# Run with Docker Compose
docker-compose up -d
```

## OPC UA Information Models

The application includes the following OPC UA information models:

- **Opc.Ua.Di.NodeSet2.xml**: Device Integration
- **Opc.Ua.IA.NodeSet2.xml**: Industrial Automation
- **Opc.Ua.Machinery.NodeSet2.xml**: Machinery
- **Opc.Ua.ISA95-JobControl.NodeSet2.xml**: ISA95 Job Control
- **Opc.Ua.Machinery.Jobs.NodeSet2.xml**: Machinery Jobs
- **Opc.Ua.MachineTool.NodeSet2.xml**: Machine Tool
- **Opc.Ua.CNC.NodeSet.xml**: CNC
- **umaticonnectdmg.xml**: DMG-specific information model
- **umaticonnectmazak.xml**: Mazak-specific information model

## Data Type Conversions

The application automatically converts MTConnect data types to OPC UA data types:

| MTConnect Type | OPC UA Type | Notes                |
| -------------- | ----------- | -------------------- |
| `EVENT`        | `String`    | Status events        |
| `SAMPLE`       | `Double`    | Numeric measurements |
| `CONDITION`    | `Boolean`   | Binary states        |
| `UNAVAILABLE`  | `String`    | Unavailable data     |

### Special Conversions

- **Light States**: `OFF`/`ON`/`BLINKING` → `false`/`true`/`true`
- **Ranges**: `"low,high"` → `ExtensionObject(Range)`
- **EU Information**: `"display,description"` → `ExtensionObject(EUInformation)`

## Troubleshooting

### Common Issues

1. **MTConnect Connection Failed**
   - Verify MTConnect agent is running
   - Check IP address and port in configuration
   - Ensure network connectivity

2. **Excel Mapping File Not Found**
   - Verify `Mapping.xlsx` exists in application directory
   - Check file permissions
   - Ensure Excel file is not open in another application

3. **OPC UA Server Not Starting**
   - Check port 5440 is available
   - Verify certificate configuration
   - Review application logs

4. **Data Not Updating**
   - Verify MTConnect agent is providing data
   - Check mapping configuration
   - Review data type conversions

### Logging

The application provides console logging for:

- Configuration loading
- MTConnect XML fetching
- OPC UA node updates
- Error conditions

### Health Check

When running in Docker, the application includes a health check endpoint:

```bash
curl http://localhost:5440/health
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE) file for details.

## Support

For issues and questions:

- Check the troubleshooting section
- Review application logs
- Open an issue in the repository

## Acknowledgments

- **IFW Hannover**: Institute for Production Engineering and Machine Tools
- **OPC Foundation**: OPC UA specifications
- **MTConnect Institute**: MTConnect standard
- **umati**: Universal machine tool interface initiative

---

**Version**: 1.0.0  
**Last Updated**: 2025  
**Author**: Aleks Arzer, IFW Hannover
