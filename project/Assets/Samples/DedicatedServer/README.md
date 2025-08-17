# DedicatedServer - Clean Architecture

This folder contains a multiplayer dedicated server implementation following **Clean Architecture** and **Domain Driven Design (DDD)** principles.

## Architecture Overview

The code is organized into four main layers with clear dependency direction (all dependencies point inward toward Domain):

```
┌─────────────────────────┐
│     Presentation        │ ← Controllers, MonoBehaviours
├─────────────────────────┤
│     Infrastructure      │ ← Unity, Networking, Config
├─────────────────────────┤ 
│     Application         │ ← Use Cases, App Services
├─────────────────────────┤
│        Domain           │ ← Entities, Value Objects, Interfaces
└─────────────────────────┘
```

## Key Benefits

- **Testable**: Each layer can be unit tested in isolation
- **Maintainable**: Clear separation of concerns
- **Framework Independent**: Business logic is not coupled to Unity/Netcode
- **Modular**: Features can be added/modified without affecting other layers
- **SOLID Principles**: Follows dependency inversion and interface segregation

## Folder Structure

### Domain Layer (`/Domain/`)
Pure business logic with no external dependencies:
- `Entities/` - Core business entities (User, GameSession)
- `ValueObjects/` - Immutable value types (ServerConfiguration, Enums)
- `Interfaces/` - Contracts for external services
- `Services/` - Domain service interfaces (when needed)

### Application Layer (`/Application/`)
Orchestrates domain objects and external services:
- `UseCases/` - Application use cases (StartServerUseCase, ConnectToServerUseCase)
- `Services/` - Application services (UserService)
- `Interfaces/` - Application service contracts

### Infrastructure Layer (`/Infrastructure/`)
External concerns and framework-specific implementations:
- `Unity/` - Unity-specific implementations
- `Networking/` - Network protocol implementations
- `Configuration/` - Configuration management
- `Persistence/` - Data persistence (if needed)

### Presentation Layer (`/Presentation/`)
User interface and external triggers:
- `Controllers/` - Application controllers
- `MonoBehaviours/` - Unity MonoBehaviour wrappers

## Usage Examples

### Starting a Server (New Architecture)
```csharp
// Dependency injection setup
var configService = new UnityConfigurationService();
var serverConfig = configService.GetServerConfiguration();
var serverService = new UnityServerService(networkManager, serverConfig);

// Use case execution
var startUseCase = new StartServerUseCase(serverService, configService);
var gameSession = new GameSession 
{ 
    Map = Map.Default, 
    GameMode = GameMode.Meditating,
    MaxUsers = 10
};

bool success = await startUseCase.ExecuteAsync(gameSession);
```

### Managing Users
```csharp
var userService = new UserService();
var user = userService.CreateUser("PlayerName", "auth123");

userService.UpdateUserPreferences(
    Map.Space, 
    GameMode.Staring, 
    GameQueue.Competitive, 
    "password123"
);
```

## Backward Compatibility

Existing code continues to work with the legacy files:
- Old files are marked as deprecated but remain functional
- Migration guide available in `MIGRATION_GUIDE.md`
- Adapter classes in `/Legacy/` folder provide smooth transition

## Testing

Run architecture tests to verify the implementation:
```csharp
DedicatedServer.Tests.ArchitectureTests.RunTests();
```

## Dependencies

The architecture enforces strict dependency rules:
- **Domain** → No external dependencies
- **Application** → Only depends on Domain
- **Infrastructure** → Depends on Domain + Application + External frameworks
- **Presentation** → Can depend on all layers (composition root)

This ensures that business logic remains pure and testable while external concerns are properly isolated.

## Legacy Information

Uses Sessions to connect players to a server and allows them to interact with each other.

## TODO: 
- Complete client-side Clean Architecture implementation
- Add more comprehensive tests
- Create example implementations for common use cases 
