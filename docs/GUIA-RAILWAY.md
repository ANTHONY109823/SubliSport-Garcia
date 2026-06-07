# Guía de despliegue — SUBLISPORT GARCIA en Railway

Esta guía te lleva paso a paso **después** de tener el código completo en tu repositorio de GitHub.

---

## Resumen del sistema

| Componente | Tecnología |
|---|---|
| Sitio público | `index.html` (landing) |
| Panel interno | .NET 10 Blazor Server |
| Base de datos | PostgreSQL |
| Hosting | Railway |

### Roles del sistema

| Rol | Permisos |
|---|---|
| **SuperAdmin** | Crear usuarios, todos los módulos, soporte |
| **Admin** | Pedidos, asignar diseñadores, gestión operativa |
| **Designer** | Ver y trabajar pedidos asignados |
| **Production** | Cola de impresión, planchado, confección, entrega |

---

## PASO 1 — Preparar PostgreSQL en Railway

1. Entra a [railway.app](https://railway.app) e inicia sesión.
2. Crea un **New Project**.
3. Haz clic en **+ New** → **Database** → **PostgreSQL**.
4. Cuando esté creada, abre la base de datos → pestaña **Variables**.
5. Copia la variable **`DATABASE_URL`** (o las variables `PGHOST`, `PGPORT`, `PGUSER`, `PGPASSWORD`, `PGDATABASE`).

> Railway usa una URL tipo:  
> `postgresql://usuario:clave@host:puerto/railway`

---

## PASO 2 — Crear el servicio de la aplicación

1. En el mismo proyecto Railway, clic en **+ New** → **GitHub Repo**.
2. Conecta tu cuenta de GitHub y selecciona **`SubliSport-Garcia`**.
3. Railway detectará el `Dockerfile` en la raíz.

---

## PASO 3 — Configurar variables de entorno (CRÍTICO)

En el servicio de la app → **Variables**, agrega:

| Variable | Valor | Notas |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Obligatorio |
| `ConnectionStrings__DefaultConnection` | Ver abajo | Conexión PostgreSQL |
| `Seed__SuperAdminEmail` | `tu-correo@dominio.com` | Solo primera vez |
| `Seed__SuperAdminPassword` | `ClaveSuperSegura123!` | Solo primera vez |

### Convertir DATABASE_URL a formato .NET

Si Railway te da:
```
postgresql://postgres:ABC123@containers-us-west-xxx.railway.app:6543/railway
```

Conviértelo a:
```
Host=containers-us-west-xxx.railway.app;Port=6543;Database=railway;Username=postgres;Password=ABC123;SSL Mode=Require;Trust Server Certificate=true
```

Pon ese valor en `ConnectionStrings__DefaultConnection`.

> **Seguridad:** Nunca pongas contraseñas en el código. Solo en variables de Railway.

---

## PASO 4 — Desplegar

1. Railway hará **build automático** con Docker.
2. Espera a que el deploy termine en verde.
3. Abre la URL pública que Railway te asigna (ej: `https://sublisport-garcia.up.railway.app`).

### Verificar

- `/` → Landing pública
- `/login` → Pantalla de ingreso (nueva pestaña desde el botón INGRESAR)
- Ingresa con el SuperAdmin que configuraste en las variables Seed

---

## PASO 5 — Primer acceso como SuperAdmin

1. Ve a `/login`.
2. Usa el correo y contraseña de `Seed__SuperAdminEmail` y `Seed__SuperAdminPassword`.
3. Entra al panel → **Usuarios**.
4. Crea:
   - 1 **Admin** (dueño/gerente)
   - 1 o más **Designer**
   - 1 o más **Production**

5. **Después de crear usuarios**, elimina o vacía las variables `Seed__*` en Railway por seguridad.

---

## PASO 6 — Flujo operativo del negocio

```
Cotización (web/WhatsApp)
        ↓
Admin registra pedido manual (/pedidos/nuevo)
        ↓
Admin asigna diseñador (/pedidos/{id})
        ↓
Diseñador trabaja y actualiza estado
        ↓
Producción: Impresión → Planchado → Confección → Entrega
        ↓
Pedido marcado como Entregado
```

---

## PASO 7 — Desarrollo local (opcional)

### Requisitos
- .NET 10 SDK
- PostgreSQL local (o Docker)

### Comandos

```powershell
# Desde la raíz del repo
cd src/SubliSport.Web
dotnet run
```

Configura `appsettings.Development.json` con tu PostgreSQL local.

La primera vez, EF Core aplicará migraciones automáticamente al iniciar.

---

## PASO 8 — Migraciones de base de datos

Si cambias modelos en el futuro:

```powershell
dotnet tool install --global dotnet-ef
cd src
dotnet ef migrations add NombreMigracion --project SubliSport.Infrastructure --startup-project SubliSport.Web
dotnet ef database update --project SubliSport.Infrastructure --startup-project SubliSport.Web
```

En Railway, las migraciones se aplican solas al iniciar la app (`InitializeDatabaseAsync`).

---

## PASO 9 — Dominio personalizado (opcional)

1. En Railway → servicio app → **Settings** → **Networking**.
2. Agrega tu dominio (ej: `sublisportgarcia.com`).
3. Configura el CNAME en tu registrador de dominios según indique Railway.

---

## Seguridad implementada

- Autenticación por cookies HttpOnly (servidor, no expuesta al JS del cliente)
- Contraseñas hasheadas con Identity
- Bloqueo tras 5 intentos fallidos
- Políticas de contraseña fuerte (10+ caracteres, mayúsculas, números, símbolos)
- Autorización por roles en cada página del panel
- Validación server-side en formularios
- Antiforgery tokens en Blazor
- Credenciales solo en variables de entorno
- Usuarios inactivos no pueden iniciar sesión

---

## Próximas fases sugeridas

1. Subida de archivos de diseño (logos, mockups)
2. Notificaciones WhatsApp/email al cambiar estado
3. Dashboard con métricas y reportes PDF
4. Auditoría completa de acciones por usuario
5. API REST protegida para app móvil

---

## Soporte

Si el deploy falla, revisa en Railway → **Deployments** → **View Logs** los errores de conexión a PostgreSQL o variables faltantes.
