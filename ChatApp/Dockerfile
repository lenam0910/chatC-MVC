# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["ChatApp.csproj", "./"]
RUN dotnet restore "ChatApp.csproj"

# Copy toàn bộ source code
COPY . .

# Build ứng dụng
RUN dotnet build "ChatApp.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ChatApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel listen trên 8080 trong container
ENV ASPNETCORE_URLS=http://+:8080

# Chỉ expose cổng thực sự dùng trong container
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Entry point
ENTRYPOINT ["dotnet", "ChatApp.dll"]
