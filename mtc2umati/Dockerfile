# Use official .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Optional: expose port if it's a web app
EXPOSE 5440

# Run the application
ENTRYPOINT ["dotnet", "mtc2umati.dll"]
