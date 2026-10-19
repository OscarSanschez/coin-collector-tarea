// Base de datos SQLite (incluida en Node 22.13+, no requiere instalar nada extra).
// El archivo game.db se crea automáticamente junto a este script.
const path = require('node:path');
const { DatabaseSync } = require('node:sqlite');

const db = new DatabaseSync(path.join(__dirname, 'game.db'));

db.exec(`
  CREATE TABLE IF NOT EXISTS users (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    name       TEXT    NOT NULL UNIQUE,
    password   TEXT    NOT NULL,          -- hash scrypt: salt:hash
    points     INTEGER NOT NULL DEFAULT 0,
    updated_at TEXT    NOT NULL DEFAULT (datetime('now'))
  );
`);

module.exports = db;
