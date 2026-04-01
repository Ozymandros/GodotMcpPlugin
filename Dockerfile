FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Godot-MCP-SK-Plugin.slnx
RUN dotnet publish samples/GodotMcp.Sample/GodotMcp.Sample.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GodotMcp.Sample.dll"]
