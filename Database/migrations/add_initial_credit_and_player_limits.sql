-- =====================================================
-- Migration: Add New Features to Snooker DB
-- =====================================================
-- 1. Add initial_credit_pk to customer table
-- 2. Add min_players and max_players to game_type table
-- =====================================================
-- Safe to run multiple times (checks existing columns)
-- =====================================================

USE snooker_club_db;

-- =====================================================
-- ADD: customer.initial_credit_pk
-- =====================================================
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists 
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = 'snooker_club_db' 
  AND TABLE_NAME = 'customer' 
  AND COLUMN_NAME = 'initial_credit_pk';

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE customer ADD COLUMN initial_credit_pk DECIMAL(10,2) NOT NULL DEFAULT 0', 
    'SELECT "✓ initial_credit_pk already exists" AS status');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- =====================================================
-- ADD: game_type.min_players
-- =====================================================
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists 
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = 'snooker_club_db' 
  AND TABLE_NAME = 'game_type' 
  AND COLUMN_NAME = 'min_players';

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE game_type ADD COLUMN min_players INT NOT NULL DEFAULT 2', 
    'SELECT "✓ min_players already exists" AS status');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- =====================================================
-- ADD: game_type.max_players
-- =====================================================
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists 
FROM information_schema.COLUMNS 
WHERE TABLE_SCHEMA = 'snooker_club_db' 
  AND TABLE_NAME = 'game_type' 
  AND COLUMN_NAME = 'max_players';

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE game_type ADD COLUMN max_players INT NOT NULL DEFAULT 4', 
    'SELECT "✓ max_players already exists" AS status');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- =====================================================
-- UPDATE: Set default values for existing rows
-- =====================================================
UPDATE game_type 
SET min_players = 2 
WHERE min_players IS NULL OR min_players = 0;

UPDATE game_type 
SET max_players = 4 
WHERE max_players IS NULL OR max_players = 0;

-- =====================================================
-- VERIFY: Show updated tables
-- =====================================================
SELECT '=== MIGRATION COMPLETE ===' AS result;

SELECT 'Customer Table:' AS section;
SELECT id, full_name, initial_credit_pk FROM customer LIMIT 5;

SELECT 'Game Type Table:' AS section;
SELECT id, name, min_players, max_players FROM game_type;
