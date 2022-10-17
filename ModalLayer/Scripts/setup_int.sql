# Sample email template detai
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