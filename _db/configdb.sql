create table Applications (
    PathHash binary(16) primary key,
    Path nvarchar(2000) not null,
    Name nvarchar(500) not null,
    IsExcluded bit not null,
    IsHidden bit not null
);

create table ApplicationConfigs (
    PathHash binary(16) not null,
    Path nvarchar(2000) not null,
    Server nvarchar(200) not null,
    Binding nvarchar(3000) not null,
    AppPoolName nvarchar(500),
    AppType nchar(3),
    ServiceName nvarchar(300),
    DisplayName nvarchar(500),
    primary key (PathHash, Server)
);
