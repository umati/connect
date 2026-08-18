# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

# Use official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c AS build
WORKDIR /src

# Copy solution and project files
COPY mtc2umati.sln ./
COPY mtc2umati/ ./mtc2umati/

# Restore dependencies
RUN dotnet restore mtc2umati.sln

# Copy all source files
COPY mtc2umati/ ./mtc2umati/

# Build and publish
WORKDIR /src/mtc2umati
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:a4556ed033fa96f984bb7a8d348851cb2d36b1281dd2420070045f664fbb5f94 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Port of the umati server
EXPOSE 5440

# Add a user
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

# Basic healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD tail -n 20 /proc/1/fd/1 | grep -q .

# Run the application
ENTRYPOINT ["dotnet", "mtc2umati.dll"]
