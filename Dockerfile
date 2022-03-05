#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /
COPY . .
RUN dotnet restore 
COPY . .
WORKDIR "/Zendesk.JsonSearch"
RUN dotnet build "Zendesk.JsonSearch.csproj" -c Release -o /app/build

WORKDIR "/Zendesk.JsonSearch.Models.Test"
RUN dotnet build "Zendesk.JsonSearch.Models.Test.csproj" -c Release -o /app/build

WORKDIR "/Zendesk.JsonSearch.Logic.Test"
RUN dotnet build "Zendesk.JsonSearch.Logic.Test.csproj" -c Release -o /app/build

WORKDIR "/Zendesk.JsonSearch"
FROM build AS publish
RUN dotnet publish "Zendesk.JsonSearch.csproj" -c Release -o /app/publish

WORKDIR "/"
FROM build AS testrunner
COPY . .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zendesk.JsonSearch.dll"]