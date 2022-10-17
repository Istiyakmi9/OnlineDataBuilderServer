
create database onlinedatabuilder;

use onlinedatabuilder;

-- MySQL dump 10.13  Distrib 8.0.30, for Win64(x86_64)
--
-- Host: 192.168.0.244    Database: onlinedatabuilder
--------------------------------------------------------
-- Server version   8.0.30 - 0ubuntu0.22.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */
;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */
;
/*!40103 SET TIME_ZONE='+00:00' */
;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */
;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */
;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */
;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */
;

--
-- Table structure for table `a`
--

DROP TABLE IF EXISTS `a`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `a` (
  `aa` varchar(64) NOT NULL,
  `ab` varchar(50) NOT NULL,
  `ac` varchar(100) NOT NULL,
  `ad` varchar(14) NOT NULL,
  PRIMARY KEY (`aa`),
  UNIQUE KEY `UQ__A__3213A92835F005F0` (`ad`),
  UNIQUE KEY `UQ__A__3213A92896E57B0F` (`ad`),
  UNIQUE KEY `UQ__A__3213A929D10F6EC5` (`ac`),
  UNIQUE KEY `UQ__A__3213A929D5831267` (`ac`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */
;

--
-- Table structure for table `accessibility_description`
--

DROP TABLE IF EXISTS `accessibility_description`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accessibility_description` (
  `AccessibilityId` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`AccessibilityId`)
) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */
;

--
-- Table structure for table `accesslevel`
--

DROP TABLE IF EXISTS `accesslevel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accesslevel` (
  `AccessLevelId` bigint NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) DEFAULT NULL,
  `AccessCodeDefination` varchar(250) DEFAULT NULL,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`AccessLevelId`)
) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */
;

--
-- Table structure for table `adhoc_detail`
--

DROP TABLE IF EXISTS `adhoc_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `adhoc_detail` (
  `AdHocId` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) DEFAULT NULL,
  `Description` varchar(250) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`AdHocId`)
) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */
;

--
-- Table structure for table `approval_request`
--

DROP TABLE IF EXISTS `approval_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `approval_request` (
  `ApprovalRequestId` bigint NOT NULL AUTO_INCREMENT,
  `UserName` varchar(100) DEFAULT NULL,
  `Message` varchar(500) DEFAULT NULL,
  `UserId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT NULL,
  `RequestedOn` datetime DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `FromDate` datetime DEFAULT NULL,
  `ToDate` datetime DEFAULT NULL,
  `AssigneeId` bigint DEFAULT NULL,
  `ProjectId` bigint DEFAULT NULL,
  `ProjectName` varchar(100) DEFAULT NULL,
  `RequestStatusId` int DEFAULT '2',
  `AttendanceId` bigint NOT NULL,
  `RequestType` int DEFAULT NULL,
  `LeaveType` int DEFAULT NULL,
  `LeaveRequestId` bigint DEFAULT NULL,
  PRIMARY KEY (`ApprovalRequestId`)
) ENGINE = InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */
;
--
-- Table structure for table `athleteevents`
--

DROP TABLE IF EXISTS `athleteevents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `athleteevents` (
  `RowIndex` double DEFAULT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Sex` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Age` double DEFAULT NULL,
  `Height` double DEFAULT NULL,
  `Weight` double DEFAULT NULL,
  `Team` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NOC` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Games` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Year` double DEFAULT NULL,
  `Season` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `City` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Sport` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Event` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Medal` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `attendance`
--

DROP TABLE IF EXISTS `attendance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `attendance` (
  `AttendanceId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT NULL,
  `AttendanceDetail` json DEFAULT NULL,
  `TotalDays` int DEFAULT NULL,
  `TotalWeekDays` int DEFAULT NULL,
  `DaysPending` int DEFAULT NULL,
  `TotalBurnedMinutes` int DEFAULT '0',
  `ForYear` int NOT NULL,
  `ForMonth` int NOT NULL,
  `SubmittedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `SubmittedBy` bigint NOT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`AttendanceId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `b`
--

DROP TABLE IF EXISTS `b`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `b` (
  `ba` varchar(64) NOT NULL,
  `bb` varchar(50) NOT NULL,
  PRIMARY KEY (`ba`),
  CONSTRAINT `FK_B_ba_C_ca` FOREIGN KEY (`ba`) REFERENCES `c` (`ca`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `bank_accounts`
--

DROP TABLE IF EXISTS `bank_accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bank_accounts` (
  `BankAccountId` int NOT NULL AUTO_INCREMENT,
  `OrganizationId` int DEFAULT NULL,
  `CompanyId` int DEFAULT NULL,
  `BankName` varchar(100) DEFAULT NULL,
  `BranchCode` varchar(20) DEFAULT NULL,
  `Branch` varchar(50) DEFAULT NULL,
  `IFSC` varchar(20) DEFAULT NULL,
  `AccountNo` varchar(45) DEFAULT NULL,
  `OpeningDate` datetime DEFAULT NULL,
  `ClosingDate` datetime DEFAULT NULL,
  `PANNo` varchar(20) DEFAULT NULL,
  `GSTNo` varchar(50) DEFAULT NULL,
  `TradeLicenseNo` varchar(50) DEFAULT NULL,
  `IsPrimaryAccount` bit(1) DEFAULT b'0',
  `CreatedBy` mediumtext NOT NULL,
  `UpdatedBy` mediumtext,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`BankAccountId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `billdetail`
--

DROP TABLE IF EXISTS `billdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `billdetail` (
  `BillDetailUid` bigint NOT NULL AUTO_INCREMENT,
  `PaidAmount` decimal(10,0) DEFAULT NULL,
  `BillForMonth` int DEFAULT NULL,
  `BillYear` int DEFAULT NULL,
  `NoOfDays` int DEFAULT NULL,
  `NoOfDaysAbsent` decimal(10,0) DEFAULT NULL,
  `IGST` int DEFAULT NULL,
  `SGST` int DEFAULT NULL,
  `CGST` int DEFAULT NULL,
  `TDS` int DEFAULT NULL,
  `BillStatusId` bigint NOT NULL,
  `PaidOn` datetime(6) DEFAULT NULL,
  `FileDetailId` bigint NOT NULL,
  `EmployeeUid` bigint DEFAULT NULL,
  `ClientId` bigint NOT NULL,
  `BillNo` varchar(45) NOT NULL,
  `UpdateSeqNo` int NOT NULL DEFAULT '0',
  `CreatedBy` bigint NOT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `BillUpdatedOn` datetime NOT NULL,
  `IsCustomBill` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`BillDetailUid`),
  KEY `fk_BillDetail_Createdby` (`CreatedBy`),
  KEY `fk_BillDetail_UpdatedBy` (`UpdatedBy`),
  KEY `fk_EmployeePayroll_EmployeeUid_idx` (`EmployeeUid`),
  KEY `fk_billdetail_FileDetailId_idx` (`FileDetailId`),
  CONSTRAINT `fk_billdetail_FileDetailId` FOREIGN KEY (`FileDetailId`) REFERENCES `filedetail` (`FileId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `bills`
--

DROP TABLE IF EXISTS `bills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bills` (
  `BillUid` bigint NOT NULL,
  `InitBillNo` int DEFAULT NULL,
  `LastBillNo` int DEFAULT NULL,
  `BillNoLength` int DEFAULT NULL,
  `BillTypeUid` bigint DEFAULT NULL,
  PRIMARY KEY (`BillUid`),
  KEY `fk_Bills_BillTypeUid` (`BillTypeUid`),
  CONSTRAINT `fk_Bills_BillTypeUid` FOREIGN KEY (`BillTypeUid`) REFERENCES `billtype` (`BillTypeUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `billtype`
--

DROP TABLE IF EXISTS `billtype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `billtype` (
  `BillTypeUid` bigint NOT NULL,
  `BillDescription` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`BillTypeUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `books`
--

DROP TABLE IF EXISTS `books`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `books` (
  `RowIndex` double DEFAULT NULL,
  `Title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Authors` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `AverageRating` double DEFAULT NULL,
  `ISBN` double DEFAULT NULL,
  `ISBN13` double DEFAULT NULL,
  `LanguageCode` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PageNum` double DEFAULT NULL,
  `RatingsCount` double DEFAULT NULL,
  `TextReviewsCount` double DEFAULT NULL,
  `F11` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `c`
--

DROP TABLE IF EXISTS `c`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `c` (
  `ca` varchar(64) NOT NULL,
  `cb` varchar(50) NOT NULL,
  PRIMARY KEY (`ca`),
  CONSTRAINT `FK_C_ca_D_da` FOREIGN KEY (`ca`) REFERENCES `d` (`da`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `candidatefiledetail`
--

DROP TABLE IF EXISTS `candidatefiledetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `candidatefiledetail` (
  `FileId` bigint NOT NULL AUTO_INCREMENT,
  `FileOwnerId` bigint NOT NULL,
  `FilePath` varchar(500) NOT NULL,
  `FileName` varchar(100) NOT NULL,
  `FileExtension` varchar(100) DEFAULT NULL,
  `UserTypeId` int NOT NULL DEFAULT '0',
  `CreatedOn` datetime(6) DEFAULT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`FileId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `candidatelogin`
--

DROP TABLE IF EXISTS `candidatelogin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `candidatelogin` (
  `UserId` bigint NOT NULL AUTO_INCREMENT,
  `AccessLevelId` bigint DEFAULT NULL,
  `FirstName` varchar(100) DEFAULT NULL,
  `LastName` varchar(100) DEFAULT NULL,
  `Password` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Email` (`Email`),
  UNIQUE KEY `Mobile` (`Mobile`),
  KEY `FK_candidatelogin_AccessLevelId` (`AccessLevelId`),
  CONSTRAINT `FK_candidatelogin_AccessLevelId` FOREIGN KEY (`AccessLevelId`) REFERENCES `accesslevel` (`AccessLevelId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clients`
--

DROP TABLE IF EXISTS `clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clients` (
  `ClientId` bigint NOT NULL AUTO_INCREMENT,
  `ClientName` varchar(250) DEFAULT NULL,
  `MobileNo` varchar(20) DEFAULT NULL,
  `PrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `SecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `Email` varchar(150) DEFAULT NULL,
  `Fax` varchar(20) DEFAULT NULL,
  `FirstAddress` varchar(100) DEFAULT NULL,
  `SecondAddress` varchar(100) DEFAULT NULL,
  `ThirdAddress` varchar(100) DEFAULT NULL,
  `ForthAddress` varchar(100) DEFAULT NULL,
  `Pincode` int DEFAULT NULL,
  `City` varchar(50) DEFAULT NULL,
  `State` varchar(50) DEFAULT NULL,
  `Country` varchar(50) DEFAULT NULL,
  `GSTNO` varchar(20) DEFAULT NULL,
  `AccountNo` varchar(25) DEFAULT NULL,
  `BankName` varchar(100) DEFAULT NULL,
  `BranchName` varchar(100) DEFAULT NULL,
  `IFSC` varchar(15) DEFAULT NULL,
  `PanNo` varchar(20) DEFAULT NULL,
  `CreatedBy` bigint NOT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `OtherEmail_1` varchar(100) DEFAULT NULL,
  `OtherEmail_2` varchar(100) DEFAULT NULL,
  `OtherEmail_3` varchar(100) DEFAULT NULL,
  `OtherEmail_4` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ClientId`),
  KEY `fk_Clients_CreatedBy` (`CreatedBy`),
  KEY `fk_Clients_UpdatedBy` (`UpdatedBy`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `company`
--

DROP TABLE IF EXISTS `company`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company` (
  `CompanyId` int NOT NULL AUTO_INCREMENT,
  `OrganizationId` int NOT NULL,
  `OrganizationName` varchar(250) DEFAULT NULL,
  `CompanyName` varchar(150) DEFAULT NULL,
  `CompanyDetail` varchar(250) DEFAULT NULL,
  `SectorType` int DEFAULT NULL,
  `Country` varchar(50) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `FirstAddress` varchar(100) DEFAULT NULL,
  `SecondAddress` varchar(100) DEFAULT NULL,
  `ThirdAddress` varchar(100) DEFAULT NULL,
  `ForthAddress` varchar(100) DEFAULT NULL,
  `FullAddress` varchar(150) DEFAULT NULL,
  `MobileNo` varchar(20) DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL,
  `FirstEmail` varchar(100) DEFAULT NULL,
  `SecondEmail` varchar(100) DEFAULT NULL,
  `ThirdEmail` varchar(100) DEFAULT NULL,
  `ForthEmail` varchar(100) DEFAULT NULL,
  `PrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `SecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `Fax` varchar(50) DEFAULT NULL,
  `Pincode` int DEFAULT NULL,
  `FileId` bigint DEFAULT NULL,
  `LegalDocumentPath` varchar(250) DEFAULT NULL,
  `LegalEntity` varchar(50) DEFAULT NULL,
  `TypeOfBusiness` varchar(150) DEFAULT NULL,
  `InCorporationDate` datetime DEFAULT NULL,
  `PANNo` varchar(45) DEFAULT NULL,
  `GSTNo` varchar(45) DEFAULT NULL,
  `TradeLicenseNo` varchar(45) DEFAULT NULL,
  `IsPrimaryCompany` bit(1) DEFAULT b'0',
  `FixedComponentsId` json DEFAULT NULL,
  `CreatedBy` mediumtext NOT NULL,
  `UpdatedBy` mediumtext,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`CompanyId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `company_calendar`
--

DROP TABLE IF EXISTS `company_calendar`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company_calendar` (
  `CompanyCalendarId` bigint NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT NULL,
  `EventDate` datetime DEFAULT NULL,
  `EventName` varchar(250) DEFAULT NULL,
  `IsHoliday` bit(1) DEFAULT NULL,
  `IsHalfDay` bit(1) DEFAULT NULL,
  `DescriptionNote` text,
  `ApplicableFor` int DEFAULT NULL,
  `CreatedBy` mediumtext,
  `UpdatedBy` mediumtext,
  `CreatedOn` datetime DEFAULT NULL,
  `Updated` datetime DEFAULT NULL,
  PRIMARY KEY (`CompanyCalendarId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `company_setting`
--

DROP TABLE IF EXISTS `company_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company_setting` (
  `SettingId` bigint NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT NULL,
  `ProbationPeriodInDays` int DEFAULT NULL,
  `NoticePeriodInDays` int DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`SettingId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `component_type`
--

DROP TABLE IF EXISTS `component_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `component_type` (
  `ComponentTypeId` int NOT NULL AUTO_INCREMENT,
  `ComponentDescription` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`ComponentTypeId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `countries`
--

DROP TABLE IF EXISTS `countries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `countries` (
  `CountryName` varchar(100) NOT NULL,
  `Code` varchar(10) NOT NULL,
  `PhoneCode` varchar(5) NOT NULL,
  `Area` bigint NOT NULL,
  `RowIndex` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `d`
--

DROP TABLE IF EXISTS `d`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `d` (
  `da` varchar(64) NOT NULL,
  `db` int NOT NULL,
  `dc` double NOT NULL,
  PRIMARY KEY (`da`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `deductions_detail`
--

DROP TABLE IF EXISTS `deductions_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `deductions_detail` (
  `DeductionId` int NOT NULL AUTO_INCREMENT,
  `DeductionDescription` varchar(100) DEFAULT NULL,
  `IsPaidByEmployee` bit(1) DEFAULT NULL,
  `IsPaidByEmployeer` bit(1) DEFAULT NULL,
  `IsMandatory` bit(1) DEFAULT NULL,
  `IsFixedAmount` bit(1) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`DeductionId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designation_detail`
--

DROP TABLE IF EXISTS `designation_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designation_detail` (
  `DesignationId` int NOT NULL AUTO_INCREMENT,
  `DesignationName` varchar(150) NOT NULL,
  `RoleDescription` text,
  `CompanyId` int DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`DesignationId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `documents`
--

DROP TABLE IF EXISTS `documents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `documents` (
  `DocumentId` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint DEFAULT NULL,
  `Title` varchar(250) NOT NULL,
  `Description` longtext,
  `FilePath` varchar(250) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`DocumentId`),
  KEY `fk_onoinedocument_UserId` (`UserId`),
  CONSTRAINT `fk_documents_UserId` FOREIGN KEY (`UserId`) REFERENCES `userdetail` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `dynamicqueryclouses`
--

DROP TABLE IF EXISTS `dynamicqueryclouses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dynamicqueryclouses` (
  `TableIndex` int NOT NULL,
  `TableName` varchar(50) NOT NULL,
  `OrderByClouse` varchar(500) NOT NULL,
  `ColumnNames` longtext,
  PRIMARY KEY (`TableIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `dynamictablequery`
--

DROP TABLE IF EXISTS `dynamictablequery`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dynamictablequery` (
  `TableIndex` int NOT NULL,
  `TableName` varchar(50) NOT NULL,
  `OrderByClouse` varchar(500) NOT NULL,
  PRIMARY KEY (`TableIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `email_setting_detail`
--

DROP TABLE IF EXISTS `email_setting_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `email_setting_detail` (
  `EmailSettingDetailId` int NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT NULL,
  `EmailAddress` varchar(200) DEFAULT NULL,
  `EmailName` varchar(100) DEFAULT NULL,
  `EmailHost` varchar(100) DEFAULT NULL,
  `POP3EmailHost` varchar(100) DEFAULT NULL,
  `PortNo` int DEFAULT NULL,
  `POP3PortNo` varchar(100) DEFAULT NULL,
  `EnableSsl` bit(1) DEFAULT NULL,
  `POP3EnableSsl` bit(1) DEFAULT NULL,
  `DeliveryMethod` varchar(50) DEFAULT NULL,
  `UserDefaultCredentials` bit(1) DEFAULT NULL,
  `Credentials` varchar(100) DEFAULT NULL,
  `IsPrimary` bit(1) DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`EmailSettingDetailId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `email_templates`
--

DROP TABLE IF EXISTS `email_templates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `email_templates` (
  `EmailTemplateId` int NOT NULL AUTO_INCREMENT,
  `TemplateName` varchar(145) DEFAULT NULL,
  `SubjectLine` varchar(100) DEFAULT NULL,
  `Salutation` varchar(100) DEFAULT NULL,
  `EmailClosingStatement` varchar(100) DEFAULT NULL,
  `BodyContent` json DEFAULT NULL,
  `EmailNote` varchar(500) DEFAULT NULL,
  `SignatureDetail` varchar(145) DEFAULT NULL,
  `ContactNo` varchar(20) DEFAULT NULL,
  `Fax` varchar(20) DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmailTemplateId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_archive`
--

DROP TABLE IF EXISTS `employee_archive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_archive` (
  `EmployeeId` bigint NOT NULL,
  `FullName` varchar(150) DEFAULT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `Email` varchar(45) DEFAULT NULL,
  `Package` decimal(10,0) DEFAULT NULL,
  `DateOfJoining` datetime DEFAULT NULL,
  `DateOfLeaving` datetime DEFAULT NULL,
  `EmployeeCompleteJsonData` json NOT NULL,
  `CreatedBy` mediumtext,
  `CreatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_brakup_detail`
--

DROP TABLE IF EXISTS `employee_brakup_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_brakup_detail` (
  `EmployeeId` bigint NOT NULL,
  `BreakUpDetail` json DEFAULT NULL,
  `BreakUpHeaderCount` int DEFAULT NULL,
  `DeductionDetail` json DEFAULT NULL,
  `DeductionHeaderCount` int DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_declaration`
--

DROP TABLE IF EXISTS `employee_declaration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_declaration` (
  `EmployeeDeclarationId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `DocumentPath` varchar(250) DEFAULT NULL,
  `DeclarationDetail` text,
  `HousingProperty` json DEFAULT NULL,
  `TotalDeclaredAmount` decimal(10,0) DEFAULT NULL,
  `TotalApprovedAmount` decimal(10,0) DEFAULT NULL,
  PRIMARY KEY (`EmployeeDeclarationId`),
  UNIQUE KEY `EmployeeId` (`EmployeeId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_leave_request`
--

DROP TABLE IF EXISTS `employee_leave_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_leave_request` (
  `LeaveRequestId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `LeaveDetail` json DEFAULT NULL,
  `Year` int DEFAULT NULL,
  `AvailableLeaves` decimal(10,0) DEFAULT '0',
  `TotalLeaveApplied` decimal(10,0) DEFAULT '0',
  `TotalApprovedLeave` decimal(10,0) DEFAULT '0',
  `TotalLeaveQuota` decimal(10,0) DEFAULT '0',
  `LeaveQuotaDetail` json DEFAULT NULL,
  PRIMARY KEY (`LeaveRequestId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_notice_period`
--

DROP TABLE IF EXISTS `employee_notice_period`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_notice_period` (
  `EmployeeNoticePeriod` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `ApprovedOn` datetime DEFAULT NULL,
  `ApplicableFrom` datetime DEFAULT NULL,
  `ApproverManagerId` int DEFAULT NULL,
  `ManagerDescription` varchar(500) DEFAULT NULL,
  `AttachmentPath` varchar(200) DEFAULT NULL,
  `EmailTitle` varchar(100) DEFAULT NULL,
  `OtherApproverManagerIds` json DEFAULT NULL,
  `ITClearanceStatus` int DEFAULT NULL,
  `ReportingManagerClearanceStatus` int DEFAULT NULL,
  `CanteenClearanceStatus` int DEFAULT NULL,
  `ClientClearanceStatus` int DEFAULT NULL,
  `HRClearanceStatus` int DEFAULT NULL,
  `OfficialLastWorkingDay` datetime DEFAULT NULL,
  `PeriodDuration` int DEFAULT NULL,
  `EarlyLeaveStatus` int DEFAULT NULL,
  `Reason` varchar(500) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeNoticePeriod`),
  UNIQUE KEY `EmployeeId` (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_notice_period_desc`
--

DROP TABLE IF EXISTS `employee_notice_period_desc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_notice_period_desc` (
  `StatusId` bigint NOT NULL AUTO_INCREMENT,
  `Status` varchar(50) DEFAULT NULL,
  `StatusDecription` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`StatusId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_notification`
--

DROP TABLE IF EXISTS `employee_notification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_notification` (
  `NotificationId` bigint NOT NULL AUTO_INCREMENT,
  `Message` varchar(500) DEFAULT NULL,
  `UserId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT NULL,
  `RequestedOn` bigint DEFAULT NULL,
  `UserName` varchar(100) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `AssigneeId` mediumtext,
  `Status` int DEFAULT NULL,
  `ActionTakenOn` datetime DEFAULT NULL,
  PRIMARY KEY (`NotificationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_request`
--

DROP TABLE IF EXISTS `employee_request`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_request` (
  `RequestId` bigint NOT NULL AUTO_INCREMENT,
  `RequestedBy` bigint DEFAULT NULL,
  `FromDate` datetime DEFAULT NULL,
  `ToDate` datetime DEFAULT NULL,
  `HandledBy` bigint DEFAULT NULL,
  `RequestedOn` datetime DEFAULT NULL,
  `RequestStatus` varchar(50) DEFAULT NULL,
  `StatusDescription` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`RequestId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_salary_detail`
--

DROP TABLE IF EXISTS `employee_salary_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_salary_detail` (
  `EmployeeId` bigint NOT NULL,
  `CTC` decimal(10,0) DEFAULT NULL,
  `GrossIncome` decimal(10,0) DEFAULT NULL,
  `NetSalary` decimal(10,0) DEFAULT NULL,
  `CompleteSalaryDetail` json DEFAULT NULL,
  `GroupId` int DEFAULT '0',
  `TaxDetail` json DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employee_timesheet`
--

DROP TABLE IF EXISTS `employee_timesheet`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_timesheet` (
  `TimesheetId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `ClientId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT NULL,
  `TimesheetMonthJson` json DEFAULT NULL,
  `TotalDays` int DEFAULT NULL,
  `DaysAbsent` int DEFAULT NULL,
  `ExpectedBurnedMinutes` int DEFAULT NULL,
  `ActualBurnedMinutes` int DEFAULT NULL,
  `TotalWeekDays` int DEFAULT NULL,
  `TotalWorkingDays` int DEFAULT NULL,
  `TotalHolidays` int DEFAULT NULL,
  `MonthTimesheetApprovalState` int DEFAULT NULL,
  `ForYear` int DEFAULT NULL,
  `ForMonth` int DEFAULT NULL,
  `SubmittedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `SubmittedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`TimesheetId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeebilldocuments`
--

DROP TABLE IF EXISTS `employeebilldocuments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeebilldocuments` (
  `EmployeeBillDocumentUid` bigint NOT NULL,
  `BillNo` varchar(25) DEFAULT NULL,
  PRIMARY KEY (`EmployeeBillDocumentUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeelogin`
--

DROP TABLE IF EXISTS `employeelogin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeelogin` (
  `EmployeeLoginId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeId` bigint DEFAULT NULL,
  `UserTypeId` int DEFAULT '1',
  `AccessLevelId` bigint DEFAULT NULL,
  `Password` varchar(150) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `OrganizationId` int DEFAULT '0',
  `CompanyId` int DEFAULT '0',
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeLoginId`),
  UNIQUE KEY `Email` (`Email`),
  UNIQUE KEY `Mobile` (`Mobile`),
  KEY `FK_UserLogin_AccessLevelId` (`AccessLevelId`),
  KEY `FK_UserLogin_CreatedBy` (`CreatedBy`),
  KEY `FK_UserLogin_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_employeelogin_AccessLevelId` FOREIGN KEY (`AccessLevelId`) REFERENCES `accesslevel` (`AccessLevelId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeelogin_archive`
--

DROP TABLE IF EXISTS `employeelogin_archive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeelogin_archive` (
  `EmployeeId` bigint NOT NULL AUTO_INCREMENT,
  `UserTypeId` int DEFAULT '1',
  `AccessLevelId` bigint DEFAULT NULL,
  `Password` varchar(150) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeemappedclients`
--

DROP TABLE IF EXISTS `employeemappedclients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeemappedclients` (
  `EmployeeMappedClientsUid` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeUid` bigint DEFAULT NULL,
  `ClientUid` bigint DEFAULT NULL,
  `ClientName` varchar(250) DEFAULT NULL,
  `FinalPackage` decimal(10,0) DEFAULT NULL,
  `ActualPackage` decimal(10,0) DEFAULT NULL,
  `TakeHomeByCandidate` decimal(10,0) DEFAULT NULL,
  `IsPermanent` bit(1) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT NULL,
  `BillingHours` int NOT NULL DEFAULT '0',
  `DaysPerWeek` int NOT NULL DEFAULT '0',
  `DateOfJoining` datetime DEFAULT NULL,
  `DateOfLeaving` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeMappedClientsUid`),
  KEY `fk_EmployeeMappedClients_EmployeeUid` (`EmployeeUid`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeepayroll`
--

DROP TABLE IF EXISTS `employeepayroll`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeepayroll` (
  `EmployeePayrollId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeUid` bigint DEFAULT NULL,
  `DateofSalary` datetime(6) DEFAULT NULL,
  `PerMonthSalary` decimal(10,2) DEFAULT NULL,
  `PaySlipNo` int DEFAULT NULL,
  `PaySlip_PDF_Filepath` varchar(250) DEFAULT NULL,
  `GeneratedOn` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeePayrollId`),
  KEY `fk_EmployeePayroll_CreatedBy` (`CreatedBy`),
  KEY `fk_EmployeePayroll_UpdatedBy` (`UpdatedBy`),
  KEY `fk_EmployeePayroll_EmployeeUid_idx` (`EmployeeUid`),
  CONSTRAINT `fk_EmployeePayroll_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `userdetail` (`UserId`),
  CONSTRAINT `fk_EmployeePayroll_EmployeeUid` FOREIGN KEY (`EmployeeUid`) REFERENCES `employees` (`EmployeeUid`),
  CONSTRAINT `fk_EmployeePayroll_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `userdetail` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeepersonaldetail`
--

DROP TABLE IF EXISTS `employeepersonaldetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeepersonaldetail` (
  `EmployeePersonalDetailId` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeUid` bigint DEFAULT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `SecondaryMobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Gender` bit(1) DEFAULT NULL,
  `FatherName` varchar(50) DEFAULT NULL,
  `SpouseName` varchar(50) DEFAULT NULL,
  `MotherName` varchar(50) DEFAULT NULL,
  `Address` varchar(100) DEFAULT NULL,
  `State` varchar(75) DEFAULT NULL,
  `City` varchar(75) DEFAULT NULL,
  `Pincode` int DEFAULT NULL,
  `IsPermanent` bit(1) DEFAULT NULL,
  `ActualPackage` decimal(10,0) DEFAULT NULL,
  `FinalPackage` decimal(10,0) DEFAULT NULL,
  `TakeHomeByCandidate` decimal(10,0) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `DOB` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeePersonalDetailId`),
  KEY `fk_EmployeePersonalDetail_CreatedBy` (`CreatedBy`),
  KEY `fk_EmployeePersonalDetail_UpdatedBy` (`UpdatedBy`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeepersonaldetail_archive`
--

DROP TABLE IF EXISTS `employeepersonaldetail_archive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeepersonaldetail_archive` (
  `EmployeeUid` bigint NOT NULL AUTO_INCREMENT,
  `Mobile` varchar(20) DEFAULT NULL,
  `SecondaryMobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Gender` bit(1) DEFAULT NULL,
  `FatherName` varchar(50) DEFAULT NULL,
  `SpouseName` varchar(50) DEFAULT NULL,
  `MotherName` varchar(50) DEFAULT NULL,
  `Address` varchar(100) DEFAULT NULL,
  `State` varchar(75) DEFAULT NULL,
  `City` varchar(75) DEFAULT NULL,
  `Pincode` int DEFAULT NULL,
  `IsPermanent` bit(1) DEFAULT NULL,
  `ActualPackage` float(10,2) DEFAULT NULL,
  `FinalPackage` float(10,2) DEFAULT NULL,
  `TakeHomeByCandidate` float(10,2) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeeprofessiondetail`
--

DROP TABLE IF EXISTS `employeeprofessiondetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeeprofessiondetail` (
  `EmpProfDetailUid` bigint NOT NULL AUTO_INCREMENT,
  `EmployeeUid` bigint DEFAULT NULL,
  `FirstName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `SecondaryMobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PANNo` varchar(20) DEFAULT NULL,
  `AadharNo` varchar(20) DEFAULT NULL,
  `AccountNumber` varchar(50) DEFAULT NULL,
  `BankName` varchar(100) DEFAULT NULL,
  `BranchName` varchar(100) DEFAULT NULL,
  `IFSCCode` varchar(20) DEFAULT NULL,
  `Domain` varchar(250) DEFAULT NULL,
  `Specification` varchar(250) DEFAULT NULL,
  `ExprienceInYear` decimal(10,0) DEFAULT NULL,
  `LastCompanyName` varchar(100) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `ProfessionalDetail_Json` json DEFAULT NULL,
  PRIMARY KEY (`EmpProfDetailUid`),
  UNIQUE KEY `EmployeeUid` (`EmployeeUid`),
  KEY `fk_employeeprofessiondetail_CreatedBy` (`CreatedBy`),
  KEY `fk_employeeprofessiondetail_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `fk_EmployeeProfessionDetail_EmployeeUid` FOREIGN KEY (`EmployeeUid`) REFERENCES `employees` (`EmployeeUid`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employeeprofessiondetail_archive`
--

DROP TABLE IF EXISTS `employeeprofessiondetail_archive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employeeprofessiondetail_archive` (
  `EmployeeUid` bigint NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `SecondaryMobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PANNo` varchar(20) DEFAULT NULL,
  `AadharNo` varchar(20) DEFAULT NULL,
  `AccountNumber` varchar(50) DEFAULT NULL,
  `BankName` varchar(100) DEFAULT NULL,
  `BranchName` varchar(100) DEFAULT NULL,
  `IFSCCode` varchar(20) DEFAULT NULL,
  `Domain` varchar(250) DEFAULT NULL,
  `Specification` varchar(250) DEFAULT NULL,
  `ExprienceInYear` float(5,2) DEFAULT NULL,
  `LastCompanyName` varchar(100) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `ProfessionalDetail_Json` json DEFAULT NULL,
  PRIMARY KEY (`EmployeeUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `EmployeeUid` bigint NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'0',
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime(6) DEFAULT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `ReportingManagerId` bigint DEFAULT '0',
  `DesignationId` int DEFAULT '0',
  `UserTypeId` int DEFAULT '2',
  `LeavePlanId` int DEFAULT '0',
  `PayrollGroupId` int DEFAULT '0',
  `SalaryGroupId` int DEFAULT '0',
  `CompanyId` int DEFAULT '0',
  `NoticePeriodId` int DEFAULT '0',
  PRIMARY KEY (`EmployeeUid`),
  KEY `fk_Employees_Createdby` (`CreatedBy`),
  KEY `fk_Employees_UpdatedBy` (`UpdatedBy`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `filedetail`
--

DROP TABLE IF EXISTS `filedetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `filedetail` (
  `FileId` bigint NOT NULL AUTO_INCREMENT,
  `FileOwnerId` bigint NOT NULL,
  `FilePath` varchar(500) NOT NULL,
  `FileName` varchar(100) NOT NULL,
  `FileExtension` varchar(10) DEFAULT NULL,
  `ItemStatusId` bigint DEFAULT NULL,
  `PaidOn` datetime(6) DEFAULT NULL,
  `UserTypeId` int NOT NULL DEFAULT '0',
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime(6) DEFAULT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`FileId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fixed_salary_component_percent`
--

DROP TABLE IF EXISTS `fixed_salary_component_percent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fixed_salary_component_percent` (
  `ComponentId` varchar(10) NOT NULL,
  `ComponentDescription` varchar(250) NOT NULL,
  `CalculateInPercentage` bit(1) DEFAULT NULL,
  `PercentageValue` decimal(10,0) DEFAULT NULL,
  `Amount` decimal(10,0) DEFAULT NULL,
  `EmployeeContribution` decimal(10,0) DEFAULT NULL,
  `EmployerContribution` decimal(10,0) DEFAULT NULL,
  `IncludeInPayslip` bit(1) DEFAULT NULL,
  `IsDeductions` bit(1) DEFAULT NULL,
  `IsOpted` bit(1) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`ComponentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gstdetail`
--

DROP TABLE IF EXISTS `gstdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gstdetail` (
  `gstId` bigint NOT NULL AUTO_INCREMENT,
  `billno` varchar(20) DEFAULT NULL,
  `gststatus` int DEFAULT NULL,
  `paidon` datetime DEFAULT NULL,
  `paidby` bigint DEFAULT NULL,
  `amount` double DEFAULT NULL,
  `fileId` bigint DEFAULT NULL,
  PRIMARY KEY (`gstId`),
  UNIQUE KEY `billno` (`billno`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `iexception`
--

DROP TABLE IF EXISTS `iexception`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iexception` (
  `ExceptionUniqueCode` varchar(50) NOT NULL,
  `StackTrace` text,
  `MethodFullyQualifiedName` varchar(100) NOT NULL,
  `ExceptionTime` datetime NOT NULL,
  `IsProcedureException` bit(1) DEFAULT NULL,
  `IsCodeException` bit(1) DEFAULT NULL,
  KEY `ExceptionUniqueCode` (`ExceptionUniqueCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `incometax_slab`
--

DROP TABLE IF EXISTS `incometax_slab`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `incometax_slab` (
  `IncometaxSlab` int NOT NULL AUTO_INCREMENT,
  `MinIncome` decimal(10,0) DEFAULT NULL,
  `MaxIncome` decimal(10,0) DEFAULT NULL,
  `RegimeType` varchar(45) DEFAULT NULL,
  `TaxPercentage` decimal(10,0) DEFAULT NULL,
  PRIMARY KEY (`IncometaxSlab`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `itemstatus`
--

DROP TABLE IF EXISTS `itemstatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `itemstatus` (
  `ItemStatusId` bigint NOT NULL AUTO_INCREMENT,
  `Status` varchar(50) NOT NULL,
  `StatusDescription` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ItemStatusId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_accrual`
--

DROP TABLE IF EXISTS `leave_accrual`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_accrual` (
  `LeaveAccrualId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `CanApplyEntireLeave` bit(1) DEFAULT NULL,
  `IsLeaveAccruedPatternAvail` bit(1) DEFAULT NULL,
  `JoiningMonthLeaveDistribution` json DEFAULT NULL,
  `IsLeaveAccruedProrateDefined` bit(1) DEFAULT NULL,
  `LeaveDistributionRateOnStartOfPeriod` json DEFAULT NULL,
  `ExitMonthLeaveDistribution` json DEFAULT NULL,
  `LeaveDistributionSequence` varchar(45) DEFAULT NULL,
  `LeaveDistributionAppliedFrom` decimal(10,0) DEFAULT NULL,
  `IsLeavesProratedForJoinigMonth` bit(1) DEFAULT NULL,
  `IsLeavesProratedOnNotice` bit(1) DEFAULT NULL,
  `IsNotAllowProratedOnNotice` bit(1) DEFAULT NULL,
  `IsNoLeaveOnNoticePeriod` bit(1) DEFAULT NULL,
  `IsVaryOnProbationOrExprience` bit(1) DEFAULT NULL,
  `IsAccrualStartsAfterJoining` bit(1) DEFAULT NULL,
  `IsAccrualStartsAfterProbationEnds` bit(1) DEFAULT NULL,
  `AccrualDaysAfterJoining` decimal(10,0) DEFAULT NULL,
  `AccrualDaysAfterProbationEnds` decimal(10,0) DEFAULT NULL,
  `AccrualProrateDetail` json DEFAULT NULL,
  `IsImpactedOnWorkDaysEveryMonth` bit(1) DEFAULT NULL,
  `WeekOffAsAbsentIfAttendaceLessThen` decimal(10,0) DEFAULT NULL,
  `HolidayAsAbsentIfAttendaceLessThen` decimal(10,0) DEFAULT NULL,
  `CanApplyForFutureDate` bit(1) DEFAULT NULL,
  `IsExtraLeaveBeyondAccruedBalance` bit(1) DEFAULT NULL,
  `IsNoExtraLeaveBeyondAccruedBalance` bit(1) DEFAULT NULL,
  `NoOfDaysForExtraLeave` decimal(10,0) DEFAULT NULL,
  `IsAccrueIfHavingLeaveBalance` bit(1) DEFAULT NULL,
  `AllowOnlyIfAccrueBalanceIsAlleast` decimal(10,0) DEFAULT NULL,
  `IsAccrueIfOnOtherLeave` bit(1) DEFAULT NULL,
  `NotAllowIfAlreadyOnLeaveMoreThan` decimal(10,0) DEFAULT NULL,
  `RoundOffLeaveBalance` bit(1) DEFAULT NULL,
  `ToNearestHalfDay` bit(1) DEFAULT NULL,
  `ToNearestFullDay` bit(1) DEFAULT NULL,
  `ToNextAvailableHalfDay` bit(1) DEFAULT NULL,
  `ToNextAvailableFullDay` bit(1) DEFAULT NULL,
  `ToPreviousHalfDay` bit(1) DEFAULT NULL,
  `DoesLeaveExpireAfterSomeTime` bit(1) DEFAULT NULL,
  `AfterHowManyDays` decimal(10,0) DEFAULT NULL,
  PRIMARY KEY (`LeaveAccrualId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_apply_detail`
--

DROP TABLE IF EXISTS `leave_apply_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_apply_detail` (
  `LeaveApplyDetailId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `IsAllowForHalfDay` bit(1) DEFAULT NULL,
  `EmployeeCanSeeAndApplyCurrentPlanLeave` bit(1) DEFAULT NULL,
  `RuleForLeaveInNotice` json DEFAULT NULL,
  `ApplyPriorBeforeLeaveDate` int DEFAULT NULL,
  `BackDateLeaveApplyNotBeyondDays` int DEFAULT NULL,
  `RestrictBackDateLeaveApplyAfter` int DEFAULT NULL,
  `CurrentLeaveRequiredComments` bit(1) DEFAULT NULL,
  `ProofRequiredIfDaysExceeds` bit(1) DEFAULT NULL,
  `NoOfDaysExceeded` int DEFAULT NULL,
  PRIMARY KEY (`LeaveApplyDetailId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_approval`
--

DROP TABLE IF EXISTS `leave_approval`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_approval` (
  `LeaveApprovalId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `IsLeaveRequiredApproval` bit(1) DEFAULT NULL,
  `ApprovalLevels` int DEFAULT NULL,
  `ApprovalChain` json DEFAULT NULL,
  `IsRequiredAllLevelApproval` bit(1) DEFAULT NULL,
  `CanHigherRankPersonsIsAvailForAction` bit(1) DEFAULT NULL,
  `IsPauseForApprovalNotification` bit(1) DEFAULT NULL,
  `IsReportingManageIsDefaultForAction` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeaveApprovalId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_detail`
--

DROP TABLE IF EXISTS `leave_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_detail` (
  `LeaveDetailId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `IsLeaveDaysLimit` bit(1) DEFAULT NULL,
  `LeaveLimit` int DEFAULT NULL,
  `CanApplyExtraLeave` bit(1) DEFAULT NULL,
  `ExtraLeaveLimit` int DEFAULT NULL,
  `IsNoLeaveAfterDate` bit(1) DEFAULT NULL,
  `LeaveNotAllocatedIfJoinAfter` int DEFAULT NULL,
  `CanCompoffAllocatedAutomatically` bit(1) DEFAULT NULL,
  `CanCompoffCreditedByManager` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeaveDetailId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_endyear_processing`
--

DROP TABLE IF EXISTS `leave_endyear_processing`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_endyear_processing` (
  `LeaveEndYearProcessingId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `IsLeaveBalanceExpiredOnEndOfYear` bit(1) DEFAULT NULL,
  `AllConvertedToPaid` bit(1) DEFAULT NULL,
  `AllLeavesCarryForwardToNextYear` bit(1) DEFAULT NULL,
  `PayFirstNCarryForwordRemaning` bit(1) DEFAULT NULL,
  `CarryForwordFirstNPayRemaning` bit(1) DEFAULT NULL,
  `PayNCarryForwardForPercent` bit(1) DEFAULT NULL,
  `PayNCarryForwardDefineType` varchar(50) DEFAULT NULL,
  `FixedPayNCarryForward` json DEFAULT NULL,
  `PercentagePayNCarryForward` json DEFAULT NULL,
  `DoestCarryForwardExpired` bit(1) DEFAULT NULL,
  `ExpiredAfter` decimal(10,0) DEFAULT NULL,
  `DoesExpiryLeaveRemainUnchange` bit(1) DEFAULT NULL,
  `DeductFromSalaryOnYearChange` bit(1) DEFAULT NULL,
  `ResetBalanceToZero` bit(1) DEFAULT NULL,
  `CarryForwardToNextYear` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeaveEndYearProcessingId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_from_management`
--

DROP TABLE IF EXISTS `leave_from_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_from_management` (
  `LeaveManagementId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `CanManagerAwardCausalLeave` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeaveManagementId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_holidays_and_weekoff`
--

DROP TABLE IF EXISTS `leave_holidays_and_weekoff`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_holidays_and_weekoff` (
  `LeaveHolidaysAndWeekOffId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanTypeId` int DEFAULT NULL,
  `AdJoiningHolidayIsConsiderAsLeave` bit(1) DEFAULT NULL,
  `ConsiderLeaveIfNumOfDays` decimal(10,0) DEFAULT NULL,
  `IfLeaveLieBetweenTwoHolidays` bit(1) DEFAULT NULL,
  `IfHolidayIsRightBeforLeave` bit(1) DEFAULT NULL,
  `IfHolidayIsRightAfterLeave` bit(1) DEFAULT NULL,
  `IfHolidayIsBetweenLeave` bit(1) DEFAULT NULL,
  `IfHolidayIsRightBeforeAfterOrInBetween` bit(1) DEFAULT NULL,
  `AdjoiningHolidayRulesIsValidForHalfDay` bit(1) DEFAULT NULL,
  `AdjoiningWeekOffIsConsiderAsLeave` bit(1) DEFAULT NULL,
  `ConsiderLeaveIfIncludeDays` decimal(10,0) DEFAULT NULL,
  `IfLeaveLieBetweenWeekOff` bit(1) DEFAULT NULL,
  `IfWeekOffIsRightBeforLeave` bit(1) DEFAULT NULL,
  `IfWeekOffIsRightAfterLeave` bit(1) DEFAULT NULL,
  `IfWeekOffIsBetweenLeave` bit(1) DEFAULT NULL,
  `IfWeekOffIsRightBeforeAfterOrInBetween` bit(1) DEFAULT NULL,
  `AdjoiningWeekOffRulesIsValidForHalfDay` bit(1) DEFAULT NULL,
  `ClubSandwichPolicy` bit(1) DEFAULT NULL,
  PRIMARY KEY (`LeaveHolidaysAndWeekOffId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_plan`
--

DROP TABLE IF EXISTS `leave_plan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_plan` (
  `LeavePlanId` int NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT '0',
  `PlanName` varchar(50) DEFAULT NULL,
  `PlanDescription` varchar(250) DEFAULT NULL,
  `AssociatedPlanTypes` json DEFAULT NULL,
  `PlanStartCalendarDate` datetime DEFAULT NULL,
  `IsShowLeavePolicy` bit(1) DEFAULT NULL,
  `IsUploadedCustomLeavePolicy` bit(1) DEFAULT NULL,
  `IsDefaultPlan` bit(1) DEFAULT b'0',
  `CanApplyEntireLeave` bit(1) DEFAULT b'0',
  PRIMARY KEY (`LeavePlanId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_plan_restriction`
--

DROP TABLE IF EXISTS `leave_plan_restriction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_plan_restriction` (
  `LeavePlanRestrictionId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanId` int DEFAULT NULL,
  `CanApplyAfterProbation` bit(1) DEFAULT NULL,
  `CanApplyAfterJoining` bit(1) DEFAULT NULL,
  `DaysAfterProbation` int DEFAULT NULL,
  `DaysAfterJoining` int DEFAULT NULL,
  `IsAvailRestrictedLeavesInProbation` bit(1) DEFAULT NULL,
  `LeaveLimitInProbation` decimal(10,0) DEFAULT NULL,
  `IsLeaveInNoticeExtendsNoticePeriod` bit(1) DEFAULT NULL,
  `NoOfTimesNoticePeriodExtended` decimal(10,0) DEFAULT NULL,
  `CanManageOverrideLeaveRestriction` bit(1) DEFAULT NULL,
  `GapBetweenTwoConsicutiveLeaveDates` decimal(10,0) DEFAULT NULL,
  `LimitOfMaximumLeavesInCalendarYear` decimal(10,0) DEFAULT NULL,
  `LimitOfMaximumLeavesInCalendarMonth` decimal(10,0) DEFAULT NULL,
  `LimitOfMaximumLeavesInEntireTenure` decimal(10,0) DEFAULT NULL,
  `MinLeaveToApplyDependsOnAvailable` decimal(10,0) DEFAULT NULL,
  `AvailableLeaves` decimal(10,0) DEFAULT NULL,
  `RestrictFromDayOfEveryMonth` int DEFAULT NULL,
  `IsCurrentPlanDepnedsOnOtherPlan` bit(1) DEFAULT NULL,
  `AssociatedPlanTypeId` int DEFAULT NULL,
  `IsCheckOtherPlanTypeBalance` bit(1) DEFAULT NULL,
  `DependentPlanTypeId` int DEFAULT NULL,
  PRIMARY KEY (`LeavePlanRestrictionId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `leave_plans_type`
--

DROP TABLE IF EXISTS `leave_plans_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `leave_plans_type` (
  `LeavePlanTypeId` int NOT NULL AUTO_INCREMENT,
  `LeavePlanCode` varchar(10) DEFAULT NULL,
  `PlanName` varchar(50) DEFAULT NULL,
  `PlanDescription` varchar(250) DEFAULT NULL,
  `MaxLeaveLimit` int DEFAULT NULL,
  `ShowDescription` bit(1) DEFAULT NULL,
  `IsPaidLeave` bit(1) DEFAULT NULL,
  `IsSickLeave` bit(1) DEFAULT NULL,
  `IsStatutoryLeave` bit(1) DEFAULT NULL,
  `IsMale` bit(1) DEFAULT NULL,
  `IsMarried` bit(1) DEFAULT NULL,
  `IsRestrictOnGender` bit(1) DEFAULT NULL,
  `IsRestrictOnMaritalStatus` bit(1) DEFAULT NULL,
  `Reasons` json DEFAULT NULL,
  `PlanConfigurationDetail` json DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`LeavePlanTypeId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `liveurl`
--

DROP TABLE IF EXISTS `liveurl`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `liveurl` (
  `savedUrlId` bigint NOT NULL,
  `method` varchar(20) DEFAULT NULL,
  `url` longtext,
  `parameter` longtext,
  `lastUsed` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`savedUrlId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mappeduisqldatatype`
--

DROP TABLE IF EXISTS `mappeduisqldatatype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mappeduisqldatatype` (
  `RowIndex` int NOT NULL,
  `value` varchar(50) NOT NULL,
  `data` varchar(50) NOT NULL,
  `TableName` varchar(50) NOT NULL,
  `SqlDataType` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`RowIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `medicinehelpfulcontacts`
--

DROP TABLE IF EXISTS `medicinehelpfulcontacts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `medicinehelpfulcontacts` (
  `state_abbr` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `state_name` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `org_type` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `org_name` varchar(120) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `agency_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_phone_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_tdd` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_tdd_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `agency_id` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `tollfree_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_instate_flag` varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_tdd` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_tdd_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `spanish_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `spanish_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `address` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `address2` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `agency_notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email_address` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `web_address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `medicinequeries`
--

DROP TABLE IF EXISTS `medicinequeries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `medicinequeries` (
  `state_abbr` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `state_name` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `topic_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `org_type` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `question` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `org_name` varchar(120) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `agency_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_phone_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_tdd` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local_tdd_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_phone_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_instate_flag` varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_notes` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_tdd` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `tollfree_tdd_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `spanish_phone` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `spanish_ext` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `email_address` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `web_address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `names`
--

DROP TABLE IF EXISTS `names`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `names` (
  `RowIndex` int NOT NULL,
  `PersonNames` varchar(100) NOT NULL,
  `Meaning` varchar(250) DEFAULT NULL,
  `Religion` varchar(20) NOT NULL,
  `Gender` tinyint(1) DEFAULT NULL,
  `EmailId` varchar(100) NOT NULL DEFAULT '',
  `MobileNo` varchar(15) DEFAULT NULL,
  PRIMARY KEY (`RowIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `noticeperioddescription`
--

DROP TABLE IF EXISTS `noticeperioddescription`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `noticeperioddescription` (
  `NoticePeriodDescriptionId` bigint NOT NULL AUTO_INCREMENT,
  `Value` int DEFAULT NULL,
  `Description` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`NoticePeriodDescriptionId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `organization_detail`
--

DROP TABLE IF EXISTS `organization_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `organization_detail` (
  `OrganizationId` int NOT NULL AUTO_INCREMENT,
  `OrganizationName` varchar(250) DEFAULT NULL,
  `OrgMobileNo` varchar(20) DEFAULT NULL,
  `OrgEmail` varchar(50) DEFAULT NULL,
  `OrgPrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgSecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgFax` varchar(50) DEFAULT NULL,
  `CreatedBy` mediumtext NOT NULL,
  `UpdatedBy` mediumtext,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`OrganizationId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pay_calculation`
--

DROP TABLE IF EXISTS `pay_calculation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pay_calculation` (
  `PayCalculationId` int NOT NULL AUTO_INCREMENT,
  `PayCalculationDesc` varchar(250) DEFAULT NULL,
  PRIMARY KEY (`PayCalculationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `payroll_cycle_setting`
--

DROP TABLE IF EXISTS `payroll_cycle_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payroll_cycle_setting` (
  `PayrollCycleSettingId` int NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT NULL,
  `OrganizationId` int DEFAULT NULL,
  `PayFrequency` varchar(45) DEFAULT NULL,
  `PayCycleMonth` int DEFAULT NULL,
  `PayCycleDayOfMonth` int DEFAULT NULL,
  `PayCalculationId` int DEFAULT NULL,
  `IsExcludeWeeklyOffs` bit(1) DEFAULT NULL,
  `IsExcludeHolidays` bit(1) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`PayrollCycleSettingId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pf_esi_setting`
--

DROP TABLE IF EXISTS `pf_esi_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pf_esi_setting` (
  `PfEsi_setting_Id` int NOT NULL AUTO_INCREMENT,
  `CompanyId` int DEFAULT NULL,
  `PFEnable` bit(1) DEFAULT NULL,
  `IsPfAmountLimitStatutory` bit(1) DEFAULT NULL,
  `IsPfCalculateInPercentage` bit(1) DEFAULT NULL,
  `IsAllowOverridingPf` bit(1) DEFAULT NULL,
  `IsPfEmployerContribution` bit(1) DEFAULT NULL,
  `EmployerPFLimit` decimal(10,0) DEFAULT NULL,
  `IsHidePfEmployer` bit(1) DEFAULT NULL,
  `IsPayOtherCharges` bit(1) DEFAULT NULL,
  `IsAllowVPF` bit(1) DEFAULT NULL,
  `EsiEnable` bit(1) DEFAULT NULL,
  `MaximumGrossForESI` decimal(10,0) DEFAULT NULL,
  `EsiEmployeeContribution` decimal(10,0) DEFAULT NULL,
  `EsiEmployerContribution` decimal(10,0) DEFAULT NULL,
  `IsAllowOverridingEsi` bit(1) DEFAULT NULL,
  `IsHideEsiEmployer` bit(1) DEFAULT NULL,
  `IsEsiExcludeEmployerShare` bit(1) DEFAULT NULL,
  `IsEsiExcludeEmployeeGratuity` bit(1) DEFAULT NULL,
  `IsEsiEmployerContributionOutside` bit(1) DEFAULT NULL,
  `IsRestrictEsi` bit(1) DEFAULT NULL,
  `IsIncludeBonusEsiEligibility` bit(1) DEFAULT NULL,
  `IsIncludeBonusEsiContribution` bit(1) DEFAULT NULL,
  `IsEmployerPFLimitContribution` bit(1) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`PfEsi_setting_Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `professionalcandidates`
--

DROP TABLE IF EXISTS `professionalcandidates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `professionalcandidates` (
  `UserId` bigint NOT NULL AUTO_INCREMENT,
  `Source_Of_Application` varchar(50) DEFAULT NULL,
  `Job_Title` varchar(50) DEFAULT NULL,
  `Date_of_application` datetime DEFAULT NULL,
  `Name` varchar(50) DEFAULT NULL,
  `Email_ID` varchar(100) NOT NULL,
  `Phone_Number` varchar(20) DEFAULT NULL,
  `Alternet_Numbers` varchar(100) DEFAULT NULL,
  `Total_Experience` float DEFAULT NULL,
  `Annual_Salary` double DEFAULT NULL,
  `Notice_Period` int DEFAULT NULL,
  `Expeceted_CTC` double DEFAULT NULL,
  `Feedback` varchar(500) DEFAULT NULL,
  `Current_Location` varchar(100) DEFAULT NULL,
  `Preferred_Locations` varchar(100) DEFAULT NULL,
  `Current_Company_name` varchar(150) DEFAULT NULL,
  `Current_Company_Designation` varchar(150) DEFAULT NULL,
  `Functional_Area` varchar(100) DEFAULT NULL,
  `Role` varchar(50) DEFAULT NULL,
  `Industry` varchar(150) DEFAULT NULL,
  `Key_Skills` varchar(500) DEFAULT NULL,
  `Resume_Headline` varchar(500) DEFAULT NULL,
  `Summary` text,
  `Under_Graduation_degree` varchar(50) DEFAULT NULL,
  `UG_Specialization` varchar(50) DEFAULT NULL,
  `UG_University_institute_Name` varchar(150) DEFAULT NULL,
  `UG_Graduation_year` int DEFAULT NULL,
  `Post_graduation_degree` varchar(50) DEFAULT NULL,
  `PG_specialization` varchar(50) DEFAULT NULL,
  `PG_university_institute_name` varchar(150) DEFAULT NULL,
  `PG_graduation_year` int DEFAULT NULL,
  `Doctorate_degree` varchar(50) DEFAULT NULL,
  `Doctorate_specialization` varchar(50) DEFAULT NULL,
  `Doctorate_university_institute_name` varchar(150) DEFAULT NULL,
  `Doctorate_graduation_year` int DEFAULT NULL,
  `Gender` varchar(10) DEFAULT NULL,
  `Marital_Status` bit(1) DEFAULT NULL,
  `Home_Town_City` varchar(50) DEFAULT NULL,
  `Pin_Code` int DEFAULT NULL,
  `Work_permit_for_USA` varchar(50) DEFAULT NULL,
  `Date_of_Birth` datetime DEFAULT NULL,
  `Latest_Star_Rating` float DEFAULT NULL,
  `Viewed` varchar(50) DEFAULT NULL,
  `Viewed_By` varchar(50) DEFAULT NULL,
  `Time_Of_View` varchar(50) DEFAULT NULL,
  `Emailed` varchar(50) DEFAULT NULL,
  `Emailed_By` varchar(50) DEFAULT NULL,
  `Time_Of_Email` varchar(50) DEFAULT NULL,
  `Calling_Status` varchar(50) DEFAULT NULL,
  `Calling_Status_updated_by` varchar(50) DEFAULT NULL,
  `Time_of_Calling_activity_update` datetime DEFAULT NULL,
  `Comment_1` varchar(50) DEFAULT NULL,
  `Comment_1_BY` varchar(50) DEFAULT NULL,
  `Time_Comment_1_posted` varchar(50) DEFAULT NULL,
  `Comment_2` varchar(50) DEFAULT NULL,
  `Comment_2_BY` varchar(50) DEFAULT NULL,
  `Time_Comment_2_posted` varchar(50) DEFAULT NULL,
  `Comment_3` varchar(50) DEFAULT NULL,
  `Comment_3_BY` varchar(50) DEFAULT NULL,
  `Time_Comment_3_posted` varchar(50) DEFAULT NULL,
  `Comment_4` varchar(50) DEFAULT NULL,
  `Comment_4_BY` varchar(50) DEFAULT NULL,
  `Time_Comment_4_posted` varchar(50) DEFAULT NULL,
  `Comment_5` varchar(50) DEFAULT NULL,
  `Comment_5_BY` varchar(50) DEFAULT NULL,
  `Time_Comment_5_posted` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Email_ID_UNIQUE` (`Email_ID`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `professionaldetail`
--

DROP TABLE IF EXISTS `professionaldetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `professionaldetail` (
  `EmployeeId` bigint NOT NULL,
  `Mobile` varchar(20) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `FirstName` varchar(100) DEFAULT NULL,
  `LastName` varchar(100) DEFAULT NULL,
  `ProfessionalDetailJson` json DEFAULT NULL,
  `CreatedOn` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `project`
--

DROP TABLE IF EXISTS `project`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project` (
  `ProjectId` bigint NOT NULL AUTO_INCREMENT,
  `ProjectName` varchar(150) DEFAULT NULL,
  `ProjectDescription` varchar(500) DEFAULT NULL,
  `ProjectManagerId` bigint DEFAULT NULL,
  `TeamMemberIds` json DEFAULT NULL,
  `ProjectStartedOn` datetime DEFAULT NULL,
  `ProjectEndedOn` datetime DEFAULT NULL,
  `ArchitectId` bigint DEFAULT NULL,
  `IsClientProject` bit(1) DEFAULT NULL,
  `ClientId` bigint DEFAULT NULL,
  `HomePageUrl` varchar(150) DEFAULT NULL,
  `PageIndexDetail` json DEFAULT NULL,
  `KeywordDetail` json DEFAULT NULL,
  `DocumentationDetail` json DEFAULT NULL,
  PRIMARY KEY (`ProjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary view structure for view `randview`
--

DROP TABLE IF EXISTS `randview`;
/*!50001 DROP VIEW IF EXISTS `randview`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `randview` AS SELECT 
 1 AS `Value`*/;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `refreshtoken`
--

DROP TABLE IF EXISTS `refreshtoken`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refreshtoken` (
  `UserId` bigint NOT NULL,
  `RefreshToken` varchar(500) DEFAULT NULL,
  `ExpiryTime` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `request_type_desc`
--

DROP TABLE IF EXISTS `request_type_desc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `request_type_desc` (
  `RequestTypeDescId` int NOT NULL AUTO_INCREMENT,
  `TypeDescription` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`RequestTypeDescId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `role_accessibility_mapping`
--

DROP TABLE IF EXISTS `role_accessibility_mapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_accessibility_mapping` (
  `RoleAccessibilityMappingId` int NOT NULL AUTO_INCREMENT,
  `AccessLevelId` int DEFAULT NULL,
  `AccessCode` int DEFAULT NULL,
  `AccessibilityId` int DEFAULT NULL,
  PRIMARY KEY (`RoleAccessibilityMappingId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rolesandmenu`
--

DROP TABLE IF EXISTS `rolesandmenu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rolesandmenu` (
  `Catagory` varchar(100) DEFAULT NULL,
  `Childs` varchar(100) DEFAULT NULL,
  `Link` varchar(100) DEFAULT NULL,
  `Icon` varchar(100) DEFAULT NULL,
  `Badge` varchar(100) DEFAULT NULL,
  `BadgeType` varchar(100) DEFAULT NULL,
  `AccessCode` bigint NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`AccessCode`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `salary_components`
--

DROP TABLE IF EXISTS `salary_components`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `salary_components` (
  `ComponentId` varchar(10) NOT NULL,
  `ComponentFullName` varchar(150) DEFAULT NULL,
  `ComponentDescription` varchar(1024) DEFAULT NULL,
  `CalculateInPercentage` bit(1) DEFAULT NULL,
  `TaxExempt` bit(1) DEFAULT NULL,
  `ComponentTypeId` int DEFAULT NULL,
  `ComponentCatagoryId` int DEFAULT NULL,
  `PercentageValue` decimal(10,0) DEFAULT NULL,
  `MaxLimit` decimal(10,0) DEFAULT NULL,
  `DeclaredValue` decimal(10,0) DEFAULT NULL,
  `AcceptedAmount` decimal(10,0) DEFAULT '0',
  `RejectedAmount` decimal(10,0) DEFAULT '0',
  `UploadedFileIds` json DEFAULT NULL,
  `Formula` varchar(100) DEFAULT NULL,
  `EmployeeContribution` decimal(10,0) DEFAULT NULL,
  `EmployerContribution` decimal(10,0) DEFAULT NULL,
  `IncludeInPayslip` bit(1) DEFAULT NULL,
  `IsAdHoc` bit(1) DEFAULT NULL,
  `AdHocId` int DEFAULT NULL,
  `Section` varchar(20) DEFAULT NULL,
  `SectionMaxLimit` decimal(10,0) DEFAULT NULL,
  `IsAffectInGross` bit(1) DEFAULT NULL,
  `RequireDocs` bit(1) DEFAULT NULL,
  `IsOpted` bit(1) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  PRIMARY KEY (`ComponentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `salary_group`
--

DROP TABLE IF EXISTS `salary_group`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `salary_group` (
  `SalaryGroupId` int NOT NULL AUTO_INCREMENT,
  `SalaryComponents` json DEFAULT NULL,
  `GroupName` varchar(45) DEFAULT NULL,
  `GroupDescription` varchar(250) DEFAULT NULL,
  `MinAmount` decimal(10,0) DEFAULT NULL,
  `MaxAmount` decimal(10,0) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `CompanyId` int DEFAULT NULL,
  PRIMARY KEY (`SalaryGroupId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sample`
--

DROP TABLE IF EXISTS `sample`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sample` (
  `Username` varchar(50) NOT NULL,
  `Password` varchar(50) NOT NULL,
  `Rollno` int DEFAULT NULL,
  `CreatedOn` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sector_type`
--

DROP TABLE IF EXISTS `sector_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sector_type` (
  `SectorTypeId` int NOT NULL AUTO_INCREMENT,
  `SectorName` varchar(150) DEFAULT NULL,
  `SectorType` varchar(150) DEFAULT NULL,
  PRIMARY KEY (`SectorTypeId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `selectsamplequery`
--

DROP TABLE IF EXISTS `selectsamplequery`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `selectsamplequery` (
  `TableIndex` int NOT NULL,
  `SelectQuery` varchar(500) NOT NULL,
  PRIMARY KEY (`TableIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sportsname`
--

DROP TABLE IF EXISTS `sportsname`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sportsname` (
  `RowIndex` int NOT NULL,
  `SportName` varchar(100) NOT NULL,
  `StartedOn` datetime(6) DEFAULT NULL,
  `StartedBy` varchar(100) DEFAULT NULL,
  `Country` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`RowIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sqlmapperdatatype`
--

DROP TABLE IF EXISTS `sqlmapperdatatype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sqlmapperdatatype` (
  `SNo` int NOT NULL,
  `ClientTypeName` varchar(100) NOT NULL,
  `SqlTypeName` varchar(100) NOT NULL,
  `TableName` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`SNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `students`
--

DROP TABLE IF EXISTS `students`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `students` (
  `StudentUid` varchar(64) NOT NULL,
  `ClassTeacherUid` varchar(64) NOT NULL,
  `StudentFirstName` varchar(50) NOT NULL,
  `StudentLastName` varchar(50) NOT NULL,
  `FatherName` varchar(100) DEFAULT NULL,
  `MotherName` varchar(100) DEFAULT NULL,
  `Class` varchar(4) DEFAULT NULL,
  `Section` char(1) DEFAULT NULL,
  `Address` varchar(250) DEFAULT NULL,
  `City` varchar(50) DEFAULT NULL,
  `State` varchar(50) DEFAULT NULL,
  `Pincode` varchar(100) DEFAULT NULL,
  `NickName` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`StudentUid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tablecolumnmapping`
--

DROP TABLE IF EXISTS `tablecolumnmapping`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tablecolumnmapping` (
  `MappingId` bigint NOT NULL AUTO_INCREMENT,
  `PageName` varchar(100) DEFAULT NULL,
  `ColumnName` varchar(50) DEFAULT NULL,
  `DisplayName` varchar(50) DEFAULT NULL,
  `Style` varchar(250) DEFAULT NULL,
  `ClassName` varchar(100) DEFAULT NULL,
  `IsHidden` bit(1) DEFAULT NULL,
  PRIMARY KEY (`MappingId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usercomments`
--

DROP TABLE IF EXISTS `usercomments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usercomments` (
  `CommentsId` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint DEFAULT NULL,
  `UserName` varchar(100) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Title` varchar(50) NOT NULL,
  `Comments` varchar(500) NOT NULL,
  `CreatedOn` datetime(6) NOT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  `Company` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`CommentsId`),
  KEY `FK_UserComments_UserId` (`UserId`),
  CONSTRAINT `FK_UserComments_UserId` FOREIGN KEY (`UserId`) REFERENCES `userdetail` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `userdetail`
--

DROP TABLE IF EXISTS `userdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userdetail` (
  `UserId` bigint NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Address` varchar(250) DEFAULT NULL,
  `EmailId` varchar(100) NOT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `CompanyName` varchar(100) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `UQ__UserDeta__B916E1C73D6300B3` (`EmailId`),
  UNIQUE KEY `UQ__UserDeta__B916E1C7675CE8E9` (`EmailId`),
  UNIQUE KEY `UQ__UserDeta__D037FED5ECCB2FF4` (`Mobile`),
  UNIQUE KEY `UQ__UserDeta__D037FED5B0CF58A8` (`Mobile`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `userfiledetail`
--

DROP TABLE IF EXISTS `userfiledetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userfiledetail` (
  `FileId` bigint NOT NULL AUTO_INCREMENT,
  `FileOwnerId` bigint NOT NULL,
  `FilePath` varchar(500) NOT NULL,
  `ParentFolder` varchar(500) DEFAULT NULL,
  `FileName` varchar(100) NOT NULL,
  `FileExtension` varchar(100) DEFAULT NULL,
  `ItemStatusId` bigint DEFAULT NULL,
  `UserTypeId` int NOT NULL DEFAULT '0',
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime(6) DEFAULT NULL,
  `UpdatedOn` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`FileId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `userlogin`
--

DROP TABLE IF EXISTS `userlogin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userlogin` (
  `UserId` bigint NOT NULL AUTO_INCREMENT,
  `UserDetailId` mediumtext,
  `EmployeeId` mediumtext,
  `UserTypeId` int DEFAULT '1',
  `AccessLevelId` bigint DEFAULT NULL,
  `Password` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Mobile` varchar(14) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `UpdatedBy` bigint DEFAULT NULL,
  `CreatedOn` datetime DEFAULT NULL,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Email` (`Email`),
  UNIQUE KEY `Mobile` (`Mobile`),
  KEY `FK_UserLogin_AccessLevelId` (`AccessLevelId`),
  KEY `FK_UserLogin_CreatedBy` (`CreatedBy`),
  KEY `FK_UserLogin_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_UserLogin_AccessLevelId` FOREIGN KEY (`AccessLevelId`) REFERENCES `accesslevel` (`AccessLevelId`),
  CONSTRAINT `FK_UserLogin_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `userdetail` (`UserId`),
  CONSTRAINT `FK_UserLogin_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `userdetail` (`UserId`),
  CONSTRAINT `FK_UserLogin_UserId` FOREIGN KEY (`UserId`) REFERENCES `userdetail` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usertypedetail`
--

DROP TABLE IF EXISTS `usertypedetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usertypedetail` (
  `UserTypeId` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) DEFAULT NULL,
  `RoleDescription` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`UserTypeId`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `vehicleandbrands`
--

DROP TABLE IF EXISTS `vehicleandbrands`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vehicleandbrands` (
  `RowIndex` int NOT NULL,
  `VehicleName` varchar(250) NOT NULL,
  `BrandName` varchar(100) NOT NULL,
  `OwnerName` varchar(250) DEFAULT NULL,
  `CreateOn` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`RowIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `worldcities`
--

DROP TABLE IF EXISTS `worldcities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `worldcities` (
  `RowIndex` double DEFAULT NULL,
  `City` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CityAscii` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Lat` double DEFAULT NULL,
  `Lng` double DEFAULT NULL,
  `Country` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ISO2` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ISO3` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `State` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Capital` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Population` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;



# Sample email template detai  (OPTIONAL)
Insert into email_templates values(default, 'Billing Template', 'Developer(s) bill detail', 
'Dear Sir/Madam', 'Thanks & Regards,', 
'{"FirstPhase":["GENERATED STAFFING BILL FOR THE LAST MONTH","BILLING DETAIL AS FOLLOWS:"],"Body":[],"EndPhase":["PLEASE FIND ATTACHED PDF BILL"]}', 
'Please check detail if required any changes do revert to us.', 'Team BottomHalf.',
'9100544384', null, 1, utc_date());


# Inserting initial bill information.
Insert into billtype values(1, 'Bill number sequence for generating payment to clients');
Insert into bills values(1, 1, 0, 8, 1);

#insert user type deatils            
INSERT INTO `usertypedetail` VALUES 
(1,'Admin','Administrator will complete access granted.'),
(2,'Employee','Employee level access.'),
(3,'Candidate','Candidate level access.'),
(4,'Client','Candidate level access.'),
(5,'Other','Other level access.'),
(6,'Company','Company related access similar to Admin. This type will be used only for file saving purpose.');

# inserting roles and menu
INSERT INTO `rolesandmenu` VALUES 
('Administration',NULL,NULL,NULL,NULL,NULL,1),
('Dashboard','Administration','dashboard','fa fa-tachometer',NULL,NULL,2),
('Employees','Administration','employees','fa fa-id-card',NULL,NULL,4),
('Client','Administration','clients','fa fa-building-o',NULL,NULL,5),
('Bills','Administration','billdetail','fa fa-file-o',NULL,NULL,7),
('Home',NULL,NULL,NULL,NULL,NULL,8),
('Profile','Manage','profile','fa fa-user',NULL,NULL,9),
('Roles','Settings','roles','fa fa-object-group',NULL,NULL,10),
('Generate Bill','Administration','generatebill','fa fa-file-pdf-o',NULL,NULL,11),
('Attendence','Manage','attendance','fa fa-id-badge',NULL,NULL,12),
('Manage',NULL,NULL,NULL,NULL,NULL,13),
('Declaration','Accounts','declaration','fa fa-handshake-o',NULL,NULL,14),
('Salary','Accounts','salary','fa fa-money',NULL,NULL,15),
('Summary','Accounts','summary','fa fa-history',NULL,NULL,16),
('Preferences','Accounts','preferences','fa fa-object-group',NULL,NULL,17),
('Dashboard','Home','dashboard','fa fa-tachometer',NULL,NULL,18),
('Accounts',NULL,NULL,NULL,NULL,NULL,19),
('Timesheet','Manage','timesheet','fa fa-calendar',NULL,NULL,20),
('Holidays','Manage','planholidays','fa fa-calendar-minus-o',NULL,NULL,21),
('About Me','Home','profile','fa fa-user',NULL,NULL,22),
('Tax','Accounts','taxcalculation','fa fa-money',NULL,NULL,23),
('Request','Team','request','fa fa-hand-o-right',NULL,NULL,24),
('Team',NULL,NULL,NULL,NULL,NULL,25),
('Notification','Team','notification','fa fa-bell-o',NULL,NULL,26),
('Settings',NULL,NULL,NULL,NULL,NULL,28),
('Leave Plan','Settings','leave','fa fa-calendar-check-o',NULL,NULL,29),
('Payroll','Settings','payrollsettings','fa fa-cog fa-spin fa-fw',NULL,NULL,30),
('Email Setting','Settings','emailsetting','fa fa-envelope',NULL,NULL,31),
('Menu','Settings','menu','fa fa-bars',NULL,NULL,32),
('Project','Team','project','fa fa-lightbulb-o',NULL,NULL,33),
('Company','Settings','companysettings','fa fa-cogs',NULL,NULL,34),
('Leave','Manage','leave','fa fa-calendar-check-o',NULL,NULL,35); 


# Insert designation deatils

Insert into designation_detail values
(1, 'Admin', 'Full access at organization level', 0, null, null),
(2, 'Project Manager', 'Project Manager', 0, null, null),
(3, 'Solution Architect', 'Solution Architect', 0, null, null),
(4, 'Application Architect', 'Application Architect', 0, null, null),
(5, 'Team Lead', 'Team Lead', 0, null, null),
(6, 'Full stact developer', 'Full stact developer', 0, null, null),
(7, 'Senior Software engineer', 'Sr. Software engineer', 0, null, null),
(8, 'Junior Software engineer', 'Junior Software engineer', 0, null, null),
(9, 'Software Associate', 'Software Associate', 0, null, null),
(10, 'Tester', 'Tester', 0, null, null),
(11, 'HR Manager', 'HR Manager', 0, null, null),
(12, 'System Engineer', 'System Engineer', 0, null, null);


#Inserting accessibility description

INSERT INTO `accessibility_description` VALUES 
(1,'Complete access. (read, udpate and detele)'),
(2,'Only read access.'),
(3,'Read and update access.');

# inserting access level
INSERT INTO `accesslevel` VALUES 
(1,'Admin','Having all rights',utc_date(),NULL);


# Insert Admin first user
set @value = '';
Call sp_new_registration('BOTTOMHALF', 'BOTTOMHALF PVT. LTD.', '9100544384','info@bottomhalf.in', 'ADMIN', 'USER', "EiOcNOYYuHiQrEc0z16rEjUlp71vYq73fNDXL1PbZZ4=", @value);
select @value;