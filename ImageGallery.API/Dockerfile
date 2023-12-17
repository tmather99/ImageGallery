#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ImageGallery.API/ImageGallery.API.csproj", "ImageGallery.API/"]
COPY ["ImageGallery.Authorization/ImageGallery.Authorization.csproj", "ImageGallery.Authorization/"]
COPY ["ImageGallery.Model/ImageGallery.Model.csproj", "ImageGallery.Model/"]
RUN dotnet restore "ImageGallery.API/ImageGallery.API.csproj"
COPY . .
WORKDIR "/src/ImageGallery.API"
RUN dotnet build "ImageGallery.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageGallery.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ImageGallery.API.dll"]