# GoldEx - Jewelry Store Management

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-blueviolet)](https://dotnet.microsoft.com/en-us/platform/get-started/get-9-sdk)
[![Blazor Web App](https://img.shields.io/badge/Blazor-WebAssembly-brightgreen)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
![Domain-Driven Design](https://img.shields.io/badge/Domain--Driven--Design-DDD-blue)

GoldEx is an invoicing and inventory management program designed specifically for jewelry stores. It empowers jewelers to efficiently track sales, manage inventory, generate reports, and streamline their overall business operations. Built with Blazor Web App in .NET 9 and adhering to DDD Architecture principles, GoldEx offers a modern, maintainable, and scalable solution.

**Note: This project is currently under active development.  Features and functionality are subject to change.**

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Running the Application](#running-the-application)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Current Status

GoldEx is in its early stages of development.  The following features are currently implemented:

* **User Authentication:** Basic login and registration functionality is working. Users can create accounts and log in.
* **Account Management:** Two factor authentication and Google authentication has been added

## Features (Planned)

The following features are planned for future releases:

- **Sales Management:** Create and manage invoices, track payments, and generate sales reports.
- **Inventory Management:**  Maintain a detailed inventory of jewelry items, including descriptions, materials, and pricing.  Track stock levels and receive alerts for low stock.
- **Customer Management:** Store customer information, track purchase history, and manage customer relationships.
- **Reporting:** Generate various reports, including sales summaries, inventory reports, and profit margins.
- **User Management:** Control access to different features based on user roles. (Future Implementation)
- **Secure Data Handling:**  (Future Implementation -  Details to be added regarding security measures implemented).

## Technologies Used

- **.NET 9:** The cross-platform framework for building modern applications.
- **Blazor Web App:** A framework for building interactive web UIs with C#.
- **DDD Architecture:** A software design principle that separates concerns and promotes maintainability.
- **Entity Framework Core:** An ORM for database interactions.
- **(Database):** SQL Server

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/platform/get-started/get-9-sdk)
- (Database Server - if applicable)
- (IDE - Recommended: Visual Studio 2022 or later)

### Installation

1. Clone the repository: `git clone https://github.com/urmiaking/GoldEx.git`
2. Navigate to the project directory: `cd GoldEx`
3. (Restore NuGet Packages): `dotnet restore`

### Running the Application

1. Open the solution in your preferred IDE (e.g., Visual Studio).
2. (Configure Database Connection): Update the database connection string in the `appsettings.json` file.
3. Build the solution.
4. Run the application.

## Project Structure

The GoldEx solution is organized as follows:

<pre>
<code>
GoldEx/
├── src/
│   ├── App/
│   │   ├── Server/
│   │   │   ├── GoldEx.Server/
│   │   │   ├── GoldEx.Server.Application/
│   │   │   ├── GoldEx.Server.Domain/
│   │   │   └── GoldEx.Server.Infrastructure/
│   │   └── Client/
│   │       ├── GoldEx.Client/
│   │       └── GoldEx.Client.Components/
│   │   └── Shared/
│   │       └── GoldEx.Shared/
│   └── Sdk/
│       ├── GoldEx.Sdk.Client/
│       ├── GoldEx.Sdk.Common/
│       └── GoldEx.Sdk.Server/
└── ...
</code>
</pre>

**Explanation of the Structure:**

* **App:** This folder encapsulates the main application logic, split into Server (backend API) and Client (Blazor frontend).  The Shared project holds common resources.
    * **Server:** Contains projects related to the backend API.
        * **GoldEx.Server:** The main API project, likely containing controllers and services that handle requests.
        * **GoldEx.Server.Application:** Implements the application logic, use cases, and business rules of the API.
        * **GoldEx.Server.Domain:** Defines the core domain entities, events, and interfaces.
        * **GoldEx.Server.Infrastructure:** Provides implementations for interfaces defined in the Domain project.  This includes data access (e.g., using Entity Framework Core), logging, and other infrastructure concerns.  This is the "Infrastructure Layer."
    * **Client:** Contains the Blazor WebAssembly application.
        * **GoldEx.Client:** The main Blazor project, setting up the UI and handling user interactions.
        * **GoldEx.Client.Components:** Holds reusable Blazor components for the UI.
    * **Shared:** Contains resources (models, DTOs, utilities) shared between the Server and Client projects.
* **Sdk:**  This folder likely contains Software Development Kits (SDKs) for interacting with the GoldEx system.
    * **GoldEx.Sdk.Client:**  A client-side SDK for simplifying communication with the GoldEx API.
    * **GoldEx.Sdk.Common:** Common utilities used by the SDK projects.
    * **GoldEx.Sdk.Server:** A server-side SDK (if needed).  This might be used if you have other services that need to interact with GoldEx.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.  See `CONTRIBUTING.md` for more details. (Create this file if you wish to accept contributions).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any questions or inquiries, please contact:

Masoud Khodadadi
masoud.xpress@gmail.com
