FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IdendityService/IdendityService.csproj", "IdendityService/"]
RUN dotnet restore "IdendityService/IdendityService.csproj"
COPY . .
RUN dotnet publish "IdendityService/IdendityService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdendityService.dll"]
