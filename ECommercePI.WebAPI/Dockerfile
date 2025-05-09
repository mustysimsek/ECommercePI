FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ECommercePI.WebAPI/ECommercePI.WebAPI.csproj", "ECommercePI.WebAPI/"]
COPY ["ECommercePI.Application/ECommercePI.Application.csproj", "ECommercePI.Application/"]
COPY ["ECommercePI.Domain/ECommercePI.Domain.csproj", "ECommercePI.Domain/"]
COPY ["ECommercePI.Infrastructure/ECommercePI.Infrastructure.csproj", "ECommercePI.Infrastructure/"]
RUN dotnet restore "ECommercePI.WebAPI/ECommercePI.WebAPI.csproj"
COPY . .
WORKDIR "/src/ECommercePI.WebAPI"
RUN dotnet build "ECommercePI.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ECommercePI.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommercePI.WebAPI.dll"]