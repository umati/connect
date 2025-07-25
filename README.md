# umatiConnect

Compatibility layer between umati OPC UA for Machine Tools and MTConnect

## MTC2UMATI - MTConnect to umati OPC UA adapter

- Connects to a MTConnect machine specified in the config.json
- Creates an OPC UA server based on the UA4MT nodeset
- Renames machine name in the OPC UA server to match the MTConnect machine name
- Continously updates values based on the mapping in Mapping.xlsx
- URL, Ports, and other settings are specified in the config.json file

### Base settings (MAZAK sample server)

- OPC UA Server URL: `opc.tcp://localhost:5440`
- MTConnect URL: `http://mtconnect.mazakcorp.com:5719/current`

#### Run MTC2UMATI

- `cd mtc2umati/mtc2umati`
- `dotnet run`

Developed with NET9.0 <https://dotnet.microsoft.com/en-us/download/dotnet/9.0>

## UMATI2MTC - umati OPC UA to MTConnect adapter

- Connects to an MQTT broker and subscribes to umatiGateway topic
- Creates a flask app that serves the XML in MTConnect format
- Processes data published by the umatiGateway
- Continuously updates XML values based on the mapping in Mapping.xlsx
- URL, Ports, and other settings are specified in the config.json

### Base settings

- MQTT Broker URL: `mqtt://localhost:1883`
- MTConnect URL (Flask app): `http://localhost:5500/current`

#### Run UMATI2MTC

- `cd umati2mtc`
- `python -m venv .venv`
- `.\.venv\Scripts\activate`
- `pip install -r requirements.txt`
- `python main.py`

## License

![GitHub](https://img.shields.io/github/license/umati/connect)

This implementation is licensed under the [Apache License v2.0](LICENSE) except otherwise stated in the header of a file.
