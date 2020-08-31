##See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
#WORKDIR /src
#COPY ["PubSubExceptionRepro/PubSubExceptionRepro.csproj", "PubSubExceptionRepro/"]
#RUN dotnet restore "PubSubExceptionRepro/PubSubExceptionRepro.csproj"
#COPY . .
#WORKDIR "/src/PubSubExceptionRepro"
#RUN dotnet build "PubSubExceptionRepro.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "PubSubExceptionRepro.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "PubSubExceptionRepro.dll"]







FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish PubSubExceptionRepro -c Release -o PubSubExceptionRepro/out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/PubSubExceptionRepro/out .

RUN ls -R #added for checking files related to exception encountered when running in GKE

ENTRYPOINT ["dotnet", "PubSubExceptionRepro.dll"]