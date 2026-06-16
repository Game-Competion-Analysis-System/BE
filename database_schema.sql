-- ============================================================
-- GameCompetionAnalysisSystem - PostgreSQL Schema
-- Source: DAL/Entities/Swd392GameAiContext.cs (EF Core Fluent API)
-- Database: swd392_game_ai
-- ============================================================

-- Drop tables in reverse FK order (safe re-run)
DROP TABLE IF EXISTS leaderboardentry  CASCADE;
DROP TABLE IF EXISTS leaderboard       CASCADE;
DROP TABLE IF EXISTS aiextractedfield  CASCADE;
DROP TABLE IF EXISTS aianalysis        CASCADE;
DROP TABLE IF EXISTS imageupload       CASCADE;
DROP TABLE IF EXISTS "User"            CASCADE;
DROP TABLE IF EXISTS player            CASCADE;
DROP TABLE IF EXISTS guild             CASCADE;
DROP TABLE IF EXISTS event             CASCADE;
DROP TABLE IF EXISTS server            CASCADE;
DROP TABLE IF EXISTS game              CASCADE;
DROP TABLE IF EXISTS company           CASCADE;

-- ============================================================
-- 1. company
-- ============================================================
CREATE TABLE company (
    companyid   SERIAL          PRIMARY KEY  CONSTRAINT company_pkey,
    companyname VARCHAR(255),
    country     VARCHAR(100),
    website     VARCHAR(255)
);

-- ============================================================
-- 2. game  (depends on: company)
-- ============================================================
CREATE TABLE game (
    gameid      SERIAL          PRIMARY KEY  CONSTRAINT game_pkey,
    gamename    VARCHAR(255),
    genre       VARCHAR(100),
    companyid   INT,

    CONSTRAINT fk_game_company
        FOREIGN KEY (companyid) REFERENCES company (companyid)
);

-- ============================================================
-- 3. server  (depends on: game)
-- ============================================================
CREATE TABLE server (
    serverid    SERIAL          PRIMARY KEY  CONSTRAINT server_pkey,
    servername  VARCHAR(255),
    gameid      INT,
    region      VARCHAR(100),
    status      VARCHAR(20),

    CONSTRAINT fk_server_game
        FOREIGN KEY (gameid) REFERENCES game (gameid)
);

-- ============================================================
-- 4. guild  (depends on: server)
-- ============================================================
CREATE TABLE guild (
    guildid         SERIAL      PRIMARY KEY  CONSTRAINT guild_pkey,
    guildname       VARCHAR(255),
    serverid        INT,
    leaderplayerid  INT,

    CONSTRAINT fk_guild_server
        FOREIGN KEY (serverid) REFERENCES server (serverid)
);

-- ============================================================
-- 5. player  (depends on: game, server, guild)
-- ============================================================
CREATE TABLE player (
    playerid    SERIAL          PRIMARY KEY  CONSTRAINT player_pkey,
    playername  VARCHAR(255),
    gameid      INT,
    serverid    INT,
    guildid     INT,

    CONSTRAINT fk_player_game
        FOREIGN KEY (gameid)   REFERENCES game   (gameid),
    CONSTRAINT fk_player_server
        FOREIGN KEY (serverid) REFERENCES server (serverid),
    CONSTRAINT fk_player_guild
        FOREIGN KEY (guildid)  REFERENCES guild  (guildid)
);

-- ============================================================
-- 6. event  (depends on: game)
-- ============================================================
CREATE TABLE event (
    eventid     SERIAL          PRIMARY KEY  CONSTRAINT event_pkey,
    eventname   VARCHAR(255),
    gameid      INT,
    eventtype   VARCHAR(100),
    startdate   TIMESTAMP WITHOUT TIME ZONE,
    enddate     TIMESTAMP WITHOUT TIME ZONE,

    CONSTRAINT fk_event_game
        FOREIGN KEY (gameid) REFERENCES game (gameid)
);

-- ============================================================
-- 7. "User"  (no external FK dependencies)
-- Note: table name is case-sensitive ("User" with capital U)
-- ============================================================
CREATE TABLE "User" (
    userid          SERIAL      PRIMARY KEY  CONSTRAINT "User_pkey",
    username        VARCHAR(100),
    email           VARCHAR(255),
    passwordhash    VARCHAR(255),
    role            VARCHAR(20),

    CONSTRAINT "User_username_key" UNIQUE (username),
    CONSTRAINT "User_email_key"    UNIQUE (email)
);

-- ============================================================
-- 8. imageupload  (depends on: "User", event)
-- ============================================================
CREATE TABLE imageupload (
    uploadid    SERIAL          PRIMARY KEY  CONSTRAINT imageupload_pkey,
    userid      INT,
    eventid     INT,
    imageurl    VARCHAR(500),
    uploadtime  TIMESTAMP WITHOUT TIME ZONE,
    status      VARCHAR(50),

    CONSTRAINT fk_upload_user
        FOREIGN KEY (userid)  REFERENCES "User" (userid),
    CONSTRAINT fk_upload_event
        FOREIGN KEY (eventid) REFERENCES event  (eventid)
);

-- ============================================================
-- 9. aianalysis  (depends on: imageupload)
-- ============================================================
CREATE TABLE aianalysis (
    analysisid      SERIAL      PRIMARY KEY  CONSTRAINT aianalysis_pkey,
    uploadid        INT,
    aimodelversion  VARCHAR(100),
    confidencescore DOUBLE PRECISION,
    processedtime   TIMESTAMP WITHOUT TIME ZONE,

    CONSTRAINT fk_analysis_upload
        FOREIGN KEY (uploadid) REFERENCES imageupload (uploadid)
);

-- ============================================================
-- 10. aiextractedfield  (depends on: aianalysis)
-- ============================================================
CREATE TABLE aiextractedfield (
    fieldid     SERIAL          PRIMARY KEY  CONSTRAINT aiextractedfield_pkey,
    analysisid  INT,
    rawtext     VARCHAR(500),
    fieldtype   VARCHAR(100),
    confidence  DOUBLE PRECISION,

    CONSTRAINT fk_field_analysis
        FOREIGN KEY (analysisid) REFERENCES aianalysis (analysisid)
);

-- ============================================================
-- 11. leaderboard  (depends on: event, aianalysis)
-- ============================================================
CREATE TABLE leaderboard (
    leaderboardid           SERIAL  PRIMARY KEY  CONSTRAINT leaderboard_pkey,
    eventid                 INT,
    title                   VARCHAR(255),
    metrictype              VARCHAR(100),
    createdfromanalysisid   INT,

    CONSTRAINT fk_leaderboard_event
        FOREIGN KEY (eventid)               REFERENCES event      (eventid),
    CONSTRAINT fk_leaderboard_analysis
        FOREIGN KEY (createdfromanalysisid) REFERENCES aianalysis (analysisid)
);

-- ============================================================
-- 12. leaderboardentry  (depends on: leaderboard, player)
-- ============================================================
CREATE TABLE leaderboardentry (
    entryid         SERIAL  PRIMARY KEY  CONSTRAINT leaderboardentry_pkey,
    leaderboardid   INT,
    playerid        INT,
    rank            INT,
    value           DOUBLE PRECISION,

    CONSTRAINT fk_entry_leaderboard
        FOREIGN KEY (leaderboardid) REFERENCES leaderboard (leaderboardid),
    CONSTRAINT fk_entry_player
        FOREIGN KEY (playerid)      REFERENCES player      (playerid)
);

-- ============================================================
-- INDEXES  (performance)
-- ============================================================
CREATE INDEX idx_game_companyid        ON game             (companyid);
CREATE INDEX idx_server_gameid         ON server           (gameid);
CREATE INDEX idx_guild_serverid        ON guild            (serverid);
CREATE INDEX idx_player_gameid         ON player           (gameid);
CREATE INDEX idx_player_serverid       ON player           (serverid);
CREATE INDEX idx_player_guildid        ON player           (guildid);
CREATE INDEX idx_event_gameid          ON event            (gameid);
CREATE INDEX idx_imageupload_userid    ON imageupload      (userid);
CREATE INDEX idx_imageupload_eventid   ON imageupload      (eventid);
CREATE INDEX idx_aianalysis_uploadid   ON aianalysis       (uploadid);
CREATE INDEX idx_aiextracted_analysisid ON aiextractedfield (analysisid);
CREATE INDEX idx_leaderboard_eventid   ON leaderboard      (eventid);
CREATE INDEX idx_leaderboard_analysisid ON leaderboard     (createdfromanalysisid);
CREATE INDEX idx_entry_leaderboardid   ON leaderboardentry (leaderboardid);
CREATE INDEX idx_entry_playerid        ON leaderboardentry (playerid);

-- ============================================================
-- SAMPLE DATA  (đủ để chạy app bình thường)
-- ============================================================

-- company
INSERT INTO company (companyname, country, website) VALUES
    ('VNG Corporation',  'Vietnam', 'https://vng.com.vn'),
    ('Garena',           'Singapore', 'https://garena.com'),
    ('Riot Games',       'USA', 'https://riotgames.com');

-- game
INSERT INTO game (gamename, genre, companyid) VALUES
    ('Võ Lâm Truyền Kỳ', 'MMORPG', 1),
    ('Liên Quân Mobile',  'MOBA',   2),
    ('League of Legends', 'MOBA',   3);

-- server
INSERT INTO server (servername, gameid, region, status) VALUES
    ('Server 1 - Thiếu Lâm', 1, 'Vietnam', 'active'),
    ('Server 2 - Võ Đang',   1, 'Vietnam', 'active'),
    ('Server SEA-01',         2, 'SEA',     'active');

-- guild
INSERT INTO guild (guildname, serverid, leaderplayerid) VALUES
    ('Thiên Long Bang', 1, 1),
    ('Cái Bang',        1, 2),
    ('Long Phụng Hội',  2, 3);

-- player
INSERT INTO player (playername, gameid, serverid, guildid) VALUES
    ('KiếmKhách01', 1, 1, 1),
    ('TiểuLongNữ',  1, 1, 2),
    ('VôKỵ',        1, 2, 3),
    ('NamVô Kỵ',    1, 2, 3);

-- "User"
INSERT INTO "User" (username, email, passwordhash, role) VALUES
    ('admin',   'admin@swd392.com',   '$2a$11$placeholder_hash_admin',  'admin'),
    ('player1', 'player1@gmail.com',  '$2a$11$placeholder_hash_p1',     'user'),
    ('player2', 'player2@gmail.com',  '$2a$11$placeholder_hash_p2',     'user');

-- event
INSERT INTO event (eventname, gameid, eventtype, startdate, enddate) VALUES
    ('Giải Đấu Bang Chiến Tháng 6',  1, 'tournament', '2026-06-01 09:00:00', '2026-06-30 23:59:59'),
    ('PK Cá Nhân Tuần 24',           1, 'pvp',        '2026-06-10 18:00:00', '2026-06-10 22:00:00');

-- imageupload
INSERT INTO imageupload (userid, eventid, imageurl, uploadtime, status) VALUES
    (2, 1, 'https://res.cloudinary.com/demo/image/upload/sample1.jpg', '2026-06-10 18:05:00', 'processed'),
    (3, 1, 'https://res.cloudinary.com/demo/image/upload/sample2.jpg', '2026-06-10 18:10:00', 'processed'),
    (2, 2, 'https://res.cloudinary.com/demo/image/upload/sample3.jpg', '2026-06-10 19:00:00', 'pending');

-- aianalysis
INSERT INTO aianalysis (uploadid, aimodelversion, confidencescore, processedtime) VALUES
    (1, 'llama-3.2-90b-vision-preview', 0.95, '2026-06-10 18:05:30'),
    (2, 'llama-3.2-90b-vision-preview', 0.87, '2026-06-10 18:10:45');

-- aiextractedfield
INSERT INTO aiextractedfield (analysisid, rawtext, fieldtype, confidence) VALUES
    (1, 'KiếmKhách01', 'player_name', 0.98),
    (1, '1',           'rank',        0.99),
    (1, '125000',      'score',       0.95),
    (2, 'TiểuLongNữ',  'player_name', 0.91),
    (2, '2',           'rank',        0.99),
    (2, '118500',      'score',       0.85);

-- leaderboard
INSERT INTO leaderboard (eventid, title, metrictype, createdfromanalysisid) VALUES
    (1, 'Bảng Xếp Hạng Bang Chiến Tháng 6', 'score', 1),
    (2, 'Bảng PK Cá Nhân Tuần 24',          'score', 2);

-- leaderboardentry
INSERT INTO leaderboardentry (leaderboardid, playerid, rank, value) VALUES
    (1, 1, 1, 125000),
    (1, 2, 2, 118500),
    (1, 3, 3, 105000),
    (2, 1, 1, 45000),
    (2, 4, 2, 42000);
