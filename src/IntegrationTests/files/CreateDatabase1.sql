USE Master;

IF DB_ID('Test1') IS NOT NULL
	DROP DATABASE Test1;

CREATE DATABASE Test1;

USE Test1;

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
	('System Access Control');

INSERT INTO Question(CategoryId,Question,UpdatedOn,UpdatedBy) VALUES
	(1,'Does you have formal information security policies that are enforced?  If so, please explain.',GETUTCDATE(),'Dale'),
	(2,'Do you conduct security awareness training?  If so, how often?',GETUTCDATE(),'Dale');

INSERT INTO Answer(QuestionId,Answer,UpdatedOn,UpdatedBy) VALUES
	(1,'Yes.  We monitor everything with our "slower-downers." When we find a violation, we head upstairs and lay down the law (gangster style).',GETUTCDATE(),'Dale'),
	(2,'Yes.  We have annual training at our Christmas party.',GETUTCDATE(),'Dale');

