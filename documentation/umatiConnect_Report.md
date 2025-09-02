# **Integration von MTConnect in das umati Ökosystem**

## 1. Beschreibung der durchgeführten Tätigkeiten

### 1.1 AP 1: Konzepterstellung - Integration von MTConnect in das umati Ökosystem

In AP 1 wurde ein Konzept zur Einbindung von Daten aus einer
MTConnect-Schnittstelle in eine OPC UA4MT-Schnittstelle entwickelt. Hierzu
wurden zunächst die relevanten Companion Specifications (CS) OPC UA for
Machinery, OPC UA for Machine Tools (UA4MT) sowie aufbauende CS eingehend
analysiert [[VDM24]](#vdm24),[[VDM25]](#vdm25). OPC UA Server, die die genannten
CS verwenden, werden im Nachfolgenden mit dem Begriff „OPC UA4MT “ bezeichnet.
Parallel erfolgte eine detaillierte Untersuchung der Variablen sowie der
Modellierungsvorgaben des MTConnect-Standards (Spezifikationen Version 2.2.0
(2023) Part 2.0 und Part 3.0 [[MTC23a]](#mtc23a),[[MTC23b]](#mtc23b)) sowie der CS „OPC 30070-1 OPC
UA for MTConnect“ [[MTC19]](#mtc19).

Ergänzend zur theoretischen Analyse wurden OPC UA4MT- und
MTConnect-Schnittstellen experimentell untersucht. Für OPC UA4MT stehen am IFW
vier DMG Mori-Maschinen sowie der auf GitHub bereitgestellte umati Sample Server
[[CHR23]](#chr23) zur Verfügung. Für MTConnect wurden die von Mazak veröffentlichten
Beispielserver für Version 1.3 bis 2.0 analysiert [[MAZ22]](#maz22). In Abstimmung mit
Mazak wurde die weitere Arbeit auf Version 1.3 fokussiert, da nur diese Version
in den in der freien Wirtschaft verfügbaren Maschinen implementiert ist.
Zusätzlich stellte Mazak den VPN-Zugang zu einer realen MTConnect-Maschine
bereit. Ebenfalls wurden die MTConnect-Implementierungen der DMG-Maschinen am
IFW untersucht. Die verfügbaren Variablen auf beiden Schnittstellen können nach
Tabelle 1 entsprechend Ihrer Verfügbarkeit sowie ihrer Modellierungsvorgabe
(_mandatory_, _optional_) (nur OPC UA) eingeteilt werden.

Tabelle 1: Aufteilung der verfügbaren Variablen

|           | OPC UA4MT | both | MTConnect |
| :-------: | :-------: | :--: | :-------: |
| mandatory |    A1     |  B1  |     C     |
| optional  |    A2     |  B2  |     C     |

Auf Basis dieser Analysen wurde geprüft, welche Variablen mit beiden
Schnittstellen verfügbar sind bzw. fehlen und wie MTConnect-Daten in die
umati-Informationsmodelle integriert werden können. Hierfür wurde eine
Mapping-Tabelle erstellt, die für jede Variable Variablenname, Datentyp,
Datenpfad und Modellierungsvorgabe enthält. Fehlende Variablen wurden
gekennzeichnet. Für die MTConnect-Implementierungen von DMG und Mazak wurde
jeweils ein Informationsmodell mit _UaModeller_ auf Grundlage der _OPC UA for
Machine Tools_ CS erstellt. MTConnect-Variablen ohne direkte Entsprechung (C)
wurden nach fachlicher Bewertung durch VDW und IFW in die Struktur aufgenommen.
Zusätzlich wurden diese in der Mapping-Tabelle auf einer Skala von 1 bis 5
hinsichtlich ihres Potenzials für künftige CS-Versionen bewertet. Mit Abschluss
von AP 1 liegen zwei _UaModeller_-Projektdateien, die zugehörigen
_NodeSet2.xml_-Informationsmodelle sowie eine vollständige Mapping-Tabelle
zwischen _OPC UA for Machine Tools_ und MTConnect vor.

### 1.2 AP 2: Softwareentwicklung: Adapter MTConnect nach OPC UA4MT

In AP 2 wurde eine prototypische Software zur Konvertierung von Daten aus
MTConnect-Schnittstellen in OPC UA4MT-Schnittstellen entwickelt (_mtc2umati_).
Die Implementierung erfolgte als .NET 9.0-Anwendung unter Verwendung der
_UA-.NETStandard_-Bibliothek (OPCFoundation.NetStandard.Opc.Ua, Version
1.5.375.457), da diese den Import des aktuellsten _OPC UA for Machine Tools_
Informationsmodells (Stand 18.08.2025) unterstützt.

Zunächst erfolgte die Einarbeitung in die _UA-.NETStandard_-Bibliothek, um die
Grundlagen für die Entwicklung zentraler Adapterfunktionen zu schaffen. Dabei
wurden Softwarefunktionen zum Import der in AP 1 erstellten Informationsmodelle
sowie der zugehörigen Excel-Mappingtabelle (mapping.xlsx) entwickelt. Das Setzen
erforderlicher Parameter (z.B. IP-Adresse und Port des MTConnect Quellservers
sowie Pfad der Mappingtabelle) wurde über eine Konfigurationsdatei (config.json)
umgesetzt.

Bei Ausführung der Adapter-Software wird zunächst auf Basis der importierten
Informationsmodelle ein OPC UA-Server aufgebaut und anschließend eine Verbindung
zum MTConnect-Server hergestellt. Der Maschinenname ist nicht als reguläre
Variable auf der MTConnect-Schnittstelle verfügbar. Stattdessen konnte gezeigt
werden, dass er automatisiert aus dem _DeviceStream_-Namen der MTConnect-XML
extrahiert werden kann. Hierfür wurde eine Funktion entwickelt, die den
entsprechenden Maschinenordner im OPC UA-Server automatisch umbenennt. Die
Variablen werden gemäß der Mappingtabelle ausgelesen und asynchron auf den OPC
UA-Server geschrieben, wobei ausschließlich geänderte Werte übertragen werden.
Für ausgewählte Variablen wurden spezifische Konvertierungsregeln konzipiert und
implementiert.

Zur Ergänzung fehlender _mandatory_-Daten (A1) wurde eine Funktion zum Einlesen
statischer Werte entwickelt. Diese Werte können direkt in der Mappingtabelle
definiert werden und werden in gleicher Weise auf den OPC UA-Server geschrieben.
Für den Adapter stehen drei Betriebsmodi zur Verfügung:

1. Übertragung ausschließlich standardkonformer A- und B-Variablen,
2. Integration zusätzlicher C-Variablen in die OPC UA-Struktur,
3. Zusammenführung der C-Variablen in einem separaten Ordner „MTConnect“.

Der Adapter ist in der Lage, Ordner und Knoten im OPC UA-Server dynamisch
anzulegen. Aufgrund der statischen MTConnect-Implementierungen ist diese
Funktion jedoch in der Praxis nicht erforderlich. Die Praxistauglichkeit wurde
durch die Anbindung der Referenzmaschinen an den umati-Showcase des VDW über das
umatiGateway (Stand 26.06.2025) nachgewiesen. Zusätzlich liegt eine
_Docker-Compose_-Implementierung bei, die eine automatisierte Verbindung zu
einem Mazak-MTConnect-Referenzserver herstellt und sich mit einem einzigen
Befehl ausführen lässt.

### 1.3 AP 3: Softwareentwicklung: Adapter UA4MT nach MTConnect

In AP 3 wurde eine Software zur Konvertierung von Daten aus der OPC
UA4MT-Schnittstelle in das MTConnect-Format entwickelt (umati2mtc). Die
Implementierung erfolgte als Python-Anwendung (3.11) in Kombination mit dem
offiziellen C++ Agent des MTConnect Institute (Version 2.5.0.11).

Zunächst wurde die vom MTConnect Institute bereitgestellte technische
Dokumentation zur vorgesehenen Umsetzung von MTConnect-Schnittstellen analysiert
[[MTC25]](#mtc25). Diese sieht die Umsetzung einer Adapter-Agent-Architektur unter
Verwendung des offiziellen C++ Agenten vor. Der Datenaustausch erfolgt über das
SHDR-Protokoll (Simple Hierarchical Data Representation) und erfordert den
Aufbau eines SHDR-Servers im Adapter. Das Informationsmodell des Agenten
definiert die Struktur der XML-Dateien, bestehend aus _Device_- und
_Component_-Streams sowie den Datentypen _Samples_, _Events_ und _Conditions_.
Jede Variable wird durch einen Namen (_SpecName_) eindeutig gekennzeichnet.
Zusätzlich werden Parameter des Agenten (z. B. IP-Adresse des SHDR-Servers) in
der Konfigurationsdatei _agent.dock_ gesetzt.

Mit dieser Ausgangslage wurde der Adapter entwickelt und implementiert. Um eine
hohe Wartbarkeit durch die Verwendung von Standardkomponenten zu erzielen, wurde
in Abstimmung mit dem VDW das _umatiGateway_ als Datenquelle genutzt. Dieses
stellt bereits Funktionen bereit, um sich mit OPC UA4MT-Servern zu verbinden und
die erfassten Daten gebündelt im JSON-Format über MQTT an einen Broker zu
übertragen. Der entwickelte Adapter baut einen MQTT-Client auf, verbindet sich
mit einem konfigurierten Broker und liest die Nachrichten an einem definierten
„Topic“ zyklisch aus. Auf Grundlage der Mappingtabelle werden die Werte in den
JSON-Nachrichten ausgelesen, bedarfsgerecht konvertiert und im Adapter
zwischengespeichert. Parallel dazu wird ein SHDR-Server aufgebaut, der als
Schnittstelle zwischen Adapter und C++ Agent fungiert.

Es wurde untersucht, wie die Daten möglichst effizient über SHDR übertragen
werden können. Dabei wurde festgestellt, dass das Protokoll die gleichzeitige
Übertragung beliebig vieler Variablen unterstützt. Eine Funktion wurde
entwickelt, die aus allen verfügbaren Daten eine einzelne Nachricht im
SHDR-Format erstellt und diese an den SHDR-Server überträgt.

SHDR-Nachricht = {Zeitstempel} | SpecName1 | Wert1 | SpecName2 | Wert2 | …

Der C++ Agent greift zyklisch auf diesen Server zu, integriert die Daten in die
XML-Struktur und stellt sie im eigenen Dashboard live dar. Damit konnte der
korrekte Datenaustausch verifiziert werden. Zusätzlich wurde die Funktionalität
des Adapters mit der von Mazak bereitgestellten MTConnect-Dashboard-Software
_Smooth Monitor AX_ validiert. Hierfür war es erforderlich, die C-Variable
_Availability_ gesondert auf „AVAILABLE“ zu setzen. Inzwischen ist Mazak von
_Smooth Monitor Ax_ auf die Dashboard-Software _iConnect_ umgestiegen, welche
MTConnect nicht mehr unterstützt. Für das Teilprojekt _umati2mtc_ wurde
zusätzlich ein Demo-Modus über eine _Docker-Compose_-Implementierung samt
Simulationsumgebung umgesetzt.

### 1.4 Einbindung weiterer Maschinen

Mit _umatiConnect_ stehen Beispielimplementierungen für DMG- und Mazak-Maschinen
bereit. Neue Maschinen lassen sich mit geringem Anpassungsaufwand integrieren.
Im Folgenden ist beschrieben, wie die vorhandenen Adapter für zusätzliche
Maschinen genutzt und Variablen hinzugefügt oder geändert werden können. Die
Schritte beziehen sich namentlich auf die im Repository abgelegten
Softwaremodule und Dateien.

#### 1.4.1 Adaption der mtc2umati-Software (Anbindung einer MTConnect Maschine)

Um die Software optimal an die vorliegende, neue MTConnect Maschine anzupassen,
kann es erforderlich sein, die bereitgestellten _NodeSet2_-Informationsmodelle
zu verändern oder ein neues Informationsmodell entsprechend der individuellen
MTConnect-Implementierung zu erstellen. Für das Informationsmodell sollte dabei
ein neuer und eindeutiger _Address Space_ definiert werden. Zur Erstellung des
Modells kann etwa _UaModeller_ eingesetzt werden. Die neue Modelldatei ist
anschließend im Verzeichnis _mtc2umati/Nodesets_ abzulegen und über den
_umatiNodeManager_ zu importieren. Danach sollte die bestehende
_mapping.xlsx_-Datei aktualisiert oder durch eine neue Datei ersetzt werden, um
die Variablenzuordnung anzupassen. In der Konfigurationsdatei
_mtc2umati/config.json_ sind anschließend sowohl die neue Mapping-Datei als auch
die Verbindungsdaten zum MTConnect-Server einzutragen. Falls für bestimmte
Variablen besondere Umwandlungen notwendig sind, sind diese im Modul
_DataConversion_ zu implementieren.

#### 1.4.1 Adaption der umati2mtc-Software (Anbindung einer OPC UA4MT-Maschine)

Die Software ist erweiterbar und kann zur Unterstützung von spezifischen
MTConnect-Dashboards angepasst werden. Hierfür lässt sich das resultierende
MTConnect-XML-Format mit minimalem Aufwand durch eine Anpassung des
MTConnect-Informationsmodells in der Datei  
_umati2mtc/Agent/Devices.xml_ flexibel ändern. Anschließend muss die bestehende
_mapping.xlsx_-Datei entsprechend aktualisiert oder eine neue Datei erstellt
werden, um die Zuordnung der Variablen festzulegen. Die neue Mapping-Datei ist
in der Konfigurationsdatei _umati2mtc/config.json_ einzutragen. Danach wird eine
Instanz des umatiGateway eingerichtet und mit dem umati-Server der Maschine
verbunden. Die MQTT-Einstellungen des Gateways sind so zu konfigurieren, dass
die Daten an den bereitgestellten MQTT-Broker gesendet werden (localhost:1883).
Falls für bestimmte Variablen spezielle Umwandlungen erforderlich sind, sind
diese im Programmcode im Modul _data_conversion_ zu implementieren.

## 2 Zusammenfassung und Ausblick

Durch die Analyse der OPC UA- und MTConnect-Spezifikationen sowie entsprechender
Softwareumsetzungen und Bibliotheken konnten Lösungen zur Integration von
MTConnect in das umati-Ökosystem konzipiert und erfolgreich realisiert werden.
Als zentrale Ergebnisse liegen Mappingtabellen zur bidirektionalen Konvertierung
von Daten zwischen beiden Schnittstellen sowie zwei funktionsfähige
Softwareadapter für die praktische Umsetzung vor. Der dokumentierte Quellcode
wurde dem VDW bereitgestellt. Damit wurden die Kernziele des Forschungsvorhabens
erreicht. Durch eine ausführliche technische Dokumentation sowie durch die
zugängliche Gestaltung der Softwarelösungen samt Demo-Modus wurde die einfache
Weiterverwendung und Weiterentwicklung der Ergebnisse sichergestellt.

Das im Rahmen des Projekts erworbene Wissen zu _OPC UA for Machine Tools_ und
MTConnect wird am IFW auch künftig genutzt. Darüber hinaus werden die erzielten
Ergebnisse und Projekttätigkeiten aktiv in der Öffentlichkeitsarbeit des IFW
kommuniziert. Aufbauend auf der gewonnenen Expertise wird das IFW auch zukünftig
einen aktiven Beitrag zur weiteren Verbreitung und Etablierung von umati in der
Industrie leisten.

## 3 Literaturverzeichnis

|                             |                                                                                                                                      |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| [<a id="chr23">CHR23</a>]   | Christian von Arnim, et al. (2023): [umati/Sample-Server: Release 1.1.1](https://github.com/umati/Sample-Server/releases/tag/v1.1.1) |
| [<a id="maz22">MAZ22</a>]   | Mazak (2022): Demo Agents <http://mtconnect.mazakcorp.com/>                                                                          |
| [<a id="mtc19">MTC19</a>]   | MTConnect Institute (2019): OPC 30070-1: OPC UA for MTConnect® Part1: Device Model (Ver. 2.00.00)                                   |
| [<a id="mtc23a">MTC23a</a>] | MTConnect Institute (2023): MTConnect Standard. Part 3.0 - Observation Information Model - Version 2.2.0                             |
| [<a id="mtc23b">MTC23b</a>] | MTConnect Institute (2023): MTConnect Standard. Part 2.0 - Device Information Model - Version 2.2.0                                  |
| [<a id="mtc25">MTC25</a>]   | MTConnect Institute (2025): [MTConnect Standard](https://www.mtconnect.org/standard-download20181), zuletzt aufgerufen am 12.08.2025 |
| [<a id="vdm24">VDM24</a>]   | VDMA (2024): VDMA 40501-1. OPC UA for Machine Tools - Part 1: Machine Monitoring and Job Management                                  |
| [<a id="vdm25">VDM25</a>]   | VDMA (2025): VDMA 40001-1. OPC UA for Machinery - Part 1: Basic Building Blocks                                                      |

---

## Integration of MTConnect into the umati Ecosystem

## Disclaimer

This English translation is provided for informational purposes only. In case of
discrepancies or conflicts, the original German version shall prevail.

## 1. Description of Performed Activities

The following section describes the activities and results of the project
"**umatiConnect – Integration of MTConnect into the umati Ecosystem**".
Additional detailed graphics can be found in the document
"**umatiConnectOverview**".

### 1.1 WP 1: Concept Development – Integration of MTConnect into the umati Ecosystem

In WP 1, a concept was developed for integrating data from an MTConnect
interface into an OPC UA4MT interface. This involved an in-depth analysis of the
relevant Companion Specifications (CS) – OPC UA for Machinery, OPC UA for
Machine Tools (UA4MT), as well as additional related CS [[VDM24]](#vdm24),[[VDM25]](#vdm25). OPC UA
servers that implement these CS will hereafter be referred to as **OPC UA4MT**.
In parallel, the variables and modeling guidelines of the MTConnect standard
were examined in detail (Specifications Version 2.2.0 (2023) Part 2.0 and Part
3.0 [[MTC23a, MTC23b]](#mtc23a), as well as CS “OPC 30070-1 OPC UA for MTConnect” [[MTC19]](#mtc19)).

In addition to theoretical analysis, OPC UA4MT and MTConnect interfaces were
experimentally evaluated. At IFW, four DMG Mori machines as well as the umati
sample server available on GitHub [CHR23](#chr23) were used for OPC UA4MT. For
MTConnect, example servers for versions 1.3 to 2.0 published by Mazak were
analyzed [MAZ22](#maz22). In consultation with Mazak, work was focused on version 1.3,
as this is the only version implemented in machines currently available in the
industry. Mazak also provided VPN access to a real MTConnect machine. MTConnect
implementations on the DMG machines at IFW were also examined. The available
variables on both interfaces can be categorized according to their availability
and modeling requirements (_mandatory_, _optional_) (OPC UA only), as shown in
Table 1.

Table 1: Classification of available variables

|           | OPC UA4MT | both | MTConnect |
| :-------- | :-------- | :--- | :-------- |
| mandatory | A1        | B1   | C         |
| optional  | A2        | B2   | C         |

Based on these analyses, it was determined which variables are available on both
interfaces or missing, and how MTConnect data can be integrated into the umati
information models. A mapping table was created containing for each variable:
name, data type, data path, and modeling requirement. Missing variables were
marked. For the MTConnect implementations from DMG and Mazak, an information
model based on the **OPC UA for Machine Tools** CS was created using
_UaModeller_. MTConnect variables without a direct equivalent (C) were included
in the structure after technical evaluation by VDW and IFW. These were also
rated on a scale from 1 to 5 in the mapping table regarding their potential for
future CS versions. At the end of WP 1, two _UaModeller_ project files, the
associated _NodeSet2.xml_ information models, and a complete mapping table
between OPC UA for Machine Tools and MTConnect were available.

### 1.2 WP 2: Software Development – Adapter MTConnect to OPC UA4MT

In WP 2, a prototype software for converting data from MTConnect interfaces to
OPC UA4MT interfaces was developed (**mtc2umati**). The implementation was done
as a .NET 9.0 application using the _UA-.NETStandard_ library
(OPCFoundation.NetStandard.Opc.Ua, version 1.5.375.457), which supports
importing the latest **OPC UA for Machine Tools** information model (as of
18.08.2025).

Functions were implemented for importing the information models created in WP 1
as well as the associated Excel mapping table (_mapping.xlsx_). Parameters such
as the MTConnect server IP address and mapping file path were configured via
_config.json_. Upon execution, the adapter sets up an OPC UA server based on the
imported models and connects to the MTConnect server. Machine names are
extracted from the _DeviceStream_ name in MTConnect XML and automatically
assigned to the OPC UA server folder. Variables are read according to the
mapping table and written asynchronously to the OPC UA server, transmitting only
changed values. Specific conversion rules were implemented for selected
variables.

To supplement missing _mandatory_ data (A1), a function was implemented for
reading static values defined in the mapping table. The adapter supports three
operation modes:

1. Transfer of standard-compliant A and B variables only.
2. Integration of additional C variables into the OPC UA structure.
3. Grouping C variables in a separate folder “MTConnect”.

The adapter supports dynamic creation of nodes, although this is rarely required
in practice due to static MTConnect implementations. Its practical suitability
was validated by connecting reference machines to the VDW umati showcase via the
umatiGateway (as of 26.06.2025). A _Docker-Compose_ implementation was added for
automated setup and execution against a Mazak MTConnect reference server.

### 1.3 WP 3: Software Development – Adapter UA4MT to MTConnect

In WP 3, software for converting data from an OPC UA4MT interface into the
MTConnect format was developed (**umati2mtc**). It was implemented as a Python
(3.11) application combined with the official MTConnect C++ Agent (version
2.5.0.11).

The adapter follows the MTConnect adapter-agent architecture using SHDR (Simple
Hierarchical Data Representation) protocol, requiring an SHDR server within the
adapter. Data from the umatiGateway (already providing OPC UA4MT data as JSON
via MQTT) was consumed, mapped using the mapping table, converted, and
transmitted to the SHDR server. The C++ agent then integrated these values into
XML structure and displayed them in its dashboard.

Efficiency tests confirmed that SHDR supports sending multiple variables in a
single message. The solution was validated with Mazak’s MTConnect dashboard
software **Smooth Monitor AX** and later adapted for environments where iConnect
replaced MTConnect dashboards. A Docker-based demo mode with simulation was also
provided.

### 1.4 Adding Additional Machines

With umatiConnect, example implementations for DMG and Mazak machines are
available. New machines can be integrated with minimal adjustment effort. The
following section describes how the existing adapters can be used for additional
machines and how variables can be added or modified. The steps specifically
refer to the software modules and files included in the repository.

#### 1.4.1 Adapting the mtc2umati Software (Connecting an MTConnect Machine)

To optimally adapt the software to the given new MTConnect machine, it may be
necessary to modify the provided NodeSet2 information models or create a new
information model according to the individual MTConnect implementation. A new
and unique Address Space should be defined for the information model. Tools such
as _UaModeller_ can be used to create the model. The new model file should then
be stored in the directory `mtc2umati/Nodesets` and imported using the
`umatiNodeManager`. Afterwards, the existing `mapping.xlsx` file should be
updated or replaced with a new file to adjust the variable mapping. In the
configuration file `mtc2umati/config.json`, both the new mapping file and the
connection details for the MTConnect server must be entered. If specific
variable conversions are required, they must be implemented in the
`DataConversion` module.

#### 1.4.2 Adapting the umati2mtc Software (Connecting an OPC UA4MT Machine)

The software is extendable and can be adapted to support specific MTConnect
dashboards. The resulting MTConnect XML format can be modified with minimal
effort by adjusting the MTConnect information model in the file
`umati2mtc/Agent/Devices.xml`. Afterwards, the existing `mapping.xlsx` file must
be updated or replaced with a new file to define the variable mapping. The new
mapping file must then be specified in the configuration file
`umati2mtc/config.json`. Next, an instance of the `umatiGateway` should be set
up and connected to the machine’s umati server. The gateway’s MQTT settings must
be configured so that the data is sent to the provided MQTT broker
(`localhost:1883`). If specific variable conversions are required, they must be
implemented in the program code in the `data_conversion` module.

## 2 Summary and Outlook

The project successfully developed concepts and software solutions for
bidirectional data exchange between MTConnect and OPC UA4MT. Results include
complete mapping tables, two fully functional adapters, and extensive technical
documentation to enable reuse and extension. The knowledge gained will be
leveraged at IFW and actively communicated to the public, supporting the broader
adoption of umati in the industry.

## 3 References

see above.
