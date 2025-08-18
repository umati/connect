# umatiConnect

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Python](https://img.shields.io/badge/Python-3.11-blue.svg)](https://www.python.org/downloads/)

Bidirectional data bridge between umati OPC UA for Machine Tools (UA4MT) and MTConnect.

## Architecture

This project provides two software adapters:

- **`mtc2umati`** (.NET 9.0): Sets up an UA4MT OPC UA Server, reads MTConnect
  XML data streams and writes to the corresponding OPC UA nodes
- **`umati2mtc`** (Python 3.11): Translates UA4MT OPC UA data provided by the
  umatiGateway to MTConnect SHDR format and sends it to an MTConnect Agent.

Both components support Excel-based mapping configurations and containerized deployment.

## Quick Start

### MTConnect → OPC UA4MT (C#/.NET)

```bash
cd mtc2umati
docker compose up --build -d
```

- Connects to public Mazak MTConnect Server at <http://mtconnect.mazakcorp.com:5610/current>
  (check if online).
- New server can be added or configured in the config.json file.
- The umati OPC UA Server will be available at `opc.tcp://localhost:5440`.

### OPC UA4MT → MTConnect (Python)

```bash
cd umati2mtc
docker compose up --build -d
```

- Simulates incoming data from an umatiGateway, sent to an MQTT broker at `mqtt://localhost:1883`.
- Builds an SHDR Server at `http://localhost:7878`.
- Parses the data and writes it to an MTConnect Agent using SHDR format.
- Uses Mazak's Device.xml information model for MTConnect as
  found in the public servers at <http://mtconnect.mazakcorp.com>.
- The MTConnect Agent dashboard will be available at `http://localhost:5000`.

## Configuration

Mapping configurations are stored in the Excel file (`mapping/mapping.xlsx`)
allowing data transformations between protocols without code changes.

## Standards Compliance

- **UA4MT**: OPC UA Companion Specification for Machine Tools (OPC 40501-1 v1.02)
- **MTConnect**: MTConnect Standard Part 2 & 3 (v2.2.0)
- **OPC UA**: Core specifications with security profiles

For details see [Specs](mapping/Specs/Specs.md)

## Limitations

Currently this project is limited to **OPC UA for Machine Tools** and the corresponding variables in **MTConnect**.

## License

This implementation is licensed under the [Apache License v2.0](LICENSE).

## Trademarks

umati is a registered trademark of VDW - German Machine Tool builders' association.

MTConnect® is a registered trademark of AMT - The Association for Manufacturing Technology.
