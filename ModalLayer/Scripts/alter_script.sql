DELIMITER $$

drop procedure if exists sp_new_registration $$

CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_new_registration`(

/*

	Call sp_new_registration(0, NULL, 'istiyaq.mi9@gmail.com', 1, 'istiyak');

*/
	_OrganizationName varchar(250),
	_CompanyName varchar(250),
	_Mobile varchar(20),
	_EmailId varchar(100),
    _FirstName varchar(100),
    _LastName varchar(100),
	_Password varchar(250),
    out _ProcessingResult varchar(50)
)
Begin
    Begin
		Declare Exit handler for sqlexception
		Begin
			Get Diagnostics condition 1 @sqlstate = RETURNED_SQLSTATE,
			@errorno = MYSQL_ERRNO,
			@errortext = MESSAGE_TEXT;
            
            RollBack;
			Set @Message = concat ('ERROR ', @errorno ,  ' (', @sqlstate, '); ', @errortext);
			Call sp_LogException (@Message, '', 'sp_new_registration', 1, 0, @Result);
		end;  
        
        Start Transaction;
        begin
			if not exists(select 1 from organization_detail where OrganizationName = _OrganizationName) then
			begin
				set @userTypeId = 0;
				set @accessLevelId = 1;
				select UserTypeId into @userTypeId from usertypedetail
				where RoleName = 'Admin';
				
				set @designationId = 0;
				select DesignationId into @designationId from designation_detail
				where DesignationName = 'Admin';
			
				set @organizationId = 0;
				Set @@SESSION.information_schema_stats_expiry = 0;
				SELECT AUTO_INCREMENT into @organizationId
				FROM information_schema.tables
				WHERE table_name = 'organization_detail'
				AND table_schema = DATABASE();	
	
				Insert into organization_detail values(
					@organizationId,
					_OrganizationName,
					_Mobile,
					_EmailId,
					_Mobile,
					null,
					null,
					null,
					utc_date()
				);            
				
				set @companyId = 0;
				Set @@SESSION.information_schema_stats_expiry = 0;
				SELECT AUTO_INCREMENT into @companyId
				FROM information_schema.tables
				WHERE table_name = 'compnay'
				AND table_schema = DATABASE();	
						
				Insert into company values(            
					@companyId,
					@organizationId,
					_OrganizationName,
					_CompanyName,
					null,
					0,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					_Mobile,
					_EmailId,
					null,
					null,
					null,
					null,
					_Mobile,
					null,
					null,
					0,
					0,
					null,
					null,
					null,
					null,
					null,
					true,
					null,
					0,
					null,
					utc_date(),
					null
				);
				
				set @employeeId = 0;
				Set @@SESSION.information_schema_stats_expiry = 0;
				SELECT AUTO_INCREMENT into @employeeId
				FROM information_schema.tables
				WHERE table_name = 'employees'
				AND table_schema = DATABASE();	
					
				Insert into employees values(
					@employeeId,
					_FirstName,
					_LastName,
					_Mobile,
					_EmailId,
					true,
					0,
					null,
					utc_date(),
					null,
					0,
					@designationId,
					@userTypeId,
					0,
					0,
                    0,
					@CompnayId,
					0
				);

				Insert into employeelogin values(
					default,
					@employeeId,
					@userTypeId,
					@accessLevelId,
					_Password,
					_EmailId,
					_Mobile,
					@organizatioId,
					@companyId,
					0,
					null,
					utc_date(),
					null
				);
				
				set _ProcessingResult = 'updated';
			end;
			end if;
            
            INSERT INTO `rolesandmenu` VALUES 
            ('Administration',NULL,NULL,NULL,NULL,NULL,1),
            ('Dashboard','Administration','dashboard','fa fa-tachometer',NULL,NULL,2),
            ('Documents','Administration','documents','fa fa-folder-open',NULL,NULL,3),
            ('Employees','Administration','employees','fa fa-id-card',NULL,NULL,4),
            ('Client','Administration','clients','fa fa-building-o',NULL,NULL,5),
            ('Resume','Administration','resumes','fa fa-file-word-o',NULL,NULL,6),
            ('Bills','Administration','billdetail','fa fa-file-o',NULL,NULL,7),
            ('Home',NULL,NULL,NULL,NULL,NULL,8),
            ('Profile','Manage','profile','fa fa-user',NULL,NULL,9),
            ('Roles','Administration','roles','fa fa-object-group',NULL,NULL,10),
            ('Build PDF','Administration','BuildPdf','fa fa-file-pdf-o',NULL,NULL,11),
            ('Attendence','Manage','attendance','fa fa-id-badge',NULL,NULL,12),
            ('Manage',NULL,NULL,NULL,NULL,NULL,13),
            ('Declaration','Accounts','declaration','fa fa-handshake-o',NULL,NULL,14),
            ('Salary','Accounts','salary','fa fa-money',NULL,NULL,15),
            ('Summary','Accounts','summary','fa fa-history',NULL,NULL,16),
            ('Preferences','Accounts','preferences','fa fa-object-group',NULL,NULL,17),
            ('Dashboard','Home','dashboard','fa fa-tachometer',NULL,NULL,18),
            ('Accounts',NULL,NULL,NULL,NULL,NULL,19),
            ('Timesheet','Manage','timesheet','fa fa-calendar',NULL,NULL,20),
            ('Leave','Manage','leave','fa fa-calendar-minus-o',NULL,NULL,21),
            ('About Me','Home','profile','fa fa-user',NULL,NULL,22),
            ('Tax','Accounts','taxcalculation','fa fa-money',NULL,NULL,23),
            ('Request','Team','request','fa fa-hand-o-right',NULL,NULL,24),
            ('Notification','Team','notification','fa fa-bell-o',NULL,NULL,25),
            ('Team',NULL,NULL,NULL,NULL,NULL,26),
            ('Settings','Administration','settings','fa fa-cog',NULL,NULL,27);
            
			INSERT INTO `usertypedetail` VALUES 
            (1,'Admin','Administrator will complete access granted.'),
            (2,'Employee','Employee level access.'),
            (3,'Candidate','Candidate level access.'),
            (4,'Client','Candidate level access.'),
            (5,'Other','Other level access.'),
            (6,'Company','Company related access similar to Admin. This type will be used only for file saving purpose.');
							
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

			INSERT INTO `accessibility_description` VALUES 
            (1,'Complete access. (read, udpate and detele)'),
            (2,'Only read access.'),
            (3,'Read and update access.');

			INSERT INTO `accesslevel` VALUES 
            (1,'Admin','Having all rights','2021-08-17 22:25:53',NULL),
            (2,'User','View or edit only personal detail.','2021-08-17 22:26:35','2022-03-13 08:48:35'),
            (3,'Management','Can add or edit most of the detail.','2022-03-13 08:49:18',NULL);
        end;
        Commit;
	End;
End$$
DELIMITER ;












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
