// Servidor estático para el login y la página del videojuego.
const express = require('express');
const path = require('node:path');

const PORT = process.env.PORT || 5000;
const app = express();

// Unity WebGL comprime con Brotli/Gzip por defecto. Estas cabeceras permiten
// servir esos archivos aunque no se haya activado "Decompression Fallback".
app.use('/unity', (req, res, next) => {
  if (req.path.endsWith('.br')) res.set('Content-Encoding', 'br');
  else if (req.path.endsWith('.gz')) res.set('Content-Encoding', 'gzip');

  if (/\.wasm(\.br|\.gz)?$/.test(req.path)) res.type('application/wasm');
  else if (/\.js(\.br|\.gz)?$/.test(req.path)) res.type('application/javascript');
  else if (/\.data(\.br|\.gz)?$/.test(req.path)) res.type('application/octet-stream');
  next();
});

app.use(express.static(path.join(__dirname, 'public')));

app.listen(PORT, () => console.log(`Web en http://localhost:${PORT}`));
