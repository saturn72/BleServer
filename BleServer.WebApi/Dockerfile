FROM microsoft/dotnet-framework:4.7.1 AS base
WORKDIR /app
EXPOSE 80

FROM  AS build
WORKDIR /src
COPY NetworkServers.sln ./
COPY BleServer/BleServer.WebApi/BleServer.WebApi.csproj BleServer/BleServer.WebApi/
COPY BleServer/BleServer.Common/BleServer.Common.csproj BleServer/BleServer.Common/
COPY BleServer/Modules/BleServer.Modules.Win10BleAdapter/BleServer.Modules.Win10BleAdapter.csproj BleServer/Modules/BleServer.Modules.Win10BleAdapter/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/BleServer/BleServer.WebApi
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BleServer.WebApi.dll"]
