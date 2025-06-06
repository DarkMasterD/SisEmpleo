# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copia csproj y restaura dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia todo y construye
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Puerto que usará la app
EXPOSE 80

ENTRYPOINT ["dotnet", "SisEmpleo.dll"]
