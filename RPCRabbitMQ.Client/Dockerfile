FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RPCRabbitMQ.Client/RPCRabbitMQ.Client.csproj", "RPCRabbitMQ.Client/"]
COPY ["RPCRabbitMQ.DataContracts/RPCRabbitMQ.DataContracts.csproj", "RPCRabbitMQ.DataContracts/"]
RUN dotnet restore "RPCRabbitMQ.Client/RPCRabbitMQ.Client.csproj"
COPY . .
WORKDIR "/src/RPCRabbitMQ.Client"
RUN dotnet build "RPCRabbitMQ.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPCRabbitMQ.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RPCRabbitMQ.Client.dll"]
