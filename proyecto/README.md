# Coin Collector — Web + Unity + REST-API

```
proyecto/
├── api/     REST-API (Node + Express + SQLite)      → http://localhost:4000
├── web/     Login web + página del juego (iframe)    → http://localhost:5000
└── unity/   Scripts del videojuego (copiar a un proyecto de Unity)
```

Flujo: **Login** → la API responde `{id, name, points}` → la web redirige a `game.html?user_id=ID` → el iframe carga `unity/index.html?user_id=ID` → Unity lee el ID con `Application.absoluteURL` → `GET /game/ID` trae los puntos → al jugar se suman → `PUT /game/ID` los guarda.

## Requisitos
- Node.js **22.13 o superior** (la base de datos usa el SQLite que ya trae Node; no hay que instalar MySQL ni nada más).
- Unity 2021.3 o superior con el módulo **WebGL Build Support**.

## 1. API
```bash
cd api
npm install
npm start          # http://localhost:4000  (crea game.db automáticamente)
```

| Método | Ruta | Body | Respuesta |
|---|---|---|---|
| POST | `/auth/register` | `{name, password}` | `{id, name, points}` |
| POST | `/auth/login` | `{name, password}` | `{id, name, points}` |
| GET | `/game/:user_id` | — | `{id, name, points}` |
| PUT | `/game/:user_id` | `{points}` | `{id, name, points}` |

Las contraseñas se guardan con hash (scrypt). Se usa el puerto 4000 porque Chrome y Firefox bloquean el 6000.

## 2. Web
```bash
cd web
npm install
npm start          # http://localhost:5000
```
Abre http://localhost:5000, regístrate e inicia sesión. Mientras no copies el build de Unity, `web/public/unity/index.html` es un simulador que hace las mismas llamadas que el juego (así puedes comprobar Login → ID → API).
El comentario con todas las rutas de la API está en `web/public/game.html`.

## 3. Unity
1. Crea un proyecto **2D** nuevo en Unity.
2. Copia la carpeta `unity/Assets/Scripts` dentro de `Assets/` del proyecto.
3. En el menú superior: **Coin Collector → Crear escena del juego**. Se arma la escena completa (jugador, monedas, plataforma de guardado, UI y GameManager) y queda en Build Settings.
4. Con la API corriendo, presiona **Play** para probar. En el Editor no hay URL, así que usa el campo *Editor User Id* del objeto `GameManager` (pon el ID de un usuario existente).
5. **File → Build Settings → WebGL → Switch Platform → Build**, eligiendo como carpeta destino `web/public/unity/` (reemplaza el simulador).
6. Recarga http://localhost:5000 e inicia sesión.

Controles: **WASD/flechas** para moverte, cada moneda da 10 puntos. Para guardar: pisa la **plataforma verde**, presiona **G** o el botón **Guardar puntaje**.

### Scripts
| Archivo | Qué hace |
|---|---|
| `UrlParams.cs` | Lee `user_id` de `Application.absoluteURL` |
| `ApiClient.cs` | `GET` y `PUT` a `/game/:user_id` con `UnityWebRequest` |
| `GameManager.cs` | Carga el usuario, muestra/actualiza puntos y los guarda |
| `PlayerController2D.cs` | Movimiento (Input System nuevo o clásico) |
| `Coin.cs` | Suma puntos y reaparece |
| `SavePoint.cs` | Punto de guardado en el mapa |
| `Editor/GameSceneBuilder.cs` | Crea la escena automáticamente |

## Problemas comunes
- **"No se pudo conectar con la API"**: verifica que `api` esté corriendo. Si cambias el puerto, actualiza `web/public/config.js` y el campo *Base Url* del `ApiClient` en Unity.
- **El build no carga / error de descompresión**: el script de escena desactiva la compresión WebGL; si la activaste, el servidor web ya envía las cabeceras para `.br`/`.gz`.
- **No abras los HTML con doble clic** (`file://`): usa siempre http://localhost:5000.
