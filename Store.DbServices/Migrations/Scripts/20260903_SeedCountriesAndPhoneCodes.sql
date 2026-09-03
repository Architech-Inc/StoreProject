-- ============================================================================
-- Migration: 20260903_SeedCountriesAndPhoneCodes.sql
-- Description: Standardize Country Codes, Dialing Codes, and Phone Links
-- ============================================================================

-- 1. Ensure country 1 is Cameroon (CM, +237) and country 2 is Nigeria (NG, +234)
UPDATE `country` 
SET `name` = 'Cameroon', `iso_code` = 'CM', `phone_code` = '+237', `last_modified` = NOW(6)
WHERE `country_id` = 1;

UPDATE `country` 
SET `name` = 'Nigeria', `iso_code` = 'NG', `phone_code` = '+234', `last_modified` = NOW(6)
WHERE `country_id` = 2;

-- 2. Seed international country registry
INSERT INTO `country` (`name`, `iso_code`, `phone_code`, `date_created`, `last_modified`) VALUES
('Gabon', 'GA', '+241', NOW(6), NOW(6)),
('Congo', 'CG', '+242', NOW(6), NOW(6)),
('Democratic Republic of the Congo', 'CD', '+243', NOW(6), NOW(6)),
('Chad', 'TD', '+235', NOW(6), NOW(6)),
('Central African Republic', 'CF', '+236', NOW(6), NOW(6)),
('Equatorial Guinea', 'GQ', '+240', NOW(6), NOW(6)),
('Ivory Coast', 'CI', '+225', NOW(6), NOW(6)),
('Ghana', 'GH', '+233', NOW(6), NOW(6)),
('Senegal', 'SN', '+221', NOW(6), NOW(6)),
('Mali', 'ML', '+223', NOW(6), NOW(6)),
('Burkina Faso', 'BF', '+226', NOW(6), NOW(6)),
('Benin', 'BJ', '+229', NOW(6), NOW(6)),
('Togo', 'TG', '+228', NOW(6), NOW(6)),
('Niger', 'NE', '+227', NOW(6), NOW(6)),
('Kenya', 'KE', '+254', NOW(6), NOW(6)),
('Rwanda', 'RW', '+250', NOW(6), NOW(6)),
('Uganda', 'UG', '+256', NOW(6), NOW(6)),
('Tanzania', 'TZ', '+255', NOW(6), NOW(6)),
('South Africa', 'ZA', '+27', NOW(6), NOW(6)),
('Ethiopia', 'ET', '+251', NOW(6), NOW(6)),
('Egypt', 'EG', '+20', NOW(6), NOW(6)),
('Morocco', 'MA', '+212', NOW(6), NOW(6)),
('Algeria', 'DZ', '+213', NOW(6), NOW(6)),
('Tunisia', 'TN', '+216', NOW(6), NOW(6)),
('France', 'FR', '+33', NOW(6), NOW(6)),
('United Kingdom', 'GB', '+44', NOW(6), NOW(6)),
('United States', 'US', '+1', NOW(6), NOW(6)),
('Canada', 'CA', '+1', NOW(6), NOW(6)),
('Germany', 'DE', '+49', NOW(6), NOW(6)),
('Belgium', 'BE', '+32', NOW(6), NOW(6)),
('Switzerland', 'CH', '+41', NOW(6), NOW(6)),
('Spain', 'ES', '+34', NOW(6), NOW(6)),
('Italy', 'IT', '+39', NOW(6), NOW(6)),
('Netherlands', 'NL', '+31', NOW(6), NOW(6)),
('Portugal', 'PT', '+351', NOW(6), NOW(6)),
('Turkey', 'TR', '+90', NOW(6), NOW(6)),
('United Arab Emirates', 'AE', '+971', NOW(6), NOW(6)),
('Saudi Arabia', 'SA', '+966', NOW(6), NOW(6)),
('China', 'CN', '+86', NOW(6), NOW(6)),
('India', 'IN', '+91', NOW(6), NOW(6)),
('Brazil', 'BR', '+55', NOW(6), NOW(6))
ON DUPLICATE KEY UPDATE 
    `name` = VALUES(`name`), 
    `phone_code` = VALUES(`phone_code`), 
    `last_modified` = NOW(6);

-- 3. Normalize legacy phone records:
-- Set any phone with country_id = 0 or NULL to country_id = 1 (Cameroon)
UPDATE `phone` SET `country_id` = 1 WHERE `country_id` = 0 OR `country_id` IS NULL;

-- 4. Create index on phone_code if not already existing
SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
               WHERE table_schema = DATABASE() AND table_name = 'country' AND index_name = 'ix_country_phone_code');
SET @sqlstmt := IF(@exist = 0, 'CREATE INDEX ix_country_phone_code ON country (phone_code)', 'SELECT 1');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
