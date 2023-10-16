#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update \
  && apt-get install -y \
  dnsutils \
  iputils-ping

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MinimalService/MinimalService.csproj", "src/MinimalService/"]
RUN dotnet restore "src/MinimalService/MinimalService.csproj"
COPY . .
WORKDIR "/src/src/MinimalService/"
RUN dotnet build "MinimalService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "MinimalService.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinimalService.dll"]