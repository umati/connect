#pragma once

#include <string>

// the configured options and settings for MTConnect Agent
constexpr const int AGENT_VERSION_MAJOR = 2;
constexpr const int AGENT_VERSION_MINOR = 5;
constexpr const int AGENT_VERSION_PATCH = 0;
constexpr const int AGENT_VERSION_BUILD = 9;
constexpr const char* AGENT_VERSION_RC = "";

extern void PrintMTConnectAgentVersion();
extern std::string GetAgentVersion();
