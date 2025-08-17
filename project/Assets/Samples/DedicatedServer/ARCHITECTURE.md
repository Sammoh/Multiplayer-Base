# Clean Architecture Diagram

```
DedicatedServer - Clean Architecture Layout
===========================================

┌──────────────────────────────────────────────────────────────────────────────┐
│                          PRESENTATION LAYER                                 │
│  ┌─────────────────────────────────┐  ┌─────────────────────────────────┐   │
│  │     Controllers/               │  │      MonoBehaviours/            │   │
│  │  ApplicationController.cs      │  │   ServerManager.cs              │   │
│  │  (Composition Root)            │  │   (Unity Lifecycle)             │   │
│  └─────────────────────────────────┘  └─────────────────────────────────┘   │
└───────────────────────────┬────────────────────────────┬─────────────────────┘
                            │                            │
                            ▼                            ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                        INFRASTRUCTURE LAYER                                 │
│  ┌─────────────────────────────────┐  ┌─────────────────────────────────┐   │
│  │        Unity/                   │  │       Networking/               │   │
│  │  UnityServerService.cs          │  │   NetworkString.cs              │   │
│  │  (Netcode Implementation)       │  │   (Serialization)               │   │
│  └─────────────────────────────────┘  └─────────────────────────────────┘   │
│  ┌─────────────────────────────────┐                                        │
│  │     Configuration/              │                                        │
│  │  UnityConfigurationService.cs   │                                        │
│  │  (Command Line & Settings)      │                                        │
│  └─────────────────────────────────┘                                        │
└───────────────────────────┬────────────────────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                                    │
│  ┌─────────────────────────────────┐  ┌─────────────────────────────────┐   │
│  │        UseCases/                │  │       Services/                 │   │
│  │  StartServerUseCase.cs          │  │   UserService.cs                │   │
│  │  ConnectToServerUseCase.cs      │  │   (User Management)             │   │
│  │  (Business Workflows)           │  │                                 │   │
│  └─────────────────────────────────┘  └─────────────────────────────────┘   │
└───────────────────────────┬────────────────────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                           DOMAIN LAYER                                      │
│  ┌─────────────────────────────────┐  ┌─────────────────────────────────┐   │
│  │        Entities/                │  │     ValueObjects/               │   │
│  │  User.cs                        │  │   GameEnums.cs                  │   │
│  │  GameSession.cs                 │  │   ServerConfiguration.cs        │   │
│  │  (Core Business Objects)        │  │   (Immutable Values)            │   │
│  └─────────────────────────────────┘  └─────────────────────────────────┘   │
│  ┌─────────────────────────────────┐                                        │
│  │       Interfaces/               │                                        │
│  │  IServerService.cs              │                                        │
│  │  IClientNetworkService.cs       │                                        │
│  │  IConfigurationService.cs       │                                        │
│  │  (Contracts & Boundaries)       │                                        │
│  └─────────────────────────────────┘                                        │
└──────────────────────────────────────────────────────────────────────────────┘

Dependency Direction: ALL ARROWS POINT INWARD ──────────────────► DOMAIN
```

## Key Architectural Decisions

### 1. Dependency Inversion
- All dependencies point toward the Domain layer
- Infrastructure implements Domain interfaces
- Application orchestrates Domain entities through interfaces

### 2. Separation of Concerns
- **Domain**: Pure business logic, no framework dependencies
- **Application**: Use cases and workflows
- **Infrastructure**: Framework-specific implementations (Unity, Netcode)
- **Presentation**: UI controllers and Unity lifecycle management

### 3. Interface Segregation
- Small, focused interfaces (IServerService, IConfigurationService)
- Easy to mock and test
- Clear contracts between layers

### 4. Single Responsibility
- Each class has one reason to change
- Clear boundaries between different concerns
- Modular and maintainable code

### 5. Backward Compatibility
- Legacy files marked as deprecated but functional
- Adapter classes for smooth migration
- Existing code continues to work during transition