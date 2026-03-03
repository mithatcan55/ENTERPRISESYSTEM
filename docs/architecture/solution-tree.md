# Solution Ağacı (İskelet)

```text
EnterpriseSystem.sln
│
├─ src
│  ├─ Host.Api
│  │  ├─ Program.cs
│  │  ├─ Middleware
│  │  │  ├─ CorrelationIdMiddleware.cs
│  │  │  └─ RequestLifecycleLoggingMiddleware.cs
│  │  └─ Services
│  │     └─ HttpContextAuditActorAccessor.cs
│  │
│  ├─ BuildingBlocks
│  │  ├─ SharedKernel
│  │  │  └─ Auditing
│  │  ├─ Application
│  │  └─ Infrastructure
│  │     └─ Persistence
│  │        ├─ BusinessDbContext.cs
│  │        ├─ LogDbContext.cs
│  │        ├─ Auditing
│  │        │  └─ IAuditActorAccessor.cs
│  │        ├─ Entities
│  │        │  ├─ Abstractions
│  │        │  │  └─ AuditableIntEntity.cs
│  │        │  ├─ Authorization
│  │        │  │  ├─ Module.cs
│  │        │  │  ├─ SubModule.cs
│  │        │  │  ├─ SubModulePage.cs
│  │        │  │  ├─ UserModulePermission.cs
│  │        │  │  ├─ UserSubModulePermission.cs
│  │        │  │  ├─ UserPagePermission.cs
│  │        │  │  ├─ UserCompanyPermission.cs
│  │        │  │  ├─ UserPageActionPermission.cs
│  │        │  │  └─ UserPageConditionPermission.cs
│  │        │  └─ (log entities)
│  │        └─ Migrations
│  │           ├─ BusinessDb
│  │           └─ LogDb
│  │
│  └─ Modules
│     └─ Identity
│        ├─ Identity.Domain
│        ├─ Identity.Application
│        ├─ Identity.Infrastructure
│        └─ Identity.Presentation
│
├─ tests
│  ├─ UnitTests
│  └─ IntegrationTests
│
└─ docs
   └─ architecture
      ├─ README.md
      ├─ solution-tree.md
      ├─ solution-dependency.mmd
      ├─ request-log-flow.mmd
      ├─ authorization-6-level.mmd
      ├─ class-diagram-authz.puml
      └─ sequence-tcode-access.puml
```
