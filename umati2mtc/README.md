# UMATI2MTC - OPC UA umati to MTConnect Bridge

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Python](https://img.shields.io/badge/Python-3.11-blue.svg)](https://www.python.org/downloads/)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](https://docs.docker.com/compose/)

## Overview

UMATI2MTC is a Python-based application that translates data from the umati Gateway software to MTConnect.
The provided docker compose file sets up a complete environment with an MQTT broker, a simulator for umati data, the adapter for processing and converting the data, and an MTConnect agent to serve the converted data.

### Key Features

- **Real-time OPC UA umati Data Processing**: Receives OPC UA umati data from an MQTT-Broker
- **MTConnect SHDR Server**: Implements SHDR server for MTConnect agent communication
- **Variable Mapping**: Excel-based configuration for mapping umati OPC UA data and MTConnect data
- **Simulation Capabilities**: Built-in simulator for testing and development
- **Docker Compose Integration**: Complete containerized deployment

## Architecture

### Docker Compose Services

The application consists of 4 interconnected services:

```
umati2mtc/
├── docker-compose.yml          # Main orchestration file
├── mosquitto/                  # MQTT message broker
│   ├── config/mosquitto.conf   # Broker configuration
│   ├── data/                   # Persistent message storage
│   └── log/                    # Broker logs
├── Simulator/                  # OPC UA umati data simulator
│   ├── Dockerfile              # Simulator container
│   ├── publish_json_via_mqtt.py # MQTT publisher
│   └── mqtt_message.json       # Sample umati data
├── Adapter/                    # Main bridge application
│   ├── Dockerfile              # Adapter container
│   ├── main.py                 # Main application
│   ├── services/               # Core services
│   └── config.json            # Configuration
└── Agent/                      # MTConnect agent
    ├── agent.dock              # Agent configuration
    └── Devices.xml             # Device definitions
```

### Data Flow

1. **Simulator**: Publishes simulated OPC UA umati data to MQTT broker
2. **MQTT Broker**: Distributes messages to subscribed clients
3. **Adapter**: Receives MQTT messages, processes data, and converts to SHDR format
4. **MTConnect Agent**: Receives SHDR data and provides MTConnect interface

## Prerequisites

- **Docker** and **Docker Compose**
- **Git** (for cloning the repository)
- **Network access** to MQTT broker (port 1883)
- **MTConnect client** (for testing)

## Quick Start

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd umati2mtc
```

### 2. Start All Services

```bash
docker-compose up -d
```

### 3. Verify Services

```bash
docker-compose ps
```

### 4. Access MTConnect Data

- **MTConnect Agent**: http://localhost:5000
- **MQTT Broker**: localhost:1883
- **SHDR Server**: localhost:7878

## Service Details

### 1. Mosquitto MQTT Broker

**Purpose**: Central message broker for OPC UA umati data distribution

**Configuration**:

```yaml
mosquitto:
  image: eclipse-mosquitto:latest
  container_name: mosquitto
  ports:
    - "1883:1883"
  volumes:
    - ./mosquitto/config:/mosquitto/config
    - ./mosquitto/data:/mosquitto/data
    - ./mosquitto/log:/mosquitto/log
```

### 2. Simulator

**Purpose**: Generates simulated OPC UA umati data for testing

**Configuration**:

```yaml
simulator:
  container_name: simulator
  image: simulator:latest
  build:
    context: ./Simulator
    dockerfile: Dockerfile
  depends_on:
    - mosquitto
  environment:
    - MQTT_BROKER_IP=mosquitto
    - MQTT_BROKER_PORT=1883
```

**Features**:

- Publishes JSON messages every second
- Simulates machine tool data (FeedOverride, PowerOnDuration)
- Random value generation

### 3. MTConnect Adapter

**Purpose**: Core bridge application that converts umati data to MTConnect SHDR

**Configuration**:

```yaml
mtc-adapter:
  container_name: mtc-adapter
  image: mtc-adapter:latest
  depends_on:
    - mosquitto
    - mtc-agent
  build:
    context: ./Adapter
    dockerfile: Dockerfile
  ports:
    - "7878:7878"
  environment:
    - MQTT_BROKER_IP=mosquitto
    - MQTT_BROKER_PORT=1883
    - MQTT_TOPIC_PREFIX=umati/v2/ifw/MachineToolType/#
    - SHDR_SERVER_IP=0.0.0.0
    - SHDR_SERVER_PORT=7878
```

**Core Components**:

- **MQTT Client**: Subscribes to umati topics
- **Message Queue**: Buffers incoming messages
- **Data Processor**: Converts umati data to MTConnect format
- **SHDR Server**: Sends data to MTConnect agent

**Services**:

- `mqtt_client.py`: MQTT message reception
- `process_queue.py`: Message processing and mapping
- `data_conversion.py`: Data type conversions
- `create_mappings.py`: Excel mapping file processing
- `send_shdr.py`: SHDR server implementation

### 4. MTConnect Agent

**Purpose**: Provides MTConnect interface for machine data

**Configuration**:

```yaml
mtc-agent:
  image: "mtconnect/demo:latest"
  container_name: mtc-agent
  volumes:
    - ./Agent:/mtconnect/config
    - ./Agent/log:/mtconnect/log
  ports:
    - "5000:5000"
  command: mtcagent run /mtconnect/config/agent.dock
```

**Features**:

- Standard MTConnect agent implementation
- SHDR adapter integration
- Web interface for data access
- Configurable device definitions

**Agent Configuration** (`Agent/agent.dock`):

```
Devices = Devices.xml
SchemaVersion = 2.0
WorkerThreads = 3
Port = 5000
JsonVersion = 2

Adapters {
  Mazak {
    Port = 7878
    Host = mtc-adapter
  }
}
```

## Configuration

### Environment Variables

| Service   | Variable            | Default                          | Description              |
| --------- | ------------------- | -------------------------------- | ------------------------ |
| Simulator | `MQTT_BROKER_IP`    | `mosquitto`                      | MQTT broker hostname     |
| Simulator | `MQTT_BROKER_PORT`  | `1883`                           | MQTT broker port         |
| Adapter   | `MQTT_BROKER_IP`    | `mosquitto`                      | MQTT broker hostname     |
| Adapter   | `MQTT_BROKER_PORT`  | `1883`                           | MQTT broker port         |
| Adapter   | `MQTT_TOPIC_PREFIX` | `umati/v2/ifw/MachineToolType/#` | MQTT topic filter        |
| Adapter   | `SHDR_SERVER_IP`    | `0.0.0.0`                        | SHDR server bind address |
| Adapter   | `SHDR_SERVER_PORT`  | `7878`                           | SHDR server port         |

### Mapping Configuration

The adapter uses Excel mapping files to convert umati data to MTConnect format:

**Configuration** (`Adapter/config.json`):

```json
{
  "mazak": {
    "Mapping_file": "Mapping.xlsx",
    "Mapping_sheet": "Mazak_MTC"
  }
}
```

**Mapping File Structure** (`Adapter/Mapping.xlsx`):
tbd

## Troubleshooting

### Common Issues

1. **MQTT Connection Failed**

   ```bash
   # Check mosquitto logs
   docker-compose logs mosquitto

   # Verify network connectivity
   docker-compose exec mtc-adapter ping mosquitto
   ```

2. **SHDR Connection Failed**

   ```bash
   # Check adapter logs
   docker-compose logs mtc-adapter

   # Verify port availability
   netstat -tulpn | grep 7878
   ```

3. **MTConnect Agent Not Responding**

   ```bash
   # Check agent logs
   docker-compose logs mtc-agent

   # Verify agent configuration
   docker-compose exec mtc-agent cat /mtconnect/config/agent.dock
   ```

4. **Simulator Not Publishing**

   ```bash
   # Check simulator logs
   docker-compose logs simulator

   # Verify MQTT connectivity
   docker-compose exec simulator python -c "import paho.mqtt.client; print('MQTT client available')"
   ```

### Health Checks

```bash
# Check service status
docker-compose ps

# Check service health
docker-compose exec mtc-adapter curl -f http://localhost:7878/health || echo "Adapter unhealthy"

# Check network connectivity
docker-compose exec mtc-adapter ping mosquitto
docker-compose exec mtc-adapter ping mtc-agent
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE) file for details.

## Acknowledgments

- **IFW Hannover**: Institute for Production Engineering and Machine Tools
- **OPC Foundation**: OPC UA specifications
- **MTConnect Institute**: MTConnect standard
- **umati**: Universal machine tool interface initiative
- **Eclipse Mosquitto**: MQTT broker implementation

---

**Version**: 1.0.0  
**Last Updated**: 2025  
**Author**: Aleks Arzer, IFW Hannover
