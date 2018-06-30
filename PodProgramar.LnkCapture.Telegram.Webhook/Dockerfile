FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY PodProgramar.LnkCapture.Telegram.Webhook/PodProgramar.LnkCapture.Telegram.Webhook.csproj PodProgramar.LnkCapture.Telegram.Webhook/
COPY PodProgramar.LnkCapture.Data.DTO/PodProgramar.LnkCapture.Data.DTO.csproj PodProgramar.LnkCapture.Data.DTO/
COPY PodProgramar.LnkCapture.Data.BusinessObjects/PodProgramar.LnkCapture.Data.BusinessObjects.csproj PodProgramar.LnkCapture.Data.BusinessObjects/
COPY PodProgramar.Utils/PodProgramar.Utils.csproj PodProgramar.Utils/
COPY PodProgramar.LnkCapture.Data.DAL/PodProgramar.LnkCapture.Data.DAL.csproj PodProgramar.LnkCapture.Data.DAL/
COPY PodProgramar.LnkCapture.Data.Models/PodProgramar.LnkCapture.Data.Models.csproj PodProgramar.LnkCapture.Data.Models/
RUN dotnet restore PodProgramar.LnkCapture.Telegram.Webhook/PodProgramar.LnkCapture.Telegram.Webhook.csproj
COPY . .
WORKDIR /src/PodProgramar.LnkCapture.Telegram.Webhook
RUN dotnet build PodProgramar.LnkCapture.Telegram.Webhook.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PodProgramar.LnkCapture.Telegram.Webhook.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PodProgramar.LnkCapture.Telegram.Webhook.dll"]
