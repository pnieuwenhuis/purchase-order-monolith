# Purchase order API service - Monolith example

This repository contains an API service for creating purchase orders based on
products known by the service and a customer that is registered at the service.
The service contains three domains (Customer, Product and Purchaseorder), all in
their own sub-project and no direct cross-references between the projects. This
solution is an example of a solution architecture where you can start by
building and growing the service with several domains, which can be splitted
later on in seperate microservices with a small effort to seperate the different
domains.

Each domain contains their own endpoint layer, a service layer and a repository
layer. Mapping of DTOs happen between the different layers. The Mapperly library
is used to facilitate the mapping code.

See this blog for more information:
https://dev.to/pnieuwenhuis/making-a-net-8-reference-solution-for-monoliths-and-microservices-3bgl

## Design goals

This example has the following design goals in mind:

- Complete seperation of the different business domains. Make sure that the
  service eventually can be splitted into mulitple microservices with little
  effort.
- Built on the latest .NET 8, including usage of Native AOT compiliation to make
  sure that the service has a low footprint on the running system. The service
  should also be runnable in an ARM-environment, such as a Kubernetes cluster
  with ARM-powered nodes.
- Includes metadata describing the service for developer platforms.

## Compilation and testing of the service

This repository requires that .NET 8 is installed on a Windows, MacOS or Linux
machine. Installation instructions can be found here:
https://dotnet.microsoft.com/en-us/download/dotnet/8.0

The given commands in this README are shell commands that can be run on MacOS or
Linux (or WSL on Windows), but could possible be executed on Windows without any
changes.

Compile the solution:

```bash
dotnet build --tl
```

Running unit-tests:

```bash
dotnet test ./tests/**/*.csproj
```

This repository also contains integration tests on the endpoints and the
repositories. To run the integration tests, the Docker runtime is required. The
command to run the integration tests:

```bash
dotnet test ./integration/**/*.csproj
```

## Running the service

The service requires some environment variables to be set. The repository
contains an `.env.example` which contains some default settings to run the
service in a local environment. Copy this file to `.env`, so the service can use
these variables.

To run the service, some dependencies such as the database are required. Use
Docker to start those dependencies:

```bash
docker compose up
```

The service can be started using this command:

```bash
ASPNETCORE_ENVIRONMENT=development dotnet run --project src/Nice.PurchaseOrder.ServiceApp
```

Or when you want to reload the application after changes:

```bash
ASPNETCORE_ENVIRONMENT=development dotnet watch --tl --project src/Nice.PurchaseOrder.ServiceApp run
```

After starting the service, an OpenAPI definition of all endpoints can be found
at: `http://localhost:5000/internal/openapi/v1/definition.json`

## Publication of the service

The service can be published as a self-contained executable without the need of
.NET for various platforms.

Windows-X64:

```bash
dotnet publish ./src/Nice.PurchaseOrder.ServiceApp/Nice.PurchaseOrder.ServiceApp.csproj -r win-x64 --configuration Release --output ./dist/win_x64
```

Linux-X64:

```bash
dotnet publish ./src/Nice.PurchaseOrder.ServiceApp/Nice.PurchaseOrder.ServiceApp.csproj -r linux-x64 --configuration Release --output ./dist/linux_x64
```

Linux-ARM:

```bash
dotnet publish ./src/Nice.PurchaseOrder.ServiceApp/Nice.PurchaseOrder.ServiceApp.csproj -r linux-arm64 --configuration Release --output ./dist/linux_arm64
```

MacOS-X64:

```bash
dotnet publish ./src/Nice.PurchaseOrder.ServiceApp/Nice.PurchaseOrder.ServiceApp.csproj -r osx-x64 --configuration Release --output ./dist/osx_x64
```

MacOS-ARM:

```bash
dotnet publish ./src/Nice.PurchaseOrder.ServiceApp/Nice.PurchaseOrder.ServiceApp.csproj -r osx-arm64 --configuration Release --output ./dist/osx_arm64
```
