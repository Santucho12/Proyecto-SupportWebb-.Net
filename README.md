# SupportWeb

Hola! Este es mi proyecto personal para la gestion de reclamos y soporte en empresas. El objetivo es ofrecer una plataforma moderna y eficiente para administrar solicitudes, notificaciones y roles de usuario.

Este sistema consume una API propia, desarrollada completamente por mi, para manejar toda la logica de usuarios, reclamos, respuestas y autenticacion.

## Estructura del proyecto

```
SupportWeb/
├── Controllers/
├── Filters/
├── Middleware/
├── Models/
│   ├── DTOs/
│   └── ViewModels/
├── Services/
├── Views/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Properties/
├── appsettings.json
├── appsettings.Development.json
├── SupportWeb.csproj
└── Program.cs
```

## Como compilar

1. Clona el repositorio en tu maquina.
2. Abre una terminal en la carpeta del proyecto.
3. Ejecuta:
   ```
   dotnet restore
   dotnet build
   dotnet run --project ./SupportWeb.csproj
   ```

## Endpoints principales

- `/Auth/Login` - Login de usuario
- `/Auth/Register` - Registro de usuario
- `/Auth/Logout` - Cerrar sesion
- `/Dashboard` - Dashboard principal segun el rol
- `/Admin/GestionReclamos` - Gestion de reclamos para admin
- `/Admin/GestionUsuarios` - Gestion de usuarios para admin
- `/Admin/Reportes` - Estadisticas y reportes
- `/Soporte/GestionReclamos` - Gestion de reclamos para soporte
- `/Soporte/CasosUrgentes` - Ver casos urgentes
- `/Usuario/CrearReclamo` - Crear nuevo reclamo
- `/Usuario/MisReclamos` - Ver mis reclamos
- `/Usuario/MiPerfil` - Ver y editar perfil
- `/Usuario/Notificaciones` - Notificaciones del usuario

Cada endpoint responde segun el rol y muestra la informacion relevante.

## Como correr los tests unitarios

1. Ve a la carpeta del proyecto.
2. Ejecuta:
   ```
   dotnet test
   ```

## Requisitos previos

- .NET 8.0 o superior
- Docker (opcional, para despliegue)
- Visual Studio o VS Code recomendado

## Consejos y buenas practicas

- Mantener la estructura modular y separar responsabilidades
- Usar servicios para la logica de negocio
- Validar datos tanto en backend como en frontend
- Usar vistas parciales para componentes reutilizables
- Documentar el codigo y las vistas
- Mantener los estilos en archivos CSS separados

## Contacto

Email: santysegal@gmail.com
LinkedIn:https://www.linkedin.com/in/santiago-segal-18bba3243/
