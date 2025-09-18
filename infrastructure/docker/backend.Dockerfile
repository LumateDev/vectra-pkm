FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["backend/src/Vectra.API/Vectra.API.csproj", "backend/src/Vectra.API/"]
COPY ["backend/src/Vectra.Modules.Identity.Application/Vectra.Modules.Identity.Application.csproj", "backend/src/Vectra.Modules.Identity.Application/"]
COPY ["backend/src/Vectra.Modules.Identity.Domain/Vectra.Modules.Identity.Domain.csproj", "backend/src/Vectra.Modules.Identity.Domain/"]
# TODO add copy to other projects 
RUN dotnet restore "backend/src/Vectra.API/Vectra.API.csproj"
COPY . .
WORKDIR "/src/backend/src/Vectra.API"
RUN dotnet build "Vectra.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vectra.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vectra.API.dll"]