# Configuración Docker + Railway — SUBLISPORT GARCIA

## PARTE A — Probar en Docker (local, en tu PC)

### 1. Levantar todo con un solo comando

Desde la carpeta del proyecto:

```powershell
docker compose up -d --build
```

Esto inicia:
- **PostgreSQL** en puerto `5432`
- **App Blazor** en puerto `8080`

### 2. Verificar que funciona

| URL | Qué deberías ver |
|-----|------------------|
| http://localhost:8080 | Landing pública |
| http://localhost:8080/login | Pantalla de ingreso |

### 3. Credenciales de prueba (Docker local)

| Campo | Valor |
|-------|-------|
| Email | `admin@sublisport.local` |
| Contraseña | `SuperAdmin2026!` |

### 4. Comandos útiles

```powershell
# Ver logs en vivo
docker compose logs -f web

# Detener todo
docker compose down

# Detener y borrar base de datos (reinicio limpio)
docker compose down -v
```

---

## PARTE B — Desplegar en Railway (producción)

### Paso 1 — Subir código a GitHub

Si aún no has subido los cambios:

```powershell
git add .
git commit -m "Sistema de gestión con Docker y Railway"
git push origin main
```

### Paso 2 — Crear proyecto en Railway

1. Entra a **[railway.app](https://railway.app)** → **New Project**
2. Clic en **+ New** → **Database** → **PostgreSQL**
3. Espera a que la base de datos quede en verde

### Paso 3 — Conectar tu repositorio

1. En el mismo proyecto → **+ New** → **GitHub Repo**
2. Selecciona **`SubliSport-Garcia`**
3. Railway detectará el `Dockerfile` automáticamente

### Paso 4 — Vincular PostgreSQL con la app

1. Abre el servicio de la **app** (no la base de datos)
2. Ve a **Variables** → **+ New Variable** → **Add Reference**
3. Selecciona el servicio **PostgreSQL** → variable **`DATABASE_URL`**

> La app convierte `DATABASE_URL` automáticamente al formato .NET. **No necesitas convertirla manualmente.**

### Paso 5 — Agregar variables de entorno

En el servicio de la app → **Variables**, agrega:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Seed__SuperAdminEmail` | Tu correo real (ej: `anthony@tudominio.com`) |
| `Seed__SuperAdminPassword` | Una clave fuerte (ej: `MiClaveSegura2026!`) |

`DATABASE_URL` ya queda referenciada desde PostgreSQL en el paso 4.

### Paso 6 — Generar dominio público

1. Servicio app → **Settings** → **Networking**
2. Clic en **Generate Domain**
3. Obtendrás una URL como: `https://sublisport-garcia-production.up.railway.app`

### Paso 7 — Verificar deploy

1. Espera que el deploy quede en **verde** (Deployments)
2. Abre tu URL + `/login`
3. Ingresa con el SuperAdmin que configuraste
4. Ve a **Usuarios** y crea Admin, Diseñadores y Producción

### Paso 8 — Seguridad post-deploy

Una vez creados tus usuarios, **elimina** estas variables en Railway:
- `Seed__SuperAdminEmail`
- `Seed__SuperAdminPassword`

---

## Resumen visual en Railway

```
Proyecto Railway
├── PostgreSQL          ← Base de datos
│   └── DATABASE_URL    ← Referenciada por la app
└── SubliSport-Garcia   ← App (Dockerfile)
    ├── ASPNETCORE_ENVIRONMENT=Production
    ├── DATABASE_URL      ← Referencia a PostgreSQL
    ├── Seed__SuperAdminEmail
    └── Seed__SuperAdminPassword
```

---

## Solución de problemas

| Problema | Solución |
|----------|----------|
| Deploy falla en build | Revisa logs en Deployments → View Logs |
| Error de conexión a BD | Verifica que `DATABASE_URL` esté referenciada desde PostgreSQL |
| Login no funciona | Confirma que Seed creó el SuperAdmin (revisa logs: "SuperAdmin inicial creado") |
| Puerto incorrecto | Railway inyecta `PORT` automáticamente; la app ya lo detecta |
| Docker local no inicia | Ejecuta `docker compose down -v` y vuelve a `docker compose up -d --build` |

---

## Tu estado actual

Docker local ya está corriendo en tu PC:
- **App:** http://localhost:8080
- **Login:** http://localhost:8080/login

Siguiente paso: subir a GitHub y configurar Railway siguiendo la Parte B.
