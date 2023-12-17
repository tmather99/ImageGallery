FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ImageGallery.Client/ImageGallery.Client.csproj", "ImageGallery.Client/"]
COPY ["ImageGallery.Authorization/ImageGallery.Authorization.csproj", "ImageGallery.Authorization/"]
COPY ["ImageGallery.Model/ImageGallery.Model.csproj", "ImageGallery.Model/"]
RUN dotnet restore "ImageGallery.Client/ImageGallery.Client.csproj"
COPY . .
WORKDIR "/src/ImageGallery.Client"
RUN dotnet build "ImageGallery.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageGallery.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ImageGallery.Client.dll"]