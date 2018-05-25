USE Master;

IF DB_ID('Test2') IS NOT NULL
	DROP DATABASE Test2;

CREATE DATABASE Test2;

USE Test2;

CREATE TABLE Category(
	Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Name] NVARCHAR(100) NOT NULL
);

CREATE TABLE Question(
	Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	CategoryId INT NOT NULL,
	Question NVARCHAR(2000) NOT NULL,
	UpdatedOn DATETIME NOT NULL,
	UpdatedBy NVARCHAR(64) NOT NULL
);

CREATE TABLE Answer(
	Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	QuestionId INT NOT NULL,
	Answer NVARCHAR(4000) NOT NULL,
	UpdatedOn DATETIME NOT NULL,
	UpdatedBy NVARCHAR(64) NOT NULL
);

INSERT INTO Category([Name]) VALUES 
	('Information Security Policy'), 
	('System Access Control'),
	('Daryl');

INSERT INTO Question(CategoryId,Question,UpdatedOn,UpdatedBy) VALUES
	(1,'Does you have formal information security policies that are enforced?  If so, please explain.',GETUTCDATE(),'Daryl'),
	(2,'Do you conduct security awareness training?  If so, how often?',GETUTCDATE(),'Daryl'),
	(3,'How old is Daryl?',GETUTCDATE(),'Daryl');

INSERT INTO Answer(QuestionId,Answer,UpdatedOn,UpdatedBy) VALUES
	(1,'Yes.  However, our definition of formal is "if you heard me say it once, you better do it."',GETUTCDATE(),'Daryl'),
	(2,'Yes.  John gives a talk every Friday afternoon.',GETUTCDATE(),'Daryl'),
	(3,'Daryl is pretty old.',GETUTCDATE(),'Dale');


