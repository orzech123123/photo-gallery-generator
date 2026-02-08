FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend/PhotoApp.csproj", "Backend/"]
RUN dotnet restore "Backend/PhotoApp.csproj"
COPY Backend/ Backend/
WORKDIR "/src/Backend"
RUN dotnet build "PhotoApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoApp.dll"]