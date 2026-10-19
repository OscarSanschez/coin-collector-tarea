// REST API del videojuego
//   Auth:  POST /auth/register   POST /auth/login
//   Juego: GET  /game/:user_id   PUT  /game/:user_id
const express = require('express');
const cors = require('cors');
const crypto = require('node:crypto');
const db = require('./db');

const PORT = process.env.PORT || 4000; // (el 6000 lo bloquean Chrome y Firefox)
const app = express();

app.use(cors());           // permite peticiones desde la web (puerto 5000) y desde Unity WebGL
app.use(express.json());

// ---------- utilidades ----------
function hashPassword(password) {
  const salt = crypto.randomBytes(16).toString('hex');
  const hash = crypto.scryptSync(password, salt, 64).toString('hex');
  return `${salt}:${hash}`;
}

function checkPassword(password, stored) {
  const [salt, hash] = stored.split(':');
  const test = crypto.scryptSync(password, salt, 64);
  return crypto.timingSafeEqual(test, Buffer.from(hash, 'hex'));
}

const publicUser = (u) => ({ id: u.id, name: u.name, points: u.points });

function parseId(req, res) {
  const id = Number.parseInt(req.params.user_id, 10);
  if (!Number.isInteger(id) || id <= 0) {
    res.status(400).json({ error: 'user_id inválido' });
    return null;
  }
  return id;
}

// ---------- autenticación ----------
// POST /auth/register   body: { name, password }   -> { id, name, points }
app.post('/auth/register', (req, res) => {
  const { name, password } = req.body || {};
  if (!name || !password) return res.status(400).json({ error: 'name y password son obligatorios' });

  const exists = db.prepare('SELECT id FROM users WHERE name = ?').get(name.trim());
  if (exists) return res.status(409).json({ error: 'Ese usuario ya existe' });

  const info = db
    .prepare('INSERT INTO users (name, password) VALUES (?, ?)')
    .run(name.trim(), hashPassword(password));
  const user = db.prepare('SELECT * FROM users WHERE id = ?').get(info.lastInsertRowid);
  res.status(201).json(publicUser(user));
});

// POST /auth/login   body: { name, password }   -> { id, name, points }
app.post('/auth/login', (req, res) => {
  const { name, password } = req.body || {};
  if (!name || !password) return res.status(400).json({ error: 'name y password son obligatorios' });

  const user = db.prepare('SELECT * FROM users WHERE name = ?').get(name.trim());
  if (!user || !checkPassword(password, user.password)) {
    return res.status(401).json({ error: 'Usuario o contraseña incorrectos' });
  }
  res.json(publicUser(user));
});

// ---------- juego ----------
// GET /game/:user_id   -> { id, name, points }
app.get('/game/:user_id', (req, res) => {
  const id = parseId(req, res);
  if (id === null) return;
  const user = db.prepare('SELECT * FROM users WHERE id = ?').get(id);
  if (!user) return res.status(404).json({ error: 'Usuario no encontrado' });
  res.json(publicUser(user));
});

// PUT /game/:user_id   body: { points }   -> { id, name, points }
app.put('/game/:user_id', (req, res) => {
  const id = parseId(req, res);
  if (id === null) return;
  const points = Number((req.body || {}).points);
  if (!Number.isInteger(points) || points < 0) {
    return res.status(400).json({ error: 'points debe ser un entero >= 0' });
  }
  const info = db
    .prepare("UPDATE users SET points = ?, updated_at = datetime('now') WHERE id = ?")
    .run(points, id);
  if (info.changes === 0) return res.status(404).json({ error: 'Usuario no encontrado' });
  const user = db.prepare('SELECT * FROM users WHERE id = ?').get(id);
  res.json(publicUser(user));
});

app.get('/', (_req, res) => res.json({ ok: true, message: 'Game API funcionando' }));

app.listen(PORT, () => console.log(`API escuchando en http://localhost:${PORT}`));
