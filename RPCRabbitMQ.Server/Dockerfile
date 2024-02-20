FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RPCRabbitMQ.Server/RPCRabbitMQ.Server.csproj", "RPCRabbitMQ.Server/"]
COPY ["RPCRabbitMQ.DataContracts/RPCRabbitMQ.DataContracts.csproj", "RPCRabbitMQ.DataContracts/"]
RUN dotnet restore "RPCRabbitMQ.Server/RPCRabbitMQ.Server.csproj"
COPY . .
WORKDIR "/src/RPCRabbitMQ.Server"
RUN dotnet build "RPCRabbitMQ.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPCRabbitMQ.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RPCRabbitMQ.Server.dll"]
