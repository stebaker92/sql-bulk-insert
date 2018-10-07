FROM microsoft/dotnet:2.0-sdk AS build-env

WORKDIR /app

# Copy the project file and restore the dependencies
COPY *.csproj ./
RUN ls
RUN dotnet restore
 
# Copy the remaining source files and build the application
COPY . ./
RUN ls
RUN dotnet publish -c Release -o out

RUN ls

FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY --from=build-env /app/out .

RUN ls

ENTRYPOINT ["dotnet", "./FtpBulkDownloader.dll"]
