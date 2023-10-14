#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY ./https/fullchain2.pem /usr/share/ca-certificates/assistdevtest.crt
RUN echo assistdevtest.crt >> /etc/ca-certificates.conf
RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Marvin.IDP/Marvin.IDP.csproj", "Marvin.IDP/"]
RUN dotnet restore "Marvin.IDP/Marvin.IDP.csproj"
COPY . .
WORKDIR "/src/Marvin.IDP"
RUN dotnet build "Marvin.IDP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Marvin.IDP.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Marvin.IDP.dll"]