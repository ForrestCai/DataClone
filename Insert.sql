insert into DB_Master (Code, Description) values ('D1', 'DebtorOne')
insert into DB_Master (Code, Description) values ('D2', 'DebtorTwo')
insert into DB_Master (Code, Description) values ('D3', 'DebtorThree')
insert into DB_Master (Code, Description) values ('D4', 'DebtorFour')

insert into OP_DebtorOption (DebtorCode, Description) values ('D1', 'D1OOne')
insert into OP_DebtorOption (DebtorCode, Description) values ('D1', 'D1OTwo')
insert into OP_DebtorOption (DebtorCode, Description) values ('D2', 'D2OOne')
insert into OP_DebtorOption (DebtorCode, Description) values ('D2', 'D2OTwo')
insert into OP_DebtorOption (DebtorCode, Description) values ('D3', 'D3OOne')
insert into OP_DebtorOption (DebtorCode, Description) values ('D3', 'D3OTwo')
insert into OP_DebtorOption (DebtorCode, Description) values ('D4', 'D4OOne')
insert into OP_DebtorOption (DebtorCode, Description) values ('D4', 'D4OTwo')

insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P1', 1, 'POne', null, null, null, null, null)
insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P2', 1, 'PTwo', 22, 1, '2016-03-05 17:30:56.120', 22.22, 22.222)
insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P3', 2, 'PThree', 33, 0, '2016-03-05 17:30:56.120', 33.33, 33.333)
insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P4', 2, 'PFour', null, null, null, null, null)
insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P5', 3, 'PFive', null, null, null, null, null)
insert into CL_Profile (Code, DebtorId, Description, TestSmallInt, TestBit, TestDateTime, TestMoney, TestFload) 
values ('P6', 3, 'PSix', null, null, null, null, null)

insert into US_User (Code, DebtorId, Name) values ('U1', 1, 'UOne')
insert into US_User (Code, DebtorId, Name) values ('U2', 1, 'UTwo')
insert into US_User (Code, DebtorId, Name) values ('U3', 2, 'UThree')
insert into US_User (Code, DebtorId, Name) values ('U4', 2, 'UFour')
insert into US_User (Code, DebtorId, Name) values ('U5', 3, 'UFive')
insert into US_User (Code, DebtorId, Name) values ('U6', 3, 'USix')
insert into US_User (Code, DebtorId, Name) values ('U7', 4, 'USeven')
insert into US_User (Code, DebtorId, Name) values ('U8', 4, 'UNine')

insert into UP_User_Profile (UserId, ProfileId) values (1, 1)
insert into UP_User_Profile (UserId, ProfileId) values (1, 2)
insert into UP_User_Profile (UserId, ProfileId) values (2, 1)
insert into UP_User_Profile (UserId, ProfileId) values (3, 3)
insert into UP_User_Profile (UserId, ProfileId) values (3, 4)
insert into UP_User_Profile (UserId, ProfileId) values (4, 3)
insert into UP_User_Profile (UserId, ProfileId) values (5, 5)
insert into UP_User_Profile (UserId, ProfileId) values (5, 6)
insert into UP_User_Profile (UserId, ProfileId) values (6, 5)

insert into PE_Profile_Email (ProfileId, Email) values (1, 'P11@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (1, 'P12@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (2, 'P21@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (3, 'P31@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (3, 'P32@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (4, 'P41@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (5, 'P51@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (5, 'P52@SERKO.COM')
insert into PE_Profile_Email (ProfileId, Email) values (6, 'P61@SERKO.COM')

insert into OP_ProfileOption (Code, Description) values ('PO1', 'POpOne')
insert into OP_ProfileOption (Code, Description) values ('PO2', 'POpTwo')

insert into PO_ProfileOption_Data (ProfileId, OptionId, Data) values (1, 1, 'POneOPOne')
insert into PO_ProfileOption_Data (ProfileId, OptionId, Data) values (1, 2, 'POneOPTwo')
insert into PO_ProfileOption_Data (ProfileId, OptionId, Data) values (2, 1, 'PTwoOPOne')
insert into PO_ProfileOption_Data (ProfileId, OptionId, Data) values (3, 1, 'PThreeOPOne')
insert into PO_ProfileOption_Data (ProfileId, OptionId, Data) values (5, 1, 'PFiveOPOne')

insert into DB_Link (FromDebtorId, ToDebtorId) values (1, null)
insert into DB_Link (FromDebtorId, ToDebtorId) values (2, 1)
insert into DB_Link (FromDebtorId, ToDebtorId) values (3, 2)
insert into DB_Link (FromDebtorId, ToDebtorId) values (4, 3)