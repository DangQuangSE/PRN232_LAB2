FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PRN232.LMSSystem.API/PRN232.LMSSystem.API.csproj", "PRN232.LMSSystem.API/"]
COPY ["PRN232.LMSSystem.Services/PRN232.LMSSystem.Services.csproj", "PRN232.LMSSystem.Services/"]
COPY ["PRN232.LMSSystem.Repositories/PRN232.LMSSystem.Repositories.csproj", "PRN232.LMSSystem.Repositories/"]
RUN dotnet restore "PRN232.LMSSystem.API/PRN232.LMSSystem.API.csproj"
COPY . .
WORKDIR "/src/PRN232.LMSSystem.API"
RUN dotnet build "PRN232.LMSSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PRN232.LMSSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PRN232.LMSSystem.API.dll"]
