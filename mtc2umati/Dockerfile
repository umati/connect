# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

# Use official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:548d93f8a18a1acbe6cc127bc4f47281430d34a9e35c18afa80a8d6741c2adc3 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:ddcf70ad1ab963a4fcd41fbd722a6b660e404e87567cfbd46fd2809c21b02088 AS final
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
