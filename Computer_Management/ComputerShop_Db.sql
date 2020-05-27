create database Computer_Management_System
go
use Computer_Management_System
drop table Account
create table Account(
    UserID int IDENTITY(1,1) PRIMARY KEY,
	UserName varchar(50),  
    Password varchar(50),  
	email varchar(50),

	Role int
)
	

