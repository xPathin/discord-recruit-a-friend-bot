FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RafBot/RafBot.csproj", "RafBot/"]
RUN dotnet restore "RafBot/RafBot.csproj"
COPY . .
WORKDIR "/src/RafBot"
RUN dotnet build "RafBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RafBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RafBot.dll"]
