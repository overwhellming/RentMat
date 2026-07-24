FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-composite AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY RentMat.sln .
COPY RentMat.API/RentMat.API.csproj RentMat.API/
COPY RentMat.Core/RentMat.Core.csproj RentMat.Core/
COPY RentMat.Infrastructure/RentMat.Infrastructure.csproj RentMat.Infrastructure/
COPY RentMat.Application/RentMat.Application.csproj RentMat.Application/
COPY RentMat.Application.UnitTests/RentMat.Application.UnitTests.csproj RentMat.Application.UnitTests/
COPY RentMat.Application.IntegrationTests/RentMat.Application.IntegrationTests.csproj RentMat.Application.IntegrationTests/
RUN dotnet restore RentMat.sln
COPY . .
RUN dotnet build RentMat.API/RentMat.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish RentMat.API/RentMat.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentMat.API.dll"]