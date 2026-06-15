-- ============================================================
-- GameCompetionAnalysisSystem - PostgreSQL Schema
-- Generated from source code (Models + Services)
-- ============================================================

-- 1. Games catalog (Game.cs)
CREATE TABLE games (
    gameid      SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    genre       VARCHAR(100),
    publisher   VARCHAR(255)
);

-- 2. OCR analysis results (OcrResult.cs + AIController)
CREATE TABLE ocr_analyses (
    analysis_id         SERIAL PRIMARY KEY,
    filename            VARCHAR(500),
    language            VARCHAR(10),          -- "eng" | "vie"
    full_text           TEXT,
    image_width         INT,
    image_height        INT,
    processing_time_ms  DOUBLE PRECISION,
    success             BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at          TIMESTAMP   NOT NULL DEFAULT NOW()
);

-- 3. Leaderboard entries parsed from OCR (LeaderboardService + LeaderboardController)
CREATE TABLE leaderboard_entries (
    entry_id      SERIAL PRIMARY KEY,
    analysis_id   INT    NOT NULL REFERENCES ocr_analyses(analysis_id) ON DELETE CASCADE,
    game_id       INT             REFERENCES games(gameid)             ON DELETE SET NULL,
    player_name   VARCHAR(255),
    rank          INT,
    score         BIGINT,
    created_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_leaderboard_analysis ON leaderboard_entries(analysis_id);
CREATE INDEX idx_leaderboard_score    ON leaderboard_entries(score DESC);
CREATE INDEX idx_games_genre          ON games(genre);
