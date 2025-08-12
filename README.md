# umatiConnect

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Python](https://img.shields.io/badge/Python-3.11-blue.svg)](https://www.python.org/downloads/)

Bidirectional data bridge between umati OPC UA for Machine Tools and MTConnect.

## Architecture

This project provides two software adapters:

- **`mtc2umati`** (.NET 9.0): Sets up an umati OPC UA Server, reads MTConnect XML data streams and writes to the corresponding OPC UA nodes
- **`umati2mtc`** (Python 3.11): Translates umati OPC UA data provided by the umati Gateway to MTConnect SHDR format and sends it to an MTConnect Agent.

Both components support Excel-based mapping configurations and containerized deployment.

## Quick Start

### MTConnect → umati (C#/.NET)
```bash
cd mtc2umati
docker compose up --build -d
```
- Connects to public Mazak MTConnect Server at http://mtconnect.mazakcorp.com:5610/current (might be offline).
- New server can be added or configured in the config.json file.
- The umati OPC UA Server will be available at `opc.tcp://localhost:5440`.

### umati OPC UA→ MTConnect (Python)
```bash
cd umati2mtc
docker compose up --build -d
```
- Simulates incoming data from an umati Gateway, sent to an MQTT broker at `mqtt://localhost:1883`
- Builds an SHDR Server at `http://localhost:7878`.
- Parses the data and writes it to an MTConnect Agent using SHDR format.
- The MTConnect Agent dashboard will be available at `http://localhost:5000`.

## Configuration

Mapping configurations are stored in Excel files (`mapping.xlsx`) allowing data transformations between protocols without code changes.

## Standards Compliance

- **umati**: OPC UA Companion Specification for Machine Tools (OPC 40501-1)
- **MTConnect**: MTConnect Standard Part 2 & 3 (v2.2.0)
- **OPC UA**: Core specifications with security profiles

## License

This implementation is licensed under the [Apache License v2.0](LICENSE).
