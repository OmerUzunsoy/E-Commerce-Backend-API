FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ECommerceAPI.sln ./
COPY ECommerceAPI.API/ECommerceAPI.API.csproj ECommerceAPI.API/
COPY ECommerceAPI.Application/ECommerceAPI.Application.csproj ECommerceAPI.Application/
COPY ECommerceAPI.Persistence/ECommerceAPI.Persistence.csproj ECommerceAPI.Persistence/
COPY ECommerceAPI.Domain/ECommerceAPI.Domain.csproj ECommerceAPI.Domain/

RUN dotnet restore ECommerceAPI.sln

COPY . .
RUN dotnet publish ECommerceAPI.API/ECommerceAPI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerceAPI.API.dll"]
