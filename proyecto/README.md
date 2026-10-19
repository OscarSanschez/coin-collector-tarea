# Coin Collector

Un login web, un videojuego de Unity (WebGL) y una REST-API con base de datos. Inicia sesión, recoge monedas y guarda tu puntaje.

## Qué necesitas

- [Node.js](https://nodejs.org) versión **22.13 o superior** (revísalo con `node -v`)

No necesitas Unity para jugar; el juego ya viene compilado.

## 1. Descargar

1. En esta página de GitHub, haz clic en el botón verde **Code** y luego en **Download ZIP**.
2. Descomprímelo. Obtendrás una carpeta como `coin-collector-main` con `api`, `web` y `unity` adentro.

## 2. Iniciar la API

Abre una terminal en la carpeta `api` y ejecuta:

```bash
npm install
npm start
```

Deja esta terminal abierta. Debe decir `API escuchando en http://localhost:4000`.

## 3. Iniciar la página web

Abre una **segunda** terminal en la carpeta `web` y ejecuta:

```bash
npm install
npm start
```

Déjala abierta también. Debe decir `Web en http://localhost:5000`.

## 4. Jugar

1. Abre **http://localhost:5000** en tu navegador.
2. Haz clic en **Registrarse** para crear una cuenta y luego inicia sesión.
3. Muévete con **WASD** o las flechas y recoge las monedas.
4. Guarda tu puntaje pisando la plataforma verde, presionando **G** o haciendo clic en **Guardar puntaje**.

## ¿Problemas?

- **Windows dice "la ejecución de scripts está deshabilitada"**: usa `npm.cmd install` y `npm.cmd start`, o ejecuta los comandos en el Símbolo del sistema (`cmd`) en lugar de PowerShell.
- **"No se pudo conectar con la API"**: verifica que la terminal del paso 2 siga abierta.
- **La página no carga**: ábrela desde `http://localhost:5000`, no dando doble clic a los archivos HTML.

## Editar el juego (opcional)

Abre la carpeta `unity` con Unity Hub (**Add → Add project from disk**). Después de hacer cambios, compila para WebGL en `web/public/unity/`.
