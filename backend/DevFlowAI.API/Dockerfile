# Etapa de compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DevFlowAI.API.csproj", "./"]
RUN dotnet restore "DevFlowAI.API.csproj"
COPY . .
RUN dotnet publish "DevFlowAI.API.csproj" -c Release -o /app/publish

# Etapa de execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DevFlowAI.API.dll"]