# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY SOSComida.csproj ./
RUN dotnet restore SOSComida.csproj

COPY . ./
RUN dotnet publish SOSComida.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# OCR: o pacote Tesseract do NuGet só traz binários nativos para Windows, então
# nesta imagem Linux a leitura automática depende do provedor em nuvem —
# defina Ocr__ChaveApi (Mistral). Sem a chave o envio de nota continua
# funcionando, só que o responsável preenche os campos à mão.

COPY --from=build /app/publish .

# Banco e uploads ficam fora da imagem: sem volume, cada novo container
# começaria com o banco vazio e sem as fotos e notas enviadas.
RUN mkdir -p /app/data /app/wwwroot/uploads
VOLUME ["/app/data", "/app/wwwroot/uploads"]

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/soscomida.db"

# Obrigatórias em produção (o app não sobe sem a chave do JWT):
#   Jwt__Chave              segredo com 32+ caracteres
#   Recaptcha__SecretKey    secret do reCAPTCHA (sem ela o login é recusado)
#   Cors__Origens__0        URL do front, ex.: https://soscomida.com.br
# Opcional:
#   Ocr__ChaveApi           chave da API da Mistral para ler as notas fiscais

ENTRYPOINT ["dotnet", "SOSComida.dll"]
