drop table if exists designation_detail; 

create table designation_detail(
	DesignationId int primary key auto_increment,
    DesignationName varchar(150) not null,
    RoleDescription text,
    CompanyId int,
    UpdatedBy bigint null,
    UpdatedOn datetime null
);

drop table if exists professionaldetail; 

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
) ENGINE=InnoDB AUTO_INCREMENT=90 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;



CREATE TABLE `organization_detail` (
  `OrganizationId` int NOT NULL AUTO_INCREMENT,
  `OrganizationName` varchar(250) DEFAULT NULL,
  `OrgMobileNo` varchar(20) DEFAULT NULL,
  `OrgEmail` varchar(50) DEFAULT NULL,
  `OrgPrimaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgSecondaryPhoneNo` varchar(20) DEFAULT NULL,
  `OrgFax` varchar(50) DEFAULT NULL,
  `UpdatedBy` mediumtext,
  `UpdatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`OrganizationId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


ALTER TABLE `onlinedatabuilder`.`employeelogin` 
DROP FOREIGN KEY `FK_employeelogin_AccessLevelId`;
ALTER TABLE `onlinedatabuilder`.`employeelogin` 
DROP INDEX `FK_UserLogin_AccessLevelId` ;
;


LOCK TABLES `rolesandmenu` WRITE;
/*!40000 ALTER TABLE `rolesandmenu` DISABLE KEYS */;
INSERT INTO `rolesandmenu` VALUES ('Administration',NULL,NULL,NULL,NULL,NULL,1),('Dashboard','Administration','dashboard','fa fa-tachometer',NULL,NULL,2),('Documents','Administration','documents','fa fa-folder-open',NULL,NULL,3),('Employees','Administration','employees','fa fa-id-card',NULL,NULL,4),('Client','Administration','clients','fa fa-building-o',NULL,NULL,5),('Resume','Administration','resumes','fa fa-file-word-o',NULL,NULL,6),('Bills','Administration','billdetail','fa fa-file-o',NULL,NULL,7),('Home',NULL,NULL,NULL,NULL,NULL,8),('Profile','Manage','profile','fa fa-user',NULL,NULL,9),('Roles','Administration','roles','fa fa-object-group',NULL,NULL,10),('Build PDF','Administration','BuildPdf','fa fa-file-pdf-o',NULL,NULL,11),('Attendence','Manage','attendance','fa fa-id-badge',NULL,NULL,12),('Manage',NULL,NULL,NULL,NULL,NULL,13),('Declaration','Accounts','declaration','fa fa-handshake-o',NULL,NULL,14),('Salary','Accounts','salary','fa fa-money',NULL,NULL,15),('Summary','Accounts','summary','fa fa-history',NULL,NULL,16),('Preferences','Accounts','preferences','fa fa-object-group',NULL,NULL,17),('Dashboard','Home','dashboard','fa fa-tachometer',NULL,NULL,18),('Accounts',NULL,NULL,NULL,NULL,NULL,19),('Timesheet','Manage','timesheet','fa fa-calendar',NULL,NULL,20),('Leave','Manage','leave','fa fa-calendar-minus-o',NULL,NULL,21),('About Me','Home','profile','fa fa-user',NULL,NULL,22),('Tax','Accounts','taxcalculation','fa fa-money',NULL,NULL,23),('Request','Team','request','fa fa-hand-o-right',NULL,NULL,24),('Notification','Team','notification','fa fa-bell-o',NULL,NULL,25),('Team',NULL,NULL,NULL,NULL,NULL,26),('Settings','Administration','settings','fa fa-cog',NULL,NULL,27);
/*!40000 ALTER TABLE `rolesandmenu` ENABLE KEYS */;
UNLOCK TABLES;


LOCK TABLES `usertypedetail` WRITE;
/*!40000 ALTER TABLE `usertypedetail` DISABLE KEYS */;
INSERT INTO `usertypedetail` VALUES (1,'Admin','Administrator will complete access granted.'),(2,'Employee','Employee level access.'),(3,'Candidate','Candidate level access.'),(4,'Client','Candidate level access.'),(5,'Other','Other level access.'),(6,'Company','Company related access similar to Admin. This type will be used only for file saving purpose.');
/*!40000 ALTER TABLE `usertypedetail` ENABLE KEYS */;
UNLOCK TABLES;

Insert into designation_detail values
(default, 'Admin', 'Full access at organization level', 0, null, null),
(default, 'Project Manager', 'Project Manager', 0, null, null),
(default, 'Solution Architect', 'Solution Architect', 0, null, null),
(default, 'Application Architect', 'Application Architect', 0, null, null),
(default, 'Team Lead', 'Team Lead', 0, null, null),
(default, 'Full stact developer', 'Full stact developer', 0, null, null),
(default, 'Senior Software engineer', 'Sr. Software engineer', 0, null, null),
(default, 'Junior Software engineer', 'Junior Software engineer', 0, null, null),
(default, 'Software Associate', 'Software Associate', 0, null, null),
(default, 'Tester', 'Tester', 0, null, null),
(default, 'HR Manager', 'HR Manager', 0, null, null),
(default, 'System Engineer', 'System Engineer', 0, null, null);