FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/ClaimPhaseService.Api/ClaimPhaseService.Api.csproj", "src/ClaimPhaseService.Api/"]

RUN dotnet restore "src/ClaimPhaseService.Api/ClaimPhaseService.Api.csproj"

COPY . .

WORKDIR "/src/src/ClaimPhaseService.Api"

RUN dotnet publish "ClaimPhaseService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ClaimPhaseService.Api.dll"]
