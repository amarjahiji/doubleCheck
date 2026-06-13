# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + build props + project files first (better layer caching)
COPY DoubleCheck.sln Directory.Build.props ./
COPY src/ src/

RUN dotnet restore src/DoubleCheck/DoubleCheck.csproj
RUN dotnet publish src/DoubleCheck/DoubleCheck.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DoubleCheck.dll"]
