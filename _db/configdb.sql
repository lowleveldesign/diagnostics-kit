create table Applications (
    PathHash binary(16) primary key,
    Path varchar(2000) not null,
    Name varchar(500) not null,
    IsExcluded bit not null,
    IsHidden bit not null
);

create table ApplicationConfigs (
    PathHash binary(16) not null,
    Path varchar(2000) not null,
    Server varchar(200) not null,
    Binding varchar(3000) not null,
    AppPoolName varchar(500),
    AppType char(3),
    ServiceName varchar(300),
    DisplayName varchar(500),
    primary key (PathHash, Server)
);

create table Users (
    Id varchar(32) not null primary key,
    UserName varchar(100) not null,
    Email varchar(100) not null,
    PasswordHash varchar(1000) null,
    Enabled bit not null,
    RegistrationDateUtc DateTime not null
);

create table UserClaims (
    UserId varchar(32) not null,
    ClaimType varchar(250) not null,
    ClaimValue varchar(1000) not null,
    primary key(UserId, ClaimType)
);

create unique nonclustered index NCIX_Users_UserName on Users(UserName);
create unique nonclustered index NCIX_Users_Email on Users(Email);
