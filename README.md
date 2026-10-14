# Sistema de Gestión de Proyectos de Graduación de la UNI

## 1. Descripción Breve del Sistema

El **Sistema de Gestión de Proyectos de Graduación de la UNI** es una plataforma backend diseñada bajo el estándar **ASP.NET Core Web API** y **Entity Framework Core**, conectada a **Microsoft SQL Server**. El sistema tiene como objetivo centralizar, automatizar y auditar el ciclo de vida de los proyectos y tesis de graduación en la Universidad Nacional de Ingeniería (UNI).

Permite a los estudiantes registrar propuestas de grado, consultar el estado de sus proyectos, y a los docentes tutores emitir revisiones y dictámenes formales. La solución implementa principios de **Clean Architecture** (separación estricta de responsabilidades entre Api, Application, Domain e Infrastructure), enfoque **API-First**, controladores delgados (*Thin Controllers*), inyección de dependencias, hashing seguro de contraseñas con **BCrypt**, autenticación sin estado mediante **JWT**, y optimizaciones de alto rendimiento utilizando `.AsNoTracking()` y operaciones asíncronas con `CancellationToken`.

---

## 2. Diagrama C4 Nivel 1: Contexto del Sistema

Este diagrama ilustra a los actores principales y las interacciones de alto nivel con el Sistema de Gestión de Proyectos de Graduación y los sistemas externos vinculados.

```mermaid
flowchart TD
    Estudiante["Estudiante UNI<br/>(Registra propuestas y consulta estados)"]
    Tutor["Docente Tutor / Revisor<br/>(Evalúa propuestas y emite dictámenes)"]
    Admin["Administrador de Facultad<br/>(Gestiona usuarios y programas académicos)"]

    SistemaUNI["Sistema de Gestión de Proyectos de Graduación UNI<br/>(Plataforma central de propuestas y revisiones)"]

    SysAcademico["Sistema de Información Académica UNI<br/>(Valida historial y matrícula estudiantil)"]
    SysCorreo["Servidor de Correo Institucional UNI<br/>(Notificaciones de cambio de estado)"]

    Estudiante -->|"Envía propuestas y consulta progreso [HTTPS]"| SistemaUNI
    Tutor -->|"Registra revisiones y aprueba propuestas [HTTPS]"| SistemaUNI
    Admin -->|"Administra usuarios y periodos de grado [HTTPS]"| SistemaUNI

    SistemaUNI -->|"Consulta elegibilidad académica [REST/SOAP]"| SysAcademico
    SistemaUNI -->|"Envía notificaciones de revisión [SMTP]"| SysCorreo
```

---

## 3. Diagrama C4 Nivel 2: Contenedores

Este diagrama detalla la arquitectura de contenedores internos del sistema, sus responsabilidades y los protocolos de comunicación.

```mermaid
flowchart TD
    subgraph Usuarios["Usuarios del Sistema"]
        U_Estudiante["Estudiante"]
        U_Docente["Docente / Tutor"]
    end

    subgraph AplicacionCliente["Frontend"]
        SPA["Aplicación Web SPA<br/>[React / Blazor / Angular]<br/>Interfaz interactiva para estudiantes y tutores"]
    end

    subgraph SistemaBackend["Contenedor Backend API"]
        API["ASP.NET Core Web API<br/>[.NET 10 / C#]<br/>Controladores REST y Endpoints API-First"]
        AppLayer["Capa de Aplicación<br/>[Services y DTOs]<br/>Lógica de negocio, validación y JWT"]
        InfraLayer["Capa de Infraestructura<br/>[AppDbContext / EF Core]<br/>Mapeo O/R y acceso a datos con AsNoTracking"]
    end

    subgraph Almacenamiento["Base de Datos"]
        DB[("Microsoft SQL Server<br/>Base de datos relacional<br/>Usuarios, Proyectos y Revisiones")]
    end

    U_Estudiante -->|"Interactúa con la interfaz [HTTPS]"| SPA
    U_Docente -->|"Interactúa con la interfaz [HTTPS]"| SPA

    SPA -->|"Solicitudes JSON / JWT [HTTPS/REST]"| API
    API -->|"Invoca servicios inyectados"| AppLayer
    AppLayer -->|"Persistencia y consultas asíncronas"| InfraLayer
    InfraLayer -->|"Consultas y comandos SQL [TCP/TDS]"| DB
```

---

## 4. Diagrama de Secuencia: Autenticación (Login)

Flujo de inicio de sesión seguro verificando credenciales con BCrypt y emitiendo un token JWT.

```mermaid
sequenceDiagram
    autonumber
    actor Usuario as Estudiante / Tutor
    participant AuthCtrl as AuthController
    participant AuthSvc as AuthService
    participant Hasher as BCrypt Net
    participant DB as SQL Server (AppDbContext)

    Usuario->>AuthCtrl: POST /api/auth/login (Email, Password)
    AuthCtrl->>AuthSvc: LoginAsync(request, cancellationToken)
    AuthSvc->>DB: Users.AsNoTracking().FirstOrDefaultAsync(Email)
    DB-->>AuthSvc: User (o null)

    alt Usuario no encontrado
        AuthSvc-->>AuthCtrl: null
        AuthCtrl-->>Usuario: 401 Unauthorized ("Credenciales incorrectas")
    else Usuario encontrado
        AuthSvc->>Hasher: Verify(Password, PasswordHash)
        alt Contraseña inválida
            Hasher-->>AuthSvc: false
            AuthSvc-->>AuthCtrl: null
            AuthCtrl-->>Usuario: 401 Unauthorized ("Credenciales incorrectas")
        else Contraseña válida
            Hasher-->>AuthSvc: true
            AuthSvc->>AuthSvc: GenerateJwtToken(user, claims, expiration)
            AuthSvc-->>AuthCtrl: LoginResponse (Token, UserId, Email, Role)
            AuthCtrl-->>Usuario: 200 OK (LoginResponse)
        end
    end
```

---

## 5. Diagrama de Secuencia: Registrar Propuesta de Proyecto

Flujo de creación de una nueva propuesta de graduación con controladores delgados y desacoplamiento de entidades mediante DTOs.

```mermaid
sequenceDiagram
    autonumber
    actor Estudiante as Estudiante
    participant ProjCtrl as ProjectsController
    participant ProjSvc as ProjectService
    participant DB as SQL Server (AppDbContext)

    Estudiante->>ProjCtrl: POST /api/projects (CreateProjectRequest)
    ProjCtrl->>ProjCtrl: Validar ModelState y parámetros de entrada
    ProjCtrl->>ProjSvc: CreateAsync(request, cancellationToken)
    ProjSvc->>ProjSvc: Mapear CreateProjectRequest a entidad Project (Status: "Pending")
    ProjSvc->>DB: Projects.Add(project)
    ProjSvc->>DB: SaveChangesAsync(cancellationToken)
    DB-->>ProjSvc: Entidad guardada con ID generado
    ProjSvc->>DB: Users.AsNoTracking().FirstOrDefaultAsync(StudentId)
    DB-->>ProjSvc: User (Estudiante)
    ProjSvc->>ProjSvc: Construir ProjectResponse (DTO)
    ProjSvc-->>ProjCtrl: ProjectResponse
    ProjCtrl-->>Estudiante: 201 Created (Location: /api/projects/{id}, ProjectResponse)
```

---

## 6. Diagrama de Secuencia: Revisión del Tutor

Flujo de emisión de dictamen y retroalimentación de un proyecto por parte de un tutor o revisor.

```mermaid
sequenceDiagram
    autonumber
    actor Tutor as Docente Tutor
    participant ProjCtrl as ProjectsController
    participant ProjSvc as ProjectService
    participant DB as SQL Server (AppDbContext)

    Tutor->>ProjCtrl: POST /api/projects/{id}/reviews (SubmitReviewRequest)
    ProjCtrl->>ProjCtrl: Validar NewStatus y formato
    ProjCtrl->>ProjSvc: ReviewProjectAsync(CreateReviewRequest, cancellationToken)
    ProjSvc->>DB: Projects.FirstOrDefaultAsync(Id)
    DB-->>ProjSvc: Project (o null)

    alt Proyecto no existe
        ProjSvc-->>ProjCtrl: null
        ProjCtrl-->>Tutor: 404 Not Found ("Proyecto no encontrado")
    else Proyecto encontrado
        ProjSvc->>ProjSvc: Actualizar Status = NewStatus
        ProjSvc->>DB: SaveChangesAsync(cancellationToken)
        DB-->>ProjSvc: Cambios guardados exitosamente
        ProjSvc->>ProjSvc: Construir ReviewResponse (DTO con marca de tiempo UTC)
        ProjSvc-->>ProjCtrl: ReviewResponse
        ProjCtrl-->>Tutor: 200 OK (ReviewResponse)
    end
```

---

## 7. Tabla de la API Principal

A continuación se resumen los endpoints desarrollados en la API, detallando sus rutas, métodos HTTP, modelos de datos de entrada/salida y códigos de estado:

| Método | Ruta Endpoint | Descripción | Request Body (DTO) | Response Body (DTO) | Códigos HTTP |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Autentica a un usuario y genera su token JWT firmado. | `LoginRequest` (`Email`, `Password`) | `LoginResponse` (`Token`, `UserId`, `Email`, `Role`) | `200 OK`<br/>`401 Unauthorized` |
| `GET` | `/api/projects/mine` | Lista los proyectos asociados al estudiante actual usando `.AsNoTracking()`. | N/A *(Query param opcional `studentId` o claim JWT)* | `IEnumerable<ProjectResponse>` | `200 OK`<br/>`400 Bad Request` |
| `POST` | `/api/projects` | Registra una nueva propuesta de proyecto de graduación. | `CreateProjectRequest` (`Title`, `Description`, `StudentId`) | `ProjectResponse` (`Id`, `Title`, `Description`, `Status`, `StudentId`, `StudentEmail`) | `201 Created`<br/>`400 Bad Request` |
| `GET` | `/api/projects/{id}` | Obtiene los detalles de un proyecto específico por su identificador. | N/A *(Parámetro de ruta `id`)* | `ProjectResponse` (`Id`, `Title`, `Description`, `Status`, `StudentId`, `StudentEmail`) | `200 OK`<br/>`404 Not Found` |
| `POST` | `/api/projects/{id}/reviews` | Registra la revisión de un tutor y actualiza el estado del proyecto. | `SubmitReviewRequest` (`Comments`, `NewStatus`) | `ReviewResponse` (`ProjectId`, `Comments`, `Status`, `ReviewedAt`) | `200 OK`<br/>`400 Bad Request`<br/>`404 Not Found` |

---

## 8. Consideraciones Técnicas y de Seguridad

- **Seguridad de Credenciales:** Las contraseñas nunca se persisten ni comparan en texto plano; se implementa el algoritmo de derivación de claves **BCrypt**.
- **Gestión de Secretos:** La cadena de conexión y la clave de firma JWT no se encuentran hardcodeadas en clases de C#, administrándose mediante el sistema de configuración de ASP.NET Core (`appsettings.json` o variables de entorno en producción).
- **Rendimiento:** Las operaciones de lectura aplican `.AsNoTracking()` para prescindir de la sobrecarga del seguimiento de cambios en memoria de Entity Framework Core.
- **Asincronía Total:** Todas las llamadas a base de datos y flujos de I/O son no bloqueantes (`async/await`) y propagan un `CancellationToken` para optimizar la cancelación temprana de solicitudes.
- **Encapsulación de Entidades:** El modelo de dominio (`User`, `Project`) nunca se expone directamente a los clientes de la API, intermediando a través de DTOs inmutables implementados con `record`.