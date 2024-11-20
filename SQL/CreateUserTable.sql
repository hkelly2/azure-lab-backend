create table [User]
(
    id       int IDENTITY(1,1) PRIMARY KEY,
    username varchar(100) null,
    name     varchar(100) null,
    email    varchar(100) null,
    password varchar(100) null
);

INSERT INTO [User] VALUES ('jeff', 'Jeff Maxwell', 'jmaxwell@okcu.edu', 'password');