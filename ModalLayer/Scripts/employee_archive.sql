drop table if exists employee_archive;

CREATE TABLE `employee_archive` (
  `EmployeeId` bigint NOT NULL,
  FullName varchar(150),
  Mobile varchar(20),
  Email varchar(45),
  Package decimal,
  DateOfJoining datetime null,
  DateOfLeaving datetime null,
  `EmployeeCompleteJsonData` json NOT NULL,
  `CreatedBy` long,
  `CreatedOn` datetime DEFAULT NULL,
  PRIMARY KEY (`EmployeeId`)
);
