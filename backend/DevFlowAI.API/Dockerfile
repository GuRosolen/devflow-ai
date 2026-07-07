# Etapa de compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["devflowai.api.csproj", "./"]
RUN dotnet restore "devflowai.api.csproj"
COPY . .
RUN dotnet publish "devflowai.api.csproj" -c Release -o /app/publish

# Etapa de execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "devflowai.api.dll"]