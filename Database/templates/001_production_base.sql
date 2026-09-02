-- =========================================================================
-- StoreProject Testing Baseline Database Template (from live store_db_v2)
-- Contains full test schema, products, categories, batches, and test data.
-- Note: The clean production template is preserved in 001_production_base.clean.sql
-- =========================================================================

SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO';
SET FOREIGN_KEY_CHECKS=0;

-- MariaDB dump 10.19  Distrib 10.4.32-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: store_db_v2
-- ------------------------------------------------------
-- Server version	10.4.32-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` VALUES ('20260421153358_InitialCreate','8.0.4'),('20260428121019_SnakeCaseNaming','8.0.4'),('20260502174832_OperationsModules','8.0.4'),('20260503180447_MobileMoneyTransactions','8.0.4'),('20260503200642_BranchSupport','8.0.4'),('20260503215059_LoyaltyModule','8.0.4'),('20260503221224_InvoiceBranchId','8.0.4'),('20260504012628_AddCustomerSegmentAndLoyaltyCampaign','8.0.4'),('20260504032440_AddDiscountExtensionsAndStockTransfers','8.0.4'),('20260504093741_AddWastageAndDiscountOverrides','8.0.4'),('20260504152256_AddPurchaseOrdersAndCashVariance','8.0.4'),('20260725201610_ImageProcessingUpdate','8.0.4'),('20260810095727_AddFidoCredentials','8.0.4'),('20260810181907_AddPasswordRecovery','8.0.4'),('20260811093154_Phase2TempPasswordExpiry','8.0.4'),('20260811130919_AddUserLockoutFields','8.0.4'),('20260811142814_AddSystemSettings','8.0.4'),('20260811152241_AddAuditLogAnd2FA','8.0.4'),('20260811153859_AddSecurityStamp','8.0.4'),('20260812143244_ContactChangeRequests','8.0.4'),('20260812230502_UpdateContactChangeStatusToString','8.0.4'),('20260817130947_SupportMultipleUserSessions','8.0.4');

--
-- Table structure for table `audit_log`
--

DROP TABLE IF EXISTS `audit_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `audit_log` (
  `audit_log_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `action` longtext NOT NULL,
  `details` longtext DEFAULT NULL,
  `ip_address` longtext DEFAULT NULL,
  `user_agent` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`audit_log_id`),
  KEY `ix_audit_log_user_id` (`user_id`),
  CONSTRAINT `fk_audit_log_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_log`
--

INSERT INTO `audit_log` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_audit_log_action',NULL,NULL,NULL,'2026-08-11 15:56:00.529342','2026-08-11 15:56:00.520699'),(2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2FA Enabled','Two-factor authentication was successfully enabled.',NULL,NULL,'2026-08-12 08:34:18.097509','2026-08-12 08:34:18.089964'),(3,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Security','Disabled Two-Factor Authentication',NULL,NULL,'2026-08-12 09:00:26.436468','2026-08-12 09:00:26.434922');

--
-- Table structure for table `batch`
--

DROP TABLE IF EXISTS `batch`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `batch` (
  `batch_id` char(36) NOT NULL,
  `item_id` char(36) NOT NULL,
  `batch_number` varchar(100) NOT NULL,
  `quantity` int(11) NOT NULL,
  `cost_price` decimal(18,4) NOT NULL,
  `received_date` datetime(6) NOT NULL,
  `expiry_date` datetime(6) DEFAULT NULL,
  `notes` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`batch_id`),
  KEY `ix_batch_item_id` (`item_id`),
  CONSTRAINT `fk_batch_items_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `batch`
--

INSERT INTO `batch` VALUES ('314a08cb-d875-4f73-a64b-0af7cc43bac2','08dea521-ced8-464b-86c9-b8499dcfca2e','seed_batch_batch_number',1,1.0000,'2026-04-28 12:29:39.566838',NULL,NULL,'2026-04-28 12:29:39.609714','2026-04-28 12:29:39.608191');

--
-- Table structure for table `branch`
--

DROP TABLE IF EXISTS `branch`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `branch` (
  `branch_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` longtext NOT NULL,
  `code` longtext NOT NULL,
  `address` longtext DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`branch_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `branch`
--

INSERT INTO `branch` VALUES (2,'Main Branch (HQ)','HQ','1 Store Avenue, City Centre',1,'2026-05-03 22:42:02.635932','2026-05-03 22:42:02.634655'),(3,'BBM Maroq','HJ-234','Bonapriso',1,'2026-07-09 20:08:15.467063','2026-07-09 20:08:15.465078');

--
-- Table structure for table `bundle_rule`
--

DROP TABLE IF EXISTS `bundle_rule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `bundle_rule` (
  `bundle_rule_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` longtext NOT NULL,
  `trigger_item_id` char(36) NOT NULL,
  `reward_item_id` char(36) NOT NULL,
  `trigger_quantity` int(11) NOT NULL,
  `reward_quantity` int(11) NOT NULL,
  `reward_discount_percent` decimal(65,30) NOT NULL,
  `valid_from` datetime(6) DEFAULT NULL,
  `valid_to` datetime(6) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`bundle_rule_id`),
  KEY `ix_bundle_rule_reward_item_id` (`reward_item_id`),
  KEY `ix_bundle_rule_trigger_item_id` (`trigger_item_id`),
  CONSTRAINT `fk_bundle_rule_items_reward_item_id` FOREIGN KEY (`reward_item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_bundle_rule_items_trigger_item_id` FOREIGN KEY (`trigger_item_id`) REFERENCES `item` (`item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bundle_rule`
--

INSERT INTO `bundle_rule` VALUES (1,'Seed BundleRule','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-ced8-464b-86c9-b8499dcfca2e',1,1,1.000000000000000000000000000000,NULL,NULL,1,'2026-05-02 17:51:08.172274','2026-05-02 17:51:08.170858');

--
-- Table structure for table `cash_variance_record`
--

DROP TABLE IF EXISTS `cash_variance_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `cash_variance_record` (
  `cash_variance_record_id` int(11) NOT NULL AUTO_INCREMENT,
  `cashier_shift_id` char(36) NOT NULL,
  `expected_amount` decimal(65,30) NOT NULL,
  `actual_amount` decimal(65,30) NOT NULL,
  `reason_code` varchar(100) DEFAULT NULL,
  `notes` varchar(2000) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `recorded_by_user_id` char(36) NOT NULL,
  `reviewed_by_user_id` char(36) DEFAULT NULL,
  `review_notes` varchar(2000) DEFAULT NULL,
  `reviewed_at` datetime(6) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`cash_variance_record_id`),
  KEY `ix_cash_variance_record_cashier_shift_id` (`cashier_shift_id`),
  KEY `ix_cash_variance_record_recorded_by_user_id` (`recorded_by_user_id`),
  KEY `ix_cash_variance_record_reviewed_by_user_id` (`reviewed_by_user_id`),
  KEY `ix_cash_variance_record_status_date_created` (`status`,`date_created`),
  CONSTRAINT `fk_cash_variance_record_cashier_shifts_cashier_shift_id` FOREIGN KEY (`cashier_shift_id`) REFERENCES `cashier_shift` (`cashier_shift_id`),
  CONSTRAINT `fk_cash_variance_record_users_recorded_by_user_id` FOREIGN KEY (`recorded_by_user_id`) REFERENCES `user` (`user_id`),
  CONSTRAINT `fk_cash_variance_record_users_reviewed_by_user_id` FOREIGN KEY (`reviewed_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cash_variance_record`
--

INSERT INTO `cash_variance_record` VALUES (1,'2a5b12ee-7529-4ab6-91b0-7a1fef150b40',1.000000000000000000000000000000,1.000000000000000000000000000000,NULL,NULL,2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','jdsklgjklsdjgklj s','2026-08-03 15:37:17.823048','2026-05-07 05:40:13.865404','2026-08-03 15:37:17.830563');

--
-- Table structure for table `cashier_shift`
--

DROP TABLE IF EXISTS `cashier_shift`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `cashier_shift` (
  `cashier_shift_id` char(36) NOT NULL,
  `opened_by_user_id` char(36) NOT NULL,
  `closed_by_user_id` char(36) DEFAULT NULL,
  `opened_at_utc` datetime(6) NOT NULL,
  `closed_at_utc` datetime(6) DEFAULT NULL,
  `opening_float` decimal(65,30) NOT NULL,
  `closing_float` decimal(65,30) DEFAULT NULL,
  `expected_closing_amount` decimal(65,30) DEFAULT NULL,
  `variance_amount` decimal(65,30) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `notes` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`cashier_shift_id`),
  KEY `ix_cashier_shift_closed_by_user_id` (`closed_by_user_id`),
  KEY `ix_cashier_shift_opened_by_user_id` (`opened_by_user_id`),
  CONSTRAINT `fk_cashier_shift_users_closed_by_user_id` FOREIGN KEY (`closed_by_user_id`) REFERENCES `user` (`user_id`),
  CONSTRAINT `fk_cashier_shift_users_opened_by_user_id` FOREIGN KEY (`opened_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cashier_shift`
--

INSERT INTO `cashier_shift` VALUES ('2a5b12ee-7529-4ab6-91b0-7a1fef150b40','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-05-02 17:51:08.979960','2026-07-25 19:04:41.788013',1.000000000000000000000000000000,2354350634563564.000000000000000000000000000000,51401.000000000000000000000000000000,2354350634512163.000000000000000000000000000000,1,'sdgsdgsdfg','2026-05-02 17:51:09.050315','2026-07-25 19:04:41.792926'),('9e623bd3-1da8-4824-9115-e26fcc74a278','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,'2026-08-04 15:56:48.789659',NULL,0.000000000000000000000000000000,NULL,NULL,NULL,0,NULL,'2026-08-04 15:56:48.907809','2026-08-04 15:56:48.906836');

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `category` (
  `category_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(150) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`category_id`),
  UNIQUE KEY `ix_category_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

INSERT INTO `category` VALUES (1,'Beverges','Beverages','/files/legacy/planet-1l.png','2026-04-28 12:29:34.828859','2026-04-28 12:29:34.819371',NULL),(2,'Groceries','General groceries','/files/legacy/parle-g.png','2026-04-28 12:29:34.829737','2026-04-28 12:29:34.829724',NULL),(3,'Alcohols','Alcoholic drinks','/files/categories/thumb/5b719495-bf41-4026-8ef2-d7f697d79af2.webp','2026-04-28 12:29:34.829751','2026-07-26 11:27:54.466680','/files/categories/full/0bcfde36-7eb1-4213-a577-19d932e7144f.webp'),(4,'Vegitebles','Vegetables','/files/categories/thumb/ff7173a4-7f9d-4c34-bc2f-e6d8eb70ded1.webp','2026-04-28 12:29:34.829764','2026-07-26 13:45:06.328256','/files/categories/full/2e6e9dc6-fced-469b-ad12-d91e8f13efdf.webp'),(5,'Fish','Fish and seafood','/files/categories/thumb/7b308b6d-4a7f-48ca-91a8-f9cb07f974c8.webp','2026-04-28 12:29:34.829777','2026-07-26 13:45:29.871071','/files/categories/full/ed1ebfbe-16a6-419b-86e0-f25dd13dc569.webp'),(6,'Meat','Meat products','/files/categories/thumb/01f8136a-52da-47ad-af8e-c7c4323c785c.webp','2026-04-28 12:29:34.829825','2026-07-26 13:45:57.022424','/files/categories/full/82a0e04c-b4ab-40c1-88c4-d8c93dff19f6.webp'),(7,'Detegents','Cleaning products','/files/legacy/omo-s.png','2026-04-28 12:29:34.829839','2026-04-28 12:29:34.829833',NULL);

--
-- Table structure for table `change_log`
--

DROP TABLE IF EXISTS `change_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `change_log` (
  `change_log_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `entity_name` varchar(100) NOT NULL,
  `entity_id` varchar(100) NOT NULL,
  `action` varchar(20) NOT NULL,
  `old_values` longtext DEFAULT NULL,
  `new_values` longtext DEFAULT NULL,
  `ip_address` varchar(50) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`change_log_id`),
  KEY `ix_change_log_user_id` (`user_id`),
  CONSTRAINT `fk_change_log_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `change_log`
--

INSERT INTO `change_log` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Seed ChangeLog','seed_change_log_entity_id','Created',NULL,NULL,NULL,'2026-04-28 12:29:40.705532','2026-04-28 12:29:40.704471'),(2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Item','08dea521-cef0-4c39-804d-2fbb09083b8c','Updated',NULL,'Stock adjustment +45. Before=50, After=95, Reason=Too much',NULL,'2026-07-09 17:28:14.968555','2026-07-09 17:28:14.968083'),(3,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Item','08dea521-cef0-4c39-804d-2fbb09083b8c','Updated',NULL,'Stock return +330. Before=95, After=425',NULL,'2026-07-09 17:28:40.379195','2026-07-09 17:28:40.379184');

--
-- Table structure for table `city`
--

DROP TABLE IF EXISTS `city`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `city` (
  `city_id` int(11) NOT NULL AUTO_INCREMENT,
  `region_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`city_id`),
  KEY `ix_city_region_id` (`region_id`),
  CONSTRAINT `fk_city_regions_region_id` FOREIGN KEY (`region_id`) REFERENCES `region` (`region_id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `city`
--

INSERT INTO `city` VALUES (1,1,'Seed City','2026-04-28 12:29:40.629081','2026-04-28 12:29:40.627496'),(6,2,'Douala','2026-08-31 22:27:08.869362','2026-08-31 22:27:08.868533');

--
-- Table structure for table `contact_change_request`
--

DROP TABLE IF EXISTS `contact_change_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `contact_change_request` (
  `id` char(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `new_email` longtext DEFAULT NULL,
  `new_phone` longtext DEFAULT NULL,
  `verification_token` longtext DEFAULT NULL,
  `status` varchar(50) NOT NULL,
  `verified_at` datetime(6) DEFAULT NULL,
  `approved_at` datetime(6) DEFAULT NULL,
  `approved_by_id` char(36) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_contact_change_request_approved_by_id` (`approved_by_id`),
  KEY `ix_contact_change_request_user_id` (`user_id`),
  CONSTRAINT `fk_contact_change_request_users_approved_by_id` FOREIGN KEY (`approved_by_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_contact_change_request_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contact_change_request`
--

INSERT INTO `contact_change_request` VALUES ('1066c3bc-d5c2-4e91-b997-2520ecf3958c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','alice.admin@store.com','678787879','84fc0122cd4f4610b62ec610069e692c','PendingVerification',NULL,NULL,NULL,'2026-08-12 23:49:04.076789','2026-08-12 23:49:04.076713'),('856d8355-70df-416d-9180-fe65bade8249','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_email_address@store.com','678787878','5f52b790a2c54ceebf9adeb8cbe9626f','Cancelled',NULL,NULL,NULL,'2026-08-12 22:20:07.425298','2026-08-12 23:45:13.671525'),('9fb09492-0536-48ac-adf0-ba5de6dff37e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','alice.admin@store.com','678787878','a3574f26c75743d4b0ed6808f21910ad','Approved',NULL,'2026-08-12 23:48:25.929019','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-08-12 23:46:21.823396','2026-08-12 23:48:26.016124');

--
-- Table structure for table `country`
--

DROP TABLE IF EXISTS `country`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `country` (
  `country_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `iso_code` varchar(3) DEFAULT NULL,
  `phone_code` varchar(10) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`country_id`),
  UNIQUE KEY `ix_country_iso_code` (`iso_code`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `country`
--

INSERT INTO `country` VALUES (1,'Seed Country',NULL,NULL,'2026-04-28 12:29:38.148501','2026-04-28 12:29:38.147105'),(2,'Default',NULL,NULL,'2026-08-31 22:27:08.652684','2026-08-31 22:27:08.643792');

--
-- Table structure for table `currency`
--

DROP TABLE IF EXISTS `currency`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `currency` (
  `currency_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` longtext NOT NULL,
  `code` longtext NOT NULL,
  `symbol` longtext NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`currency_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `currency`
--

INSERT INTO `currency` VALUES (1,'Seed Currency','SEED-CODE','seed_currency_symbol','2026-04-28 12:29:38.197136','2026-04-28 12:29:38.195911');

--
-- Table structure for table `customer`
--

DROP TABLE IF EXISTS `customer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer` (
  `customer_id` char(36) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `middle_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) NOT NULL,
  `gender` varchar(20) NOT NULL,
  `date_of_birth` datetime(6) DEFAULT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `segment` int(11) NOT NULL DEFAULT 0,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`customer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer`
--

INSERT INTO `customer` VALUES ('04224c2d-9499-4f49-afdd-76e40f9785dc','Rhoda','Bei','Kah','Female',NULL,NULL,'/files/customers/thumb/2bd7db2c-b840-4fae-b4a9-25f3f5009389.webp','2026-08-05 08:05:34.782891','2026-08-05 08:05:34.781234',2,'/files/customers/full/e657ae33-5f49-4951-af2a-f892452f264b.webp'),('1f7d24b1-14de-42a2-afde-a96f9cbc124c','Customer',NULL,'Customer','Male',NULL,NULL,NULL,'2026-04-28 12:29:38.294517','2026-05-03 14:23:17.796575',0,NULL),('49e6416a-fd21-41c2-a8ff-fe5730f32e14','Bill','J.','Rhanda','Male',NULL,NULL,'/files/customers/d3ff6b1a-ac09-4ae8-836b-07453cde15dc.png','2026-05-03 14:24:58.386334','2026-08-04 13:49:52.544234',0,NULL),('83927f4e-c47b-4856-8895-a214c3613671','James','D.','Bond','Male',NULL,'Bond Special','/files/customers/41c37402-a4da-4417-b77a-f31d80ed8f84.png','2026-07-24 10:03:19.077770','2026-07-24 13:25:43.366590',2,NULL);

--
-- Table structure for table `customer_email`
--

DROP TABLE IF EXISTS `customer_email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_email` (
  `customer_email_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` char(36) NOT NULL,
  `email_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_email_id`),
  KEY `ix_customer_email_email_id` (`email_id`),
  KEY `ix_customer_email_customer_id` (`customer_id`),
  CONSTRAINT `fk_customer_email_customers_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_customer_email_emails_email_id` FOREIGN KEY (`email_id`) REFERENCES `email` (`email_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_email`
--

INSERT INTO `customer_email` VALUES (1,'1f7d24b1-14de-42a2-afde-a96f9cbc124c',1,1,'2026-04-28 12:29:39.319425','2026-04-28 12:29:39.318223'),(2,'49e6416a-fd21-41c2-a8ff-fe5730f32e14',2,1,'2026-08-04 13:49:52.542988','2026-08-04 13:49:52.541797'),(3,'04224c2d-9499-4f49-afdd-76e40f9785dc',3,1,'2026-08-05 08:05:34.784760','2026-08-05 08:05:34.783624');

--
-- Table structure for table `customer_location`
--

DROP TABLE IF EXISTS `customer_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_location` (
  `customer_location_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` char(36) NOT NULL,
  `location_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_location_id`),
  KEY `ix_customer_location_location_id` (`location_id`),
  KEY `ix_customer_location_customer_id` (`customer_id`),
  CONSTRAINT `fk_customer_location_customer_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_customer_location_locations_location_id` FOREIGN KEY (`location_id`) REFERENCES `location` (`location_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_location`
--

INSERT INTO `customer_location` VALUES (1,'1f7d24b1-14de-42a2-afde-a96f9cbc124c',1,1,'2026-04-28 12:29:39.976879','2026-04-28 12:29:39.975996');

--
-- Table structure for table `customer_loyalty_account`
--

DROP TABLE IF EXISTS `customer_loyalty_account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_loyalty_account` (
  `loyalty_account_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` char(36) NOT NULL,
  `points` int(11) NOT NULL,
  `tier` int(11) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`loyalty_account_id`),
  UNIQUE KEY `ix_customer_loyalty_account_customer_id` (`customer_id`),
  CONSTRAINT `fk_customer_loyalty_account_customer_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_loyalty_account`
--

INSERT INTO `customer_loyalty_account` VALUES (1,'1f7d24b1-14de-42a2-afde-a96f9cbc124c',201,0,'2026-05-03 22:01:23.351498','2026-08-28 16:29:14.804540'),(2,'83927f4e-c47b-4856-8895-a214c3613671',36956,2,'2026-08-04 13:59:34.800993','2026-08-04 23:39:10.483382'),(3,'04224c2d-9499-4f49-afdd-76e40f9785dc',10100,2,'2026-08-05 08:05:34.785622','2026-08-28 16:27:49.205085');

--
-- Table structure for table `customer_phone`
--

DROP TABLE IF EXISTS `customer_phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_phone` (
  `customer_phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` char(36) NOT NULL,
  `phone_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_phone_id`),
  KEY `ix_customer_phone_phone_id` (`phone_id`),
  KEY `ix_customer_phone_customer_id` (`customer_id`),
  CONSTRAINT `fk_customer_phone_customers_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_customer_phone_phones_phone_id` FOREIGN KEY (`phone_id`) REFERENCES `phone` (`phone_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_phone`
--

INSERT INTO `customer_phone` VALUES (1,'1f7d24b1-14de-42a2-afde-a96f9cbc124c',1,1,'2026-04-28 12:29:39.374024','2026-04-28 12:29:39.372764'),(2,'49e6416a-fd21-41c2-a8ff-fe5730f32e14',2,1,'2026-08-04 13:49:52.501492','2026-08-04 13:49:52.500570'),(3,'04224c2d-9499-4f49-afdd-76e40f9785dc',3,1,'2026-08-05 08:05:34.787180','2026-08-05 08:05:34.786564');

--
-- Table structure for table `customer_privilege`
--

DROP TABLE IF EXISTS `customer_privilege`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_privilege` (
  `customer_privilege_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` char(36) NOT NULL,
  `privilege_id` int(11) NOT NULL,
  `type` int(11) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_privilege_id`),
  KEY `ix_customer_privilege_privilege_id` (`privilege_id`),
  KEY `ix_customer_privilege_customer_id` (`customer_id`),
  CONSTRAINT `fk_customer_privilege_customer_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_customer_privilege_privileges_privilege_id` FOREIGN KEY (`privilege_id`) REFERENCES `privilege` (`privilege_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_privilege`
--

INSERT INTO `customer_privilege` VALUES (1,'1f7d24b1-14de-42a2-afde-a96f9cbc124c',1,0,1,'2026-04-28 12:29:40.314346','2026-04-28 12:29:40.313051');

--
-- Table structure for table `customer_privilege_action`
--

DROP TABLE IF EXISTS `customer_privilege_action`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_privilege_action` (
  `customer_privilege_action_id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_privilege_id` int(11) NOT NULL,
  `performed_by_user_id` char(36) NOT NULL,
  `action` longtext NOT NULL,
  `notes` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_privilege_action_id`),
  KEY `ix_customer_privilege_action_performed_by_user_id` (`performed_by_user_id`),
  KEY `ix_customer_privilege_action_customer_privilege_id` (`customer_privilege_id`),
  CONSTRAINT `fk_customer_privilege_action_customer_privilege_customer_privil~` FOREIGN KEY (`customer_privilege_id`) REFERENCES `customer_privilege` (`customer_privilege_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_customer_privilege_action_users_performed_by_user_id` FOREIGN KEY (`performed_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_privilege_action`
--

INSERT INTO `customer_privilege_action` VALUES (1,1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_customer_privilege_action_action',NULL,'2026-04-28 12:29:40.871606','2026-04-28 12:29:40.870733');

--
-- Table structure for table `customer_segment_price`
--

DROP TABLE IF EXISTS `customer_segment_price`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_segment_price` (
  `customer_segment_price_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `segment` int(11) NOT NULL,
  `price_override` decimal(65,30) NOT NULL,
  `valid_from` datetime(6) DEFAULT NULL,
  `valid_to` datetime(6) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`customer_segment_price_id`),
  KEY `ix_customer_segment_price_item_id_segment_is_active` (`item_id`,`segment`,`is_active`),
  CONSTRAINT `fk_customer_segment_price_items_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer_segment_price`
--

INSERT INTO `customer_segment_price` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e',0,1.000000000000000000000000000000,NULL,NULL,1,'2026-05-02 17:51:08.270711','2026-05-02 17:51:08.268543');

--
-- Table structure for table `department`
--

DROP TABLE IF EXISTS `department`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `department` (
  `department_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(150) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`department_id`),
  UNIQUE KEY `ix_department_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `department`
--

INSERT INTO `department` VALUES (1,'Seed Department',NULL,'2026-04-28 12:29:38.341099','2026-04-28 12:29:38.339731'),(2,'Management','Management department','2026-05-03 22:42:02.538599','2026-05-03 22:42:02.538582'),(3,'Sales','Sales department','2026-05-03 22:42:02.538562','2026-05-03 22:42:02.538540'),(4,'Operations','Operations department','2026-05-03 22:42:02.537890','2026-05-03 22:42:02.536963');

--
-- Table structure for table `discount`
--

DROP TABLE IF EXISTS `discount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `discount` (
  `discount_id` int(11) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) DEFAULT NULL,
  `managed_by_user_id` char(36) DEFAULT NULL,
  `name` varchar(150) NOT NULL,
  `percentage` decimal(5,2) NOT NULL,
  `valid_from` datetime(6) DEFAULT NULL,
  `valid_to` datetime(6) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `category_id` int(11) DEFAULT NULL,
  `coupon_code` varchar(50) DEFAULT NULL,
  `discount_type` int(11) NOT NULL DEFAULT 0,
  `fixed_amount` decimal(65,30) DEFAULT NULL,
  `max_uses` int(11) DEFAULT NULL,
  `min_quantity` int(11) NOT NULL DEFAULT 0,
  `target_segment` int(11) DEFAULT NULL,
  `used_count` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`discount_id`),
  UNIQUE KEY `ix_discount_item_id` (`item_id`),
  UNIQUE KEY `ix_discount_coupon_code` (`coupon_code`),
  KEY `ix_discount_managed_by_user_id` (`managed_by_user_id`),
  KEY `ix_discount_category_id` (`category_id`),
  KEY `ix_discount_is_active_valid_from_valid_to` (`is_active`,`valid_from`,`valid_to`),
  CONSTRAINT `fk_discount_category_category_id` FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`),
  CONSTRAINT `fk_discount_items_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_discount_users_managed_by_user_id` FOREIGN KEY (`managed_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `discount`
--

INSERT INTO `discount` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Seed Discount',1.00,NULL,NULL,1,'2026-04-28 12:29:38.462584','2026-04-28 12:29:38.461084',NULL,NULL,0,NULL,NULL,0,NULL,0),(2,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','New special',34.00,'2026-07-09 21:11:00.000000','2026-07-25 21:11:00.000000',1,'2026-07-09 20:11:27.609491','2026-07-09 20:11:27.608086',NULL,'034895WPOE',0,NULL,10,6,NULL,0);

--
-- Table structure for table `discount_override_request`
--

DROP TABLE IF EXISTS `discount_override_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `discount_override_request` (
  `discount_override_request_id` int(11) NOT NULL AUTO_INCREMENT,
  `invoice_id` char(36) DEFAULT NULL,
  `item_id` char(36) DEFAULT NULL,
  `override_type` int(11) NOT NULL,
  `override_value` decimal(65,30) NOT NULL,
  `justification` varchar(1000) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `requested_by_user_id` char(36) NOT NULL,
  `reviewed_by_user_id` char(36) DEFAULT NULL,
  `review_notes` varchar(1000) DEFAULT NULL,
  `reviewed_at` datetime(6) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`discount_override_request_id`),
  KEY `ix_discount_override_request_invoice_id` (`invoice_id`),
  KEY `ix_discount_override_request_item_id` (`item_id`),
  KEY `ix_discount_override_request_requested_by_user_id` (`requested_by_user_id`),
  KEY `ix_discount_override_request_reviewed_by_user_id` (`reviewed_by_user_id`),
  KEY `ix_discount_override_request_status_date_created` (`status`,`date_created`),
  CONSTRAINT `fk_discount_override_request_invoices_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`),
  CONSTRAINT `fk_discount_override_request_items_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_discount_override_request_users_requested_by_user_id` FOREIGN KEY (`requested_by_user_id`) REFERENCES `user` (`user_id`),
  CONSTRAINT `fk_discount_override_request_users_reviewed_by_user_id` FOREIGN KEY (`reviewed_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `discount_override_request`
--

INSERT INTO `discount_override_request` VALUES (1,'705c66a3-1fac-497e-ade9-d037550cd9d8','08dea521-ced8-464b-86c9-b8499dcfca2e',0,1.000000000000000000000000000000,NULL,0,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,'2026-05-04 11:39:30.362801','2026-05-04 11:39:30.357820'),(2,NULL,'08dea521-cef0-4b84-83a7-348648a9aa1d',0,100.000000000000000000000000000000,'More discount',3,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,NULL,'2026-08-28 07:54:29.232475','2026-08-28 07:55:05.701091');

--
-- Table structure for table `document`
--

DROP TABLE IF EXISTS `document`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `document` (
  `document_id` int(11) NOT NULL AUTO_INCREMENT,
  `entity_name` longtext DEFAULT NULL,
  `entity_id` longtext DEFAULT NULL,
  `file_name` longtext NOT NULL,
  `file_path` longtext NOT NULL,
  `mime_type` longtext DEFAULT NULL,
  `file_size_bytes` bigint(20) DEFAULT NULL,
  `uploaded_by_user_id` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`document_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `document`
--

INSERT INTO `document` VALUES (1,NULL,NULL,'Seed Document','seed_document_file_path',NULL,NULL,NULL,'2026-04-28 12:29:38.528993','2026-04-28 12:29:38.528017');

--
-- Table structure for table `email`
--

DROP TABLE IF EXISTS `email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `email` (
  `email_id` int(11) NOT NULL AUTO_INCREMENT,
  `address` varchar(254) NOT NULL,
  `type` varchar(20) NOT NULL,
  `is_verified` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`email_id`),
  UNIQUE KEY `ix_email_address` (`address`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `email`
--

INSERT INTO `email` VALUES (1,'alice.admin@store.com','Personal',1,'2026-04-28 12:29:38.012171','2026-08-12 23:48:26.016137'),(2,'bill@rhandacorps.com','Personal',0,'2026-08-04 13:49:52.500295','2026-08-04 13:49:52.499694'),(3,'kah.rhoda@gmail.com','Personal',0,'2026-08-05 08:05:34.535027','2026-08-05 08:05:34.534144'),(4,'main@manesupplies.com','Work',0,'2026-08-31 22:27:08.393854','2026-08-31 22:27:08.393135');

--
-- Table structure for table `employee`
--

DROP TABLE IF EXISTS `employee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee` (
  `employee_id` char(36) NOT NULL,
  `department_id` int(11) DEFAULT NULL,
  `salary_id` int(11) DEFAULT NULL,
  `first_name` varchar(100) NOT NULL,
  `middle_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) NOT NULL,
  `nid_number` varchar(50) DEFAULT NULL,
  `gender` varchar(20) NOT NULL DEFAULT 'NotSpecified',
  `date_of_birth` datetime(6) DEFAULT NULL,
  `place_of_birth` varchar(200) DEFAULT NULL,
  `date_employed` datetime(6) NOT NULL,
  `status` varchar(50) NOT NULL DEFAULT 'Pending',
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`employee_id`),
  KEY `ix_employee_salary_id` (`salary_id`),
  KEY `ix_employee_department_id` (`department_id`),
  CONSTRAINT `fk_employee_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`department_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_employee_salaries_salary_id` FOREIGN KEY (`salary_id`) REFERENCES `salary` (`salary_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee`
--

INSERT INTO `employee` VALUES ('08dea965-5112-49dd-8600-fa31b090bf9f',3,1,'Mike',NULL,'Manager',NULL,'Male',NULL,NULL,'2026-05-03 22:42:54.978646','Active','img/user_default.png','2026-05-03 22:42:55.320254','2026-07-26 13:25:30.945201',NULL),('08dea965-529c-424e-8b22-54203048b90a',4,NULL,'Chris',NULL,'Cashier',NULL,'Male','1994-03-17 00:00:00.000000',NULL,'2026-05-03 22:42:57.593542','Active','img/user_default.png','2026-05-03 22:42:57.596332','2026-07-26 13:43:26.830798',NULL),('267ade5c-d834-4d31-8b3b-c05ddff5d6ed',3,NULL,'Jade',NULL,'Parker',NULL,'Female','2000-06-22 00:00:00.000000',NULL,'2026-07-23 00:00:00.000000','Active','/files/employees/e9732a6b-267d-474e-ada2-9a3f08c5a27c.png','2026-07-25 18:24:30.131418','2026-07-26 13:43:40.822879',NULL),('3d7c0852-cf8a-4c63-99fb-80b952983a20',1,NULL,'Seed Employee',NULL,'Seed Employee',NULL,'NotSpecified',NULL,NULL,'2026-04-28 12:29:38.560365','Pending',NULL,'2026-04-28 12:29:38.643775','2026-07-26 13:43:57.468593',NULL),('e4ed1796-4741-11f1-814d-c858c0c6a8bc',2,NULL,'Alice',NULL,'Admin',NULL,'Female',NULL,NULL,'2026-05-03 23:45:55.000000','Active','/files/employees/thumb/2315ed89-8cbe-4835-8a2e-209d1d1b59c3.webp','2026-07-24 15:32:11.000000','2026-07-26 13:42:23.712930','/files/employees/full/1abc7ad8-ac4e-4b1a-92dc-684bf658b68c.webp');

--
-- Table structure for table `employee_email`
--

DROP TABLE IF EXISTS `employee_email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee_email` (
  `employee_email_id` int(11) NOT NULL AUTO_INCREMENT,
  `employee_id` char(36) NOT NULL,
  `email_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`employee_email_id`),
  KEY `ix_employee_email_employee_id` (`employee_id`),
  KEY `ix_employee_email_email_id` (`email_id`),
  CONSTRAINT `fk_employee_email_email_email_id` FOREIGN KEY (`email_id`) REFERENCES `email` (`email_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_employee_email_employees_employee_id` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_email`
--

INSERT INTO `employee_email` VALUES (1,'3d7c0852-cf8a-4c63-99fb-80b952983a20',1,1,'2026-04-28 12:29:39.426198','2026-04-28 12:29:39.424972');

--
-- Table structure for table `employee_location`
--

DROP TABLE IF EXISTS `employee_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee_location` (
  `employee_location_id` int(11) NOT NULL AUTO_INCREMENT,
  `employee_id` char(36) NOT NULL,
  `location_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`employee_location_id`),
  KEY `ix_employee_location_location_id` (`location_id`),
  KEY `ix_employee_location_employee_id` (`employee_id`),
  CONSTRAINT `fk_employee_location_employee_employee_id` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_employee_location_locations_location_id` FOREIGN KEY (`location_id`) REFERENCES `location` (`location_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_location`
--

INSERT INTO `employee_location` VALUES (1,'3d7c0852-cf8a-4c63-99fb-80b952983a20',1,1,'2026-04-28 12:29:40.028495','2026-04-28 12:29:40.027727');

--
-- Table structure for table `employee_phone`
--

DROP TABLE IF EXISTS `employee_phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee_phone` (
  `employee_phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `employee_id` char(36) NOT NULL,
  `phone_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`employee_phone_id`),
  KEY `ix_employee_phone_phone_id` (`phone_id`),
  KEY `ix_employee_phone_employee_id` (`employee_id`),
  CONSTRAINT `fk_employee_phone_employees_employee_id` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_employee_phone_phones_phone_id` FOREIGN KEY (`phone_id`) REFERENCES `phone` (`phone_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_phone`
--

INSERT INTO `employee_phone` VALUES (1,'3d7c0852-cf8a-4c63-99fb-80b952983a20',1,1,'2026-04-28 12:29:39.476666','2026-04-28 12:29:39.475091');

--
-- Table structure for table `employee_privilege`
--

DROP TABLE IF EXISTS `employee_privilege`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee_privilege` (
  `employee_privilege_id` int(11) NOT NULL AUTO_INCREMENT,
  `employee_id` char(36) NOT NULL,
  `privilege_id` int(11) NOT NULL,
  `type` int(11) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`employee_privilege_id`),
  KEY `ix_employee_privilege_privilege_id` (`privilege_id`),
  KEY `ix_employee_privilege_employee_id` (`employee_id`),
  CONSTRAINT `fk_employee_privilege_employee_employee_id` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_employee_privilege_privileges_privilege_id` FOREIGN KEY (`privilege_id`) REFERENCES `privilege` (`privilege_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_privilege`
--

INSERT INTO `employee_privilege` VALUES (1,'3d7c0852-cf8a-4c63-99fb-80b952983a20',1,0,1,'2026-04-28 12:29:40.392367','2026-04-28 12:29:40.391015');

--
-- Table structure for table `employee_privilege_action`
--

DROP TABLE IF EXISTS `employee_privilege_action`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `employee_privilege_action` (
  `employee_privilege_action_id` int(11) NOT NULL AUTO_INCREMENT,
  `employee_privilege_id` int(11) NOT NULL,
  `performed_by_user_id` char(36) NOT NULL,
  `action` longtext NOT NULL,
  `notes` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`employee_privilege_action_id`),
  KEY `ix_employee_privilege_action_performed_by_user_id` (`performed_by_user_id`),
  KEY `ix_employee_privilege_action_employee_privilege_id` (`employee_privilege_id`),
  CONSTRAINT `fk_employee_privilege_action_employee_privilege_employee_privil~` FOREIGN KEY (`employee_privilege_id`) REFERENCES `employee_privilege` (`employee_privilege_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_employee_privilege_action_users_performed_by_user_id` FOREIGN KEY (`performed_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_privilege_action`
--

INSERT INTO `employee_privilege_action` VALUES (1,1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_employee_privilege_action_action',NULL,'2026-04-28 12:29:40.929004','2026-04-28 12:29:40.927619');

--
-- Table structure for table `fido_credential`
--

DROP TABLE IF EXISTS `fido_credential`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fido_credential` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `credential_id` longblob NOT NULL,
  `public_key` longblob NOT NULL,
  `user_handle` longblob NOT NULL,
  `signature_counter` int(10) unsigned NOT NULL,
  `aa_guid` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `cred_type` longtext NOT NULL,
  `reg_date` datetime(6) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_fido_credential_user_id` (`user_id`),
  CONSTRAINT `fk_fido_credential_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `fido_credential`
--

INSERT INTO `fido_credential` VALUES (4,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','��u�wNGB�������@','�& !X �Īw?�}WB����di��C�%?��:C�c�G�\"X �W''0�,)�k''MB���T^f���8���U�*�','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,'d3452668-01fd-4c12-926c-83a4204853aa','public-key','2026-08-12 14:17:22.406922','2026-08-12 14:17:22.431006','2026-08-15 15:55:12.619929');

--
-- Table structure for table `invoice`
--

DROP TABLE IF EXISTS `invoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `invoice` (
  `invoice_id` char(36) NOT NULL,
  `user_id` char(36) DEFAULT NULL,
  `customer_id` char(36) DEFAULT NULL,
  `total_amount` decimal(18,2) NOT NULL,
  `amount_tendered` decimal(18,2) NOT NULL,
  `change_given` decimal(18,2) NOT NULL,
  `payment_type` varchar(30) NOT NULL DEFAULT 'Cash',
  `is_paid` tinyint(1) NOT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `branch_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`invoice_id`),
  KEY `ix_invoice_user_id` (`user_id`),
  KEY `ix_invoice_customer_id` (`customer_id`),
  KEY `ix_invoice_branch_id` (`branch_id`),
  CONSTRAINT `fk_invoice_branch_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`),
  CONSTRAINT `fk_invoice_customer_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`customer_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_invoice_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoice`
--

INSERT INTO `invoice` VALUES ('27a976d6-4c27-4058-bbc5-c144099a5006','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','04224c2d-9499-4f49-afdd-76e40f9785dc',10500.00,20500.00,10000.00,'Cash',1,'You owe me 10,000 XAF','2026-08-12 13:00:03.598742','2026-08-12 13:00:03.596242',2),('47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','49e6416a-fd21-41c2-a8ff-fe5730f32e14',3075.00,5000.00,1925.00,'Cheque',1,'None','2026-08-31 18:32:44.314754','2026-08-31 18:32:44.314009',2),('66406176-c89f-4d2d-a650-4ddbffc9c525','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','83927f4e-c47b-4856-8895-a214c3613671',53539.00,56000.00,2461.00,'Cash',1,NULL,'2026-08-04 14:15:23.571352','2026-08-04 14:15:23.570973',NULL),('705c66a3-1fac-497e-ade9-d037550cd9d8','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','1f7d24b1-14de-42a2-afde-a96f9cbc124c',1.00,1.00,1.00,'Cash',1,NULL,'2026-04-28 12:29:38.745246','2026-04-28 12:29:38.744070',NULL),('8c30dbf5-dec0-42ec-af55-049f01f1a072','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,8800.00,10000.00,1200.00,'Cash',1,'New buy','2026-07-09 18:55:25.688253','2026-07-09 18:55:25.687781',NULL),('b340a5a7-3115-4f0f-9012-2faeb58696c9','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,2193.00,5000.00,2807.00,'BankTransfer',1,'jjhbhbb lb  k','2026-08-02 15:44:19.144617','2026-08-02 15:44:19.142750',NULL),('b9164763-1870-491c-9279-640a4a27cc06','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','49e6416a-fd21-41c2-a8ff-fe5730f32e14',25675.00,30000.00,4325.00,'MobileMoney',1,NULL,'2026-08-05 08:02:35.688624','2026-08-05 08:02:35.687703',2),('cbe79e8d-9682-4abe-927c-e26169e062b1','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,42600.00,50000.00,7400.00,'Cash',1,NULL,'2026-07-09 18:57:23.555669','2026-07-09 18:57:23.555663',NULL),('d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','04224c2d-9499-4f49-afdd-76e40f9785dc',13875.00,15000.00,1125.00,'BankTransfer',1,NULL,'2026-08-12 13:01:43.824518','2026-08-12 13:01:43.823786',2);

--
-- Table structure for table `invoice_tender`
--

DROP TABLE IF EXISTS `invoice_tender`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `invoice_tender` (
  `invoice_tender_id` int(11) NOT NULL AUTO_INCREMENT,
  `invoice_id` char(36) NOT NULL,
  `payment_type` varchar(30) NOT NULL,
  `amount` decimal(18,2) NOT NULL,
  `reference` varchar(200) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`invoice_tender_id`),
  KEY `ix_invoice_tender_invoice_id` (`invoice_id`),
  CONSTRAINT `fk_invoice_tender_invoice_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoice_tender`
--

INSERT INTO `invoice_tender` VALUES (1,'705c66a3-1fac-497e-ade9-d037550cd9d8','Cash',1.00,NULL,'2026-04-28 12:29:39.537606','2026-04-28 12:29:39.536561'),(2,'b9164763-1870-491c-9279-640a4a27cc06','MobileMoney',30000.00,NULL,'2026-08-05 08:02:35.691361','2026-08-05 08:02:35.690383'),(3,'27a976d6-4c27-4058-bbc5-c144099a5006','Cash',20500.00,NULL,'2026-08-12 13:00:03.600985','2026-08-12 13:00:03.600534'),(4,'d22653b7-a0a3-4d92-8801-349bd9909f08','BankTransfer',15000.00,NULL,'2026-08-12 13:01:43.824824','2026-08-12 13:01:43.824818'),(5,'47ca810c-3292-4550-b619-4d1c7985c409','Cheque',5000.00,NULL,'2026-08-31 18:32:44.316362','2026-08-31 18:32:44.315909');

--
-- Table structure for table `item`
--

DROP TABLE IF EXISTS `item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item` (
  `item_id` char(36) NOT NULL,
  `category_id` int(11) DEFAULT NULL,
  `unit_id` int(11) DEFAULT NULL,
  `manufacturer_id` char(36) DEFAULT NULL,
  `name` varchar(200) NOT NULL,
  `description` varchar(2000) DEFAULT NULL,
  `unit_price` decimal(18,4) NOT NULL,
  `cost_price` decimal(18,4) DEFAULT NULL,
  `in_stock` int(11) NOT NULL,
  `reorder_level` int(11) DEFAULT NULL,
  `type` varchar(20) NOT NULL DEFAULT 'Product',
  `barcode` varchar(100) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `tax_profile_id` int(11) DEFAULT NULL,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`item_id`),
  UNIQUE KEY `ix_item_barcode` (`barcode`),
  KEY `ix_item_unit_id` (`unit_id`),
  KEY `ix_item_manufacturer_id` (`manufacturer_id`),
  KEY `ix_item_category_id` (`category_id`),
  KEY `ix_item_tax_profile_id` (`tax_profile_id`),
  CONSTRAINT `fk_item_category_category_id` FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_item_manufacturers_manufacturer_id` FOREIGN KEY (`manufacturer_id`) REFERENCES `manufacturer` (`manufacturer_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_item_tax_profiles_tax_profile_id` FOREIGN KEY (`tax_profile_id`) REFERENCES `tax_profile` (`tax_profile_id`),
  CONSTRAINT `fk_item_units_unit_id` FOREIGN KEY (`unit_id`) REFERENCES `unit` (`unit_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item`
--

INSERT INTO `item` VALUES ('08dea521-ced8-464b-86c9-b8499dcfca2e',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Chin-chin','small',100.0000,70.0000,4028,5,'Product','SEED-CHI-50',1,'/files/legacy/chinchin-pkg.png','2026-04-28 12:29:35.818822','2026-08-31 18:32:44.317645',NULL,NULL),('08dea521-cef0-4041-89c8-61936bece7ad',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Chin-chin','medium',500.0000,350.0000,19,5,'Product','SEED-CHI-21',1,'/files/legacy/chinchin-p.png','2026-04-28 12:29:35.818804','2026-08-12 13:01:43.824860',NULL,NULL),('08dea521-cef0-4283-85ae-527de2916094',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Chin-chin','large',1000.0000,700.0000,5,5,'Product','SEED-CHI-5',0,'/files/legacy/peanuts-g.png','2026-04-28 12:29:35.818765','2026-05-03 14:02:33.250457',NULL,NULL),('08dea521-cef0-44cb-829b-e0b7ac49ee8b',2,1,'08dea521-cea7-4353-813b-4d2c0e7599f4','Dough-nuts','regular',100.0000,70.0000,50,5,'Product','SEED-DOU-44',1,'/files/legacy/doughnt.png','2026-04-28 12:29:35.818749','2026-08-31 22:31:22.617854',NULL,NULL),('08dea521-cef0-4658-80a1-c48e6a26b7a0',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Pea nuts','small',100.0000,70.0000,75,5,'Product','SEED-PEA-76',1,'/files/legacy/peanuts-fl.png','2026-04-28 12:29:35.818733','2026-08-31 18:32:44.317692',NULL,NULL),('08dea521-cef0-4a04-8f9e-d0ba087434cf',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Pea nuts','large',1300.0000,910.0000,0,5,'Product','SEED-PEA-10',1,'/files/legacy/peanuts-g.png','2026-04-28 12:29:35.818716','2026-08-04 14:15:23.572359',NULL,NULL),('08dea521-cef0-4add-87bc-de5cbcd4f390',2,1,'08dea521-cea7-4353-813b-4d2c0e7599f4','Eggs','retail',100.0000,70.0000,89,5,'Product','SEED-EGG-90',1,'/files/legacy/egg-cr.png','2026-04-28 12:29:35.817932','2026-08-31 18:32:44.317682',NULL,NULL),('08dea521-cef0-4b84-83a7-348648a9aa1d',2,3,'08dea521-cea7-4353-813b-4d2c0e7599f4','Eggs','tray',2200.0000,1540.0000,123,5,'Product','SEED-EGG-4',1,'/files/legacy/eggs-tray.png','2026-04-28 12:29:35.818838','2026-09-01 11:36:26.966158',NULL,NULL),('08dea521-cef0-4c39-804d-2fbb09083b8c',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Maggi','5-pieces',50.0000,35.0000,424,5,'Product','SEED-MAG-50',1,'/files/legacy/maggi-s-c.png','2026-04-28 12:29:35.818853','2026-08-05 08:02:35.692760',NULL,NULL),('08dea521-cef1-401a-8d61-b015149e112b',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Salt','small',50.0000,35.0000,97,5,'Product','SEED-SAL-100',1,'/files/legacy/salt.png','2026-04-28 12:29:35.818869','2026-08-31 18:32:44.317697',NULL,NULL),('08dea521-cef1-4145-89a0-93a853adbdaa',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Sugar','5-cube',25.0000,17.5000,472,5,'Product','SEED-SUG-300',1,'/files/legacy/sugar-c.png','2026-04-28 12:29:35.818885','2026-08-27 19:56:24.521521',NULL,NULL),('08dea521-cef1-4393-89bc-81ee43a577e9',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Parle G','small size',25.0000,17.5000,59,5,'Product','SEED-PAR-62',1,'/files/legacy/parle-g.png','2026-04-28 12:29:35.818900','2026-08-31 18:32:44.317688',NULL,NULL),('08dea521-cef1-4532-873e-152c73c450ea',1,5,'08dea521-cea7-4353-813b-4d2c0e7599f4','Planet','1.25l',500.0000,350.0000,6,5,'Product','SEED-PLA-12',1,'/files/legacy/planet-1l.png','2026-04-28 12:29:35.818928','2026-08-12 13:01:43.824909',NULL,NULL),('08dea521-cef1-47f3-8aa3-3af000328106',7,1,'08dea521-cea7-4353-813b-4d2c0e7599f4','Sponge','soft',50.0000,35.0000,48,5,'Product','SEED-SPO-13',1,'/files/legacy/sponge-1.png','2026-04-28 12:29:35.818943','2026-08-13 03:42:33.158782',NULL,NULL),('08dea521-cef1-48b7-8fed-1eb919645da8',7,1,'08dea521-cea7-4353-813b-4d2c0e7599f4','Sponge','small-strong',50.0000,35.0000,6,5,'Product','SEED-SPO-8',1,'/files/legacy/sponge-2.png','2026-04-28 12:29:35.818959','2026-08-12 13:01:43.824882',NULL,NULL),('08dea521-cef1-4e71-86ab-dd55bb82e6af',4,4,'08dea521-cea7-4353-813b-4d2c0e7599f4','Tomatoes','Sachet',100.0000,70.0000,383,5,'Product','SEED-TOM-42',1,'/files/legacy/tomat-ss.png','2026-04-28 12:29:35.818975','2026-08-31 22:31:22.617858',NULL,NULL),('08dea521-cef1-4f65-80d8-1f2950510353',2,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Spagetti','small-250',250.0000,175.0000,300,5,'Product','SEED-SPA-12',1,'/files/legacy/spagg-s.png','2026-04-28 12:29:35.818990','2026-08-31 22:31:22.616900',NULL,NULL),('08dea521-cef2-4023-8f03-be2047308897',7,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Omo','local',50.0000,35.0000,30,5,'Product','SEED-OMO-30',0,'/files/legacy/omo-s.png','2026-04-28 12:29:35.819005','2026-05-03 14:02:28.542583',NULL,NULL),('08dea521-cef2-433a-8356-21df728c32cb',1,2,'08dea521-cea7-4353-813b-4d2c0e7599f4','Top milk','small',50.0000,35.0000,8,5,'Product','SEED-TOP-8',0,'/files/legacy/top-milk.png','2026-04-28 12:29:35.819030','2026-07-09 17:10:26.199045',NULL,NULL),('4ea66295-b6c4-458e-ae58-83ae51670839',2,5,NULL,'Broli Milk','Can milk',1500.0000,1300.0000,119,10,'Product','34567890',1,'/files/items/4a5d4dd0-dcca-4206-9205-031638e688b1.jpg','2026-07-24 10:57:31.767748','2026-08-31 18:32:44.316657',NULL,NULL),('d77e5326-3f06-44c4-997b-a3eb2889f20c',2,2,NULL,'Rice','Just rice',1000.0000,800.0000,130,30,'Product','34567890sd',1,'/files/items/4a5d4dd0-dcca-4206-9205-031638e688b1.jpg','2026-07-24 13:23:31.196806','2026-08-31 18:32:44.317694',NULL,NULL),('dbeca20f-61cf-44fa-934e-7e76be480414',2,2,NULL,'Bread','Corn bread made with white maize',1000.0000,800.0000,208,25,'Product','8856976000016',1,'/files/items/4a5d4dd0-dcca-4206-9205-031638e688b1.jpg','2026-07-09 17:12:50.989122','2026-08-12 13:01:43.824845',NULL,NULL);

--
-- Table structure for table `item_category`
--

DROP TABLE IF EXISTS `item_category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_category` (
  `item_category_id` int(11) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `category_id` int(11) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`item_category_id`),
  KEY `ix_item_category_item_id` (`item_id`),
  KEY `ix_item_category_category_id` (`category_id`),
  CONSTRAINT `fk_item_category_category_category_id` FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_item_category_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_category`
--

INSERT INTO `item_category` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e',1,'2026-04-28 12:29:39.669187','2026-04-28 12:29:39.667854');

--
-- Table structure for table `item_code`
--

DROP TABLE IF EXISTS `item_code`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_code` (
  `item_code_id` int(11) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `code` varchar(100) NOT NULL,
  `code_type` varchar(50) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`item_code_id`),
  UNIQUE KEY `ix_item_code_code` (`code`),
  KEY `ix_item_code_item_id` (`item_id`),
  CONSTRAINT `fk_item_code_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_code`
--

INSERT INTO `item_code` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e','SEED-CODE','seed_item_code_code_type','2026-04-28 12:29:39.718757','2026-04-28 12:29:39.717430');

--
-- Table structure for table `item_expiry`
--

DROP TABLE IF EXISTS `item_expiry`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_expiry` (
  `item_expiry_id` int(11) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `expiry_date` datetime(6) NOT NULL,
  `days_warning_before` int(11) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`item_expiry_id`),
  UNIQUE KEY `ix_item_expiry_item_id` (`item_id`),
  CONSTRAINT `fk_item_expiry_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_expiry`
--

INSERT INTO `item_expiry` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e','2026-04-28 12:29:39.741688',NULL,'2026-04-28 12:29:39.767751','2026-04-28 12:29:39.766423');

--
-- Table structure for table `items_order`
--

DROP TABLE IF EXISTS `items_order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `items_order` (
  `items_order_id` char(36) NOT NULL,
  `supplier_id` char(36) DEFAULT NULL,
  `created_by_user_id` char(36) DEFAULT NULL,
  `order_number` varchar(100) NOT NULL,
  `status` varchar(30) NOT NULL DEFAULT 'Draft',
  `total_amount` decimal(18,2) NOT NULL,
  `expected_delivery_date` datetime(6) DEFAULT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`items_order_id`),
  UNIQUE KEY `ix_items_order_order_number` (`order_number`),
  KEY `ix_items_order_supplier_id` (`supplier_id`),
  KEY `ix_items_order_created_by_user_id` (`created_by_user_id`),
  CONSTRAINT `fk_items_order_suppliers_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `supplier` (`supplier_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_items_order_users_created_by_user_id` FOREIGN KEY (`created_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `items_order`
--

INSERT INTO `items_order` VALUES ('5a6647b8-301a-4ca8-9091-cb7f186958f3',NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','PO-20260709-F87869','Pending',2380.00,NULL,'New shipment','2026-07-09 17:21:19.114556','2026-07-09 17:21:19.113229'),('ca8cf8ff-3f53-4819-a6a5-5e1df9df9629',NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','PO-20260709-BEC5C7','Pending',654080.00,NULL,NULL,'2026-07-09 17:22:55.269479','2026-07-09 17:22:55.269458'),('f757f246-505c-467f-bc06-f09f7af7933c',NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','ORD-20260428122938','Draft',1.00,NULL,NULL,'2026-04-28 12:29:38.862191','2026-04-28 12:29:38.861265');

--
-- Table structure for table `language`
--

DROP TABLE IF EXISTS `language`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `language` (
  `language_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` longtext NOT NULL,
  `code` longtext NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`language_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `language`
--

INSERT INTO `language` VALUES (1,'Seed Language','SEED-CODE','2026-04-28 12:29:38.927309','2026-04-28 12:29:38.925843');

--
-- Table structure for table `location`
--

DROP TABLE IF EXISTS `location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `location` (
  `location_id` int(11) NOT NULL AUTO_INCREMENT,
  `city_id` int(11) DEFAULT NULL,
  `street_address` varchar(300) DEFAULT NULL,
  `postal_code` varchar(20) DEFAULT NULL,
  `latitude` varchar(20) DEFAULT NULL,
  `longitude` varchar(20) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`location_id`),
  KEY `ix_location_city_id` (`city_id`),
  CONSTRAINT `fk_location_city_city_id` FOREIGN KEY (`city_id`) REFERENCES `city` (`city_id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `location`
--

INSERT INTO `location` VALUES (1,NULL,NULL,NULL,NULL,NULL,'2026-04-28 12:29:39.016176','2026-04-28 12:29:39.014631'),(2,6,'Mboppi, 2PR8+V82',NULL,NULL,NULL,'2026-08-31 22:27:09.010043','2026-08-31 22:27:09.009336');

--
-- Table structure for table `loyalty_campaign`
--

DROP TABLE IF EXISTS `loyalty_campaign`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `loyalty_campaign` (
  `loyalty_campaign_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `description` varchar(1000) DEFAULT NULL,
  `campaign_type` int(11) NOT NULL,
  `target_segment` int(11) DEFAULT NULL,
  `multiplier_factor` decimal(65,30) NOT NULL,
  `bonus_points` int(11) NOT NULL,
  `start_date` datetime(6) NOT NULL,
  `end_date` datetime(6) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`loyalty_campaign_id`),
  KEY `ix_loyalty_campaign_is_active_start_date_end_date` (`is_active`,`start_date`,`end_date`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `loyalty_campaign`
--

INSERT INTO `loyalty_campaign` VALUES (1,'Black Friday','Campaign sales for Black Friday',0,NULL,1.000000000000000000000000000000,1,'2026-10-26 00:00:00.000000','2026-12-13 00:00:00.000000',1,'2026-05-04 02:58:22.139225','2026-08-13 00:08:11.608350'),(2,'Easter',NULL,1,0,1.000000000000000000000000000000,12,'2026-03-23 00:00:00.000000','2026-06-21 00:00:00.000000',1,'2026-05-04 03:04:18.744259','2026-08-17 12:26:00.124019');

--
-- Table structure for table `loyalty_transaction`
--

DROP TABLE IF EXISTS `loyalty_transaction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `loyalty_transaction` (
  `loyalty_transaction_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `loyalty_account_id` int(11) NOT NULL,
  `invoice_id` char(36) DEFAULT NULL,
  `points` int(11) NOT NULL,
  `transaction_type` int(11) NOT NULL,
  `note` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`loyalty_transaction_id`),
  KEY `ix_loyalty_transaction_invoice_id` (`invoice_id`),
  KEY `ix_loyalty_transaction_loyalty_account_id` (`loyalty_account_id`),
  CONSTRAINT `fk_loyalty_transaction_customer_loyalty_account_loyalty_account~` FOREIGN KEY (`loyalty_account_id`) REFERENCES `customer_loyalty_account` (`loyalty_account_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_loyalty_transaction_invoice_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `loyalty_transaction`
--

INSERT INTO `loyalty_transaction` VALUES (1,1,'705c66a3-1fac-497e-ade9-d037550cd9d8',1,0,NULL,'2026-05-03 22:01:25.553445','2026-05-03 22:01:25.552447'),(2,2,NULL,47,2,'Test','2026-08-04 13:59:34.991667','2026-08-04 13:59:34.985613'),(3,2,NULL,36909,0,'Purchase Reward Earned','2026-08-04 23:39:10.477313','2026-08-04 23:39:10.468067'),(4,3,NULL,100,0,'Purchase Reward Earned','2026-08-28 16:26:39.424780','2026-08-28 16:26:39.423751'),(5,3,NULL,10000,0,'Purchase Reward Earned','2026-08-28 16:27:49.205079','2026-08-28 16:27:49.205069'),(6,1,NULL,100,2,'Administrative Points Adjustment','2026-08-28 16:28:35.390832','2026-08-28 16:28:35.390817'),(7,1,NULL,100,2,'Administrative Points Adjustment','2026-08-28 16:29:14.804528','2026-08-28 16:29:14.804483');

--
-- Table structure for table `manufacturer`
--

DROP TABLE IF EXISTS `manufacturer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `manufacturer` (
  `manufacturer_id` char(36) NOT NULL,
  `name` varchar(200) NOT NULL,
  `registration_number` varchar(100) DEFAULT NULL,
  `website` varchar(300) DEFAULT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`manufacturer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturer`
--

INSERT INTO `manufacturer` VALUES ('08dea521-cea7-4353-813b-4d2c0e7599f4','Clexan Foods','CLX-001','https://clexan.local','Seed manufacturer from legacy dataset','/files/legacy/chinchin-pkg.png','2026-04-28 12:29:35.366578','2026-04-28 12:29:35.365335',NULL);

--
-- Table structure for table `manufacturer_email`
--

DROP TABLE IF EXISTS `manufacturer_email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `manufacturer_email` (
  `manufacturer_email_id` int(11) NOT NULL AUTO_INCREMENT,
  `manufacturer_id` char(36) NOT NULL,
  `email_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`manufacturer_email_id`),
  KEY `ix_manufacturer_email_manufacturer_id` (`manufacturer_id`),
  KEY `ix_manufacturer_email_email_id` (`email_id`),
  CONSTRAINT `fk_manufacturer_email_email_email_id` FOREIGN KEY (`email_id`) REFERENCES `email` (`email_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_manufacturer_email_manufacturers_manufacturer_id` FOREIGN KEY (`manufacturer_id`) REFERENCES `manufacturer` (`manufacturer_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturer_email`
--

INSERT INTO `manufacturer_email` VALUES (1,'08dea521-cea7-4353-813b-4d2c0e7599f4',1,1,'2026-04-28 12:29:40.133574','2026-04-28 12:29:40.131696');

--
-- Table structure for table `manufacturer_location`
--

DROP TABLE IF EXISTS `manufacturer_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `manufacturer_location` (
  `manufacturer_location_id` int(11) NOT NULL AUTO_INCREMENT,
  `manufacturer_id` char(36) NOT NULL,
  `location_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`manufacturer_location_id`),
  KEY `ix_manufacturer_location_manufacturer_id` (`manufacturer_id`),
  KEY `ix_manufacturer_location_location_id` (`location_id`),
  CONSTRAINT `fk_manufacturer_location_location_location_id` FOREIGN KEY (`location_id`) REFERENCES `location` (`location_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_manufacturer_location_manufacturer_manufacturer_id` FOREIGN KEY (`manufacturer_id`) REFERENCES `manufacturer` (`manufacturer_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturer_location`
--

INSERT INTO `manufacturer_location` VALUES (1,'08dea521-cea7-4353-813b-4d2c0e7599f4',1,1,'2026-04-28 12:29:40.242536','2026-04-28 12:29:40.241261');

--
-- Table structure for table `manufacturer_phone`
--

DROP TABLE IF EXISTS `manufacturer_phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `manufacturer_phone` (
  `manufacturer_phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `manufacturer_id` char(36) NOT NULL,
  `phone_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`manufacturer_phone_id`),
  KEY `ix_manufacturer_phone_phone_id` (`phone_id`),
  KEY `ix_manufacturer_phone_manufacturer_id` (`manufacturer_id`),
  CONSTRAINT `fk_manufacturer_phone_manufacturers_manufacturer_id` FOREIGN KEY (`manufacturer_id`) REFERENCES `manufacturer` (`manufacturer_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_manufacturer_phone_phones_phone_id` FOREIGN KEY (`phone_id`) REFERENCES `phone` (`phone_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturer_phone`
--

INSERT INTO `manufacturer_phone` VALUES (1,'08dea521-cea7-4353-813b-4d2c0e7599f4',1,1,'2026-04-28 12:29:40.186956','2026-04-28 12:29:40.185615');

--
-- Table structure for table `mobile_money_transaction`
--

DROP TABLE IF EXISTS `mobile_money_transaction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mobile_money_transaction` (
  `mobile_money_transaction_id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `invoice_id` char(36) NOT NULL,
  `provider` int(11) NOT NULL,
  `phone_number` longtext NOT NULL,
  `amount` decimal(65,30) NOT NULL,
  `status` int(11) NOT NULL,
  `provider_transaction_id` longtext DEFAULT NULL,
  `callback_payload` longtext DEFAULT NULL,
  `completed_at_utc` datetime(6) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`mobile_money_transaction_id`),
  KEY `ix_mobile_money_transaction_invoice_id` (`invoice_id`),
  CONSTRAINT `fk_mobile_money_transaction_invoice_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mobile_money_transaction`
--

INSERT INTO `mobile_money_transaction` VALUES ('29c56d0b-9b4a-4acf-9db1-76133110edd8','705c66a3-1fac-497e-ade9-d037550cd9d8',0,'+237600000001',1.000000000000000000000000000000,0,NULL,NULL,NULL,'2026-05-03 18:21:11.769963','2026-05-03 18:21:11.767285');

--
-- Table structure for table `notification`
--

DROP TABLE IF EXISTS `notification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `notification` (
  `notification_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `title` varchar(200) NOT NULL,
  `message` varchar(2000) NOT NULL,
  `type` varchar(20) NOT NULL,
  `is_read` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`notification_id`),
  KEY `ix_notification_user_id` (`user_id`),
  CONSTRAINT `fk_notification_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification`
--

INSERT INTO `notification` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_notification_title','seed_notification_message','Info',1,'2026-04-28 12:29:40.989062','2026-04-28 12:29:40.987306');

--
-- Table structure for table `order_item`
--

DROP TABLE IF EXISTS `order_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `order_item` (
  `order_item_id` int(11) NOT NULL AUTO_INCREMENT,
  `items_order_id` char(36) NOT NULL,
  `item_id` char(36) NOT NULL,
  `item_name` varchar(200) NOT NULL,
  `unit_cost` decimal(18,4) DEFAULT NULL,
  `quantity_ordered` int(11) NOT NULL,
  `quantity_received` int(11) NOT NULL,
  `line_total` decimal(18,2) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`order_item_id`),
  KEY `ix_order_item_items_order_id` (`items_order_id`),
  KEY `ix_order_item_item_id` (`item_id`),
  CONSTRAINT `fk_order_item_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_order_item_items_order_items_order_id` FOREIGN KEY (`items_order_id`) REFERENCES `items_order` (`items_order_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_item`
--

INSERT INTO `order_item` VALUES (1,'f757f246-505c-467f-bc06-f09f7af7933c','08dea521-ced8-464b-86c9-b8499dcfca2e','Seed OrderItem',NULL,1,1,1.00,'2026-04-28 12:29:39.926544','2026-04-28 12:29:39.925249'),(2,'5a6647b8-301a-4ca8-9091-cb7f186958f3','08dea521-cef1-4393-89bc-81ee43a577e9','Parle G',17.5000,136,0,2380.00,'2026-07-09 17:21:19.112474','2026-07-09 17:21:19.106069'),(3,'ca8cf8ff-3f53-4819-a6a5-5e1df9df9629','08dea521-cef0-4658-80a1-c48e6a26b7a0','Pea nuts',70.0000,478,0,33460.00,'2026-07-09 17:22:55.269411','2026-07-09 17:22:55.269380'),(4,'ca8cf8ff-3f53-4819-a6a5-5e1df9df9629','08dea521-cef0-4b84-83a7-348648a9aa1d','Eggs',1540.0000,403,0,620620.00,'2026-07-09 17:22:55.269450','2026-07-09 17:22:55.269423');

--
-- Table structure for table `otp`
--

DROP TABLE IF EXISTS `otp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `otp` (
  `otp_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `code` varchar(10) NOT NULL,
  `purpose` varchar(30) NOT NULL,
  `expires_at` datetime(6) NOT NULL,
  `is_used` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`otp_id`),
  KEY `ix_otp_user_id` (`user_id`),
  CONSTRAINT `fk_otp_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `otp`
--

INSERT INTO `otp` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','SEED-CODE','PasswordReset','2026-04-28 12:29:41.015325',1,'2026-04-28 12:29:41.054852','2026-04-28 12:29:41.053876');

--
-- Table structure for table `password_reset_token`
--

DROP TABLE IF EXISTS `password_reset_token`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `password_reset_token` (
  `password_reset_token_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `token_hash` longtext NOT NULL,
  `expiry_date` datetime(6) NOT NULL,
  `is_used` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`password_reset_token_id`),
  KEY `ix_password_reset_token_user_id` (`user_id`),
  CONSTRAINT `fk_password_reset_token_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `password_reset_token`
--

INSERT INTO `password_reset_token` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_password_reset_token_token_hash','2026-08-10 19:10:26.421947',1,'2026-08-10 19:10:26.492772','2026-08-10 19:10:26.490032'),(2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','4EjuWbRvCLpl9efsMHeQOZPLNQtYSNxIRg/QbYqjTXk=','2026-08-10 21:11:25.930726',0,'2026-08-10 20:41:26.034933','2026-08-10 20:41:26.031003');

--
-- Table structure for table `phone`
--

DROP TABLE IF EXISTS `phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `phone` (
  `phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `country_id` int(11) NOT NULL,
  `number` varchar(30) NOT NULL,
  `type` varchar(20) NOT NULL,
  `is_verified` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`phone_id`),
  UNIQUE KEY `ix_phone_country_id_number` (`country_id`,`number`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `phone`
--

INSERT INTO `phone` VALUES (1,1,'678787878','Mobile',1,'2026-04-28 12:29:38.096520','2026-08-12 23:48:26.016497'),(2,0,'694949494','Mobile',0,'2026-08-04 13:49:52.368728','2026-08-04 13:49:52.367600'),(3,0,'634783738','Mobile',0,'2026-08-05 08:05:34.419569','2026-08-05 08:05:34.417237'),(4,0,'+237651515151','Work',0,'2026-08-31 22:27:08.459664','2026-08-31 22:27:08.458970');

--
-- Table structure for table `privilege`
--

DROP TABLE IF EXISTS `privilege`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `privilege` (
  `privilege_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `module` varchar(100) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`privilege_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `privilege`
--

INSERT INTO `privilege` VALUES (1,'Seed Privilege',NULL,'seed_privilege_module','2026-04-28 12:29:39.080574','2026-04-28 12:29:39.079311');

--
-- Table structure for table `purchase_order`
--

DROP TABLE IF EXISTS `purchase_order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `purchase_order` (
  `purchase_order_id` int(11) NOT NULL AUTO_INCREMENT,
  `reference_number` varchar(100) DEFAULT NULL,
  `supplier_id` char(36) NOT NULL,
  `branch_id` int(11) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `expected_delivery_date` datetime(6) DEFAULT NULL,
  `notes` varchar(2000) DEFAULT NULL,
  `requested_by_user_id` char(36) NOT NULL,
  `approved_by_user_id` char(36) DEFAULT NULL,
  `approved_at` datetime(6) DEFAULT NULL,
  `received_at` datetime(6) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`purchase_order_id`),
  UNIQUE KEY `ix_purchase_order_reference_number` (`reference_number`),
  KEY `ix_purchase_order_approved_by_user_id` (`approved_by_user_id`),
  KEY `ix_purchase_order_branch_id` (`branch_id`),
  KEY `ix_purchase_order_requested_by_user_id` (`requested_by_user_id`),
  KEY `ix_purchase_order_status_date_created` (`status`,`date_created`),
  KEY `ix_purchase_order_supplier_id` (`supplier_id`),
  CONSTRAINT `fk_purchase_order_branch_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_purchase_order_suppliers_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `supplier` (`supplier_id`),
  CONSTRAINT `fk_purchase_order_users_approved_by_user_id` FOREIGN KEY (`approved_by_user_id`) REFERENCES `user` (`user_id`),
  CONSTRAINT `fk_purchase_order_users_requested_by_user_id` FOREIGN KEY (`requested_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `purchase_order`
--

INSERT INTO `purchase_order` VALUES (1,NULL,'cb681cbf-6200-450f-9416-71d5b106d490',2,0,NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,'2026-05-07 05:40:13.449380','2026-05-07 05:40:13.445089'),(2,'PO-202608-6599','cb681cbf-6200-450f-9416-71d5b106d490',2,4,'2026-08-27 00:00:00.000000','Running out','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-08-27 19:56:17.755176','2026-08-27 19:56:24.463189','2026-08-27 19:55:52.875755','2026-08-27 19:56:24.521532'),(3,'PO-AUTO-20260831-782','cb681cbf-6200-450f-9416-71d5b106d490',NULL,0,'2026-09-03 19:04:10.300602','Automated stock replenishment order generated on 2026-08-31 19:04.','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,NULL,'2026-08-31 19:04:10.511280','2026-08-31 21:39:48.154039'),(4,'PO-202608-2072','bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',2,4,'2026-09-02 00:00:00.000000','Urgent!!!','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-08-31 22:30:59.698563','2026-08-31 22:31:22.588615','2026-08-31 22:30:23.094796','2026-08-31 22:31:22.617863'),(5,'PO-202609-5414','bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',2,4,'2026-09-01 00:00:00.000000','Getting some eggs','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-09-01 11:36:23.650142','2026-09-01 11:36:26.950231','2026-09-01 11:36:12.901901','2026-09-01 11:36:26.966209'),(6,'PO-AUTO-20260901-587','bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',NULL,0,'2026-09-04 13:31:10.417728','Automated stock replenishment order generated on 2026-09-01 13:31.','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,NULL,'2026-09-01 13:31:10.426839','2026-09-01 13:31:10.426798'),(7,'PO-AUTO-20260902-348','bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',NULL,0,'2026-09-05 11:25:08.279136','Automated stock replenishment order generated on 2026-09-02 11:25.','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,NULL,'2026-09-02 11:25:09.163683','2026-09-02 19:38:01.599077');

--
-- Table structure for table `purchase_order_item`
--

DROP TABLE IF EXISTS `purchase_order_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `purchase_order_item` (
  `purchase_order_item_id` int(11) NOT NULL AUTO_INCREMENT,
  `purchase_order_id` int(11) NOT NULL,
  `item_id` char(36) NOT NULL,
  `ordered_quantity` int(11) NOT NULL,
  `unit_cost` decimal(65,30) NOT NULL,
  `received_quantity` int(11) NOT NULL,
  `notes` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`purchase_order_item_id`),
  KEY `ix_purchase_order_item_item_id` (`item_id`),
  KEY `ix_purchase_order_item_purchase_order_id` (`purchase_order_id`),
  CONSTRAINT `fk_purchase_order_item_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_purchase_order_item_purchase_order_purchase_order_id` FOREIGN KEY (`purchase_order_id`) REFERENCES `purchase_order` (`purchase_order_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `purchase_order_item`
--

INSERT INTO `purchase_order_item` VALUES (1,1,'08dea521-ced8-464b-86c9-b8499dcfca2e',1,1.000000000000000000000000000000,1,NULL,'2026-05-07 05:40:14.079029','2026-05-07 05:40:14.078191'),(2,2,'08dea521-ced8-464b-86c9-b8499dcfca2e',4000,70.000000000000000000000000000000,4000,NULL,'2026-08-27 19:55:53.025399','2026-08-27 19:56:24.521508'),(3,2,'08dea521-cef1-4145-89a0-93a853adbdaa',174,17.500000000000000000000000000000,174,NULL,'2026-08-27 19:55:53.028577','2026-08-27 19:56:24.521525'),(4,3,'08dea521-cef0-4a04-8f9e-d0ba087434cf',10,910.000000000000000000000000000000,0,'Auto-replenish: Stock 0 <= Reorder 5','2026-08-31 19:04:10.512672','2026-08-31 19:04:10.511823'),(5,3,'08dea521-cef0-4b84-83a7-348648a9aa1d',10,1540.000000000000000000000000000000,0,'Auto-replenish: Stock 0 <= Reorder 5','2026-08-31 19:04:10.513193','2026-08-31 19:04:10.513185'),(6,3,'08dea521-cef1-4f65-80d8-1f2950510353',10,175.000000000000000000000000000000,0,'Auto-replenish: Stock 0 <= Reorder 5','2026-08-31 19:04:10.513199','2026-08-31 19:04:10.513196'),(7,4,'08dea521-cef1-4f65-80d8-1f2950510353',100,500.000000000000000000000000000000,100,NULL,'2026-08-31 22:30:23.214578','2026-08-31 22:31:22.617824'),(8,4,'08dea521-cef1-4f65-80d8-1f2950510353',100,175.000000000000000000000000000000,100,NULL,'2026-08-31 22:30:23.215575','2026-08-31 22:31:22.617831'),(9,4,'08dea521-cef1-4f65-80d8-1f2950510353',100,175.000000000000000000000000000000,100,NULL,'2026-08-31 22:30:23.215589','2026-08-31 22:31:22.617836'),(10,4,'08dea521-cef0-44cb-829b-e0b7ac49ee8b',8,140.000000000000000000000000000000,8,NULL,'2026-08-31 22:30:23.215595','2026-08-31 22:31:22.617856'),(11,4,'08dea521-cef1-4e71-86ab-dd55bb82e6af',345,5000.000000000000000000000000000000,345,NULL,'2026-08-31 22:30:23.215600','2026-08-31 22:31:22.617860'),(12,5,'08dea521-cef0-4b84-83a7-348648a9aa1d',123,1540.000000000000000000000000000000,123,NULL,'2026-09-01 11:36:12.945871','2026-09-01 11:36:26.966196'),(13,6,'08dea521-cef0-4a04-8f9e-d0ba087434cf',10,910.000000000000000000000000000000,0,'Auto-replenish: Stock 0 <= Reorder 5','2026-09-01 13:31:10.426886','2026-09-01 13:31:10.426881'),(14,7,'08dea521-cef0-4a04-8f9e-d0ba087434cf',10,910.000000000000000000000000000000,0,'Auto-replenish: Stock 0 <= Reorder 5','2026-09-02 11:25:09.166149','2026-09-02 11:25:09.165150');

--
-- Table structure for table `region`
--

DROP TABLE IF EXISTS `region`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `region` (
  `region_id` int(11) NOT NULL AUTO_INCREMENT,
  `country_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`region_id`),
  KEY `ix_region_country_id` (`country_id`),
  CONSTRAINT `fk_region_country_country_id` FOREIGN KEY (`country_id`) REFERENCES `country` (`country_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `region`
--

INSERT INTO `region` VALUES (1,1,'Seed Region','2026-04-28 12:29:39.254502','2026-04-28 12:29:39.253609'),(2,2,'Default','2026-08-31 22:27:08.772890','2026-08-31 22:27:08.769674');

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `role` (
  `role_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `description` varchar(300) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `ix_role_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

INSERT INTO `role` VALUES (1,'Admin','Full system access','2024-01-01 00:00:00.000000','2024-01-01 00:00:00.000000'),(2,'Manager','Management level access','2024-01-01 00:00:00.000000','2024-01-01 00:00:00.000000'),(3,'User','Standard user access','2024-01-01 00:00:00.000000','2024-01-01 00:00:00.000000'),(4,'Cashier','Cash operations access only','2026-05-03 22:42:01.594506','2026-05-03 22:42:01.589203');

--
-- Table structure for table `role_permission`
--

DROP TABLE IF EXISTS `role_permission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `role_permission` (
  `role_permission_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `role_id` int(11) NOT NULL,
  `permission_key` varchar(255) NOT NULL,
  `is_allowed` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`role_permission_id`),
  UNIQUE KEY `ix_role_permission_role_id_permission_key` (`role_id`,`permission_key`),
  CONSTRAINT `fk_role_permission_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role_permission`
--

INSERT INTO `role_permission` VALUES (2,1,'inventory.read',1,'2026-05-03 22:42:02.414389','2026-05-03 22:42:02.412736'),(3,1,'inventory.write',1,'2026-05-03 22:42:02.415135','2026-05-03 22:42:02.415122'),(4,1,'pricing.read',1,'2026-05-03 22:42:02.415149','2026-05-03 22:42:02.415144'),(5,1,'pricing.write',1,'2026-05-03 22:42:02.415162','2026-05-03 22:42:02.415156'),(6,1,'cash.read',1,'2026-05-03 22:42:02.415194','2026-05-03 22:42:02.415186'),(7,1,'cash.write',1,'2026-05-03 22:42:02.415207','2026-05-03 22:42:02.415202'),(8,1,'reports.read',1,'2026-05-03 22:42:02.415220','2026-05-03 22:42:02.415215'),(9,1,'admin.rolematrix',1,'2026-05-03 22:42:02.415232','2026-05-03 22:42:02.415227'),(10,1,'payments.read',1,'2026-05-03 22:42:02.415245','2026-05-03 22:42:02.415239'),(11,1,'admin.branches',1,'2026-05-03 22:42:02.415265','2026-05-03 22:42:02.415254'),(12,2,'inventory.read',1,'2026-05-03 22:42:02.415285','2026-05-03 22:42:02.415273'),(13,2,'inventory.write',1,'2026-05-03 22:42:02.415299','2026-05-03 22:42:02.415293'),(14,2,'pricing.read',1,'2026-05-03 22:42:02.415322','2026-05-03 22:42:02.415316'),(15,2,'pricing.write',1,'2026-05-03 22:42:02.415335','2026-05-03 22:42:02.415329'),(16,2,'cash.read',1,'2026-05-03 22:42:02.415530','2026-05-03 22:42:02.415373'),(17,2,'cash.write',1,'2026-05-03 22:42:02.415570','2026-05-03 22:42:02.415554'),(18,2,'reports.read',1,'2026-05-03 22:42:02.415607','2026-05-03 22:42:02.415591'),(19,2,'payments.read',1,'2026-05-03 22:42:02.415643','2026-05-03 22:42:02.415628'),(20,4,'cash.read',0,'2026-05-03 22:42:02.415670','2026-08-28 16:23:43.963425'),(21,4,'cash.write',0,'2026-05-03 22:42:02.415708','2026-08-28 16:23:45.074714'),(22,2,'admin.rolematrix',1,'2026-08-15 15:59:16.889916','2026-08-15 15:59:18.122392'),(23,3,'pricing.read',0,'2026-08-15 15:59:31.562733','2026-08-31 20:32:46.432082'),(24,3,'inventory.read',0,'2026-08-28 16:23:29.779838','2026-08-28 16:23:31.812934'),(25,1,'admin.users',1,'2026-08-31 15:19:01.485920','2026-08-31 15:19:01.475736'),(26,1,'admin.settings',1,'2026-08-31 15:19:01.486908','2026-08-31 15:19:01.486897'),(27,4,'pricing.read',1,'2026-08-31 20:32:47.392217','2026-08-31 20:32:47.392204'),(28,4,'inventory.read',1,'2026-08-31 20:32:51.774312','2026-08-31 20:32:51.774294'),(29,4,'payments.read',1,'2026-08-31 20:33:00.301066','2026-08-31 20:33:00.301043'),(30,4,'reports.read',1,'2026-08-31 20:33:04.517135','2026-08-31 20:33:04.517125');

--
-- Table structure for table `salary`
--

DROP TABLE IF EXISTS `salary`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `salary` (
  `salary_id` int(11) NOT NULL AUTO_INCREMENT,
  `grade` varchar(50) NOT NULL,
  `basic_amount` decimal(18,2) NOT NULL,
  `allowance_amount` decimal(18,2) DEFAULT NULL,
  `description` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`salary_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `salary`
--

INSERT INTO `salary` VALUES (1,'seed_salary_grade',1.00,NULL,NULL,'2026-04-28 12:29:39.145849','2026-04-28 12:29:39.144953');

--
-- Table structure for table `sale`
--

DROP TABLE IF EXISTS `sale`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `sale` (
  `sale_id` char(36) NOT NULL,
  `invoice_id` char(36) NOT NULL,
  `item_id` char(36) NOT NULL,
  `user_id` char(36) DEFAULT NULL,
  `item_name` varchar(200) NOT NULL,
  `unit_abbreviation` varchar(20) DEFAULT NULL,
  `unit_price` decimal(18,4) NOT NULL,
  `discount_amount` decimal(18,4) DEFAULT NULL,
  `quantity` int(11) NOT NULL,
  `line_total` decimal(18,2) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`sale_id`),
  KEY `ix_sale_user_id` (`user_id`),
  KEY `ix_sale_item_id` (`item_id`),
  KEY `ix_sale_invoice_id` (`invoice_id`),
  CONSTRAINT `fk_sale_invoice_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_sale_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_sale_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sale`
--

INSERT INTO `sale` VALUES ('0156b020-c75a-4d12-aa06-f6864813152d','cbe79e8d-9682-4abe-927c-e26169e062b1','dbeca20f-61cf-44fa-934e-7e76be480414','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Bread','pack',1000.0000,0.0000,32,32000.00,'2026-07-09 18:57:23.555655','2026-07-09 18:57:23.555643'),('15e9ad84-3f7f-4ee6-8ac1-43eec1dc00ab','47ca810c-3292-4550-b619-4d1c7985c409','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,NULL,1,1500.00,'2026-08-31 18:32:44.315565','2026-08-31 18:32:44.315097'),('16068540-558b-4eac-8233-338098c377ad','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-4145-89a0-93a853adbdaa','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Sugar','pack',25.0000,NULL,2,50.00,'2026-08-12 13:01:43.824767','2026-08-12 13:01:43.824747'),('1f8e7a07-ee58-4d2b-8920-9ddab2e169b9','cbe79e8d-9682-4abe-927c-e26169e062b1','08dea521-cef1-4f65-80d8-1f2950510353','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Spagetti','pack',250.0000,0.0000,3,750.00,'2026-07-09 18:57:23.555323','2026-07-09 18:57:23.555315'),('22980588-d6fd-43ee-a6b3-3843feb3e90b','66406176-c89f-4d2d-a650-4ddbffc9c525','d77e5326-3f06-44c4-997b-a3eb2889f20c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Rice','pack',1000.0000,0.0000,9,9000.00,'2026-08-04 14:15:23.570968','2026-08-04 14:15:23.570967'),('249b21e5-d7cb-43af-99b4-3e3a9749644f','b9164763-1870-491c-9279-640a4a27cc06','08dea521-cef0-44cb-829b-e0b7ac49ee8b','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Dough-nuts','retail',100.0000,NULL,1,100.00,'2026-08-05 08:02:35.690346','2026-08-05 08:02:35.690339'),('2ded627d-b692-4b4e-92f0-b27b91ca2430','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef0-4041-89c8-61936bece7ad','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',500.0000,NULL,1,500.00,'2026-08-12 13:01:43.824590','2026-08-12 13:01:43.824586'),('3083db6a-828e-475f-bc7e-7f15342b286b','b9164763-1870-491c-9279-640a4a27cc06','08dea521-cef0-4c39-804d-2fbb09083b8c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Maggi','pack',50.0000,NULL,1,50.00,'2026-08-05 08:02:35.690311','2026-08-05 08:02:35.690289'),('30df18c3-4345-47d1-a9f7-058e8775c7be','b9164763-1870-491c-9279-640a4a27cc06','08dea521-cef1-4393-89bc-81ee43a577e9','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Parle G','pack',25.0000,NULL,1,25.00,'2026-08-05 08:02:35.690333','2026-08-05 08:02:35.690328'),('32721380-5475-4891-9303-f7dfe0b6e3dd','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',100.0000,NULL,2,200.00,'2026-08-31 18:32:44.315867','2026-08-31 18:32:44.315860'),('349a27ad-f15f-423d-bf27-4d5a7b896ca9','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-48b7-8fed-1eb919645da8','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Sponge','retail',50.0000,NULL,2,100.00,'2026-08-12 13:01:43.824624','2026-08-12 13:01:43.824620'),('36e50a3e-63c1-4e98-88b0-b72d03e1e5cc','27a976d6-4c27-4058-bbc5-c144099a5006','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,NULL,7,10500.00,'2026-08-12 13:00:03.600137','2026-08-12 13:00:03.599406'),('37916b94-c54b-4bf9-bbe9-3b078ec76ce1','cbe79e8d-9682-4abe-927c-e26169e062b1','08dea521-cef0-4a04-8f9e-d0ba087434cf','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Pea nuts','pack',1300.0000,0.0000,6,7800.00,'2026-07-09 18:57:23.555290','2026-07-09 18:57:23.555268'),('399d3f9b-7896-4256-94f8-7a27fde2b480','66406176-c89f-4d2d-a650-4ddbffc9c525','08dea521-cef0-4a04-8f9e-d0ba087434cf','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Pea nuts','pack',1300.0000,0.0000,4,5200.00,'2026-08-04 14:15:23.570954','2026-08-04 14:15:23.570951'),('3bcd4531-7122-412f-be8e-ea129c7ff91e','cbe79e8d-9682-4abe-927c-e26169e062b1','08dea521-cef1-4532-873e-152c73c450ea','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Planet','bottle',500.0000,0.0000,4,2000.00,'2026-07-09 18:57:23.555305','2026-07-09 18:57:23.555299'),('415cd328-487d-4f47-8b78-053dbb2eff51','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cef0-4add-87bc-de5cbcd4f390','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Eggs','retail',100.0000,NULL,1,100.00,'2026-08-31 18:32:44.315872','2026-08-31 18:32:44.315870'),('4c22514f-394f-4491-af49-1779a814e781','705c66a3-1fac-497e-ade9-d037550cd9d8','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Seed Sale',NULL,1.0000,NULL,1,1.00,'2026-04-28 12:29:39.856531','2026-04-28 12:29:39.854724'),('4cf08e1d-6605-403e-871f-b8cf92da821c','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cef0-44cb-829b-e0b7ac49ee8b','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Dough-nuts','retail',100.0000,NULL,1,100.00,'2026-08-31 18:32:44.315877','2026-08-31 18:32:44.315875'),('577a10c7-e011-4bc3-9d20-237088745659','66406176-c89f-4d2d-a650-4ddbffc9c525','dbeca20f-61cf-44fa-934e-7e76be480414','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Bread','pack',1000.0000,0.0000,8,8000.00,'2026-08-04 14:15:23.570958','2026-08-04 14:15:23.570956'),('59a36b46-3c74-4ca5-b01d-71f2c6849047','b340a5a7-3115-4f0f-9012-2faeb58696c9','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',100.0000,1.0000,7,693.00,'2026-08-02 15:44:19.142440','2026-08-02 15:44:19.141477'),('65ceb5eb-17f1-40ca-8444-763aba05c032','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-4e71-86ab-dd55bb82e6af','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Tomatoes','sachet',100.0000,NULL,4,400.00,'2026-08-12 13:01:43.824789','2026-08-12 13:01:43.824785'),('68c131ba-1ee5-4d8a-b49b-09505ff86b54','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cef0-4658-80a1-c48e6a26b7a0','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Pea nuts','pack',100.0000,NULL,1,100.00,'2026-08-31 18:32:44.315893','2026-08-31 18:32:44.315889'),('6e59dd3f-6d25-4493-acdf-45e7ed55c999','d22653b7-a0a3-4d92-8801-349bd9909f08','dbeca20f-61cf-44fa-934e-7e76be480414','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Bread','pack',1000.0000,NULL,1,1000.00,'2026-08-12 13:01:43.824581','2026-08-12 13:01:43.824577'),('7189a6dd-40af-4f9f-a6fd-1d1dc93394c3','66406176-c89f-4d2d-a650-4ddbffc9c525','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,0.0000,19,28500.00,'2026-08-04 14:15:23.570654','2026-08-04 14:15:23.570030'),('740b5ebb-ef89-4a5d-9bd3-7a16fa2b5be9','d22653b7-a0a3-4d92-8801-349bd9909f08','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,NULL,4,6000.00,'2026-08-12 13:01:43.824564','2026-08-12 13:01:43.824557'),('83fd307c-500b-42ca-b531-c3f033a95624','d22653b7-a0a3-4d92-8801-349bd9909f08','d77e5326-3f06-44c4-997b-a3eb2889f20c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Rice','pack',1000.0000,NULL,4,4000.00,'2026-08-12 13:01:43.824605','2026-08-12 13:01:43.824602'),('891d8d1b-8eb6-42bc-aeea-2c3aa4325396','cbe79e8d-9682-4abe-927c-e26169e062b1','08dea521-cef1-47f3-8aa3-3af000328106','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Sponge','retail',50.0000,0.0000,1,50.00,'2026-07-09 18:57:23.555575','2026-07-09 18:57:23.555335'),('92aafde3-4b44-4686-9906-6feb250f6d9c','8c30dbf5-dec0-42ec-af55-049f01f1a072','08dea521-cef0-4b84-83a7-348648a9aa1d','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Eggs','tray',2200.0000,0.0000,4,8800.00,'2026-07-09 18:55:25.687424','2026-07-09 18:55:25.686035'),('958f9947-9dd8-4786-888f-adada7a75ffd','47ca810c-3292-4550-b619-4d1c7985c409','d77e5326-3f06-44c4-997b-a3eb2889f20c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Rice','pack',1000.0000,NULL,1,1000.00,'2026-08-31 18:32:44.315902','2026-08-31 18:32:44.315900'),('b03ede85-4020-44fa-8e4c-50220188d726','b340a5a7-3115-4f0f-9012-2faeb58696c9','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,0.0000,1,1500.00,'2026-08-02 15:44:19.142741','2026-08-02 15:44:19.142736'),('b65ae7bb-62af-450b-9446-386238570742','b9164763-1870-491c-9279-640a4a27cc06','dbeca20f-61cf-44fa-934e-7e76be480414','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Bread','pack',1000.0000,NULL,1,1000.00,'2026-08-05 08:02:35.689536','2026-08-05 08:02:35.689067'),('b85cad49-17c9-406d-a68f-8c4b422c8190','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cef1-4393-89bc-81ee43a577e9','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Parle G','pack',25.0000,NULL,1,25.00,'2026-08-31 18:32:44.315885','2026-08-31 18:32:44.315880'),('cb2db68b-7746-463c-b3b8-5e0ca5e4c9b4','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-401a-8d61-b015149e112b','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Salt','pack',50.0000,NULL,2,100.00,'2026-08-12 13:01:43.824614','2026-08-12 13:01:43.824610'),('cef700ea-5cbf-4efc-a3a1-c1878c5a3dac','b9164763-1870-491c-9279-640a4a27cc06','08dea521-cef0-4041-89c8-61936bece7ad','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',500.0000,NULL,1,500.00,'2026-08-05 08:02:35.690130','2026-08-05 08:02:35.690088'),('d3ebcb80-ca14-4788-bbd6-b6268246212b','b9164763-1870-491c-9279-640a4a27cc06','4ea66295-b6c4-458e-ae58-83ae51670839','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Broli Milk','bottle',1500.0000,NULL,16,24000.00,'2026-08-05 08:02:35.690367','2026-08-05 08:02:35.690356'),('d47693c1-43db-4af0-972c-9285878e1ef8','66406176-c89f-4d2d-a650-4ddbffc9c525','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',100.0000,1.0000,11,1089.00,'2026-08-04 14:15:23.570963','2026-08-04 14:15:23.570960'),('d6b11133-ae33-4ab2-b75a-dbafce175712','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-4393-89bc-81ee43a577e9','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Parle G','pack',25.0000,NULL,1,25.00,'2026-08-12 13:01:43.824598','2026-08-12 13:01:43.824595'),('dfa09c35-ec00-4714-ac17-3108ccdbb8af','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-ced8-464b-86c9-b8499dcfca2e','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Chin-chin','pack',100.0000,NULL,2,200.00,'2026-08-12 13:01:43.824780','2026-08-12 13:01:43.824776'),('e3170c39-87b3-444b-8e93-3cad791ff3e4','66406176-c89f-4d2d-a650-4ddbffc9c525','08dea521-cef1-4f65-80d8-1f2950510353','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Spagetti','pack',250.0000,0.0000,7,1750.00,'2026-08-04 14:15:23.570933','2026-08-04 14:15:23.570927'),('e987a321-ba3b-4be8-a299-f0fecc5dd473','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-4532-873e-152c73c450ea','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Planet','bottle',500.0000,NULL,2,1000.00,'2026-08-12 13:01:43.824812','2026-08-12 13:01:43.824809'),('e9c3cb76-a8d4-4425-8348-a52fbeb9344f','d22653b7-a0a3-4d92-8801-349bd9909f08','08dea521-cef1-4f65-80d8-1f2950510353','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Spagetti','pack',250.0000,NULL,2,500.00,'2026-08-12 13:01:43.824804','2026-08-12 13:01:43.824798'),('fb910fe8-69fd-4085-b1b1-c6e7b84189e3','47ca810c-3292-4550-b619-4d1c7985c409','08dea521-cef1-401a-8d61-b015149e112b','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','Salt','pack',50.0000,NULL,1,50.00,'2026-08-31 18:32:44.315906','2026-08-31 18:32:44.315904');

--
-- Table structure for table `stock_movement`
--

DROP TABLE IF EXISTS `stock_movement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `stock_movement` (
  `stock_movement_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `invoice_id` char(36) DEFAULT NULL,
  `items_order_id` char(36) DEFAULT NULL,
  `performed_by_user_id` char(36) DEFAULT NULL,
  `movement_type` int(11) NOT NULL,
  `quantity_delta` int(11) NOT NULL,
  `stock_before` int(11) NOT NULL,
  `stock_after` int(11) NOT NULL,
  `unit_cost` decimal(65,30) DEFAULT NULL,
  `unit_price` decimal(65,30) DEFAULT NULL,
  `reason` longtext NOT NULL,
  `reference_code` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`stock_movement_id`),
  KEY `ix_stock_movement_invoice_id` (`invoice_id`),
  KEY `ix_stock_movement_item_id_date_created` (`item_id`,`date_created`),
  KEY `ix_stock_movement_items_order_id` (`items_order_id`),
  KEY `ix_stock_movement_performed_by_user_id` (`performed_by_user_id`),
  CONSTRAINT `fk_stock_movement_invoice_invoice_id` FOREIGN KEY (`invoice_id`) REFERENCES `invoice` (`invoice_id`),
  CONSTRAINT `fk_stock_movement_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_stock_movement_items_order_items_order_id` FOREIGN KEY (`items_order_id`) REFERENCES `items_order` (`items_order_id`),
  CONSTRAINT `fk_stock_movement_users_performed_by_user_id` FOREIGN KEY (`performed_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock_movement`
--

INSERT INTO `stock_movement` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e','705c66a3-1fac-497e-ade9-d037550cd9d8','f757f246-505c-467f-bc06-f09f7af7933c','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,1,1,1,NULL,NULL,'seed_stock_movement_reason',NULL,'2026-05-02 17:51:08.757053','2026-05-02 17:51:08.755793'),(2,'08dea521-cef0-4c39-804d-2fbb09083b8c',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',3,45,50,95,35.000000000000000000000000000000,50.000000000000000000000000000000,'Too much','ADJ-20260709172814','2026-07-09 17:28:14.967675','2026-07-09 17:28:14.966759'),(3,'08dea521-cef0-4c39-804d-2fbb09083b8c',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',2,330,95,425,35.000000000000000000000000000000,50.000000000000000000000000000000,'Customer return',NULL,'2026-07-09 17:28:40.379160','2026-07-09 17:28:40.379086'),(4,'08dea521-cef1-47f3-8aa3-3af000328106',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',3,-13,12,0,NULL,NULL,'Wastage [Other]: Rusting','34987','2026-07-10 17:57:14.946764','2026-07-10 17:57:14.946303'),(5,'d77e5326-3f06-44c4-997b-a3eb2889f20c',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',3,-156,287,131,800.000000000000000000000000000000,1000.000000000000000000000000000000,'Wastage [Theft]: Some stole riceee!','WASTE-202608-4136','2026-08-27 18:48:51.295275','2026-08-27 18:48:51.293880'),(6,'08dea521-ced8-464b-86c9-b8499dcfca2e',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,4000,30,4030,70.000000000000000000000000000000,100.000000000000000000000000000000,'Goods receipt against PO #2 (PO-202608-6599)','PO-202608-6599','2026-08-27 19:56:24.518280','2026-08-27 19:56:24.517282'),(7,'08dea521-cef1-4145-89a0-93a853adbdaa',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,174,298,472,17.500000000000000000000000000000,25.000000000000000000000000000000,'Goods receipt against PO #2 (PO-202608-6599)','PO-202608-6599','2026-08-27 19:56:24.518709','2026-08-27 19:56:24.518702'),(8,'08dea521-cef1-4f65-80d8-1f2950510353',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,100,0,100,500.000000000000000000000000000000,250.000000000000000000000000000000,'Goods receipt against PO #4 (PO-202608-2072)','PO-202608-2072','2026-08-31 22:31:22.615226','2026-08-31 22:31:22.613450'),(9,'08dea521-cef1-4f65-80d8-1f2950510353',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,100,100,200,175.000000000000000000000000000000,250.000000000000000000000000000000,'Goods receipt against PO #4 (PO-202608-2072)','PO-202608-2072','2026-08-31 22:31:22.616808','2026-08-31 22:31:22.616707'),(10,'08dea521-cef1-4f65-80d8-1f2950510353',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,100,200,300,175.000000000000000000000000000000,250.000000000000000000000000000000,'Goods receipt against PO #4 (PO-202608-2072)','PO-202608-2072','2026-08-31 22:31:22.616838','2026-08-31 22:31:22.616834'),(11,'08dea521-cef0-44cb-829b-e0b7ac49ee8b',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,8,42,50,140.000000000000000000000000000000,100.000000000000000000000000000000,'Goods receipt against PO #4 (PO-202608-2072)','PO-202608-2072','2026-08-31 22:31:22.616865','2026-08-31 22:31:22.616861'),(12,'08dea521-cef1-4e71-86ab-dd55bb82e6af',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,345,38,383,5000.000000000000000000000000000000,100.000000000000000000000000000000,'Goods receipt against PO #4 (PO-202608-2072)','PO-202608-2072','2026-08-31 22:31:22.616872','2026-08-31 22:31:22.616869'),(13,'08dea521-cef0-4b84-83a7-348648a9aa1d',NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',0,123,0,123,1540.000000000000000000000000000000,2200.000000000000000000000000000000,'Goods receipt against PO #5 (PO-202609-5414)','PO-202609-5414','2026-09-01 11:36:26.966093','2026-09-01 11:36:26.966041');

--
-- Table structure for table `stock_transfer`
--

DROP TABLE IF EXISTS `stock_transfer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `stock_transfer` (
  `stock_transfer_id` int(11) NOT NULL AUTO_INCREMENT,
  `from_branch_id` int(11) NOT NULL,
  `to_branch_id` int(11) NOT NULL,
  `requested_by_user_id` char(36) NOT NULL,
  `approved_by_user_id` char(36) DEFAULT NULL,
  `dispatched_by_user_id` char(36) DEFAULT NULL,
  `received_by_user_id` char(36) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `rejection_reason` varchar(1000) DEFAULT NULL,
  `approved_at` datetime(6) DEFAULT NULL,
  `dispatched_at` datetime(6) DEFAULT NULL,
  `received_at` datetime(6) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`stock_transfer_id`),
  KEY `ix_stock_transfer_from_branch_id` (`from_branch_id`),
  KEY `ix_stock_transfer_requested_by_user_id` (`requested_by_user_id`),
  KEY `ix_stock_transfer_status_date_created` (`status`,`date_created`),
  KEY `ix_stock_transfer_to_branch_id` (`to_branch_id`),
  CONSTRAINT `fk_stock_transfer_branch_from_branch_id` FOREIGN KEY (`from_branch_id`) REFERENCES `branch` (`branch_id`),
  CONSTRAINT `fk_stock_transfer_branch_to_branch_id` FOREIGN KEY (`to_branch_id`) REFERENCES `branch` (`branch_id`),
  CONSTRAINT `fk_stock_transfer_users_requested_by_user_id` FOREIGN KEY (`requested_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock_transfer`
--

INSERT INTO `stock_transfer` VALUES (1,2,2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,1,'dsgsafsfas',NULL,'2026-07-08 10:08:23.336401',NULL,NULL,'2026-05-04 06:44:30.422554','2026-07-08 10:08:23.480840'),(2,3,2,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',NULL,NULL,NULL,0,'Some reason',NULL,NULL,NULL,NULL,'2026-08-27 18:07:13.642662','2026-08-27 18:07:13.641982');

--
-- Table structure for table `stock_transfer_item`
--

DROP TABLE IF EXISTS `stock_transfer_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `stock_transfer_item` (
  `stock_transfer_item_id` int(11) NOT NULL AUTO_INCREMENT,
  `stock_transfer_id` int(11) NOT NULL,
  `item_id` char(36) NOT NULL,
  `requested_quantity` int(11) NOT NULL,
  `dispatched_quantity` int(11) DEFAULT NULL,
  `received_quantity` int(11) DEFAULT NULL,
  `notes` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`stock_transfer_item_id`),
  KEY `ix_stock_transfer_item_item_id` (`item_id`),
  KEY `ix_stock_transfer_item_stock_transfer_id` (`stock_transfer_id`),
  CONSTRAINT `fk_stock_transfer_item_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_stock_transfer_item_stock_transfer_stock_transfer_id` FOREIGN KEY (`stock_transfer_id`) REFERENCES `stock_transfer` (`stock_transfer_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock_transfer_item`
--

INSERT INTO `stock_transfer_item` VALUES (1,1,'08dea521-ced8-464b-86c9-b8499dcfca2e',1,NULL,NULL,NULL,'2026-05-04 06:44:33.373573','2026-05-04 06:44:33.372226'),(2,2,'08dea521-cef0-4c39-804d-2fbb09083b8c',69,NULL,NULL,'Noting 1','2026-08-27 18:07:13.643420','2026-08-27 18:07:13.643004'),(3,2,'d77e5326-3f06-44c4-997b-a3eb2889f20c',100,NULL,NULL,'Some rice more','2026-08-27 18:07:13.643681','2026-08-27 18:07:13.643668');

--
-- Table structure for table `supplier`
--

DROP TABLE IF EXISTS `supplier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `supplier` (
  `supplier_id` char(36) NOT NULL,
  `name` varchar(200) NOT NULL,
  `registration_number` varchar(100) DEFAULT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `full_image_url` longtext DEFAULT NULL,
  PRIMARY KEY (`supplier_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier`
--

INSERT INTO `supplier` VALUES ('bc258ad1-b2e3-4071-81c2-8979d4d5d3d0','Mane Supplies','ET-345-289-R3','Net 12 days, with preferred deliveries on Thursdays','/files/suppliers/thumb/e1bcea06-c71e-4a1f-a467-1fa19865cbeb.webp','2026-08-31 22:27:09.238756','2026-08-31 22:27:09.237750','/files/suppliers/full/c70deab8-120f-4710-8daa-8c83784d0a6d.webp'),('cb681cbf-6200-450f-9416-71d5b106d490','Seed Supplier',NULL,NULL,NULL,'2026-04-28 12:29:39.207704','2026-04-28 12:29:39.206212',NULL);

--
-- Table structure for table `supplier_email`
--

DROP TABLE IF EXISTS `supplier_email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `supplier_email` (
  `supplier_email_id` int(11) NOT NULL AUTO_INCREMENT,
  `supplier_id` char(36) NOT NULL,
  `email_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`supplier_email_id`),
  KEY `ix_supplier_email_supplier_id` (`supplier_id`),
  KEY `ix_supplier_email_email_id` (`email_id`),
  CONSTRAINT `fk_supplier_email_email_email_id` FOREIGN KEY (`email_id`) REFERENCES `email` (`email_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_supplier_email_suppliers_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `supplier` (`supplier_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier_email`
--

INSERT INTO `supplier_email` VALUES (1,'cb681cbf-6200-450f-9416-71d5b106d490',1,1,'2026-04-28 12:29:40.457364','2026-04-28 12:29:40.456426'),(2,'bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',4,1,'2026-08-31 22:27:09.240239','2026-08-31 22:27:09.239460');

--
-- Table structure for table `supplier_location`
--

DROP TABLE IF EXISTS `supplier_location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `supplier_location` (
  `supplier_location_id` int(11) NOT NULL AUTO_INCREMENT,
  `supplier_id` char(36) NOT NULL,
  `location_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`supplier_location_id`),
  KEY `ix_supplier_location_supplier_id` (`supplier_id`),
  KEY `ix_supplier_location_location_id` (`location_id`),
  CONSTRAINT `fk_supplier_location_location_location_id` FOREIGN KEY (`location_id`) REFERENCES `location` (`location_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_supplier_location_supplier_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `supplier` (`supplier_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier_location`
--

INSERT INTO `supplier_location` VALUES (1,'cb681cbf-6200-450f-9416-71d5b106d490',1,1,'2026-04-28 12:29:40.562157','2026-04-28 12:29:40.561361'),(2,'bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',2,1,'2026-08-31 22:27:09.243552','2026-08-31 22:27:09.241836');

--
-- Table structure for table `supplier_phone`
--

DROP TABLE IF EXISTS `supplier_phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `supplier_phone` (
  `supplier_phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `supplier_id` char(36) NOT NULL,
  `phone_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`supplier_phone_id`),
  KEY `ix_supplier_phone_supplier_id` (`supplier_id`),
  KEY `ix_supplier_phone_phone_id` (`phone_id`),
  CONSTRAINT `fk_supplier_phone_phone_phone_id` FOREIGN KEY (`phone_id`) REFERENCES `phone` (`phone_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_supplier_phone_suppliers_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `supplier` (`supplier_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier_phone`
--

INSERT INTO `supplier_phone` VALUES (1,'cb681cbf-6200-450f-9416-71d5b106d490',1,1,'2026-04-28 12:29:40.512874','2026-04-28 12:29:40.511782'),(2,'bc258ad1-b2e3-4071-81c2-8979d4d5d3d0',4,1,'2026-08-31 22:27:09.245059','2026-08-31 22:27:09.243933');

--
-- Table structure for table `system_setting`
--

DROP TABLE IF EXISTS `system_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `system_setting` (
  `setting_key` varchar(128) NOT NULL,
  `setting_value` longtext DEFAULT NULL,
  `description` varchar(256) DEFAULT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`setting_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `system_setting`
--

INSERT INTO `system_setting` VALUES ('Auth:PasswordRecoveryMethod','TempPassword','Determines allowed password recovery methods (OTP, TempPassword, Both)','2026-08-17 13:09:45.000000');

--
-- Table structure for table `tax_profile`
--

DROP TABLE IF EXISTS `tax_profile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tax_profile` (
  `tax_profile_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` longtext NOT NULL,
  `rate_percent` decimal(65,30) NOT NULL,
  `application_type` int(11) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`tax_profile_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tax_profile`
--

INSERT INTO `tax_profile` VALUES (1,'Seed TaxProfile',1.000000000000000000000000000000,0,1,'2026-05-02 17:51:07.491265','2026-05-02 17:51:07.483003'),(2,'B1',1.520000000000000000000000000000,1,1,'2026-05-03 22:57:53.342277','2026-05-03 22:57:53.340942'),(3,'B1 Plus',2.720000000000000000000000000000,0,1,'2026-05-03 22:58:18.768237','2026-05-03 22:58:18.768226');

--
-- Table structure for table `unit`
--

DROP TABLE IF EXISTS `unit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `unit` (
  `unit_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `abbreviation` varchar(20) NOT NULL,
  `description` varchar(300) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`unit_id`),
  UNIQUE KEY `ix_unit_abbreviation` (`abbreviation`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `unit`
--

INSERT INTO `unit` VALUES (1,'Retail','retail','Single retail unit','2026-04-28 12:29:35.237197','2026-04-28 12:29:35.237191'),(2,'Pack','pack','Packaged unit','2026-04-28 12:29:35.237183','2026-04-28 12:29:35.237177'),(3,'Tray','tray','Tray unit','2026-04-28 12:29:35.237170','2026-04-28 12:29:35.237165'),(4,'Sachet','sachet','Sachet unit','2026-04-28 12:29:35.237158','2026-04-28 12:29:35.237146'),(5,'Bottle','bottle','Bottle unit','2026-04-28 12:29:35.236379','2026-04-28 12:29:35.234205');

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user` (
  `user_id` char(36) NOT NULL,
  `employee_id` char(36) DEFAULT NULL,
  `role_id` int(11) NOT NULL,
  `username` varchar(100) NOT NULL,
  `status` varchar(50) NOT NULL DEFAULT 'NotVerified',
  `thumbnail_url` varchar(500) DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `full_image_url` longtext DEFAULT NULL,
  `failed_login_attempts` int(11) NOT NULL DEFAULT 0,
  `lockout_end` datetime(6) DEFAULT NULL,
  `two_factor_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `two_factor_secret` longtext DEFAULT NULL,
  `security_stamp` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `ix_user_username` (`username`),
  KEY `ix_user_role_id` (`role_id`),
  KEY `ix_user_employee_id` (`employee_id`),
  CONSTRAINT `fk_user_employee_employee_id` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`) ON DELETE SET NULL,
  CONSTRAINT `fk_user_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

INSERT INTO `user` VALUES ('08dea521-cf2a-4d1a-8f7f-10c06b03b54a','e4ed1796-4741-11f1-814d-c858c0c6a8bc',1,'admin','Active','/files/users/thumb/2d325cc8-b5c1-493e-9609-1ef1fd05a790.webp','2026-04-28 12:29:36.270871','2026-09-02 12:15:15.530244','/files/users/full/e6888e07-44dd-44a4-a308-d2da2ce672eb.webp',0,NULL,0,'UIGJWY6NFKSI34LMLZUOKH52N7CJKIEQ','00000000-0000-0000-0000-000000000000'),('08dea965-515c-460c-8615-637eaaf1aae7','08dea965-5112-49dd-8600-fa31b090bf9f',2,'manager','Active','/files/legacy/user_default.png','2026-05-03 22:42:55.649309','2026-05-03 22:42:55.648578',NULL,0,NULL,0,NULL,'00000000-0000-0000-0000-000000000000'),('08dea965-529d-495c-8b9f-bd9147a02f23','08dea965-529c-424e-8b22-54203048b90a',4,'cashier','Active','/files/legacy/user_default.png','2026-05-03 22:42:57.608533','2026-08-02 15:37:22.160146',NULL,0,NULL,0,NULL,'00000000-0000-0000-0000-000000000000');

--
-- Table structure for table `user_branch_role`
--

DROP TABLE IF EXISTS `user_branch_role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_branch_role` (
  `user_branch_role_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `branch_id` int(11) NOT NULL,
  `role_id` int(11) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`user_branch_role_id`),
  KEY `ix_user_branch_role_branch_id` (`branch_id`),
  KEY `ix_user_branch_role_role_id` (`role_id`),
  KEY `ix_user_branch_role_user_id` (`user_id`),
  CONSTRAINT `fk_user_branch_role_branch_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `branch` (`branch_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_branch_role_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_branch_role_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_branch_role`
--

INSERT INTO `user_branch_role` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',2,1,'2026-05-03 21:24:10.153406','2026-05-03 21:24:10.152674'),(2,'08dea965-515c-460c-8615-637eaaf1aae7',2,2,'2026-05-03 22:42:57.548001','2026-05-03 22:42:57.547244'),(3,'08dea965-529d-495c-8b9f-bd9147a02f23',2,4,'2026-05-03 22:42:59.100236','2026-05-03 22:42:59.100204'),(4,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',3,4,'2026-08-13 03:58:01.986550','2026-08-13 03:58:01.984196');

--
-- Table structure for table `user_email`
--

DROP TABLE IF EXISTS `user_email`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_email` (
  `user_email_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `email_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`user_email_id`),
  KEY `ix_user_email_user_id` (`user_id`),
  KEY `ix_user_email_email_id` (`email_id`),
  CONSTRAINT `fk_user_email_email_email_id` FOREIGN KEY (`email_id`) REFERENCES `email` (`email_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_email_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_email`
--

INSERT INTO `user_email` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',1,1,'2026-04-28 12:29:40.765141','2026-04-28 12:29:40.764515');

--
-- Table structure for table `user_password`
--

DROP TABLE IF EXISTS `user_password`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_password` (
  `user_password_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `password_hash` varchar(256) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `force_password_change` tinyint(1) NOT NULL DEFAULT 0,
  `temp_password_expires_at` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`user_password_id`),
  UNIQUE KEY `ix_user_password_user_id` (`user_id`),
  CONSTRAINT `fk_user_password_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_password`
--

INSERT INTO `user_password` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','$2a$12$My/bX37EeHKPD/F1girzj.wkV5GEIFeetA7RXdp2Zzx6CxYzFVZD.','2026-04-28 12:29:37.908091','2026-08-12 08:29:46.916149',0,NULL),(2,'08dea965-515c-460c-8615-637eaaf1aae7','$2a$12$p9Y04RiRUpCcsztNpBpj8Ozg3tsGexcNCsmtFQr/EJ8cdnZPy.KRq','2026-05-03 22:42:57.546629','2026-05-03 22:42:57.545805',0,NULL),(3,'08dea965-529d-495c-8b9f-bd9147a02f23','$2a$12$Hhf0zN1kMWukHc86cwtHPu0nB.m1JJbtbU/C0UO1.pMQXIefHNr9a','2026-05-03 22:42:59.100261','2026-05-03 22:42:59.100247',0,NULL);

--
-- Table structure for table `user_phone`
--

DROP TABLE IF EXISTS `user_phone`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_phone` (
  `user_phone_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `phone_id` int(11) NOT NULL,
  `is_primary` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`user_phone_id`),
  KEY `ix_user_phone_user_id` (`user_id`),
  KEY `ix_user_phone_phone_id` (`phone_id`),
  CONSTRAINT `fk_user_phone_phone_phone_id` FOREIGN KEY (`phone_id`) REFERENCES `phone` (`phone_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_phone_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_phone`
--

INSERT INTO `user_phone` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',1,1,'2026-04-28 12:29:40.813611','2026-04-28 12:29:40.812970');

--
-- Table structure for table `user_privilege`
--

DROP TABLE IF EXISTS `user_privilege`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_privilege` (
  `user_privilege_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `privilege_id` int(11) NOT NULL,
  `type` int(11) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`user_privilege_id`),
  KEY `ix_user_privilege_user_id` (`user_id`),
  KEY `ix_user_privilege_privilege_id` (`privilege_id`),
  CONSTRAINT `fk_user_privilege_privilege_privilege_id` FOREIGN KEY (`privilege_id`) REFERENCES `privilege` (`privilege_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_privilege_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_privilege`
--

INSERT INTO `user_privilege` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a',1,0,1,'2026-04-28 12:29:41.120622','2026-04-28 12:29:41.119182');

--
-- Table structure for table `user_privilege_action`
--

DROP TABLE IF EXISTS `user_privilege_action`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_privilege_action` (
  `user_privilege_action_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_privilege_id` int(11) NOT NULL,
  `performed_by_user_id` char(36) NOT NULL,
  `action` longtext NOT NULL,
  `notes` longtext DEFAULT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`user_privilege_action_id`),
  KEY `ix_user_privilege_action_user_privilege_id` (`user_privilege_id`),
  KEY `ix_user_privilege_action_performed_by_user_id` (`performed_by_user_id`),
  CONSTRAINT `fk_user_privilege_action_user_performed_by_user_id` FOREIGN KEY (`performed_by_user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_user_privilege_action_user_privilege_user_privilege_id` FOREIGN KEY (`user_privilege_id`) REFERENCES `user_privilege` (`user_privilege_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_privilege_action`
--

INSERT INTO `user_privilege_action` VALUES (1,1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','seed_user_privilege_action_action',NULL,'2026-04-28 12:29:41.240862','2026-04-28 12:29:41.239955');

--
-- Table structure for table `user_token`
--

DROP TABLE IF EXISTS `user_token`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_token` (
  `user_token_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `token` varchar(2000) NOT NULL,
  `refresh_token_hash` varchar(256) NOT NULL,
  `expiry_date` datetime(6) NOT NULL,
  `refresh_token_expiry_date` datetime(6) NOT NULL,
  `is_revoked` tinyint(1) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  `device_name` varchar(100) DEFAULT NULL,
  `ip_address` varchar(45) DEFAULT NULL,
  `last_active` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `user_agent` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`user_token_id`),
  KEY `ix_user_token_user_id` (`user_id`),
  CONSTRAINT `fk_user_token_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=65 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_token`
--

INSERT INTO `user_token` VALUES (1,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiN2ViNTQ0ODgtOWE0ZC00MDc0LTk1YTUtZjc3OThiNDRjNGZjIiwiaWF0IjoxNzg2OTY3MjE2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg2OTcwODE2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.16VGeK6HN3NWmmiYrYiLtcrz2KNYmWD3w8aIpnKVKic','gX9AMtmnrmIr14fSZSHuAkpicaY3tByQO7g5qZami+U=','2026-08-17 12:46:56.353418','2026-08-24 11:46:56.353420',0,'2026-04-28 12:29:41.186471','2026-08-17 11:46:56.922455',NULL,NULL,'0001-01-01 00:00:00.000000',NULL),(2,'08dea965-515c-460c-8615-637eaaf1aae7','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTk2NS01MTVjLTQ2MGMtODYxNS02MzdlYWFmMWFhZTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJNYW5hZ2VyIiwic3ViIjoibWFuYWdlciIsImp0aSI6Ijc2MzhkOWYzLWNhZjItNDA2ZS05NTZkLTlkOWFlOWRmMzRkZCIsImlhdCI6MTc4NjQ1MjkwOSwicGVybSI6WyJjYXNoLnJlYWQiLCJjYXNoLndyaXRlIiwiaW52ZW50b3J5LnJlYWQiLCJpbnZlbnRvcnkud3JpdGUiLCJwYXltZW50cy5yZWFkIiwicHJpY2luZy5yZWFkIiwicHJpY2luZy53cml0ZSIsInJlcG9ydHMucmVhZCJdLCJleHAiOjE3ODY0NTY1MDksImlzcyI6IlN0b3JlQVBJIiwiYXVkIjoiU3RvcmVDbGllbnRzIn0.Jec7yoMwIcJwWUKblshJgPZsPI83TSw66e58Rivj4-U','ffTJrx08a+5VBwXrhuAgm6wrLfn8Ac2wJRmMGUr9iA4=','2026-08-11 13:55:09.055867','2026-08-18 12:55:09.055867',0,'2026-05-04 00:12:53.815950','2026-08-11 12:55:09.130016',NULL,NULL,'0001-01-01 00:00:00.000000',NULL),(3,'08dea965-529d-495c-8b9f-bd9147a02f23','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTk2NS01MjlkLTQ5NWMtOGI5Zi1iZDkxNDdhMDJmMjMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDYXNoaWVyIiwic3ViIjoiY2FzaGllciIsImp0aSI6IjE2ZGZjZDU4LWQ3N2EtNDM0My1iYjExLWEyODc0M2Y1YWJjOCIsImlhdCI6MTc4NjQzODA5NCwicGVybSI6WyJjYXNoLnJlYWQiLCJjYXNoLndyaXRlIl0sImV4cCI6MTc4NjQ0MTY5NCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.Ks-ltl4z9tG_VEfF_LvKsJn9Eaa5lS55WoVb_cx6no8','VFyuu4ovhFe+ab/4WiQ5b+ReX6nl00khonqjb0JOOlA=','2026-08-11 09:48:14.715273','2026-08-18 08:48:14.715274',0,'2026-05-04 00:14:21.445553','2026-08-11 08:48:14.770599',NULL,NULL,'0001-01-01 00:00:00.000000',NULL),(4,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOWU4OWJiZWMtYWQwMS00Y2Q3LTkyN2UtNDcwMjk4NzRjOWRlIiwiaWF0IjoxNzg2OTc1NDYwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg2OTc5MDYwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.JTPJBDZ6ORCdCCxEk1kEAuV9mklB6eTuDPlWdNqUDWo','ICyQIPd6s9uK4Xc0Uk1UtTVaTs16exIXtM44RhkS0Cg=','2026-08-17 15:04:20.506263','2026-08-24 14:04:20.506265',0,'2026-08-17 14:04:21.174501','2026-08-17 14:04:21.104355',NULL,'::1','2026-08-17 14:04:20.723899',''),(5,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZTE5OGE3NWMtMmI2Yy00ZjZhLTk3NGUtZTY2ZmVhNGZjYjM3IiwiaWF0IjoxNzg3MDMzMTA5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3MDM2NzA5LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.kX_Cho0yPpCl1Q2VKYyM4JkiPJVeMBKEBi-CsYrK6G4','sTAKWNsGrM78n9z2HstPv+j1P/idulfu8MP32C4+6zA=','2026-08-18 07:05:09.164970','2026-08-25 06:05:09.164972',0,'2026-08-18 06:05:09.625864','2026-08-18 06:05:09.614288',NULL,'::1','2026-08-18 06:05:09.251396',''),(6,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiY2E2ZTkzZWQtYmNiOS00NGY1LTkyYWEtN2IwMjA4YzRlZDUxIiwiaWF0IjoxNzg3NTc4Mjc5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3NTgxODc5LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.czfzNUQFjjPbLnQxf_1YEAgTXV49GMUJLX_KvtOE4-4','csfV3R653xsGREsqHiSOSMlu+lk5A7wwOgnVcSgKNZk=','2026-08-24 14:31:19.315035','2026-08-31 13:31:19.315045',0,'2026-08-24 13:31:19.478144','2026-08-24 13:31:19.472866',NULL,'::1','2026-08-24 13:31:19.365176',''),(7,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiYzk2NTM0NzYtYWI1My00OWY2LWJhNDUtNmE3NjY4MWE5MmYwIiwiaWF0IjoxNzg3NTgyNjMyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3NTg2MjMyLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.oO3TqC3UHwCoKrwn8gegNKIfQIioHR9lfzDLaNr1ihI','3sKxS/g3P8PGzDtAOfufo57o18vueRr2auODXcfPZv4=','2026-08-24 15:43:52.392506','2026-08-31 14:43:52.392509',0,'2026-08-24 14:43:52.406520','2026-08-24 14:43:52.406496',NULL,'::1','2026-08-24 14:43:52.399019',''),(8,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOGQxYjc3NWItNWVhOC00OTU5LThjOGUtOTFlZmQ1YTc0YzE0IiwiaWF0IjoxNzg3NTg3NjU3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3NTkxMjU3LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.-fBqhxJUpfllpxeycwkrSBRkqOhL5pMEjvuP6uwlDT0','uhYf4Z96Wfe2bVX3/2tQPu/VpjTTWGkw8uQkBPmwpGs=','2026-08-24 17:07:37.381043','2026-08-31 16:07:37.381106',0,'2026-08-24 16:07:37.418830','2026-08-24 16:07:37.418673',NULL,'::1','2026-08-24 16:07:37.392921',''),(9,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiY2U5ZGE1ZWEtNmEwMS00NjZhLTgxMGMtOGE3YjFiZWFmMzEyIiwiaWF0IjoxNzg3NTg4NzgyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3NTkyMzgyLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.KdnGDI4guH1SFN-lRZCh8KObwHMoRGLJ2Yc0IQY7Fvo','1yTX22VTwqcypMOwcvK5r443jaP5MZAZnc2G6jrhQr8=','2026-08-24 17:26:22.537454','2026-08-31 16:26:22.537456',0,'2026-08-24 16:26:22.722938','2026-08-24 16:26:22.717657',NULL,'::1','2026-08-24 16:26:22.600803',''),(10,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDljNTZlMDYtZjkxZS00MWNlLWFjZjMtNzIzZWU5YjkxYWJiIiwiaWF0IjoxNzg3ODMwOTIwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODM0NTIwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.fc_wcbzzqUY6kPuYKLInIVLGplg-J0GC6gQGLVi4TrU','IxQFNNPFgKgYlQZSOhwLSmyzfdHJ1DCmI/8KTqpFAjc=','2026-08-27 12:42:00.296088','2026-09-03 11:42:00.296089',0,'2026-08-27 11:42:00.411266','2026-08-27 11:42:00.408746',NULL,'::1','2026-08-27 11:42:00.330207',''),(11,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOWNmZjVlMWUtOTJkMi00OWQxLTljZDctYWE4MmQyYzIyMzJiIiwiaWF0IjoxNzg3ODM5NTU2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODQzMTU2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.JKT1Zi31g9KSpjjtpz21xolRrKes3NTkjq4Ubq4lq9I','nOZrygeEfhbbKJ4CN6wHccDeEONM3X/vJrL9EUQyAPc=','2026-08-27 15:05:56.055082','2026-09-03 14:05:56.055098',0,'2026-08-27 14:05:56.067523','2026-08-27 14:05:56.067436',NULL,'::1','2026-08-27 14:05:56.058063',''),(12,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDUxMGM4ZjItMWM5Zi00ZmI4LTk0ZDUtMTVjOTU0YzQ3OGE2IiwiaWF0IjoxNzg3ODQxNDIxLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODQ1MDIxLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.YUJEgXui4K03OCj2vxG8nEXDagTXeK7fd_65icTNWTI','18NWwoXc72LvyBgiQ5ffiwDyNfvSgaLNUU8rfIRqOIE=','2026-08-27 15:37:01.115232','2026-09-03 14:37:01.115233',0,'2026-08-27 14:37:01.271538','2026-08-27 14:37:01.267694',NULL,'::1','2026-08-27 14:37:01.146165',''),(13,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDYxYTE5ZGUtN2ZiOC00MTFjLWFhNjUtYzM1YjgxMzg0OTkxIiwiaWF0IjoxNzg3ODQ0NzgzLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODQ4MzgzLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.KIoJD2xxjZw-8FvLpRl0f_ZKzIsTOQoo68ctmvQts4M','87WYVH6ry5311PwdZLkTeV10lcEzxLgjZU8XiuEIId4=','2026-08-27 16:33:03.324924','2026-09-03 15:33:03.324925',0,'2026-08-27 15:33:03.414628','2026-08-27 15:33:03.408026',NULL,'::1','2026-08-27 15:33:03.349637',''),(14,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNjkyNmRhZjktM2FiNS00MGVlLTlmODMtMjc1N2MyNDg4ODg1IiwiaWF0IjoxNzg3ODQ2NjczLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODUwMjczLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.QYm3YWa1N5v0tZd3JmgLAOMv2kJvRYGoiymRG04oHNE','CYCIHJEg9/jiiy3/FQ5/NcbiwnSi5v83gASERRHrLUg=','2026-08-27 17:04:33.237842','2026-09-03 16:04:33.237843',0,'2026-08-27 16:04:33.382233','2026-08-27 16:04:33.378600',NULL,'::1','2026-08-27 16:04:33.282735',''),(15,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNGUwYjMzMDUtMzA5Ni00ZTMzLTk5YjItZTM5YTQ5ZDk5OWM4IiwiaWF0IjoxNzg3ODUxMTY5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODU0NzY5LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.qHd8vbqRgNVtV814ULMiIb4kuRBDQtz4HK3edZcnjlY','Boy5acW01dfMWHClluK1xS99ztniLqfHEYCrUMg9gwY=','2026-08-27 18:19:29.284762','2026-09-03 17:19:29.284763',0,'2026-08-27 17:19:29.287651','2026-08-27 17:19:29.287627',NULL,'::1','2026-08-27 17:19:29.285403',''),(16,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNTM3MzQ5ZTgtM2QzOC00Y2M0LTg4NTktZTdiYjljOWNhNmQ0IiwiaWF0IjoxNzg3ODUzNDA4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODU3MDA4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.gdxsdexXMVipkzl2BkCfC-3UAvcI4e8V98JgoIQiC-w','AXs0iBXrcOc3ZyzpTA5lsaASne7RcI1U+xm8CL4YCXg=','2026-08-27 18:56:48.529403','2026-09-03 17:56:48.529405',0,'2026-08-27 17:56:48.531670','2026-08-27 17:56:48.531658',NULL,'::1','2026-08-27 17:56:48.529708',''),(17,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNTQ1NTUyMTEtMzYxOC00YWQ5LWFmMmMtYzRmNWQ2ZWI5Y2RjIiwiaWF0IjoxNzg3ODUzOTA4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODU3NTA4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.TY7W6q81kK3pCXT-tA7vgHC_gCnG-kZf0XEkG1g-SH4','208DX+xRuHkbl63djwvZN/cwv4tRn0gh8f5b9KLAXcA=','2026-08-27 19:05:08.696490','2026-09-03 18:05:08.696491',0,'2026-08-27 18:05:08.784698','2026-08-27 18:05:08.782333',NULL,'::1','2026-08-27 18:05:08.717904',''),(18,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZWU1NTA5MTAtYzI0OS00N2E3LThlMjEtZGJkZjUxNDA3YmI1IiwiaWF0IjoxNzg3ODU2Mzk0LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODU5OTk0LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.YzYwFzlKPuvWkp9CzSDGxNqIkfew4RL2kdO2_nVusYo','aSWP6TcS/SJ0/RT3LcSBdQ73A6Nne8ZugJK0/Y0urUk=','2026-08-27 19:46:34.726182','2026-09-03 18:46:34.726183',0,'2026-08-27 18:46:34.808879','2026-08-27 18:46:34.806525',NULL,'::1','2026-08-27 18:46:34.745915',''),(19,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMWQ4YjVlOGEtNjkyYi00ZGU1LWJiZTktMGNhMjZlZTRmYzMwIiwiaWF0IjoxNzg3ODU4OTczLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODYyNTczLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.3m4CcHITtQLBfkFT0CN5cdmdvYPMfosZZsJGwwgJmS4','Jge6mRTOoL+tfJLq3attIv/HfAlqklQCOtVWFO1MXcE=','2026-08-27 20:29:33.249410','2026-09-03 19:29:33.249412',0,'2026-08-27 19:29:33.251461','2026-08-27 19:29:33.251447',NULL,'::1','2026-08-27 19:29:33.249902',''),(20,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNjQ3NThiYWMtOTliMi00ZjI4LTkzYTAtNzVmNTEwNWE4NzMxIiwiaWF0IjoxNzg3ODYwNDMwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODY0MDMwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.J5vOGGEntlMRo4sHPCuPp1h0Objn3gEeyFO-xFrES-s','b+JVcxNZAQ7QMccGL1DjYEtrk+B5Y4DsDAYDlhUtaFQ=','2026-08-27 20:53:50.513502','2026-09-03 19:53:50.513503',0,'2026-08-27 19:53:50.604029','2026-08-27 19:53:50.597426',NULL,'::1','2026-08-27 19:53:50.538549',''),(21,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMTkzODg3OTgtNTU5Yi00OTE4LWEwODUtNmMxNzBjN2Y2NDgyIiwiaWF0IjoxNzg3ODYzNzkzLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODY3MzkzLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.JN6CPfSwDwOKqWWqogYDKOezMP77p5z88556zO7HZp4','+ndMv+Na1GJNgA1TxPFmsygm+lAWCHFdzp2CPFJdaMo=','2026-08-27 21:49:53.981267','2026-09-03 20:49:53.981268',0,'2026-08-27 20:49:54.121028','2026-08-27 20:49:54.113380',NULL,'::1','2026-08-27 20:49:54.016609',''),(22,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiY2RlMTdjMmQtMWY3OC00NWJjLWFlNWEtNzg4MWI5NDFjMWRlIiwiaWF0IjoxNzg3ODY1OTA4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODY5NTA4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.7WiN_MnP5iUPJyB-uCZFvbX-EDqZG1lJl9X2I7z_G3w','Ld9qmfjumHLjuq8aiVFJA6l7h7yqWrlJ06pNxSv1dBI=','2026-08-27 22:25:08.260433','2026-09-03 21:25:08.260435',0,'2026-08-27 21:25:08.265239','2026-08-27 21:25:08.265208',NULL,'::1','2026-08-27 21:25:08.260931',''),(23,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZWExMDcxZTQtOTVmMi00YmRkLTk0ZDQtZmMzNjE2ZTAzZWJjIiwiaWF0IjoxNzg3ODY4MTEwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3ODcxNzEwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.g4RdKy71SZ9sVKmfgjVpEuofjuf8O1h3GQ9SEiLqwmA','TPMIcgJlQneNkOkE5vz0I9oJ/2F7FbVudZDRIGvqObg=','2026-08-27 23:01:50.793556','2026-09-03 22:01:50.793558',0,'2026-08-27 22:01:50.935166','2026-08-27 22:01:50.930723',NULL,'::1','2026-08-27 22:01:50.830103',''),(24,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiN2U1OWY3NTUtYjBmZi00YTEyLTkyZmYtYjJlYTY1N2IxYzM2IiwiaWF0IjoxNzg3OTAwNjU0LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTA0MjU0LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.4YsMBTmD-oWVvflRpTv-WAouRX1fDlSiBYVneQMkg3A','o6VbJ8RF/z1j4aFrT59okSNps2FEEbY+XdyiwSZKASU=','2026-08-28 08:04:14.415565','2026-09-04 07:04:14.415568',0,'2026-08-28 07:04:14.449996','2026-08-28 07:04:14.449931',NULL,'::1','2026-08-28 07:04:14.417114',''),(25,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZWY4NGIwMzItNjMzZC00NWNmLWE4ZDQtMWZjMmM3Y2U0ZjI0IiwiaWF0IjoxNzg3OTAwODA4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTA0NDA4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.IUxG5Vniuy98yck0oWc9XuqM83WroSZ__SJLuQ3LKJM','HGRjMj6I58b+g+jYjA/gWacSI4TxAlF9Q7n8INKJsT8=','2026-08-28 08:06:48.633747','2026-09-04 07:06:48.633749',0,'2026-08-28 07:06:48.761519','2026-08-28 07:06:48.755806',NULL,'::1','2026-08-28 07:06:48.666382',''),(26,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiODNiMTI4ODUtNjRjNi00MzllLWE1ZmQtYmZlNDQ4ZjU4YjE5IiwiaWF0IjoxNzg3OTAzNTQxLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTA3MTQxLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.Zf70nwX2sJ5WSTv0LmYt7hEEsY_nz7nPQTemrut_I8s','IFX6S6WfS3robGqRs/ewRfxC5Qv/IqF8lMsMKUXC1EI=','2026-08-28 08:52:21.109204','2026-09-04 07:52:21.109204',0,'2026-08-28 07:52:21.239319','2026-08-28 07:52:21.233207',NULL,'::1','2026-08-28 07:52:21.140905',''),(27,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZTNlYzVkMmUtNWQxOS00ODlhLWIwYzItMGE3MmJlNGEzZWQzIiwiaWF0IjoxNzg3OTA2NDYwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTEwMDYwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.fmPHuXOkH4h7-7UXEUyQNF1bS_LLKbqV8XvlAzYmBxQ','Tayh5GKkYFR5w04bDyfkjYvIHyy0oEdHLkAXVq+oSxo=','2026-08-28 09:41:00.503089','2026-09-04 08:41:00.503090',0,'2026-08-28 08:41:00.633215','2026-08-28 08:41:00.628841',NULL,'::1','2026-08-28 08:41:00.534000',''),(28,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDg2MjQ3MTEtNWQ2My00MGY0LWEzNDUtN2RiNjU4MDI1OGI0IiwiaWF0IjoxNzg3OTA4NzA2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTEyMzA2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.xeaE-ZcH8VT2eHFkxIbDm37wPKhDMJWRpPnVIJFW9ss','aWZr/FMIgmFZPOotKOulUgWsV8nZqjLWaiDfzX3mWcg=','2026-08-28 10:18:26.990596','2026-09-04 09:18:26.990598',0,'2026-08-28 09:18:26.998832','2026-08-28 09:18:26.998796',NULL,'::1','2026-08-28 09:18:26.991373',''),(29,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZGVlNDJlMDItMDU2Mi00OGE5LWE0NDEtM2JkZDUyMjVlZGQ4IiwiaWF0IjoxNzg3OTEwMDc3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTEzNjc3LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.dKAvxa2DCMFskZj-Ofkno_Gb-r1EszcdjmlB1AnYuvE','oyhhqCfN7LoDYwl2R/TxuMotxQ8aVqMVslgQqOpzOP8=','2026-08-28 10:41:17.957894','2026-09-04 09:41:17.957894',0,'2026-08-28 09:41:18.051794','2026-08-28 09:41:18.045283',NULL,'::1','2026-08-28 09:41:17.984846',''),(30,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDliZmYwMDAtNzE1Mi00ZTdkLWFjMzUtOWM1YTg5MzNmYjE0IiwiaWF0IjoxNzg3OTExNTM2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTE1MTM2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.xROtioZ44TplhYUV2FYxhjef57RBsszzgV4C7scSSXQ','ouqVgUoTGh4fGg4dp+QYC7y9gQf3w3frgO45+huxUJY=','2026-08-28 11:05:36.352095','2026-09-04 10:05:36.352096',0,'2026-08-28 10:05:36.357119','2026-08-28 10:05:36.357076',NULL,'::1','2026-08-28 10:05:36.352719',''),(31,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZDQzZGM0MDEtYzNhYi00YjBlLWE4ZWMtMDdiYTBjN2JlMDFiIiwiaWF0IjoxNzg3OTEzOTI1LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTE3NTI1LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.X8Fu1PZ60b8luIavrf-QsJBYyeVOpojMM0tSx24bEWc','eSw+F7Kca0lnagGqKm5aqTGo51CM4ZFXySssmO+UR2k=','2026-08-28 11:45:25.194742','2026-09-04 10:45:25.194743',0,'2026-08-28 10:45:25.312389','2026-08-28 10:45:25.302785',NULL,'::1','2026-08-28 10:45:25.217530',''),(32,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZmY2MmMxMzktNGY0My00ZDg5LWFmNGEtMTFlZmZlZDNiYTEzIiwiaWF0IjoxNzg3OTE0OTc5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTE4NTc5LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.SuEiL2cG4gXH2YOY2Ojd9QYhHRi4Sb3xJgg9rJVv-Uk','UbpntznhwtnADTqjmO3Gv3mJWkUM0sAnYqU9KlGHLLo=','2026-08-28 12:02:59.345821','2026-09-04 11:02:59.345822',0,'2026-08-28 11:02:59.435216','2026-08-28 11:02:59.432785',NULL,'::1','2026-08-28 11:02:59.368151',''),(33,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMWRmNThkMjQtNDFlZC00NTE3LTk1ZjMtMjI1OGNmYTdjMzk2IiwiaWF0IjoxNzg3OTE1NzYzLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTE5MzYzLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.eeavPBaRP6P3IAW1eJ-QXgcbrRIXWFA6zgUzfrPlx0I','EBeK/RRCgo+WmpvRm+KMnZW26Hb1tTGoiLX2F6Sn/Vs=','2026-08-28 12:16:03.477471','2026-09-04 11:16:03.477472',0,'2026-08-28 11:16:03.567966','2026-08-28 11:16:03.565528',NULL,'::1','2026-08-28 11:16:03.498985',''),(34,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOGIyZTMwODgtMGNmZC00ZDc4LWI5ZjEtNWE0ZWQwOTA5OWUwIiwiaWF0IjoxNzg3OTE3OTE4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTIxNTE4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.RH7N-kLTuB0HWFWHvYpxsQJXxMZofrA3sirr7GMIMBs','FPu54hTQn2SilY4VIXBfPGBQzUNSc2PijR6HOV4vFiw=','2026-08-28 12:51:58.760830','2026-09-04 11:51:58.760831',0,'2026-08-28 11:51:58.852249','2026-08-28 11:51:58.849616',NULL,'::1','2026-08-28 11:51:58.787913',''),(35,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiYzdkMTNjYWMtN2Q0Ni00OTVlLTk2YzItNTBlYWJmM2IxYmE1IiwiaWF0IjoxNzg3OTI3Mzc3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTMwOTc3LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.xanINjYDs9AErmExbg1C6uB5RQTGXBCYQVlkUkh_jo4','d+Eej7VFSEeX00g9kFhVpobNm2s+DC6ql5iLkPROPLY=','2026-08-28 15:29:37.555481','2026-09-04 14:29:37.555482',0,'2026-08-28 14:29:37.955731','2026-08-28 14:29:37.943285',NULL,'::1','2026-08-28 14:29:37.706377',''),(36,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiYTAyY2QyZGItYjdkZS00MTdjLWJhYTMtYmZhYjkwM2IwZjE3IiwiaWF0IjoxNzg3OTI4NjQ4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTMyMjQ4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.YG8D6m0zrktPepRkf_EBNIjV-d_Ma9QkvaJyJl79hzE','gR0vpztbq6QIZRhXYDVE1rXy9wSaxfQHVAZ4r/kJZhc=','2026-08-28 15:50:48.072049','2026-09-04 14:50:48.072050',0,'2026-08-28 14:50:48.160290','2026-08-28 14:50:48.157867',NULL,'::1','2026-08-28 14:50:48.094650',''),(37,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNWU4Nzc1M2UtODJmYy00ODE2LWFmMTItNmFkZmE4MmNhZDg5IiwiaWF0IjoxNzg3OTI5NTU1LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTMzMTU1LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.cJln7s0nNRVAtd6J94QImf_0jyGew7EIX8Bc3SB1KKk','5iFy1AKdKoF/3HlBx0YvjknugQfuXm8RWJadArxr3hs=','2026-08-28 16:05:55.728819','2026-09-04 15:05:55.728820',0,'2026-08-28 15:05:55.859364','2026-08-28 15:05:55.856434',NULL,'::1','2026-08-28 15:05:55.763443',''),(38,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiODkyYmM2OWQtZDE3Ni00OGNkLWI2ZjgtMGQ1ZDc2NWFkMWViIiwiaWF0IjoxNzg3OTMwNDc5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTM0MDc5LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.IHieTE6JUfLQAyGzWkS6-0voUodKVpQoJTiSUjhJ75Y','yxK2sf5vryWXudGmKW35TAG9bxs1MhT3yqBbZcZHcqQ=','2026-08-28 16:21:19.296084','2026-09-04 15:21:19.296086',0,'2026-08-28 15:21:19.455911','2026-08-28 15:21:19.447598',NULL,'::1','2026-08-28 15:21:19.343292',''),(39,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiY2QzMzVjNmQtYzc5Zi00MDI5LTliNDYtZDE5MTYyMzdjOGI3IiwiaWF0IjoxNzg3OTMzODg1LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTM3NDg1LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.wQ5axcJ4988McOVT_5UCAZO5OGPxgX_kvT3Okdl38ME','92ddWqKLMcGMV5ksQGO/b1d67sXiIOe3dpCee5wLi+k=','2026-08-28 17:18:05.347799','2026-09-04 16:18:05.347800',0,'2026-08-28 16:18:05.353912','2026-08-28 16:18:05.353886',NULL,'::1','2026-08-28 16:18:05.349227',''),(40,'08dea965-529d-495c-8b9f-bd9147a02f23','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTk2NS01MjlkLTQ5NWMtOGI5Zi1iZDkxNDdhMDJmMjMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDYXNoaWVyIiwic3ViIjoiY2FzaGllciIsImp0aSI6ImZlYjg2OWFiLWE2M2UtNDVhMy04NjYzLTA5YmMwMzllYjhhNyIsImlhdCI6MTc4NzkzNDIzMywic3RhbXAiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDAiLCJwZXJtIjpbImNhc2gucmVhZCIsImNhc2gud3JpdGUiXSwiZXhwIjoxNzg3OTM3ODMzLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.ZNTcyEZc5y89GATHEhQ_2BxuJLrXWjtAj2oxi2lU-nA','XiZaq9R/UgqG80tNvWGtLjMuHSB9a1letkdECe226JM=','2026-08-28 17:23:53.754199','2026-09-04 16:23:53.754200',0,'2026-08-28 16:23:53.755198','2026-08-28 16:23:53.755190',NULL,'::1','2026-08-28 16:23:53.754561',''),(41,'08dea965-529d-495c-8b9f-bd9147a02f23','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTk2NS01MjlkLTQ5NWMtOGI5Zi1iZDkxNDdhMDJmMjMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDYXNoaWVyIiwic3ViIjoiY2FzaGllciIsImp0aSI6IjA3ZDRlYWFmLThjMTAtNGU4Ni1iMDBjLTE3OTJhYWMxNTI5NyIsImlhdCI6MTc4NzkzNTk1MSwic3RhbXAiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDAiLCJwZXJtIjpbImNhc2gucmVhZCIsImNhc2gud3JpdGUiXSwiZXhwIjoxNzg3OTM5NTUxLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.EKemJuriLFe1t5OptENGssp0ANYQSrOmSUDbvvu9zRo','ajv1BpkRmFASf66Mio4qBxl232KnE99G4NSe2oDb/DM=','2026-08-28 17:52:31.169957','2026-09-04 16:52:31.169958',0,'2026-08-28 16:52:31.170862','2026-08-28 16:52:31.170852',NULL,'::1','2026-08-28 16:52:31.170326',''),(42,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMmM4ZTcxMjgtNzhmNy00ZjRkLWEyNmEtNGE1NjU2N2FiNGI0IiwiaWF0IjoxNzg3OTM1OTY2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTM5NTY2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.qDbMdk2R4aK5PRl53PtyEpSk7suELNFQO8RRmmLunD4','Di96aTrbBylk0NiT+E8I4lu2WtulRK65W68itBiBEZw=','2026-08-28 17:52:46.433393','2026-09-04 16:52:46.433394',0,'2026-08-28 16:52:46.434215','2026-08-28 16:52:46.434201',NULL,'::1','2026-08-28 16:52:46.433620',''),(43,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiM2MxZTQ1NWItNDljYS00MmZmLWFmYzMtYmQ3MzAxMTgxMzI4IiwiaWF0IjoxNzg3OTQwNjQ4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTQ0MjQ4LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.hV5u-aV6W1GJoFwOP-ll2ojZG7lJi02vvooFjaA1_m0','P4+S/auVxqGP2hPWAxFbzyoqmyGFeiq4ZJZENMNPuZk=','2026-08-28 19:10:48.307833','2026-09-04 18:10:48.307834',0,'2026-08-28 18:10:48.434203','2026-08-28 18:10:48.431428',NULL,'::1','2026-08-28 18:10:48.345747',''),(44,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMmFkYWNhMzktZGRmYS00NDkxLThiYzAtZGVkYjA5MDM1YzdiIiwiaWF0IjoxNzg3OTQyNTgwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTQ2MTgwLCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.W9oKvI0w1hjan-APYQQ0sxR5jS_8AHZYmXB0J8wLgqc','oRhG0egnOKcqAbi01H1Utp39fFWINXPkCf1+ZzeT5Fw=','2026-08-28 19:43:00.842513','2026-09-04 18:43:00.842516',0,'2026-08-28 18:43:00.852367','2026-08-28 18:43:00.852331',NULL,'::1','2026-08-28 18:43:00.843620',''),(45,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiYWQxYjEzM2ItZmJhZi00MDkxLWI0NzktY2VjOTQ1NjhhMzU4IiwiaWF0IjoxNzg3OTQzNTc2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg3OTQ3MTc2LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9._UhvdgqJqIhyo86l75DxbJnF5_tlo7M3uEJ1He7mkGg','Tw9Y1+f1s1smmK5glSuxFWYnWB+IMSPX0HBDtw9JkMA=','2026-08-28 19:59:36.257822','2026-09-04 18:59:36.257825',0,'2026-08-28 18:59:36.260077','2026-08-28 18:59:36.260061',NULL,'::1','2026-08-28 18:59:36.258409',''),(46,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiM2RhZjJmMTItMWYxOC00OGJlLWI2Y2MtZDc4ZjJlNTQ3MTFhIiwiaWF0IjoxNzg4MDIxNTg3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg4MDI1MTg3LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.SNBvLVCnW6GilDZ1KUT9rxku4bmjNIeUSZSaxulk2zU','6wWvEnLWMf+AMaBO1NYqTErv4Ac6K8F2SFsa0Mgg3/Y=','2026-08-29 17:39:47.589675','2026-09-05 16:39:47.589686',0,'2026-08-29 16:39:47.993840','2026-08-29 16:39:47.988103',NULL,'::1','2026-08-29 16:39:47.721115',''),(47,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOGI5YmQyNTUtZDI1OC00ODFhLWIwYjItMDY2MjQzYTk5ZjQ4IiwiaWF0IjoxNzg4MTg2NTE3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiY2FzaC5yZWFkIiwiY2FzaC53cml0ZSIsImludmVudG9yeS5yZWFkIiwiaW52ZW50b3J5LndyaXRlIiwicGF5bWVudHMucmVhZCIsInByaWNpbmcucmVhZCIsInByaWNpbmcud3JpdGUiLCJyZXBvcnRzLnJlYWQiXSwiZXhwIjoxNzg4MTkwMTE3LCJpc3MiOiJTdG9yZUFQSSIsImF1ZCI6IlN0b3JlQ2xpZW50cyJ9.yGHS5TB1KQo5uQfCSn0SGVGpbj7nvFlNUv89KxUbEZk','R3gTIvFzLICVNRvGw7+xLbf8XKv7XrhhRMUBgdTA5gA=','2026-08-31 15:28:37.418002','2026-09-07 14:28:37.418004',0,'2026-08-31 14:28:37.749329','2026-08-31 14:28:37.737961',NULL,'::1','2026-08-31 14:28:37.445148',''),(48,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDIzZmQ5NDgtNWFmZS00NTViLTkwMTMtMGFhY2IyNThjMmNkIiwiaWF0IjoxNzg4MTk5NTE0LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIwMzExNCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.sT7r5oZTQLCw4-AkUSpsgzfkCk4kfNLSXtwediAN1Yk','bapVF0S2ZhSe6Tgpf7KCKjLFn501FKbxjARwPVGFUnc=','2026-08-31 19:05:14.780891','2026-09-07 18:05:14.780893',0,'2026-08-31 18:05:15.042632','2026-08-31 18:05:15.031739',NULL,'::1','2026-08-31 18:05:14.855665',''),(49,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNjRjYTg0MDktMjBlMi00ODk5LThmZTktNjEzM2M4ODk3ZmQ0IiwiaWF0IjoxNzg4MjAxMDUzLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIwNDY1MywiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.HaI30E1SIBJtH-XcO3628qUQpfXtfGNVJ6oX7zGuFnI','g5B6cV+nZ+4YrEjaw7QVZPtQFBkn0NqLQAJTf2L48sg=','2026-08-31 19:30:53.498077','2026-09-07 18:30:53.498078',0,'2026-08-31 18:30:53.633313','2026-08-31 18:30:53.630126',NULL,'::1','2026-08-31 18:30:53.535180',''),(50,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZjlmNTYyMzAtMWZmMy00ZGY5LWEwYjEtYjkyNDZmNTJhY2Y5IiwiaWF0IjoxNzg4MjAxNjcwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIwNTI3MCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.QCTTadpqbqq37_dKg_J5Ug0Aq9qRxaqg3zCFuypYzhI','lGxGPCSsBDP9dyUPmlFkNbC7bVIqY9KlZAwYOv3kTds=','2026-08-31 19:41:10.990098','2026-09-07 18:41:10.990100',0,'2026-08-31 18:41:10.998285','2026-08-31 18:41:10.998254',NULL,'::1','2026-08-31 18:41:10.991761',''),(51,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiN2Q3Y2M2MGUtZGI2OC00M2QyLWI0MTQtODlhODU4MDQ0MDlhIiwiaWF0IjoxNzg4MjAyNDQ0LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIwNjA0NCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.SAdnMqcbMdGFBbSuQt1lx7omHJiTMzhL8Y1Dl2hixhM','eQjK8Ghnz8g9L/EAUsV8wUfJHRhrgD18FqBsTY8wKdg=','2026-08-31 19:54:04.420123','2026-09-07 18:54:04.420124',0,'2026-08-31 18:54:04.559336','2026-08-31 18:54:04.554038',NULL,'::1','2026-08-31 18:54:04.454735',''),(52,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNmY3NzMxNjEtMjFlYy00MDI3LTljZDItMTU2YmQ1MzBkMTk5IiwiaWF0IjoxNzg4MjAzMDU3LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIwNjY1NywiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.t7gY0dQXaDf3qGMUcFKCi0A-lFZsxNkJCff1qnG7nkM','Y/NtXNkYLrsVfqcfSQE0r5euS6KG93Rg3w2HEy0XARU=','2026-08-31 20:04:17.108454','2026-09-07 19:04:17.108455',0,'2026-08-31 19:04:17.206325','2026-08-31 19:04:17.204346',NULL,'::1','2026-08-31 19:04:17.143832',''),(53,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNmFiNjU4NzctZGM1Mi00NWNmLWFhNzYtN2U4YTY5Y2YyZGUzIiwiaWF0IjoxNzg4MjA3MTIyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxMDcyMiwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.O6BG6qwWDnZAuXiVmL24FtWObRdHFEB6rDMRPvM_tig','dBUEy8NRXQryVmfgPueVPWb/4y5r1CDy+8PVxaAJGG8=','2026-08-31 21:12:02.557418','2026-09-07 20:12:02.557419',0,'2026-08-31 20:12:02.700799','2026-08-31 20:12:02.698814',NULL,'::1','2026-08-31 20:12:02.608704',''),(54,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOTUxNGFjOGItNTFmZi00ZTAzLThiMjMtZWJjMTI2ZmEyNWU4IiwiaWF0IjoxNzg4MjA3NzU2LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxMTM1NiwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.YoDG24MqOP1ijBXYbDYUAwq1T9sR8FmDiAnWwCKeyng','MAHROf/L4rOu+Iw9MkvePU8Wj7hEkVHq+TNKbRo7eIY=','2026-08-31 21:22:36.692348','2026-09-07 20:22:36.692349',0,'2026-08-31 20:22:36.819028','2026-08-31 20:22:36.816215',NULL,'::1','2026-08-31 20:22:36.730567',''),(55,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMDcwOGNkMjctM2NjYy00YWZjLWExMGUtMDFjOTYxMjdlMjBlIiwiaWF0IjoxNzg4MjA4NjcwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxMjI3MCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.qLZqmMcIVTyDjzS0vPCZAgNsoEOazc3UFxiMeTRgriM','ieq1RZ2n+NgfpVZODWmLnwtfzK/TYzXsyScfX58+QtU=','2026-08-31 21:37:50.925062','2026-09-07 20:37:50.925064',0,'2026-08-31 20:37:50.927201','2026-08-31 20:37:50.927192',NULL,'::1','2026-08-31 20:37:50.925761',''),(56,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiODNhYTc5YjYtMjc2ZC00NDhiLThiYWUtMThmMzkwZTNkZGI2IiwiaWF0IjoxNzg4MjA5MTQ5LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxMjc0OSwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.J9g4bSJdLY9QpzpYkbVAJr_UhAMjGo-eZtL-rBXds1c','kVd+XcwzMQsifsadFadqMkEW8ppRGt6aKRgiNCQGR8I=','2026-08-31 21:45:49.645172','2026-09-07 20:45:49.645175',0,'2026-08-31 20:45:49.646734','2026-08-31 20:45:49.646696',NULL,'::1','2026-08-31 20:45:49.645618',''),(57,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMTBhYmZhNTMtM2JiYS00MzI5LWEyN2UtYmJiZjlhMWZlMmNhIiwiaWF0IjoxNzg4MjA5MTcyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxMjc3MiwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.vFFBAiXrL1V7bLAHR23uA0xZT3rgXKD2EBr5RYvNwdQ','WYLIWTjJP6qYM424Zliq0M9V2cxpb2Do3QKuiE3Usv8=','2026-08-31 21:46:12.372857','2026-09-07 20:46:12.372858',0,'2026-08-31 20:46:12.374085','2026-08-31 20:46:12.374068',NULL,'::1','2026-08-31 20:46:12.373168',''),(58,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMmRkNDIwZjAtNmM2MC00MTdhLTk3YmMtYzlmOWZhY2JkMjM3IiwiaWF0IjoxNzg4MjExODAyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxNTQwMiwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.sAbfjXCfPofroPwD7KxU308pw__N7x3iKlKLgffII6M','QWN0O4yzpf5/s9QMXsWc93NzInR1Iw4XTGpoGCWjMJM=','2026-08-31 22:30:02.359889','2026-09-07 21:30:02.359894',0,'2026-08-31 21:30:02.361819','2026-08-31 21:30:02.361737',NULL,'::1','2026-08-31 21:30:02.360419',''),(59,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiYTZhZDFjOTQtNmNjYy00N2ViLWJkNmQtYzU3MWU5MTZhNjJiIiwiaWF0IjoxNzg4MjE1MTczLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODIxODc3MywiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.PkY6JtHJIZbHAqaHXKrKvXNz48JSI-dnZ-CdA0nnLEo','UAaTnnKa7yXMFelTwPPzRPiEJsCrxG2KGrhQJzY3pUM=','2026-08-31 23:26:13.683641','2026-09-07 22:26:13.683642',0,'2026-08-31 22:26:13.775459','2026-08-31 22:26:13.770440',NULL,'::1','2026-08-31 22:26:13.715342',''),(60,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNGNlZjg4NDMtZWQ4NS00MDczLThmMTYtZjM0NmJkM2JjYjNhIiwiaWF0IjoxNzg4MjYyNDgwLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODI2NjA4MCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.WqXxHoVE0LsdfOvcYbz5K6knRDWCD3dE2WKR7IPJGfk','vwQMx5vlitVF6d/NAiZyE3TSDL2dVoQ3MTeFXm9SX04=','2026-09-01 12:34:40.706438','2026-09-08 11:34:40.706454',0,'2026-09-01 11:34:40.710191','2026-09-01 11:34:40.710112',NULL,'::1','2026-09-01 11:34:40.707377',''),(61,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiNDVhMTg4ZmQtM2ZjYS00ZDliLTkwYjEtOTA5ZDNhZmExYmRiIiwiaWF0IjoxNzg4Mjc2ODc4LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODI4MDQ3OCwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.vMccBlwwVT9gt5jLxkrc5EaaMQA_pa2LRMUy4MkuQgg','EkUw4CdirSXn5xUUrk+GtGRPo5Vgq2BZpZbIi6PvSEY=','2026-09-01 16:34:38.535170','2026-09-08 15:34:38.535172',0,'2026-09-01 15:34:38.851230','2026-09-01 15:34:38.824956',NULL,'::1','2026-09-01 15:34:38.568763',''),(62,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiZDQxYTI5ODItMTcwZS00YTUxLThkN2QtNTEyYjEzMDA3YTNhIiwiaWF0IjoxNzg4MzQ4NjQ1LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODM1MjI0NSwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.rIqj9jupyn__NKIvGigWyAKrCB-7ZYBAN0ieUEch3g8','G3HU2ac6aRTXigb/vO33WwjY79mQiw6CA736w7DBw2E=','2026-09-02 12:30:45.859915','2026-09-09 11:30:45.859917',0,'2026-09-02 11:30:45.918705','2026-09-02 11:30:45.917271',NULL,'::ffff:172.19.0.2','2026-09-02 11:30:45.904113','Mozilla/5.0 (Windows NT 10.0; Microsoft Windows 10.0.26200; en-CM) PowerShell/7.6.5'),(63,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiOGEyYzEwZGItZjE5Yy00OTVkLWI1MzgtODVmMGVhMjhmYWMwIiwiaWF0IjoxNzg4MzUxMzE1LCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODM1NDkxNSwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.Be1MZbz-Mdt3qxKqB4G_yjbgeVrSxhKgzr7Kk0I3e-E','SlUECYrNZ07r7U/F3Y0tajz7PN66A39rgf2CNvWVP1g=','2026-09-02 13:15:15.552668','2026-09-09 12:15:15.552670',0,'2026-09-02 12:15:15.564922','2026-09-02 12:15:15.564831',NULL,'::ffff:172.19.0.2','2026-09-02 12:15:15.559001','Mozilla/5.0 (Windows NT 10.0; Microsoft Windows 10.0.26200; en-CM) PowerShell/7.6.5'),(64,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwOGRlYTUyMS1jZjJhLTRkMWEtOGY3Zi0xMGMwNmIwM2I1NGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInN1YiI6ImFkbWluIiwianRpIjoiMmE5MTU0YjMtMWI0YS00ZjhlLWIxZGMtNWY5NzVlM2UwOWU2IiwiaWF0IjoxNzg4MzUxNDcyLCJzdGFtcCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInBlcm0iOlsiYWRtaW4uYnJhbmNoZXMiLCJhZG1pbi5yb2xlbWF0cml4IiwiYWRtaW4uc2V0dGluZ3MiLCJhZG1pbi51c2VycyIsImNhc2gucmVhZCIsImNhc2gud3JpdGUiLCJpbnZlbnRvcnkucmVhZCIsImludmVudG9yeS53cml0ZSIsInBheW1lbnRzLnJlYWQiLCJwcmljaW5nLnJlYWQiLCJwcmljaW5nLndyaXRlIiwicmVwb3J0cy5yZWFkIl0sImV4cCI6MTc4ODM1NTA3MiwiaXNzIjoiU3RvcmVBUEkiLCJhdWQiOiJTdG9yZUNsaWVudHMifQ.cxIeK7X9eNhE_MMQPSjprJMnhPvVKkCDZcK791leG2c','kgH6H6a+a1W7VKbPXMJSXcVq0Jh76WU8r0JBLrYEbts=','2026-09-02 13:17:52.267145','2026-09-09 12:17:52.267148',0,'2026-09-02 12:17:52.269468','2026-09-02 12:17:52.269446',NULL,'::ffff:172.20.0.4','2026-09-02 12:17:52.268154','');

--
-- Table structure for table `wastage_entry`
--

DROP TABLE IF EXISTS `wastage_entry`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `wastage_entry` (
  `wastage_entry_id` int(11) NOT NULL AUTO_INCREMENT,
  `item_id` char(36) NOT NULL,
  `wastage_type` int(11) NOT NULL,
  `quantity` int(11) NOT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  `reference_code` varchar(100) DEFAULT NULL,
  `recorded_by_user_id` char(36) NOT NULL,
  `date_created` datetime(6) NOT NULL,
  `last_modified` datetime(6) NOT NULL,
  PRIMARY KEY (`wastage_entry_id`),
  KEY `ix_wastage_entry_item_id_date_created` (`item_id`,`date_created`),
  KEY `ix_wastage_entry_recorded_by_user_id` (`recorded_by_user_id`),
  CONSTRAINT `fk_wastage_entry_item_item_id` FOREIGN KEY (`item_id`) REFERENCES `item` (`item_id`),
  CONSTRAINT `fk_wastage_entry_user_recorded_by_user_id` FOREIGN KEY (`recorded_by_user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `wastage_entry`
--

INSERT INTO `wastage_entry` VALUES (1,'08dea521-ced8-464b-86c9-b8499dcfca2e',0,1,NULL,NULL,'08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-05-04 11:39:30.670216','2026-05-04 11:39:30.669019'),(2,'08dea521-cef1-47f3-8aa3-3af000328106',5,13,'Rusting','34987','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-07-10 17:57:14.945969','2026-07-10 17:57:14.944899'),(3,'d77e5326-3f06-44c4-997b-a3eb2889f20c',2,156,'Some stole riceee!','WASTE-202608-4136','08dea521-cf2a-4d1a-8f7f-10c06b03b54a','2026-08-27 18:48:51.293448','2026-08-27 18:48:51.291643');
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-09-02 23:05:25

SET FOREIGN_KEY_CHECKS=1;
SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '');
