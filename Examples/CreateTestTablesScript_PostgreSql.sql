CREATE TABLE "TestDataTable"(
	"Id" uuid PRIMARY KEY,
	"Name" VARCHAR (50) NOT NULL,
	"CreationDate" TIMESTAMP NOT NULL,
	"Active" boolean NOT NULL
);

CREATE TABLE "TestDataChild"(
	"Id" uuid PRIMARY KEY,
	"IdParent" uuid NOT NULL,
	"Name" VARCHAR (50) NOT NULL,
	"CreationDate" TIMESTAMP NOT NULL,
	"Active" boolean NOT NULL
);

CREATE TABLE "TestDataAttr"(
	"Id" uuid PRIMARY KEY,
	"IsBeautiful" boolean NOT NULL
);

CREATE USER "fluenthelperexample" WITH PASSWORD 'fhe2k23!';
GRANT ALL ON "TestDataTable" TO fluenthelperexample;
GRANT ALL ON "TestDataChild" TO fluenthelperexample;
GRANT ALL ON "TestDataAttr" TO fluenthelperexample;