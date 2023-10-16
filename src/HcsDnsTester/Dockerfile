#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
RUN apt-get update \
  && apt-get install -y \
  dnsutils \
  iputils-ping

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/HcsDnsTester/HcsDnsTester.csproj", "src/HcsDnsTester/"]
RUN dotnet restore "src/HcsDnsTester/HcsDnsTester.csproj"
COPY . .
WORKDIR "/src/src/HcsDnsTester"
RUN dotnet build "HcsDnsTester.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "HcsDnsTester.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HcsDnsTester.dll"]