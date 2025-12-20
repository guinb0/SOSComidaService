# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos do projeto
COPY *.csproj ./
RUN dotnet restore

# Copiar código fonte e buildar
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Expor porta
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Iniciar aplicação
ENTRYPOINT ["dotnet", "SOSComida.dll"]
