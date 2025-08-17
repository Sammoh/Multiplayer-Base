# Clean Architecture Migration Guide

## Overview

The DedicatedServer folder has been reorganized following Clean Architecture / Domain Driven Design (DDD) principles. This ensures proper separation of concerns, clear dependency direction (always pointing inward toward Domain), and better testability.

## New Structure

```
DedicatedServer/
├── Domain/                          # Core business logic (no dependencies)
│   ├── Entities/                    # Core domain entities
│   │   ├── GameSession.cs           # Game configuration entity
│   │   └── User.cs                  # User domain entity
│   ├── ValueObjects/                # Immutable value types
│   │   ├── GameEnums.cs            # Map, GameMode, GameQueue, ConnectStatus
│   │   └── ServerConfiguration.cs   # Server network configuration
│   ├── Services/                    # Domain services (interfaces only)
│   └── Interfaces/                  # Repository and service interfaces
│       ├── IServerService.cs        # Server operations interface
│       ├── IClientNetworkService.cs # Client networking interface
│       └── IConfigurationService.cs # Configuration interface
├── Application/                     # Use cases and application logic
│   ├── UseCases/                   # Application use cases
│   │   ├── StartServerUseCase.cs   # Server startup orchestration
│   │   └── ConnectToServerUseCase.cs # Client connection logic
│   ├── Services/                   # Application services
│   │   └── UserService.cs          # User management service
│   └── Interfaces/                 # Application service interfaces
├── Infrastructure/                  # External concerns (Unity, Networking)
│   ├── Unity/                      # Unity-specific implementations
│   │   └── UnityServerService.cs   # Unity Netcode server implementation
│   ├── Networking/                 # Network implementations
│   │   └── NetworkString.cs        # Unity Netcode string serialization
│   ├── Configuration/              # Configuration and startup
│   │   └── UnityConfigurationService.cs # Unity command line and config
│   └── Persistence/                # Data persistence (if any)
└── Presentation/                   # UI and controllers
    ├── Controllers/                # Application controllers
    │   └── ApplicationController.cs # Main application entry point
    └── MonoBehaviours/             # Unity MonoBehaviour components
        └── ServerManager.cs        # Unity server lifecycle management
```

## Migration Path

### 1. Deprecated Files (Kept for Backward Compatibility)

The following files are marked as deprecated but remain functional:

- `ApplicationController.cs` → Use `Presentation/Controllers/ApplicationController.cs`
- `ApplicationData.cs` → Use `Infrastructure/Configuration/UnityConfigurationService.cs`
- `GameData.cs` → Use `Domain/Entities/GameSession.cs` and `Domain/Entities/User.cs`
- `NetworkModel.cs` → Use `Infrastructure/Networking/NetworkString.cs`
- `ServerGameManager.cs` → Use `Infrastructure/Unity/UnityServerService.cs`
- `ServerSingleton.cs` → Use `Presentation/MonoBehaviours/ServerManager.cs`

### 2. Key Benefits of New Architecture

- **Dependency Direction**: All dependencies point inward toward Domain
- **Testability**: Each layer can be tested in isolation
- **Separation of Concerns**: Clear boundaries between business logic and framework code
- **Modularity**: Features are modular and can be changed independently
- **Framework Independence**: Domain logic is independent of Unity/Netcode

### 3. Usage Examples

#### Old Way (Deprecated)
```csharp
// Old approach - tightly coupled to Unity
var serverManager = new ServerGameManager("127.0.0.1", 7777, networkManager);
await serverManager.StartGameServerAsync(serverStartInfo);
```

#### New Way (Clean Architecture)
```csharp
// New approach - dependency injection and separation of concerns
var configService = new UnityConfigurationService();
var serverConfig = configService.GetServerConfiguration();
var serverService = new UnityServerService(networkManager, serverConfig);

var startUseCase = new StartServerUseCase(serverService, configService);
var gameSession = new GameSession { Map = Map.Default, GameMode = GameMode.Meditating };

await startUseCase.ExecuteAsync(gameSession);
```

## Integration Points

The new architecture maintains all existing functionality while providing cleaner abstractions. Existing code will continue to work during the transition period.