-- ==============================================================================
-- Multi-Branch Isolation, Smart Pricing & Temporal Access Migration
-- ==============================================================================

-- 1. Alter branch table
ALTER TABLE `branch`
    ADD COLUMN IF NOT EXISTS `price_multiplier` decimal(18,4) NOT NULL DEFAULT 1.0000,
    ADD COLUMN IF NOT EXISTS `tax_rate_override` decimal(18,4) NULL;

-- 2. Alter user_branch_role table
ALTER TABLE `user_branch_role`
    ADD COLUMN IF NOT EXISTS `valid_from` datetime(6) NULL,
    ADD COLUMN IF NOT EXISTS `valid_to` datetime(6) NULL,
    ADD COLUMN IF NOT EXISTS `grant_reason` varchar(255) NULL,
    ADD COLUMN IF NOT EXISTS `is_active` tinyint(1) NOT NULL DEFAULT 1;

-- 3. Alter employee table
ALTER TABLE `employee`
    ADD COLUMN IF NOT EXISTS `home_branch_id` int(11) NULL;

-- 4. Alter discount table
ALTER TABLE `discount`
    ADD COLUMN IF NOT EXISTS `scope` int(11) NOT NULL DEFAULT 0;

-- 5. Alter loyalty_campaign table
ALTER TABLE `loyalty_campaign`
    ADD COLUMN IF NOT EXISTS `scope` int(11) NOT NULL DEFAULT 0;

-- 6. Alter stock_movement table
ALTER TABLE `stock_movement`
    ADD COLUMN IF NOT EXISTS `branch_id` int(11) NULL;

-- 7. Create branch_item_stock table
CREATE TABLE IF NOT EXISTS `branch_item_stock` (
    `branch_id` int(11) NOT NULL,
    `item_id` char(36) NOT NULL,
    `in_stock` int(11) NOT NULL DEFAULT 0,
    `reorder_level` int(11) NULL,
    `custom_unit_price` decimal(18,2) NULL,
    `custom_cost_price` decimal(18,2) NULL,
    `date_created` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `last_modified` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`branch_id`, `item_id`),
    CONSTRAINT `FK_branch_item_stock_branch` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_branch_item_stock_item` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Create personnel_transfer_history table
CREATE TABLE IF NOT EXISTS `personnel_transfer_history` (
    `personnel_transfer_id` bigint(20) NOT NULL AUTO_INCREMENT,
    `employee_id` char(36) NOT NULL,
    `from_branch_id` int(11) NOT NULL,
    `to_branch_id` int(11) NOT NULL,
    `transfer_type` int(11) NOT NULL DEFAULT 0,
    `effective_date` datetime(6) NOT NULL,
    `expected_end_date` datetime(6) NULL,
    `reason` varchar(500) NULL,
    `approved_by_user_id` char(36) NULL,
    `date_created` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `last_modified` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`personnel_transfer_id`),
    CONSTRAINT `FK_personnel_transfer_employee` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_personnel_transfer_from_branch` FOREIGN KEY (`from_branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_personnel_transfer_to_branch` FOREIGN KEY (`to_branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_personnel_transfer_approved_by` FOREIGN KEY (`approved_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Create discount_branch table
CREATE TABLE IF NOT EXISTS `discount_branch` (
    `discount_id` int(11) NOT NULL,
    `branch_id` int(11) NOT NULL,
    `date_created` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `last_modified` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`discount_id`, `branch_id`),
    CONSTRAINT `FK_discount_branch_discount` FOREIGN KEY (`discount_id`) REFERENCES `discount` (`discount_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_discount_branch_branch` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10. Create loyalty_campaign_branch table
CREATE TABLE IF NOT EXISTS `loyalty_campaign_branch` (
    `loyalty_campaign_id` int(11) NOT NULL,
    `branch_id` int(11) NOT NULL,
    `date_created` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `last_modified` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`loyalty_campaign_id`, `branch_id`),
    CONSTRAINT `FK_loyalty_campaign_branch_campaign` FOREIGN KEY (`loyalty_campaign_id`) REFERENCES `loyalty_campaign` (`loyalty_campaign_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_loyalty_campaign_branch_branch` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 11. Seed default system setting for cross-branch lookup if not exists
INSERT IGNORE INTO `system_setting` (`setting_key`, `setting_value`, `description`, `last_modified`)
VALUES ('Inventory.AllowCrossBranchLookup', 'true', 'Allows cashiers and staff to view inventory levels at other store branches', NOW(6));

-- 12. Seed branch_item_stock for existing items into the first active branch
INSERT IGNORE INTO `branch_item_stock` (`branch_id`, `item_id`, `in_stock`, `reorder_level`, `date_created`, `last_modified`)
SELECT 
    (SELECT `branch_id` FROM `branch` ORDER BY `branch_id` ASC LIMIT 1) AS `branch_id`,
    i.`item_id`,
    i.`in_stock`,
    i.`reorder_level`,
    NOW(6),
    NOW(6)
FROM `item` i
WHERE (SELECT `branch_id` FROM `branch` ORDER BY `branch_id` ASC LIMIT 1) IS NOT NULL;
