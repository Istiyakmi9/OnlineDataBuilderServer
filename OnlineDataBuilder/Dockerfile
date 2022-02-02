#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["OnlineDataBuilder/OnlineDataBuilder.csproj", "OnlineDataBuilder/"]
COPY ["MultiTypeDocumentConverter/MultiTypeDocumentConverter.csproj", "MultiTypeDocumentConverter/"]
COPY ["EMailService/EMailService.csproj", "EMailService/"]
COPY ["DocMaker/DocMaker.csproj", "DocMaker/"]
COPY ["ModalLayer/ModalLayer.csproj", "ModalLayer/"]
COPY ["BottomhalfCore/BottomhalfCore.csproj", "BottomhalfCore/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "ServiceLayer/"]
COPY ["SocialMediaServices/SocialMediaServices.csproj", "SocialMediaServices/"]
RUN dotnet restore "OnlineDataBuilder/OnlineDataBuilder.csproj"
COPY . .
WORKDIR "/src/OnlineDataBuilder"
RUN dotnet build "OnlineDataBuilder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineDataBuilder.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineDataBuilder.dll"]