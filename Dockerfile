# ---------- Base Image ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# ---------- Build Image ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ECommercePI.WebAPI/*.csproj ./ECommercePI.WebAPI/
COPY ECommercePI.Application/*.csproj ./ECommercePI.Application/
COPY ECommercePI.Infrastructure/*.csproj ./ECommercePI.Infrastructure/
COPY ECommercePI.Domain/*.csproj ./ECommercePI.Domain/

RUN dotnet restore ./ECommercePI.WebAPI/ECommercePI.WebAPI.csproj

# Copy all source code and build
COPY . .
WORKDIR /src/ECommercePI.WebAPI
RUN dotnet publish -c Release -o /app/publish

# ---------- Final Image ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommercePI.WebAPI.dll"]
