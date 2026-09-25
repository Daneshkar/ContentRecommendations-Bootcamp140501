# WatchPair

WatchPair is an emotion-aware content recommendation system built with .NET 8. It recommends movies, books, games, and music from a user's current mood or from an experience the user has already shared.

Users can register and sign in, choose a primary mood with optional additional moods and themes, receive ranked recommendations, record experiences with media items, and review their experience history. Experience scores contribute to the aggregate mood and theme weights used by the recommendation engine.

## Solution Overview

The solution is divided into two backend services and one server-rendered web client:

```text
EmotionContentRecommender/
├── AuthService
├── AuthService.Application
├── AuthService.Domain
├── AuthService.Infrastructure
├── EmotionService
├── EmotionService.Application
├── EmotionService.Domain
├── EmotionService.Infrastructure
└── EmotionClient
```

### AuthService

`AuthService` owns identity and access concerns. Its use cases include:

- User registration and login
- JWT access and refresh tokens
- Logout and profile retrieval
- Mobile OTP delivery and verification
- Role- and status-aware user accounts

### EmotionService

`EmotionService` owns the recommendation domain and content catalog. Its use cases include:

- Managing media items for movies, books, games, and music
- Managing moods, themes, genres, item types, and media-specific details
- Recording, updating, deleting, and retrieving user experiences
- Producing mood-first recommendations
- Producing recommendations from a previous experience
- Recalculating aggregate mood and theme weights from user feedback

The recommendation queries rank eligible content by weighted mood and theme signals. They exclude content the user has already experienced and use encrypted, criteria-bound cursors for deterministic keyset pagination.

### EmotionClient

`EmotionClient` is an ASP.NET Core MVC application that provides the WatchPair user interface. It acts as the web-facing integration layer for the backend APIs through typed `HttpClient` services and contains the following flows:

- Registration and login
- Mood-based recommendation selection
- Experience submission and experience-based recommendations
- Recommendation results with a next-result flow
- User experience history

Authentication tokens are handled through HTTP-only cookies while the MVC application forwards authenticated requests to the backend services.

## Architecture

### Clean Architecture

Each backend service is organized into the familiar Clean Architecture areas:

- **Domain** contains entities, domain rules, enums, aggregate-root building blocks, and domain events.
- **Application** contains use cases, commands, queries, handlers, validators, and response contracts.
- **Infrastructure** contains Entity Framework Core persistence, SQL Server integration, JWT services, SMS integration, middleware, and background processing.
- **API** contains controllers and the application composition root.

This separation keeps domain concepts, application workflows, infrastructure concerns, and HTTP delivery represented as distinct parts of the solution.

### Vertical Slice Architecture

Backend use cases are grouped by feature rather than by technical type. Each feature folder contains the request, handler, validation, and response types required for one operation. Examples include:

```text
Features/
├── Experiences/
│   ├── Create/
│   ├── Update/
│   ├── Delete/
│   ├── GetById/
│   └── GetMine/
└── Recommendations/
    ├── Get/
    └── GetByExperience/
```

This structure makes each application use case an explicit vertical slice from its HTTP endpoint to its persistence interaction.

### CQRS and Mediator

Commands represent operations that change system state, while queries represent read operations. Both are dispatched through MediatR to dedicated request handlers. Controllers remain focused on HTTP concerns and delegate application behavior to these handlers.

MediatR also provides the mediator mechanism used by domain notifications and the request pipeline.

## Patterns and Building Blocks

### Domain-Driven Design Building Blocks

The domain projects model the system with entities and behavior instead of exposing persistence-only data structures. The solution uses:

- Encapsulated entity state and factory methods
- Aggregate-root and auditable entity abstractions in the authentication domain
- Domain events such as user creation notifications
- Explicit relationship entities for experience moods, experience themes, and media genres
- Domain services for rules such as converting experience scores into user weights

### Pipeline Behavior

FluentValidation validators are executed through a MediatR pipeline behavior. Validation therefore runs consistently before a command or query reaches its handler.

### Result and Error Handling

Application result models provide structured success and failure responses where required. Central exception-handling middleware maps application and domain failures to consistent HTTP error responses, including authentication and authorization errors.

### Dependency Injection and Options

Each executable project uses dependency injection as its composition mechanism. Infrastructure registrations, typed HTTP clients, JWT settings, SMS settings, and background-job settings are configured through extension methods and the .NET options pattern.

### Persistence

Entity Framework Core with SQL Server is used for persistence. Database mappings are separated into `IEntityTypeConfiguration` classes, migrations are maintained by the infrastructure projects, read-only queries use no-tracking projections, and multi-step writes use explicit transactions when atomicity is required.

### Background Processing and Durable Work Queue

Experience changes place the affected media item in a database-backed pending-update queue. A hosted background service processes pending items in batches and recalculates aggregate mood and theme weights. The processor uses versioned pending records and a SQL Server application lock to coordinate execution safely.

### Cursor-Based Pagination

Recommendation results use keyset pagination instead of offset pagination. The cursor stores the ranking boundary, aggregate support, and media item key. It is encrypted with AES-GCM and bound to a fingerprint of the active recommendation criteria so it cannot be reused with a different request.

### API Client Boundary

The MVC client accesses the backend through dedicated authentication and recommendation API services. These services centralize serialization, token forwarding, timeout handling, service-unavailable responses, and API error translation for the UI.

## Recommendation Model

WatchPair supports two recommendation modes:

1. **Mood-first recommendation** combines a primary mood, up to two additional moods, and optional themes. The primary mood has the largest ranking coefficient.
2. **Experience-based recommendation** uses the moods and themes recorded for a previous user experience to find related content of the same media type.

Both modes use aggregate item weights, active catalog data, deterministic tie-breaking, and user experience history when selecting eligible results.

Experience scores from 1 to 5 are converted into weights around a neutral midpoint. The scheduled aggregate-weight process applies those values to the mood and theme signals associated with each media item.

## Technology Stack

- .NET 8 and ASP.NET Core Web API
- ASP.NET Core MVC with Razor views
- Entity Framework Core and SQL Server
- MediatR
- FluentValidation
- JWT bearer authentication and refresh tokens
- BCrypt password hashing
- Swagger / OpenAPI
- Hosted background services
- Typed `HttpClient`
