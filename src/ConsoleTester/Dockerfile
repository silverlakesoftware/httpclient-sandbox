#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ConsoleTester/ConsoleTester.csproj", "src/ConsoleTester/"]
RUN dotnet restore "src/ConsoleTester/ConsoleTester.csproj"
COPY . .
WORKDIR "/src/src/ConsoleTester"
RUN dotnet build "ConsoleTester.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsoleTester.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal
ENTRYPOINT ["dotnet", "ConsoleTester.dll"]