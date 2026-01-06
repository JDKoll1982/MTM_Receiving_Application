MySQL 5.7 Error Message Reference
Abstract
This is the MySQL 5.7 Error Message Reference. It lists all error messages produced by server and client programs in
MySQL 5.7. This document accompanies Error Messages and Common Problems, in MySQL 5.7 Reference Manual.
For help with using MySQL, please visit the MySQL Forums, where you can discuss your issues with other MySQL
users.
Document generated on: 2025-12-31 (revision: 84186)
Table of Contents
Preface and Legal Notices .................................................................................................................. v
1 MySQL Error Reference .................................................................................................................. 1
2 Server Error Message Reference ..................................................................................................... 3
3 Client Error Message Reference .................................................................................................... 99
4 Global Error Message Reference ................................................................................................. 105
Index .............................................................................................................................................. 109
iii
iv
Preface and Legal Notices
This is the MySQL 5.7 Error Message Reference. It lists all error messages produced by server and client
programs in MySQL 5.7.
Legal Notices
Copyright © 1997, 2026, Oracle and/or its affiliates.
License Restrictions
This software and related documentation are provided under a license agreement containing restrictions
on use and disclosure and are protected by intellectual property laws. Except as expressly permitted
in your license agreement or allowed by law, you may not use, copy, reproduce, translate, broadcast,
modify, license, transmit, distribute, exhibit, perform, publish, or display any part, in any form, or by any
means. Reverse engineering, disassembly, or decompilation of this software, unless required by law for
interoperability, is prohibited.
Warranty Disclaimer
The information contained herein is subject to change without notice and is not warranted to be error-free.
If you find any errors, please report them to us in writing.
Restricted Rights Notice
If this is software, software documentation, data (as defined in the Federal Acquisition Regulation), or
related documentation that is delivered to the U.S. Government or anyone licensing it on behalf of the U.S.
Government, then the following notice is applicable:
U.S. GOVERNMENT END USERS: Oracle programs (including any operating system, integrated
software, any programs embedded, installed, or activated on delivered hardware, and modifications
of such programs) and Oracle computer documentation or other Oracle data delivered to or accessed
by U.S. Government end users are "commercial computer software," "commercial computer software
documentation," or "limited rights data" pursuant to the applicable Federal Acquisition Regulation and
agency-specific supplemental regulations. As such, the use, reproduction, duplication, release, display,
disclosure, modification, preparation of derivative works, and/or adaptation of i) Oracle programs (including
any operating system, integrated software, any programs embedded, installed, or activated on delivered
hardware, and modifications of such programs), ii) Oracle computer documentation and/or iii) other Oracle
data, is subject to the rights and limitations specified in the license contained in the applicable contract.
The terms governing the U.S. Government's use of Oracle cloud services are defined by the applicable
contract for such services. No other rights are granted to the U.S. Government.
Hazardous Applications Notice
This software or hardware is developed for general use in a variety of information management
applications. It is not developed or intended for use in any inherently dangerous applications, including
applications that may create a risk of personal injury. If you use this software or hardware in dangerous
applications, then you shall be responsible to take all appropriate fail-safe, backup, redundancy, and other
measures to ensure its safe use. Oracle Corporation and its affiliates disclaim any liability for any damages
caused by use of this software or hardware in dangerous applications.
Trademark Notice
Oracle, Java, MySQL, and NetSuite are registered trademarks of Oracle and/or its affiliates. Other names
may be trademarks of their respective owners.
v
Documentation Accessibility
Intel and Intel Inside are trademarks or registered trademarks of Intel Corporation. All SPARC trademarks
are used under license and are trademarks or registered trademarks of SPARC International, Inc. AMD,
Epyc, and the AMD logo are trademarks or registered trademarks of Advanced Micro Devices. UNIX is a
registered trademark of The Open Group.
Third-Party Content, Products, and Services Disclaimer
This software or hardware and documentation may provide access to or information about content,
products, and services from third parties. Oracle Corporation and its affiliates are not responsible for and
expressly disclaim all warranties of any kind with respect to third-party content, products, and services
unless otherwise set forth in an applicable agreement between you and Oracle. Oracle Corporation and its
affiliates will not be responsible for any loss, costs, or damages incurred due to your access to or use of
third-party content, products, or services, except as set forth in an applicable agreement between you and
Oracle.
Use of This Documentation
This documentation is NOT distributed under a GPL license. Use of this documentation is subject to the
following terms:
You may create a printed copy of this documentation solely for your own personal use. Conversion to other
formats is allowed as long as the actual content is not altered or edited in any way. You shall not publish
or distribute this documentation in any form or on any media, except if you distribute the documentation in
a manner similar to how Oracle disseminates it (that is, electronically for download on a Web site with the
software) or on a CD-ROM or similar medium, provided however that the documentation is disseminated
together with the software on the same medium. Any other use, such as any dissemination of printed
copies or use of this documentation, in whole or in part, in another publication, requires the prior written
consent from an authorized representative of Oracle. Oracle and/or its affiliates reserve any and all rights
to this documentation not expressly granted above.
Documentation Accessibility
For information about Oracle's commitment to accessibility, visit the Oracle Accessibility Program website
at
http://www.oracle.com/pls/topic/lookup?ctx=acc&id=docacc.
Access to Oracle Support for Accessibility
Oracle customers that have purchased support have access to electronic support through My Oracle
Support. For information, visit
http://www.oracle.com/pls/topic/lookup?ctx=acc&id=info or visit http://www.oracle.com/pls/topic/
lookup?ctx=acc&id=trs if you are hearing impaired.
vi
Chapter 1 MySQL Error Reference
This document provides a reference for the various types of error messages in MySQL:
• Error messages produced only by the server: Chapter 2, Server Error Message Reference
On the server side, error messages may occur during the startup and shutdown processes, as a result of
issues that occur during SQL statement execution, and so forth.
• The MySQL server writes some error messages to its error log. These indicate issues of interest to
database administrators or that require DBA action.
• The server sends other error messages to client programs. These indicate issues pertaining only to
a particular client. The MySQL client library takes errors received from the server and makes them
available to the host client program.
• Error messages that originate only from within the MySQL client library: Chapter 3, Client Error Message
Reference
Client-side error messages are generated from within the MySQL client library, usually involving
problems communicating with the server.
• Error messages that are shared between the server and the client library: Chapter 4, Global Error
Message Reference
Some “global” error messages are shared in the sense that they can be produced by the MySQL server
or by MySQL client programs.
For a description of the types of error information MySQL provides and how to obtain information about
them, see Error Messages and Common Problems, in MySQL 5.7 Reference Manual.
1
2
Chapter 2 Server Error Message Reference
The MySQL server writes some error messages to its error log, and sends others to client programs.
Example server-side error messages written to the error log:
2018-09-26T14:46:06.326016Z 0 [Note] Skipping generation of SSL
certificates as options related to SSL are specified.
2018-10-02T03:20:39.410387Z 0 [ERROR] Plugin 'InnoDB'
registration as a STORAGE ENGINE failed.
Example server-side error message sent to client programs, as displayed by the mysql client:
mysql> SELECT * FROM no_such_table;
ERROR 1146 (42S02): Table 'test.no_such_table' doesn't exist
Each server error message includes an error code, SQLSTATE value, and message string, as described
in Error Message Sources and Elements. These elements are available as described in Error Information
Interfaces.
In addition to the errors in the following list, the server can also produce error messages that have error
codes in the range from 1 to 999. See Chapter 4, Global Error Message Reference
• Error number: 1000; Symbol: ER_HASHCHK; SQLSTATE: HY000
Message: hashchk
Unused.
• Error number: 1001; Symbol: ER_NISAMCHK; SQLSTATE: HY000
Message: isamchk
Unused.
• Error number: 1002; Symbol: ER_NO; SQLSTATE: HY000
Message: NO
Used in the construction of other messages.
• Error number: 1003; Symbol: ER_YES; SQLSTATE: HY000
Message: YES
Used in the construction of other messages.
Extended EXPLAIN format generates Note messages. ER_YES is used in the Code column for these
messages in subsequent SHOW WARNINGS output.
• Error number: 1004; Symbol: ER_CANT_CREATE_FILE; SQLSTATE: HY000
Message: Can't create file '%s' (errno: %d - %s)
Occurs for failure to create or copy a file needed for some operation.
Possible causes: Permissions problem for source file; destination file already exists but is not writeable.
• Error number: 1005; Symbol: ER_CANT_CREATE_TABLE; SQLSTATE: HY000
3
Message: Can't create table '%s' (errno: %d)
InnoDB reports this error when a table cannot be created. If the error message refers to error 150, table
creation failed because a foreign key constraint was not correctly formed. If the error message refers
to error −1, table creation probably failed because the table includes a column name that matched the
name of an internal InnoDB table.
• Error number: 1006; Symbol: ER_CANT_CREATE_DB; SQLSTATE: HY000
Message: Can't create database '%s' (errno: %d)
• Error number: 1007; Symbol: ER_DB_CREATE_EXISTS; SQLSTATE: HY000
Message: Can't create database '%s'; database exists
An attempt to create a database failed because the database already exists.
Drop the database first if you really want to replace an existing database, or add an IF NOT EXISTS
clause to the CREATE DATABASE statement if to retain an existing database without having the
statement produce an error.
• Error number: 1008; Symbol: ER_DB_DROP_EXISTS; SQLSTATE: HY000
Message: Can't drop database '%s'; database doesn't exist
• Error number: 1009; Symbol: ER_DB_DROP_DELETE; SQLSTATE: HY000
Message: Error dropping database (can't delete '%s', errno: %d)
• Error number: 1010; Symbol: ER_DB_DROP_RMDIR; SQLSTATE: HY000
Message: Error dropping database (can't rmdir '%s', errno: %d)
• Error number: 1011; Symbol: ER_CANT_DELETE_FILE; SQLSTATE: HY000
Message: Error on delete of '%s' (errno: %d - %s)
• Error number: 1012; Symbol: ER_CANT_FIND_SYSTEM_REC; SQLSTATE: HY000
Message: Can't read record in system table
Returned by InnoDB for attempts to access InnoDB INFORMATION_SCHEMA tables when InnoDB is
unavailable.
• Error number: 1013; Symbol: ER_CANT_GET_STAT; SQLSTATE: HY000
Message: Can't get status of '%s' (errno: %d - %s)
• Error number: 1014; Symbol: ER_CANT_GET_WD; SQLSTATE: HY000
Message: Can't get working directory (errno: %d - %s)
• Error number: 1015; Symbol: ER_CANT_LOCK; SQLSTATE: HY000
Message: Can't lock file (errno: %d - %s)
• Error number: 1016; Symbol: ER_CANT_OPEN_FILE; SQLSTATE: HY000
Message: Can't open file: '%s' (errno: %d - %s)
4
InnoDB reports this error when the table from the InnoDB data files cannot be found, even though the
.frm file for the table exists. See Troubleshooting InnoDB Data Dictionary Operations.
• Error number: 1017; Symbol: ER_FILE_NOT_FOUND; SQLSTATE: HY000
Message: Can't find file: '%s' (errno: %d - %s)
• Error number: 1018; Symbol: ER_CANT_READ_DIR; SQLSTATE: HY000
Message: Can't read dir of '%s' (errno: %d - %s)
• Error number: 1019; Symbol: ER_CANT_SET_WD; SQLSTATE: HY000
Message: Can't change dir to '%s' (errno: %d - %s)
• Error number: 1020; Symbol: ER_CHECKREAD; SQLSTATE: HY000
Message: Record has changed since last read in table '%s'
• Error number: 1021; Symbol: ER_DISK_FULL; SQLSTATE: HY000
Message: Disk full (%s); waiting for someone to free some space... (errno: %d - %s)
• Error number: 1022; Symbol: ER_DUP_KEY; SQLSTATE: 23000
Message: Can't write; duplicate key in table '%s'
• Error number: 1023; Symbol: ER_ERROR_ON_CLOSE; SQLSTATE: HY000
Message: Error on close of '%s' (errno: %d - %s)
• Error number: 1024; Symbol: ER_ERROR_ON_READ; SQLSTATE: HY000
Message: Error reading file '%s' (errno: %d - %s)
• Error number: 1025; Symbol: ER_ERROR_ON_RENAME; SQLSTATE: HY000
Message: Error on rename of '%s' to '%s' (errno: %d - %s)
• Error number: 1026; Symbol: ER_ERROR_ON_WRITE; SQLSTATE: HY000
Message: Error writing file '%s' (errno: %d - %s)
• Error number: 1027; Symbol: ER_FILE_USED; SQLSTATE: HY000
Message: '%s' is locked against change
• Error number: 1028; Symbol: ER_FILSORT_ABORT; SQLSTATE: HY000
Message: Sort aborted
• Error number: 1029; Symbol: ER_FORM_NOT_FOUND; SQLSTATE: HY000
Message: View '%s' doesn't exist for '%s'
• Error number: 1030; Symbol: ER_GET_ERRNO; SQLSTATE: HY000
Message: Got error %d from storage engine
5
Check the %d value to see what the OS error means. For example, 28 indicates that you have run out of
disk space.
• Error number: 1031; Symbol: ER_ILLEGAL_HA; SQLSTATE: HY000
Message: Table storage engine for '%s' doesn't have this option
• Error number: 1032; Symbol: ER_KEY_NOT_FOUND; SQLSTATE: HY000
Message: Can't find record in '%s'
• Error number: 1033; Symbol: ER_NOT_FORM_FILE; SQLSTATE: HY000
Message: Incorrect information in file: '%s'
• Error number: 1034; Symbol: ER_NOT_KEYFILE; SQLSTATE: HY000
Message: Incorrect key file for table '%s'; try to repair it
• Error number: 1035; Symbol: ER_OLD_KEYFILE; SQLSTATE: HY000
Message: Old key file for table '%s'; repair it!
• Error number: 1036; Symbol: ER_OPEN_AS_READONLY; SQLSTATE: HY000
Message: Table '%s' is read only
• Error number: 1037; Symbol: ER_OUTOFMEMORY; SQLSTATE: HY001
Message: Out of memory; restart server and try again (needed %d bytes)
• Error number: 1038; Symbol: ER_OUT_OF_SORTMEMORY; SQLSTATE: HY001
Message: Out of sort memory, consider increasing server sort buffer size
• Error number: 1039; Symbol: ER_UNEXPECTED_EOF; SQLSTATE: HY000
Message: Unexpected EOF found when reading file '%s' (errno: %d - %s)
• Error number: 1040; Symbol: ER_CON_COUNT_ERROR; SQLSTATE: 08004
Message: Too many connections
• Error number: 1041; Symbol: ER_OUT_OF_RESOURCES; SQLSTATE: HY000
Message: Out of memory; check if mysqld or some other process uses all available memory; if not, you
may have to use 'ulimit' to allow mysqld to use more memory or you can add more swap space
• Error number: 1042; Symbol: ER_BAD_HOST_ERROR; SQLSTATE: 08S01
Message: Can't get hostname for your address
• Error number: 1043; Symbol: ER_HANDSHAKE_ERROR; SQLSTATE: 08S01
Message: Bad handshake
• Error number: 1044; Symbol: ER_DBACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: Access denied for user '%s'@'%s' to database '%s'
6
• Error number: 1045; Symbol: ER_ACCESS_DENIED_ERROR; SQLSTATE: 28000
Message: Access denied for user '%s'@'%s' (using password: %s)
• Error number: 1046; Symbol: ER_NO_DB_ERROR; SQLSTATE: 3D000
Message: No database selected
• Error number: 1047; Symbol: ER_UNKNOWN_COM_ERROR; SQLSTATE: 08S01
Message: Unknown command
• Error number: 1048; Symbol: ER_BAD_NULL_ERROR; SQLSTATE: 23000
Message: Column '%s' cannot be null
• Error number: 1049; Symbol: ER_BAD_DB_ERROR; SQLSTATE: 42000
Message: Unknown database '%s'
• Error number: 1050; Symbol: ER_TABLE_EXISTS_ERROR; SQLSTATE: 42S01
Message: Table '%s' already exists
• Error number: 1051; Symbol: ER_BAD_TABLE_ERROR; SQLSTATE: 42S02
Message: Unknown table '%s'
• Error number: 1052; Symbol: ER_NON_UNIQ_ERROR; SQLSTATE: 23000
Message: Column '%s' in %s is ambiguous
%s = column name
%s = location of column (for example, "field list")
Likely cause: A column appears in a query without appropriate qualification, such as in a select list or ON
clause.
Examples:
mysql> SELECT i FROM t INNER JOIN t AS t2;
ERROR 1052 (23000): Column 'i' in field list is ambiguous
mysql> SELECT * FROM t LEFT JOIN t AS t2 ON i = i;
ERROR 1052 (23000): Column 'i' in on clause is ambiguous
Resolution:
• Qualify the column with the appropriate table name:
mysql> SELECT t2.i FROM t INNER JOIN t AS t2;
• Modify the query to avoid the need for qualification:
mysql> SELECT * FROM t LEFT JOIN t AS t2 USING (i);
• Error number: 1053; Symbol: ER_SERVER_SHUTDOWN; SQLSTATE: 08S01
Message: Server shutdown in progress
• Error number: 1054; Symbol: ER_BAD_FIELD_ERROR; SQLSTATE: 42S22
7
Message: Unknown column '%s' in '%s'
• Error number: 1055; Symbol: ER_WRONG_FIELD_WITH_GROUP; SQLSTATE: 42000
Message: '%s' isn't in GROUP BY
• Error number: 1056; Symbol: ER_WRONG_GROUP_FIELD; SQLSTATE: 42000
Message: Can't group on '%s'
• Error number: 1057; Symbol: ER_WRONG_SUM_SELECT; SQLSTATE: 42000
Message: Statement has sum functions and columns in same statement
• Error number: 1058; Symbol: ER_WRONG_VALUE_COUNT; SQLSTATE: 21S01
Message: Column count doesn't match value count
• Error number: 1059; Symbol: ER_TOO_LONG_IDENT; SQLSTATE: 42000
Message: Identifier name '%s' is too long
• Error number: 1060; Symbol: ER_DUP_FIELDNAME; SQLSTATE: 42S21
Message: Duplicate column name '%s'
• Error number: 1061; Symbol: ER_DUP_KEYNAME; SQLSTATE: 42000
Message: Duplicate key name '%s'
• Error number: 1062; Symbol: ER_DUP_ENTRY; SQLSTATE: 23000
Message: Duplicate entry '%s' for key %d
The message returned with this error uses the format string for ER_DUP_ENTRY_WITH_KEY_NAME.
• Error number: 1063; Symbol: ER_WRONG_FIELD_SPEC; SQLSTATE: 42000
Message: Incorrect column specifier for column '%s'
• Error number: 1064; Symbol: ER_PARSE_ERROR; SQLSTATE: 42000
Message: %s near '%s' at line %d
• Error number: 1065; Symbol: ER_EMPTY_QUERY; SQLSTATE: 42000
Message: Query was empty
• Error number: 1066; Symbol: ER_NONUNIQ_TABLE; SQLSTATE: 42000
Message: Not unique table/alias: '%s'
• Error number: 1067; Symbol: ER_INVALID_DEFAULT; SQLSTATE: 42000
Message: Invalid default value for '%s'
• Error number: 1068; Symbol: ER_MULTIPLE_PRI_KEY; SQLSTATE: 42000
Message: Multiple primary key defined
8
• Error number: 1069; Symbol: ER_TOO_MANY_KEYS; SQLSTATE: 42000
Message: Too many keys specified; max %d keys allowed
• Error number: 1070; Symbol: ER_TOO_MANY_KEY_PARTS; SQLSTATE: 42000
Message: Too many key parts specified; max %d parts allowed
• Error number: 1071; Symbol: ER_TOO_LONG_KEY; SQLSTATE: 42000
Message: Specified key was too long; max key length is %d bytes
• Error number: 1072; Symbol: ER_KEY_COLUMN_DOES_NOT_EXITS; SQLSTATE: 42000
Message: Key column '%s' doesn't exist in table
• Error number: 1073; Symbol: ER_BLOB_USED_AS_KEY; SQLSTATE: 42000
Message: BLOB column '%s' can't be used in key specification with the used table type
• Error number: 1074; Symbol: ER_TOO_BIG_FIELDLENGTH; SQLSTATE: 42000
Message: Column length too big for column '%s' (max = %lu); use BLOB or TEXT instead
• Error number: 1075; Symbol: ER_WRONG_AUTO_KEY; SQLSTATE: 42000
Message: Incorrect table definition; there can be only one auto column and it must be defined as a key
• Error number: 1076; Symbol: ER_READY; SQLSTATE: HY000
Message: %s: ready for connections. Version: '%s' socket: '%s' port: %d
• Error number: 1077; Symbol: ER_NORMAL_SHUTDOWN; SQLSTATE: HY000
Message: %s: Normal shutdown
• Error number: 1078; Symbol: ER_GOT_SIGNAL; SQLSTATE: HY000
Message: %s: Got signal %d. Aborting!
• Error number: 1079; Symbol: ER_SHUTDOWN_COMPLETE; SQLSTATE: HY000
Message: %s: Shutdown complete
• Error number: 1080; Symbol: ER_FORCING_CLOSE; SQLSTATE: 08S01
Message: %s: Forcing close of thread %ld user: '%s'
• Error number: 1081; Symbol: ER_IPSOCK_ERROR; SQLSTATE: 08S01
Message: Can't create IP socket
• Error number: 1082; Symbol: ER_NO_SUCH_INDEX; SQLSTATE: 42S12
Message: Table '%s' has no index like the one used in CREATE INDEX; recreate the table
• Error number: 1083; Symbol: ER_WRONG_FIELD_TERMINATORS; SQLSTATE: 42000
Message: Field separator argument is not what is expected; check the manual
9
• Error number: 1084; Symbol: ER_BLOBS_AND_NO_TERMINATED; SQLSTATE: 42000
Message: You can't use fixed rowlength with BLOBs; please use 'fields terminated by'
• Error number: 1085; Symbol: ER_TEXTFILE_NOT_READABLE; SQLSTATE: HY000
Message: The file '%s' must be in the database directory or be readable by all
• Error number: 1086; Symbol: ER_FILE_EXISTS_ERROR; SQLSTATE: HY000
Message: File '%s' already exists
• Error number: 1087; Symbol: ER_LOAD_INFO; SQLSTATE: HY000
Message: Records: %ld Deleted: %ld Skipped: %ld Warnings: %ld
• Error number: 1088; Symbol: ER_ALTER_INFO; SQLSTATE: HY000
Message: Records: %ld Duplicates: %ld
• Error number: 1089; Symbol: ER_WRONG_SUB_KEY; SQLSTATE: HY000
Message: Incorrect prefix key; the used key part isn't a string, the used length is longer than the key part,
or the storage engine doesn't support unique prefix keys
• Error number: 1090; Symbol: ER_CANT_REMOVE_ALL_FIELDS; SQLSTATE: 42000
Message: You can't delete all columns with ALTER TABLE; use DROP TABLE instead
• Error number: 1091; Symbol: ER_CANT_DROP_FIELD_OR_KEY; SQLSTATE: 42000
Message: Can't DROP '%s'; check that column/key exists
• Error number: 1092; Symbol: ER_INSERT_INFO; SQLSTATE: HY000
Message: Records: %ld Duplicates: %ld Warnings: %ld
• Error number: 1093; Symbol: ER_UPDATE_TABLE_USED; SQLSTATE: HY000
Message: You can't specify target table '%s' for update in FROM clause
This error occurs for attempts to select from and modify the same table within a single statement. If the
select attempt occurs within a derived table, you can avoid this error by setting the derived_merge flag
of the optimizer_switch system variable to force the subquery to be materialized into a temporary
table, which effectively causes it to be a different table from the one modified. See Optimizing Derived
Tables and View References with Merging or Materialization.
• Error number: 1094; Symbol: ER_NO_SUCH_THREAD; SQLSTATE: HY000
Message: Unknown thread id: %lu
• Error number: 1095; Symbol: ER_KILL_DENIED_ERROR; SQLSTATE: HY000
Message: You are not owner of thread %lu
• Error number: 1096; Symbol: ER_NO_TABLES_USED; SQLSTATE: HY000
Message: No tables used
• Error number: 1097; Symbol: ER_TOO_BIG_SET; SQLSTATE: HY000
10
Message: Too many strings for column %s and SET
• Error number: 1098; Symbol: ER_NO_UNIQUE_LOGFILE; SQLSTATE: HY000
Message: Can't generate a unique log-filename %s.(1-999)
• Error number: 1099; Symbol: ER_TABLE_NOT_LOCKED_FOR_WRITE; SQLSTATE: HY000
Message: Table '%s' was locked with a READ lock and can't be updated
• Error number: 1100; Symbol: ER_TABLE_NOT_LOCKED; SQLSTATE: HY000
Message: Table '%s' was not locked with LOCK TABLES
• Error number: 1101; Symbol: ER_BLOB_CANT_HAVE_DEFAULT; SQLSTATE: 42000
Message: BLOB, TEXT, GEOMETRY or JSON column '%s' can't have a default value
• Error number: 1102; Symbol: ER_WRONG_DB_NAME; SQLSTATE: 42000
Message: Incorrect database name '%s'
• Error number: 1103; Symbol: ER_WRONG_TABLE_NAME; SQLSTATE: 42000
Message: Incorrect table name '%s'
• Error number: 1104; Symbol: ER_TOO_BIG_SELECT; SQLSTATE: 42000
Message: The SELECT would examine more than MAX_JOIN_SIZE rows; check your WHERE and use
SET SQL_BIG_SELECTS=1 or SET MAX_JOIN_SIZE=# if the SELECT is okay
• Error number: 1105; Symbol: ER_UNKNOWN_ERROR; SQLSTATE: HY000
Message: Unknown error
• Error number: 1106; Symbol: ER_UNKNOWN_PROCEDURE; SQLSTATE: 42000
Message: Unknown procedure '%s'
• Error number: 1107; Symbol: ER_WRONG_PARAMCOUNT_TO_PROCEDURE; SQLSTATE: 42000
Message: Incorrect parameter count to procedure '%s'
• Error number: 1108; Symbol: ER_WRONG_PARAMETERS_TO_PROCEDURE; SQLSTATE: HY000
Message: Incorrect parameters to procedure '%s'
• Error number: 1109; Symbol: ER_UNKNOWN_TABLE; SQLSTATE: 42S02
Message: Unknown table '%s' in %s
• Error number: 1110; Symbol: ER_FIELD_SPECIFIED_TWICE; SQLSTATE: 42000
Message: Column '%s' specified twice
• Error number: 1111; Symbol: ER_INVALID_GROUP_FUNC_USE; SQLSTATE: HY000
Message: Invalid use of group function
11
• Error number: 1112; Symbol: ER_UNSUPPORTED_EXTENSION; SQLSTATE: 42000
Message: Table '%s' uses an extension that doesn't exist in this MySQL version
• Error number: 1113; Symbol: ER_TABLE_MUST_HAVE_COLUMNS; SQLSTATE: 42000
Message: A table must have at least 1 column
• Error number: 1114; Symbol: ER_RECORD_FILE_FULL; SQLSTATE: HY000
Message: The table '%s' is full
InnoDB reports this error when the system tablespace runs out of free space. Reconfigure the system
tablespace to add a new data file.
• Error number: 1115; Symbol: ER_UNKNOWN_CHARACTER_SET; SQLSTATE: 42000
Message: Unknown character set: '%s'
• Error number: 1116; Symbol: ER_TOO_MANY_TABLES; SQLSTATE: HY000
Message: Too many tables; MySQL can only use %d tables in a join
• Error number: 1117; Symbol: ER_TOO_MANY_FIELDS; SQLSTATE: HY000
Message: Too many columns
• Error number: 1118; Symbol: ER_TOO_BIG_ROWSIZE; SQLSTATE: 42000
Message: Row size too large. The maximum row size for the used table type, not counting BLOBs, is
%ld. This includes storage overhead, check the manual. You have to change some columns to TEXT or
BLOBs
• Error number: 1119; Symbol: ER_STACK_OVERRUN; SQLSTATE: HY000
Message: Thread stack overrun: Used: %ld of a %ld stack. Use 'mysqld --thread_stack=#' to specify a
bigger stack if needed
• Error number: 1120; Symbol: ER_WRONG_OUTER_JOIN; SQLSTATE: 42000
Message: Cross dependency found in OUTER JOIN; examine your ON conditions
• Error number: 1121; Symbol: ER_NULL_COLUMN_IN_INDEX; SQLSTATE: 42000
Message: Table handler doesn't support NULL in given index. Please change column '%s' to be NOT
NULL or use another handler
• Error number: 1122; Symbol: ER_CANT_FIND_UDF; SQLSTATE: HY000
Message: Can't load function '%s'
• Error number: 1123; Symbol: ER_CANT_INITIALIZE_UDF; SQLSTATE: HY000
Message: Can't initialize function '%s'; %s
• Error number: 1124; Symbol: ER_UDF_NO_PATHS; SQLSTATE: HY000
Message: No paths allowed for shared library
• Error number: 1125; Symbol: ER_UDF_EXISTS; SQLSTATE: HY000
12
Message: Function '%s' already exists
• Error number: 1126; Symbol: ER_CANT_OPEN_LIBRARY; SQLSTATE: HY000
Message: Can't open shared library '%s' (errno: %d %s)
• Error number: 1127; Symbol: ER_CANT_FIND_DL_ENTRY; SQLSTATE: HY000
Message: Can't find symbol '%s' in library
• Error number: 1128; Symbol: ER_FUNCTION_NOT_DEFINED; SQLSTATE: HY000
Message: Function '%s' is not defined
• Error number: 1129; Symbol: ER_HOST_IS_BLOCKED; SQLSTATE: HY000
Message: Host '%s' is blocked because of many connection errors; unblock with 'mysqladmin flushhosts'
• Error number: 1130; Symbol: ER_HOST_NOT_PRIVILEGED; SQLSTATE: HY000
Message: Host '%s' is not allowed to connect to this MySQL server
• Error number: 1131; Symbol: ER_PASSWORD_ANONYMOUS_USER; SQLSTATE: 42000
Message: You are using MySQL as an anonymous user and anonymous users are not allowed to
change passwords
• Error number: 1132; Symbol: ER_PASSWORD_NOT_ALLOWED; SQLSTATE: 42000
Message: You must have privileges to update tables in the mysql database to be able to change
passwords for others
• Error number: 1133; Symbol: ER_PASSWORD_NO_MATCH; SQLSTATE: 42000
Message: Can't find any matching row in the user table
• Error number: 1134; Symbol: ER_UPDATE_INFO; SQLSTATE: HY000
Message: Rows matched: %ld Changed: %ld Warnings: %ld
• Error number: 1135; Symbol: ER_CANT_CREATE_THREAD; SQLSTATE: HY000
Message: Can't create a new thread (errno %d); if you are not out of available memory, you can consult
the manual for a possible OS-dependent bug
• Error number: 1136; Symbol: ER_WRONG_VALUE_COUNT_ON_ROW; SQLSTATE: 21S01
Message: Column count doesn't match value count at row %ld
• Error number: 1137; Symbol: ER_CANT_REOPEN_TABLE; SQLSTATE: HY000
Message: Can't reopen table: '%s'
• Error number: 1138; Symbol: ER_INVALID_USE_OF_NULL; SQLSTATE: 22004
Message: Invalid use of NULL value
• Error number: 1139; Symbol: ER_REGEXP_ERROR; SQLSTATE: 42000
13
Message: Got error '%s' from regexp
• Error number: 1140; Symbol: ER_MIX_OF_GROUP_FUNC_AND_FIELDS; SQLSTATE: 42000
Message: Mixing of GROUP columns (MIN(),MAX(),COUNT(),...) with no GROUP columns is illegal if
there is no GROUP BY clause
• Error number: 1141; Symbol: ER_NONEXISTING_GRANT; SQLSTATE: 42000
Message: There is no such grant defined for user '%s' on host '%s'
• Error number: 1142; Symbol: ER_TABLEACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: %s command denied to user '%s'@'%s' for table '%s'
• Error number: 1143; Symbol: ER_COLUMNACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: %s command denied to user '%s'@'%s' for column '%s' in table '%s'
• Error number: 1144; Symbol: ER_ILLEGAL_GRANT_FOR_TABLE; SQLSTATE: 42000
Message: Illegal GRANT/REVOKE command; please consult the manual to see which privileges can be
used
• Error number: 1145; Symbol: ER_GRANT_WRONG_HOST_OR_USER; SQLSTATE: 42000
Message: The host or user argument to GRANT is too long
• Error number: 1146; Symbol: ER_NO_SUCH_TABLE; SQLSTATE: 42S02
Message: Table '%s.%s' doesn't exist
• Error number: 1147; Symbol: ER_NONEXISTING_TABLE_GRANT; SQLSTATE: 42000
Message: There is no such grant defined for user '%s' on host '%s' on table '%s'
• Error number: 1148; Symbol: ER_NOT_ALLOWED_COMMAND; SQLSTATE: 42000
Message: The used command is not allowed with this MySQL version
• Error number: 1149; Symbol: ER_SYNTAX_ERROR; SQLSTATE: 42000
Message: You have an error in your SQL syntax; check the manual that corresponds to your MySQL
server version for the right syntax to use
• Error number: 1150; Symbol: ER_UNUSED1; SQLSTATE: HY000
Message: Delayed insert thread couldn't get requested lock for table %s
• Error number: 1151; Symbol: ER_UNUSED2; SQLSTATE: HY000
Message: Too many delayed threads in use
• Error number: 1152; Symbol: ER_ABORTING_CONNECTION; SQLSTATE: 08S01
Message: Aborted connection %ld to db: '%s' user: '%s' (%s)
• Error number: 1153; Symbol: ER_NET_PACKET_TOO_LARGE; SQLSTATE: 08S01
Message: Got a packet bigger than 'max_allowed_packet' bytes
14
• Error number: 1154; Symbol: ER_NET_READ_ERROR_FROM_PIPE; SQLSTATE: 08S01
Message: Got a read error from the connection pipe
• Error number: 1155; Symbol: ER_NET_FCNTL_ERROR; SQLSTATE: 08S01
Message: Got an error from fcntl()
• Error number: 1156; Symbol: ER_NET_PACKETS_OUT_OF_ORDER; SQLSTATE: 08S01
Message: Got packets out of order
• Error number: 1157; Symbol: ER_NET_UNCOMPRESS_ERROR; SQLSTATE: 08S01
Message: Couldn't uncompress communication packet
• Error number: 1158; Symbol: ER_NET_READ_ERROR; SQLSTATE: 08S01
Message: Got an error reading communication packets
• Error number: 1159; Symbol: ER_NET_READ_INTERRUPTED; SQLSTATE: 08S01
Message: Got timeout reading communication packets
• Error number: 1160; Symbol: ER_NET_ERROR_ON_WRITE; SQLSTATE: 08S01
Message: Got an error writing communication packets
• Error number: 1161; Symbol: ER_NET_WRITE_INTERRUPTED; SQLSTATE: 08S01
Message: Got timeout writing communication packets
• Error number: 1162; Symbol: ER_TOO_LONG_STRING; SQLSTATE: 42000
Message: Result string is longer than 'max_allowed_packet' bytes
• Error number: 1163; Symbol: ER_TABLE_CANT_HANDLE_BLOB; SQLSTATE: 42000
Message: The used table type doesn't support BLOB/TEXT columns
• Error number: 1164; Symbol: ER_TABLE_CANT_HANDLE_AUTO_INCREMENT; SQLSTATE: 42000
Message: The used table type doesn't support AUTO_INCREMENT columns
• Error number: 1165; Symbol: ER_UNUSED3; SQLSTATE: HY000
Message: INSERT DELAYED can't be used with table '%s' because it is locked with LOCK TABLES
• Error number: 1166; Symbol: ER_WRONG_COLUMN_NAME; SQLSTATE: 42000
Message: Incorrect column name '%s'
• Error number: 1167; Symbol: ER_WRONG_KEY_COLUMN; SQLSTATE: 42000
Message: The used storage engine can't index column '%s'
• Error number: 1168; Symbol: ER_WRONG_MRG_TABLE; SQLSTATE: HY000
Message: Unable to open underlying table which is differently defined or of non-MyISAM type or doesn't
exist
15
• Error number: 1169; Symbol: ER_DUP_UNIQUE; SQLSTATE: 23000
Message: Can't write, because of unique constraint, to table '%s'
• Error number: 1170; Symbol: ER_BLOB_KEY_WITHOUT_LENGTH; SQLSTATE: 42000
Message: BLOB/TEXT column '%s' used in key specification without a key length
• Error number: 1171; Symbol: ER_PRIMARY_CANT_HAVE_NULL; SQLSTATE: 42000
Message: All parts of a PRIMARY KEY must be NOT NULL; if you need NULL in a key, use UNIQUE
instead
• Error number: 1172; Symbol: ER_TOO_MANY_ROWS; SQLSTATE: 42000
Message: Result consisted of more than one row
• Error number: 1173; Symbol: ER_REQUIRES_PRIMARY_KEY; SQLSTATE: 42000
Message: This table type requires a primary key
• Error number: 1174; Symbol: ER_NO_RAID_COMPILED; SQLSTATE: HY000
Message: This version of MySQL is not compiled with RAID support
• Error number: 1175; Symbol: ER_UPDATE_WITHOUT_KEY_IN_SAFE_MODE; SQLSTATE: HY000
Message: You are using safe update mode and you tried to update a table without a WHERE that uses a
KEY column. %s
• Error number: 1176; Symbol: ER_KEY_DOES_NOT_EXITS; SQLSTATE: 42000
Message: Key '%s' doesn't exist in table '%s'
• Error number: 1177; Symbol: ER_CHECK_NO_SUCH_TABLE; SQLSTATE: 42000
Message: Can't open table
• Error number: 1178; Symbol: ER_CHECK_NOT_IMPLEMENTED; SQLSTATE: 42000
Message: The storage engine for the table doesn't support %s
• Error number: 1179; Symbol: ER_CANT_DO_THIS_DURING_AN_TRANSACTION; SQLSTATE: 25000
Message: You are not allowed to execute this command in a transaction
• Error number: 1180; Symbol: ER_ERROR_DURING_COMMIT; SQLSTATE: HY000
Message: Got error %d during COMMIT
• Error number: 1181; Symbol: ER_ERROR_DURING_ROLLBACK; SQLSTATE: HY000
Message: Got error %d during ROLLBACK
• Error number: 1182; Symbol: ER_ERROR_DURING_FLUSH_LOGS; SQLSTATE: HY000
Message: Got error %d during FLUSH_LOGS
• Error number: 1183; Symbol: ER_ERROR_DURING_CHECKPOINT; SQLSTATE: HY000
Message: Got error %d during CHECKPOINT
16
• Error number: 1184; Symbol: ER_NEW_ABORTING_CONNECTION; SQLSTATE: 08S01
Message: Aborted connection %u to db: '%s' user: '%s' host: '%s' (%s)
• Error number: 1185; Symbol: ER_DUMP_NOT_IMPLEMENTED; SQLSTATE: HY000
Message: The storage engine for the table does not support binary table dump
• Error number: 1186; Symbol: ER_FLUSH_MASTER_BINLOG_CLOSED; SQLSTATE: HY000
Message: Binlog closed, cannot RESET MASTER
• Error number: 1187; Symbol: ER_INDEX_REBUILD; SQLSTATE: HY000
Message: Failed rebuilding the index of dumped table '%s'
• Error number: 1188; Symbol: ER_MASTER; SQLSTATE: HY000
Message: Error from master: '%s'
• Error number: 1189; Symbol: ER_MASTER_NET_READ; SQLSTATE: 08S01
Message: Net error reading from master
• Error number: 1190; Symbol: ER_MASTER_NET_WRITE; SQLSTATE: 08S01
Message: Net error writing to master
• Error number: 1191; Symbol: ER_FT_MATCHING_KEY_NOT_FOUND; SQLSTATE: HY000
Message: Can't find FULLTEXT index matching the column list
• Error number: 1192; Symbol: ER_LOCK_OR_ACTIVE_TRANSACTION; SQLSTATE: HY000
Message: Can't execute the given command because you have active locked tables or an active
transaction
• Error number: 1193; Symbol: ER_UNKNOWN_SYSTEM_VARIABLE; SQLSTATE: HY000
Message: Unknown system variable '%s'
• Error number: 1194; Symbol: ER_CRASHED_ON_USAGE; SQLSTATE: HY000
Message: Table '%s' is marked as crashed and should be repaired
• Error number: 1195; Symbol: ER_CRASHED_ON_REPAIR; SQLSTATE: HY000
Message: Table '%s' is marked as crashed and last (automatic?) repair failed
• Error number: 1196; Symbol: ER_WARNING_NOT_COMPLETE_ROLLBACK; SQLSTATE: HY000
Message: Some non-transactional changed tables couldn't be rolled back
• Error number: 1197; Symbol: ER_TRANS_CACHE_FULL; SQLSTATE: HY000
Message: Multi-statement transaction required more than 'max_binlog_cache_size' bytes of storage;
increase this mysqld variable and try again
• Error number: 1198; Symbol: ER_SLAVE_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with a running slave; run STOP SLAVE first
17
• Error number: 1199; Symbol: ER_SLAVE_NOT_RUNNING; SQLSTATE: HY000
Message: This operation requires a running slave; configure slave and do START SLAVE
• Error number: 1200; Symbol: ER_BAD_SLAVE; SQLSTATE: HY000
Message: The server is not configured as slave; fix in config file or with CHANGE MASTER TO
• Error number: 1201; Symbol: ER_MASTER_INFO; SQLSTATE: HY000
Message: Could not initialize master info structure; more error messages can be found in the MySQL
error log
• Error number: 1202; Symbol: ER_SLAVE_THREAD; SQLSTATE: HY000
Message: Could not create slave thread; check system resources
• Error number: 1203; Symbol: ER_TOO_MANY_USER_CONNECTIONS; SQLSTATE: 42000
Message: User %s already has more than 'max_user_connections' active connections
• Error number: 1204; Symbol: ER_SET_CONSTANTS_ONLY; SQLSTATE: HY000
Message: You may only use constant expressions with SET
• Error number: 1205; Symbol: ER_LOCK_WAIT_TIMEOUT; SQLSTATE: HY000
Message: Lock wait timeout exceeded; try restarting transaction
InnoDB reports this error when lock wait timeout expires. The statement that waited too long was rolled
back (not the entire transaction). You can increase the value of the innodb_lock_wait_timeout
configuration option if SQL statements should wait longer for other transactions to complete, or decrease
it if too many long-running transactions are causing locking problems and reducing concurrency on a
busy system.
• Error number: 1206; Symbol: ER_LOCK_TABLE_FULL; SQLSTATE: HY000
Message: The total number of locks exceeds the lock table size
InnoDB reports this error when the total number of locks exceeds the amount of memory devoted to
managing locks. To avoid this error, increase the value of innodb_buffer_pool_size. Within an
individual application, a workaround may be to break a large operation into smaller pieces. For example,
if the error occurs for a large INSERT, perform several smaller INSERT operations.
• Error number: 1207; Symbol: ER_READ_ONLY_TRANSACTION; SQLSTATE: 25000
Message: Update locks cannot be acquired during a READ UNCOMMITTED transaction
• Error number: 1208; Symbol: ER_DROP_DB_WITH_READ_LOCK; SQLSTATE: HY000
Message: DROP DATABASE not allowed while thread is holding global read lock
• Error number: 1209; Symbol: ER_CREATE_DB_WITH_READ_LOCK; SQLSTATE: HY000
Message: CREATE DATABASE not allowed while thread is holding global read lock
• Error number: 1210; Symbol: ER_WRONG_ARGUMENTS; SQLSTATE: HY000
Message: Incorrect arguments to %s
18
• Error number: 1211; Symbol: ER_NO_PERMISSION_TO_CREATE_USER; SQLSTATE: 42000
Message: '%s'@'%s' is not allowed to create new users
• Error number: 1212; Symbol: ER_UNION_TABLES_IN_DIFFERENT_DIR; SQLSTATE: HY000
Message: Incorrect table definition; all MERGE tables must be in the same database
• Error number: 1213; Symbol: ER_LOCK_DEADLOCK; SQLSTATE: 40001
Message: Deadlock found when trying to get lock; try restarting transaction
InnoDB reports this error when a transaction encounters a deadlock and is automatically rolled back
so that your application can take corrective action. To recover from this error, run all the operations in
this transaction again. A deadlock occurs when requests for locks arrive in inconsistent order between
transactions. The transaction that was rolled back released all its locks, and the other transaction
can now get all the locks it requested. Thus, when you re-run the transaction that was rolled back, it
might have to wait for other transactions to complete, but typically the deadlock does not recur. If you
encounter frequent deadlocks, make the sequence of locking operations (LOCK TABLES, SELECT ...
FOR UPDATE, and so on) consistent between the different transactions or applications that experience
the issue. See Deadlocks in InnoDB for details.
• Error number: 1214; Symbol: ER_TABLE_CANT_HANDLE_FT; SQLSTATE: HY000
Message: The used table type doesn't support FULLTEXT indexes
• Error number: 1215; Symbol: ER_CANNOT_ADD_FOREIGN; SQLSTATE: HY000
Message: Cannot add foreign key constraint
• Error number: 1216; Symbol: ER_NO_REFERENCED_ROW; SQLSTATE: 23000
Message: Cannot add or update a child row: a foreign key constraint fails
InnoDB reports this error when you try to add a row but there is no parent row, and a foreign key
constraint fails. Add the parent row first.
• Error number: 1217; Symbol: ER_ROW_IS_REFERENCED; SQLSTATE: 23000
Message: Cannot delete or update a parent row: a foreign key constraint fails
InnoDB reports this error when you try to delete a parent row that has children, and a foreign key
constraint fails. Delete the children first.
• Error number: 1218; Symbol: ER_CONNECT_TO_MASTER; SQLSTATE: 08S01
Message: Error connecting to master: %s
• Error number: 1219; Symbol: ER_QUERY_ON_MASTER; SQLSTATE: HY000
Message: Error running query on master: %s
• Error number: 1220; Symbol: ER_ERROR_WHEN_EXECUTING_COMMAND; SQLSTATE: HY000
Message: Error when executing command %s: %s
• Error number: 1221; Symbol: ER_WRONG_USAGE; SQLSTATE: HY000
Message: Incorrect usage of %s and %s
19
• Error number: 1222; Symbol: ER_WRONG_NUMBER_OF_COLUMNS_IN_SELECT; SQLSTATE: 21000
Message: The used SELECT statements have a different number of columns
• Error number: 1223; Symbol: ER_CANT_UPDATE_WITH_READLOCK; SQLSTATE: HY000
Message: Can't execute the query because you have a conflicting read lock
• Error number: 1224; Symbol: ER_MIXING_NOT_ALLOWED; SQLSTATE: HY000
Message: Mixing of transactional and non-transactional tables is disabled
• Error number: 1225; Symbol: ER_DUP_ARGUMENT; SQLSTATE: HY000
Message: Option '%s' used twice in statement
• Error number: 1226; Symbol: ER_USER_LIMIT_REACHED; SQLSTATE: 42000
Message: User '%s' has exceeded the '%s' resource (current value: %ld)
• Error number: 1227; Symbol: ER_SPECIFIC_ACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: Access denied; you need (at least one of) the %s privilege(s) for this operation
• Error number: 1228; Symbol: ER_LOCAL_VARIABLE; SQLSTATE: HY000
Message: Variable '%s' is a SESSION variable and can't be used with SET GLOBAL
• Error number: 1229; Symbol: ER_GLOBAL_VARIABLE; SQLSTATE: HY000
Message: Variable '%s' is a GLOBAL variable and should be set with SET GLOBAL
• Error number: 1230; Symbol: ER_NO_DEFAULT; SQLSTATE: 42000
Message: Variable '%s' doesn't have a default value
• Error number: 1231; Symbol: ER_WRONG_VALUE_FOR_VAR; SQLSTATE: 42000
Message: Variable '%s' can't be set to the value of '%s'
• Error number: 1232; Symbol: ER_WRONG_TYPE_FOR_VAR; SQLSTATE: 42000
Message: Incorrect argument type to variable '%s'
• Error number: 1233; Symbol: ER_VAR_CANT_BE_READ; SQLSTATE: HY000
Message: Variable '%s' can only be set, not read
• Error number: 1234; Symbol: ER_CANT_USE_OPTION_HERE; SQLSTATE: 42000
Message: Incorrect usage/placement of '%s'
• Error number: 1235; Symbol: ER_NOT_SUPPORTED_YET; SQLSTATE: 42000
Message: This version of MySQL doesn't yet support '%s'
• Error number: 1236; Symbol: ER_MASTER_FATAL_ERROR_READING_BINLOG; SQLSTATE: HY000
Message: Got fatal error %d from master when reading data from binary log: '%s'
• Error number: 1237; Symbol: ER_SLAVE_IGNORED_TABLE; SQLSTATE: HY000
20
Message: Slave SQL thread ignored the query because of replicate-*-table rules
• Error number: 1238; Symbol: ER_INCORRECT_GLOBAL_LOCAL_VAR; SQLSTATE: HY000
Message: Variable '%s' is a %s variable
• Error number: 1239; Symbol: ER_WRONG_FK_DEF; SQLSTATE: 42000
Message: Incorrect foreign key definition for '%s': %s
• Error number: 1240; Symbol: ER_KEY_REF_DO_NOT_MATCH_TABLE_REF; SQLSTATE: HY000
Message: Key reference and table reference don't match
• Error number: 1241; Symbol: ER_OPERAND_COLUMNS; SQLSTATE: 21000
Message: Operand should contain %d column(s)
• Error number: 1242; Symbol: ER_SUBQUERY_NO_1_ROW; SQLSTATE: 21000
Message: Subquery returns more than 1 row
• Error number: 1243; Symbol: ER_UNKNOWN_STMT_HANDLER; SQLSTATE: HY000
Message: Unknown prepared statement handler (%.*s) given to %s
• Error number: 1244; Symbol: ER_CORRUPT_HELP_DB; SQLSTATE: HY000
Message: Help database is corrupt or does not exist
• Error number: 1245; Symbol: ER_CYCLIC_REFERENCE; SQLSTATE: HY000
Message: Cyclic reference on subqueries
• Error number: 1246; Symbol: ER_AUTO_CONVERT; SQLSTATE: HY000
Message: Converting column '%s' from %s to %s
• Error number: 1247; Symbol: ER_ILLEGAL_REFERENCE; SQLSTATE: 42S22
Message: Reference '%s' not supported (%s)
• Error number: 1248; Symbol: ER_DERIVED_MUST_HAVE_ALIAS; SQLSTATE: 42000
Message: Every derived table must have its own alias
• Error number: 1249; Symbol: ER_SELECT_REDUCED; SQLSTATE: 01000
Message: Select %u was reduced during optimization
• Error number: 1250; Symbol: ER_TABLENAME_NOT_ALLOWED_HERE; SQLSTATE: 42000
Message: Table '%s' from one of the SELECTs cannot be used in %s
• Error number: 1251; Symbol: ER_NOT_SUPPORTED_AUTH_MODE; SQLSTATE: 08004
Message: Client does not support authentication protocol requested by server; consider upgrading
MySQL client
• Error number: 1252; Symbol: ER_SPATIAL_CANT_HAVE_NULL; SQLSTATE: 42000
21
Message: All parts of a SPATIAL index must be NOT NULL
• Error number: 1253; Symbol: ER_COLLATION_CHARSET_MISMATCH; SQLSTATE: 42000
Message: COLLATION '%s' is not valid for CHARACTER SET '%s'
• Error number: 1254; Symbol: ER_SLAVE_WAS_RUNNING; SQLSTATE: HY000
Message: Slave is already running
• Error number: 1255; Symbol: ER_SLAVE_WAS_NOT_RUNNING; SQLSTATE: HY000
Message: Slave already has been stopped
• Error number: 1256; Symbol: ER_TOO_BIG_FOR_UNCOMPRESS; SQLSTATE: HY000
Message: Uncompressed data size too large; the maximum size is %d (probably, length of
uncompressed data was corrupted)
• Error number: 1257; Symbol: ER_ZLIB_Z_MEM_ERROR; SQLSTATE: HY000
Message: ZLIB: Not enough memory
• Error number: 1258; Symbol: ER_ZLIB_Z_BUF_ERROR; SQLSTATE: HY000
Message: ZLIB: Not enough room in the output buffer (probably, length of uncompressed data was
corrupted)
• Error number: 1259; Symbol: ER_ZLIB_Z_DATA_ERROR; SQLSTATE: HY000
Message: ZLIB: Input data corrupted
• Error number: 1260; Symbol: ER_CUT_VALUE_GROUP_CONCAT; SQLSTATE: HY000
Message: Row %u was cut by GROUP_CONCAT()
• Error number: 1261; Symbol: ER_WARN_TOO_FEW_RECORDS; SQLSTATE: 01000
Message: Row %ld doesn't contain data for all columns
• Error number: 1262; Symbol: ER_WARN_TOO_MANY_RECORDS; SQLSTATE: 01000
Message: Row %ld was truncated; it contained more data than there were input columns
• Error number: 1263; Symbol: ER_WARN_NULL_TO_NOTNULL; SQLSTATE: 22004
Message: Column set to default value; NULL supplied to NOT NULL column '%s' at row %ld
• Error number: 1264; Symbol: ER_WARN_DATA_OUT_OF_RANGE; SQLSTATE: 22003
Message: Out of range value for column '%s' at row %ld
• Error number: 1265; Symbol: WARN_DATA_TRUNCATED; SQLSTATE: 01000
Message: Data truncated for column '%s' at row %ld
• Error number: 1266; Symbol: ER_WARN_USING_OTHER_HANDLER; SQLSTATE: HY000
Message: Using storage engine %s for table '%s'
22
• Error number: 1267; Symbol: ER_CANT_AGGREGATE_2COLLATIONS; SQLSTATE: HY000
Message: Illegal mix of collations (%s,%s) and (%s,%s) for operation '%s'
• Error number: 1268; Symbol: ER_DROP_USER; SQLSTATE: HY000
Message: Cannot drop one or more of the requested users
• Error number: 1269; Symbol: ER_REVOKE_GRANTS; SQLSTATE: HY000
Message: Can't revoke all privileges for one or more of the requested users
• Error number: 1270; Symbol: ER_CANT_AGGREGATE_3COLLATIONS; SQLSTATE: HY000
Message: Illegal mix of collations (%s,%s), (%s,%s), (%s,%s) for operation '%s'
• Error number: 1271; Symbol: ER_CANT_AGGREGATE_NCOLLATIONS; SQLSTATE: HY000
Message: Illegal mix of collations for operation '%s'
• Error number: 1272; Symbol: ER_VARIABLE_IS_NOT_STRUCT; SQLSTATE: HY000
Message: Variable '%s' is not a variable component (can't be used as XXXX.variable_name)
• Error number: 1273; Symbol: ER_UNKNOWN_COLLATION; SQLSTATE: HY000
Message: Unknown collation: '%s'
• Error number: 1274; Symbol: ER_SLAVE_IGNORED_SSL_PARAMS; SQLSTATE: HY000
Message: SSL parameters in CHANGE MASTER are ignored because this MySQL slave was compiled
without SSL support; they can be used later if MySQL slave with SSL is started
• Error number: 1275; Symbol: ER_SERVER_IS_IN_SECURE_AUTH_MODE; SQLSTATE: HY000
Message: Server is running in --secure-auth mode, but '%s'@'%s' has a password in the old format;
please change the password to the new format
• Error number: 1276; Symbol: ER_WARN_FIELD_RESOLVED; SQLSTATE: HY000
Message: Field or reference '%s%s%s%s%s' of SELECT #%d was resolved in SELECT #%d
• Error number: 1277; Symbol: ER_BAD_SLAVE_UNTIL_COND; SQLSTATE: HY000
Message: Incorrect parameter or combination of parameters for START SLAVE UNTIL
• Error number: 1278; Symbol: ER_MISSING_SKIP_SLAVE; SQLSTATE: HY000
Message: It is recommended to use --skip-slave-start when doing step-by-step replication with START
SLAVE UNTIL; otherwise, you will get problems if you get an unexpected slave's mysqld restart
• Error number: 1279; Symbol: ER_UNTIL_COND_IGNORED; SQLSTATE: HY000
Message: SQL thread is not to be started so UNTIL options are ignored
• Error number: 1280; Symbol: ER_WRONG_NAME_FOR_INDEX; SQLSTATE: 42000
Message: Incorrect index name '%s'
• Error number: 1281; Symbol: ER_WRONG_NAME_FOR_CATALOG; SQLSTATE: 42000
23
Message: Incorrect catalog name '%s'
• Error number: 1282; Symbol: ER_WARN_QC_RESIZE; SQLSTATE: HY000
Message: Query cache failed to set size %lu; new query cache size is %lu
• Error number: 1283; Symbol: ER_BAD_FT_COLUMN; SQLSTATE: HY000
Message: Column '%s' cannot be part of FULLTEXT index
• Error number: 1284; Symbol: ER_UNKNOWN_KEY_CACHE; SQLSTATE: HY000
Message: Unknown key cache '%s'
• Error number: 1285; Symbol: ER_WARN_HOSTNAME_WONT_WORK; SQLSTATE: HY000
Message: MySQL is started in --skip-name-resolve mode; you must restart it without this switch for this
grant to work
• Error number: 1286; Symbol: ER_UNKNOWN_STORAGE_ENGINE; SQLSTATE: 42000
Message: Unknown storage engine '%s'
• Error number: 1287; Symbol: ER_WARN_DEPRECATED_SYNTAX; SQLSTATE: HY000
Message: '%s' is deprecated and will be removed in a future release. Please use %s instead
• Error number: 1288; Symbol: ER_NON_UPDATABLE_TABLE; SQLSTATE: HY000
Message: The target table %s of the %s is not updatable
• Error number: 1289; Symbol: ER_FEATURE_DISABLED; SQLSTATE: HY000
Message: The '%s' feature is disabled; you need MySQL built with '%s' to have it working
• Error number: 1290; Symbol: ER_OPTION_PREVENTS_STATEMENT; SQLSTATE: HY000
Message: The MySQL server is running with the %s option so it cannot execute this statement
• Error number: 1291; Symbol: ER_DUPLICATED_VALUE_IN_TYPE; SQLSTATE: HY000
Message: Column '%s' has duplicated value '%s' in %s
• Error number: 1292; Symbol: ER_TRUNCATED_WRONG_VALUE; SQLSTATE: 22007
Message: Truncated incorrect %s value: '%s'
• Error number: 1293; Symbol: ER_TOO_MUCH_AUTO_TIMESTAMP_COLS; SQLSTATE: HY000
Message: Incorrect table definition; there can be only one TIMESTAMP column with
CURRENT_TIMESTAMP in DEFAULT or ON UPDATE clause
• Error number: 1294; Symbol: ER_INVALID_ON_UPDATE; SQLSTATE: HY000
Message: Invalid ON UPDATE clause for '%s' column
• Error number: 1295; Symbol: ER_UNSUPPORTED_PS; SQLSTATE: HY000
Message: This command is not supported in the prepared statement protocol yet
24
• Error number: 1296; Symbol: ER_GET_ERRMSG; SQLSTATE: HY000
Message: Got error %d '%s' from %s
• Error number: 1297; Symbol: ER_GET_TEMPORARY_ERRMSG; SQLSTATE: HY000
Message: Got temporary error %d '%s' from %s
• Error number: 1298; Symbol: ER_UNKNOWN_TIME_ZONE; SQLSTATE: HY000
Message: Unknown or incorrect time zone: '%s'
• Error number: 1299; Symbol: ER_WARN_INVALID_TIMESTAMP; SQLSTATE: HY000
Message: Invalid TIMESTAMP value in column '%s' at row %ld
• Error number: 1300; Symbol: ER_INVALID_CHARACTER_STRING; SQLSTATE: HY000
Message: Invalid %s character string: '%s'
• Error number: 1301; Symbol: ER_WARN_ALLOWED_PACKET_OVERFLOWED; SQLSTATE: HY000
Message: Result of %s() was larger than max_allowed_packet (%ld) - truncated
• Error number: 1302; Symbol: ER_CONFLICTING_DECLARATIONS; SQLSTATE: HY000
Message: Conflicting declarations: '%s%s' and '%s%s'
• Error number: 1303; Symbol: ER_SP_NO_RECURSIVE_CREATE; SQLSTATE: 2F003
Message: Can't create a %s from within another stored routine
• Error number: 1304; Symbol: ER_SP_ALREADY_EXISTS; SQLSTATE: 42000
Message: %s %s already exists
• Error number: 1305; Symbol: ER_SP_DOES_NOT_EXIST; SQLSTATE: 42000
Message: %s %s does not exist
• Error number: 1306; Symbol: ER_SP_DROP_FAILED; SQLSTATE: HY000
Message: Failed to DROP %s %s
• Error number: 1307; Symbol: ER_SP_STORE_FAILED; SQLSTATE: HY000
Message: Failed to CREATE %s %s
• Error number: 1308; Symbol: ER_SP_LILABEL_MISMATCH; SQLSTATE: 42000
Message: %s with no matching label: %s
• Error number: 1309; Symbol: ER_SP_LABEL_REDEFINE; SQLSTATE: 42000
Message: Redefining label %s
• Error number: 1310; Symbol: ER_SP_LABEL_MISMATCH; SQLSTATE: 42000
Message: End-label %s without match
• Error number: 1311; Symbol: ER_SP_UNINIT_VAR; SQLSTATE: 01000
25
Message: Referring to uninitialized variable %s
• Error number: 1312; Symbol: ER_SP_BADSELECT; SQLSTATE: 0A000
Message: PROCEDURE %s can't return a result set in the given context
• Error number: 1313; Symbol: ER_SP_BADRETURN; SQLSTATE: 42000
Message: RETURN is only allowed in a FUNCTION
• Error number: 1314; Symbol: ER_SP_BADSTATEMENT; SQLSTATE: 0A000
Message: %s is not allowed in stored procedures
• Error number: 1315; Symbol: ER_UPDATE_LOG_DEPRECATED_IGNORED; SQLSTATE: 42000
Message: The update log is deprecated and replaced by the binary log; SET SQL_LOG_UPDATE has
been ignored.
• Error number: 1316; Symbol: ER_UPDATE_LOG_DEPRECATED_TRANSLATED; SQLSTATE: 42000
Message: The update log is deprecated and replaced by the binary log; SET SQL_LOG_UPDATE has
been translated to SET SQL_LOG_BIN.
• Error number: 1317; Symbol: ER_QUERY_INTERRUPTED; SQLSTATE: 70100
Message: Query execution was interrupted
• Error number: 1318; Symbol: ER_SP_WRONG_NO_OF_ARGS; SQLSTATE: 42000
Message: Incorrect number of arguments for %s %s; expected %u, got %u
• Error number: 1319; Symbol: ER_SP_COND_MISMATCH; SQLSTATE: 42000
Message: Undefined CONDITION: %s
• Error number: 1320; Symbol: ER_SP_NORETURN; SQLSTATE: 42000
Message: No RETURN found in FUNCTION %s
• Error number: 1321; Symbol: ER_SP_NORETURNEND; SQLSTATE: 2F005
Message: FUNCTION %s ended without RETURN
• Error number: 1322; Symbol: ER_SP_BAD_CURSOR_QUERY; SQLSTATE: 42000
Message: Cursor statement must be a SELECT
• Error number: 1323; Symbol: ER_SP_BAD_CURSOR_SELECT; SQLSTATE: 42000
Message: Cursor SELECT must not have INTO
• Error number: 1324; Symbol: ER_SP_CURSOR_MISMATCH; SQLSTATE: 42000
Message: Undefined CURSOR: %s
• Error number: 1325; Symbol: ER_SP_CURSOR_ALREADY_OPEN; SQLSTATE: 24000
Message: Cursor is already open
26
• Error number: 1326; Symbol: ER_SP_CURSOR_NOT_OPEN; SQLSTATE: 24000
Message: Cursor is not open
• Error number: 1327; Symbol: ER_SP_UNDECLARED_VAR; SQLSTATE: 42000
Message: Undeclared variable: %s
• Error number: 1328; Symbol: ER_SP_WRONG_NO_OF_FETCH_ARGS; SQLSTATE: HY000
Message: Incorrect number of FETCH variables
• Error number: 1329; Symbol: ER_SP_FETCH_NO_DATA; SQLSTATE: 02000
Message: No data - zero rows fetched, selected, or processed
• Error number: 1330; Symbol: ER_SP_DUP_PARAM; SQLSTATE: 42000
Message: Duplicate parameter: %s
• Error number: 1331; Symbol: ER_SP_DUP_VAR; SQLSTATE: 42000
Message: Duplicate variable: %s
• Error number: 1332; Symbol: ER_SP_DUP_COND; SQLSTATE: 42000
Message: Duplicate condition: %s
• Error number: 1333; Symbol: ER_SP_DUP_CURS; SQLSTATE: 42000
Message: Duplicate cursor: %s
• Error number: 1334; Symbol: ER_SP_CANT_ALTER; SQLSTATE: HY000
Message: Failed to ALTER %s %s
• Error number: 1335; Symbol: ER_SP_SUBSELECT_NYI; SQLSTATE: 0A000
Message: Subquery value not supported
• Error number: 1336; Symbol: ER_STMT_NOT_ALLOWED_IN_SF_OR_TRG; SQLSTATE: 0A000
Message: %s is not allowed in stored function or trigger
• Error number: 1337; Symbol: ER_SP_VARCOND_AFTER_CURSHNDLR; SQLSTATE: 42000
Message: Variable or condition declaration after cursor or handler declaration
• Error number: 1338; Symbol: ER_SP_CURSOR_AFTER_HANDLER; SQLSTATE: 42000
Message: Cursor declaration after handler declaration
• Error number: 1339; Symbol: ER_SP_CASE_NOT_FOUND; SQLSTATE: 20000
Message: Case not found for CASE statement
• Error number: 1340; Symbol: ER_FPARSER_TOO_BIG_FILE; SQLSTATE: HY000
Message: Configuration file '%s' is too big
• Error number: 1341; Symbol: ER_FPARSER_BAD_HEADER; SQLSTATE: HY000
27
Message: Malformed file type header in file '%s'
• Error number: 1342; Symbol: ER_FPARSER_EOF_IN_COMMENT; SQLSTATE: HY000
Message: Unexpected end of file while parsing comment '%s'
• Error number: 1343; Symbol: ER_FPARSER_ERROR_IN_PARAMETER; SQLSTATE: HY000
Message: Error while parsing parameter '%s' (line: '%s')
• Error number: 1344; Symbol: ER_FPARSER_EOF_IN_UNKNOWN_PARAMETER; SQLSTATE: HY000
Message: Unexpected end of file while skipping unknown parameter '%s'
• Error number: 1345; Symbol: ER_VIEW_NO_EXPLAIN; SQLSTATE: HY000
Message: EXPLAIN/SHOW can not be issued; lacking privileges for underlying table
• Error number: 1346; Symbol: ER_FRM_UNKNOWN_TYPE; SQLSTATE: HY000
Message: File '%s' has unknown type '%s' in its header
• Error number: 1347; Symbol: ER_WRONG_OBJECT; SQLSTATE: HY000
Message: '%s.%s' is not %s
The named object is incorrect for the type of operation attempted on it. It must be an object of the named
type.
• Error number: 1348; Symbol: ER_NONUPDATEABLE_COLUMN; SQLSTATE: HY000
Message: Column '%s' is not updatable
• Error number: 1349; Symbol: ER_VIEW_SELECT_DERIVED; SQLSTATE: HY000
Message: View's SELECT contains a subquery in the FROM clause
ER_VIEW_SELECT_DERIVED was removed after 5.7.6.
• Error number: 1349; Symbol: ER_VIEW_SELECT_DERIVED_UNUSED; SQLSTATE: HY000
Message: View's SELECT contains a subquery in the FROM clause
ER_VIEW_SELECT_DERIVED_UNUSED was added in 5.7.7.
• Error number: 1350; Symbol: ER_VIEW_SELECT_CLAUSE; SQLSTATE: HY000
Message: View's SELECT contains a '%s' clause
• Error number: 1351; Symbol: ER_VIEW_SELECT_VARIABLE; SQLSTATE: HY000
Message: View's SELECT contains a variable or parameter
• Error number: 1352; Symbol: ER_VIEW_SELECT_TMPTABLE; SQLSTATE: HY000
Message: View's SELECT refers to a temporary table '%s'
• Error number: 1353; Symbol: ER_VIEW_WRONG_LIST; SQLSTATE: HY000
Message: View's SELECT and view's field list have different column counts
28
• Error number: 1354; Symbol: ER_WARN_VIEW_MERGE; SQLSTATE: HY000
Message: View merge algorithm can't be used here for now (assumed undefined algorithm)
• Error number: 1355; Symbol: ER_WARN_VIEW_WITHOUT_KEY; SQLSTATE: HY000
Message: View being updated does not have complete key of underlying table in it
• Error number: 1356; Symbol: ER_VIEW_INVALID; SQLSTATE: HY000
Message: View '%s.%s' references invalid table(s) or column(s) or function(s) or definer/invoker of view
lack rights to use them
• Error number: 1357; Symbol: ER_SP_NO_DROP_SP; SQLSTATE: HY000
Message: Can't drop or alter a %s from within another stored routine
• Error number: 1358; Symbol: ER_SP_GOTO_IN_HNDLR; SQLSTATE: HY000
Message: GOTO is not allowed in a stored procedure handler
• Error number: 1359; Symbol: ER_TRG_ALREADY_EXISTS; SQLSTATE: HY000
Message: Trigger already exists
• Error number: 1360; Symbol: ER_TRG_DOES_NOT_EXIST; SQLSTATE: HY000
Message: Trigger does not exist
• Error number: 1361; Symbol: ER_TRG_ON_VIEW_OR_TEMP_TABLE; SQLSTATE: HY000
Message: Trigger's '%s' is view or temporary table
• Error number: 1362; Symbol: ER_TRG_CANT_CHANGE_ROW; SQLSTATE: HY000
Message: Updating of %s row is not allowed in %strigger
• Error number: 1363; Symbol: ER_TRG_NO_SUCH_ROW_IN_TRG; SQLSTATE: HY000
Message: There is no %s row in %s trigger
• Error number: 1364; Symbol: ER_NO_DEFAULT_FOR_FIELD; SQLSTATE: HY000
Message: Field '%s' doesn't have a default value
• Error number: 1365; Symbol: ER_DIVISION_BY_ZERO; SQLSTATE: 22012
Message: Division by 0
• Error number: 1366; Symbol: ER_TRUNCATED_WRONG_VALUE_FOR_FIELD; SQLSTATE: HY000
Message: Incorrect %s value: '%s' for column '%s' at row %ld
• Error number: 1367; Symbol: ER_ILLEGAL_VALUE_FOR_TYPE; SQLSTATE: 22007
Message: Illegal %s '%s' value found during parsing
• Error number: 1368; Symbol: ER_VIEW_NONUPD_CHECK; SQLSTATE: HY000
Message: CHECK OPTION on non-updatable view '%s.%s'
29
• Error number: 1369; Symbol: ER_VIEW_CHECK_FAILED; SQLSTATE: HY000
Message: CHECK OPTION failed '%s.%s'
• Error number: 1370; Symbol: ER_PROCACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: %s command denied to user '%s'@'%s' for routine '%s'
• Error number: 1371; Symbol: ER_RELAY_LOG_FAIL; SQLSTATE: HY000
Message: Failed purging old relay logs: %s
• Error number: 1372; Symbol: ER_PASSWD_LENGTH; SQLSTATE: HY000
Message: Password hash should be a %d-digit hexadecimal number
• Error number: 1373; Symbol: ER_UNKNOWN_TARGET_BINLOG; SQLSTATE: HY000
Message: Target log not found in binlog index
• Error number: 1374; Symbol: ER_IO_ERR_LOG_INDEX_READ; SQLSTATE: HY000
Message: I/O error reading log index file
• Error number: 1375; Symbol: ER_BINLOG_PURGE_PROHIBITED; SQLSTATE: HY000
Message: Server configuration does not permit binlog purge
• Error number: 1376; Symbol: ER_FSEEK_FAIL; SQLSTATE: HY000
Message: Failed on fseek()
• Error number: 1377; Symbol: ER_BINLOG_PURGE_FATAL_ERR; SQLSTATE: HY000
Message: Fatal error during log purge
• Error number: 1378; Symbol: ER_LOG_IN_USE; SQLSTATE: HY000
Message: A purgeable log is in use, will not purge
• Error number: 1379; Symbol: ER_LOG_PURGE_UNKNOWN_ERR; SQLSTATE: HY000
Message: Unknown error during log purge
• Error number: 1380; Symbol: ER_RELAY_LOG_INIT; SQLSTATE: HY000
Message: Failed initializing relay log position: %s
• Error number: 1381; Symbol: ER_NO_BINARY_LOGGING; SQLSTATE: HY000
Message: You are not using binary logging
• Error number: 1382; Symbol: ER_RESERVED_SYNTAX; SQLSTATE: HY000
Message: The '%s' syntax is reserved for purposes internal to the MySQL server
• Error number: 1383; Symbol: ER_WSAS_FAILED; SQLSTATE: HY000
Message: WSAStartup Failed
30
• Error number: 1384; Symbol: ER_DIFF_GROUPS_PROC; SQLSTATE: HY000
Message: Can't handle procedures with different groups yet
• Error number: 1385; Symbol: ER_NO_GROUP_FOR_PROC; SQLSTATE: HY000
Message: Select must have a group with this procedure
• Error number: 1386; Symbol: ER_ORDER_WITH_PROC; SQLSTATE: HY000
Message: Can't use ORDER clause with this procedure
• Error number: 1387; Symbol: ER_LOGGING_PROHIBIT_CHANGING_OF; SQLSTATE: HY000
Message: Binary logging and replication forbid changing the global server %s
• Error number: 1388; Symbol: ER_NO_FILE_MAPPING; SQLSTATE: HY000
Message: Can't map file: %s, errno: %d
• Error number: 1389; Symbol: ER_WRONG_MAGIC; SQLSTATE: HY000
Message: Wrong magic in %s
• Error number: 1390; Symbol: ER_PS_MANY_PARAM; SQLSTATE: HY000
Message: Prepared statement contains too many placeholders
• Error number: 1391; Symbol: ER_KEY_PART_0; SQLSTATE: HY000
Message: Key part '%s' length cannot be 0
• Error number: 1392; Symbol: ER_VIEW_CHECKSUM; SQLSTATE: HY000
Message: View text checksum failed
• Error number: 1393; Symbol: ER_VIEW_MULTIUPDATE; SQLSTATE: HY000
Message: Can not modify more than one base table through a join view '%s.%s'
• Error number: 1394; Symbol: ER_VIEW_NO_INSERT_FIELD_LIST; SQLSTATE: HY000
Message: Can not insert into join view '%s.%s' without fields list
• Error number: 1395; Symbol: ER_VIEW_DELETE_MERGE_VIEW; SQLSTATE: HY000
Message: Can not delete from join view '%s.%s'
• Error number: 1396; Symbol: ER_CANNOT_USER; SQLSTATE: HY000
Message: Operation %s failed for %s
• Error number: 1397; Symbol: ER_XAER_NOTA; SQLSTATE: XAE04
Message: XAER_NOTA: Unknown XID
• Error number: 1398; Symbol: ER_XAER_INVAL; SQLSTATE: XAE05
Message: XAER_INVAL: Invalid arguments (or unsupported command)
• Error number: 1399; Symbol: ER_XAER_RMFAIL; SQLSTATE: XAE07
31
Message: XAER_RMFAIL: The command cannot be executed when global transaction is in the %s state
• Error number: 1400; Symbol: ER_XAER_OUTSIDE; SQLSTATE: XAE09
Message: XAER_OUTSIDE: Some work is done outside global transaction
• Error number: 1401; Symbol: ER_XAER_RMERR; SQLSTATE: XAE03
Message: XAER_RMERR: Fatal error occurred in the transaction branch - check your data for
consistency
• Error number: 1402; Symbol: ER_XA_RBROLLBACK; SQLSTATE: XA100
Message: XA_RBROLLBACK: Transaction branch was rolled back
• Error number: 1403; Symbol: ER_NONEXISTING_PROC_GRANT; SQLSTATE: 42000
Message: There is no such grant defined for user '%s' on host '%s' on routine '%s'
• Error number: 1404; Symbol: ER_PROC_AUTO_GRANT_FAIL; SQLSTATE: HY000
Message: Failed to grant EXECUTE and ALTER ROUTINE privileges
• Error number: 1405; Symbol: ER_PROC_AUTO_REVOKE_FAIL; SQLSTATE: HY000
Message: Failed to revoke all privileges to dropped routine
• Error number: 1406; Symbol: ER_DATA_TOO_LONG; SQLSTATE: 22001
Message: Data too long for column '%s' at row %ld
• Error number: 1407; Symbol: ER_SP_BAD_SQLSTATE; SQLSTATE: 42000
Message: Bad SQLSTATE: '%s'
• Error number: 1408; Symbol: ER_STARTUP; SQLSTATE: HY000
Message: %s: ready for connections. Version: '%s' socket: '%s' port: %d %s
• Error number: 1409; Symbol: ER_LOAD_FROM_FIXED_SIZE_ROWS_TO_VAR; SQLSTATE: HY000
Message: Can't load value from file with fixed size rows to variable
• Error number: 1410; Symbol: ER_CANT_CREATE_USER_WITH_GRANT; SQLSTATE: 42000
Message: You are not allowed to create a user with GRANT
• Error number: 1411; Symbol: ER_WRONG_VALUE_FOR_TYPE; SQLSTATE: HY000
Message: Incorrect %s value: '%s' for function %s
• Error number: 1412; Symbol: ER_TABLE_DEF_CHANGED; SQLSTATE: HY000
Message: Table definition has changed, please retry transaction
• Error number: 1413; Symbol: ER_SP_DUP_HANDLER; SQLSTATE: 42000
Message: Duplicate handler declared in the same block
• Error number: 1414; Symbol: ER_SP_NOT_VAR_ARG; SQLSTATE: 42000
32
Message: OUT or INOUT argument %d for routine %s is not a variable or NEW pseudo-variable in
BEFORE trigger
• Error number: 1415; Symbol: ER_SP_NO_RETSET; SQLSTATE: 0A000
Message: Not allowed to return a result set from a %s
• Error number: 1416; Symbol: ER_CANT_CREATE_GEOMETRY_OBJECT; SQLSTATE: 22003
Message: Cannot get geometry object from data you send to the GEOMETRY field
• Error number: 1417; Symbol: ER_FAILED_ROUTINE_BREAK_BINLOG; SQLSTATE: HY000
Message: A routine failed and has neither NO SQL nor READS SQL DATA in its declaration and binary
logging is enabled; if non-transactional tables were updated, the binary log will miss their changes
• Error number: 1418; Symbol: ER_BINLOG_UNSAFE_ROUTINE; SQLSTATE: HY000
Message: This function has none of DETERMINISTIC, NO SQL, or READS SQL DATA in its declaration
and binary logging is enabled (you *might* want to use the less safe log_bin_trust_function_creators
variable)
• Error number: 1419; Symbol: ER_BINLOG_CREATE_ROUTINE_NEED_SUPER; SQLSTATE: HY000
Message: You do not have the SUPER privilege and binary logging is enabled (you *might* want to use
the less safe log_bin_trust_function_creators variable)
• Error number: 1420; Symbol: ER_EXEC_STMT_WITH_OPEN_CURSOR; SQLSTATE: HY000
Message: You can't execute a prepared statement which has an open cursor associated with it. Reset
the statement to re-execute it.
• Error number: 1421; Symbol: ER_STMT_HAS_NO_OPEN_CURSOR; SQLSTATE: HY000
Message: The statement (%lu) has no open cursor.
• Error number: 1422; Symbol: ER_COMMIT_NOT_ALLOWED_IN_SF_OR_TRG; SQLSTATE: HY000
Message: Explicit or implicit commit is not allowed in stored function or trigger.
• Error number: 1423; Symbol: ER_NO_DEFAULT_FOR_VIEW_FIELD; SQLSTATE: HY000
Message: Field of view '%s.%s' underlying table doesn't have a default value
• Error number: 1424; Symbol: ER_SP_NO_RECURSION; SQLSTATE: HY000
Message: Recursive stored functions and triggers are not allowed.
• Error number: 1425; Symbol: ER_TOO_BIG_SCALE; SQLSTATE: 42000
Message: Too big scale %d specified for column '%s'. Maximum is %lu.
• Error number: 1426; Symbol: ER_TOO_BIG_PRECISION; SQLSTATE: 42000
Message: Too-big precision %d specified for '%s'. Maximum is %lu.
• Error number: 1427; Symbol: ER_M_BIGGER_THAN_D; SQLSTATE: 42000
Message: For float(M,D), double(M,D) or decimal(M,D), M must be >= D (column '%s').
33
• Error number: 1428; Symbol: ER_WRONG_LOCK_OF_SYSTEM_TABLE; SQLSTATE: HY000
Message: You can't combine write-locking of system tables with other tables or lock types
• Error number: 1429; Symbol: ER_CONNECT_TO_FOREIGN_DATA_SOURCE; SQLSTATE: HY000
Message: Unable to connect to foreign data source: %s
• Error number: 1430; Symbol: ER_QUERY_ON_FOREIGN_DATA_SOURCE; SQLSTATE: HY000
Message: There was a problem processing the query on the foreign data source. Data source error: %s
• Error number: 1431; Symbol: ER_FOREIGN_DATA_SOURCE_DOESNT_EXIST; SQLSTATE: HY000
Message: The foreign data source you are trying to reference does not exist. Data source error: %s
• Error number: 1432; Symbol: ER_FOREIGN_DATA_STRING_INVALID_CANT_CREATE; SQLSTATE:
HY000
Message: Can't create federated table. The data source connection string '%s' is not in the correct
format
• Error number: 1433; Symbol: ER_FOREIGN_DATA_STRING_INVALID; SQLSTATE: HY000
Message: The data source connection string '%s' is not in the correct format
• Error number: 1434; Symbol: ER_CANT_CREATE_FEDERATED_TABLE; SQLSTATE: HY000
Message: Can't create federated table. Foreign data src error: %s
• Error number: 1435; Symbol: ER_TRG_IN_WRONG_SCHEMA; SQLSTATE: HY000
Message: Trigger in wrong schema
• Error number: 1436; Symbol: ER_STACK_OVERRUN_NEED_MORE; SQLSTATE: HY000
Message: Thread stack overrun: %ld bytes used of a %ld byte stack, and %ld bytes needed. Use
'mysqld --thread_stack=#' to specify a bigger stack.
• Error number: 1437; Symbol: ER_TOO_LONG_BODY; SQLSTATE: 42000
Message: Routine body for '%s' is too long
• Error number: 1438; Symbol: ER_WARN_CANT_DROP_DEFAULT_KEYCACHE; SQLSTATE: HY000
Message: Cannot drop default keycache
• Error number: 1439; Symbol: ER_TOO_BIG_DISPLAYWIDTH; SQLSTATE: 42000
Message: Display width out of range for column '%s' (max = %lu)
• Error number: 1440; Symbol: ER_XAER_DUPID; SQLSTATE: XAE08
Message: XAER_DUPID: The XID already exists
• Error number: 1441; Symbol: ER_DATETIME_FUNCTION_OVERFLOW; SQLSTATE: 22008
Message: Datetime function: %s field overflow
• Error number: 1442; Symbol: ER_CANT_UPDATE_USED_TABLE_IN_SF_OR_TRG; SQLSTATE: HY000
34
Message: Can't update table '%s' in stored function/trigger because it is already used by statement
which invoked this stored function/trigger.
• Error number: 1443; Symbol: ER_VIEW_PREVENT_UPDATE; SQLSTATE: HY000
Message: The definition of table '%s' prevents operation %s on table '%s'.
• Error number: 1444; Symbol: ER_PS_NO_RECURSION; SQLSTATE: HY000
Message: The prepared statement contains a stored routine call that refers to that same statement. It's
not allowed to execute a prepared statement in such a recursive manner
• Error number: 1445; Symbol: ER_SP_CANT_SET_AUTOCOMMIT; SQLSTATE: HY000
Message: Not allowed to set autocommit from a stored function or trigger
• Error number: 1446; Symbol: ER_MALFORMED_DEFINER; SQLSTATE: HY000
Message: Definer is not fully qualified
• Error number: 1447; Symbol: ER_VIEW_FRM_NO_USER; SQLSTATE: HY000
Message: View '%s'.'%s' has no definer information (old table format). Current user is used as definer.
Please recreate the view!
• Error number: 1448; Symbol: ER_VIEW_OTHER_USER; SQLSTATE: HY000
Message: You need the SUPER privilege for creation view with '%s'@'%s' definer
• Error number: 1449; Symbol: ER_NO_SUCH_USER; SQLSTATE: HY000
Message: The user specified as a definer ('%s'@'%s') does not exist
• Error number: 1450; Symbol: ER_FORBID_SCHEMA_CHANGE; SQLSTATE: HY000
Message: Changing schema from '%s' to '%s' is not allowed.
• Error number: 1451; Symbol: ER_ROW_IS_REFERENCED_2; SQLSTATE: 23000
Message: Cannot delete or update a parent row: a foreign key constraint fails (%s)
InnoDB reports this error when you try to delete a parent row that has children, and a foreign key
constraint fails. Delete the children first.
• Error number: 1452; Symbol: ER_NO_REFERENCED_ROW_2; SQLSTATE: 23000
Message: Cannot add or update a child row: a foreign key constraint fails (%s)
InnoDB reports this error when you try to add a row but there is no parent row, and a foreign key
constraint fails. Add the parent row first.
• Error number: 1453; Symbol: ER_SP_BAD_VAR_SHADOW; SQLSTATE: 42000
Message: Variable '%s' must be quoted with `...`, or renamed
• Error number: 1454; Symbol: ER_TRG_NO_DEFINER; SQLSTATE: HY000
Message: No definer attribute for trigger '%s'.'%s'. The trigger will be activated under the authorization of
the invoker, which may have insufficient privileges. Please recreate the trigger.
35
• Error number: 1455; Symbol: ER_OLD_FILE_FORMAT; SQLSTATE: HY000
Message: '%s' has an old format, you should re-create the '%s' object(s)
• Error number: 1456; Symbol: ER_SP_RECURSION_LIMIT; SQLSTATE: HY000
Message: Recursive limit %d (as set by the max_sp_recursion_depth variable) was exceeded for routine
%s
• Error number: 1457; Symbol: ER_SP_PROC_TABLE_CORRUPT; SQLSTATE: HY000
Message: Failed to load routine %s. The table mysql.proc is missing, corrupt, or contains bad data
(internal code %d)
• Error number: 1458; Symbol: ER_SP_WRONG_NAME; SQLSTATE: 42000
Message: Incorrect routine name '%s'
• Error number: 1459; Symbol: ER_TABLE_NEEDS_UPGRADE; SQLSTATE: HY000
Message: Table upgrade required. Please do "REPAIR TABLE `%s`" or dump/reload to fix it!
• Error number: 1460; Symbol: ER_SP_NO_AGGREGATE; SQLSTATE: 42000
Message: AGGREGATE is not supported for stored functions
• Error number: 1461; Symbol: ER_MAX_PREPARED_STMT_COUNT_REACHED; SQLSTATE: 42000
Message: Can't create more than max_prepared_stmt_count statements (current value: %lu)
• Error number: 1462; Symbol: ER_VIEW_RECURSIVE; SQLSTATE: HY000
Message: `%s`.`%s` contains view recursion
• Error number: 1463; Symbol: ER_NON_GROUPING_FIELD_USED; SQLSTATE: 42000
Message: Non-grouping field '%s' is used in %s clause
• Error number: 1464; Symbol: ER_TABLE_CANT_HANDLE_SPKEYS; SQLSTATE: HY000
Message: The used table type doesn't support SPATIAL indexes
• Error number: 1465; Symbol: ER_NO_TRIGGERS_ON_SYSTEM_SCHEMA; SQLSTATE: HY000
Message: Triggers can not be created on system tables
• Error number: 1466; Symbol: ER_REMOVED_SPACES; SQLSTATE: HY000
Message: Leading spaces are removed from name '%s'
• Error number: 1467; Symbol: ER_AUTOINC_READ_FAILED; SQLSTATE: HY000
Message: Failed to read auto-increment value from storage engine
• Error number: 1468; Symbol: ER_USERNAME; SQLSTATE: HY000
Message: user name
• Error number: 1469; Symbol: ER_HOSTNAME; SQLSTATE: HY000
36
Message: host name
• Error number: 1470; Symbol: ER_WRONG_STRING_LENGTH; SQLSTATE: HY000
Message: String '%s' is too long for %s (should be no longer than %d)
• Error number: 1471; Symbol: ER_NON_INSERTABLE_TABLE; SQLSTATE: HY000
Message: The target table %s of the %s is not insertable-into
• Error number: 1472; Symbol: ER_ADMIN_WRONG_MRG_TABLE; SQLSTATE: HY000
Message: Table '%s' is differently defined or of non-MyISAM type or doesn't exist
• Error number: 1473; Symbol: ER_TOO_HIGH_LEVEL_OF_NESTING_FOR_SELECT; SQLSTATE: HY000
Message: Too high level of nesting for select
• Error number: 1474; Symbol: ER_NAME_BECOMES_EMPTY; SQLSTATE: HY000
Message: Name '%s' has become ''
• Error number: 1475; Symbol: ER_AMBIGUOUS_FIELD_TERM; SQLSTATE: HY000
Message: First character of the FIELDS TERMINATED string is ambiguous; please use non-optional and
non-empty FIELDS ENCLOSED BY
• Error number: 1476; Symbol: ER_FOREIGN_SERVER_EXISTS; SQLSTATE: HY000
Message: The foreign server, %s, you are trying to create already exists.
• Error number: 1477; Symbol: ER_FOREIGN_SERVER_DOESNT_EXIST; SQLSTATE: HY000
Message: The foreign server name you are trying to reference does not exist. Data source error: %s
• Error number: 1478; Symbol: ER_ILLEGAL_HA_CREATE_OPTION; SQLSTATE: HY000
Message: Table storage engine '%s' does not support the create option '%s'
• Error number: 1479; Symbol: ER_PARTITION_REQUIRES_VALUES_ERROR; SQLSTATE: HY000
Message: Syntax error: %s PARTITIONING requires definition of VALUES %s for each partition
• Error number: 1480; Symbol: ER_PARTITION_WRONG_VALUES_ERROR; SQLSTATE: HY000
Message: Only %s PARTITIONING can use VALUES %s in partition definition
• Error number: 1481; Symbol: ER_PARTITION_MAXVALUE_ERROR; SQLSTATE: HY000
Message: MAXVALUE can only be used in last partition definition
• Error number: 1482; Symbol: ER_PARTITION_SUBPARTITION_ERROR; SQLSTATE: HY000
Message: Subpartitions can only be hash partitions and by key
• Error number: 1483; Symbol: ER_PARTITION_SUBPART_MIX_ERROR; SQLSTATE: HY000
Message: Must define subpartitions on all partitions if on one partition
• Error number: 1484; Symbol: ER_PARTITION_WRONG_NO_PART_ERROR; SQLSTATE: HY000
37
Message: Wrong number of partitions defined, mismatch with previous setting
• Error number: 1485; Symbol: ER_PARTITION_WRONG_NO_SUBPART_ERROR; SQLSTATE: HY000
Message: Wrong number of subpartitions defined, mismatch with previous setting
• Error number: 1486; Symbol: ER_WRONG_EXPR_IN_PARTITION_FUNC_ERROR; SQLSTATE: HY000
Message: Constant, random or timezone-dependent expressions in (sub)partitioning function are not
allowed
• Error number: 1487; Symbol: ER_NO_CONST_EXPR_IN_RANGE_OR_LIST_ERROR; SQLSTATE: HY000
Message: Expression in RANGE/LIST VALUES must be constant
• Error number: 1488; Symbol: ER_FIELD_NOT_FOUND_PART_ERROR; SQLSTATE: HY000
Message: Field in list of fields for partition function not found in table
• Error number: 1489; Symbol: ER_LIST_OF_FIELDS_ONLY_IN_HASH_ERROR; SQLSTATE: HY000
Message: List of fields is only allowed in KEY partitions
• Error number: 1490; Symbol: ER_INCONSISTENT_PARTITION_INFO_ERROR; SQLSTATE: HY000
Message: The partition info in the frm file is not consistent with what can be written into the frm file
• Error number: 1491; Symbol: ER_PARTITION_FUNC_NOT_ALLOWED_ERROR; SQLSTATE: HY000
Message: The %s function returns the wrong type
• Error number: 1492; Symbol: ER_PARTITIONS_MUST_BE_DEFINED_ERROR; SQLSTATE: HY000
Message: For %s partitions each partition must be defined
• Error number: 1493; Symbol: ER_RANGE_NOT_INCREASING_ERROR; SQLSTATE: HY000
Message: VALUES LESS THAN value must be strictly increasing for each partition
• Error number: 1494; Symbol: ER_INCONSISTENT_TYPE_OF_FUNCTIONS_ERROR; SQLSTATE: HY000
Message: VALUES value must be of same type as partition function
• Error number: 1495; Symbol: ER_MULTIPLE_DEF_CONST_IN_LIST_PART_ERROR; SQLSTATE:
HY000
Message: Multiple definition of same constant in list partitioning
• Error number: 1496; Symbol: ER_PARTITION_ENTRY_ERROR; SQLSTATE: HY000
Message: Partitioning can not be used stand-alone in query
• Error number: 1497; Symbol: ER_MIX_HANDLER_ERROR; SQLSTATE: HY000
Message: The mix of handlers in the partitions is not allowed in this version of MySQL
• Error number: 1498; Symbol: ER_PARTITION_NOT_DEFINED_ERROR; SQLSTATE: HY000
Message: For the partitioned engine it is necessary to define all %s
38
• Error number: 1499; Symbol: ER_TOO_MANY_PARTITIONS_ERROR; SQLSTATE: HY000
Message: Too many partitions (including subpartitions) were defined
• Error number: 1500; Symbol: ER_SUBPARTITION_ERROR; SQLSTATE: HY000
Message: It is only possible to mix RANGE/LIST partitioning with HASH/KEY partitioning for
subpartitioning
• Error number: 1501; Symbol: ER_CANT_CREATE_HANDLER_FILE; SQLSTATE: HY000
Message: Failed to create specific handler file
• Error number: 1502; Symbol: ER_BLOB_FIELD_IN_PART_FUNC_ERROR; SQLSTATE: HY000
Message: A BLOB field is not allowed in partition function
• Error number: 1503; Symbol: ER_UNIQUE_KEY_NEED_ALL_FIELDS_IN_PF; SQLSTATE: HY000
Message: A %s must include all columns in the table's partitioning function
• Error number: 1504; Symbol: ER_NO_PARTS_ERROR; SQLSTATE: HY000
Message: Number of %s = 0 is not an allowed value
• Error number: 1505; Symbol: ER_PARTITION_MGMT_ON_NONPARTITIONED; SQLSTATE: HY000
Message: Partition management on a not partitioned table is not possible
• Error number: 1506; Symbol: ER_FOREIGN_KEY_ON_PARTITIONED; SQLSTATE: HY000
Message: Foreign keys are not yet supported in conjunction with partitioning
• Error number: 1507; Symbol: ER_DROP_PARTITION_NON_EXISTENT; SQLSTATE: HY000
Message: Error in list of partitions to %s
• Error number: 1508; Symbol: ER_DROP_LAST_PARTITION; SQLSTATE: HY000
Message: Cannot remove all partitions, use DROP TABLE instead
• Error number: 1509; Symbol: ER_COALESCE_ONLY_ON_HASH_PARTITION; SQLSTATE: HY000
Message: COALESCE PARTITION can only be used on HASH/KEY partitions
• Error number: 1510; Symbol: ER_REORG_HASH_ONLY_ON_SAME_NO; SQLSTATE: HY000
Message: REORGANIZE PARTITION can only be used to reorganize partitions not to change their
numbers
• Error number: 1511; Symbol: ER_REORG_NO_PARAM_ERROR; SQLSTATE: HY000
Message: REORGANIZE PARTITION without parameters can only be used on auto-partitioned tables
using HASH PARTITIONs
• Error number: 1512; Symbol: ER_ONLY_ON_RANGE_LIST_PARTITION; SQLSTATE: HY000
Message: %s PARTITION can only be used on RANGE/LIST partitions
• Error number: 1513; Symbol: ER_ADD_PARTITION_SUBPART_ERROR; SQLSTATE: HY000
39
Message: Trying to Add partition(s) with wrong number of subpartitions
• Error number: 1514; Symbol: ER_ADD_PARTITION_NO_NEW_PARTITION; SQLSTATE: HY000
Message: At least one partition must be added
• Error number: 1515; Symbol: ER_COALESCE_PARTITION_NO_PARTITION; SQLSTATE: HY000
Message: At least one partition must be coalesced
• Error number: 1516; Symbol: ER_REORG_PARTITION_NOT_EXIST; SQLSTATE: HY000
Message: More partitions to reorganize than there are partitions
• Error number: 1517; Symbol: ER_SAME_NAME_PARTITION; SQLSTATE: HY000
Message: Duplicate partition name %s
• Error number: 1518; Symbol: ER_NO_BINLOG_ERROR; SQLSTATE: HY000
Message: It is not allowed to shut off binlog on this command
• Error number: 1519; Symbol: ER_CONSECUTIVE_REORG_PARTITIONS; SQLSTATE: HY000
Message: When reorganizing a set of partitions they must be in consecutive order
• Error number: 1520; Symbol: ER_REORG_OUTSIDE_RANGE; SQLSTATE: HY000
Message: Reorganize of range partitions cannot change total ranges except for last partition where it can
extend the range
• Error number: 1521; Symbol: ER_PARTITION_FUNCTION_FAILURE; SQLSTATE: HY000
Message: Partition function not supported in this version for this handler
• Error number: 1522; Symbol: ER_PART_STATE_ERROR; SQLSTATE: HY000
Message: Partition state cannot be defined from CREATE/ALTER TABLE
• Error number: 1523; Symbol: ER_LIMITED_PART_RANGE; SQLSTATE: HY000
Message: The %s handler only supports 32 bit integers in VALUES
• Error number: 1524; Symbol: ER_PLUGIN_IS_NOT_LOADED; SQLSTATE: HY000
Message: Plugin '%s' is not loaded
• Error number: 1525; Symbol: ER_WRONG_VALUE; SQLSTATE: HY000
Message: Incorrect %s value: '%s'
• Error number: 1526; Symbol: ER_NO_PARTITION_FOR_GIVEN_VALUE; SQLSTATE: HY000
Message: Table has no partition for value %s
• Error number: 1527; Symbol: ER_FILEGROUP_OPTION_ONLY_ONCE; SQLSTATE: HY000
Message: It is not allowed to specify %s more than once
40
• Error number: 1528; Symbol: ER_CREATE_FILEGROUP_FAILED; SQLSTATE: HY000
Message: Failed to create %s
• Error number: 1529; Symbol: ER_DROP_FILEGROUP_FAILED; SQLSTATE: HY000
Message: Failed to drop %s
• Error number: 1530; Symbol: ER_TABLESPACE_AUTO_EXTEND_ERROR; SQLSTATE: HY000
Message: The handler doesn't support autoextend of tablespaces
• Error number: 1531; Symbol: ER_WRONG_SIZE_NUMBER; SQLSTATE: HY000
Message: A size parameter was incorrectly specified, either number or on the form 10M
• Error number: 1532; Symbol: ER_SIZE_OVERFLOW_ERROR; SQLSTATE: HY000
Message: The size number was correct but we don't allow the digit part to be more than 2 billion
• Error number: 1533; Symbol: ER_ALTER_FILEGROUP_FAILED; SQLSTATE: HY000
Message: Failed to alter: %s
• Error number: 1534; Symbol: ER_BINLOG_ROW_LOGGING_FAILED; SQLSTATE: HY000
Message: Writing one row to the row-based binary log failed
• Error number: 1535; Symbol: ER_BINLOG_ROW_WRONG_TABLE_DEF; SQLSTATE: HY000
Message: Table definition on master and slave does not match: %s
• Error number: 1536; Symbol: ER_BINLOG_ROW_RBR_TO_SBR; SQLSTATE: HY000
Message: Slave running with --log-slave-updates must use row-based binary logging to be able to
replicate row-based binary log events
• Error number: 1537; Symbol: ER_EVENT_ALREADY_EXISTS; SQLSTATE: HY000
Message: Event '%s' already exists
• Error number: 1538; Symbol: ER_EVENT_STORE_FAILED; SQLSTATE: HY000
Message: Failed to store event %s. Error code %d from storage engine.
• Error number: 1539; Symbol: ER_EVENT_DOES_NOT_EXIST; SQLSTATE: HY000
Message: Unknown event '%s'
• Error number: 1540; Symbol: ER_EVENT_CANT_ALTER; SQLSTATE: HY000
Message: Failed to alter event '%s'
• Error number: 1541; Symbol: ER_EVENT_DROP_FAILED; SQLSTATE: HY000
Message: Failed to drop %s
• Error number: 1542; Symbol: ER_EVENT_INTERVAL_NOT_POSITIVE_OR_TOO_BIG; SQLSTATE:
HY000
41
Message: INTERVAL is either not positive or too big
• Error number: 1543; Symbol: ER_EVENT_ENDS_BEFORE_STARTS; SQLSTATE: HY000
Message: ENDS is either invalid or before STARTS
• Error number: 1544; Symbol: ER_EVENT_EXEC_TIME_IN_THE_PAST; SQLSTATE: HY000
Message: Event execution time is in the past. Event has been disabled
• Error number: 1545; Symbol: ER_EVENT_OPEN_TABLE_FAILED; SQLSTATE: HY000
Message: Failed to open mysql.event
• Error number: 1546; Symbol: ER_EVENT_NEITHER_M_EXPR_NOR_M_AT; SQLSTATE: HY000
Message: No datetime expression provided
• Error number: 1547; Symbol: ER_OBSOLETE_COL_COUNT_DOESNT_MATCH_CORRUPTED; SQLSTATE:
HY000
Message: Column count of mysql.%s is wrong. Expected %d, found %d. The table is probably corrupted
• Error number: 1548; Symbol: ER_OBSOLETE_CANNOT_LOAD_FROM_TABLE; SQLSTATE: HY000
Message: Cannot load from mysql.%s. The table is probably corrupted
• Error number: 1549; Symbol: ER_EVENT_CANNOT_DELETE; SQLSTATE: HY000
Message: Failed to delete the event from mysql.event
• Error number: 1550; Symbol: ER_EVENT_COMPILE_ERROR; SQLSTATE: HY000
Message: Error during compilation of event's body
• Error number: 1551; Symbol: ER_EVENT_SAME_NAME; SQLSTATE: HY000
Message: Same old and new event name
• Error number: 1552; Symbol: ER_EVENT_DATA_TOO_LONG; SQLSTATE: HY000
Message: Data for column '%s' too long
• Error number: 1553; Symbol: ER_DROP_INDEX_FK; SQLSTATE: HY000
Message: Cannot drop index '%s': needed in a foreign key constraint
InnoDB reports this error when you attempt to drop the last index that can enforce a particular referential
constraint.
For optimal performance with DML statements, InnoDB requires an index to exist on foreign key
columns, so that UPDATE and DELETE operations on a parent table can easily check whether
corresponding rows exist in the child table. MySQL creates or drops such indexes automatically when
needed, as a side-effect of CREATE TABLE, CREATE INDEX, and ALTER TABLE statements.
When you drop an index, InnoDB checks if the index is used for checking a foreign key constraint. It
is still OK to drop the index if there is another index that can be used to enforce the same constraint.
InnoDB prevents you from dropping the last index that can enforce a particular referential constraint.
42
• Error number: 1554; Symbol: ER_WARN_DEPRECATED_SYNTAX_WITH_VER; SQLSTATE: HY000
Message: The syntax '%s' is deprecated and will be removed in MySQL %s. Please use %s instead
• Error number: 1555; Symbol: ER_CANT_WRITE_LOCK_LOG_TABLE; SQLSTATE: HY000
Message: You can't write-lock a log table. Only read access is possible
• Error number: 1556; Symbol: ER_CANT_LOCK_LOG_TABLE; SQLSTATE: HY000
Message: You can't use locks with log tables.
• Error number: 1557; Symbol: ER_FOREIGN_DUPLICATE_KEY_OLD_UNUSED; SQLSTATE: 23000
Message: Upholding foreign key constraints for table '%s', entry '%s', key %d would lead to a duplicate
entry
• Error number: 1558; Symbol: ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE; SQLSTATE: HY000
Message: Column count of mysql.%s is wrong. Expected %d, found %d. Created with MySQL %d, now
running %d. Please use mysql_upgrade to fix this error.
• Error number: 1559; Symbol: ER_TEMP_TABLE_PREVENTS_SWITCH_OUT_OF_RBR; SQLSTATE:
HY000
Message: Cannot switch out of the row-based binary log format when the session has open temporary
tables
• Error number: 1560; Symbol: ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_FORMAT;
SQLSTATE: HY000
Message: Cannot change the binary logging format inside a stored function or trigger
• Error number: 1561; Symbol: ER_NDB_CANT_SWITCH_BINLOG_FORMAT; SQLSTATE: HY000
Message: The NDB cluster engine does not support changing the binlog format on the fly yet
• Error number: 1562; Symbol: ER_PARTITION_NO_TEMPORARY; SQLSTATE: HY000
Message: Cannot create temporary table with partitions
• Error number: 1563; Symbol: ER_PARTITION_CONST_DOMAIN_ERROR; SQLSTATE: HY000
Message: Partition constant is out of partition function domain
• Error number: 1564; Symbol: ER_PARTITION_FUNCTION_IS_NOT_ALLOWED; SQLSTATE: HY000
Message: This partition function is not allowed
• Error number: 1565; Symbol: ER_DDL_LOG_ERROR; SQLSTATE: HY000
Message: Error in DDL log
• Error number: 1566; Symbol: ER_NULL_IN_VALUES_LESS_THAN; SQLSTATE: HY000
Message: Not allowed to use NULL value in VALUES LESS THAN
• Error number: 1567; Symbol: ER_WRONG_PARTITION_NAME; SQLSTATE: HY000
Message: Incorrect partition name
43
• Error number: 1568; Symbol: ER_CANT_CHANGE_TX_CHARACTERISTICS; SQLSTATE: 25001
Message: Transaction characteristics can't be changed while a transaction is in progress
• Error number: 1569; Symbol: ER_DUP_ENTRY_AUTOINCREMENT_CASE; SQLSTATE: HY000
Message: ALTER TABLE causes auto_increment resequencing, resulting in duplicate entry '%s' for key
'%s'
• Error number: 1570; Symbol: ER_EVENT_MODIFY_QUEUE_ERROR; SQLSTATE: HY000
Message: Internal scheduler error %d
• Error number: 1571; Symbol: ER_EVENT_SET_VAR_ERROR; SQLSTATE: HY000
Message: Error during starting/stopping of the scheduler. Error code %u
• Error number: 1572; Symbol: ER_PARTITION_MERGE_ERROR; SQLSTATE: HY000
Message: Engine cannot be used in partitioned tables
• Error number: 1573; Symbol: ER_CANT_ACTIVATE_LOG; SQLSTATE: HY000
Message: Cannot activate '%s' log
• Error number: 1574; Symbol: ER_RBR_NOT_AVAILABLE; SQLSTATE: HY000
Message: The server was not built with row-based replication
• Error number: 1575; Symbol: ER_BASE64_DECODE_ERROR; SQLSTATE: HY000
Message: Decoding of base64 string failed
• Error number: 1576; Symbol: ER_EVENT_RECURSION_FORBIDDEN; SQLSTATE: HY000
Message: Recursion of EVENT DDL statements is forbidden when body is present
• Error number: 1577; Symbol: ER_EVENTS_DB_ERROR; SQLSTATE: HY000
Message: Cannot proceed because system tables used by Event Scheduler were found damaged at
server start
To address this issue, try running mysql_upgrade.
• Error number: 1578; Symbol: ER_ONLY_INTEGERS_ALLOWED; SQLSTATE: HY000
Message: Only integers allowed as number here
• Error number: 1579; Symbol: ER_UNSUPORTED_LOG_ENGINE; SQLSTATE: HY000
Message: This storage engine cannot be used for log tables"
• Error number: 1580; Symbol: ER_BAD_LOG_STATEMENT; SQLSTATE: HY000
Message: You cannot '%s' a log table if logging is enabled
• Error number: 1581; Symbol: ER_CANT_RENAME_LOG_TABLE; SQLSTATE: HY000
Message: Cannot rename '%s'. When logging enabled, rename to/from log table must rename two
tables: the log table to an archive table and another table back to '%s'
44
• Error number: 1582; Symbol: ER_WRONG_PARAMCOUNT_TO_NATIVE_FCT; SQLSTATE: 42000
Message: Incorrect parameter count in the call to native function '%s'
• Error number: 1583; Symbol: ER_WRONG_PARAMETERS_TO_NATIVE_FCT; SQLSTATE: 42000
Message: Incorrect parameters in the call to native function '%s'
• Error number: 1584; Symbol: ER_WRONG_PARAMETERS_TO_STORED_FCT; SQLSTATE: 42000
Message: Incorrect parameters in the call to stored function %s
• Error number: 1585; Symbol: ER_NATIVE_FCT_NAME_COLLISION; SQLSTATE: HY000
Message: This function '%s' has the same name as a native function
• Error number: 1586; Symbol: ER_DUP_ENTRY_WITH_KEY_NAME; SQLSTATE: 23000
Message: Duplicate entry '%s' for key '%s'
The format string for this error is also used with ER_DUP_ENTRY.
• Error number: 1587; Symbol: ER_BINLOG_PURGE_EMFILE; SQLSTATE: HY000
Message: Too many files opened, please execute the command again
• Error number: 1588; Symbol: ER_EVENT_CANNOT_CREATE_IN_THE_PAST; SQLSTATE: HY000
Message: Event execution time is in the past and ON COMPLETION NOT PRESERVE is set. The event
was dropped immediately after creation.
• Error number: 1589; Symbol: ER_EVENT_CANNOT_ALTER_IN_THE_PAST; SQLSTATE: HY000
Message: Event execution time is in the past and ON COMPLETION NOT PRESERVE is set. The event
was not changed. Specify a time in the future.
• Error number: 1590; Symbol: ER_SLAVE_INCIDENT; SQLSTATE: HY000
Message: The incident %s occured on the master. Message: %s
• Error number: 1591; Symbol: ER_NO_PARTITION_FOR_GIVEN_VALUE_SILENT; SQLSTATE: HY000
Message: Table has no partition for some existing values
• Error number: 1592; Symbol: ER_BINLOG_UNSAFE_STATEMENT; SQLSTATE: HY000
Message: Unsafe statement written to the binary log using statement format since BINLOG_FORMAT =
STATEMENT. %s
• Error number: 1593; Symbol: ER_SLAVE_FATAL_ERROR; SQLSTATE: HY000
Message: Fatal error: %s
• Error number: 1594; Symbol: ER_SLAVE_RELAY_LOG_READ_FAILURE; SQLSTATE: HY000
Message: Relay log read failure: %s
• Error number: 1595; Symbol: ER_SLAVE_RELAY_LOG_WRITE_FAILURE; SQLSTATE: HY000
Message: Relay log write failure: %s
45
• Error number: 1596; Symbol: ER_SLAVE_CREATE_EVENT_FAILURE; SQLSTATE: HY000
Message: Failed to create %s
• Error number: 1597; Symbol: ER_SLAVE_MASTER_COM_FAILURE; SQLSTATE: HY000
Message: Master command %s failed: %s
• Error number: 1598; Symbol: ER_BINLOG_LOGGING_IMPOSSIBLE; SQLSTATE: HY000
Message: Binary logging not possible. Message: %s
• Error number: 1599; Symbol: ER_VIEW_NO_CREATION_CTX; SQLSTATE: HY000
Message: View `%s`.`%s` has no creation context
• Error number: 1600; Symbol: ER_VIEW_INVALID_CREATION_CTX; SQLSTATE: HY000
Message: Creation context of view `%s`.`%s' is invalid
• Error number: 1601; Symbol: ER_SR_INVALID_CREATION_CTX; SQLSTATE: HY000
Message: Creation context of stored routine `%s`.`%s` is invalid
• Error number: 1602; Symbol: ER_TRG_CORRUPTED_FILE; SQLSTATE: HY000
Message: Corrupted TRG file for table `%s`.`%s`
• Error number: 1603; Symbol: ER_TRG_NO_CREATION_CTX; SQLSTATE: HY000
Message: Triggers for table `%s`.`%s` have no creation context
• Error number: 1604; Symbol: ER_TRG_INVALID_CREATION_CTX; SQLSTATE: HY000
Message: Trigger creation context of table `%s`.`%s` is invalid
• Error number: 1605; Symbol: ER_EVENT_INVALID_CREATION_CTX; SQLSTATE: HY000
Message: Creation context of event `%s`.`%s` is invalid
• Error number: 1606; Symbol: ER_TRG_CANT_OPEN_TABLE; SQLSTATE: HY000
Message: Cannot open table for trigger `%s`.`%s`
• Error number: 1607; Symbol: ER_CANT_CREATE_SROUTINE; SQLSTATE: HY000
Message: Cannot create stored routine `%s`. Check warnings
• Error number: 1608; Symbol: ER_NEVER_USED; SQLSTATE: HY000
Message: Ambiguous slave modes combination. %s
• Error number: 1609; Symbol: ER_NO_FORMAT_DESCRIPTION_EVENT_BEFORE_BINLOG_STATEMENT;
SQLSTATE: HY000
Message: The BINLOG statement of type `%s` was not preceded by a format description BINLOG
statement.
• Error number: 1610; Symbol: ER_SLAVE_CORRUPT_EVENT; SQLSTATE: HY000
46
Message: Corrupted replication event was detected
• Error number: 1611; Symbol: ER_LOAD_DATA_INVALID_COLUMN; SQLSTATE: HY000
Message: Invalid column reference (%s) in LOAD DATA
ER_LOAD_DATA_INVALID_COLUMN was removed after 5.7.7.
• Error number: 1611; Symbol: ER_LOAD_DATA_INVALID_COLUMN_UNUSED; SQLSTATE: HY000
Message: Invalid column reference (%s) in LOAD DATA
ER_LOAD_DATA_INVALID_COLUMN_UNUSED was added in 5.7.8.
• Error number: 1612; Symbol: ER_LOG_PURGE_NO_FILE; SQLSTATE: HY000
Message: Being purged log %s was not found
• Error number: 1613; Symbol: ER_XA_RBTIMEOUT; SQLSTATE: XA106
Message: XA_RBTIMEOUT: Transaction branch was rolled back: took too long
• Error number: 1614; Symbol: ER_XA_RBDEADLOCK; SQLSTATE: XA102
Message: XA_RBDEADLOCK: Transaction branch was rolled back: deadlock was detected
• Error number: 1615; Symbol: ER_NEED_REPREPARE; SQLSTATE: HY000
Message: Prepared statement needs to be re-prepared
• Error number: 1616; Symbol: ER_DELAYED_NOT_SUPPORTED; SQLSTATE: HY000
Message: DELAYED option not supported for table '%s'
• Error number: 1617; Symbol: WARN_NO_MASTER_INFO; SQLSTATE: HY000
Message: The master info structure does not exist
• Error number: 1618; Symbol: WARN_OPTION_IGNORED; SQLSTATE: HY000
Message: <%s> option ignored
• Error number: 1619; Symbol: WARN_PLUGIN_DELETE_BUILTIN; SQLSTATE: HY000
Message: Built-in plugins cannot be deleted
WARN_PLUGIN_DELETE_BUILTIN was removed after 5.7.4.
• Error number: 1619; Symbol: ER_PLUGIN_DELETE_BUILTIN; SQLSTATE: HY000
Message: Built-in plugins cannot be deleted
ER_PLUGIN_DELETE_BUILTIN was added in 5.7.5.
• Error number: 1620; Symbol: WARN_PLUGIN_BUSY; SQLSTATE: HY000
Message: Plugin is busy and will be uninstalled on shutdown
• Error number: 1621; Symbol: ER_VARIABLE_IS_READONLY; SQLSTATE: HY000
47
Message: %s variable '%s' is read-only. Use SET %s to assign the value
• Error number: 1622; Symbol: ER_WARN_ENGINE_TRANSACTION_ROLLBACK; SQLSTATE: HY000
Message: Storage engine %s does not support rollback for this statement. Transaction rolled back and
must be restarted
• Error number: 1623; Symbol: ER_SLAVE_HEARTBEAT_FAILURE; SQLSTATE: HY000
Message: Unexpected master's heartbeat data: %s
• Error number: 1624; Symbol: ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE; SQLSTATE: HY000
Message: The requested value for the heartbeat period is either negative or exceeds the maximum
allowed (%s seconds).
• Error number: 1625; Symbol: ER_NDB_REPLICATION_SCHEMA_ERROR; SQLSTATE: HY000
Message: Bad schema for mysql.ndb_replication table. Message: %s
• Error number: 1626; Symbol: ER_CONFLICT_FN_PARSE_ERROR; SQLSTATE: HY000
Message: Error in parsing conflict function. Message: %s
• Error number: 1627; Symbol: ER_EXCEPTIONS_WRITE_ERROR; SQLSTATE: HY000
Message: Write to exceptions table failed. Message: %s"
• Error number: 1628; Symbol: ER_TOO_LONG_TABLE_COMMENT; SQLSTATE: HY000
Message: Comment for table '%s' is too long (max = %lu)
• Error number: 1629; Symbol: ER_TOO_LONG_FIELD_COMMENT; SQLSTATE: HY000
Message: Comment for field '%s' is too long (max = %lu)
• Error number: 1630; Symbol: ER_FUNC_INEXISTENT_NAME_COLLISION; SQLSTATE: 42000
Message: FUNCTION %s does not exist. Check the 'Function Name Parsing and Resolution' section in
the Reference Manual
• Error number: 1631; Symbol: ER_DATABASE_NAME; SQLSTATE: HY000
Message: Database
• Error number: 1632; Symbol: ER_TABLE_NAME; SQLSTATE: HY000
Message: Table
• Error number: 1633; Symbol: ER_PARTITION_NAME; SQLSTATE: HY000
Message: Partition
• Error number: 1634; Symbol: ER_SUBPARTITION_NAME; SQLSTATE: HY000
Message: Subpartition
• Error number: 1635; Symbol: ER_TEMPORARY_NAME; SQLSTATE: HY000
Message: Temporary
48
• Error number: 1636; Symbol: ER_RENAMED_NAME; SQLSTATE: HY000
Message: Renamed
• Error number: 1637; Symbol: ER_TOO_MANY_CONCURRENT_TRXS; SQLSTATE: HY000
Message: Too many active concurrent transactions
• Error number: 1638; Symbol: WARN_NON_ASCII_SEPARATOR_NOT_IMPLEMENTED; SQLSTATE:
HY000
Message: Non-ASCII separator arguments are not fully supported
• Error number: 1639; Symbol: ER_DEBUG_SYNC_TIMEOUT; SQLSTATE: HY000
Message: debug sync point wait timed out
• Error number: 1640; Symbol: ER_DEBUG_SYNC_HIT_LIMIT; SQLSTATE: HY000
Message: debug sync point hit limit reached
• Error number: 1641; Symbol: ER_DUP_SIGNAL_SET; SQLSTATE: 42000
Message: Duplicate condition information item '%s'
• Error number: 1642; Symbol: ER_SIGNAL_WARN; SQLSTATE: 01000
Message: Unhandled user-defined warning condition
• Error number: 1643; Symbol: ER_SIGNAL_NOT_FOUND; SQLSTATE: 02000
Message: Unhandled user-defined not found condition
• Error number: 1644; Symbol: ER_SIGNAL_EXCEPTION; SQLSTATE: HY000
Message: Unhandled user-defined exception condition
• Error number: 1645; Symbol: ER_RESIGNAL_WITHOUT_ACTIVE_HANDLER; SQLSTATE: 0K000
Message: RESIGNAL when handler not active
• Error number: 1646; Symbol: ER_SIGNAL_BAD_CONDITION_TYPE; SQLSTATE: HY000
Message: SIGNAL/RESIGNAL can only use a CONDITION defined with SQLSTATE
• Error number: 1647; Symbol: WARN_COND_ITEM_TRUNCATED; SQLSTATE: HY000
Message: Data truncated for condition item '%s'
• Error number: 1648; Symbol: ER_COND_ITEM_TOO_LONG; SQLSTATE: HY000
Message: Data too long for condition item '%s'
• Error number: 1649; Symbol: ER_UNKNOWN_LOCALE; SQLSTATE: HY000
Message: Unknown locale: '%s'
• Error number: 1650; Symbol: ER_SLAVE_IGNORE_SERVER_IDS; SQLSTATE: HY000
Message: The requested server id %d clashes with the slave startup option --replicate-same-server-id
49
• Error number: 1651; Symbol: ER_QUERY_CACHE_DISABLED; SQLSTATE: HY000
Message: Query cache is disabled; restart the server with query_cache_type=1 to enable it
• Error number: 1652; Symbol: ER_SAME_NAME_PARTITION_FIELD; SQLSTATE: HY000
Message: Duplicate partition field name '%s'
• Error number: 1653; Symbol: ER_PARTITION_COLUMN_LIST_ERROR; SQLSTATE: HY000
Message: Inconsistency in usage of column lists for partitioning
• Error number: 1654; Symbol: ER_WRONG_TYPE_COLUMN_VALUE_ERROR; SQLSTATE: HY000
Message: Partition column values of incorrect type
• Error number: 1655; Symbol: ER_TOO_MANY_PARTITION_FUNC_FIELDS_ERROR; SQLSTATE: HY000
Message: Too many fields in '%s'
• Error number: 1656; Symbol: ER_MAXVALUE_IN_VALUES_IN; SQLSTATE: HY000
Message: Cannot use MAXVALUE as value in VALUES IN
• Error number: 1657; Symbol: ER_TOO_MANY_VALUES_ERROR; SQLSTATE: HY000
Message: Cannot have more than one value for this type of %s partitioning
• Error number: 1658; Symbol: ER_ROW_SINGLE_PARTITION_FIELD_ERROR; SQLSTATE: HY000
Message: Row expressions in VALUES IN only allowed for multi-field column partitioning
• Error number: 1659; Symbol: ER_FIELD_TYPE_NOT_ALLOWED_AS_PARTITION_FIELD; SQLSTATE:
HY000
Message: Field '%s' is of a not allowed type for this type of partitioning
• Error number: 1660; Symbol: ER_PARTITION_FIELDS_TOO_LONG; SQLSTATE: HY000
Message: The total length of the partitioning fields is too large
• Error number: 1661; Symbol: ER_BINLOG_ROW_ENGINE_AND_STMT_ENGINE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since both row-incapable engines
and statement-incapable engines are involved.
• Error number: 1662; Symbol: ER_BINLOG_ROW_MODE_AND_STMT_ENGINE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since BINLOG_FORMAT = ROW
and at least one table uses a storage engine limited to statement-based logging.
• Error number: 1663; Symbol: ER_BINLOG_UNSAFE_AND_STMT_ENGINE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since statement is unsafe, storage
engine is limited to statement-based logging, and BINLOG_FORMAT = MIXED. %s
• Error number: 1664; Symbol: ER_BINLOG_ROW_INJECTION_AND_STMT_ENGINE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since statement is in row format
and at least one table uses a storage engine limited to statement-based logging.
50
• Error number: 1665; Symbol: ER_BINLOG_STMT_MODE_AND_ROW_ENGINE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since BINLOG_FORMAT =
STATEMENT and at least one table uses a storage engine limited to row-based logging.%s
• Error number: 1666; Symbol: ER_BINLOG_ROW_INJECTION_AND_STMT_MODE; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since statement is in row format
and BINLOG_FORMAT = STATEMENT.
• Error number: 1667; Symbol: ER_BINLOG_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINE;
SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since more than one engine is
involved and at least one engine is self-logging.
• Error number: 1668; Symbol: ER_BINLOG_UNSAFE_LIMIT; SQLSTATE: HY000
Message: The statement is unsafe because it uses a LIMIT clause. This is unsafe because the set of
rows included cannot be predicted.
• Error number: 1669; Symbol: ER_UNUSED4; SQLSTATE: HY000
Message: The statement is unsafe because it uses INSERT DELAYED. This is unsafe because the
times when rows are inserted cannot be predicted.
• Error number: 1670; Symbol: ER_BINLOG_UNSAFE_SYSTEM_TABLE; SQLSTATE: HY000
Message: The statement is unsafe because it uses the general log, slow query log, or
performance_schema table(s). This is unsafe because system tables may differ on slaves.
• Error number: 1671; Symbol: ER_BINLOG_UNSAFE_AUTOINC_COLUMNS; SQLSTATE: HY000
Message: Statement is unsafe because it invokes a trigger or a stored function that inserts into an
AUTO_INCREMENT column. Inserted values cannot be logged correctly.
• Error number: 1672; Symbol: ER_BINLOG_UNSAFE_UDF; SQLSTATE: HY000
Message: Statement is unsafe because it uses a UDF which may not return the same value on the
slave.
• Error number: 1673; Symbol: ER_BINLOG_UNSAFE_SYSTEM_VARIABLE; SQLSTATE: HY000
Message: Statement is unsafe because it uses a system variable that may have a different value on the
slave.
• Error number: 1674; Symbol: ER_BINLOG_UNSAFE_SYSTEM_FUNCTION; SQLSTATE: HY000
Message: Statement is unsafe because it uses a system function that may return a different value on the
slave.
• Error number: 1675; Symbol: ER_BINLOG_UNSAFE_NONTRANS_AFTER_TRANS; SQLSTATE: HY000
Message: Statement is unsafe because it accesses a non-transactional table after accessing a
transactional table within the same transaction.
• Error number: 1676; Symbol: ER_MESSAGE_AND_STATEMENT; SQLSTATE: HY000
Message: %s Statement: %s
51
• Error number: 1677; Symbol: ER_SLAVE_CONVERSION_FAILED; SQLSTATE: HY000
Message: Column %d of table '%s.%s' cannot be converted from type '%s' to type '%s'
• Error number: 1678; Symbol: ER_SLAVE_CANT_CREATE_CONVERSION; SQLSTATE: HY000
Message: Can't create conversion table for table '%s.%s'
• Error number: 1679; Symbol: ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_FORMAT;
SQLSTATE: HY000
Message: Cannot modify @@session.binlog_format inside a transaction
• Error number: 1680; Symbol: ER_PATH_LENGTH; SQLSTATE: HY000
Message: The path specified for %s is too long.
• Error number: 1681; Symbol: ER_WARN_DEPRECATED_SYNTAX_NO_REPLACEMENT; SQLSTATE:
HY000
Message: '%s' is deprecated and will be removed in a future release.
• Error number: 1682; Symbol: ER_WRONG_NATIVE_TABLE_STRUCTURE; SQLSTATE: HY000
Message: Native table '%s'.'%s' has the wrong structure
• Error number: 1683; Symbol: ER_WRONG_PERFSCHEMA_USAGE; SQLSTATE: HY000
Message: Invalid performance_schema usage.
• Error number: 1684; Symbol: ER_WARN_I_S_SKIPPED_TABLE; SQLSTATE: HY000
Message: Table '%s'.'%s' was skipped since its definition is being modified by concurrent DDL statement
• Error number: 1685; Symbol: ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_DIRECT;
SQLSTATE: HY000
Message: Cannot modify @@session.binlog_direct_non_transactional_updates inside a transaction
• Error number: 1686; Symbol: ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_DIRECT;
SQLSTATE: HY000
Message: Cannot change the binlog direct flag inside a stored function or trigger
• Error number: 1687; Symbol: ER_SPATIAL_MUST_HAVE_GEOM_COL; SQLSTATE: 42000
Message: A SPATIAL index may only contain a geometrical type column
• Error number: 1688; Symbol: ER_TOO_LONG_INDEX_COMMENT; SQLSTATE: HY000
Message: Comment for index '%s' is too long (max = %lu)
• Error number: 1689; Symbol: ER_LOCK_ABORTED; SQLSTATE: HY000
Message: Wait on a lock was aborted due to a pending exclusive lock
• Error number: 1690; Symbol: ER_DATA_OUT_OF_RANGE; SQLSTATE: 22003
Message: %s value is out of range in '%s'
52
• Error number: 1691; Symbol: ER_WRONG_SPVAR_TYPE_IN_LIMIT; SQLSTATE: HY000
Message: A variable of a non-integer based type in LIMIT clause
• Error number: 1692; Symbol:
ER_BINLOG_UNSAFE_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINE; SQLSTATE: HY000
Message: Mixing self-logging and non-self-logging engines in a statement is unsafe.
• Error number: 1693; Symbol: ER_BINLOG_UNSAFE_MIXED_STATEMENT; SQLSTATE: HY000
Message: Statement accesses nontransactional table as well as transactional or temporary table, and
writes to any of them.
• Error number: 1694; Symbol: ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_SQL_LOG_BIN;
SQLSTATE: HY000
Message: Cannot modify @@session.sql_log_bin inside a transaction
• Error number: 1695; Symbol: ER_STORED_FUNCTION_PREVENTS_SWITCH_SQL_LOG_BIN;
SQLSTATE: HY000
Message: Cannot change the sql_log_bin inside a stored function or trigger
• Error number: 1696; Symbol: ER_FAILED_READ_FROM_PAR_FILE; SQLSTATE: HY000
Message: Failed to read from the .par file
• Error number: 1697; Symbol: ER_VALUES_IS_NOT_INT_TYPE_ERROR; SQLSTATE: HY000
Message: VALUES value for partition '%s' must have type INT
• Error number: 1698; Symbol: ER_ACCESS_DENIED_NO_PASSWORD_ERROR; SQLSTATE: 28000
Message: Access denied for user '%s'@'%s'
• Error number: 1699; Symbol: ER_SET_PASSWORD_AUTH_PLUGIN; SQLSTATE: HY000
Message: SET PASSWORD has no significance for users authenticating via plugins
• Error number: 1700; Symbol: ER_GRANT_PLUGIN_USER_EXISTS; SQLSTATE: HY000
Message: GRANT with IDENTIFIED WITH is illegal because the user %-.*s already exists
• Error number: 1701; Symbol: ER_TRUNCATE_ILLEGAL_FK; SQLSTATE: 42000
Message: Cannot truncate a table referenced in a foreign key constraint (%s)
• Error number: 1702; Symbol: ER_PLUGIN_IS_PERMANENT; SQLSTATE: HY000
Message: Plugin '%s' is force_plus_permanent and can not be unloaded
• Error number: 1703; Symbol: ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MIN; SQLSTATE:
HY000
Message: The requested value for the heartbeat period is less than 1 millisecond. The value is reset to
0, meaning that heartbeating will effectively be disabled.
• Error number: 1704; Symbol: ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MAX; SQLSTATE:
HY000
53
Message: The requested value for the heartbeat period exceeds the value of `slave_net_timeout'
seconds. A sensible value for the period should be less than the timeout.
• Error number: 1705; Symbol: ER_STMT_CACHE_FULL; SQLSTATE: HY000
Message: Multi-row statements required more than 'max_binlog_stmt_cache_size' bytes of storage;
increase this mysqld variable and try again
• Error number: 1706; Symbol: ER_MULTI_UPDATE_KEY_CONFLICT; SQLSTATE: HY000
Message: Primary key/partition key update is not allowed since the table is updated both as '%s' and
'%s'.
• Error number: 1707; Symbol: ER_TABLE_NEEDS_REBUILD; SQLSTATE: HY000
Message: Table rebuild required. Please do "ALTER TABLE `%s` FORCE" or dump/reload to fix it!
• Error number: 1708; Symbol: WARN_OPTION_BELOW_LIMIT; SQLSTATE: HY000
Message: The value of '%s' should be no less than the value of '%s'
• Error number: 1709; Symbol: ER_INDEX_COLUMN_TOO_LONG; SQLSTATE: HY000
Message: Index column size too large. The maximum column size is %lu bytes.
• Error number: 1710; Symbol: ER_ERROR_IN_TRIGGER_BODY; SQLSTATE: HY000
Message: Trigger '%s' has an error in its body: '%s'
• Error number: 1711; Symbol: ER_ERROR_IN_UNKNOWN_TRIGGER_BODY; SQLSTATE: HY000
Message: Unknown trigger has an error in its body: '%s'
• Error number: 1712; Symbol: ER_INDEX_CORRUPT; SQLSTATE: HY000
Message: Index %s is corrupted
• Error number: 1713; Symbol: ER_UNDO_RECORD_TOO_BIG; SQLSTATE: HY000
Message: Undo log record is too big.
• Error number: 1714; Symbol: ER_BINLOG_UNSAFE_INSERT_IGNORE_SELECT; SQLSTATE: HY000
Message: INSERT IGNORE... SELECT is unsafe because the order in which rows are retrieved by the
SELECT determines which (if any) rows are ignored. This order cannot be predicted and may differ on
master and the slave.
• Error number: 1715; Symbol: ER_BINLOG_UNSAFE_INSERT_SELECT_UPDATE; SQLSTATE: HY000
Message: INSERT... SELECT... ON DUPLICATE KEY UPDATE is unsafe because the order in which
rows are retrieved by the SELECT determines which (if any) rows are updated. This order cannot be
predicted and may differ on master and the slave.
• Error number: 1716; Symbol: ER_BINLOG_UNSAFE_REPLACE_SELECT; SQLSTATE: HY000
Message: REPLACE... SELECT is unsafe because the order in which rows are retrieved by the SELECT
determines which (if any) rows are replaced. This order cannot be predicted and may differ on master
and the slave.
54
• Error number: 1717; Symbol: ER_BINLOG_UNSAFE_CREATE_IGNORE_SELECT; SQLSTATE: HY000
Message: CREATE... IGNORE SELECT is unsafe because the order in which rows are retrieved by the
SELECT determines which (if any) rows are ignored. This order cannot be predicted and may differ on
master and the slave.
• Error number: 1718; Symbol: ER_BINLOG_UNSAFE_CREATE_REPLACE_SELECT; SQLSTATE: HY000
Message: CREATE... REPLACE SELECT is unsafe because the order in which rows are retrieved by
the SELECT determines which (if any) rows are replaced. This order cannot be predicted and may differ
on master and the slave.
• Error number: 1719; Symbol: ER_BINLOG_UNSAFE_UPDATE_IGNORE; SQLSTATE: HY000
Message: UPDATE IGNORE is unsafe because the order in which rows are updated determines which
(if any) rows are ignored. This order cannot be predicted and may differ on master and the slave.
• Error number: 1720; Symbol: ER_PLUGIN_NO_UNINSTALL; SQLSTATE: HY000
Message: Plugin '%s' is marked as not dynamically uninstallable. You have to stop the server to uninstall
it.
• Error number: 1721; Symbol: ER_PLUGIN_NO_INSTALL; SQLSTATE: HY000
Message: Plugin '%s' is marked as not dynamically installable. You have to stop the server to install it.
• Error number: 1722; Symbol: ER_BINLOG_UNSAFE_WRITE_AUTOINC_SELECT; SQLSTATE: HY000
Message: Statements writing to a table with an auto-increment column after selecting from another table
are unsafe because the order in which rows are retrieved determines what (if any) rows will be written.
This order cannot be predicted and may differ on master and the slave.
• Error number: 1723; Symbol: ER_BINLOG_UNSAFE_CREATE_SELECT_AUTOINC; SQLSTATE: HY000
Message: CREATE TABLE... SELECT... on a table with an auto-increment column is unsafe because
the order in which rows are retrieved by the SELECT determines which (if any) rows are inserted. This
order cannot be predicted and may differ on master and the slave.
• Error number: 1724; Symbol: ER_BINLOG_UNSAFE_INSERT_TWO_KEYS; SQLSTATE: HY000
Message: INSERT... ON DUPLICATE KEY UPDATE on a table with more than one UNIQUE KEY is
unsafe
• Error number: 1725; Symbol: ER_TABLE_IN_FK_CHECK; SQLSTATE: HY000
Message: Table is being used in foreign key check.
• Error number: 1726; Symbol: ER_UNSUPPORTED_ENGINE; SQLSTATE: HY000
Message: Storage engine '%s' does not support system tables. [%s.%s]
• Error number: 1727; Symbol: ER_BINLOG_UNSAFE_AUTOINC_NOT_FIRST; SQLSTATE: HY000
Message: INSERT into autoincrement field which is not the first part in the composed primary key is
unsafe.
• Error number: 1728; Symbol: ER_CANNOT_LOAD_FROM_TABLE_V2; SQLSTATE: HY000
Message: Cannot load from %s.%s. The table is probably corrupted
55
• Error number: 1729; Symbol: ER_MASTER_DELAY_VALUE_OUT_OF_RANGE; SQLSTATE: HY000
Message: The requested value %s for the master delay exceeds the maximum %u
• Error number: 1730; Symbol: ER_ONLY_FD_AND_RBR_EVENTS_ALLOWED_IN_BINLOG_STATEMENT;
SQLSTATE: HY000
Message: Only Format_description_log_event and row events are allowed in BINLOG statements (but
%s was provided)
• Error number: 1731; Symbol: ER_PARTITION_EXCHANGE_DIFFERENT_OPTION; SQLSTATE: HY000
Message: Non matching attribute '%s' between partition and table
• Error number: 1732; Symbol: ER_PARTITION_EXCHANGE_PART_TABLE; SQLSTATE: HY000
Message: Table to exchange with partition is partitioned: '%s'
• Error number: 1733; Symbol: ER_PARTITION_EXCHANGE_TEMP_TABLE; SQLSTATE: HY000
Message: Table to exchange with partition is temporary: '%s'
• Error number: 1734; Symbol: ER_PARTITION_INSTEAD_OF_SUBPARTITION; SQLSTATE: HY000
Message: Subpartitioned table, use subpartition instead of partition
• Error number: 1735; Symbol: ER_UNKNOWN_PARTITION; SQLSTATE: HY000
Message: Unknown partition '%s' in table '%s'
• Error number: 1736; Symbol: ER_TABLES_DIFFERENT_METADATA; SQLSTATE: HY000
Message: Tables have different definitions
• Error number: 1737; Symbol: ER_ROW_DOES_NOT_MATCH_PARTITION; SQLSTATE: HY000
Message: Found a row that does not match the partition
• Error number: 1738; Symbol: ER_BINLOG_CACHE_SIZE_GREATER_THAN_MAX; SQLSTATE: HY000
Message: Option binlog_cache_size (%lu) is greater than max_binlog_cache_size (%lu); setting
binlog_cache_size equal to max_binlog_cache_size.
• Error number: 1739; Symbol: ER_WARN_INDEX_NOT_APPLICABLE; SQLSTATE: HY000
Message: Cannot use %s access on index '%s' due to type or collation conversion on field '%s'
• Error number: 1740; Symbol: ER_PARTITION_EXCHANGE_FOREIGN_KEY; SQLSTATE: HY000
Message: Table to exchange with partition has foreign key references: '%s'
• Error number: 1741; Symbol: ER_NO_SUCH_KEY_VALUE; SQLSTATE: HY000
Message: Key value '%s' was not found in table '%s.%s'
• Error number: 1742; Symbol: ER_RPL_INFO_DATA_TOO_LONG; SQLSTATE: HY000
Message: Data for column '%s' too long
• Error number: 1743; Symbol: ER_NETWORK_READ_EVENT_CHECKSUM_FAILURE; SQLSTATE: HY000
56
Message: Replication event checksum verification failed while reading from network.
• Error number: 1744; Symbol: ER_BINLOG_READ_EVENT_CHECKSUM_FAILURE; SQLSTATE: HY000
Message: Replication event checksum verification failed while reading from a log file.
• Error number: 1745; Symbol: ER_BINLOG_STMT_CACHE_SIZE_GREATER_THAN_MAX; SQLSTATE:
HY000
Message: Option binlog_stmt_cache_size (%lu) is greater than max_binlog_stmt_cache_size (%lu);
setting binlog_stmt_cache_size equal to max_binlog_stmt_cache_size.
• Error number: 1746; Symbol: ER_CANT_UPDATE_TABLE_IN_CREATE_TABLE_SELECT; SQLSTATE:
HY000
Message: Can't update table '%s' while '%s' is being created.
• Error number: 1747; Symbol: ER_PARTITION_CLAUSE_ON_NONPARTITIONED; SQLSTATE: HY000
Message: PARTITION () clause on non partitioned table
• Error number: 1748; Symbol: ER_ROW_DOES_NOT_MATCH_GIVEN_PARTITION_SET; SQLSTATE:
HY000
Message: Found a row not matching the given partition set
• Error number: 1749; Symbol: ER_NO_SUCH_PARTITION__UNUSED; SQLSTATE: HY000
Message: partition '%s' doesn't exist
• Error number: 1750; Symbol: ER_CHANGE_RPL_INFO_REPOSITORY_FAILURE; SQLSTATE: HY000
Message: Failure while changing the type of replication repository: %s.
• Error number: 1751; Symbol:
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_CREATED_TEMP_TABLE; SQLSTATE: HY000
Message: The creation of some temporary tables could not be rolled back.
• Error number: 1752; Symbol:
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_DROPPED_TEMP_TABLE; SQLSTATE: HY000
Message: Some temporary tables were dropped, but these operations could not be rolled back.
• Error number: 1753; Symbol: ER_MTS_FEATURE_IS_NOT_SUPPORTED; SQLSTATE: HY000
Message: %s is not supported in multi-threaded slave mode. %s
• Error number: 1754; Symbol: ER_MTS_UPDATED_DBS_GREATER_MAX; SQLSTATE: HY000
Message: The number of modified databases exceeds the maximum %d; the database names will not
be included in the replication event metadata.
• Error number: 1755; Symbol: ER_MTS_CANT_PARALLEL; SQLSTATE: HY000
Message: Cannot execute the current event group in the parallel mode. Encountered event %s, relay-log
name %s, position %s which prevents execution of this event group in parallel mode. Reason: %s.
• Error number: 1756; Symbol: ER_MTS_INCONSISTENT_DATA; SQLSTATE: HY000
57
Message: %s
• Error number: 1757; Symbol: ER_FULLTEXT_NOT_SUPPORTED_WITH_PARTITIONING; SQLSTATE:
HY000
Message: FULLTEXT index is not supported for partitioned tables.
• Error number: 1758; Symbol: ER_DA_INVALID_CONDITION_NUMBER; SQLSTATE: 35000
Message: Invalid condition number
• Error number: 1759; Symbol: ER_INSECURE_PLAIN_TEXT; SQLSTATE: HY000
Message: Sending passwords in plain text without SSL/TLS is extremely insecure.
• Error number: 1760; Symbol: ER_INSECURE_CHANGE_MASTER; SQLSTATE: HY000
Message: Storing MySQL user name or password information in the master info repository is not secure
and is therefore not recommended. Please consider using the USER and PASSWORD connection
options for START SLAVE; see the 'START SLAVE Syntax' in the MySQL Manual for more information.
• Error number: 1761; Symbol: ER_FOREIGN_DUPLICATE_KEY_WITH_CHILD_INFO; SQLSTATE:
23000
Message: Foreign key constraint for table '%s', record '%s' would lead to a duplicate entry in table '%s',
key '%s'
• Error number: 1762; Symbol: ER_FOREIGN_DUPLICATE_KEY_WITHOUT_CHILD_INFO; SQLSTATE:
23000
Message: Foreign key constraint for table '%s', record '%s' would lead to a duplicate entry in a child table
• Error number: 1763; Symbol: ER_SQLTHREAD_WITH_SECURE_SLAVE; SQLSTATE: HY000
Message: Setting authentication options is not possible when only the Slave SQL Thread is being
started.
• Error number: 1764; Symbol: ER_TABLE_HAS_NO_FT; SQLSTATE: HY000
Message: The table does not have FULLTEXT index to support this query
• Error number: 1765; Symbol: ER_VARIABLE_NOT_SETTABLE_IN_SF_OR_TRIGGER; SQLSTATE:
HY000
Message: The system variable %s cannot be set in stored functions or triggers.
• Error number: 1766; Symbol: ER_VARIABLE_NOT_SETTABLE_IN_TRANSACTION; SQLSTATE: HY000
Message: The system variable %s cannot be set when there is an ongoing transaction.
• Error number: 1767; Symbol: ER_GTID_NEXT_IS_NOT_IN_GTID_NEXT_LIST; SQLSTATE: HY000
Message: The system variable @@SESSION.GTID_NEXT has the value %s, which is not listed in
@@SESSION.GTID_NEXT_LIST.
• Error number: 1768; Symbol:
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION_WHEN_GTID_NEXT_LIST_IS_NULL; SQLSTATE:
HY000
58
Message: The system variable @@SESSION.GTID_NEXT cannot change inside a transaction.
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION_WHEN_GTID_NEXT_LIST_IS_NULL was
removed after 5.7.5.
• Error number: 1768; Symbol: ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION; SQLSTATE: HY000
Message: The system variable @@SESSION.GTID_NEXT cannot change inside a transaction.
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION was added in 5.7.6.
• Error number: 1769; Symbol: ER_SET_STATEMENT_CANNOT_INVOKE_FUNCTION; SQLSTATE: HY000
Message: The statement 'SET %s' cannot invoke a stored function.
• Error number: 1770; Symbol:
ER_GTID_NEXT_CANT_BE_AUTOMATIC_IF_GTID_NEXT_LIST_IS_NON_NULL; SQLSTATE: HY000
Message: The system variable @@SESSION.GTID_NEXT cannot be 'AUTOMATIC' when
@@SESSION.GTID_NEXT_LIST is non-NULL.
• Error number: 1771; Symbol: ER_SKIPPING_LOGGED_TRANSACTION; SQLSTATE: HY000
Message: Skipping transaction %s because it has already been executed and logged.
• Error number: 1772; Symbol: ER_MALFORMED_GTID_SET_SPECIFICATION; SQLSTATE: HY000
Message: Malformed GTID set specification '%s'.
• Error number: 1773; Symbol: ER_MALFORMED_GTID_SET_ENCODING; SQLSTATE: HY000
Message: Malformed GTID set encoding.
• Error number: 1774; Symbol: ER_MALFORMED_GTID_SPECIFICATION; SQLSTATE: HY000
Message: Malformed GTID specification '%s'.
• Error number: 1775; Symbol: ER_GNO_EXHAUSTED; SQLSTATE: HY000
Message: Impossible to generate GTID: the integer component reached the maximum value. Restart the
server with a new server_uuid.
• Error number: 1776; Symbol: ER_BAD_SLAVE_AUTO_POSITION; SQLSTATE: HY000
Message: Parameters MASTER_LOG_FILE, MASTER_LOG_POS, RELAY_LOG_FILE and
RELAY_LOG_POS cannot be set when MASTER_AUTO_POSITION is active.
• Error number: 1777; Symbol: ER_AUTO_POSITION_REQUIRES_GTID_MODE_ON; SQLSTATE: HY000
Message: CHANGE MASTER TO MASTER_AUTO_POSITION = 1 can only be executed when
@@GLOBAL.GTID_MODE = ON.
ER_AUTO_POSITION_REQUIRES_GTID_MODE_ON was removed after 5.7.5.
• Error number: 1777; Symbol: ER_AUTO_POSITION_REQUIRES_GTID_MODE_NOT_OFF; SQLSTATE:
HY000
Message: CHANGE MASTER TO MASTER_AUTO_POSITION = 1 cannot be executed because
@@GLOBAL.GTID_MODE = OFF.
59
ER_AUTO_POSITION_REQUIRES_GTID_MODE_NOT_OFF was added in 5.7.6.
• Error number: 1778; Symbol:
ER_CANT_DO_IMPLICIT_COMMIT_IN_TRX_WHEN_GTID_NEXT_IS_SET; SQLSTATE: HY000
Message: Cannot execute statements with implicit commit inside a transaction when
@@SESSION.GTID_NEXT == 'UUID:NUMBER'.
• Error number: 1779; Symbol:
ER_GTID_MODE_2_OR_3_REQUIRES_ENFORCE_GTID_CONSISTENCY_ON; SQLSTATE: HY000
Message: @@GLOBAL.GTID_MODE = ON or UPGRADE_STEP_2 requires
@@GLOBAL.ENFORCE_GTID_CONSISTENCY = 1.
ER_GTID_MODE_2_OR_3_REQUIRES_ENFORCE_GTID_CONSISTENCY_ON was removed after 5.7.5.
• Error number: 1779; Symbol: ER_GTID_MODE_ON_REQUIRES_ENFORCE_GTID_CONSISTENCY_ON;
SQLSTATE: HY000
Message: GTID_MODE = ON requires ENFORCE_GTID_CONSISTENCY = ON.
ER_GTID_MODE_ON_REQUIRES_ENFORCE_GTID_CONSISTENCY_ON was added in 5.7.6.
• Error number: 1780; Symbol: ER_GTID_MODE_REQUIRES_BINLOG; SQLSTATE: HY000
Message: @@GLOBAL.GTID_MODE = ON or ON_PERMISSIVE or OFF_PERMISSIVE requires --logbin and --log-slave-updates.
• Error number: 1781; Symbol: ER_CANT_SET_GTID_NEXT_TO_GTID_WHEN_GTID_MODE_IS_OFF;
SQLSTATE: HY000
Message: @@SESSION.GTID_NEXT cannot be set to UUID:NUMBER when
@@GLOBAL.GTID_MODE = OFF.
• Error number: 1782; Symbol:
ER_CANT_SET_GTID_NEXT_TO_ANONYMOUS_WHEN_GTID_MODE_IS_ON; SQLSTATE: HY000
Message: @@SESSION.GTID_NEXT cannot be set to ANONYMOUS when
@@GLOBAL.GTID_MODE = ON.
• Error number: 1783; Symbol:
ER_CANT_SET_GTID_NEXT_LIST_TO_NON_NULL_WHEN_GTID_MODE_IS_OFF; SQLSTATE: HY000
Message: @@SESSION.GTID_NEXT_LIST cannot be set to a non-NULL value when
@@GLOBAL.GTID_MODE = OFF.
• Error number: 1784; Symbol: ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF; SQLSTATE:
HY000
Message: Found a Gtid_log_event or Previous_gtids_log_event when @@GLOBAL.GTID_MODE =
OFF.
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF was removed after 5.7.5.
• Error number: 1784; Symbol: ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF__UNUSED;
SQLSTATE: HY000
Message: Found a Gtid_log_event when @@GLOBAL.GTID_MODE = OFF.
60
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF__UNUSED was added in 5.7.6.
• Error number: 1785; Symbol: ER_GTID_UNSAFE_NON_TRANSACTIONAL_TABLE; SQLSTATE: HY000
Message: Statement violates GTID consistency: Updates to non-transactional tables can only be done in
either autocommitted statements or single-statement transactions, and never in the same statement as
updates to transactional tables.
• Error number: 1786; Symbol: ER_GTID_UNSAFE_CREATE_SELECT; SQLSTATE: HY000
Message: Statement violates GTID consistency: CREATE TABLE ... SELECT.
• Error number: 1787; Symbol:
ER_GTID_UNSAFE_CREATE_DROP_TEMPORARY_TABLE_IN_TRANSACTION; SQLSTATE: HY000
Message: Statement violates GTID consistency: CREATE TEMPORARY TABLE and DROP
TEMPORARY TABLE can only be executed outside transactional context. These statements are also
not allowed in a function or trigger because functions and triggers are also considered to be multistatement transactions.
• Error number: 1788; Symbol: ER_GTID_MODE_CAN_ONLY_CHANGE_ONE_STEP_AT_A_TIME;
SQLSTATE: HY000
Message: The value of @@GLOBAL.GTID_MODE can only be changed one step at a time: OFF <-
> OFF_PERMISSIVE <-> ON_PERMISSIVE <-> ON. Also note that this value must be stepped up or
down simultaneously on all servers. See the Manual for instructions.
• Error number: 1789; Symbol: ER_MASTER_HAS_PURGED_REQUIRED_GTIDS; SQLSTATE: HY000
Message: The slave is connecting using CHANGE MASTER TO MASTER_AUTO_POSITION = 1,
but the master has purged binary logs containing GTIDs that the slave requires. Replicate the missing
transactions from elsewhere, or provision a new slave from backup. Consider increasing the master's
binary log expiration period. %s.
• Error number: 1790; Symbol: ER_CANT_SET_GTID_NEXT_WHEN_OWNING_GTID; SQLSTATE: HY000
Message: @@SESSION.GTID_NEXT cannot be changed by a client that owns a GTID. The client owns
%s. Ownership is released on COMMIT or ROLLBACK.
• Error number: 1791; Symbol: ER_UNKNOWN_EXPLAIN_FORMAT; SQLSTATE: HY000
Message: Unknown EXPLAIN format name: '%s'
• Error number: 1792; Symbol: ER_CANT_EXECUTE_IN_READ_ONLY_TRANSACTION; SQLSTATE:
25006
Message: Cannot execute statement in a READ ONLY transaction.
• Error number: 1793; Symbol: ER_TOO_LONG_TABLE_PARTITION_COMMENT; SQLSTATE: HY000
Message: Comment for table partition '%s' is too long (max = %lu)
• Error number: 1794; Symbol: ER_SLAVE_CONFIGURATION; SQLSTATE: HY000
Message: Slave is not configured or failed to initialize properly. You must at least set --server-id to
enable either a master or a slave. Additional error messages can be found in the MySQL error log.
• Error number: 1795; Symbol: ER_INNODB_FT_LIMIT; SQLSTATE: HY000
61
Message: InnoDB presently supports one FULLTEXT index creation at a time
• Error number: 1796; Symbol: ER_INNODB_NO_FT_TEMP_TABLE; SQLSTATE: HY000
Message: Cannot create FULLTEXT index on temporary InnoDB table
• Error number: 1797; Symbol: ER_INNODB_FT_WRONG_DOCID_COLUMN; SQLSTATE: HY000
Message: Column '%s' is of wrong type for an InnoDB FULLTEXT index
• Error number: 1798; Symbol: ER_INNODB_FT_WRONG_DOCID_INDEX; SQLSTATE: HY000
Message: Index '%s' is of wrong type for an InnoDB FULLTEXT index
• Error number: 1799; Symbol: ER_INNODB_ONLINE_LOG_TOO_BIG; SQLSTATE: HY000
Message: Creating index '%s' required more than 'innodb_online_alter_log_max_size' bytes of
modification log. Please try again.
• Error number: 1800; Symbol: ER_UNKNOWN_ALTER_ALGORITHM; SQLSTATE: HY000
Message: Unknown ALGORITHM '%s'
• Error number: 1801; Symbol: ER_UNKNOWN_ALTER_LOCK; SQLSTATE: HY000
Message: Unknown LOCK type '%s'
• Error number: 1802; Symbol: ER_MTS_CHANGE_MASTER_CANT_RUN_WITH_GAPS; SQLSTATE: HY000
Message: CHANGE MASTER cannot be executed when the slave was stopped with an error or killed in
MTS mode. Consider using RESET SLAVE or START SLAVE UNTIL.
• Error number: 1803; Symbol: ER_MTS_RECOVERY_FAILURE; SQLSTATE: HY000
Message: Cannot recover after SLAVE errored out in parallel execution mode. Additional error
messages can be found in the MySQL error log.
• Error number: 1804; Symbol: ER_MTS_RESET_WORKERS; SQLSTATE: HY000
Message: Cannot clean up worker info tables. Additional error messages can be found in the MySQL
error log.
• Error number: 1805; Symbol: ER_COL_COUNT_DOESNT_MATCH_CORRUPTED_V2; SQLSTATE: HY000
Message: Column count of %s.%s is wrong. Expected %d, found %d. The table is probably corrupted
• Error number: 1806; Symbol: ER_SLAVE_SILENT_RETRY_TRANSACTION; SQLSTATE: HY000
Message: Slave must silently retry current transaction
• Error number: 1807; Symbol: ER_DISCARD_FK_CHECKS_RUNNING; SQLSTATE: HY000
Message: There is a foreign key check running on table '%s'. Cannot discard the table.
• Error number: 1808; Symbol: ER_TABLE_SCHEMA_MISMATCH; SQLSTATE: HY000
Message: Schema mismatch (%s)
• Error number: 1809; Symbol: ER_TABLE_IN_SYSTEM_TABLESPACE; SQLSTATE: HY000
62
Message: Table '%s' in system tablespace
• Error number: 1810; Symbol: ER_IO_READ_ERROR; SQLSTATE: HY000
Message: IO Read error: (%lu, %s) %s
• Error number: 1811; Symbol: ER_IO_WRITE_ERROR; SQLSTATE: HY000
Message: IO Write error: (%lu, %s) %s
• Error number: 1812; Symbol: ER_TABLESPACE_MISSING; SQLSTATE: HY000
Message: Tablespace is missing for table %s.
• Error number: 1813; Symbol: ER_TABLESPACE_EXISTS; SQLSTATE: HY000
Message: Tablespace '%s' exists.
• Error number: 1814; Symbol: ER_TABLESPACE_DISCARDED; SQLSTATE: HY000
Message: Tablespace has been discarded for table '%s'
• Error number: 1815; Symbol: ER_INTERNAL_ERROR; SQLSTATE: HY000
Message: Internal error: %s
• Error number: 1816; Symbol: ER_INNODB_IMPORT_ERROR; SQLSTATE: HY000
Message: ALTER TABLE %s IMPORT TABLESPACE failed with error %lu : '%s'
• Error number: 1817; Symbol: ER_INNODB_INDEX_CORRUPT; SQLSTATE: HY000
Message: Index corrupt: %s
• Error number: 1818; Symbol: ER_INVALID_YEAR_COLUMN_LENGTH; SQLSTATE: HY000
Message: Supports only YEAR or YEAR(4) column.
• Error number: 1819; Symbol: ER_NOT_VALID_PASSWORD; SQLSTATE: HY000
Message: Your password does not satisfy the current policy requirements
• Error number: 1820; Symbol: ER_MUST_CHANGE_PASSWORD; SQLSTATE: HY000
Message: You must reset your password using ALTER USER statement before executing this
statement.
• Error number: 1821; Symbol: ER_FK_NO_INDEX_CHILD; SQLSTATE: HY000
Message: Failed to add the foreign key constaint. Missing index for constraint '%s' in the foreign table
'%s'
• Error number: 1822; Symbol: ER_FK_NO_INDEX_PARENT; SQLSTATE: HY000
Message: Failed to add the foreign key constaint. Missing index for constraint '%s' in the referenced
table '%s'
• Error number: 1823; Symbol: ER_FK_FAIL_ADD_SYSTEM; SQLSTATE: HY000
63
Message: Failed to add the foreign key constraint '%s' to system tables
• Error number: 1824; Symbol: ER_FK_CANNOT_OPEN_PARENT; SQLSTATE: HY000
Message: Failed to open the referenced table '%s'
• Error number: 1825; Symbol: ER_FK_INCORRECT_OPTION; SQLSTATE: HY000
Message: Failed to add the foreign key constraint on table '%s'. Incorrect options in FOREIGN KEY
constraint '%s'
• Error number: 1826; Symbol: ER_FK_DUP_NAME; SQLSTATE: HY000
Message: Duplicate foreign key constraint name '%s'
• Error number: 1827; Symbol: ER_PASSWORD_FORMAT; SQLSTATE: HY000
Message: The password hash doesn't have the expected format. Check if the correct password
algorithm is being used with the PASSWORD() function.
• Error number: 1828; Symbol: ER_FK_COLUMN_CANNOT_DROP; SQLSTATE: HY000
Message: Cannot drop column '%s': needed in a foreign key constraint '%s'
• Error number: 1829; Symbol: ER_FK_COLUMN_CANNOT_DROP_CHILD; SQLSTATE: HY000
Message: Cannot drop column '%s': needed in a foreign key constraint '%s' of table '%s'
• Error number: 1830; Symbol: ER_FK_COLUMN_NOT_NULL; SQLSTATE: HY000
Message: Column '%s' cannot be NOT NULL: needed in a foreign key constraint '%s' SET NULL
• Error number: 1831; Symbol: ER_DUP_INDEX; SQLSTATE: HY000
Message: Duplicate index '%s' defined on the table '%s.%s'. This is deprecated and will be disallowed in
a future release.
• Error number: 1832; Symbol: ER_FK_COLUMN_CANNOT_CHANGE; SQLSTATE: HY000
Message: Cannot change column '%s': used in a foreign key constraint '%s'
• Error number: 1833; Symbol: ER_FK_COLUMN_CANNOT_CHANGE_CHILD; SQLSTATE: HY000
Message: Cannot change column '%s': used in a foreign key constraint '%s' of table '%s'
• Error number: 1834; Symbol: ER_FK_CANNOT_DELETE_PARENT; SQLSTATE: HY000
Message: Cannot delete rows from table which is parent in a foreign key constraint '%s' of table '%s'
ER_FK_CANNOT_DELETE_PARENT was removed after 5.7.3.
• Error number: 1834; Symbol: ER_UNUSED5; SQLSTATE: HY000
Message: Cannot delete rows from table which is parent in a foreign key constraint '%s' of table '%s'
ER_UNUSED5 was added in 5.7.4.
• Error number: 1835; Symbol: ER_MALFORMED_PACKET; SQLSTATE: HY000
Message: Malformed communication packet.
64
• Error number: 1836; Symbol: ER_READ_ONLY_MODE; SQLSTATE: HY000
Message: Running in read-only mode
• Error number: 1837; Symbol: ER_GTID_NEXT_TYPE_UNDEFINED_GROUP; SQLSTATE: HY000
Message: When @@SESSION.GTID_NEXT is set to a GTID, you must explicitly set it to a different
value after a COMMIT or ROLLBACK. Please check GTID_NEXT variable manual page for detailed
explanation. Current @@SESSION.GTID_NEXT is '%s'.
• Error number: 1838; Symbol: ER_VARIABLE_NOT_SETTABLE_IN_SP; SQLSTATE: HY000
Message: The system variable %s cannot be set in stored procedures.
• Error number: 1839; Symbol: ER_CANT_SET_GTID_PURGED_WHEN_GTID_MODE_IS_OFF; SQLSTATE:
HY000
Message: @@GLOBAL.GTID_PURGED can only be set when @@GLOBAL.GTID_MODE = ON.
• Error number: 1840; Symbol:
ER_CANT_SET_GTID_PURGED_WHEN_GTID_EXECUTED_IS_NOT_EMPTY; SQLSTATE: HY000
Message: @@GLOBAL.GTID_PURGED can only be set when @@GLOBAL.GTID_EXECUTED is
empty.
• Error number: 1841; Symbol: ER_CANT_SET_GTID_PURGED_WHEN_OWNED_GTIDS_IS_NOT_EMPTY;
SQLSTATE: HY000
Message: @@GLOBAL.GTID_PURGED can only be set when there are no ongoing transactions (not
even in other clients).
• Error number: 1842; Symbol: ER_GTID_PURGED_WAS_CHANGED; SQLSTATE: HY000
Message: @@GLOBAL.GTID_PURGED was changed from '%s' to '%s'.
• Error number: 1843; Symbol: ER_GTID_EXECUTED_WAS_CHANGED; SQLSTATE: HY000
Message: @@GLOBAL.GTID_EXECUTED was changed from '%s' to '%s'.
• Error number: 1844; Symbol: ER_BINLOG_STMT_MODE_AND_NO_REPL_TABLES; SQLSTATE: HY000
Message: Cannot execute statement: impossible to write to binary log since BINLOG_FORMAT =
STATEMENT, and both replicated and non replicated tables are written to.
• Error number: 1845; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED; SQLSTATE: 0A000
Message: %s is not supported for this operation. Try %s.
ER_ALTER_OPERATION_NOT_SUPPORTED was added in 5.7.1.
• Error number: 1846; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON; SQLSTATE: 0A000
Message: %s is not supported. Reason: %s. Try %s.
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON was added in 5.7.1.
• Error number: 1847; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COPY; SQLSTATE:
HY000
65
Message: COPY algorithm requires a lock
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COPY was added in 5.7.1.
• Error number: 1848; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_PARTITION;
SQLSTATE: HY000
Message: Partition specific operations do not yet support LOCK/ALGORITHM
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_PARTITION was added in 5.7.1.
• Error number: 1849; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_RENAME;
SQLSTATE: HY000
Message: Columns participating in a foreign key are renamed
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_RENAME was added in 5.7.1.
• Error number: 1850; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COLUMN_TYPE;
SQLSTATE: HY000
Message: Cannot change column type INPLACE
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COLUMN_TYPE was added in 5.7.1.
• Error number: 1851; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_CHECK;
SQLSTATE: HY000
Message: Adding foreign keys needs foreign_key_checks=OFF
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_CHECK was added in 5.7.1.
• Error number: 1852; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_IGNORE;
SQLSTATE: HY000
Message: Creating unique indexes with IGNORE requires COPY algorithm to remove duplicate rows
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_IGNORE was added in 5.7.1, removed after 5.7.3.
• Error number: 1852; Symbol: ER_UNUSED6; SQLSTATE: HY000
Message: Creating unique indexes with IGNORE requires COPY algorithm to remove duplicate rows
ER_UNUSED6 was added in 5.7.4.
• Error number: 1853; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOPK; SQLSTATE:
HY000
Message: Dropping a primary key is not allowed without also adding a new primary key
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOPK was added in 5.7.1.
• Error number: 1854; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_AUTOINC;
SQLSTATE: HY000
Message: Adding an auto-increment column requires a lock
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_AUTOINC was added in 5.7.1.
66
• Error number: 1855; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_HIDDEN_FTS;
SQLSTATE: HY000
Message: Cannot replace hidden FTS_DOC_ID with a user-visible one
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_HIDDEN_FTS was added in 5.7.1.
• Error number: 1856; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_CHANGE_FTS;
SQLSTATE: HY000
Message: Cannot drop or rename FTS_DOC_ID
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_CHANGE_FTS was added in 5.7.1.
• Error number: 1857; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FTS; SQLSTATE:
HY000
Message: Fulltext index creation requires a lock
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FTS was added in 5.7.1.
• Error number: 1858; Symbol: ER_SQL_SLAVE_SKIP_COUNTER_NOT_SETTABLE_IN_GTID_MODE;
SQLSTATE: HY000
Message: sql_slave_skip_counter can not be set when the server is running with
@@GLOBAL.GTID_MODE = ON. Instead, for each transaction that you want to skip, generate an empty
transaction with the same GTID as the transaction
ER_SQL_SLAVE_SKIP_COUNTER_NOT_SETTABLE_IN_GTID_MODE was added in 5.7.1.
• Error number: 1859; Symbol: ER_DUP_UNKNOWN_IN_INDEX; SQLSTATE: 23000
Message: Duplicate entry for key '%s'
ER_DUP_UNKNOWN_IN_INDEX was added in 5.7.1.
• Error number: 1860; Symbol: ER_IDENT_CAUSES_TOO_LONG_PATH; SQLSTATE: HY000
Message: Long database name and identifier for object resulted in path length exceeding %d characters.
Path: '%s'.
ER_IDENT_CAUSES_TOO_LONG_PATH was added in 5.7.1.
• Error number: 1861; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOT_NULL;
SQLSTATE: HY000
Message: cannot silently convert NULL values, as required in this SQL_MODE
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOT_NULL was added in 5.7.1.
• Error number: 1862; Symbol: ER_MUST_CHANGE_PASSWORD_LOGIN; SQLSTATE: HY000
Message: Your password has expired. To log in you must change it using a client that supports expired
passwords.
ER_MUST_CHANGE_PASSWORD_LOGIN was added in 5.7.1.
• Error number: 1863; Symbol: ER_ROW_IN_WRONG_PARTITION; SQLSTATE: HY000
67
Message: Found a row in wrong partition %s
ER_ROW_IN_WRONG_PARTITION was added in 5.7.1.
• Error number: 1864; Symbol: ER_MTS_EVENT_BIGGER_PENDING_JOBS_SIZE_MAX; SQLSTATE:
HY000
Message: Cannot schedule event %s, relay-log name %s, position %s to Worker thread because its size
%lu exceeds %lu of slave_pending_jobs_size_max.
ER_MTS_EVENT_BIGGER_PENDING_JOBS_SIZE_MAX was added in 5.7.2.
• Error number: 1865; Symbol: ER_INNODB_NO_FT_USES_PARSER; SQLSTATE: HY000
Message: Cannot CREATE FULLTEXT INDEX WITH PARSER on InnoDB table
ER_INNODB_NO_FT_USES_PARSER was added in 5.7.2.
• Error number: 1866; Symbol: ER_BINLOG_LOGICAL_CORRUPTION; SQLSTATE: HY000
Message: The binary log file '%s' is logically corrupted: %s
ER_BINLOG_LOGICAL_CORRUPTION was added in 5.7.2.
• Error number: 1867; Symbol: ER_WARN_PURGE_LOG_IN_USE; SQLSTATE: HY000
Message: file %s was not purged because it was being read by %d thread(s), purged only %d out of %d
files.
ER_WARN_PURGE_LOG_IN_USE was added in 5.7.2.
• Error number: 1868; Symbol: ER_WARN_PURGE_LOG_IS_ACTIVE; SQLSTATE: HY000
Message: file %s was not purged because it is the active log file.
ER_WARN_PURGE_LOG_IS_ACTIVE was added in 5.7.2.
• Error number: 1869; Symbol: ER_AUTO_INCREMENT_CONFLICT; SQLSTATE: HY000
Message: Auto-increment value in UPDATE conflicts with internally generated values
ER_AUTO_INCREMENT_CONFLICT was added in 5.7.2.
• Error number: 1870; Symbol: WARN_ON_BLOCKHOLE_IN_RBR; SQLSTATE: HY000
Message: Row events are not logged for %s statements that modify BLACKHOLE tables in row format.
Table(s): '%s'
WARN_ON_BLOCKHOLE_IN_RBR was added in 5.7.2.
• Error number: 1871; Symbol: ER_SLAVE_MI_INIT_REPOSITORY; SQLSTATE: HY000
Message: Slave failed to initialize master info structure from the repository
ER_SLAVE_MI_INIT_REPOSITORY was added in 5.7.2.
• Error number: 1872; Symbol: ER_SLAVE_RLI_INIT_REPOSITORY; SQLSTATE: HY000
Message: Slave failed to initialize relay log info structure from the repository
68
ER_SLAVE_RLI_INIT_REPOSITORY was added in 5.7.2.
• Error number: 1873; Symbol: ER_ACCESS_DENIED_CHANGE_USER_ERROR; SQLSTATE: 28000
Message: Access denied trying to change to user '%s'@'%s' (using password: %s). Disconnecting.
ER_ACCESS_DENIED_CHANGE_USER_ERROR was added in 5.7.2.
• Error number: 1874; Symbol: ER_INNODB_READ_ONLY; SQLSTATE: HY000
Message: InnoDB is in read only mode.
ER_INNODB_READ_ONLY was added in 5.7.2.
• Error number: 1875; Symbol: ER_STOP_SLAVE_SQL_THREAD_TIMEOUT; SQLSTATE: HY000
Message: STOP SLAVE command execution is incomplete: Slave SQL thread got the stop signal,
thread is busy, SQL thread will stop once the current task is complete.
ER_STOP_SLAVE_SQL_THREAD_TIMEOUT was added in 5.7.2.
• Error number: 1876; Symbol: ER_STOP_SLAVE_IO_THREAD_TIMEOUT; SQLSTATE: HY000
Message: STOP SLAVE command execution is incomplete: Slave IO thread got the stop signal, thread
is busy, IO thread will stop once the current task is complete.
ER_STOP_SLAVE_IO_THREAD_TIMEOUT was added in 5.7.2.
• Error number: 1877; Symbol: ER_TABLE_CORRUPT; SQLSTATE: HY000
Message: Operation cannot be performed. The table '%s.%s' is missing, corrupt or contains bad data.
ER_TABLE_CORRUPT was added in 5.7.2.
• Error number: 1878; Symbol: ER_TEMP_FILE_WRITE_FAILURE; SQLSTATE: HY000
Message: Temporary file write failure.
ER_TEMP_FILE_WRITE_FAILURE was added in 5.7.3.
• Error number: 1879; Symbol: ER_INNODB_FT_AUX_NOT_HEX_ID; SQLSTATE: HY000
Message: Upgrade index name failed, please use create index(alter table) algorithm copy to rebuild
index.
ER_INNODB_FT_AUX_NOT_HEX_ID was added in 5.7.4.
• Error number: 1880; Symbol: ER_OLD_TEMPORALS_UPGRADED; SQLSTATE: HY000
Message: TIME/TIMESTAMP/DATETIME columns of old format have been upgraded to the new format.
ER_OLD_TEMPORALS_UPGRADED was added in 5.7.4.
• Error number: 1881; Symbol: ER_INNODB_FORCED_RECOVERY; SQLSTATE: HY000
Message: Operation not allowed when innodb_forced_recovery > 0.
ER_INNODB_FORCED_RECOVERY was added in 5.7.4.
69
• Error number: 1882; Symbol: ER_AES_INVALID_IV; SQLSTATE: HY000
Message: The initialization vector supplied to %s is too short. Must be at least %d bytes long
ER_AES_INVALID_IV was added in 5.7.4.
• Error number: 1883; Symbol: ER_PLUGIN_CANNOT_BE_UNINSTALLED; SQLSTATE: HY000
Message: Plugin '%s' cannot be uninstalled now. %s
ER_PLUGIN_CANNOT_BE_UNINSTALLED was added in 5.7.5.
• Error number: 1884; Symbol:
ER_GTID_UNSAFE_BINLOG_SPLITTABLE_STATEMENT_AND_GTID_GROUP; SQLSTATE: HY000
Message: Cannot execute statement because it needs to be written to the binary log as multiple
statements, and this is not allowed when @@SESSION.GTID_NEXT == 'UUID:NUMBER'.
ER_GTID_UNSAFE_BINLOG_SPLITTABLE_STATEMENT_AND_GTID_GROUP was added in 5.7.5.
• Error number: 1885; Symbol: ER_SLAVE_HAS_MORE_GTIDS_THAN_MASTER; SQLSTATE: HY000
Message: Slave has more GTIDs than the master has, using the master's SERVER_UUID. This may
indicate that the end of the binary log was truncated or that the last binary log file was lost, e.g., after a
power or disk failure when sync_binlog != 1. The master may or may not have rolled back transactions
that were already replicated to the slave. Suggest to replicate any transactions that master has rolled
back from slave to master, and/or commit empty transactions on master to account for transactions that
have been committed on master but are not included in GTID_EXECUTED.
ER_SLAVE_HAS_MORE_GTIDS_THAN_MASTER was added in 5.7.6.
• Error number: 1886; Symbol: ER_MISSING_KEY; SQLSTATE: HY000
Message: The table '%s.%s' does not have the necessary key(s) defined on it. Please check the table
definition and create index(s) accordingly.
ER_MISSING_KEY was added in 5.7.22.
• Error number: 1887; Symbol: WARN_NAMED_PIPE_ACCESS_EVERYONE; SQLSTATE: HY000
Message: Setting named_pipe_full_access_group='%s' is insecure. Consider using a Windows group
with fewer members.
WARN_NAMED_PIPE_ACCESS_EVERYONE was added in 5.7.27.
• Error number: 1888; Symbol: ER_FOUND_MISSING_GTIDS; SQLSTATE: HY000
Message: Cannot replicate to server with server_uuid='%s' because the present server has purged
required binary logs. The connecting server needs to replicate the missing transactions from elsewhere,
or be replaced by a new server created from a more recent backup. To prevent this error in the future,
consider increasing the binary log expiration period on the present server. %s.
ER_FOUND_MISSING_GTIDS was added in 5.7.29.
• Error number: 1906; Symbol: ER_SLAVE_IO_THREAD_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with a running slave io thread; run STOP SLAVE
IO_THREAD first.
70
ER_SLAVE_IO_THREAD_MUST_STOP was added in 5.7.4, removed after 5.7.5.
• Error number: 3000; Symbol: ER_FILE_CORRUPT; SQLSTATE: HY000
Message: File %s is corrupted
• Error number: 3001; Symbol: ER_ERROR_ON_MASTER; SQLSTATE: HY000
Message: Query partially completed on the master (error on master: %d) and was aborted. There is a
chance that your master is inconsistent at this point. If you are sure that your master is ok, run this query
manually on the slave and then restart the slave with SET GLOBAL SQL_SLAVE_SKIP_COUNTER=1;
START SLAVE;. Query:'%s'
• Error number: 3002; Symbol: ER_INCONSISTENT_ERROR; SQLSTATE: HY000
Message: Query caused different errors on master and slave. Error on master: message (format)='%s'
error code=%d; Error on slave:actual message='%s', error code=%d. Default database:'%s'. Query:'%s'
• Error number: 3003; Symbol: ER_STORAGE_ENGINE_NOT_LOADED; SQLSTATE: HY000
Message: Storage engine for table '%s'.'%s' is not loaded.
• Error number: 3004; Symbol: ER_GET_STACKED_DA_WITHOUT_ACTIVE_HANDLER; SQLSTATE:
0Z002
Message: GET STACKED DIAGNOSTICS when handler not active
• Error number: 3005; Symbol: ER_WARN_LEGACY_SYNTAX_CONVERTED; SQLSTATE: HY000
Message: %s is no longer supported. The statement was converted to %s.
• Error number: 3006; Symbol: ER_BINLOG_UNSAFE_FULLTEXT_PLUGIN; SQLSTATE: HY000
Message: Statement is unsafe because it uses a fulltext parser plugin which may not return the same
value on the slave.
ER_BINLOG_UNSAFE_FULLTEXT_PLUGIN was added in 5.7.1.
• Error number: 3007; Symbol: ER_CANNOT_DISCARD_TEMPORARY_TABLE; SQLSTATE: HY000
Message: Cannot DISCARD/IMPORT tablespace associated with temporary table
ER_CANNOT_DISCARD_TEMPORARY_TABLE was added in 5.7.1.
• Error number: 3008; Symbol: ER_FK_DEPTH_EXCEEDED; SQLSTATE: HY000
Message: Foreign key cascade delete/update exceeds max depth of %d.
ER_FK_DEPTH_EXCEEDED was added in 5.7.2.
• Error number: 3009; Symbol: ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE_V2; SQLSTATE:
HY000
Message: Column count of %s.%s is wrong. Expected %d, found %d. Created with MySQL %d, now
running %d. Please use mysql_upgrade to fix this error.
ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE_V2 was added in 5.7.2.
• Error number: 3010; Symbol: ER_WARN_TRIGGER_DOESNT_HAVE_CREATED; SQLSTATE: HY000
71
Message: Trigger %s.%s.%s does not have CREATED attribute.
ER_WARN_TRIGGER_DOESNT_HAVE_CREATED was added in 5.7.2.
• Error number: 3011; Symbol: ER_REFERENCED_TRG_DOES_NOT_EXIST; SQLSTATE: HY000
Message: Referenced trigger '%s' for the given action time and event type does not exist.
ER_REFERENCED_TRG_DOES_NOT_EXIST was added in 5.7.2.
• Error number: 3012; Symbol: ER_EXPLAIN_NOT_SUPPORTED; SQLSTATE: HY000
Message: EXPLAIN FOR CONNECTION command is supported only for SELECT/UPDATE/INSERT/
DELETE/REPLACE
ER_EXPLAIN_NOT_SUPPORTED was added in 5.7.2.
• Error number: 3013; Symbol: ER_INVALID_FIELD_SIZE; SQLSTATE: HY000
Message: Invalid size for column '%s'.
ER_INVALID_FIELD_SIZE was added in 5.7.2.
• Error number: 3014; Symbol: ER_MISSING_HA_CREATE_OPTION; SQLSTATE: HY000
Message: Table storage engine '%s' found required create option missing
ER_MISSING_HA_CREATE_OPTION was added in 5.7.2.
• Error number: 3015; Symbol: ER_ENGINE_OUT_OF_MEMORY; SQLSTATE: HY000
Message: Out of memory in storage engine '%s'.
ER_ENGINE_OUT_OF_MEMORY was added in 5.7.3.
• Error number: 3016; Symbol: ER_PASSWORD_EXPIRE_ANONYMOUS_USER; SQLSTATE: HY000
Message: The password for anonymous user cannot be expired.
ER_PASSWORD_EXPIRE_ANONYMOUS_USER was added in 5.7.3.
• Error number: 3017; Symbol: ER_SLAVE_SQL_THREAD_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with a running slave sql thread; run STOP SLAVE
SQL_THREAD first
ER_SLAVE_SQL_THREAD_MUST_STOP was added in 5.7.3.
• Error number: 3018; Symbol: ER_NO_FT_MATERIALIZED_SUBQUERY; SQLSTATE: HY000
Message: Cannot create FULLTEXT index on materialized subquery
ER_NO_FT_MATERIALIZED_SUBQUERY was added in 5.7.4.
• Error number: 3019; Symbol: ER_INNODB_UNDO_LOG_FULL; SQLSTATE: HY000
Message: Undo Log error: %s
ER_INNODB_UNDO_LOG_FULL was added in 5.7.4.
72
• Error number: 3020; Symbol: ER_INVALID_ARGUMENT_FOR_LOGARITHM; SQLSTATE: 2201E
Message: Invalid argument for logarithm
ER_INVALID_ARGUMENT_FOR_LOGARITHM was added in 5.7.4.
• Error number: 3021; Symbol: ER_SLAVE_CHANNEL_IO_THREAD_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with a running slave io thread; run STOP SLAVE
IO_THREAD FOR CHANNEL '%s' first.
ER_SLAVE_CHANNEL_IO_THREAD_MUST_STOP was added in 5.7.6.
• Error number: 3022; Symbol: ER_WARN_OPEN_TEMP_TABLES_MUST_BE_ZERO; SQLSTATE: HY000
Message: This operation may not be safe when the slave has temporary tables. The tables will be kept
open until the server restarts or until the tables are deleted by any replicated DROP statement. Suggest
to wait until slave_open_temp_tables = 0.
ER_WARN_OPEN_TEMP_TABLES_MUST_BE_ZERO was added in 5.7.4.
• Error number: 3023; Symbol: ER_WARN_ONLY_MASTER_LOG_FILE_NO_POS; SQLSTATE: HY000
Message: CHANGE MASTER TO with a MASTER_LOG_FILE clause but no MASTER_LOG_POS
clause may not be safe. The old position value may not be valid for the new binary log file.
ER_WARN_ONLY_MASTER_LOG_FILE_NO_POS was added in 5.7.4.
• Error number: 3024; Symbol: ER_QUERY_TIMEOUT; SQLSTATE: HY000
Message: Query execution was interrupted, maximum statement execution time exceeded
ER_QUERY_TIMEOUT was added in 5.7.4.
• Error number: 3025; Symbol: ER_NON_RO_SELECT_DISABLE_TIMER; SQLSTATE: HY000
Message: Select is not a read only statement, disabling timer
ER_NON_RO_SELECT_DISABLE_TIMER was added in 5.7.4.
• Error number: 3026; Symbol: ER_DUP_LIST_ENTRY; SQLSTATE: HY000
Message: Duplicate entry '%s'.
ER_DUP_LIST_ENTRY was added in 5.7.4.
• Error number: 3027; Symbol: ER_SQL_MODE_NO_EFFECT; SQLSTATE: HY000
Message: '%s' mode no longer has any effect. Use STRICT_ALL_TABLES or
STRICT_TRANS_TABLES instead.
ER_SQL_MODE_NO_EFFECT was added in 5.7.4.
• Error number: 3028; Symbol: ER_AGGREGATE_ORDER_FOR_UNION; SQLSTATE: HY000
Message: Expression #%u of ORDER BY contains aggregate function and applies to a UNION
ER_AGGREGATE_ORDER_FOR_UNION was added in 5.7.5.
• Error number: 3029; Symbol: ER_AGGREGATE_ORDER_NON_AGG_QUERY; SQLSTATE: HY000
73
Message: Expression #%u of ORDER BY contains aggregate function and applies to the result of a nonaggregated query
ER_AGGREGATE_ORDER_NON_AGG_QUERY was added in 5.7.5.
• Error number: 3030; Symbol: ER_SLAVE_WORKER_STOPPED_PREVIOUS_THD_ERROR; SQLSTATE:
HY000
Message: Slave worker has stopped after at least one previous worker encountered an error when
slave-preserve-commit-order was enabled. To preserve commit order, the last transaction executed by
this thread has not been committed. When restarting the slave after fixing any failed threads, you should
fix this worker as well.
ER_SLAVE_WORKER_STOPPED_PREVIOUS_THD_ERROR was added in 5.7.5.
• Error number: 3031; Symbol: ER_DONT_SUPPORT_SLAVE_PRESERVE_COMMIT_ORDER; SQLSTATE:
HY000
Message: slave_preserve_commit_order is not supported %s.
ER_DONT_SUPPORT_SLAVE_PRESERVE_COMMIT_ORDER was added in 5.7.5.
• Error number: 3032; Symbol: ER_SERVER_OFFLINE_MODE; SQLSTATE: HY000
Message: The server is currently in offline mode
ER_SERVER_OFFLINE_MODE was added in 5.7.5.
• Error number: 3033; Symbol: ER_GIS_DIFFERENT_SRIDS; SQLSTATE: HY000
Message: Binary geometry function %s given two geometries of different srids: %u and %u, which
should have been identical.
Geometry values passed as arguments to spatial functions must have the same SRID value.
ER_GIS_DIFFERENT_SRIDS was added in 5.7.5.
• Error number: 3034; Symbol: ER_GIS_UNSUPPORTED_ARGUMENT; SQLSTATE: HY000
Message: Calling geometry function %s with unsupported types of arguments.
A spatial function was called with a combination of argument types that the function does not support.
ER_GIS_UNSUPPORTED_ARGUMENT was added in 5.7.5.
• Error number: 3035; Symbol: ER_GIS_UNKNOWN_ERROR; SQLSTATE: HY000
Message: Unknown GIS error occured in function %s.
ER_GIS_UNKNOWN_ERROR was added in 5.7.5.
• Error number: 3036; Symbol: ER_GIS_UNKNOWN_EXCEPTION; SQLSTATE: HY000
Message: Unknown exception caught in GIS function %s.
ER_GIS_UNKNOWN_EXCEPTION was added in 5.7.5.
• Error number: 3037; Symbol: ER_GIS_INVALID_DATA; SQLSTATE: 22023
74
Message: Invalid GIS data provided to function %s.
A spatial function was called with an argument not recognized as a valid geometry value.
ER_GIS_INVALID_DATA was added in 5.7.5.
• Error number: 3038; Symbol: ER_BOOST_GEOMETRY_EMPTY_INPUT_EXCEPTION; SQLSTATE: HY000
Message: The geometry has no data in function %s.
ER_BOOST_GEOMETRY_EMPTY_INPUT_EXCEPTION was added in 5.7.5.
• Error number: 3039; Symbol: ER_BOOST_GEOMETRY_CENTROID_EXCEPTION; SQLSTATE: HY000
Message: Unable to calculate centroid because geometry is empty in function %s.
ER_BOOST_GEOMETRY_CENTROID_EXCEPTION was added in 5.7.5.
• Error number: 3040; Symbol: ER_BOOST_GEOMETRY_OVERLAY_INVALID_INPUT_EXCEPTION;
SQLSTATE: HY000
Message: Geometry overlay calculation error: geometry data is invalid in function %s.
ER_BOOST_GEOMETRY_OVERLAY_INVALID_INPUT_EXCEPTION was added in 5.7.5.
• Error number: 3041; Symbol: ER_BOOST_GEOMETRY_TURN_INFO_EXCEPTION; SQLSTATE: HY000
Message: Geometry turn info calculation error: geometry data is invalid in function %s.
ER_BOOST_GEOMETRY_TURN_INFO_EXCEPTION was added in 5.7.5.
• Error number: 3042; Symbol: ER_BOOST_GEOMETRY_SELF_INTERSECTION_POINT_EXCEPTION;
SQLSTATE: HY000
Message: Analysis procedures of intersection points interrupted unexpectedly in function %s.
ER_BOOST_GEOMETRY_SELF_INTERSECTION_POINT_EXCEPTION was added in 5.7.5.
• Error number: 3043; Symbol: ER_BOOST_GEOMETRY_UNKNOWN_EXCEPTION; SQLSTATE: HY000
Message: Unknown exception thrown in function %s.
ER_BOOST_GEOMETRY_UNKNOWN_EXCEPTION was added in 5.7.5.
• Error number: 3044; Symbol: ER_STD_BAD_ALLOC_ERROR; SQLSTATE: HY000
Message: Memory allocation error: %s in function %s.
ER_STD_BAD_ALLOC_ERROR was added in 5.7.5.
• Error number: 3045; Symbol: ER_STD_DOMAIN_ERROR; SQLSTATE: HY000
Message: Domain error: %s in function %s.
ER_STD_DOMAIN_ERROR was added in 5.7.5.
• Error number: 3046; Symbol: ER_STD_LENGTH_ERROR; SQLSTATE: HY000
Message: Length error: %s in function %s.
75
ER_STD_LENGTH_ERROR was added in 5.7.5.
• Error number: 3047; Symbol: ER_STD_INVALID_ARGUMENT; SQLSTATE: HY000
Message: Invalid argument error: %s in function %s.
ER_STD_INVALID_ARGUMENT was added in 5.7.5.
• Error number: 3048; Symbol: ER_STD_OUT_OF_RANGE_ERROR; SQLSTATE: HY000
Message: Out of range error: %s in function %s.
ER_STD_OUT_OF_RANGE_ERROR was added in 5.7.5.
• Error number: 3049; Symbol: ER_STD_OVERFLOW_ERROR; SQLSTATE: HY000
Message: Overflow error error: %s in function %s.
ER_STD_OVERFLOW_ERROR was added in 5.7.5.
• Error number: 3050; Symbol: ER_STD_RANGE_ERROR; SQLSTATE: HY000
Message: Range error: %s in function %s.
ER_STD_RANGE_ERROR was added in 5.7.5.
• Error number: 3051; Symbol: ER_STD_UNDERFLOW_ERROR; SQLSTATE: HY000
Message: Underflow error: %s in function %s.
ER_STD_UNDERFLOW_ERROR was added in 5.7.5.
• Error number: 3052; Symbol: ER_STD_LOGIC_ERROR; SQLSTATE: HY000
Message: Logic error: %s in function %s.
ER_STD_LOGIC_ERROR was added in 5.7.5.
• Error number: 3053; Symbol: ER_STD_RUNTIME_ERROR; SQLSTATE: HY000
Message: Runtime error: %s in function %s.
ER_STD_RUNTIME_ERROR was added in 5.7.5.
• Error number: 3054; Symbol: ER_STD_UNKNOWN_EXCEPTION; SQLSTATE: HY000
Message: Unknown exception: %s in function %s.
ER_STD_UNKNOWN_EXCEPTION was added in 5.7.5.
• Error number: 3055; Symbol: ER_GIS_DATA_WRONG_ENDIANESS; SQLSTATE: HY000
Message: Geometry byte string must be little endian.
ER_GIS_DATA_WRONG_ENDIANESS was added in 5.7.5.
• Error number: 3056; Symbol: ER_CHANGE_MASTER_PASSWORD_LENGTH; SQLSTATE: HY000
Message: The password provided for the replication user exceeds the maximum length of 32 characters
76
ER_CHANGE_MASTER_PASSWORD_LENGTH was added in 5.7.5.
• Error number: 3057; Symbol: ER_USER_LOCK_WRONG_NAME; SQLSTATE: 42000
Message: Incorrect user-level lock name '%s'.
ER_USER_LOCK_WRONG_NAME was added in 5.7.5.
• Error number: 3058; Symbol: ER_USER_LOCK_DEADLOCK; SQLSTATE: HY000
Message: Deadlock found when trying to get user-level lock; try rolling back transaction/releasing locks
and restarting lock acquisition.
This error is returned when the metdata locking subsystem detects a deadlock for an attempt to acquire
a named lock with GET_LOCK.
ER_USER_LOCK_DEADLOCK was added in 5.7.5.
• Error number: 3059; Symbol: ER_REPLACE_INACCESSIBLE_ROWS; SQLSTATE: HY000
Message: REPLACE cannot be executed as it requires deleting rows that are not in the view
ER_REPLACE_INACCESSIBLE_ROWS was added in 5.7.5.
• Error number: 3060; Symbol: ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_GIS; SQLSTATE:
HY000
Message: Do not support online operation on table with GIS index
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_GIS was added in 5.7.5.
• Error number: 3061; Symbol: ER_ILLEGAL_USER_VAR; SQLSTATE: 42000
Message: User variable name '%s' is illegal
ER_ILLEGAL_USER_VAR was added in 5.7.5.
• Error number: 3062; Symbol: ER_GTID_MODE_OFF; SQLSTATE: HY000
Message: Cannot %s when GTID_MODE = OFF.
ER_GTID_MODE_OFF was added in 5.7.5.
• Error number: 3063; Symbol: ER_UNSUPPORTED_BY_REPLICATION_THREAD; SQLSTATE: HY000
Message: Cannot %s from a replication slave thread.
ER_UNSUPPORTED_BY_REPLICATION_THREAD was added in 5.7.5.
• Error number: 3064; Symbol: ER_INCORRECT_TYPE; SQLSTATE: HY000
Message: Incorrect type for argument %s in function %s.
ER_INCORRECT_TYPE was added in 5.7.5.
• Error number: 3065; Symbol: ER_FIELD_IN_ORDER_NOT_SELECT; SQLSTATE: HY000
Message: Expression #%u of ORDER BY clause is not in SELECT list, references column '%s' which is
not in SELECT list; this is incompatible with %s
77
ER_FIELD_IN_ORDER_NOT_SELECT was added in 5.7.5.
• Error number: 3066; Symbol: ER_AGGREGATE_IN_ORDER_NOT_SELECT; SQLSTATE: HY000
Message: Expression #%u of ORDER BY clause is not in SELECT list, contains aggregate function; this
is incompatible with %s
ER_AGGREGATE_IN_ORDER_NOT_SELECT was added in 5.7.5.
• Error number: 3067; Symbol: ER_INVALID_RPL_WILD_TABLE_FILTER_PATTERN; SQLSTATE:
HY000
Message: Supplied filter list contains a value which is not in the required format
'db_pattern.table_pattern'
ER_INVALID_RPL_WILD_TABLE_FILTER_PATTERN was added in 5.7.5.
• Error number: 3068; Symbol: ER_NET_OK_PACKET_TOO_LARGE; SQLSTATE: 08S01
Message: OK packet too large
ER_NET_OK_PACKET_TOO_LARGE was added in 5.7.5.
• Error number: 3069; Symbol: ER_INVALID_JSON_DATA; SQLSTATE: HY000
Message: Invalid JSON data provided to function %s: %s
ER_INVALID_JSON_DATA was added in 5.7.5.
• Error number: 3070; Symbol: ER_INVALID_GEOJSON_MISSING_MEMBER; SQLSTATE: HY000
Message: Invalid GeoJSON data provided to function %s: Missing required member '%s'
ER_INVALID_GEOJSON_MISSING_MEMBER was added in 5.7.5.
• Error number: 3071; Symbol: ER_INVALID_GEOJSON_WRONG_TYPE; SQLSTATE: HY000
Message: Invalid GeoJSON data provided to function %s: Member '%s' must be of type '%s'
ER_INVALID_GEOJSON_WRONG_TYPE was added in 5.7.5.
• Error number: 3072; Symbol: ER_INVALID_GEOJSON_UNSPECIFIED; SQLSTATE: HY000
Message: Invalid GeoJSON data provided to function %s
ER_INVALID_GEOJSON_UNSPECIFIED was added in 5.7.5.
• Error number: 3073; Symbol: ER_DIMENSION_UNSUPPORTED; SQLSTATE: HY000
Message: Unsupported number of coordinate dimensions in function %s: Found %u, expected %u
ER_DIMENSION_UNSUPPORTED was added in 5.7.5.
• Error number: 3074; Symbol: ER_SLAVE_CHANNEL_DOES_NOT_EXIST; SQLSTATE: HY000
Message: Slave channel '%s' does not exist.
ER_SLAVE_CHANNEL_DOES_NOT_EXIST was added in 5.7.6.
78
• Error number: 3075; Symbol: ER_SLAVE_MULTIPLE_CHANNELS_HOST_PORT; SQLSTATE: HY000
Message: A slave channel '%s' already exists for the given host and port combination.
ER_SLAVE_MULTIPLE_CHANNELS_HOST_PORT was added in 5.7.6.
• Error number: 3076; Symbol: ER_SLAVE_CHANNEL_NAME_INVALID_OR_TOO_LONG; SQLSTATE:
HY000
Message: Couldn't create channel: Channel name is either invalid or too long.
ER_SLAVE_CHANNEL_NAME_INVALID_OR_TOO_LONG was added in 5.7.6.
• Error number: 3077; Symbol: ER_SLAVE_NEW_CHANNEL_WRONG_REPOSITORY; SQLSTATE: HY000
Message: To have multiple channels, repository cannot be of type FILE; Please check the repository
configuration and convert them to TABLE.
ER_SLAVE_NEW_CHANNEL_WRONG_REPOSITORY was added in 5.7.6.
• Error number: 3078; Symbol: ER_SLAVE_CHANNEL_DELETE; SQLSTATE: HY000
Message: Cannot delete slave info objects for channel '%s'.
ER_SLAVE_CHANNEL_DELETE was added in 5.7.6.
• Error number: 3079; Symbol: ER_SLAVE_MULTIPLE_CHANNELS_CMD; SQLSTATE: HY000
Message: Multiple channels exist on the slave. Please provide channel name as an argument.
ER_SLAVE_MULTIPLE_CHANNELS_CMD was added in 5.7.6.
• Error number: 3080; Symbol: ER_SLAVE_MAX_CHANNELS_EXCEEDED; SQLSTATE: HY000
Message: Maximum number of replication channels allowed exceeded.
ER_SLAVE_MAX_CHANNELS_EXCEEDED was added in 5.7.6.
• Error number: 3081; Symbol: ER_SLAVE_CHANNEL_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with running replication threads; run STOP SLAVE FOR
CHANNEL '%s' first
ER_SLAVE_CHANNEL_MUST_STOP was added in 5.7.6.
• Error number: 3082; Symbol: ER_SLAVE_CHANNEL_NOT_RUNNING; SQLSTATE: HY000
Message: This operation requires running replication threads; configure slave and run START SLAVE
FOR CHANNEL '%s'
ER_SLAVE_CHANNEL_NOT_RUNNING was added in 5.7.6.
• Error number: 3083; Symbol: ER_SLAVE_CHANNEL_WAS_RUNNING; SQLSTATE: HY000
Message: Replication thread(s) for channel '%s' are already runnning.
ER_SLAVE_CHANNEL_WAS_RUNNING was added in 5.7.6.
• Error number: 3084; Symbol: ER_SLAVE_CHANNEL_WAS_NOT_RUNNING; SQLSTATE: HY000
79
Message: Replication thread(s) for channel '%s' are already stopped.
ER_SLAVE_CHANNEL_WAS_NOT_RUNNING was added in 5.7.6.
• Error number: 3085; Symbol: ER_SLAVE_CHANNEL_SQL_THREAD_MUST_STOP; SQLSTATE: HY000
Message: This operation cannot be performed with a running slave sql thread; run STOP SLAVE
SQL_THREAD FOR CHANNEL '%s' first.
ER_SLAVE_CHANNEL_SQL_THREAD_MUST_STOP was added in 5.7.6.
• Error number: 3086; Symbol: ER_SLAVE_CHANNEL_SQL_SKIP_COUNTER; SQLSTATE: HY000
Message: When sql_slave_skip_counter > 0, it is not allowed to start more than one SQL thread by
using 'START SLAVE [SQL_THREAD]'. Value of sql_slave_skip_counter can only be used by one SQL
thread at a time. Please use 'START SLAVE [SQL_THREAD] FOR CHANNEL' to start the SQL thread
which will use the value of sql_slave_skip_counter.
ER_SLAVE_CHANNEL_SQL_SKIP_COUNTER was added in 5.7.6.
• Error number: 3087; Symbol: ER_WRONG_FIELD_WITH_GROUP_V2; SQLSTATE: HY000
Message: Expression #%u of %s is not in GROUP BY clause and contains nonaggregated column
'%s' which is not functionally dependent on columns in GROUP BY clause; this is incompatible with
sql_mode=only_full_group_by
ER_WRONG_FIELD_WITH_GROUP_V2 was added in 5.7.6.
• Error number: 3088; Symbol: ER_MIX_OF_GROUP_FUNC_AND_FIELDS_V2; SQLSTATE: HY000
Message: In aggregated query without GROUP BY, expression #%u of %s contains nonaggregated
column '%s'; this is incompatible with sql_mode=only_full_group_by
ER_MIX_OF_GROUP_FUNC_AND_FIELDS_V2 was added in 5.7.6.
• Error number: 3089; Symbol: ER_WARN_DEPRECATED_SYSVAR_UPDATE; SQLSTATE: HY000
Message: Updating '%s' is deprecated. It will be made read-only in a future release.
ER_WARN_DEPRECATED_SYSVAR_UPDATE was added in 5.7.6.
• Error number: 3090; Symbol: ER_WARN_DEPRECATED_SQLMODE; SQLSTATE: HY000
Message: Changing sql mode '%s' is deprecated. It will be removed in a future release.
ER_WARN_DEPRECATED_SQLMODE was added in 5.7.6.
• Error number: 3091; Symbol: ER_CANNOT_LOG_PARTIAL_DROP_DATABASE_WITH_GTID; SQLSTATE:
HY000
Message: DROP DATABASE failed; some tables may have been dropped but the database directory
remains. The GTID has not been added to GTID_EXECUTED and the statement was not written
to the binary log. Fix this as follows: (1) remove all files from the database directory %s; (2) SET
GTID_NEXT='%s'; (3) DROP DATABASE `%s`.
ER_CANNOT_LOG_PARTIAL_DROP_DATABASE_WITH_GTID was added in 5.7.6.
• Error number: 3092; Symbol: ER_GROUP_REPLICATION_CONFIGURATION; SQLSTATE: HY000
80
Message: The server is not configured properly to be an active member of the group. Please see more
details on error log.
ER_GROUP_REPLICATION_CONFIGURATION was added in 5.7.6.
• Error number: 3093; Symbol: ER_GROUP_REPLICATION_RUNNING; SQLSTATE: HY000
Message: The START GROUP_REPLICATION command failed since the group is already running.
ER_GROUP_REPLICATION_RUNNING was added in 5.7.6.
• Error number: 3094; Symbol: ER_GROUP_REPLICATION_APPLIER_INIT_ERROR; SQLSTATE: HY000
Message: The START GROUP_REPLICATION command failed as the applier module failed to start.
ER_GROUP_REPLICATION_APPLIER_INIT_ERROR was added in 5.7.6.
• Error number: 3095; Symbol: ER_GROUP_REPLICATION_STOP_APPLIER_THREAD_TIMEOUT;
SQLSTATE: HY000
Message: The STOP GROUP_REPLICATION command execution is incomplete: The applier thread got
the stop signal while it was busy. The applier thread will stop once the current task is complete.
ER_GROUP_REPLICATION_STOP_APPLIER_THREAD_TIMEOUT was added in 5.7.6.
• Error number: 3096; Symbol: ER_GROUP_REPLICATION_COMMUNICATION_LAYER_SESSION_ERROR;
SQLSTATE: HY000
Message: The START GROUP_REPLICATION command failed as there was an error when initializing
the group communication layer.
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_SESSION_ERROR was added in 5.7.6.
• Error number: 3097; Symbol: ER_GROUP_REPLICATION_COMMUNICATION_LAYER_JOIN_ERROR;
SQLSTATE: HY000
Message: The START GROUP_REPLICATION command failed as there was an error when joining the
communication group.
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_JOIN_ERROR was added in 5.7.6.
• Error number: 3098; Symbol: ER_BEFORE_DML_VALIDATION_ERROR; SQLSTATE: HY000
Message: The table does not comply with the requirements by an external plugin.
ER_BEFORE_DML_VALIDATION_ERROR was added in 5.7.6.
• Error number: 3099; Symbol: ER_PREVENTS_VARIABLE_WITHOUT_RBR; SQLSTATE: HY000
Message: Cannot change the value of variable %s without binary log format as ROW.
From 5.7.6: transaction_write_set_extraction option value is set and binlog_format is not
ROW.
ER_PREVENTS_VARIABLE_WITHOUT_RBR was added in 5.7.6.
81
• Error number: 3100; Symbol: ER_RUN_HOOK_ERROR; SQLSTATE: HY000
Message: Error on observer while running replication hook '%s'.
ER_RUN_HOOK_ERROR was added in 5.7.6.
• Error number: 3101; Symbol: ER_TRANSACTION_ROLLBACK_DURING_COMMIT; SQLSTATE: HY000
Message: Plugin instructed the server to rollback the current transaction.
When using Group Replication, this means that a transaction failed the group certification process, due
to one or more members detecting a potential conflict, and was thus rolled back. See Group Replication.
ER_TRANSACTION_ROLLBACK_DURING_COMMIT was added in 5.7.6.
• Error number: 3102; Symbol: ER_GENERATED_COLUMN_FUNCTION_IS_NOT_ALLOWED; SQLSTATE:
HY000
Message: Expression of generated column '%s' contains a disallowed function.
ER_GENERATED_COLUMN_FUNCTION_IS_NOT_ALLOWED was added in 5.7.6.
• Error number: 3103; Symbol: ER_KEY_BASED_ON_GENERATED_COLUMN; SQLSTATE: HY000
Message: Key/Index cannot be defined on a virtual generated column.
ER_KEY_BASED_ON_GENERATED_COLUMN was added in 5.7.6, removed after 5.7.7.
• Error number: 3103; Symbol: ER_UNSUPPORTED_ALTER_INPLACE_ON_VIRTUAL_COLUMN;
SQLSTATE: HY000
Message: INPLACE ADD or DROP of virtual columns cannot be combined with other ALTER TABLE
actions
ER_UNSUPPORTED_ALTER_INPLACE_ON_VIRTUAL_COLUMN was added in 5.7.8.
• Error number: 3104; Symbol: ER_WRONG_FK_OPTION_FOR_GENERATED_COLUMN; SQLSTATE: HY000
Message: Cannot define foreign key with %s clause on a generated column.
ER_WRONG_FK_OPTION_FOR_GENERATED_COLUMN was added in 5.7.6.
• Error number: 3105; Symbol: ER_NON_DEFAULT_VALUE_FOR_GENERATED_COLUMN; SQLSTATE:
HY000
Message: The value specified for generated column '%s' in table '%s' is not allowed.
ER_NON_DEFAULT_VALUE_FOR_GENERATED_COLUMN was added in 5.7.6.
• Error number: 3106; Symbol: ER_UNSUPPORTED_ACTION_ON_GENERATED_COLUMN; SQLSTATE:
HY000
Message: '%s' is not supported for generated columns.
ER_UNSUPPORTED_ACTION_ON_GENERATED_COLUMN was added in 5.7.6.
• Error number: 3107; Symbol: ER_GENERATED_COLUMN_NON_PRIOR; SQLSTATE: HY000
Message: Generated column can refer only to generated columns defined prior to it.
82
To address this issue, change the table definition to define each generated column later than any
generated columns to which it refers.
ER_GENERATED_COLUMN_NON_PRIOR was added in 5.7.6.
• Error number: 3108; Symbol: ER_DEPENDENT_BY_GENERATED_COLUMN; SQLSTATE: HY000
Message: Column '%s' has a generated column dependency.
You cannot drop or rename a generated column if another column refers to it. You must either drop
those columns as well, or redefine them not to refer to the generated column.
ER_DEPENDENT_BY_GENERATED_COLUMN was added in 5.7.6.
• Error number: 3109; Symbol: ER_GENERATED_COLUMN_REF_AUTO_INC; SQLSTATE: HY000
Message: Generated column '%s' cannot refer to auto-increment column.
ER_GENERATED_COLUMN_REF_AUTO_INC was added in 5.7.6.
• Error number: 3110; Symbol: ER_FEATURE_NOT_AVAILABLE; SQLSTATE: HY000
Message: The '%s' feature is not available; you need to remove '%s' or use MySQL built with '%s'
ER_FEATURE_NOT_AVAILABLE was added in 5.7.6.
• Error number: 3111; Symbol: ER_CANT_SET_GTID_MODE; SQLSTATE: HY000
Message: SET @@GLOBAL.GTID_MODE = %s is not allowed because %s.
ER_CANT_SET_GTID_MODE was added in 5.7.6.
• Error number: 3112; Symbol: ER_CANT_USE_AUTO_POSITION_WITH_GTID_MODE_OFF; SQLSTATE:
HY000
Message: The replication receiver thread%s cannot start in AUTO_POSITION mode: this server uses
@@GLOBAL.GTID_MODE = OFF.
ER_CANT_USE_AUTO_POSITION_WITH_GTID_MODE_OFF was added in 5.7.6.
• Error number: 3113; Symbol: ER_CANT_REPLICATE_ANONYMOUS_WITH_AUTO_POSITION;
SQLSTATE: HY000
Message: Cannot replicate anonymous transaction when AUTO_POSITION = 1, at file %s, position %lld.
ER_CANT_REPLICATE_ANONYMOUS_WITH_AUTO_POSITION was added in 5.7.6.
• Error number: 3114; Symbol: ER_CANT_REPLICATE_ANONYMOUS_WITH_GTID_MODE_ON; SQLSTATE:
HY000
Message: Cannot replicate anonymous transaction when @@GLOBAL.GTID_MODE = ON, at file %s,
position %lld.
ER_CANT_REPLICATE_ANONYMOUS_WITH_GTID_MODE_ON was added in 5.7.6.
83
• Error number: 3115; Symbol: ER_CANT_REPLICATE_GTID_WITH_GTID_MODE_OFF; SQLSTATE:
HY000
Message: Cannot replicate GTID-transaction when @@GLOBAL.GTID_MODE = OFF, at file %s,
position %lld.
ER_CANT_REPLICATE_GTID_WITH_GTID_MODE_OFF was added in 5.7.6.
• Error number: 3116; Symbol:
ER_CANT_SET_ENFORCE_GTID_CONSISTENCY_ON_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS;
SQLSTATE: HY000
Message: Cannot set ENFORCE_GTID_CONSISTENCY = ON because there are ongoing transactions
that violate GTID consistency.
ER_CANT_SET_ENFORCE_GTID_CONSISTENCY_ON_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS
is renamed to ER_CANT_ENFORCE_GTID_CONSISTENCY_WITH_ONGOING_GTID_VIOLATING_TX in
MySQL 8.0.
ER_CANT_SET_ENFORCE_GTID_CONSISTENCY_ON_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS
was added in 5.7.6.
• Error number: 3117; Symbol:
ER_SET_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS;
SQLSTATE: HY000
Message: There are ongoing transactions that violate GTID consistency.
ER_SET_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS
is renamed to ER_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOING_GTID_VIOLATING_TX in
MySQL 8.0.
ER_SET_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS
was added in 5.7.6.
• Error number: 3118; Symbol: ER_ACCOUNT_HAS_BEEN_LOCKED; SQLSTATE: HY000
Message: Access denied for user '%s'@'%s'. Account is locked.
The account was locked with CREATE USER ... ACCOUNT LOCK or ALTER USER ... ACCOUNT
LOCK. An administrator can unlock it with ALTER USER ... ACCOUNT UNLOCK.
ER_ACCOUNT_HAS_BEEN_LOCKED was added in 5.7.6.
• Error number: 3119; Symbol: ER_WRONG_TABLESPACE_NAME; SQLSTATE: 42000
Message: Incorrect tablespace name `%s`
ER_WRONG_TABLESPACE_NAME was added in 5.7.6.
• Error number: 3120; Symbol: ER_TABLESPACE_IS_NOT_EMPTY; SQLSTATE: HY000
Message: Tablespace `%s` is not empty.
ER_TABLESPACE_IS_NOT_EMPTY was added in 5.7.6.
84
• Error number: 3121; Symbol: ER_WRONG_FILE_NAME; SQLSTATE: HY000
Message: Incorrect File Name '%s'.
ER_WRONG_FILE_NAME was added in 5.7.6.
• Error number: 3122; Symbol: ER_BOOST_GEOMETRY_INCONSISTENT_TURNS_EXCEPTION;
SQLSTATE: HY000
Message: Inconsistent intersection points.
ER_BOOST_GEOMETRY_INCONSISTENT_TURNS_EXCEPTION was added in 5.7.7.
• Error number: 3123; Symbol: ER_WARN_OPTIMIZER_HINT_SYNTAX_ERROR; SQLSTATE: HY000
Message: Optimizer hint syntax error
ER_WARN_OPTIMIZER_HINT_SYNTAX_ERROR was added in 5.7.7.
• Error number: 3124; Symbol: ER_WARN_BAD_MAX_EXECUTION_TIME; SQLSTATE: HY000
Message: Unsupported MAX_EXECUTION_TIME
ER_WARN_BAD_MAX_EXECUTION_TIME was added in 5.7.7.
• Error number: 3125; Symbol: ER_WARN_UNSUPPORTED_MAX_EXECUTION_TIME; SQLSTATE: HY000
Message: MAX_EXECUTION_TIME hint is supported by top-level standalone SELECT statements only
The MAX_EXECUTION_TIME optimizer hint is supported only for SELECT statements.
ER_WARN_UNSUPPORTED_MAX_EXECUTION_TIME was added in 5.7.7.
• Error number: 3126; Symbol: ER_WARN_CONFLICTING_HINT; SQLSTATE: HY000
Message: Hint %s is ignored as conflicting/duplicated
ER_WARN_CONFLICTING_HINT was added in 5.7.7.
• Error number: 3127; Symbol: ER_WARN_UNKNOWN_QB_NAME; SQLSTATE: HY000
Message: Query block name %s is not found for %s hint
ER_WARN_UNKNOWN_QB_NAME was added in 5.7.7.
• Error number: 3128; Symbol: ER_UNRESOLVED_HINT_NAME; SQLSTATE: HY000
Message: Unresolved name %s for %s hint
ER_UNRESOLVED_HINT_NAME was added in 5.7.7.
• Error number: 3129; Symbol: ER_WARN_DEPRECATED_SQLMODE_UNSET; SQLSTATE: HY000
Message: Unsetting sql mode '%s' is deprecated. It will be made read-only in a future release.
ER_WARN_DEPRECATED_SQLMODE_UNSET was added in 5.7.7, removed after 5.7.7.
• Error number: 3129; Symbol: ER_WARN_ON_MODIFYING_GTID_EXECUTED_TABLE; SQLSTATE:
HY000
85
Message: Please do not modify the %s table. This is a mysql internal system table to store GTIDs for
committed transactions. Modifying it can lead to an inconsistent GTID state.
ER_WARN_ON_MODIFYING_GTID_EXECUTED_TABLE was added in 5.7.8.
• Error number: 3130; Symbol: ER_PLUGGABLE_PROTOCOL_COMMAND_NOT_SUPPORTED; SQLSTATE:
HY000
Message: Command not supported by pluggable protocols
ER_PLUGGABLE_PROTOCOL_COMMAND_NOT_SUPPORTED was added in 5.7.8.
• Error number: 3131; Symbol: ER_LOCKING_SERVICE_WRONG_NAME; SQLSTATE: 42000
Message: Incorrect locking service lock name '%s'.
A locking service name was specified as NULL, the empty string, or a string longer than 64 characters.
Namespace and lock names must be non-NULL, nonempty, and no more than 64 characters long.
ER_LOCKING_SERVICE_WRONG_NAME was added in 5.7.8.
• Error number: 3132; Symbol: ER_LOCKING_SERVICE_DEADLOCK; SQLSTATE: HY000
Message: Deadlock found when trying to get locking service lock; try releasing locks and restarting lock
acquisition.
ER_LOCKING_SERVICE_DEADLOCK was added in 5.7.8.
• Error number: 3133; Symbol: ER_LOCKING_SERVICE_TIMEOUT; SQLSTATE: HY000
Message: Service lock wait timeout exceeded.
ER_LOCKING_SERVICE_TIMEOUT was added in 5.7.8.
• Error number: 3134; Symbol: ER_GIS_MAX_POINTS_IN_GEOMETRY_OVERFLOWED; SQLSTATE:
HY000
Message: Parameter %s exceeds the maximum number of points in a geometry (%lu) in function %s.
ER_GIS_MAX_POINTS_IN_GEOMETRY_OVERFLOWED was added in 5.7.8.
• Error number: 3135; Symbol: ER_SQL_MODE_MERGED; SQLSTATE: HY000
Message: 'NO_ZERO_DATE', 'NO_ZERO_IN_DATE' and 'ERROR_FOR_DIVISION_BY_ZERO' sql
modes should be used with strict mode. They will be merged with strict mode in a future release.
ER_SQL_MODE_MERGED was added in 5.7.8.
• Error number: 3136; Symbol: ER_VTOKEN_PLUGIN_TOKEN_MISMATCH; SQLSTATE: HY000
Message: Version token mismatch for %.*s. Correct value %.*s
The client has set its version_tokens_session system variable to the list of tokens it requires the
server to match, but the server token list has at least one matching token name that has a value different
from what the client requires. See Version Tokens.
ER_VTOKEN_PLUGIN_TOKEN_MISMATCH was added in 5.7.8.
86
• Error number: 3137; Symbol: ER_VTOKEN_PLUGIN_TOKEN_NOT_FOUND; SQLSTATE: HY000
Message: Version token %.*s not found.
The client has set its version_tokens_session system variable to the list of tokens it requires the
server to match, but the server token list is missing at least one of those tokens. See Version Tokens.
ER_VTOKEN_PLUGIN_TOKEN_NOT_FOUND was added in 5.7.8.
• Error number: 3138; Symbol: ER_CANT_SET_VARIABLE_WHEN_OWNING_GTID; SQLSTATE: HY000
Message: Variable %s cannot be changed by a client that owns a GTID. The client owns %s. Ownership
is released on COMMIT or ROLLBACK.
ER_CANT_SET_VARIABLE_WHEN_OWNING_GTID was added in 5.7.8.
• Error number: 3139; Symbol: ER_SLAVE_CHANNEL_OPERATION_NOT_ALLOWED; SQLSTATE: HY000
Message: %s cannot be performed on channel '%s'.
ER_SLAVE_CHANNEL_OPERATION_NOT_ALLOWED was added in 5.7.8.
• Error number: 3140; Symbol: ER_INVALID_JSON_TEXT; SQLSTATE: 22032
Message: Invalid JSON text: "%s" at position %u in value for column '%s'.
ER_INVALID_JSON_TEXT was added in 5.7.8.
• Error number: 3141; Symbol: ER_INVALID_JSON_TEXT_IN_PARAM; SQLSTATE: 22032
Message: Invalid JSON text in argument %u to function %s: "%s" at position %u.%s
ER_INVALID_JSON_TEXT_IN_PARAM was added in 5.7.8.
• Error number: 3142; Symbol: ER_INVALID_JSON_BINARY_DATA; SQLSTATE: HY000
Message: The JSON binary value contains invalid data.
ER_INVALID_JSON_BINARY_DATA was added in 5.7.8.
• Error number: 3143; Symbol: ER_INVALID_JSON_PATH; SQLSTATE: 42000
Message: Invalid JSON path expression. The error is around character position %u.%s
ER_INVALID_JSON_PATH was added in 5.7.8.
• Error number: 3144; Symbol: ER_INVALID_JSON_CHARSET; SQLSTATE: 22032
Message: Cannot create a JSON value from a string with CHARACTER SET '%s'.
ER_INVALID_JSON_CHARSET was added in 5.7.8.
• Error number: 3145; Symbol: ER_INVALID_JSON_CHARSET_IN_FUNCTION; SQLSTATE: 22032
Message: Invalid JSON character data provided to function %s: '%s'; utf8 is required.
ER_INVALID_JSON_CHARSET_IN_FUNCTION was added in 5.7.8.
• Error number: 3146; Symbol: ER_INVALID_TYPE_FOR_JSON; SQLSTATE: 22032
87
Message: Invalid data type for JSON data in argument %u to function %s; a JSON string or JSON type
is required.
ER_INVALID_TYPE_FOR_JSON was added in 5.7.8.
• Error number: 3147; Symbol: ER_INVALID_CAST_TO_JSON; SQLSTATE: 22032
Message: Cannot CAST value to JSON.
ER_INVALID_CAST_TO_JSON was added in 5.7.8.
• Error number: 3148; Symbol: ER_INVALID_JSON_PATH_CHARSET; SQLSTATE: 42000
Message: A path expression must be encoded in the utf8 character set. The path expression '%s' is
encoded in character set '%s'.
ER_INVALID_JSON_PATH_CHARSET was added in 5.7.8.
• Error number: 3149; Symbol: ER_INVALID_JSON_PATH_WILDCARD; SQLSTATE: 42000
Message: In this situation, path expressions may not contain the * and ** tokens.
ER_INVALID_JSON_PATH_WILDCARD was added in 5.7.8.
• Error number: 3150; Symbol: ER_JSON_VALUE_TOO_BIG; SQLSTATE: 22032
Message: The JSON value is too big to be stored in a JSON column.
ER_JSON_VALUE_TOO_BIG was added in 5.7.8.
• Error number: 3151; Symbol: ER_JSON_KEY_TOO_BIG; SQLSTATE: 22032
Message: The JSON object contains a key name that is too long.
ER_JSON_KEY_TOO_BIG was added in 5.7.8.
• Error number: 3152; Symbol: ER_JSON_USED_AS_KEY; SQLSTATE: 42000
Message: JSON column '%s' cannot be used in key specification.
ER_JSON_USED_AS_KEY was added in 5.7.8.
• Error number: 3153; Symbol: ER_JSON_VACUOUS_PATH; SQLSTATE: 42000
Message: The path expression '$' is not allowed in this context.
ER_JSON_VACUOUS_PATH was added in 5.7.8.
• Error number: 3154; Symbol: ER_JSON_BAD_ONE_OR_ALL_ARG; SQLSTATE: 42000
Message: The oneOrAll argument to %s may take these values: 'one' or 'all'.
ER_JSON_BAD_ONE_OR_ALL_ARG was added in 5.7.8.
• Error number: 3155; Symbol: ER_NUMERIC_JSON_VALUE_OUT_OF_RANGE; SQLSTATE: 22003
Message: Out of range JSON value for CAST to %s%s from column %s at row %ld
ER_NUMERIC_JSON_VALUE_OUT_OF_RANGE was added in 5.7.8.
88
• Error number: 3156; Symbol: ER_INVALID_JSON_VALUE_FOR_CAST; SQLSTATE: 22018
Message: Invalid JSON value for CAST to %s%s from column %s at row %ld
ER_INVALID_JSON_VALUE_FOR_CAST was added in 5.7.8.
• Error number: 3157; Symbol: ER_JSON_DOCUMENT_TOO_DEEP; SQLSTATE: 22032
Message: The JSON document exceeds the maximum depth.
ER_JSON_DOCUMENT_TOO_DEEP was added in 5.7.8.
• Error number: 3158; Symbol: ER_JSON_DOCUMENT_NULL_KEY; SQLSTATE: 22032
Message: JSON documents may not contain NULL member names.
ER_JSON_DOCUMENT_NULL_KEY was added in 5.7.8.
• Error number: 3159; Symbol: ER_SECURE_TRANSPORT_REQUIRED; SQLSTATE: HY000
Message: Connections using insecure transport are prohibited while --require_secure_transport=ON.
With the require_secure_transport system variable, clients can connect only using secure
transports. Qualifying connections are those using SSL, a Unix socket file, or shared memory.
ER_SECURE_TRANSPORT_REQUIRED was added in 5.7.8.
• Error number: 3160; Symbol: ER_NO_SECURE_TRANSPORTS_CONFIGURED; SQLSTATE: HY000
Message: No secure transports (SSL or Shared Memory) are configured, unable to set --
require_secure_transport=ON.
The require_secure_transport system variable cannot be enabled if the server does not support
at least one secure transport. Configure the server with the required SSL keys/certificates to enable SSL
connections, or enable the shared_memory system variable to enable shared-memory connections.
ER_NO_SECURE_TRANSPORTS_CONFIGURED was added in 5.7.8.
• Error number: 3161; Symbol: ER_DISABLED_STORAGE_ENGINE; SQLSTATE: HY000
Message: Storage engine %s is disabled (Table creation is disallowed).
An attempt was made to create a table or tablespace using a storage engine listed in the value of the
disabled_storage_engines system variable, or to change an existing table or tablespace to such
an engine. Choose a different storage engine.
ER_DISABLED_STORAGE_ENGINE was added in 5.7.8.
• Error number: 3162; Symbol: ER_USER_DOES_NOT_EXIST; SQLSTATE: HY000
Message: User %s does not exist.
ER_USER_DOES_NOT_EXIST was added in 5.7.8.
• Error number: 3163; Symbol: ER_USER_ALREADY_EXISTS; SQLSTATE: HY000
Message: User %s already exists.
ER_USER_ALREADY_EXISTS was added in 5.7.8.
89
• Error number: 3164; Symbol: ER_AUDIT_API_ABORT; SQLSTATE: HY000
Message: Aborted by Audit API ('%s';%d).
This error indicates that an audit plugin terminated execution of an event. The message typically
indicates the event subclass name and a numeric status value.
ER_AUDIT_API_ABORT was added in 5.7.8.
• Error number: 3165; Symbol: ER_INVALID_JSON_PATH_ARRAY_CELL; SQLSTATE: 42000
Message: A path expression is not a path to a cell in an array.
ER_INVALID_JSON_PATH_ARRAY_CELL was added in 5.7.8.
• Error number: 3166; Symbol: ER_BUFPOOL_RESIZE_INPROGRESS; SQLSTATE: HY000
Message: Another buffer pool resize is already in progress.
ER_BUFPOOL_RESIZE_INPROGRESS was added in 5.7.9.
• Error number: 3167; Symbol: ER_FEATURE_DISABLED_SEE_DOC; SQLSTATE: HY000
Message: The '%s' feature is disabled; see the documentation for '%s'
ER_FEATURE_DISABLED_SEE_DOC was added in 5.7.9.
• Error number: 3168; Symbol: ER_SERVER_ISNT_AVAILABLE; SQLSTATE: HY000
Message: Server isn't available
ER_SERVER_ISNT_AVAILABLE was added in 5.7.9.
• Error number: 3169; Symbol: ER_SESSION_WAS_KILLED; SQLSTATE: HY000
Message: Session was killed
ER_SESSION_WAS_KILLED was added in 5.7.9.
• Error number: 3170; Symbol: ER_CAPACITY_EXCEEDED; SQLSTATE: HY000
Message: Memory capacity of %llu bytes for '%s' exceeded. %s
ER_CAPACITY_EXCEEDED was added in 5.7.9.
• Error number: 3171; Symbol: ER_CAPACITY_EXCEEDED_IN_RANGE_OPTIMIZER; SQLSTATE: HY000
Message: Range optimization was not done for this query.
ER_CAPACITY_EXCEEDED_IN_RANGE_OPTIMIZER was added in 5.7.9.
• Error number: 3172; Symbol: ER_TABLE_NEEDS_UPG_PART; SQLSTATE: HY000
Message: Partitioning upgrade required. Please dump/reload to fix it or do: ALTER TABLE `%s`.`%s`
UPGRADE PARTITIONING
ER_TABLE_NEEDS_UPG_PART was added in 5.7.9.
• Error number: 3173; Symbol: ER_CANT_WAIT_FOR_EXECUTED_GTID_SET_WHILE_OWNING_A_GTID;
SQLSTATE: HY000
90
Message: The client holds ownership of the GTID %s. Therefore, WAIT_FOR_EXECUTED_GTID_SET
cannot wait for this GTID.
ER_CANT_WAIT_FOR_EXECUTED_GTID_SET_WHILE_OWNING_A_GTID was added in 5.7.9.
• Error number: 3174; Symbol: ER_CANNOT_ADD_FOREIGN_BASE_COL_VIRTUAL; SQLSTATE: HY000
Message: Cannot add foreign key on the base column of indexed virtual column.
ER_CANNOT_ADD_FOREIGN_BASE_COL_VIRTUAL was added in 5.7.10.
• Error number: 3175; Symbol: ER_CANNOT_CREATE_VIRTUAL_INDEX_CONSTRAINT; SQLSTATE:
HY000
Message: Cannot create index on virtual column whose base column has foreign constraint.
ER_CANNOT_CREATE_VIRTUAL_INDEX_CONSTRAINT was added in 5.7.10.
• Error number: 3176; Symbol: ER_ERROR_ON_MODIFYING_GTID_EXECUTED_TABLE; SQLSTATE:
HY000
Message: Please do not modify the %s table with an XA transaction. This is an internal system table
used to store GTIDs for committed transactions. Although modifying it can lead to an inconsistent GTID
state, if neccessary you can modify it with a non-XA transaction.
ER_ERROR_ON_MODIFYING_GTID_EXECUTED_TABLE was added in 5.7.11.
• Error number: 3177; Symbol: ER_LOCK_REFUSED_BY_ENGINE; SQLSTATE: HY000
Message: Lock acquisition refused by storage engine.
ER_LOCK_REFUSED_BY_ENGINE was added in 5.7.11.
• Error number: 3178; Symbol: ER_UNSUPPORTED_ALTER_ONLINE_ON_VIRTUAL_COLUMN; SQLSTATE:
HY000
Message: ADD COLUMN col...VIRTUAL, ADD INDEX(col)
ER_UNSUPPORTED_ALTER_ONLINE_ON_VIRTUAL_COLUMN was added in 5.7.11.
• Error number: 3179; Symbol: ER_MASTER_KEY_ROTATION_NOT_SUPPORTED_BY_SE; SQLSTATE:
HY000
Message: Master key rotation is not supported by storage engine.
ER_MASTER_KEY_ROTATION_NOT_SUPPORTED_BY_SE was added in 5.7.11.
• Error number: 3180; Symbol: ER_MASTER_KEY_ROTATION_ERROR_BY_SE; SQLSTATE: HY000
Message: Encryption key rotation error reported by SE: %s
ER_MASTER_KEY_ROTATION_ERROR_BY_SE was added in 5.7.11.
• Error number: 3181; Symbol: ER_MASTER_KEY_ROTATION_BINLOG_FAILED; SQLSTATE: HY000
Message: Write to binlog failed. However, master key rotation has been completed successfully.
ER_MASTER_KEY_ROTATION_BINLOG_FAILED was added in 5.7.11.
91
• Error number: 3182; Symbol: ER_MASTER_KEY_ROTATION_SE_UNAVAILABLE; SQLSTATE: HY000
Message: Storage engine is not available.
ER_MASTER_KEY_ROTATION_SE_UNAVAILABLE was added in 5.7.11.
• Error number: 3183; Symbol: ER_TABLESPACE_CANNOT_ENCRYPT; SQLSTATE: HY000
Message: This tablespace can't be encrypted.
ER_TABLESPACE_CANNOT_ENCRYPT was added in 5.7.11.
• Error number: 3184; Symbol: ER_INVALID_ENCRYPTION_OPTION; SQLSTATE: HY000
Message: Invalid encryption option.
ER_INVALID_ENCRYPTION_OPTION was added in 5.7.11.
• Error number: 3185; Symbol: ER_CANNOT_FIND_KEY_IN_KEYRING; SQLSTATE: HY000
Message: Can't find master key from keyring, please check in the server log if a keyring plugin is loaded
and initialized successfully.
ER_CANNOT_FIND_KEY_IN_KEYRING was added in 5.7.11.
• Error number: 3186; Symbol: ER_CAPACITY_EXCEEDED_IN_PARSER; SQLSTATE: HY000
Message: Parser bailed out for this query.
ER_CAPACITY_EXCEEDED_IN_PARSER was added in 5.7.12.
• Error number: 3187; Symbol: ER_UNSUPPORTED_ALTER_ENCRYPTION_INPLACE; SQLSTATE: HY000
Message: Cannot alter encryption attribute by inplace algorithm.
ER_UNSUPPORTED_ALTER_ENCRYPTION_INPLACE was added in 5.7.13.
• Error number: 3188; Symbol: ER_KEYRING_UDF_KEYRING_SERVICE_ERROR; SQLSTATE: HY000
Message: Function '%s' failed because underlying keyring service returned an error. Please check if a
keyring plugin is installed and that provided arguments are valid for the keyring you are using.
ER_KEYRING_UDF_KEYRING_SERVICE_ERROR was added in 5.7.13.
• Error number: 3189; Symbol: ER_USER_COLUMN_OLD_LENGTH; SQLSTATE: HY000
Message: It seems that your db schema is old. The %s column is 77 characters long and should be 93
characters long. Please run mysql_upgrade.
ER_USER_COLUMN_OLD_LENGTH was added in 5.7.13.
• Error number: 3190; Symbol: ER_CANT_RESET_MASTER; SQLSTATE: HY000
Message: RESET MASTER is not allowed because %s.
ER_CANT_RESET_MASTER was added in 5.7.14.
92
• Error number: 3191; Symbol: ER_GROUP_REPLICATION_MAX_GROUP_SIZE; SQLSTATE: HY000
Message: The START GROUP_REPLICATION command failed since the group already has 9
members.
ER_GROUP_REPLICATION_MAX_GROUP_SIZE was added in 5.7.14.
• Error number: 3192; Symbol: ER_CANNOT_ADD_FOREIGN_BASE_COL_STORED; SQLSTATE: HY000
Message: Cannot add foreign key on the base column of stored column.
ER_CANNOT_ADD_FOREIGN_BASE_COL_STORED was added in 5.7.14.
• Error number: 3193; Symbol: ER_TABLE_REFERENCED; SQLSTATE: HY000
Message: Cannot complete the operation because table is referenced by another connection.
ER_TABLE_REFERENCED was added in 5.7.14.
• Error number: 3194; Symbol: ER_PARTITION_ENGINE_DEPRECATED_FOR_TABLE; SQLSTATE:
HY000
Message: The partition engine, used by table '%s.%s', is deprecated and will be removed in a future
release. Please use native partitioning instead.
ER_PARTITION_ENGINE_DEPRECATED_FOR_TABLE was added in 5.7.17.
• Error number: 3195; Symbol: ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID_ZERO; SQLSTATE:
01000
Message: %s(geometry) is deprecated and will be replaced by st_srid(geometry, 0) in a future version.
Use %s(st_aswkb(geometry), 0) instead.
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID_ZERO was added in 5.7.19.
• Error number: 3196; Symbol: ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID; SQLSTATE: 01000
Message: %s(geometry, srid) is deprecated and will be replaced by st_srid(geometry, srid) in a future
version. Use %s(st_aswkb(geometry), srid) instead.
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID was added in 5.7.19.
• Error number: 3197; Symbol: ER_XA_RETRY; SQLSTATE: HY000
Message: The resource manager is not able to commit the transaction branch at this time. Please retry
later.
ER_XA_RETRY was added in 5.7.19.
• Error number: 3198; Symbol: ER_KEYRING_AWS_UDF_AWS_KMS_ERROR; SQLSTATE: HY000
Message: Function %s failed due to: %s.
ER_KEYRING_AWS_UDF_AWS_KMS_ERROR was added in 5.7.19.
• Error number: 3199; Symbol: ER_BINLOG_UNSAFE_XA; SQLSTATE: HY000
Message: Statement is unsafe because it is being used inside a XA transaction. Concurrent XA
transactions may deadlock on slaves when replicated using statements.
93
ER_BINLOG_UNSAFE_XA was added in 5.7.20.
• Error number: 3200; Symbol: ER_UDF_ERROR; SQLSTATE: HY000
Message: %s UDF failed; %s
ER_UDF_ERROR was added in 5.7.21.
• Error number: 3201; Symbol: ER_KEYRING_MIGRATION_FAILURE; SQLSTATE: HY000
Message: Can not perform keyring migration : %s
ER_KEYRING_MIGRATION_FAILURE was added in 5.7.21.
• Error number: 3202; Symbol: ER_KEYRING_ACCESS_DENIED_ERROR; SQLSTATE: 42000
Message: Access denied; you need %s privileges for this operation
ER_KEYRING_ACCESS_DENIED_ERROR was added in 5.7.21.
• Error number: 3203; Symbol: ER_KEYRING_MIGRATION_STATUS; SQLSTATE: HY000
Message: Keyring migration %s.
ER_KEYRING_MIGRATION_STATUS was added in 5.7.21.
• Error number: 3204; Symbol: ER_PLUGIN_FAILED_TO_OPEN_TABLES; SQLSTATE: HY000
Message: Failed to open the %s filter tables.
ER_PLUGIN_FAILED_TO_OPEN_TABLES was added in 5.7.22.
• Error number: 3205; Symbol: ER_PLUGIN_FAILED_TO_OPEN_TABLE; SQLSTATE: HY000
Message: Failed to open '%s.%s' %s table.
ER_PLUGIN_FAILED_TO_OPEN_TABLE was added in 5.7.22.
• Error number: 3206; Symbol: ER_AUDIT_LOG_NO_KEYRING_PLUGIN_INSTALLED; SQLSTATE:
HY000
Message: No keyring plugin installed.
ER_AUDIT_LOG_NO_KEYRING_PLUGIN_INSTALLED was added in 5.7.22.
• Error number: 3207; Symbol: ER_AUDIT_LOG_ENCRYPTION_PASSWORD_HAS_NOT_BEEN_SET;
SQLSTATE: HY000
Message: Audit log encryption password has not been set; it will be generated automatically. Use
audit_log_encryption_password_get to obtain the password or audit_log_encryption_password_set to
set a new one.
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_HAS_NOT_BEEN_SET was added in 5.7.22.
• Error number: 3208; Symbol: ER_AUDIT_LOG_COULD_NOT_CREATE_AES_KEY; SQLSTATE: HY000
Message: Could not create AES key. OpenSSL's EVP_BytesToKey function failed.
ER_AUDIT_LOG_COULD_NOT_CREATE_AES_KEY was added in 5.7.22.
94
• Error number: 3209; Symbol: ER_AUDIT_LOG_ENCRYPTION_PASSWORD_CANNOT_BE_FETCHED;
SQLSTATE: HY000
Message: Audit log encryption password cannot be fetched from the keyring. Password used so far is
used for encryption.
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_CANNOT_BE_FETCHED was added in 5.7.22.
• Error number: 3210; Symbol: ER_AUDIT_LOG_JSON_FILTERING_NOT_ENABLED; SQLSTATE: HY000
Message: Audit Log filtering has not been installed.
ER_AUDIT_LOG_JSON_FILTERING_NOT_ENABLED was added in 5.7.22.
• Error number: 3211; Symbol: ER_AUDIT_LOG_UDF_INSUFFICIENT_PRIVILEGE; SQLSTATE: HY000
Message: Request ignored for '%s'@'%s'. SUPER_ACL needed to perform operation
ER_AUDIT_LOG_UDF_INSUFFICIENT_PRIVILEGE was added in 5.7.22.
• Error number: 3212; Symbol: ER_AUDIT_LOG_SUPER_PRIVILEGE_REQUIRED; SQLSTATE: HY000
Message: SUPER privilege required for '%s'@'%s' user.
ER_AUDIT_LOG_SUPER_PRIVILEGE_REQUIRED was added in 5.7.22.
• Error number: 3213; Symbol: ER_COULD_NOT_REINITIALIZE_AUDIT_LOG_FILTERS; SQLSTATE:
HY000
Message: Could not reinitialize audit log filters.
ER_COULD_NOT_REINITIALIZE_AUDIT_LOG_FILTERS was added in 5.7.22.
• Error number: 3214; Symbol: ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_TYPE; SQLSTATE: HY000
Message: Invalid argument type
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_TYPE was added in 5.7.22.
• Error number: 3215; Symbol: ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_COUNT; SQLSTATE: HY000
Message: Invalid argument count
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_COUNT was added in 5.7.22.
• Error number: 3216; Symbol: ER_AUDIT_LOG_HAS_NOT_BEEN_INSTALLED; SQLSTATE: HY000
Message: audit_log plugin has not been installed using INSTALL PLUGIN syntax.
ER_AUDIT_LOG_HAS_NOT_BEEN_INSTALLED was added in 5.7.22.
• Error number: 3217; Symbol:
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_TYPE; SQLSTATE: HY000
Message: Invalid "max_array_length" argument type.
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_TYPE was added in 5.7.22.
• Error number: 3218; Symbol:
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_VALUE; SQLSTATE: HY000
95
Message: Invalid "max_array_length" argument value.
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_VALUE was added in 5.7.22.
• Error number: 3219; Symbol: ER_AUDIT_LOG_JSON_FILTER_PARSING_ERROR; SQLSTATE: HY000
Message: %s
ER_AUDIT_LOG_JSON_FILTER_PARSING_ERROR was added in 5.7.22.
• Error number: 3220; Symbol: ER_AUDIT_LOG_JSON_FILTER_NAME_CANNOT_BE_EMPTY; SQLSTATE:
HY000
Message: Filter name cannot be empty.
ER_AUDIT_LOG_JSON_FILTER_NAME_CANNOT_BE_EMPTY was added in 5.7.22.
• Error number: 3221; Symbol: ER_AUDIT_LOG_JSON_USER_NAME_CANNOT_BE_EMPTY; SQLSTATE:
HY000
Message: User cannot be empty.
ER_AUDIT_LOG_JSON_USER_NAME_CANNOT_BE_EMPTY was added in 5.7.22.
• Error number: 3222; Symbol: ER_AUDIT_LOG_JSON_FILTER_DOES_NOT_EXISTS; SQLSTATE:
HY000
Message: Specified filter has not been found.
ER_AUDIT_LOG_JSON_FILTER_DOES_NOT_EXISTS was added in 5.7.22.
• Error number: 3223; Symbol: ER_AUDIT_LOG_USER_FIRST_CHARACTER_MUST_BE_ALPHANUMERIC;
SQLSTATE: HY000
Message: First character of the user name must be alphanumeric.
ER_AUDIT_LOG_USER_FIRST_CHARACTER_MUST_BE_ALPHANUMERIC was added in 5.7.22.
• Error number: 3224; Symbol: ER_AUDIT_LOG_USER_NAME_INVALID_CHARACTER; SQLSTATE:
HY000
Message: Invalid character in the user name.
ER_AUDIT_LOG_USER_NAME_INVALID_CHARACTER was added in 5.7.22.
• Error number: 3225; Symbol: ER_AUDIT_LOG_HOST_NAME_INVALID_CHARACTER; SQLSTATE:
HY000
Message: Invalid character in the host name.
ER_AUDIT_LOG_HOST_NAME_INVALID_CHARACTER was added in 5.7.22.
• Error number: 3226; Symbol: WARN_DEPRECATED_MAXDB_SQL_MODE_FOR_TIMESTAMP; SQLSTATE:
HY000
Message: With the MAXDB SQL mode enabled, TIMESTAMP is identical with DATETIME. The MAXDB
SQL mode is deprecated and will be removed in a future release. Please disable the MAXDB SQL mode
and use DATETIME instead.
96
WARN_DEPRECATED_MAXDB_SQL_MODE_FOR_TIMESTAMP was added in 5.7.22.
• Error number: 3227; Symbol: ER_XA_REPLICATION_FILTERS; SQLSTATE: HY000
Message: The use of replication filters with XA transactions is not supported, and can lead to an
undefined state in the replication slave.
ER_XA_REPLICATION_FILTERS was added in 5.7.23.
• Error number: 3228; Symbol: ER_CANT_OPEN_ERROR_LOG; SQLSTATE: HY000
Message: Could not open file '%s' for error logging%s%s
ER_CANT_OPEN_ERROR_LOG was added in 5.7.24.
• Error number: 3229; Symbol: ER_GROUPING_ON_TIMESTAMP_IN_DST; SQLSTATE: HY000
Message: Grouping on temporal is non-deterministic for timezones having DST. Please consider
switching to UTC for this query.
ER_GROUPING_ON_TIMESTAMP_IN_DST was added in 5.7.27.
• Error number: 3230; Symbol: ER_CANT_START_SERVER_NAMED_PIPE; SQLSTATE: HY000
Message: Can't start server : Named Pipe "%s" already in use.
ER_CANT_START_SERVER_NAMED_PIPE was added in 5.7.27.
• Error number: 3231; Symbol: ER_WRITE_SET_EXCEEDS_LIMIT; SQLSTATE: HY000
Message: The size of writeset data for the current transaction exceeds a limit imposed by an external
component. If using Group Replication check 'group_replication_transaction_size_limit'.
ER_WRITE_SET_EXCEEDS_LIMIT was added in 5.7.33.
• Error number: 3232; Symbol: ER_DEPRECATED_TLS_VERSION_SESSION; SQLSTATE: HY000
Message: Accepted a connection with deprecated protocol '%s' for account `%s`@`%s` from host `%s`.
Client supplied username `%s`
ER_DEPRECATED_TLS_VERSION_SESSION was added in 5.7.35.
• Error number: 3233; Symbol: ER_WARN_DEPRECATED_TLS_VERSION; SQLSTATE: HY000
Message: A deprecated TLS version %s is enabled. Please use TLSv1.2 or higher.
ER_WARN_DEPRECATED_TLS_VERSION was added in 5.7.35.
• Error number: 3234; Symbol: ER_WARN_WRONG_NATIVE_TABLE_STRUCTURE; SQLSTATE: HY000
Message: Optional native table '%s'.'%s' has the wrong structure or is missing.
ER_WARN_WRONG_NATIVE_TABLE_STRUCTURE was added in 5.7.39.
• Error number: 3235; Symbol: ER_AES_INVALID_KDF_NAME; SQLSTATE: HY000
Message: KDF method name is not valid. Please use hkdf or pbkdf2_hmac method name
ER_AES_INVALID_KDF_NAME was added in 5.7.40.
97
• Error number: 3236; Symbol: ER_AES_INVALID_KDF_ITERATIONS; SQLSTATE: HY000
Message: For KDF method pbkdf2_hmac iterations value less than 1000 or more than 65535 is not
allowed due to security reasons. Please provide iterations >= 1000 and iterations < 65535
ER_AES_INVALID_KDF_ITERATIONS was added in 5.7.40.
• Error number: 3237; Symbol: WARN_AES_KEY_SIZE; SQLSTATE: HY000
Message: AES key size should be %d bytes length or secure KDF methods hkdf or pbkdf2_hmac should
be used, please provide exact AES key size or use KDF methods for better security.
WARN_AES_KEY_SIZE was added in 5.7.40.
• Error number: 3238; Symbol: ER_AES_INVALID_KDF_OPTION_SIZE; SQLSTATE: HY000
Message: KDF option size is invalid, please provide valid size < %d bytes and not NULL
ER_AES_INVALID_KDF_OPTION_SIZE was added in 5.7.40.
98
Chapter 3 Client Error Message Reference
Client error messages originate from within the MySQL client library. Here is an example client error
message, as displayed by the mysql client:
$> mysql -h no-such-host
ERROR 2005 (HY000): Unknown MySQL server host 'no-such-host' (0)
Each client error message includes an error code, SQLSTATE value, and message string, as described
in Error Message Sources and Elements. These elements are available as described in Error Information
Interfaces. For client errors, the SQLSTATE value is always 'HY000' (general error), so it is not
meaningful for distinguishing one client error from another.
The client library also makes available to host client programs any errors that originate on the server side
and are recieved by the client from the server. For a list of server-side errors, see Chapter 2, Server Error
Message Reference.
In addition to the errors in the following list, the client library can also produce error messages that have
error codes in the range from 1 to 999. See Chapter 4, Global Error Message Reference
• Error number: 2000; Symbol: CR_UNKNOWN_ERROR;
Message: Unknown MySQL error
• Error number: 2001; Symbol: CR_SOCKET_CREATE_ERROR;
Message: Can't create UNIX socket (%d)
• Error number: 2002; Symbol: CR_CONNECTION_ERROR;
Message: Can't connect to local MySQL server through socket '%s' (%d)
• Error number: 2003; Symbol: CR_CONN_HOST_ERROR;
Message: Can't connect to MySQL server on '%s' (%d)
• Error number: 2004; Symbol: CR_IPSOCK_ERROR;
Message: Can't create TCP/IP socket (%d)
• Error number: 2005; Symbol: CR_UNKNOWN_HOST;
Message: Unknown MySQL server host '%s' (%d)
• Error number: 2006; Symbol: CR_SERVER_GONE_ERROR;
Message: MySQL server has gone away
• Error number: 2007; Symbol: CR_VERSION_ERROR;
Message: Protocol mismatch; server version = %d, client version = %d
• Error number: 2008; Symbol: CR_OUT_OF_MEMORY;
Message: MySQL client ran out of memory
• Error number: 2009; Symbol: CR_WRONG_HOST_INFO;
Message: Wrong host info
99
• Error number: 2010; Symbol: CR_LOCALHOST_CONNECTION;
Message: Localhost via UNIX socket
• Error number: 2011; Symbol: CR_TCP_CONNECTION;
Message: %s via TCP/IP
• Error number: 2012; Symbol: CR_SERVER_HANDSHAKE_ERR;
Message: Error in server handshake
• Error number: 2013; Symbol: CR_SERVER_LOST;
Message: Lost connection to MySQL server during query
• Error number: 2014; Symbol: CR_COMMANDS_OUT_OF_SYNC;
Message: Commands out of sync; you can't run this command now
Commands were executed in an improper order. This error occurs when a function is called that is not
appropriate for the current state of the connection. For example, if mysql_stmt_fetch() is not called
enough times to read an entire result set (that is, enough times to return MYSQL_NO_DATA), this error
may occur for the following C API call.
• Error number: 2015; Symbol: CR_NAMEDPIPE_CONNECTION;
Message: Named pipe: %s
• Error number: 2016; Symbol: CR_NAMEDPIPEWAIT_ERROR;
Message: Can't wait for named pipe to host: %s pipe: %s (%lu)
• Error number: 2017; Symbol: CR_NAMEDPIPEOPEN_ERROR;
Message: Can't open named pipe to host: %s pipe: %s (%lu)
• Error number: 2018; Symbol: CR_NAMEDPIPESETSTATE_ERROR;
Message: Can't set state of named pipe to host: %s pipe: %s (%lu)
• Error number: 2019; Symbol: CR_CANT_READ_CHARSET;
Message: Can't initialize character set %s (path: %s)
• Error number: 2020; Symbol: CR_NET_PACKET_TOO_LARGE;
Message: Got packet bigger than 'max_allowed_packet' bytes
• Error number: 2021; Symbol: CR_EMBEDDED_CONNECTION;
Message: Embedded server
• Error number: 2022; Symbol: CR_PROBE_SLAVE_STATUS;
Message: Error on SHOW SLAVE STATUS:
• Error number: 2023; Symbol: CR_PROBE_SLAVE_HOSTS;
Message: Error on SHOW SLAVE HOSTS:
100
• Error number: 2024; Symbol: CR_PROBE_SLAVE_CONNECT;
Message: Error connecting to slave:
• Error number: 2025; Symbol: CR_PROBE_MASTER_CONNECT;
Message: Error connecting to master:
• Error number: 2026; Symbol: CR_SSL_CONNECTION_ERROR;
Message: SSL connection error: %s
• Error number: 2027; Symbol: CR_MALFORMED_PACKET;
Message: Malformed packet
• Error number: 2028; Symbol: CR_WRONG_LICENSE;
Message: This client library is licensed only for use with MySQL servers having '%s' license
• Error number: 2029; Symbol: CR_NULL_POINTER;
Message: Invalid use of null pointer
• Error number: 2030; Symbol: CR_NO_PREPARE_STMT;
Message: Statement not prepared
• Error number: 2031; Symbol: CR_PARAMS_NOT_BOUND;
Message: No data supplied for parameters in prepared statement
• Error number: 2032; Symbol: CR_DATA_TRUNCATED;
Message: Data truncated
• Error number: 2033; Symbol: CR_NO_PARAMETERS_EXISTS;
Message: No parameters exist in the statement
• Error number: 2034; Symbol: CR_INVALID_PARAMETER_NO;
Message: Invalid parameter number
The column number for mysql_stmt_fetch_column() was invalid.
The parameter number for mysql_stmt_send_long_data() was invalid.
A key name was empty or the amount of connection attribute data for mysql_options4() exceeds the
64KB limit.
• Error number: 2035; Symbol: CR_INVALID_BUFFER_USE;
Message: Can't send long data for non-string/non-binary data types (parameter: %d)
• Error number: 2036; Symbol: CR_UNSUPPORTED_PARAM_TYPE;
Message: Using unsupported buffer type: %d (parameter: %d)
• Error number: 2037; Symbol: CR_SHARED_MEMORY_CONNECTION;
101
Message: Shared memory: %s
• Error number: 2038; Symbol: CR_SHARED_MEMORY_CONNECT_REQUEST_ERROR;
Message: Can't open shared memory; client could not create request event (%lu)
• Error number: 2039; Symbol: CR_SHARED_MEMORY_CONNECT_ANSWER_ERROR;
Message: Can't open shared memory; no answer event received from server (%lu)
• Error number: 2040; Symbol: CR_SHARED_MEMORY_CONNECT_FILE_MAP_ERROR;
Message: Can't open shared memory; server could not allocate file mapping (%lu)
• Error number: 2041; Symbol: CR_SHARED_MEMORY_CONNECT_MAP_ERROR;
Message: Can't open shared memory; server could not get pointer to file mapping (%lu)
• Error number: 2042; Symbol: CR_SHARED_MEMORY_FILE_MAP_ERROR;
Message: Can't open shared memory; client could not allocate file mapping (%lu)
• Error number: 2043; Symbol: CR_SHARED_MEMORY_MAP_ERROR;
Message: Can't open shared memory; client could not get pointer to file mapping (%lu)
• Error number: 2044; Symbol: CR_SHARED_MEMORY_EVENT_ERROR;
Message: Can't open shared memory; client could not create %s event (%lu)
• Error number: 2045; Symbol: CR_SHARED_MEMORY_CONNECT_ABANDONED_ERROR;
Message: Can't open shared memory; no answer from server (%lu)
• Error number: 2046; Symbol: CR_SHARED_MEMORY_CONNECT_SET_ERROR;
Message: Can't open shared memory; cannot send request event to server (%lu)
• Error number: 2047; Symbol: CR_CONN_UNKNOW_PROTOCOL;
Message: Wrong or unknown protocol
• Error number: 2048; Symbol: CR_INVALID_CONN_HANDLE;
Message: Invalid connection handle
• Error number: 2049; Symbol: CR_SECURE_AUTH;
Message: Connection using old (pre-4.1.1) authentication protocol refused (client option 'secure_auth'
enabled)
CR_SECURE_AUTH was removed after 5.7.4.
• Error number: 2049; Symbol: CR_UNUSED_1;
Message: Connection using old (pre-4.1.1) authentication protocol refused (client option 'secure_auth'
enabled)
CR_UNUSED_1 was added in 5.7.5.
102
• Error number: 2050; Symbol: CR_FETCH_CANCELED;
Message: Row retrieval was canceled by mysql_stmt_close() call
• Error number: 2051; Symbol: CR_NO_DATA;
Message: Attempt to read column without prior row fetch
• Error number: 2052; Symbol: CR_NO_STMT_METADATA;
Message: Prepared statement contains no metadata
• Error number: 2053; Symbol: CR_NO_RESULT_SET;
Message: Attempt to read a row while there is no result set associated with the statement
• Error number: 2054; Symbol: CR_NOT_IMPLEMENTED;
Message: This feature is not implemented yet
• Error number: 2055; Symbol: CR_SERVER_LOST_EXTENDED;
Message: Lost connection to MySQL server at '%s', system error: %d
• Error number: 2056; Symbol: CR_STMT_CLOSED;
Message: Statement closed indirectly because of a preceding %s() call
• Error number: 2057; Symbol: CR_NEW_STMT_METADATA;
Message: The number of columns in the result set differs from the number of bound buffers. You must
reset the statement, rebind the result set columns, and execute the statement again
• Error number: 2058; Symbol: CR_ALREADY_CONNECTED;
Message: This handle is already connected. Use a separate handle for each connection.
• Error number: 2059; Symbol: CR_AUTH_PLUGIN_CANNOT_LOAD;
Message: Authentication plugin '%s' cannot be loaded: %s
• Error number: 2060; Symbol: CR_DUPLICATE_CONNECTION_ATTR;
Message: There is an attribute with the same name already
A duplicate connection attribute name was specified for mysql_options4().
• Error number: 2061; Symbol: CR_AUTH_PLUGIN_ERR;
Message: Authentication plugin '%s' reported error: %s
CR_AUTH_PLUGIN_ERR was added in 5.7.1.
• Error number: 2062; Symbol: CR_INSECURE_API_ERR;
Message: Insecure API function call: '%s' Use instead: '%s'
An insecure function call was detected. Modify the application to use the suggested alternative function
instead.
103
CR_INSECURE_API_ERR was added in 5.7.6.
• Error number: 2063; Symbol: CR_INVALID_CLIENT_CHARSET;
Message: '%s' character set is having more than 1 byte minimum character length, which cannot be
used as a client character set. Please use any of the single byte minimum ones, e.g. utf8mb4, latin1 etc.
CR_INVALID_CLIENT_CHARSET was added in 5.7.42.
104
Chapter 4 Global Error Message Reference
This document lists “global” error messages that are shared in the sense that they can be produced by the
MySQL server or by MySQL client programs. These errors have error codes in the range from 1 to 999.
Each global error message includes an error code, SQLSTATE value, and message string, as described
in Error Message Sources and Elements. These elements are available as described in Error Information
Interfaces. For global errors, the SQLSTATE value is always 'HY000' (general error), so it is not
meaningful for distinguishing one client error from another.
• Error number: 1; Symbol: EE_CANTCREATEFILE;
Message: Can't create/write to file '%s' (Errcode: %d - %s)
• Error number: 2; Symbol: EE_READ;
Message: Error reading file '%s' (Errcode: %d - %s)
• Error number: 3; Symbol: EE_WRITE;
Message: Error writing file '%s' (Errcode: %d - %s)
• Error number: 4; Symbol: EE_BADCLOSE;
Message: Error on close of '%s' (Errcode: %d - %s)
• Error number: 5; Symbol: EE_OUTOFMEMORY;
Message: Out of memory (Needed %u bytes)
• Error number: 6; Symbol: EE_DELETE;
Message: Error on delete of '%s' (Errcode: %d - %s)
• Error number: 7; Symbol: EE_LINK;
Message: Error on rename of '%s' to '%s' (Errcode: %d - %s)
• Error number: 9; Symbol: EE_EOFERR;
Message: Unexpected EOF found when reading file '%s' (Errcode: %d - %s)
• Error number: 10; Symbol: EE_CANTLOCK;
Message: Can't lock file (Errcode: %d - %s)
• Error number: 11; Symbol: EE_CANTUNLOCK;
Message: Can't unlock file (Errcode: %d - %s)
• Error number: 12; Symbol: EE_DIR;
Message: Can't read dir of '%s' (Errcode: %d - %s)
• Error number: 13; Symbol: EE_STAT;
Message: Can't get stat of '%s' (Errcode: %d - %s)
• Error number: 14; Symbol: EE_CANT_CHSIZE;
105
Message: Can't change size of file (Errcode: %d - %s)
• Error number: 15; Symbol: EE_CANT_OPEN_STREAM;
Message: Can't open stream from handle (Errcode: %d - %s)
• Error number: 16; Symbol: EE_GETWD;
Message: Can't get working directory (Errcode: %d - %s)
• Error number: 17; Symbol: EE_SETWD;
Message: Can't change dir to '%s' (Errcode: %d - %s)
• Error number: 18; Symbol: EE_LINK_WARNING;
Message: Warning: '%s' had %d links
• Error number: 19; Symbol: EE_OPEN_WARNING;
Message: Warning: %d files and %d streams is left open
• Error number: 20; Symbol: EE_DISK_FULL;
Message: Disk is full writing '%s' (Errcode: %d - %s). Waiting for someone to free space...
• Error number: 21; Symbol: EE_CANT_MKDIR;
Message: Can't create directory '%s' (Errcode: %d - %s)
• Error number: 22; Symbol: EE_UNKNOWN_CHARSET;
Message: Character set '%s' is not a compiled character set and is not specified in the '%s' file
• Error number: 23; Symbol: EE_OUT_OF_FILERESOURCES;
Message: Out of resources when opening file '%s' (Errcode: %d - %s)
• Error number: 24; Symbol: EE_CANT_READLINK;
Message: Can't read value for symlink '%s' (Error %d - %s)
• Error number: 25; Symbol: EE_CANT_SYMLINK;
Message: Can't create symlink '%s' pointing at '%s' (Error %d - %s)
• Error number: 26; Symbol: EE_REALPATH;
Message: Error on realpath() on '%s' (Error %d - %s)
• Error number: 27; Symbol: EE_SYNC;
Message: Can't sync file '%s' to disk (Errcode: %d - %s)
• Error number: 28; Symbol: EE_UNKNOWN_COLLATION;
Message: Collation '%s' is not a compiled collation and is not specified in the '%s' file
• Error number: 29; Symbol: EE_FILENOTFOUND;
106
Message: File '%s' not found (Errcode: %d - %s)
• Error number: 30; Symbol: EE_FILE_NOT_CLOSED;
Message: File '%s' (fileno: %d) was not closed
• Error number: 31; Symbol: EE_CHANGE_OWNERSHIP;
Message: Can't change ownership of the file '%s' (Errcode: %d - %s)
• Error number: 32; Symbol: EE_CHANGE_PERMISSIONS;
Message: Can't change permissions of the file '%s' (Errcode: %d - %s)
• Error number: 33; Symbol: EE_CANT_SEEK;
Message: Can't seek in file '%s' (Errcode: %d - %s)
• Error number: 34; Symbol: EE_CAPACITY_EXCEEDED;
Message: Memory capacity exceeded (capacity %llu bytes)
EE_CAPACITY_EXCEEDED was added in 5.7.9.
107
108
Index
C
CR_ALREADY_CONNECTED error code, 103
CR_AUTH_PLUGIN_CANNOT_LOAD error code, 103
CR_AUTH_PLUGIN_ERR error code, 103
CR_CANT_READ_CHARSET error code, 100
CR_COMMANDS_OUT_OF_SYNC error code, 100
CR_CONNECTION_ERROR error code, 99
CR_CONN_HOST_ERROR error code, 99
CR_CONN_UNKNOW_PROTOCOL error code, 102
CR_DATA_TRUNCATED error code, 101
CR_DUPLICATE_CONNECTION_ATTR error code, 103
CR_EMBEDDED_CONNECTION error code, 100
CR_FETCH_CANCELED error code, 103
CR_INSECURE_API_ERR error code, 103
CR_INVALID_BUFFER_USE error code, 101
CR_INVALID_CLIENT_CHARSET error code, 104
CR_INVALID_CONN_HANDLE error code, 102
CR_INVALID_PARAMETER_NO error code, 101
CR_IPSOCK_ERROR error code, 99
CR_LOCALHOST_CONNECTION error code, 100
CR_MALFORMED_PACKET error code, 101
CR_NAMEDPIPEOPEN_ERROR error code, 100
CR_NAMEDPIPESETSTATE_ERROR error code, 100
CR_NAMEDPIPEWAIT_ERROR error code, 100
CR_NAMEDPIPE_CONNECTION error code, 100
CR_NET_PACKET_TOO_LARGE error code, 100
CR_NEW_STMT_METADATA error code, 103
CR_NOT_IMPLEMENTED error code, 103
CR_NO_DATA error code, 103
CR_NO_PARAMETERS_EXISTS error code, 101
CR_NO_PREPARE_STMT error code, 101
CR_NO_RESULT_SET error code, 103
CR_NO_STMT_METADATA error code, 103
CR_NULL_POINTER error code, 101
CR_OUT_OF_MEMORY error code, 99
CR_PARAMS_NOT_BOUND error code, 101
CR_PROBE_MASTER_CONNECT error code, 101
CR_PROBE_SLAVE_CONNECT error code, 101
CR_PROBE_SLAVE_HOSTS error code, 100
CR_PROBE_SLAVE_STATUS error code, 100
CR_SECURE_AUTH error code, 102
CR_SERVER_GONE_ERROR error code, 99
CR_SERVER_HANDSHAKE_ERR error code, 100
CR_SERVER_LOST error code, 100
CR_SERVER_LOST_EXTENDED error code, 103
CR_SHARED_MEMORY_CONNECTION error code,
102
CR_SHARED_MEMORY_CONNECT_ABANDONED_ERROR
error code, 102
CR_SHARED_MEMORY_CONNECT_ANSWER_ERROR
error code, 102
CR_SHARED_MEMORY_CONNECT_FILE_MAP_ERROR
error code, 102
CR_SHARED_MEMORY_CONNECT_MAP_ERROR
error code, 102
CR_SHARED_MEMORY_CONNECT_REQUEST_ERROR
error code, 102
CR_SHARED_MEMORY_CONNECT_SET_ERROR
error code, 102
CR_SHARED_MEMORY_EVENT_ERROR error code,
102
CR_SHARED_MEMORY_FILE_MAP_ERROR error
code, 102
CR_SHARED_MEMORY_MAP_ERROR error code, 102
CR_SOCKET_CREATE_ERROR error code, 99
CR_SSL_CONNECTION_ERROR error code, 101
CR_STMT_CLOSED error code, 103
CR_TCP_CONNECTION error code, 100
CR_UNKNOWN_ERROR error code, 99
CR_UNKNOWN_HOST error code, 99
CR_UNSUPPORTED_PARAM_TYPE error code, 101
CR_UNUSED_1 error code, 102
CR_VERSION_ERROR error code, 99
CR_WRONG_HOST_INFO error code, 99
CR_WRONG_LICENSE error code, 101
E
EE_BADCLOSE error code, 105
EE_CANTCREATEFILE error code, 105
EE_CANTLOCK error code, 105
EE_CANTUNLOCK error code, 105
EE_CANT_CHSIZE error code, 106
EE_CANT_MKDIR error code, 106
EE_CANT_OPEN_STREAM error code, 106
EE_CANT_READLINK error code, 106
EE_CANT_SEEK error code, 107
EE_CANT_SYMLINK error code, 106
EE_CAPACITY_EXCEEDED error code, 107
EE_CHANGE_OWNERSHIP error code, 107
EE_CHANGE_PERMISSIONS error code, 107
EE_DELETE error code, 105
EE_DIR error code, 105
EE_DISK_FULL error code, 106
EE_EOFERR error code, 105
EE_FILENOTFOUND error code, 107
EE_FILE_NOT_CLOSED error code, 107
EE_GETWD error code, 106
EE_LINK error code, 105
EE_LINK_WARNING error code, 106
EE_OPEN_WARNING error code, 106
EE_OUTOFMEMORY error code, 105
EE_OUT_OF_FILERESOURCES error code, 106
EE_READ error code, 105
EE_REALPATH error code, 106
109
EE_SETWD error code, 106
EE_STAT error code, 105
EE_SYNC error code, 106
EE_UNKNOWN_CHARSET error code, 106
EE_UNKNOWN_COLLATION error code, 106
EE_WRITE error code, 105
error code
CR_ALREADY_CONNECTED, 103
CR_AUTH_PLUGIN_CANNOT_LOAD, 103
CR_AUTH_PLUGIN_ERR, 103
CR_CANT_READ_CHARSET, 100
CR_COMMANDS_OUT_OF_SYNC, 100
CR_CONNECTION_ERROR, 99
CR_CONN_HOST_ERROR, 99
CR_CONN_UNKNOW_PROTOCOL, 102
CR_DATA_TRUNCATED, 101
CR_DUPLICATE_CONNECTION_ATTR, 103
CR_EMBEDDED_CONNECTION, 100
CR_FETCH_CANCELED, 103
CR_INSECURE_API_ERR, 103
CR_INVALID_BUFFER_USE, 101
CR_INVALID_CLIENT_CHARSET, 104
CR_INVALID_CONN_HANDLE, 102
CR_INVALID_PARAMETER_NO, 101
CR_IPSOCK_ERROR, 99
CR_LOCALHOST_CONNECTION, 100
CR_MALFORMED_PACKET, 101
CR_NAMEDPIPEOPEN_ERROR, 100
CR_NAMEDPIPESETSTATE_ERROR, 100
CR_NAMEDPIPEWAIT_ERROR, 100
CR_NAMEDPIPE_CONNECTION, 100
CR_NET_PACKET_TOO_LARGE, 100
CR_NEW_STMT_METADATA, 103
CR_NOT_IMPLEMENTED, 103
CR_NO_DATA, 103
CR_NO_PARAMETERS_EXISTS, 101
CR_NO_PREPARE_STMT, 101
CR_NO_RESULT_SET, 103
CR_NO_STMT_METADATA, 103
CR_NULL_POINTER, 101
CR_OUT_OF_MEMORY, 99
CR_PARAMS_NOT_BOUND, 101
CR_PROBE_MASTER_CONNECT, 101
CR_PROBE_SLAVE_CONNECT, 101
CR_PROBE_SLAVE_HOSTS, 100
CR_PROBE_SLAVE_STATUS, 100
CR_SECURE_AUTH, 102
CR_SERVER_GONE_ERROR, 99
CR_SERVER_HANDSHAKE_ERR, 100
CR_SERVER_LOST, 100
CR_SERVER_LOST_EXTENDED, 103
CR_SHARED_MEMORY_CONNECTION, 102
CR_SHARED_MEMORY_CONNECT_ABANDONED_ERROR,
102
CR_SHARED_MEMORY_CONNECT_ANSWER_ERROR,
102
CR_SHARED_MEMORY_CONNECT_FILE_MAP_ERROR,
102
CR_SHARED_MEMORY_CONNECT_MAP_ERROR,
102
CR_SHARED_MEMORY_CONNECT_REQUEST_ERROR,
102
CR_SHARED_MEMORY_CONNECT_SET_ERROR,
102
CR_SHARED_MEMORY_EVENT_ERROR, 102
CR_SHARED_MEMORY_FILE_MAP_ERROR, 102
CR_SHARED_MEMORY_MAP_ERROR, 102
CR_SOCKET_CREATE_ERROR, 99
CR_SSL_CONNECTION_ERROR, 101
CR_STMT_CLOSED, 103
CR_TCP_CONNECTION, 100
CR_UNKNOWN_ERROR, 99
CR_UNKNOWN_HOST, 99
CR_UNSUPPORTED_PARAM_TYPE, 101
CR_UNUSED_1, 102
CR_VERSION_ERROR, 99
CR_WRONG_HOST_INFO, 99
CR_WRONG_LICENSE, 101
EE_BADCLOSE, 105
EE_CANTCREATEFILE, 105
EE_CANTLOCK, 105
EE_CANTUNLOCK, 105
EE_CANT_CHSIZE, 106
EE_CANT_MKDIR, 106
EE_CANT_OPEN_STREAM, 106
EE_CANT_READLINK, 106
EE_CANT_SEEK, 107
EE_CANT_SYMLINK, 106
EE_CAPACITY_EXCEEDED, 107
EE_CHANGE_OWNERSHIP, 107
EE_CHANGE_PERMISSIONS, 107
EE_DELETE, 105
EE_DIR, 105
EE_DISK_FULL, 106
EE_EOFERR, 105
EE_FILENOTFOUND, 107
EE_FILE_NOT_CLOSED, 107
EE_GETWD, 106
EE_LINK, 105
EE_LINK_WARNING, 106
EE_OPEN_WARNING, 106
EE_OUTOFMEMORY, 105
EE_OUT_OF_FILERESOURCES, 106
EE_READ, 105
EE_REALPATH, 106
EE_SETWD, 106
EE_STAT, 105
EE_SYNC, 106
110
EE_UNKNOWN_CHARSET, 106
EE_UNKNOWN_COLLATION, 106
EE_WRITE, 105
ER_ABORTING_CONNECTION, 14
ER_ACCESS_DENIED_CHANGE_USER_ERROR,
69
ER_ACCESS_DENIED_ERROR, 7
ER_ACCESS_DENIED_NO_PASSWORD_ERROR,
53
ER_ACCOUNT_HAS_BEEN_LOCKED, 84
ER_ADD_PARTITION_NO_NEW_PARTITION, 40
ER_ADD_PARTITION_SUBPART_ERROR, 40
ER_ADMIN_WRONG_MRG_TABLE, 37
ER_AES_INVALID_IV, 70
ER_AES_INVALID_KDF_ITERATIONS, 98
ER_AES_INVALID_KDF_NAME, 97
ER_AES_INVALID_KDF_OPTION_SIZE, 98
ER_AGGREGATE_IN_ORDER_NOT_SELECT, 78
ER_AGGREGATE_ORDER_FOR_UNION, 73
ER_AGGREGATE_ORDER_NON_AGG_QUERY, 74
ER_ALTER_FILEGROUP_FAILED, 41
ER_ALTER_INFO, 10
ER_ALTER_OPERATION_NOT_SUPPORTED, 65
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON,
65
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_AUTOINC,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_CHANGE_FTS,
67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COLUMN_TYPE,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COPY,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_CHECK,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_RENAME,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FTS,
67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_GIS,
77
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_HIDDEN_FTS,
67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_IGNORE,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOPK,
66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOT_NULL,
67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_PARTITION,
66
ER_AMBIGUOUS_FIELD_TERM, 37
ER_AUDIT_API_ABORT, 90
ER_AUDIT_LOG_COULD_NOT_CREATE_AES_KEY,
94
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_CANNOT_BE_95
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_HAS_NOT_BE94
ER_AUDIT_LOG_HAS_NOT_BEEN_INSTALLED, 95
ER_AUDIT_LOG_HOST_NAME_INVALID_CHARACTER,
96
ER_AUDIT_LOG_JSON_FILTERING_NOT_ENABLED,
95
ER_AUDIT_LOG_JSON_FILTER_DOES_NOT_EXISTS,
96
ER_AUDIT_LOG_JSON_FILTER_NAME_CANNOT_BE_EMPT96
ER_AUDIT_LOG_JSON_FILTER_PARSING_ERROR,
96
ER_AUDIT_LOG_JSON_USER_NAME_CANNOT_BE_EMPTY96
ER_AUDIT_LOG_NO_KEYRING_PLUGIN_INSTALLED,
94
ER_AUDIT_LOG_SUPER_PRIVILEGE_REQUIRED,
95
ER_AUDIT_LOG_UDF_INSUFFICIENT_PRIVILEGE,
95
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_COUNT,
95
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_TYPE,
95
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENG95
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENG96
ER_AUDIT_LOG_USER_FIRST_CHARACTER_MUST_BE_AL96
ER_AUDIT_LOG_USER_NAME_INVALID_CHARACTER,
96
ER_AUTOINC_READ_FAILED, 36
ER_AUTO_CONVERT, 21
ER_AUTO_INCREMENT_CONFLICT, 68
ER_AUTO_POSITION_REQUIRES_GTID_MODE_NOT_OFF,
60
ER_AUTO_POSITION_REQUIRES_GTID_MODE_ON,
59
ER_BAD_DB_ERROR, 7
ER_BAD_FIELD_ERROR, 8
ER_BAD_FT_COLUMN, 24
ER_BAD_HOST_ERROR, 6
ER_BAD_LOG_STATEMENT, 44
ER_BAD_NULL_ERROR, 7
ER_BAD_SLAVE, 18
ER_BAD_SLAVE_AUTO_POSITION, 59
ER_BAD_SLAVE_UNTIL_COND, 23
ER_BAD_TABLE_ERROR, 7
111
ER_BASE64_DECODE_ERROR, 44
ER_BEFORE_DML_VALIDATION_ERROR, 81
ER_BINLOG_CACHE_SIZE_GREATER_THAN_MAX,
56
ER_BINLOG_CREATE_ROUTINE_NEED_SUPER,
33
ER_BINLOG_LOGGING_IMPOSSIBLE, 46
ER_BINLOG_LOGICAL_CORRUPTION, 68
ER_BINLOG_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINE,
51
ER_BINLOG_PURGE_EMFILE, 45
ER_BINLOG_PURGE_FATAL_ERR, 30
ER_BINLOG_PURGE_PROHIBITED, 30
ER_BINLOG_READ_EVENT_CHECKSUM_FAILURE,
57
ER_BINLOG_ROW_ENGINE_AND_STMT_ENGINE,
50
ER_BINLOG_ROW_INJECTION_AND_STMT_ENGINE,
50
ER_BINLOG_ROW_INJECTION_AND_STMT_MODE,
51
ER_BINLOG_ROW_LOGGING_FAILED, 41
ER_BINLOG_ROW_MODE_AND_STMT_ENGINE, 50
ER_BINLOG_ROW_RBR_TO_SBR, 41
ER_BINLOG_ROW_WRONG_TABLE_DEF, 41
ER_BINLOG_STMT_CACHE_SIZE_GREATER_THAN_MAX,
57
ER_BINLOG_STMT_MODE_AND_NO_REPL_TABLES,
65
ER_BINLOG_STMT_MODE_AND_ROW_ENGINE, 51
ER_BINLOG_UNSAFE_AND_STMT_ENGINE, 50
ER_BINLOG_UNSAFE_AUTOINC_COLUMNS, 51
ER_BINLOG_UNSAFE_AUTOINC_NOT_FIRST, 55
ER_BINLOG_UNSAFE_CREATE_IGNORE_SELECT,
55
ER_BINLOG_UNSAFE_CREATE_REPLACE_SELECT,
55
ER_BINLOG_UNSAFE_CREATE_SELECT_AUTOINC,
55
ER_BINLOG_UNSAFE_FULLTEXT_PLUGIN, 71
ER_BINLOG_UNSAFE_INSERT_IGNORE_SELECT,
54
ER_BINLOG_UNSAFE_INSERT_SELECT_UPDATE,
54
ER_BINLOG_UNSAFE_INSERT_TWO_KEYS, 55
ER_BINLOG_UNSAFE_LIMIT, 51
ER_BINLOG_UNSAFE_MIXED_STATEMENT, 53
ER_BINLOG_UNSAFE_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINE,
53
ER_BINLOG_UNSAFE_NONTRANS_AFTER_TRANS,
51
ER_BINLOG_UNSAFE_REPLACE_SELECT, 54
ER_BINLOG_UNSAFE_ROUTINE, 33
ER_BINLOG_UNSAFE_STATEMENT, 45
ER_BINLOG_UNSAFE_SYSTEM_FUNCTION, 51
ER_BINLOG_UNSAFE_SYSTEM_TABLE, 51
ER_BINLOG_UNSAFE_SYSTEM_VARIABLE, 51
ER_BINLOG_UNSAFE_UDF, 51
ER_BINLOG_UNSAFE_UPDATE_IGNORE, 55
ER_BINLOG_UNSAFE_WRITE_AUTOINC_SELECT,
55
ER_BINLOG_UNSAFE_XA, 94
ER_BLOBS_AND_NO_TERMINATED, 10
ER_BLOB_CANT_HAVE_DEFAULT, 11
ER_BLOB_FIELD_IN_PART_FUNC_ERROR, 39
ER_BLOB_KEY_WITHOUT_LENGTH, 16
ER_BLOB_USED_AS_KEY, 9
ER_BOOST_GEOMETRY_CENTROID_EXCEPTION,
75
ER_BOOST_GEOMETRY_EMPTY_INPUT_EXCEPTION,
75
ER_BOOST_GEOMETRY_INCONSISTENT_TURNS_EXCEPTION85
ER_BOOST_GEOMETRY_OVERLAY_INVALID_INPUT_EXCEPT75
ER_BOOST_GEOMETRY_SELF_INTERSECTION_POINT_EXCE75
ER_BOOST_GEOMETRY_TURN_INFO_EXCEPTION,
75
ER_BOOST_GEOMETRY_UNKNOWN_EXCEPTION,
75
ER_BUFPOOL_RESIZE_INPROGRESS, 90
ER_CANNOT_ADD_FOREIGN, 19
ER_CANNOT_ADD_FOREIGN_BASE_COL_STORED,
93
ER_CANNOT_ADD_FOREIGN_BASE_COL_VIRTUAL,
91
ER_CANNOT_CREATE_VIRTUAL_INDEX_CONSTRAINT,
91
ER_CANNOT_DISCARD_TEMPORARY_TABLE, 71
ER_CANNOT_FIND_KEY_IN_KEYRING, 92
ER_CANNOT_LOAD_FROM_TABLE_V2, 55
ER_CANNOT_LOG_PARTIAL_DROP_DATABASE_WITH_GTID,
80
ER_CANNOT_USER, 31
ER_CANT_ACTIVATE_LOG, 44
ER_CANT_AGGREGATE_2COLLATIONS, 23
ER_CANT_AGGREGATE_3COLLATIONS, 23
ER_CANT_AGGREGATE_NCOLLATIONS, 23
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION,
59
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION_WHEN_G59
ER_CANT_CHANGE_TX_CHARACTERISTICS, 44
ER_CANT_CREATE_DB, 4
ER_CANT_CREATE_FEDERATED_TABLE, 34
ER_CANT_CREATE_FILE, 3
ER_CANT_CREATE_GEOMETRY_OBJECT, 33
112
ER_CANT_CREATE_HANDLER_FILE, 39
ER_CANT_CREATE_SROUTINE, 46
ER_CANT_CREATE_TABLE, 4
ER_CANT_CREATE_THREAD, 13
ER_CANT_CREATE_USER_WITH_GRANT, 32
ER_CANT_DELETE_FILE, 4
ER_CANT_DO_IMPLICIT_COMMIT_IN_TRX_WHEN_GTID_NEXT_IS_SET,
60
ER_CANT_DO_THIS_DURING_AN_TRANSACTION,
16
ER_CANT_DROP_FIELD_OR_KEY, 10
ER_CANT_EXECUTE_IN_READ_ONLY_TRANSACTION,
61
ER_CANT_FIND_DL_ENTRY, 13
ER_CANT_FIND_SYSTEM_REC, 4
ER_CANT_FIND_UDF, 12
ER_CANT_GET_STAT, 4
ER_CANT_GET_WD, 4
ER_CANT_INITIALIZE_UDF, 12
ER_CANT_LOCK, 4
ER_CANT_LOCK_LOG_TABLE, 43
ER_CANT_OPEN_ERROR_LOG, 97
ER_CANT_OPEN_FILE, 5
ER_CANT_OPEN_LIBRARY, 13
ER_CANT_READ_DIR, 5
ER_CANT_REMOVE_ALL_FIELDS, 10
ER_CANT_RENAME_LOG_TABLE, 44
ER_CANT_REOPEN_TABLE, 13
ER_CANT_REPLICATE_ANONYMOUS_WITH_AUTO_POSITION,
83
ER_CANT_REPLICATE_ANONYMOUS_WITH_GTID_MODE_ON,
83
ER_CANT_REPLICATE_GTID_WITH_GTID_MODE_OFF,
84
ER_CANT_RESET_MASTER, 92
ER_CANT_SET_ENFORCE_GTID_CONSISTENCY_ON_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS,
84
ER_CANT_SET_GTID_MODE, 83
ER_CANT_SET_GTID_NEXT_LIST_TO_NON_NULL_WHEN_GTID_MODE_IS_OFF,
60
ER_CANT_SET_GTID_NEXT_TO_ANONYMOUS_WHEN_GTID_MODE_IS_ON,
60
ER_CANT_SET_GTID_NEXT_TO_GTID_WHEN_GTID_MODE_IS_OFF,
60
ER_CANT_SET_GTID_NEXT_WHEN_OWNING_GTID,
61
ER_CANT_SET_GTID_PURGED_WHEN_GTID_EXECUTED_IS_NOT_EMPTY,
65
ER_CANT_SET_GTID_PURGED_WHEN_GTID_MODE_IS_OFF,
65
ER_CANT_SET_GTID_PURGED_WHEN_OWNED_GTIDS_IS_NOT_EMPTY,
65
ER_CANT_SET_VARIABLE_WHEN_OWNING_GTID,
87
ER_CANT_SET_WD, 5
ER_CANT_START_SERVER_NAMED_PIPE, 97
ER_CANT_UPDATE_TABLE_IN_CREATE_TABLE_SELECT,
57
ER_CANT_UPDATE_USED_TABLE_IN_SF_OR_TRG,
35
ER_CANT_UPDATE_WITH_READLOCK, 20
ER_CANT_USE_AUTO_POSITION_WITH_GTID_MODE_OFF83
ER_CANT_USE_OPTION_HERE, 20
ER_CANT_WAIT_FOR_EXECUTED_GTID_SET_WHILE_OW91
ER_CANT_WRITE_LOCK_LOG_TABLE, 43
ER_CAPACITY_EXCEEDED, 90
ER_CAPACITY_EXCEEDED_IN_PARSER, 92
ER_CAPACITY_EXCEEDED_IN_RANGE_OPTIMIZER,
90
ER_CHANGE_MASTER_PASSWORD_LENGTH, 77
ER_CHANGE_RPL_INFO_REPOSITORY_FAILURE,
57
ER_CHECKREAD, 5
ER_CHECK_NOT_IMPLEMENTED, 16
ER_CHECK_NO_SUCH_TABLE, 16
ER_COALESCE_ONLY_ON_HASH_PARTITION, 39
ER_COALESCE_PARTITION_NO_PARTITION, 40
ER_COLLATION_CHARSET_MISMATCH, 22
ER_COLUMNACCESS_DENIED_ERROR, 14
ER_COL_COUNT_DOESNT_MATCH_CORRUPTED_V2,
62
ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE,
43
ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE_V271
ER_COMMIT_NOT_ALLOWED_IN_SF_OR_TRG, 33
ER_COND_ITEM_TOO_LONG, 49
ER_CONFLICTING_DECLARATIONS, 25
ER_CONFLICT_FN_PARSE_ERROR, 48
ER_CONNECT_TO_FOREIGN_DATA_SOURCE, 34
ER_CONNECT_TO_MASTER, 19
ER_CONSECUTIVE_REORG_PARTITIONS, 40
ER_CON_COUNT_ERROR, 6
ER_CORRUPT_HELP_DB, 21
ER_COULD_NOT_REINITIALIZE_AUDIT_LOG_FILTERS,
95
ER_CRASHED_ON_REPAIR, 17
ER_CRASHED_ON_USAGE, 17
ER_CREATE_DB_WITH_READ_LOCK, 18
ER_CREATE_FILEGROUP_FAILED, 41
ER_CUT_VALUE_GROUP_CONCAT, 22
ER_CYCLIC_REFERENCE, 21
ER_DATABASE_NAME, 48
ER_DATA_OUT_OF_RANGE, 52
ER_DATA_TOO_LONG, 32
ER_DATETIME_FUNCTION_OVERFLOW, 34
113
ER_DA_INVALID_CONDITION_NUMBER, 58
ER_DBACCESS_DENIED_ERROR, 6
ER_DB_CREATE_EXISTS, 4
ER_DB_DROP_DELETE, 4
ER_DB_DROP_EXISTS, 4
ER_DB_DROP_RMDIR, 4
ER_DDL_LOG_ERROR, 43
ER_DEBUG_SYNC_HIT_LIMIT, 49
ER_DEBUG_SYNC_TIMEOUT, 49
ER_DELAYED_NOT_SUPPORTED, 47
ER_DEPENDENT_BY_GENERATED_COLUMN, 83
ER_DEPRECATED_TLS_VERSION_SESSION, 97
ER_DERIVED_MUST_HAVE_ALIAS, 21
ER_DIFF_GROUPS_PROC, 31
ER_DIMENSION_UNSUPPORTED, 78
ER_DISABLED_STORAGE_ENGINE, 89
ER_DISCARD_FK_CHECKS_RUNNING, 62
ER_DISK_FULL, 5
ER_DIVISION_BY_ZERO, 29
ER_DONT_SUPPORT_SLAVE_PRESERVE_COMMIT_ORDER,
74
ER_DROP_DB_WITH_READ_LOCK, 18
ER_DROP_FILEGROUP_FAILED, 41
ER_DROP_INDEX_FK, 42
ER_DROP_LAST_PARTITION, 39
ER_DROP_PARTITION_NON_EXISTENT, 39
ER_DROP_USER, 23
ER_DUMP_NOT_IMPLEMENTED, 17
ER_DUPLICATED_VALUE_IN_TYPE, 24
ER_DUP_ARGUMENT, 20
ER_DUP_ENTRY, 8
ER_DUP_ENTRY_AUTOINCREMENT_CASE, 44
ER_DUP_ENTRY_WITH_KEY_NAME, 45
ER_DUP_FIELDNAME, 8
ER_DUP_INDEX, 64
ER_DUP_KEY, 5
ER_DUP_KEYNAME, 8
ER_DUP_LIST_ENTRY, 73
ER_DUP_SIGNAL_SET, 49
ER_DUP_UNIQUE, 16
ER_DUP_UNKNOWN_IN_INDEX, 67
ER_EMPTY_QUERY, 8
ER_ENGINE_OUT_OF_MEMORY, 72
ER_ERROR_DURING_CHECKPOINT, 16
ER_ERROR_DURING_COMMIT, 16
ER_ERROR_DURING_FLUSH_LOGS, 16
ER_ERROR_DURING_ROLLBACK, 16
ER_ERROR_IN_TRIGGER_BODY, 54
ER_ERROR_IN_UNKNOWN_TRIGGER_BODY, 54
ER_ERROR_ON_CLOSE, 5
ER_ERROR_ON_MASTER, 71
ER_ERROR_ON_MODIFYING_GTID_EXECUTED_TABLE,
91
ER_ERROR_ON_READ, 5
ER_ERROR_ON_RENAME, 5
ER_ERROR_ON_WRITE, 5
ER_ERROR_WHEN_EXECUTING_COMMAND, 19
ER_EVENTS_DB_ERROR, 44
ER_EVENT_ALREADY_EXISTS, 41
ER_EVENT_CANNOT_ALTER_IN_THE_PAST, 45
ER_EVENT_CANNOT_CREATE_IN_THE_PAST, 45
ER_EVENT_CANNOT_DELETE, 42
ER_EVENT_CANT_ALTER, 41
ER_EVENT_COMPILE_ERROR, 42
ER_EVENT_DATA_TOO_LONG, 42
ER_EVENT_DOES_NOT_EXIST, 41
ER_EVENT_DROP_FAILED, 41
ER_EVENT_ENDS_BEFORE_STARTS, 42
ER_EVENT_EXEC_TIME_IN_THE_PAST, 42
ER_EVENT_INTERVAL_NOT_POSITIVE_OR_TOO_BIG,
42
ER_EVENT_INVALID_CREATION_CTX, 46
ER_EVENT_MODIFY_QUEUE_ERROR, 44
ER_EVENT_NEITHER_M_EXPR_NOR_M_AT, 42
ER_EVENT_OPEN_TABLE_FAILED, 42
ER_EVENT_RECURSION_FORBIDDEN, 44
ER_EVENT_SAME_NAME, 42
ER_EVENT_SET_VAR_ERROR, 44
ER_EVENT_STORE_FAILED, 41
ER_EXCEPTIONS_WRITE_ERROR, 48
ER_EXEC_STMT_WITH_OPEN_CURSOR, 33
ER_EXPLAIN_NOT_SUPPORTED, 72
ER_FAILED_READ_FROM_PAR_FILE, 53
ER_FAILED_ROUTINE_BREAK_BINLOG, 33
ER_FEATURE_DISABLED, 24
ER_FEATURE_DISABLED_SEE_DOC, 90
ER_FEATURE_NOT_AVAILABLE, 83
ER_FIELD_IN_ORDER_NOT_SELECT, 78
ER_FIELD_NOT_FOUND_PART_ERROR, 38
ER_FIELD_SPECIFIED_TWICE, 11
ER_FIELD_TYPE_NOT_ALLOWED_AS_PARTITION_FIELD,
50
ER_FILEGROUP_OPTION_ONLY_ONCE, 40
ER_FILE_CORRUPT, 71
ER_FILE_EXISTS_ERROR, 10
ER_FILE_NOT_FOUND, 5
ER_FILE_USED, 5
ER_FILSORT_ABORT, 5
ER_FK_CANNOT_DELETE_PARENT, 64
ER_FK_CANNOT_OPEN_PARENT, 64
ER_FK_COLUMN_CANNOT_CHANGE, 64
ER_FK_COLUMN_CANNOT_CHANGE_CHILD, 64
ER_FK_COLUMN_CANNOT_DROP, 64
ER_FK_COLUMN_CANNOT_DROP_CHILD, 64
ER_FK_COLUMN_NOT_NULL, 64
ER_FK_DEPTH_EXCEEDED, 71
ER_FK_DUP_NAME, 64
ER_FK_FAIL_ADD_SYSTEM, 64
114
ER_FK_INCORRECT_OPTION, 64
ER_FK_NO_INDEX_CHILD, 63
ER_FK_NO_INDEX_PARENT, 63
ER_FLUSH_MASTER_BINLOG_CLOSED, 17
ER_FORBID_SCHEMA_CHANGE, 35
ER_FORCING_CLOSE, 9
ER_FOREIGN_DATA_SOURCE_DOESNT_EXIST,
34
ER_FOREIGN_DATA_STRING_INVALID, 34
ER_FOREIGN_DATA_STRING_INVALID_CANT_CREATE,
34
ER_FOREIGN_DUPLICATE_KEY_OLD_UNUSED, 43
ER_FOREIGN_DUPLICATE_KEY_WITHOUT_CHILD_INFO,
58
ER_FOREIGN_DUPLICATE_KEY_WITH_CHILD_INFO,
58
ER_FOREIGN_KEY_ON_PARTITIONED, 39
ER_FOREIGN_SERVER_DOESNT_EXIST, 37
ER_FOREIGN_SERVER_EXISTS, 37
ER_FORM_NOT_FOUND, 5
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF,
60
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF__UNUSED,
61
ER_FOUND_MISSING_GTIDS, 70
ER_FPARSER_BAD_HEADER, 28
ER_FPARSER_EOF_IN_COMMENT, 28
ER_FPARSER_EOF_IN_UNKNOWN_PARAMETER,
28
ER_FPARSER_ERROR_IN_PARAMETER, 28
ER_FPARSER_TOO_BIG_FILE, 27
ER_FRM_UNKNOWN_TYPE, 28
ER_FSEEK_FAIL, 30
ER_FT_MATCHING_KEY_NOT_FOUND, 17
ER_FULLTEXT_NOT_SUPPORTED_WITH_PARTITIONING,
58
ER_FUNCTION_NOT_DEFINED, 13
ER_FUNC_INEXISTENT_NAME_COLLISION, 48
ER_GENERATED_COLUMN_FUNCTION_IS_NOT_ALLOWED,
82
ER_GENERATED_COLUMN_NON_PRIOR, 82
ER_GENERATED_COLUMN_REF_AUTO_INC, 83
ER_GET_ERRMSG, 25
ER_GET_ERRNO, 6
ER_GET_STACKED_DA_WITHOUT_ACTIVE_HANDLER,
71
ER_GET_TEMPORARY_ERRMSG, 25
ER_GIS_DATA_WRONG_ENDIANESS, 76
ER_GIS_DIFFERENT_SRIDS, 74
ER_GIS_INVALID_DATA, 75
ER_GIS_MAX_POINTS_IN_GEOMETRY_OVERFLOWED,
86
ER_GIS_UNKNOWN_ERROR, 74
ER_GIS_UNKNOWN_EXCEPTION, 74
ER_GIS_UNSUPPORTED_ARGUMENT, 74
ER_GLOBAL_VARIABLE, 20
ER_GNO_EXHAUSTED, 59
ER_GOT_SIGNAL, 9
ER_GRANT_PLUGIN_USER_EXISTS, 53
ER_GRANT_WRONG_HOST_OR_USER, 14
ER_GROUPING_ON_TIMESTAMP_IN_DST, 97
ER_GROUP_REPLICATION_APPLIER_INIT_ERROR,
81
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_JOI81
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_SES81
ER_GROUP_REPLICATION_CONFIGURATION, 81
ER_GROUP_REPLICATION_MAX_GROUP_SIZE, 93
ER_GROUP_REPLICATION_RUNNING, 81
ER_GROUP_REPLICATION_STOP_APPLIER_THREAD_TIM81
ER_GTID_EXECUTED_WAS_CHANGED, 65
ER_GTID_MODE_2_OR_3_REQUIRES_ENFORCE_GTID_CO60
ER_GTID_MODE_CAN_ONLY_CHANGE_ONE_STEP_AT_A_61
ER_GTID_MODE_OFF, 77
ER_GTID_MODE_ON_REQUIRES_ENFORCE_GTID_CONSI60
ER_GTID_MODE_REQUIRES_BINLOG, 60
ER_GTID_NEXT_CANT_BE_AUTOMATIC_IF_GTID_NEXT_L59
ER_GTID_NEXT_IS_NOT_IN_GTID_NEXT_LIST, 58
ER_GTID_NEXT_TYPE_UNDEFINED_GROUP, 65
ER_GTID_PURGED_WAS_CHANGED, 65
ER_GTID_UNSAFE_BINLOG_SPLITTABLE_STATEMENT_AN70
ER_GTID_UNSAFE_CREATE_DROP_TEMPORARY_TABLE_61
ER_GTID_UNSAFE_CREATE_SELECT, 61
ER_GTID_UNSAFE_NON_TRANSACTIONAL_TABLE,
61
ER_HANDSHAKE_ERROR, 6
ER_HASHCHK, 3
ER_HOSTNAME, 37
ER_HOST_IS_BLOCKED, 13
ER_HOST_NOT_PRIVILEGED, 13
ER_IDENT_CAUSES_TOO_LONG_PATH, 67
ER_ILLEGAL_GRANT_FOR_TABLE, 14
ER_ILLEGAL_HA, 6
ER_ILLEGAL_HA_CREATE_OPTION, 37
ER_ILLEGAL_REFERENCE, 21
ER_ILLEGAL_USER_VAR, 77
ER_ILLEGAL_VALUE_FOR_TYPE, 29
ER_INCONSISTENT_ERROR, 71
ER_INCONSISTENT_PARTITION_INFO_ERROR, 38
115
ER_INCONSISTENT_TYPE_OF_FUNCTIONS_ERROR,
38
ER_INCORRECT_GLOBAL_LOCAL_VAR, 21
ER_INCORRECT_TYPE, 77
ER_INDEX_COLUMN_TOO_LONG, 54
ER_INDEX_CORRUPT, 54
ER_INDEX_REBUILD, 17
ER_INNODB_FORCED_RECOVERY, 69
ER_INNODB_FT_AUX_NOT_HEX_ID, 69
ER_INNODB_FT_LIMIT, 62
ER_INNODB_FT_WRONG_DOCID_COLUMN, 62
ER_INNODB_FT_WRONG_DOCID_INDEX, 62
ER_INNODB_IMPORT_ERROR, 63
ER_INNODB_INDEX_CORRUPT, 63
ER_INNODB_NO_FT_TEMP_TABLE, 62
ER_INNODB_NO_FT_USES_PARSER, 68
ER_INNODB_ONLINE_LOG_TOO_BIG, 62
ER_INNODB_READ_ONLY, 69
ER_INNODB_UNDO_LOG_FULL, 72
ER_INSECURE_CHANGE_MASTER, 58
ER_INSECURE_PLAIN_TEXT, 58
ER_INSERT_INFO, 10
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_DIRECT,
52
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_FORMAT,
52
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_SQL_LOG_BIN,
53
ER_INTERNAL_ERROR, 63
ER_INVALID_ARGUMENT_FOR_LOGARITHM, 73
ER_INVALID_CAST_TO_JSON, 88
ER_INVALID_CHARACTER_STRING, 25
ER_INVALID_DEFAULT, 8
ER_INVALID_ENCRYPTION_OPTION, 92
ER_INVALID_FIELD_SIZE, 72
ER_INVALID_GEOJSON_MISSING_MEMBER, 78
ER_INVALID_GEOJSON_UNSPECIFIED, 78
ER_INVALID_GEOJSON_WRONG_TYPE, 78
ER_INVALID_GROUP_FUNC_USE, 11
ER_INVALID_JSON_BINARY_DATA, 87
ER_INVALID_JSON_CHARSET, 87
ER_INVALID_JSON_CHARSET_IN_FUNCTION, 87
ER_INVALID_JSON_DATA, 78
ER_INVALID_JSON_PATH, 87
ER_INVALID_JSON_PATH_ARRAY_CELL, 90
ER_INVALID_JSON_PATH_CHARSET, 88
ER_INVALID_JSON_PATH_WILDCARD, 88
ER_INVALID_JSON_TEXT, 87
ER_INVALID_JSON_TEXT_IN_PARAM, 87
ER_INVALID_JSON_VALUE_FOR_CAST, 89
ER_INVALID_ON_UPDATE, 24
ER_INVALID_RPL_WILD_TABLE_FILTER_PATTERN,
78
ER_INVALID_TYPE_FOR_JSON, 88
ER_INVALID_USE_OF_NULL, 13
ER_INVALID_YEAR_COLUMN_LENGTH, 63
ER_IO_ERR_LOG_INDEX_READ, 30
ER_IO_READ_ERROR, 63
ER_IO_WRITE_ERROR, 63
ER_IPSOCK_ERROR, 9
ER_JSON_BAD_ONE_OR_ALL_ARG, 88
ER_JSON_DOCUMENT_NULL_KEY, 89
ER_JSON_DOCUMENT_TOO_DEEP, 89
ER_JSON_KEY_TOO_BIG, 88
ER_JSON_USED_AS_KEY, 88
ER_JSON_VACUOUS_PATH, 88
ER_JSON_VALUE_TOO_BIG, 88
ER_KEYRING_ACCESS_DENIED_ERROR, 94
ER_KEYRING_AWS_UDF_AWS_KMS_ERROR, 93
ER_KEYRING_MIGRATION_FAILURE, 94
ER_KEYRING_MIGRATION_STATUS, 94
ER_KEYRING_UDF_KEYRING_SERVICE_ERROR,
92
ER_KEY_BASED_ON_GENERATED_COLUMN, 82
ER_KEY_COLUMN_DOES_NOT_EXITS, 9
ER_KEY_DOES_NOT_EXITS, 16
ER_KEY_NOT_FOUND, 6
ER_KEY_PART_0, 31
ER_KEY_REF_DO_NOT_MATCH_TABLE_REF, 21
ER_KILL_DENIED_ERROR, 10
ER_LIMITED_PART_RANGE, 40
ER_LIST_OF_FIELDS_ONLY_IN_HASH_ERROR, 38
ER_LOAD_DATA_INVALID_COLUMN, 47
ER_LOAD_DATA_INVALID_COLUMN_UNUSED, 47
ER_LOAD_FROM_FIXED_SIZE_ROWS_TO_VAR,
32
ER_LOAD_INFO, 10
ER_LOCAL_VARIABLE, 20
ER_LOCKING_SERVICE_DEADLOCK, 86
ER_LOCKING_SERVICE_TIMEOUT, 86
ER_LOCKING_SERVICE_WRONG_NAME, 86
ER_LOCK_ABORTED, 52
ER_LOCK_DEADLOCK, 19
ER_LOCK_OR_ACTIVE_TRANSACTION, 17
ER_LOCK_REFUSED_BY_ENGINE, 91
ER_LOCK_TABLE_FULL, 18
ER_LOCK_WAIT_TIMEOUT, 18
ER_LOGGING_PROHIBIT_CHANGING_OF, 31
ER_LOG_IN_USE, 30
ER_LOG_PURGE_NO_FILE, 47
ER_LOG_PURGE_UNKNOWN_ERR, 30
ER_MALFORMED_DEFINER, 35
ER_MALFORMED_GTID_SET_ENCODING, 59
ER_MALFORMED_GTID_SET_SPECIFICATION, 59
ER_MALFORMED_GTID_SPECIFICATION, 59
ER_MALFORMED_PACKET, 64
ER_MASTER, 17
ER_MASTER_DELAY_VALUE_OUT_OF_RANGE, 56
116
ER_MASTER_FATAL_ERROR_READING_BINLOG,
20
ER_MASTER_HAS_PURGED_REQUIRED_GTIDS,
61
ER_MASTER_INFO, 18
ER_MASTER_KEY_ROTATION_BINLOG_FAILED,
91
ER_MASTER_KEY_ROTATION_ERROR_BY_SE, 91
ER_MASTER_KEY_ROTATION_NOT_SUPPORTED_BY_SE,
91
ER_MASTER_KEY_ROTATION_SE_UNAVAILABLE,
92
ER_MASTER_NET_READ, 17
ER_MASTER_NET_WRITE, 17
ER_MAXVALUE_IN_VALUES_IN, 50
ER_MAX_PREPARED_STMT_COUNT_REACHED,
36
ER_MESSAGE_AND_STATEMENT, 51
ER_MISSING_HA_CREATE_OPTION, 72
ER_MISSING_KEY, 70
ER_MISSING_SKIP_SLAVE, 23
ER_MIXING_NOT_ALLOWED, 20
ER_MIX_HANDLER_ERROR, 38
ER_MIX_OF_GROUP_FUNC_AND_FIELDS, 14
ER_MIX_OF_GROUP_FUNC_AND_FIELDS_V2, 80
ER_MTS_CANT_PARALLEL, 57
ER_MTS_CHANGE_MASTER_CANT_RUN_WITH_GAPS,
62
ER_MTS_EVENT_BIGGER_PENDING_JOBS_SIZE_MAX,
68
ER_MTS_FEATURE_IS_NOT_SUPPORTED, 57
ER_MTS_INCONSISTENT_DATA, 58
ER_MTS_RECOVERY_FAILURE, 62
ER_MTS_RESET_WORKERS, 62
ER_MTS_UPDATED_DBS_GREATER_MAX, 57
ER_MULTIPLE_DEF_CONST_IN_LIST_PART_ERROR,
38
ER_MULTIPLE_PRI_KEY, 8
ER_MULTI_UPDATE_KEY_CONFLICT, 54
ER_MUST_CHANGE_PASSWORD, 63
ER_MUST_CHANGE_PASSWORD_LOGIN, 67
ER_M_BIGGER_THAN_D, 33
ER_NAME_BECOMES_EMPTY, 37
ER_NATIVE_FCT_NAME_COLLISION, 45
ER_NDB_CANT_SWITCH_BINLOG_FORMAT, 43
ER_NDB_REPLICATION_SCHEMA_ERROR, 48
ER_NEED_REPREPARE, 47
ER_NETWORK_READ_EVENT_CHECKSUM_FAILURE,
57
ER_NET_ERROR_ON_WRITE, 15
ER_NET_FCNTL_ERROR, 15
ER_NET_OK_PACKET_TOO_LARGE, 78
ER_NET_PACKETS_OUT_OF_ORDER, 15
ER_NET_PACKET_TOO_LARGE, 14
ER_NET_READ_ERROR, 15
ER_NET_READ_ERROR_FROM_PIPE, 15
ER_NET_READ_INTERRUPTED, 15
ER_NET_UNCOMPRESS_ERROR, 15
ER_NET_WRITE_INTERRUPTED, 15
ER_NEVER_USED, 46
ER_NEW_ABORTING_CONNECTION, 17
ER_NISAMCHK, 3
ER_NO, 3
ER_NONEXISTING_GRANT, 14
ER_NONEXISTING_PROC_GRANT, 32
ER_NONEXISTING_TABLE_GRANT, 14
ER_NONUNIQ_TABLE, 8
ER_NONUPDATEABLE_COLUMN, 28
ER_NON_DEFAULT_VALUE_FOR_GENERATED_COLUMN,
82
ER_NON_GROUPING_FIELD_USED, 36
ER_NON_INSERTABLE_TABLE, 37
ER_NON_RO_SELECT_DISABLE_TIMER, 73
ER_NON_UNIQ_ERROR, 7
ER_NON_UPDATABLE_TABLE, 24
ER_NORMAL_SHUTDOWN, 9
ER_NOT_ALLOWED_COMMAND, 14
ER_NOT_FORM_FILE, 6
ER_NOT_KEYFILE, 6
ER_NOT_SUPPORTED_AUTH_MODE, 21
ER_NOT_SUPPORTED_YET, 20
ER_NOT_VALID_PASSWORD, 63
ER_NO_BINARY_LOGGING, 30
ER_NO_BINLOG_ERROR, 40
ER_NO_CONST_EXPR_IN_RANGE_OR_LIST_ERROR,
38
ER_NO_DB_ERROR, 7
ER_NO_DEFAULT, 20
ER_NO_DEFAULT_FOR_FIELD, 29
ER_NO_DEFAULT_FOR_VIEW_FIELD, 33
ER_NO_FILE_MAPPING, 31
ER_NO_FORMAT_DESCRIPTION_EVENT_BEFORE_BINLOG46
ER_NO_FT_MATERIALIZED_SUBQUERY, 72
ER_NO_GROUP_FOR_PROC, 31
ER_NO_PARTITION_FOR_GIVEN_VALUE, 40
ER_NO_PARTITION_FOR_GIVEN_VALUE_SILENT,
45
ER_NO_PARTS_ERROR, 39
ER_NO_PERMISSION_TO_CREATE_USER, 19
ER_NO_RAID_COMPILED, 16
ER_NO_REFERENCED_ROW, 19
ER_NO_REFERENCED_ROW_2, 35
ER_NO_SECURE_TRANSPORTS_CONFIGURED,
89
ER_NO_SUCH_INDEX, 9
ER_NO_SUCH_KEY_VALUE, 56
ER_NO_SUCH_PARTITION__UNUSED, 57
117
ER_NO_SUCH_TABLE, 14
ER_NO_SUCH_THREAD, 10
ER_NO_SUCH_USER, 35
ER_NO_TABLES_USED, 10
ER_NO_TRIGGERS_ON_SYSTEM_SCHEMA, 36
ER_NO_UNIQUE_LOGFILE, 11
ER_NULL_COLUMN_IN_INDEX, 12
ER_NULL_IN_VALUES_LESS_THAN, 43
ER_NUMERIC_JSON_VALUE_OUT_OF_RANGE, 88
ER_OBSOLETE_CANNOT_LOAD_FROM_TABLE, 42
ER_OBSOLETE_COL_COUNT_DOESNT_MATCH_CORRUPTED,
42
ER_OLD_FILE_FORMAT, 36
ER_OLD_KEYFILE, 6
ER_OLD_TEMPORALS_UPGRADED, 69
ER_ONLY_FD_AND_RBR_EVENTS_ALLOWED_IN_BINLOG_STATEMENT,
56
ER_ONLY_INTEGERS_ALLOWED, 44
ER_ONLY_ON_RANGE_LIST_PARTITION, 39
ER_OPEN_AS_READONLY, 6
ER_OPERAND_COLUMNS, 21
ER_OPTION_PREVENTS_STATEMENT, 24
ER_ORDER_WITH_PROC, 31
ER_OUTOFMEMORY, 6
ER_OUT_OF_RESOURCES, 6
ER_OUT_OF_SORTMEMORY, 6
ER_PARSE_ERROR, 8
ER_PARTITIONS_MUST_BE_DEFINED_ERROR, 38
ER_PARTITION_CLAUSE_ON_NONPARTITIONED,
57
ER_PARTITION_COLUMN_LIST_ERROR, 50
ER_PARTITION_CONST_DOMAIN_ERROR, 43
ER_PARTITION_ENGINE_DEPRECATED_FOR_TABLE,
93
ER_PARTITION_ENTRY_ERROR, 38
ER_PARTITION_EXCHANGE_DIFFERENT_OPTION,
56
ER_PARTITION_EXCHANGE_FOREIGN_KEY, 56
ER_PARTITION_EXCHANGE_PART_TABLE, 56
ER_PARTITION_EXCHANGE_TEMP_TABLE, 56
ER_PARTITION_FIELDS_TOO_LONG, 50
ER_PARTITION_FUNCTION_FAILURE, 40
ER_PARTITION_FUNCTION_IS_NOT_ALLOWED, 43
ER_PARTITION_FUNC_NOT_ALLOWED_ERROR,
38
ER_PARTITION_INSTEAD_OF_SUBPARTITION, 56
ER_PARTITION_MAXVALUE_ERROR, 37
ER_PARTITION_MERGE_ERROR, 44
ER_PARTITION_MGMT_ON_NONPARTITIONED, 39
ER_PARTITION_NAME, 48
ER_PARTITION_NOT_DEFINED_ERROR, 38
ER_PARTITION_NO_TEMPORARY, 43
ER_PARTITION_REQUIRES_VALUES_ERROR, 37
ER_PARTITION_SUBPARTITION_ERROR, 37
ER_PARTITION_SUBPART_MIX_ERROR, 37
ER_PARTITION_WRONG_NO_PART_ERROR, 38
ER_PARTITION_WRONG_NO_SUBPART_ERROR,
38
ER_PARTITION_WRONG_VALUES_ERROR, 37
ER_PART_STATE_ERROR, 40
ER_PASSWD_LENGTH, 30
ER_PASSWORD_ANONYMOUS_USER, 13
ER_PASSWORD_EXPIRE_ANONYMOUS_USER, 72
ER_PASSWORD_FORMAT, 64
ER_PASSWORD_NOT_ALLOWED, 13
ER_PASSWORD_NO_MATCH, 13
ER_PATH_LENGTH, 52
ER_PLUGGABLE_PROTOCOL_COMMAND_NOT_SUPPORTED,86
ER_PLUGIN_CANNOT_BE_UNINSTALLED, 70
ER_PLUGIN_DELETE_BUILTIN, 47
ER_PLUGIN_FAILED_TO_OPEN_TABLE, 94
ER_PLUGIN_FAILED_TO_OPEN_TABLES, 94
ER_PLUGIN_IS_NOT_LOADED, 40
ER_PLUGIN_IS_PERMANENT, 53
ER_PLUGIN_NO_INSTALL, 55
ER_PLUGIN_NO_UNINSTALL, 55
ER_PREVENTS_VARIABLE_WITHOUT_RBR, 81
ER_PRIMARY_CANT_HAVE_NULL, 16
ER_PROCACCESS_DENIED_ERROR, 30
ER_PROC_AUTO_GRANT_FAIL, 32
ER_PROC_AUTO_REVOKE_FAIL, 32
ER_PS_MANY_PARAM, 31
ER_PS_NO_RECURSION, 35
ER_QUERY_CACHE_DISABLED, 50
ER_QUERY_INTERRUPTED, 26
ER_QUERY_ON_FOREIGN_DATA_SOURCE, 34
ER_QUERY_ON_MASTER, 19
ER_QUERY_TIMEOUT, 73
ER_RANGE_NOT_INCREASING_ERROR, 38
ER_RBR_NOT_AVAILABLE, 44
ER_READY, 9
ER_READ_ONLY_MODE, 65
ER_READ_ONLY_TRANSACTION, 18
ER_RECORD_FILE_FULL, 12
ER_REFERENCED_TRG_DOES_NOT_EXIST, 72
ER_REGEXP_ERROR, 14
ER_RELAY_LOG_FAIL, 30
ER_RELAY_LOG_INIT, 30
ER_REMOVED_SPACES, 36
ER_RENAMED_NAME, 49
ER_REORG_HASH_ONLY_ON_SAME_NO, 39
ER_REORG_NO_PARAM_ERROR, 39
ER_REORG_OUTSIDE_RANGE, 40
ER_REORG_PARTITION_NOT_EXIST, 40
ER_REPLACE_INACCESSIBLE_ROWS, 77
ER_REQUIRES_PRIMARY_KEY, 16
ER_RESERVED_SYNTAX, 30
118
ER_RESIGNAL_WITHOUT_ACTIVE_HANDLER, 49
ER_REVOKE_GRANTS, 23
ER_ROW_DOES_NOT_MATCH_GIVEN_PARTITION_SET,
57
ER_ROW_DOES_NOT_MATCH_PARTITION, 56
ER_ROW_IN_WRONG_PARTITION, 68
ER_ROW_IS_REFERENCED, 19
ER_ROW_IS_REFERENCED_2, 35
ER_ROW_SINGLE_PARTITION_FIELD_ERROR, 50
ER_RPL_INFO_DATA_TOO_LONG, 56
ER_RUN_HOOK_ERROR, 82
ER_SAME_NAME_PARTITION, 40
ER_SAME_NAME_PARTITION_FIELD, 50
ER_SECURE_TRANSPORT_REQUIRED, 89
ER_SELECT_REDUCED, 21
ER_SERVER_ISNT_AVAILABLE, 90
ER_SERVER_IS_IN_SECURE_AUTH_MODE, 23
ER_SERVER_OFFLINE_MODE, 74
ER_SERVER_SHUTDOWN, 7
ER_SESSION_WAS_KILLED, 90
ER_SET_CONSTANTS_ONLY, 18
ER_SET_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS,
84
ER_SET_PASSWORD_AUTH_PLUGIN, 53
ER_SET_STATEMENT_CANNOT_INVOKE_FUNCTION,
59
ER_SHUTDOWN_COMPLETE, 9
ER_SIGNAL_BAD_CONDITION_TYPE, 49
ER_SIGNAL_EXCEPTION, 49
ER_SIGNAL_NOT_FOUND, 49
ER_SIGNAL_WARN, 49
ER_SIZE_OVERFLOW_ERROR, 41
ER_SKIPPING_LOGGED_TRANSACTION, 59
ER_SLAVE_CANT_CREATE_CONVERSION, 52
ER_SLAVE_CHANNEL_DELETE, 79
ER_SLAVE_CHANNEL_DOES_NOT_EXIST, 78
ER_SLAVE_CHANNEL_IO_THREAD_MUST_STOP,
73
ER_SLAVE_CHANNEL_MUST_STOP, 79
ER_SLAVE_CHANNEL_NAME_INVALID_OR_TOO_LONG,
79
ER_SLAVE_CHANNEL_NOT_RUNNING, 79
ER_SLAVE_CHANNEL_OPERATION_NOT_ALLOWED,
87
ER_SLAVE_CHANNEL_SQL_SKIP_COUNTER, 80
ER_SLAVE_CHANNEL_SQL_THREAD_MUST_STOP,
80
ER_SLAVE_CHANNEL_WAS_NOT_RUNNING, 80
ER_SLAVE_CHANNEL_WAS_RUNNING, 79
ER_SLAVE_CONFIGURATION, 61
ER_SLAVE_CONVERSION_FAILED, 52
ER_SLAVE_CORRUPT_EVENT, 47
ER_SLAVE_CREATE_EVENT_FAILURE, 46
ER_SLAVE_FATAL_ERROR, 45
ER_SLAVE_HAS_MORE_GTIDS_THAN_MASTER,
70
ER_SLAVE_HEARTBEAT_FAILURE, 48
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE,
48
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MAX,
54
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MIN,
53
ER_SLAVE_IGNORED_SSL_PARAMS, 23
ER_SLAVE_IGNORED_TABLE, 21
ER_SLAVE_IGNORE_SERVER_IDS, 49
ER_SLAVE_INCIDENT, 45
ER_SLAVE_IO_THREAD_MUST_STOP, 71
ER_SLAVE_MASTER_COM_FAILURE, 46
ER_SLAVE_MAX_CHANNELS_EXCEEDED, 79
ER_SLAVE_MI_INIT_REPOSITORY, 68
ER_SLAVE_MULTIPLE_CHANNELS_CMD, 79
ER_SLAVE_MULTIPLE_CHANNELS_HOST_PORT,
79
ER_SLAVE_MUST_STOP, 17
ER_SLAVE_NEW_CHANNEL_WRONG_REPOSITORY,
79
ER_SLAVE_NOT_RUNNING, 18
ER_SLAVE_RELAY_LOG_READ_FAILURE, 45
ER_SLAVE_RELAY_LOG_WRITE_FAILURE, 45
ER_SLAVE_RLI_INIT_REPOSITORY, 69
ER_SLAVE_SILENT_RETRY_TRANSACTION, 62
ER_SLAVE_SQL_THREAD_MUST_STOP, 72
ER_SLAVE_THREAD, 18
ER_SLAVE_WAS_NOT_RUNNING, 22
ER_SLAVE_WAS_RUNNING, 22
ER_SLAVE_WORKER_STOPPED_PREVIOUS_THD_ERROR74
ER_SPATIAL_CANT_HAVE_NULL, 22
ER_SPATIAL_MUST_HAVE_GEOM_COL, 52
ER_SPECIFIC_ACCESS_DENIED_ERROR, 20
ER_SP_ALREADY_EXISTS, 25
ER_SP_BADRETURN, 26
ER_SP_BADSELECT, 26
ER_SP_BADSTATEMENT, 26
ER_SP_BAD_CURSOR_QUERY, 26
ER_SP_BAD_CURSOR_SELECT, 26
ER_SP_BAD_SQLSTATE, 32
ER_SP_BAD_VAR_SHADOW, 35
ER_SP_CANT_ALTER, 27
ER_SP_CANT_SET_AUTOCOMMIT, 35
ER_SP_CASE_NOT_FOUND, 27
ER_SP_COND_MISMATCH, 26
ER_SP_CURSOR_AFTER_HANDLER, 27
ER_SP_CURSOR_ALREADY_OPEN, 26
ER_SP_CURSOR_MISMATCH, 26
ER_SP_CURSOR_NOT_OPEN, 27
ER_SP_DOES_NOT_EXIST, 25
119
ER_SP_DROP_FAILED, 25
ER_SP_DUP_COND, 27
ER_SP_DUP_CURS, 27
ER_SP_DUP_HANDLER, 32
ER_SP_DUP_PARAM, 27
ER_SP_DUP_VAR, 27
ER_SP_FETCH_NO_DATA, 27
ER_SP_GOTO_IN_HNDLR, 29
ER_SP_LABEL_MISMATCH, 25
ER_SP_LABEL_REDEFINE, 25
ER_SP_LILABEL_MISMATCH, 25
ER_SP_NORETURN, 26
ER_SP_NORETURNEND, 26
ER_SP_NOT_VAR_ARG, 33
ER_SP_NO_AGGREGATE, 36
ER_SP_NO_DROP_SP, 29
ER_SP_NO_RECURSION, 33
ER_SP_NO_RECURSIVE_CREATE, 25
ER_SP_NO_RETSET, 33
ER_SP_PROC_TABLE_CORRUPT, 36
ER_SP_RECURSION_LIMIT, 36
ER_SP_STORE_FAILED, 25
ER_SP_SUBSELECT_NYI, 27
ER_SP_UNDECLARED_VAR, 27
ER_SP_UNINIT_VAR, 26
ER_SP_VARCOND_AFTER_CURSHNDLR, 27
ER_SP_WRONG_NAME, 36
ER_SP_WRONG_NO_OF_ARGS, 26
ER_SP_WRONG_NO_OF_FETCH_ARGS, 27
ER_SQLTHREAD_WITH_SECURE_SLAVE, 58
ER_SQL_MODE_MERGED, 86
ER_SQL_MODE_NO_EFFECT, 73
ER_SQL_SLAVE_SKIP_COUNTER_NOT_SETTABLE_IN_GTID_MODE,
67
ER_SR_INVALID_CREATION_CTX, 46
ER_STACK_OVERRUN, 12
ER_STACK_OVERRUN_NEED_MORE, 34
ER_STARTUP, 32
ER_STD_BAD_ALLOC_ERROR, 75
ER_STD_DOMAIN_ERROR, 75
ER_STD_INVALID_ARGUMENT, 76
ER_STD_LENGTH_ERROR, 76
ER_STD_LOGIC_ERROR, 76
ER_STD_OUT_OF_RANGE_ERROR, 76
ER_STD_OVERFLOW_ERROR, 76
ER_STD_RANGE_ERROR, 76
ER_STD_RUNTIME_ERROR, 76
ER_STD_UNDERFLOW_ERROR, 76
ER_STD_UNKNOWN_EXCEPTION, 76
ER_STMT_CACHE_FULL, 54
ER_STMT_HAS_NO_OPEN_CURSOR, 33
ER_STMT_NOT_ALLOWED_IN_SF_OR_TRG, 27
ER_STOP_SLAVE_IO_THREAD_TIMEOUT, 69
ER_STOP_SLAVE_SQL_THREAD_TIMEOUT, 69
ER_STORAGE_ENGINE_NOT_LOADED, 71
ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_DIREC52
ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_FORM43
ER_STORED_FUNCTION_PREVENTS_SWITCH_SQL_LOG_BIN53
ER_SUBPARTITION_ERROR, 39
ER_SUBPARTITION_NAME, 48
ER_SUBQUERY_NO_1_ROW, 21
ER_SYNTAX_ERROR, 14
ER_TABLEACCESS_DENIED_ERROR, 14
ER_TABLENAME_NOT_ALLOWED_HERE, 21
ER_TABLESPACE_AUTO_EXTEND_ERROR, 41
ER_TABLESPACE_CANNOT_ENCRYPT, 92
ER_TABLESPACE_DISCARDED, 63
ER_TABLESPACE_EXISTS, 63
ER_TABLESPACE_IS_NOT_EMPTY, 84
ER_TABLESPACE_MISSING, 63
ER_TABLES_DIFFERENT_METADATA, 56
ER_TABLE_CANT_HANDLE_AUTO_INCREMENT,
15
ER_TABLE_CANT_HANDLE_BLOB, 15
ER_TABLE_CANT_HANDLE_FT, 19
ER_TABLE_CANT_HANDLE_SPKEYS, 36
ER_TABLE_CORRUPT, 69
ER_TABLE_DEF_CHANGED, 32
ER_TABLE_EXISTS_ERROR, 7
ER_TABLE_HAS_NO_FT, 58
ER_TABLE_IN_FK_CHECK, 55
ER_TABLE_IN_SYSTEM_TABLESPACE, 63
ER_TABLE_MUST_HAVE_COLUMNS, 12
ER_TABLE_NAME, 48
ER_TABLE_NEEDS_REBUILD, 54
ER_TABLE_NEEDS_UPGRADE, 36
ER_TABLE_NEEDS_UPG_PART, 90
ER_TABLE_NOT_LOCKED, 11
ER_TABLE_NOT_LOCKED_FOR_WRITE, 11
ER_TABLE_REFERENCED, 93
ER_TABLE_SCHEMA_MISMATCH, 62
ER_TEMPORARY_NAME, 48
ER_TEMP_FILE_WRITE_FAILURE, 69
ER_TEMP_TABLE_PREVENTS_SWITCH_OUT_OF_RBR,
43
ER_TEXTFILE_NOT_READABLE, 10
ER_TOO_BIG_DISPLAYWIDTH, 34
ER_TOO_BIG_FIELDLENGTH, 9
ER_TOO_BIG_FOR_UNCOMPRESS, 22
ER_TOO_BIG_PRECISION, 33
ER_TOO_BIG_ROWSIZE, 12
ER_TOO_BIG_SCALE, 33
ER_TOO_BIG_SELECT, 11
ER_TOO_BIG_SET, 11
120
ER_TOO_HIGH_LEVEL_OF_NESTING_FOR_SELECT,
37
ER_TOO_LONG_BODY, 34
ER_TOO_LONG_FIELD_COMMENT, 48
ER_TOO_LONG_IDENT, 8
ER_TOO_LONG_INDEX_COMMENT, 52
ER_TOO_LONG_KEY, 9
ER_TOO_LONG_STRING, 15
ER_TOO_LONG_TABLE_COMMENT, 48
ER_TOO_LONG_TABLE_PARTITION_COMMENT,
61
ER_TOO_MANY_CONCURRENT_TRXS, 49
ER_TOO_MANY_FIELDS, 12
ER_TOO_MANY_KEYS, 9
ER_TOO_MANY_KEY_PARTS, 9
ER_TOO_MANY_PARTITIONS_ERROR, 39
ER_TOO_MANY_PARTITION_FUNC_FIELDS_ERROR,
50
ER_TOO_MANY_ROWS, 16
ER_TOO_MANY_TABLES, 12
ER_TOO_MANY_USER_CONNECTIONS, 18
ER_TOO_MANY_VALUES_ERROR, 50
ER_TOO_MUCH_AUTO_TIMESTAMP_COLS, 24
ER_TRANSACTION_ROLLBACK_DURING_COMMIT,
82
ER_TRANS_CACHE_FULL, 17
ER_TRG_ALREADY_EXISTS, 29
ER_TRG_CANT_CHANGE_ROW, 29
ER_TRG_CANT_OPEN_TABLE, 46
ER_TRG_CORRUPTED_FILE, 46
ER_TRG_DOES_NOT_EXIST, 29
ER_TRG_INVALID_CREATION_CTX, 46
ER_TRG_IN_WRONG_SCHEMA, 34
ER_TRG_NO_CREATION_CTX, 46
ER_TRG_NO_DEFINER, 35
ER_TRG_NO_SUCH_ROW_IN_TRG, 29
ER_TRG_ON_VIEW_OR_TEMP_TABLE, 29
ER_TRUNCATED_WRONG_VALUE, 24
ER_TRUNCATED_WRONG_VALUE_FOR_FIELD, 29
ER_TRUNCATE_ILLEGAL_FK, 53
ER_UDF_ERROR, 94
ER_UDF_EXISTS, 13
ER_UDF_NO_PATHS, 12
ER_UNDO_RECORD_TOO_BIG, 54
ER_UNEXPECTED_EOF, 6
ER_UNION_TABLES_IN_DIFFERENT_DIR, 19
ER_UNIQUE_KEY_NEED_ALL_FIELDS_IN_PF, 39
ER_UNKNOWN_ALTER_ALGORITHM, 62
ER_UNKNOWN_ALTER_LOCK, 62
ER_UNKNOWN_CHARACTER_SET, 12
ER_UNKNOWN_COLLATION, 23
ER_UNKNOWN_COM_ERROR, 7
ER_UNKNOWN_ERROR, 11
ER_UNKNOWN_EXPLAIN_FORMAT, 61
ER_UNKNOWN_KEY_CACHE, 24
ER_UNKNOWN_LOCALE, 49
ER_UNKNOWN_PARTITION, 56
ER_UNKNOWN_PROCEDURE, 11
ER_UNKNOWN_STMT_HANDLER, 21
ER_UNKNOWN_STORAGE_ENGINE, 24
ER_UNKNOWN_SYSTEM_VARIABLE, 17
ER_UNKNOWN_TABLE, 11
ER_UNKNOWN_TARGET_BINLOG, 30
ER_UNKNOWN_TIME_ZONE, 25
ER_UNRESOLVED_HINT_NAME, 85
ER_UNSUPORTED_LOG_ENGINE, 44
ER_UNSUPPORTED_ACTION_ON_GENERATED_COLUMN,82
ER_UNSUPPORTED_ALTER_ENCRYPTION_INPLACE,
92
ER_UNSUPPORTED_ALTER_INPLACE_ON_VIRTUAL_COLU82
ER_UNSUPPORTED_ALTER_ONLINE_ON_VIRTUAL_COLU91
ER_UNSUPPORTED_BY_REPLICATION_THREAD,
77
ER_UNSUPPORTED_ENGINE, 55
ER_UNSUPPORTED_EXTENSION, 12
ER_UNSUPPORTED_PS, 24
ER_UNTIL_COND_IGNORED, 23
ER_UNUSED1, 14
ER_UNUSED2, 14
ER_UNUSED3, 15
ER_UNUSED4, 51
ER_UNUSED5, 64
ER_UNUSED6, 66
ER_UPDATE_INFO, 13
ER_UPDATE_LOG_DEPRECATED_IGNORED, 26
ER_UPDATE_LOG_DEPRECATED_TRANSLATED,
26
ER_UPDATE_TABLE_USED, 10
ER_UPDATE_WITHOUT_KEY_IN_SAFE_MODE, 16
ER_USERNAME, 36
ER_USER_ALREADY_EXISTS, 89
ER_USER_COLUMN_OLD_LENGTH, 92
ER_USER_DOES_NOT_EXIST, 89
ER_USER_LIMIT_REACHED, 20
ER_USER_LOCK_DEADLOCK, 77
ER_USER_LOCK_WRONG_NAME, 77
ER_VALUES_IS_NOT_INT_TYPE_ERROR, 53
ER_VARIABLE_IS_NOT_STRUCT, 23
ER_VARIABLE_IS_READONLY, 48
ER_VARIABLE_NOT_SETTABLE_IN_SF_OR_TRIGGER,
58
ER_VARIABLE_NOT_SETTABLE_IN_SP, 65
ER_VARIABLE_NOT_SETTABLE_IN_TRANSACTION,
58
ER_VAR_CANT_BE_READ, 20
121
ER_VIEW_CHECKSUM, 31
ER_VIEW_CHECK_FAILED, 30
ER_VIEW_DELETE_MERGE_VIEW, 31
ER_VIEW_FRM_NO_USER, 35
ER_VIEW_INVALID, 29
ER_VIEW_INVALID_CREATION_CTX, 46
ER_VIEW_MULTIUPDATE, 31
ER_VIEW_NONUPD_CHECK, 29
ER_VIEW_NO_CREATION_CTX, 46
ER_VIEW_NO_EXPLAIN, 28
ER_VIEW_NO_INSERT_FIELD_LIST, 31
ER_VIEW_OTHER_USER, 35
ER_VIEW_PREVENT_UPDATE, 35
ER_VIEW_RECURSIVE, 36
ER_VIEW_SELECT_CLAUSE, 28
ER_VIEW_SELECT_DERIVED, 28
ER_VIEW_SELECT_DERIVED_UNUSED, 28
ER_VIEW_SELECT_TMPTABLE, 28
ER_VIEW_SELECT_VARIABLE, 28
ER_VIEW_WRONG_LIST, 28
ER_VTOKEN_PLUGIN_TOKEN_MISMATCH, 86
ER_VTOKEN_PLUGIN_TOKEN_NOT_FOUND, 87
ER_WARNING_NOT_COMPLETE_ROLLBACK, 17
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_CREATED_TEMP_TABLE,
57
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_DROPPED_TEMP_TABLE,
57
ER_WARN_ALLOWED_PACKET_OVERFLOWED,
25
ER_WARN_BAD_MAX_EXECUTION_TIME, 85
ER_WARN_CANT_DROP_DEFAULT_KEYCACHE,
34
ER_WARN_CONFLICTING_HINT, 85
ER_WARN_DATA_OUT_OF_RANGE, 22
ER_WARN_DEPRECATED_SQLMODE, 80
ER_WARN_DEPRECATED_SQLMODE_UNSET, 85
ER_WARN_DEPRECATED_SYNTAX, 24
ER_WARN_DEPRECATED_SYNTAX_NO_REPLACEMENT,
52
ER_WARN_DEPRECATED_SYNTAX_WITH_VER,
43
ER_WARN_DEPRECATED_SYSVAR_UPDATE, 80
ER_WARN_DEPRECATED_TLS_VERSION, 97
ER_WARN_ENGINE_TRANSACTION_ROLLBACK,
48
ER_WARN_FIELD_RESOLVED, 23
ER_WARN_HOSTNAME_WONT_WORK, 24
ER_WARN_INDEX_NOT_APPLICABLE, 56
ER_WARN_INVALID_TIMESTAMP, 25
ER_WARN_I_S_SKIPPED_TABLE, 52
ER_WARN_LEGACY_SYNTAX_CONVERTED, 71
ER_WARN_NULL_TO_NOTNULL, 22
ER_WARN_ONLY_MASTER_LOG_FILE_NO_POS,
73
ER_WARN_ON_MODIFYING_GTID_EXECUTED_TABLE,
86
ER_WARN_OPEN_TEMP_TABLES_MUST_BE_ZERO,
73
ER_WARN_OPTIMIZER_HINT_SYNTAX_ERROR, 85
ER_WARN_PURGE_LOG_IN_USE, 68
ER_WARN_PURGE_LOG_IS_ACTIVE, 68
ER_WARN_QC_RESIZE, 24
ER_WARN_TOO_FEW_RECORDS, 22
ER_WARN_TOO_MANY_RECORDS, 22
ER_WARN_TRIGGER_DOESNT_HAVE_CREATED,
72
ER_WARN_UNKNOWN_QB_NAME, 85
ER_WARN_UNSUPPORTED_MAX_EXECUTION_TIME,
85
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID,
93
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID_ZERO,
93
ER_WARN_USING_OTHER_HANDLER, 22
ER_WARN_VIEW_MERGE, 29
ER_WARN_VIEW_WITHOUT_KEY, 29
ER_WARN_WRONG_NATIVE_TABLE_STRUCTURE,
97
ER_WRITE_SET_EXCEEDS_LIMIT, 97
ER_WRONG_ARGUMENTS, 18
ER_WRONG_AUTO_KEY, 9
ER_WRONG_COLUMN_NAME, 15
ER_WRONG_DB_NAME, 11
ER_WRONG_EXPR_IN_PARTITION_FUNC_ERROR,
38
ER_WRONG_FIELD_SPEC, 8
ER_WRONG_FIELD_TERMINATORS, 9
ER_WRONG_FIELD_WITH_GROUP, 8
ER_WRONG_FIELD_WITH_GROUP_V2, 80
ER_WRONG_FILE_NAME, 85
ER_WRONG_FK_DEF, 21
ER_WRONG_FK_OPTION_FOR_GENERATED_COLUMN,
82
ER_WRONG_GROUP_FIELD, 8
ER_WRONG_KEY_COLUMN, 15
ER_WRONG_LOCK_OF_SYSTEM_TABLE, 34
ER_WRONG_MAGIC, 31
ER_WRONG_MRG_TABLE, 15
ER_WRONG_NAME_FOR_CATALOG, 24
ER_WRONG_NAME_FOR_INDEX, 23
ER_WRONG_NATIVE_TABLE_STRUCTURE, 52
ER_WRONG_NUMBER_OF_COLUMNS_IN_SELECT,
20
ER_WRONG_OBJECT, 28
ER_WRONG_OUTER_JOIN, 12
ER_WRONG_PARAMCOUNT_TO_NATIVE_FCT, 45
ER_WRONG_PARAMCOUNT_TO_PROCEDURE, 11
ER_WRONG_PARAMETERS_TO_NATIVE_FCT, 45
122
ER_WRONG_PARAMETERS_TO_PROCEDURE, 11
ER_WRONG_PARAMETERS_TO_STORED_FCT, 45
ER_WRONG_PARTITION_NAME, 43
ER_WRONG_PERFSCHEMA_USAGE, 52
ER_WRONG_SIZE_NUMBER, 41
ER_WRONG_SPVAR_TYPE_IN_LIMIT, 53
ER_WRONG_STRING_LENGTH, 37
ER_WRONG_SUB_KEY, 10
ER_WRONG_SUM_SELECT, 8
ER_WRONG_TABLESPACE_NAME, 84
ER_WRONG_TABLE_NAME, 11
ER_WRONG_TYPE_COLUMN_VALUE_ERROR, 50
ER_WRONG_TYPE_FOR_VAR, 20
ER_WRONG_USAGE, 19
ER_WRONG_VALUE, 40
ER_WRONG_VALUE_COUNT, 8
ER_WRONG_VALUE_COUNT_ON_ROW, 13
ER_WRONG_VALUE_FOR_TYPE, 32
ER_WRONG_VALUE_FOR_VAR, 20
ER_WSAS_FAILED, 30
ER_XAER_DUPID, 34
ER_XAER_INVAL, 31
ER_XAER_NOTA, 31
ER_XAER_OUTSIDE, 32
ER_XAER_RMERR, 32
ER_XAER_RMFAIL, 32
ER_XA_RBDEADLOCK, 47
ER_XA_RBROLLBACK, 32
ER_XA_RBTIMEOUT, 47
ER_XA_REPLICATION_FILTERS, 97
ER_XA_RETRY, 93
ER_YES, 3
ER_ZLIB_Z_BUF_ERROR, 22
ER_ZLIB_Z_DATA_ERROR, 22
ER_ZLIB_Z_MEM_ERROR, 22
WARN_AES_KEY_SIZE, 98
WARN_COND_ITEM_TRUNCATED, 49
WARN_DATA_TRUNCATED, 22
WARN_DEPRECATED_MAXDB_SQL_MODE_FOR_TIMESTAMP,
97
WARN_NAMED_PIPE_ACCESS_EVERYONE, 70
WARN_NON_ASCII_SEPARATOR_NOT_IMPLEMENTED,
49
WARN_NO_MASTER_INFO, 47
WARN_ON_BLOCKHOLE_IN_RBR, 68
WARN_OPTION_BELOW_LIMIT, 54
WARN_OPTION_IGNORED, 47
WARN_PLUGIN_BUSY, 47
WARN_PLUGIN_DELETE_BUILTIN, 47
ER_ABORTING_CONNECTION error code, 14
ER_ACCESS_DENIED_CHANGE_USER_ERROR error
code, 69
ER_ACCESS_DENIED_ERROR error code, 7
ER_ACCESS_DENIED_NO_PASSWORD_ERROR error
code, 53
ER_ACCOUNT_HAS_BEEN_LOCKED error code, 84
ER_ADD_PARTITION_NO_NEW_PARTITION error
code, 40
ER_ADD_PARTITION_SUBPART_ERROR error code,
40
ER_ADMIN_WRONG_MRG_TABLE error code, 37
ER_AES_INVALID_IV error code, 70
ER_AES_INVALID_KDF_ITERATIONS error code, 98
ER_AES_INVALID_KDF_NAME error code, 97
ER_AES_INVALID_KDF_OPTION_SIZE error code, 98
ER_AGGREGATE_IN_ORDER_NOT_SELECT error
code, 78
ER_AGGREGATE_ORDER_FOR_UNION error code, 73
ER_AGGREGATE_ORDER_NON_AGG_QUERY error
code, 74
ER_ALTER_FILEGROUP_FAILED error code, 41
ER_ALTER_INFO error code, 10
ER_ALTER_OPERATION_NOT_SUPPORTED error
code, 65
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON
error code, 65
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_AUTOerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_CHANerror code, 67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COLUerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_COPYerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_CHerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FK_REerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_FTS
error code, 67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_GIS
error code, 77
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_HIDDEerror code, 67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_IGNORerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOPKerror code, 66
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_NOT_error code, 67
ER_ALTER_OPERATION_NOT_SUPPORTED_REASON_PARTerror code, 66
ER_AMBIGUOUS_FIELD_TERM error code, 37
ER_AUDIT_API_ABORT error code, 90
ER_AUDIT_LOG_COULD_NOT_CREATE_AES_KEY
error code, 94
123
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_CANNOT_BE_FETCHED
error code, 95
ER_AUDIT_LOG_ENCRYPTION_PASSWORD_HAS_NOT_BEEN_SET
error code, 94
ER_AUDIT_LOG_HAS_NOT_BEEN_INSTALLED error
code, 95
ER_AUDIT_LOG_HOST_NAME_INVALID_CHARACTER
error code, 96
ER_AUDIT_LOG_JSON_FILTERING_NOT_ENABLED
error code, 95
ER_AUDIT_LOG_JSON_FILTER_DOES_NOT_EXISTS
error code, 96
ER_AUDIT_LOG_JSON_FILTER_NAME_CANNOT_BE_EMPTY
error code, 96
ER_AUDIT_LOG_JSON_FILTER_PARSING_ERROR
error code, 96
ER_AUDIT_LOG_JSON_USER_NAME_CANNOT_BE_EMPTY
error code, 96
ER_AUDIT_LOG_NO_KEYRING_PLUGIN_INSTALLED
error code, 94
ER_AUDIT_LOG_SUPER_PRIVILEGE_REQUIRED
error code, 95
ER_AUDIT_LOG_UDF_INSUFFICIENT_PRIVILEGE
error code, 95
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_COUNT
error code, 95
ER_AUDIT_LOG_UDF_INVALID_ARGUMENT_TYPE
error code, 95
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_TYPE
error code, 95
ER_AUDIT_LOG_UDF_READ_INVALID_MAX_ARRAY_LENGTH_ARG_VALUE
error code, 96
ER_AUDIT_LOG_USER_FIRST_CHARACTER_MUST_BE_ALPHANUMERIC
error code, 96
ER_AUDIT_LOG_USER_NAME_INVALID_CHARACTER
error code, 96
ER_AUTOINC_READ_FAILED error code, 36
ER_AUTO_CONVERT error code, 21
ER_AUTO_INCREMENT_CONFLICT error code, 68
ER_AUTO_POSITION_REQUIRES_GTID_MODE_NOT_OFF
error code, 60
ER_AUTO_POSITION_REQUIRES_GTID_MODE_ON
error code, 59
ER_BAD_DB_ERROR error code, 7
ER_BAD_FIELD_ERROR error code, 8
ER_BAD_FT_COLUMN error code, 24
ER_BAD_HOST_ERROR error code, 6
ER_BAD_LOG_STATEMENT error code, 44
ER_BAD_NULL_ERROR error code, 7
ER_BAD_SLAVE error code, 18
ER_BAD_SLAVE_AUTO_POSITION error code, 59
ER_BAD_SLAVE_UNTIL_COND error code, 23
ER_BAD_TABLE_ERROR error code, 7
ER_BASE64_DECODE_ERROR error code, 44
ER_BEFORE_DML_VALIDATION_ERROR error code,
81
ER_BINLOG_CACHE_SIZE_GREATER_THAN_MAX
error code, 56
ER_BINLOG_CREATE_ROUTINE_NEED_SUPER error
code, 33
ER_BINLOG_LOGGING_IMPOSSIBLE error code, 46
ER_BINLOG_LOGICAL_CORRUPTION error code, 68
ER_BINLOG_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINerror code, 51
ER_BINLOG_PURGE_EMFILE error code, 45
ER_BINLOG_PURGE_FATAL_ERR error code, 30
ER_BINLOG_PURGE_PROHIBITED error code, 30
ER_BINLOG_READ_EVENT_CHECKSUM_FAILURE
error code, 57
ER_BINLOG_ROW_ENGINE_AND_STMT_ENGINE
error code, 50
ER_BINLOG_ROW_INJECTION_AND_STMT_ENGINE
error code, 50
ER_BINLOG_ROW_INJECTION_AND_STMT_MODE
error code, 51
ER_BINLOG_ROW_LOGGING_FAILED error code, 41
ER_BINLOG_ROW_MODE_AND_STMT_ENGINE error
code, 50
ER_BINLOG_ROW_RBR_TO_SBR error code, 41
ER_BINLOG_ROW_WRONG_TABLE_DEF error code,
41
ER_BINLOG_STMT_CACHE_SIZE_GREATER_THAN_MAX
error code, 57
ER_BINLOG_STMT_MODE_AND_NO_REPL_TABLES
error code, 65
ER_BINLOG_STMT_MODE_AND_ROW_ENGINE error
code, 51
ER_BINLOG_UNSAFE_AND_STMT_ENGINE error
code, 50
ER_BINLOG_UNSAFE_AUTOINC_COLUMNS error
code, 51
ER_BINLOG_UNSAFE_AUTOINC_NOT_FIRST error
code, 55
ER_BINLOG_UNSAFE_CREATE_IGNORE_SELECT
error code, 55
ER_BINLOG_UNSAFE_CREATE_REPLACE_SELECT
error code, 55
ER_BINLOG_UNSAFE_CREATE_SELECT_AUTOINC
error code, 55
ER_BINLOG_UNSAFE_FULLTEXT_PLUGIN error code,
71
ER_BINLOG_UNSAFE_INSERT_IGNORE_SELECT
error code, 54
ER_BINLOG_UNSAFE_INSERT_SELECT_UPDATE
error code, 54
ER_BINLOG_UNSAFE_INSERT_TWO_KEYS error
code, 55
ER_BINLOG_UNSAFE_LIMIT error code, 51
124
ER_BINLOG_UNSAFE_MIXED_STATEMENT error
code, 53
ER_BINLOG_UNSAFE_MULTIPLE_ENGINES_AND_SELF_LOGGING_ENGINE
error code, 53
ER_BINLOG_UNSAFE_NONTRANS_AFTER_TRANS
error code, 51
ER_BINLOG_UNSAFE_REPLACE_SELECT error code,
54
ER_BINLOG_UNSAFE_ROUTINE error code, 33
ER_BINLOG_UNSAFE_STATEMENT error code, 45
ER_BINLOG_UNSAFE_SYSTEM_FUNCTION error
code, 51
ER_BINLOG_UNSAFE_SYSTEM_TABLE error code, 51
ER_BINLOG_UNSAFE_SYSTEM_VARIABLE error
code, 51
ER_BINLOG_UNSAFE_UDF error code, 51
ER_BINLOG_UNSAFE_UPDATE_IGNORE error code,
55
ER_BINLOG_UNSAFE_WRITE_AUTOINC_SELECT
error code, 55
ER_BINLOG_UNSAFE_XA error code, 94
ER_BLOBS_AND_NO_TERMINATED error code, 10
ER_BLOB_CANT_HAVE_DEFAULT error code, 11
ER_BLOB_FIELD_IN_PART_FUNC_ERROR error code,
39
ER_BLOB_KEY_WITHOUT_LENGTH error code, 16
ER_BLOB_USED_AS_KEY error code, 9
ER_BOOST_GEOMETRY_CENTROID_EXCEPTION
error code, 75
ER_BOOST_GEOMETRY_EMPTY_INPUT_EXCEPTION
error code, 75
ER_BOOST_GEOMETRY_INCONSISTENT_TURNS_EXCEPTION
error code, 85
ER_BOOST_GEOMETRY_OVERLAY_INVALID_INPUT_EXCEPTION
error code, 75
ER_BOOST_GEOMETRY_SELF_INTERSECTION_POINT_EXCEPTION
error code, 75
ER_BOOST_GEOMETRY_TURN_INFO_EXCEPTION
error code, 75
ER_BOOST_GEOMETRY_UNKNOWN_EXCEPTION
error code, 75
ER_BUFPOOL_RESIZE_INPROGRESS error code, 90
ER_CANNOT_ADD_FOREIGN error code, 19
ER_CANNOT_ADD_FOREIGN_BASE_COL_STORED
error code, 93
ER_CANNOT_ADD_FOREIGN_BASE_COL_VIRTUAL
error code, 91
ER_CANNOT_CREATE_VIRTUAL_INDEX_CONSTRAINT
error code, 91
ER_CANNOT_DISCARD_TEMPORARY_TABLE error
code, 71
ER_CANNOT_FIND_KEY_IN_KEYRING error code, 92
ER_CANNOT_LOAD_FROM_TABLE_V2 error code, 55
ER_CANNOT_LOG_PARTIAL_DROP_DATABASE_WITH_GTID
error code, 80
ER_CANNOT_USER error code, 31
ER_CANT_ACTIVATE_LOG error code, 44
ER_CANT_AGGREGATE_2COLLATIONS error code,
23
ER_CANT_AGGREGATE_3COLLATIONS error code,
23
ER_CANT_AGGREGATE_NCOLLATIONS error code,
23
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION
error code, 59
ER_CANT_CHANGE_GTID_NEXT_IN_TRANSACTION_WHEN_error code, 59
ER_CANT_CHANGE_TX_CHARACTERISTICS error
code, 44
ER_CANT_CREATE_DB error code, 4
ER_CANT_CREATE_FEDERATED_TABLE error code,
34
ER_CANT_CREATE_FILE error code, 3
ER_CANT_CREATE_GEOMETRY_OBJECT error code,
33
ER_CANT_CREATE_HANDLER_FILE error code, 39
ER_CANT_CREATE_SROUTINE error code, 46
ER_CANT_CREATE_TABLE error code, 4
ER_CANT_CREATE_THREAD error code, 13
ER_CANT_CREATE_USER_WITH_GRANT error code,
32
ER_CANT_DELETE_FILE error code, 4
ER_CANT_DO_IMPLICIT_COMMIT_IN_TRX_WHEN_GTID_NEXerror code, 60
ER_CANT_DO_THIS_DURING_AN_TRANSACTION
error code, 16
ER_CANT_DROP_FIELD_OR_KEY error code, 10
ER_CANT_EXECUTE_IN_READ_ONLY_TRANSACTION
error code, 61
ER_CANT_FIND_DL_ENTRY error code, 13
ER_CANT_FIND_SYSTEM_REC error code, 4
ER_CANT_FIND_UDF error code, 12
ER_CANT_GET_STAT error code, 4
ER_CANT_GET_WD error code, 4
ER_CANT_INITIALIZE_UDF error code, 12
ER_CANT_LOCK error code, 4
ER_CANT_LOCK_LOG_TABLE error code, 43
ER_CANT_OPEN_ERROR_LOG error code, 97
ER_CANT_OPEN_FILE error code, 5
ER_CANT_OPEN_LIBRARY error code, 13
ER_CANT_READ_DIR error code, 5
ER_CANT_REMOVE_ALL_FIELDS error code, 10
ER_CANT_RENAME_LOG_TABLE error code, 44
ER_CANT_REOPEN_TABLE error code, 13
ER_CANT_REPLICATE_ANONYMOUS_WITH_AUTO_POSITIONerror code, 83
125
ER_CANT_REPLICATE_ANONYMOUS_WITH_GTID_MODE_ON
error code, 83
ER_CANT_REPLICATE_GTID_WITH_GTID_MODE_OFF
error code, 84
ER_CANT_RESET_MASTER error code, 92
ER_CANT_SET_ENFORCE_GTID_CONSISTENCY_ON_WITH_ONGOING_GTID_VIOLATING_TRANSACTIONS
error code, 84
ER_CANT_SET_GTID_MODE error code, 83
ER_CANT_SET_GTID_NEXT_LIST_TO_NON_NULL_WHEN_GTID_MODE_IS_OFF
error code, 60
ER_CANT_SET_GTID_NEXT_TO_ANONYMOUS_WHEN_GTID_MODE_IS_ON
error code, 60
ER_CANT_SET_GTID_NEXT_TO_GTID_WHEN_GTID_MODE_IS_OFF
error code, 60
ER_CANT_SET_GTID_NEXT_WHEN_OWNING_GTID
error code, 61
ER_CANT_SET_GTID_PURGED_WHEN_GTID_EXECUTED_IS_NOT_EMPTY
error code, 65
ER_CANT_SET_GTID_PURGED_WHEN_GTID_MODE_IS_OFF
error code, 65
ER_CANT_SET_GTID_PURGED_WHEN_OWNED_GTIDS_IS_NOT_EMPTY
error code, 65
ER_CANT_SET_VARIABLE_WHEN_OWNING_GTID
error code, 87
ER_CANT_SET_WD error code, 5
ER_CANT_START_SERVER_NAMED_PIPE error code,
97
ER_CANT_UPDATE_TABLE_IN_CREATE_TABLE_SELECT
error code, 57
ER_CANT_UPDATE_USED_TABLE_IN_SF_OR_TRG
error code, 35
ER_CANT_UPDATE_WITH_READLOCK error code, 20
ER_CANT_USE_AUTO_POSITION_WITH_GTID_MODE_OFF
error code, 83
ER_CANT_USE_OPTION_HERE error code, 20
ER_CANT_WAIT_FOR_EXECUTED_GTID_SET_WHILE_OWNING_A_GTID
error code, 91
ER_CANT_WRITE_LOCK_LOG_TABLE error code, 43
ER_CAPACITY_EXCEEDED error code, 90
ER_CAPACITY_EXCEEDED_IN_PARSER error code,
92
ER_CAPACITY_EXCEEDED_IN_RANGE_OPTIMIZER
error code, 90
ER_CHANGE_MASTER_PASSWORD_LENGTH error
code, 77
ER_CHANGE_RPL_INFO_REPOSITORY_FAILURE
error code, 57
ER_CHECKREAD error code, 5
ER_CHECK_NOT_IMPLEMENTED error code, 16
ER_CHECK_NO_SUCH_TABLE error code, 16
ER_COALESCE_ONLY_ON_HASH_PARTITION error
code, 39
ER_COALESCE_PARTITION_NO_PARTITION error
code, 40
ER_COLLATION_CHARSET_MISMATCH error code, 22
ER_COLUMNACCESS_DENIED_ERROR error code, 14
ER_COL_COUNT_DOESNT_MATCH_CORRUPTED_V2
error code, 62
ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE
error code, 43
ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE_V2
error code, 71
ER_COMMIT_NOT_ALLOWED_IN_SF_OR_TRG error
code, 33
ER_COND_ITEM_TOO_LONG error code, 49
ER_CONFLICTING_DECLARATIONS error code, 25
ER_CONFLICT_FN_PARSE_ERROR error code, 48
ER_CONNECT_TO_FOREIGN_DATA_SOURCE error
code, 34
ER_CONNECT_TO_MASTER error code, 19
ER_CONSECUTIVE_REORG_PARTITIONS error code,
40
ER_CON_COUNT_ERROR error code, 6
ER_CORRUPT_HELP_DB error code, 21
ER_COULD_NOT_REINITIALIZE_AUDIT_LOG_FILTERS
error code, 95
ER_CRASHED_ON_REPAIR error code, 17
ER_CRASHED_ON_USAGE error code, 17
ER_CREATE_DB_WITH_READ_LOCK error code, 18
ER_CREATE_FILEGROUP_FAILED error code, 41
ER_CUT_VALUE_GROUP_CONCAT error code, 22
ER_CYCLIC_REFERENCE error code, 21
ER_DATABASE_NAME error code, 48
ER_DATA_OUT_OF_RANGE error code, 52
ER_DATA_TOO_LONG error code, 32
ER_DATETIME_FUNCTION_OVERFLOW error code,
34
ER_DA_INVALID_CONDITION_NUMBER error code, 58
ER_DBACCESS_DENIED_ERROR error code, 6
ER_DB_CREATE_EXISTS error code, 4
ER_DB_DROP_DELETE error code, 4
ER_DB_DROP_EXISTS error code, 4
ER_DB_DROP_RMDIR error code, 4
ER_DDL_LOG_ERROR error code, 43
ER_DEBUG_SYNC_HIT_LIMIT error code, 49
ER_DEBUG_SYNC_TIMEOUT error code, 49
ER_DELAYED_NOT_SUPPORTED error code, 47
ER_DEPENDENT_BY_GENERATED_COLUMN error
code, 83
ER_DEPRECATED_TLS_VERSION_SESSION error
code, 97
ER_DERIVED_MUST_HAVE_ALIAS error code, 21
ER_DIFF_GROUPS_PROC error code, 31
ER_DIMENSION_UNSUPPORTED error code, 78
ER_DISABLED_STORAGE_ENGINE error code, 89
ER_DISCARD_FK_CHECKS_RUNNING error code, 62
ER_DISK_FULL error code, 5
ER_DIVISION_BY_ZERO error code, 29
126
ER_DONT_SUPPORT_SLAVE_PRESERVE_COMMIT_ORDER
error code, 74
ER_DROP_DB_WITH_READ_LOCK error code, 18
ER_DROP_FILEGROUP_FAILED error code, 41
ER_DROP_INDEX_FK error code, 42
ER_DROP_LAST_PARTITION error code, 39
ER_DROP_PARTITION_NON_EXISTENT error code, 39
ER_DROP_USER error code, 23
ER_DUMP_NOT_IMPLEMENTED error code, 17
ER_DUPLICATED_VALUE_IN_TYPE error code, 24
ER_DUP_ARGUMENT error code, 20
ER_DUP_ENTRY error code, 8
ER_DUP_ENTRY_AUTOINCREMENT_CASE error
code, 44
ER_DUP_ENTRY_WITH_KEY_NAME error code, 45
ER_DUP_FIELDNAME error code, 8
ER_DUP_INDEX error code, 64
ER_DUP_KEY error code, 5
ER_DUP_KEYNAME error code, 8
ER_DUP_LIST_ENTRY error code, 73
ER_DUP_SIGNAL_SET error code, 49
ER_DUP_UNIQUE error code, 16
ER_DUP_UNKNOWN_IN_INDEX error code, 67
ER_EMPTY_QUERY error code, 8
ER_ENGINE_OUT_OF_MEMORY error code, 72
ER_ERROR_DURING_CHECKPOINT error code, 16
ER_ERROR_DURING_COMMIT error code, 16
ER_ERROR_DURING_FLUSH_LOGS error code, 16
ER_ERROR_DURING_ROLLBACK error code, 16
ER_ERROR_IN_TRIGGER_BODY error code, 54
ER_ERROR_IN_UNKNOWN_TRIGGER_BODY error
code, 54
ER_ERROR_ON_CLOSE error code, 5
ER_ERROR_ON_MASTER error code, 71
ER_ERROR_ON_MODIFYING_GTID_EXECUTED_TABLE
error code, 91
ER_ERROR_ON_READ error code, 5
ER_ERROR_ON_RENAME error code, 5
ER_ERROR_ON_WRITE error code, 5
ER_ERROR_WHEN_EXECUTING_COMMAND error
code, 19
ER_EVENTS_DB_ERROR error code, 44
ER_EVENT_ALREADY_EXISTS error code, 41
ER_EVENT_CANNOT_ALTER_IN_THE_PAST error
code, 45
ER_EVENT_CANNOT_CREATE_IN_THE_PAST error
code, 45
ER_EVENT_CANNOT_DELETE error code, 42
ER_EVENT_CANT_ALTER error code, 41
ER_EVENT_COMPILE_ERROR error code, 42
ER_EVENT_DATA_TOO_LONG error code, 42
ER_EVENT_DOES_NOT_EXIST error code, 41
ER_EVENT_DROP_FAILED error code, 41
ER_EVENT_ENDS_BEFORE_STARTS error code, 42
ER_EVENT_EXEC_TIME_IN_THE_PAST error code, 42
ER_EVENT_INTERVAL_NOT_POSITIVE_OR_TOO_BIG
error code, 42
ER_EVENT_INVALID_CREATION_CTX error code, 46
ER_EVENT_MODIFY_QUEUE_ERROR error code, 44
ER_EVENT_NEITHER_M_EXPR_NOR_M_AT error
code, 42
ER_EVENT_OPEN_TABLE_FAILED error code, 42
ER_EVENT_RECURSION_FORBIDDEN error code, 44
ER_EVENT_SAME_NAME error code, 42
ER_EVENT_SET_VAR_ERROR error code, 44
ER_EVENT_STORE_FAILED error code, 41
ER_EXCEPTIONS_WRITE_ERROR error code, 48
ER_EXEC_STMT_WITH_OPEN_CURSOR error code,
33
ER_EXPLAIN_NOT_SUPPORTED error code, 72
ER_FAILED_READ_FROM_PAR_FILE error code, 53
ER_FAILED_ROUTINE_BREAK_BINLOG error code, 33
ER_FEATURE_DISABLED error code, 24
ER_FEATURE_DISABLED_SEE_DOC error code, 90
ER_FEATURE_NOT_AVAILABLE error code, 83
ER_FIELD_IN_ORDER_NOT_SELECT error code, 78
ER_FIELD_NOT_FOUND_PART_ERROR error code, 38
ER_FIELD_SPECIFIED_TWICE error code, 11
ER_FIELD_TYPE_NOT_ALLOWED_AS_PARTITION_FIELD
error code, 50
ER_FILEGROUP_OPTION_ONLY_ONCE error code, 40
ER_FILE_CORRUPT error code, 71
ER_FILE_EXISTS_ERROR error code, 10
ER_FILE_NOT_FOUND error code, 5
ER_FILE_USED error code, 5
ER_FILSORT_ABORT error code, 5
ER_FK_CANNOT_DELETE_PARENT error code, 64
ER_FK_CANNOT_OPEN_PARENT error code, 64
ER_FK_COLUMN_CANNOT_CHANGE error code, 64
ER_FK_COLUMN_CANNOT_CHANGE_CHILD error
code, 64
ER_FK_COLUMN_CANNOT_DROP error code, 64
ER_FK_COLUMN_CANNOT_DROP_CHILD error code,
64
ER_FK_COLUMN_NOT_NULL error code, 64
ER_FK_DEPTH_EXCEEDED error code, 71
ER_FK_DUP_NAME error code, 64
ER_FK_FAIL_ADD_SYSTEM error code, 64
ER_FK_INCORRECT_OPTION error code, 64
ER_FK_NO_INDEX_CHILD error code, 63
ER_FK_NO_INDEX_PARENT error code, 63
ER_FLUSH_MASTER_BINLOG_CLOSED error code, 17
ER_FORBID_SCHEMA_CHANGE error code, 35
ER_FORCING_CLOSE error code, 9
ER_FOREIGN_DATA_SOURCE_DOESNT_EXIST error
code, 34
ER_FOREIGN_DATA_STRING_INVALID error code, 34
127
ER_FOREIGN_DATA_STRING_INVALID_CANT_CREATE
error code, 34
ER_FOREIGN_DUPLICATE_KEY_OLD_UNUSED error
code, 43
ER_FOREIGN_DUPLICATE_KEY_WITHOUT_CHILD_INFO
error code, 58
ER_FOREIGN_DUPLICATE_KEY_WITH_CHILD_INFO
error code, 58
ER_FOREIGN_KEY_ON_PARTITIONED error code, 39
ER_FOREIGN_SERVER_DOESNT_EXIST error code,
37
ER_FOREIGN_SERVER_EXISTS error code, 37
ER_FORM_NOT_FOUND error code, 5
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF
error code, 60
ER_FOUND_GTID_EVENT_WHEN_GTID_MODE_IS_OFF__UNUSED
error code, 61
ER_FOUND_MISSING_GTIDS error code, 70
ER_FPARSER_BAD_HEADER error code, 28
ER_FPARSER_EOF_IN_COMMENT error code, 28
ER_FPARSER_EOF_IN_UNKNOWN_PARAMETER
error code, 28
ER_FPARSER_ERROR_IN_PARAMETER error code,
28
ER_FPARSER_TOO_BIG_FILE error code, 27
ER_FRM_UNKNOWN_TYPE error code, 28
ER_FSEEK_FAIL error code, 30
ER_FT_MATCHING_KEY_NOT_FOUND error code, 17
ER_FULLTEXT_NOT_SUPPORTED_WITH_PARTITIONING
error code, 58
ER_FUNCTION_NOT_DEFINED error code, 13
ER_FUNC_INEXISTENT_NAME_COLLISION error
code, 48
ER_GENERATED_COLUMN_FUNCTION_IS_NOT_ALLOWED
error code, 82
ER_GENERATED_COLUMN_NON_PRIOR error code,
82
ER_GENERATED_COLUMN_REF_AUTO_INC error
code, 83
ER_GET_ERRMSG error code, 25
ER_GET_ERRNO error code, 6
ER_GET_STACKED_DA_WITHOUT_ACTIVE_HANDLER
error code, 71
ER_GET_TEMPORARY_ERRMSG error code, 25
ER_GIS_DATA_WRONG_ENDIANESS error code, 76
ER_GIS_DIFFERENT_SRIDS error code, 74
ER_GIS_INVALID_DATA error code, 75
ER_GIS_MAX_POINTS_IN_GEOMETRY_OVERFLOWED
error code, 86
ER_GIS_UNKNOWN_ERROR error code, 74
ER_GIS_UNKNOWN_EXCEPTION error code, 74
ER_GIS_UNSUPPORTED_ARGUMENT error code, 74
ER_GLOBAL_VARIABLE error code, 20
ER_GNO_EXHAUSTED error code, 59
ER_GOT_SIGNAL error code, 9
ER_GRANT_PLUGIN_USER_EXISTS error code, 53
ER_GRANT_WRONG_HOST_OR_USER error code, 14
ER_GROUPING_ON_TIMESTAMP_IN_DST error code,
97
ER_GROUP_REPLICATION_APPLIER_INIT_ERROR
error code, 81
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_JOIN_ERRerror code, 81
ER_GROUP_REPLICATION_COMMUNICATION_LAYER_SESSIONerror code, 81
ER_GROUP_REPLICATION_CONFIGURATION error
code, 81
ER_GROUP_REPLICATION_MAX_GROUP_SIZE error
code, 93
ER_GROUP_REPLICATION_RUNNING error code, 81
ER_GROUP_REPLICATION_STOP_APPLIER_THREAD_TIMEOUT
error code, 81
ER_GTID_EXECUTED_WAS_CHANGED error code, 65
ER_GTID_MODE_2_OR_3_REQUIRES_ENFORCE_GTID_CONSISTerror code, 60
ER_GTID_MODE_CAN_ONLY_CHANGE_ONE_STEP_AT_A_TIME
error code, 61
ER_GTID_MODE_OFF error code, 77
ER_GTID_MODE_ON_REQUIRES_ENFORCE_GTID_CONSISTENCerror code, 60
ER_GTID_MODE_REQUIRES_BINLOG error code, 60
ER_GTID_NEXT_CANT_BE_AUTOMATIC_IF_GTID_NEXT_LIST_ISerror code, 59
ER_GTID_NEXT_IS_NOT_IN_GTID_NEXT_LIST error
code, 58
ER_GTID_NEXT_TYPE_UNDEFINED_GROUP error
code, 65
ER_GTID_PURGED_WAS_CHANGED error code, 65
ER_GTID_UNSAFE_BINLOG_SPLITTABLE_STATEMENT_AND_GTerror code, 70
ER_GTID_UNSAFE_CREATE_DROP_TEMPORARY_TABLE_IN_TRerror code, 61
ER_GTID_UNSAFE_CREATE_SELECT error code, 61
ER_GTID_UNSAFE_NON_TRANSACTIONAL_TABLE
error code, 61
ER_HANDSHAKE_ERROR error code, 6
ER_HASHCHK error code, 3
ER_HOSTNAME error code, 37
ER_HOST_IS_BLOCKED error code, 13
ER_HOST_NOT_PRIVILEGED error code, 13
ER_IDENT_CAUSES_TOO_LONG_PATH error code, 67
ER_ILLEGAL_GRANT_FOR_TABLE error code, 14
ER_ILLEGAL_HA error code, 6
ER_ILLEGAL_HA_CREATE_OPTION error code, 37
ER_ILLEGAL_REFERENCE error code, 21
ER_ILLEGAL_USER_VAR error code, 77
ER_ILLEGAL_VALUE_FOR_TYPE error code, 29
ER_INCONSISTENT_ERROR error code, 71
128
ER_INCONSISTENT_PARTITION_INFO_ERROR error
code, 38
ER_INCONSISTENT_TYPE_OF_FUNCTIONS_ERROR
error code, 38
ER_INCORRECT_GLOBAL_LOCAL_VAR error code, 21
ER_INCORRECT_TYPE error code, 77
ER_INDEX_COLUMN_TOO_LONG error code, 54
ER_INDEX_CORRUPT error code, 54
ER_INDEX_REBUILD error code, 17
ER_INNODB_FORCED_RECOVERY error code, 69
ER_INNODB_FT_AUX_NOT_HEX_ID error code, 69
ER_INNODB_FT_LIMIT error code, 62
ER_INNODB_FT_WRONG_DOCID_COLUMN error
code, 62
ER_INNODB_FT_WRONG_DOCID_INDEX error code,
62
ER_INNODB_IMPORT_ERROR error code, 63
ER_INNODB_INDEX_CORRUPT error code, 63
ER_INNODB_NO_FT_TEMP_TABLE error code, 62
ER_INNODB_NO_FT_USES_PARSER error code, 68
ER_INNODB_ONLINE_LOG_TOO_BIG error code, 62
ER_INNODB_READ_ONLY error code, 69
ER_INNODB_UNDO_LOG_FULL error code, 72
ER_INSECURE_CHANGE_MASTER error code, 58
ER_INSECURE_PLAIN_TEXT error code, 58
ER_INSERT_INFO error code, 10
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_DIRECT
error code, 52
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_BINLOG_FORMAT
error code, 52
ER_INSIDE_TRANSACTION_PREVENTS_SWITCH_SQL_LOG_BIN
error code, 53
ER_INTERNAL_ERROR error code, 63
ER_INVALID_ARGUMENT_FOR_LOGARITHM error
code, 73
ER_INVALID_CAST_TO_JSON error code, 88
ER_INVALID_CHARACTER_STRING error code, 25
ER_INVALID_DEFAULT error code, 8
ER_INVALID_ENCRYPTION_OPTION error code, 92
ER_INVALID_FIELD_SIZE error code, 72
ER_INVALID_GEOJSON_MISSING_MEMBER error
code, 78
ER_INVALID_GEOJSON_UNSPECIFIED error code, 78
ER_INVALID_GEOJSON_WRONG_TYPE error code, 78
ER_INVALID_GROUP_FUNC_USE error code, 11
ER_INVALID_JSON_BINARY_DATA error code, 87
ER_INVALID_JSON_CHARSET error code, 87
ER_INVALID_JSON_CHARSET_IN_FUNCTION error
code, 87
ER_INVALID_JSON_DATA error code, 78
ER_INVALID_JSON_PATH error code, 87
ER_INVALID_JSON_PATH_ARRAY_CELL error code,
90
ER_INVALID_JSON_PATH_CHARSET error code, 88
ER_INVALID_JSON_PATH_WILDCARD error code, 88
ER_INVALID_JSON_TEXT error code, 87
ER_INVALID_JSON_TEXT_IN_PARAM error code, 87
ER_INVALID_JSON_VALUE_FOR_CAST error code, 89
ER_INVALID_ON_UPDATE error code, 24
ER_INVALID_RPL_WILD_TABLE_FILTER_PATTERN
error code, 78
ER_INVALID_TYPE_FOR_JSON error code, 88
ER_INVALID_USE_OF_NULL error code, 13
ER_INVALID_YEAR_COLUMN_LENGTH error code, 63
ER_IO_ERR_LOG_INDEX_READ error code, 30
ER_IO_READ_ERROR error code, 63
ER_IO_WRITE_ERROR error code, 63
ER_IPSOCK_ERROR error code, 9
ER_JSON_BAD_ONE_OR_ALL_ARG error code, 88
ER_JSON_DOCUMENT_NULL_KEY error code, 89
ER_JSON_DOCUMENT_TOO_DEEP error code, 89
ER_JSON_KEY_TOO_BIG error code, 88
ER_JSON_USED_AS_KEY error code, 88
ER_JSON_VACUOUS_PATH error code, 88
ER_JSON_VALUE_TOO_BIG error code, 88
ER_KEYRING_ACCESS_DENIED_ERROR error code,
94
ER_KEYRING_AWS_UDF_AWS_KMS_ERROR error
code, 93
ER_KEYRING_MIGRATION_FAILURE error code, 94
ER_KEYRING_MIGRATION_STATUS error code, 94
ER_KEYRING_UDF_KEYRING_SERVICE_ERROR
error code, 92
ER_KEY_BASED_ON_GENERATED_COLUMN error
code, 82
ER_KEY_COLUMN_DOES_NOT_EXITS error code, 9
ER_KEY_DOES_NOT_EXITS error code, 16
ER_KEY_NOT_FOUND error code, 6
ER_KEY_PART_0 error code, 31
ER_KEY_REF_DO_NOT_MATCH_TABLE_REF error
code, 21
ER_KILL_DENIED_ERROR error code, 10
ER_LIMITED_PART_RANGE error code, 40
ER_LIST_OF_FIELDS_ONLY_IN_HASH_ERROR error
code, 38
ER_LOAD_DATA_INVALID_COLUMN error code, 47
ER_LOAD_DATA_INVALID_COLUMN_UNUSED error
code, 47
ER_LOAD_FROM_FIXED_SIZE_ROWS_TO_VAR error
code, 32
ER_LOAD_INFO error code, 10
ER_LOCAL_VARIABLE error code, 20
ER_LOCKING_SERVICE_DEADLOCK error code, 86
ER_LOCKING_SERVICE_TIMEOUT error code, 86
ER_LOCKING_SERVICE_WRONG_NAME error code,
86
ER_LOCK_ABORTED error code, 52
ER_LOCK_DEADLOCK error code, 19
129
ER_LOCK_OR_ACTIVE_TRANSACTION error code, 17
ER_LOCK_REFUSED_BY_ENGINE error code, 91
ER_LOCK_TABLE_FULL error code, 18
ER_LOCK_WAIT_TIMEOUT error code, 18
ER_LOGGING_PROHIBIT_CHANGING_OF error code,
31
ER_LOG_IN_USE error code, 30
ER_LOG_PURGE_NO_FILE error code, 47
ER_LOG_PURGE_UNKNOWN_ERR error code, 30
ER_MALFORMED_DEFINER error code, 35
ER_MALFORMED_GTID_SET_ENCODING error code,
59
ER_MALFORMED_GTID_SET_SPECIFICATION error
code, 59
ER_MALFORMED_GTID_SPECIFICATION error code,
59
ER_MALFORMED_PACKET error code, 64
ER_MASTER error code, 17
ER_MASTER_DELAY_VALUE_OUT_OF_RANGE error
code, 56
ER_MASTER_FATAL_ERROR_READING_BINLOG
error code, 20
ER_MASTER_HAS_PURGED_REQUIRED_GTIDS error
code, 61
ER_MASTER_INFO error code, 18
ER_MASTER_KEY_ROTATION_BINLOG_FAILED error
code, 91
ER_MASTER_KEY_ROTATION_ERROR_BY_SE error
code, 91
ER_MASTER_KEY_ROTATION_NOT_SUPPORTED_BY_SE
error code, 91
ER_MASTER_KEY_ROTATION_SE_UNAVAILABLE
error code, 92
ER_MASTER_NET_READ error code, 17
ER_MASTER_NET_WRITE error code, 17
ER_MAXVALUE_IN_VALUES_IN error code, 50
ER_MAX_PREPARED_STMT_COUNT_REACHED error
code, 36
ER_MESSAGE_AND_STATEMENT error code, 51
ER_MISSING_HA_CREATE_OPTION error code, 72
ER_MISSING_KEY error code, 70
ER_MISSING_SKIP_SLAVE error code, 23
ER_MIXING_NOT_ALLOWED error code, 20
ER_MIX_HANDLER_ERROR error code, 38
ER_MIX_OF_GROUP_FUNC_AND_FIELDS error code,
14
ER_MIX_OF_GROUP_FUNC_AND_FIELDS_V2 error
code, 80
ER_MTS_CANT_PARALLEL error code, 57
ER_MTS_CHANGE_MASTER_CANT_RUN_WITH_GAPS
error code, 62
ER_MTS_EVENT_BIGGER_PENDING_JOBS_SIZE_MAX
error code, 68
ER_MTS_FEATURE_IS_NOT_SUPPORTED error code,
57
ER_MTS_INCONSISTENT_DATA error code, 58
ER_MTS_RECOVERY_FAILURE error code, 62
ER_MTS_RESET_WORKERS error code, 62
ER_MTS_UPDATED_DBS_GREATER_MAX error code,
57
ER_MULTIPLE_DEF_CONST_IN_LIST_PART_ERROR
error code, 38
ER_MULTIPLE_PRI_KEY error code, 8
ER_MULTI_UPDATE_KEY_CONFLICT error code, 54
ER_MUST_CHANGE_PASSWORD error code, 63
ER_MUST_CHANGE_PASSWORD_LOGIN error code,
67
ER_M_BIGGER_THAN_D error code, 33
ER_NAME_BECOMES_EMPTY error code, 37
ER_NATIVE_FCT_NAME_COLLISION error code, 45
ER_NDB_CANT_SWITCH_BINLOG_FORMAT error
code, 43
ER_NDB_REPLICATION_SCHEMA_ERROR error code,
48
ER_NEED_REPREPARE error code, 47
ER_NETWORK_READ_EVENT_CHECKSUM_FAILURE
error code, 57
ER_NET_ERROR_ON_WRITE error code, 15
ER_NET_FCNTL_ERROR error code, 15
ER_NET_OK_PACKET_TOO_LARGE error code, 78
ER_NET_PACKETS_OUT_OF_ORDER error code, 15
ER_NET_PACKET_TOO_LARGE error code, 14
ER_NET_READ_ERROR error code, 15
ER_NET_READ_ERROR_FROM_PIPE error code, 15
ER_NET_READ_INTERRUPTED error code, 15
ER_NET_UNCOMPRESS_ERROR error code, 15
ER_NET_WRITE_INTERRUPTED error code, 15
ER_NEVER_USED error code, 46
ER_NEW_ABORTING_CONNECTION error code, 17
ER_NISAMCHK error code, 3
ER_NO error code, 3
ER_NONEXISTING_GRANT error code, 14
ER_NONEXISTING_PROC_GRANT error code, 32
ER_NONEXISTING_TABLE_GRANT error code, 14
ER_NONUNIQ_TABLE error code, 8
ER_NONUPDATEABLE_COLUMN error code, 28
ER_NON_DEFAULT_VALUE_FOR_GENERATED_COLUMN
error code, 82
ER_NON_GROUPING_FIELD_USED error code, 36
ER_NON_INSERTABLE_TABLE error code, 37
ER_NON_RO_SELECT_DISABLE_TIMER error code,
73
ER_NON_UNIQ_ERROR error code, 7
ER_NON_UPDATABLE_TABLE error code, 24
ER_NORMAL_SHUTDOWN error code, 9
ER_NOT_ALLOWED_COMMAND error code, 14
ER_NOT_FORM_FILE error code, 6
130
ER_NOT_KEYFILE error code, 6
ER_NOT_SUPPORTED_AUTH_MODE error code, 21
ER_NOT_SUPPORTED_YET error code, 20
ER_NOT_VALID_PASSWORD error code, 63
ER_NO_BINARY_LOGGING error code, 30
ER_NO_BINLOG_ERROR error code, 40
ER_NO_CONST_EXPR_IN_RANGE_OR_LIST_ERROR
error code, 38
ER_NO_DB_ERROR error code, 7
ER_NO_DEFAULT error code, 20
ER_NO_DEFAULT_FOR_FIELD error code, 29
ER_NO_DEFAULT_FOR_VIEW_FIELD error code, 33
ER_NO_FILE_MAPPING error code, 31
ER_NO_FORMAT_DESCRIPTION_EVENT_BEFORE_BINLOG_STATEMENT
error code, 46
ER_NO_FT_MATERIALIZED_SUBQUERY error code,
72
ER_NO_GROUP_FOR_PROC error code, 31
ER_NO_PARTITION_FOR_GIVEN_VALUE error code,
40
ER_NO_PARTITION_FOR_GIVEN_VALUE_SILENT
error code, 45
ER_NO_PARTS_ERROR error code, 39
ER_NO_PERMISSION_TO_CREATE_USER error code,
19
ER_NO_RAID_COMPILED error code, 16
ER_NO_REFERENCED_ROW error code, 19
ER_NO_REFERENCED_ROW_2 error code, 35
ER_NO_SECURE_TRANSPORTS_CONFIGURED error
code, 89
ER_NO_SUCH_INDEX error code, 9
ER_NO_SUCH_KEY_VALUE error code, 56
ER_NO_SUCH_PARTITION__UNUSED error code, 57
ER_NO_SUCH_TABLE error code, 14
ER_NO_SUCH_THREAD error code, 10
ER_NO_SUCH_USER error code, 35
ER_NO_TABLES_USED error code, 10
ER_NO_TRIGGERS_ON_SYSTEM_SCHEMA error
code, 36
ER_NO_UNIQUE_LOGFILE error code, 11
ER_NULL_COLUMN_IN_INDEX error code, 12
ER_NULL_IN_VALUES_LESS_THAN error code, 43
ER_NUMERIC_JSON_VALUE_OUT_OF_RANGE error
code, 88
ER_OBSOLETE_CANNOT_LOAD_FROM_TABLE error
code, 42
ER_OBSOLETE_COL_COUNT_DOESNT_MATCH_CORRUPTED
error code, 42
ER_OLD_FILE_FORMAT error code, 36
ER_OLD_KEYFILE error code, 6
ER_OLD_TEMPORALS_UPGRADED error code, 69
ER_ONLY_FD_AND_RBR_EVENTS_ALLOWED_IN_BINLOG_STATEMENT
error code, 56
ER_ONLY_INTEGERS_ALLOWED error code, 44
ER_ONLY_ON_RANGE_LIST_PARTITION error code,
39
ER_OPEN_AS_READONLY error code, 6
ER_OPERAND_COLUMNS error code, 21
ER_OPTION_PREVENTS_STATEMENT error code, 24
ER_ORDER_WITH_PROC error code, 31
ER_OUTOFMEMORY error code, 6
ER_OUT_OF_RESOURCES error code, 6
ER_OUT_OF_SORTMEMORY error code, 6
ER_PARSE_ERROR error code, 8
ER_PARTITIONS_MUST_BE_DEFINED_ERROR error
code, 38
ER_PARTITION_CLAUSE_ON_NONPARTITIONED
error code, 57
ER_PARTITION_COLUMN_LIST_ERROR error code,
50
ER_PARTITION_CONST_DOMAIN_ERROR error code,
43
ER_PARTITION_ENGINE_DEPRECATED_FOR_TABLE
error code, 93
ER_PARTITION_ENTRY_ERROR error code, 38
ER_PARTITION_EXCHANGE_DIFFERENT_OPTION
error code, 56
ER_PARTITION_EXCHANGE_FOREIGN_KEY error
code, 56
ER_PARTITION_EXCHANGE_PART_TABLE error
code, 56
ER_PARTITION_EXCHANGE_TEMP_TABLE error
code, 56
ER_PARTITION_FIELDS_TOO_LONG error code, 50
ER_PARTITION_FUNCTION_FAILURE error code, 40
ER_PARTITION_FUNCTION_IS_NOT_ALLOWED error
code, 43
ER_PARTITION_FUNC_NOT_ALLOWED_ERROR error
code, 38
ER_PARTITION_INSTEAD_OF_SUBPARTITION error
code, 56
ER_PARTITION_MAXVALUE_ERROR error code, 37
ER_PARTITION_MERGE_ERROR error code, 44
ER_PARTITION_MGMT_ON_NONPARTITIONED error
code, 39
ER_PARTITION_NAME error code, 48
ER_PARTITION_NOT_DEFINED_ERROR error code,
38
ER_PARTITION_NO_TEMPORARY error code, 43
ER_PARTITION_REQUIRES_VALUES_ERROR error
code, 37
ER_PARTITION_SUBPARTITION_ERROR error code,
37
ER_PARTITION_SUBPART_MIX_ERROR error code,
37
ER_PARTITION_WRONG_NO_PART_ERROR error
code, 38
131
ER_PARTITION_WRONG_NO_SUBPART_ERROR
error code, 38
ER_PARTITION_WRONG_VALUES_ERROR error
code, 37
ER_PART_STATE_ERROR error code, 40
ER_PASSWD_LENGTH error code, 30
ER_PASSWORD_ANONYMOUS_USER error code, 13
ER_PASSWORD_EXPIRE_ANONYMOUS_USER error
code, 72
ER_PASSWORD_FORMAT error code, 64
ER_PASSWORD_NOT_ALLOWED error code, 13
ER_PASSWORD_NO_MATCH error code, 13
ER_PATH_LENGTH error code, 52
ER_PLUGGABLE_PROTOCOL_COMMAND_NOT_SUPPORTED
error code, 86
ER_PLUGIN_CANNOT_BE_UNINSTALLED error code,
70
ER_PLUGIN_DELETE_BUILTIN error code, 47
ER_PLUGIN_FAILED_TO_OPEN_TABLE error code, 94
ER_PLUGIN_FAILED_TO_OPEN_TABLES error code,
94
ER_PLUGIN_IS_NOT_LOADED error code, 40
ER_PLUGIN_IS_PERMANENT error code, 53
ER_PLUGIN_NO_INSTALL error code, 55
ER_PLUGIN_NO_UNINSTALL error code, 55
ER_PREVENTS_VARIABLE_WITHOUT_RBR error
code, 81
ER_PRIMARY_CANT_HAVE_NULL error code, 16
ER_PROCACCESS_DENIED_ERROR error code, 30
ER_PROC_AUTO_GRANT_FAIL error code, 32
ER_PROC_AUTO_REVOKE_FAIL error code, 32
ER_PS_MANY_PARAM error code, 31
ER_PS_NO_RECURSION error code, 35
ER_QUERY_CACHE_DISABLED error code, 50
ER_QUERY_INTERRUPTED error code, 26
ER_QUERY_ON_FOREIGN_DATA_SOURCE error
code, 34
ER_QUERY_ON_MASTER error code, 19
ER_QUERY_TIMEOUT error code, 73
ER_RANGE_NOT_INCREASING_ERROR error code,
38
ER_RBR_NOT_AVAILABLE error code, 44
ER_READY error code, 9
ER_READ_ONLY_MODE error code, 65
ER_READ_ONLY_TRANSACTION error code, 18
ER_RECORD_FILE_FULL error code, 12
ER_REFERENCED_TRG_DOES_NOT_EXIST error
code, 72
ER_REGEXP_ERROR error code, 14
ER_RELAY_LOG_FAIL error code, 30
ER_RELAY_LOG_INIT error code, 30
ER_REMOVED_SPACES error code, 36
ER_RENAMED_NAME error code, 49
ER_REORG_HASH_ONLY_ON_SAME_NO error code,
39
ER_REORG_NO_PARAM_ERROR error code, 39
ER_REORG_OUTSIDE_RANGE error code, 40
ER_REORG_PARTITION_NOT_EXIST error code, 40
ER_REPLACE_INACCESSIBLE_ROWS error code, 77
ER_REQUIRES_PRIMARY_KEY error code, 16
ER_RESERVED_SYNTAX error code, 30
ER_RESIGNAL_WITHOUT_ACTIVE_HANDLER error
code, 49
ER_REVOKE_GRANTS error code, 23
ER_ROW_DOES_NOT_MATCH_GIVEN_PARTITION_SET
error code, 57
ER_ROW_DOES_NOT_MATCH_PARTITION error
code, 56
ER_ROW_IN_WRONG_PARTITION error code, 68
ER_ROW_IS_REFERENCED error code, 19
ER_ROW_IS_REFERENCED_2 error code, 35
ER_ROW_SINGLE_PARTITION_FIELD_ERROR error
code, 50
ER_RPL_INFO_DATA_TOO_LONG error code, 56
ER_RUN_HOOK_ERROR error code, 82
ER_SAME_NAME_PARTITION error code, 40
ER_SAME_NAME_PARTITION_FIELD error code, 50
ER_SECURE_TRANSPORT_REQUIRED error code, 89
ER_SELECT_REDUCED error code, 21
ER_SERVER_ISNT_AVAILABLE error code, 90
ER_SERVER_IS_IN_SECURE_AUTH_MODE error
code, 23
ER_SERVER_OFFLINE_MODE error code, 74
ER_SERVER_SHUTDOWN error code, 7
ER_SESSION_WAS_KILLED error code, 90
ER_SET_CONSTANTS_ONLY error code, 18
ER_SET_ENFORCE_GTID_CONSISTENCY_WARN_WITH_ONGOINerror code, 84
ER_SET_PASSWORD_AUTH_PLUGIN error code, 53
ER_SET_STATEMENT_CANNOT_INVOKE_FUNCTION
error code, 59
ER_SHUTDOWN_COMPLETE error code, 9
ER_SIGNAL_BAD_CONDITION_TYPE error code, 49
ER_SIGNAL_EXCEPTION error code, 49
ER_SIGNAL_NOT_FOUND error code, 49
ER_SIGNAL_WARN error code, 49
ER_SIZE_OVERFLOW_ERROR error code, 41
ER_SKIPPING_LOGGED_TRANSACTION error code,
59
ER_SLAVE_CANT_CREATE_CONVERSION error code,
52
ER_SLAVE_CHANNEL_DELETE error code, 79
ER_SLAVE_CHANNEL_DOES_NOT_EXIST error code,
78
ER_SLAVE_CHANNEL_IO_THREAD_MUST_STOP
error code, 73
ER_SLAVE_CHANNEL_MUST_STOP error code, 79
132
ER_SLAVE_CHANNEL_NAME_INVALID_OR_TOO_LONG
error code, 79
ER_SLAVE_CHANNEL_NOT_RUNNING error code, 79
ER_SLAVE_CHANNEL_OPERATION_NOT_ALLOWED
error code, 87
ER_SLAVE_CHANNEL_SQL_SKIP_COUNTER error
code, 80
ER_SLAVE_CHANNEL_SQL_THREAD_MUST_STOP
error code, 80
ER_SLAVE_CHANNEL_WAS_NOT_RUNNING error
code, 80
ER_SLAVE_CHANNEL_WAS_RUNNING error code, 79
ER_SLAVE_CONFIGURATION error code, 61
ER_SLAVE_CONVERSION_FAILED error code, 52
ER_SLAVE_CORRUPT_EVENT error code, 47
ER_SLAVE_CREATE_EVENT_FAILURE error code, 46
ER_SLAVE_FATAL_ERROR error code, 45
ER_SLAVE_HAS_MORE_GTIDS_THAN_MASTER error
code, 70
ER_SLAVE_HEARTBEAT_FAILURE error code, 48
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE
error code, 48
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MAX
error code, 54
ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE_MIN
error code, 53
ER_SLAVE_IGNORED_SSL_PARAMS error code, 23
ER_SLAVE_IGNORED_TABLE error code, 21
ER_SLAVE_IGNORE_SERVER_IDS error code, 49
ER_SLAVE_INCIDENT error code, 45
ER_SLAVE_IO_THREAD_MUST_STOP error code, 71
ER_SLAVE_MASTER_COM_FAILURE error code, 46
ER_SLAVE_MAX_CHANNELS_EXCEEDED error code,
79
ER_SLAVE_MI_INIT_REPOSITORY error code, 68
ER_SLAVE_MULTIPLE_CHANNELS_CMD error code,
79
ER_SLAVE_MULTIPLE_CHANNELS_HOST_PORT
error code, 79
ER_SLAVE_MUST_STOP error code, 17
ER_SLAVE_NEW_CHANNEL_WRONG_REPOSITORY
error code, 79
ER_SLAVE_NOT_RUNNING error code, 18
ER_SLAVE_RELAY_LOG_READ_FAILURE error code,
45
ER_SLAVE_RELAY_LOG_WRITE_FAILURE error code,
45
ER_SLAVE_RLI_INIT_REPOSITORY error code, 69
ER_SLAVE_SILENT_RETRY_TRANSACTION error
code, 62
ER_SLAVE_SQL_THREAD_MUST_STOP error code,
72
ER_SLAVE_THREAD error code, 18
ER_SLAVE_WAS_NOT_RUNNING error code, 22
ER_SLAVE_WAS_RUNNING error code, 22
ER_SLAVE_WORKER_STOPPED_PREVIOUS_THD_ERROR
error code, 74
ER_SPATIAL_CANT_HAVE_NULL error code, 22
ER_SPATIAL_MUST_HAVE_GEOM_COL error code, 52
ER_SPECIFIC_ACCESS_DENIED_ERROR error code,
20
ER_SP_ALREADY_EXISTS error code, 25
ER_SP_BADRETURN error code, 26
ER_SP_BADSELECT error code, 26
ER_SP_BADSTATEMENT error code, 26
ER_SP_BAD_CURSOR_QUERY error code, 26
ER_SP_BAD_CURSOR_SELECT error code, 26
ER_SP_BAD_SQLSTATE error code, 32
ER_SP_BAD_VAR_SHADOW error code, 35
ER_SP_CANT_ALTER error code, 27
ER_SP_CANT_SET_AUTOCOMMIT error code, 35
ER_SP_CASE_NOT_FOUND error code, 27
ER_SP_COND_MISMATCH error code, 26
ER_SP_CURSOR_AFTER_HANDLER error code, 27
ER_SP_CURSOR_ALREADY_OPEN error code, 26
ER_SP_CURSOR_MISMATCH error code, 26
ER_SP_CURSOR_NOT_OPEN error code, 27
ER_SP_DOES_NOT_EXIST error code, 25
ER_SP_DROP_FAILED error code, 25
ER_SP_DUP_COND error code, 27
ER_SP_DUP_CURS error code, 27
ER_SP_DUP_HANDLER error code, 32
ER_SP_DUP_PARAM error code, 27
ER_SP_DUP_VAR error code, 27
ER_SP_FETCH_NO_DATA error code, 27
ER_SP_GOTO_IN_HNDLR error code, 29
ER_SP_LABEL_MISMATCH error code, 25
ER_SP_LABEL_REDEFINE error code, 25
ER_SP_LILABEL_MISMATCH error code, 25
ER_SP_NORETURN error code, 26
ER_SP_NORETURNEND error code, 26
ER_SP_NOT_VAR_ARG error code, 33
ER_SP_NO_AGGREGATE error code, 36
ER_SP_NO_DROP_SP error code, 29
ER_SP_NO_RECURSION error code, 33
ER_SP_NO_RECURSIVE_CREATE error code, 25
ER_SP_NO_RETSET error code, 33
ER_SP_PROC_TABLE_CORRUPT error code, 36
ER_SP_RECURSION_LIMIT error code, 36
ER_SP_STORE_FAILED error code, 25
ER_SP_SUBSELECT_NYI error code, 27
ER_SP_UNDECLARED_VAR error code, 27
ER_SP_UNINIT_VAR error code, 26
ER_SP_VARCOND_AFTER_CURSHNDLR error code,
27
ER_SP_WRONG_NAME error code, 36
ER_SP_WRONG_NO_OF_ARGS error code, 26
133
ER_SP_WRONG_NO_OF_FETCH_ARGS error code,
27
ER_SQLTHREAD_WITH_SECURE_SLAVE error code,
58
ER_SQL_MODE_MERGED error code, 86
ER_SQL_MODE_NO_EFFECT error code, 73
ER_SQL_SLAVE_SKIP_COUNTER_NOT_SETTABLE_IN_GTID_MODE
error code, 67
ER_SR_INVALID_CREATION_CTX error code, 46
ER_STACK_OVERRUN error code, 12
ER_STACK_OVERRUN_NEED_MORE error code, 34
ER_STARTUP error code, 32
ER_STD_BAD_ALLOC_ERROR error code, 75
ER_STD_DOMAIN_ERROR error code, 75
ER_STD_INVALID_ARGUMENT error code, 76
ER_STD_LENGTH_ERROR error code, 76
ER_STD_LOGIC_ERROR error code, 76
ER_STD_OUT_OF_RANGE_ERROR error code, 76
ER_STD_OVERFLOW_ERROR error code, 76
ER_STD_RANGE_ERROR error code, 76
ER_STD_RUNTIME_ERROR error code, 76
ER_STD_UNDERFLOW_ERROR error code, 76
ER_STD_UNKNOWN_EXCEPTION error code, 76
ER_STMT_CACHE_FULL error code, 54
ER_STMT_HAS_NO_OPEN_CURSOR error code, 33
ER_STMT_NOT_ALLOWED_IN_SF_OR_TRG error
code, 27
ER_STOP_SLAVE_IO_THREAD_TIMEOUT error code,
69
ER_STOP_SLAVE_SQL_THREAD_TIMEOUT error
code, 69
ER_STORAGE_ENGINE_NOT_LOADED error code, 71
ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_DIRECT
error code, 52
ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_FORMAT
error code, 43
ER_STORED_FUNCTION_PREVENTS_SWITCH_SQL_LOG_BIN
error code, 53
ER_SUBPARTITION_ERROR error code, 39
ER_SUBPARTITION_NAME error code, 48
ER_SUBQUERY_NO_1_ROW error code, 21
ER_SYNTAX_ERROR error code, 14
ER_TABLEACCESS_DENIED_ERROR error code, 14
ER_TABLENAME_NOT_ALLOWED_HERE error code,
21
ER_TABLESPACE_AUTO_EXTEND_ERROR error
code, 41
ER_TABLESPACE_CANNOT_ENCRYPT error code, 92
ER_TABLESPACE_DISCARDED error code, 63
ER_TABLESPACE_EXISTS error code, 63
ER_TABLESPACE_IS_NOT_EMPTY error code, 84
ER_TABLESPACE_MISSING error code, 63
ER_TABLES_DIFFERENT_METADATA error code, 56
ER_TABLE_CANT_HANDLE_AUTO_INCREMENT error
code, 15
ER_TABLE_CANT_HANDLE_BLOB error code, 15
ER_TABLE_CANT_HANDLE_FT error code, 19
ER_TABLE_CANT_HANDLE_SPKEYS error code, 36
ER_TABLE_CORRUPT error code, 69
ER_TABLE_DEF_CHANGED error code, 32
ER_TABLE_EXISTS_ERROR error code, 7
ER_TABLE_HAS_NO_FT error code, 58
ER_TABLE_IN_FK_CHECK error code, 55
ER_TABLE_IN_SYSTEM_TABLESPACE error code, 63
ER_TABLE_MUST_HAVE_COLUMNS error code, 12
ER_TABLE_NAME error code, 48
ER_TABLE_NEEDS_REBUILD error code, 54
ER_TABLE_NEEDS_UPGRADE error code, 36
ER_TABLE_NEEDS_UPG_PART error code, 90
ER_TABLE_NOT_LOCKED error code, 11
ER_TABLE_NOT_LOCKED_FOR_WRITE error code, 11
ER_TABLE_REFERENCED error code, 93
ER_TABLE_SCHEMA_MISMATCH error code, 62
ER_TEMPORARY_NAME error code, 48
ER_TEMP_FILE_WRITE_FAILURE error code, 69
ER_TEMP_TABLE_PREVENTS_SWITCH_OUT_OF_RBR
error code, 43
ER_TEXTFILE_NOT_READABLE error code, 10
ER_TOO_BIG_DISPLAYWIDTH error code, 34
ER_TOO_BIG_FIELDLENGTH error code, 9
ER_TOO_BIG_FOR_UNCOMPRESS error code, 22
ER_TOO_BIG_PRECISION error code, 33
ER_TOO_BIG_ROWSIZE error code, 12
ER_TOO_BIG_SCALE error code, 33
ER_TOO_BIG_SELECT error code, 11
ER_TOO_BIG_SET error code, 11
ER_TOO_HIGH_LEVEL_OF_NESTING_FOR_SELECT
error code, 37
ER_TOO_LONG_BODY error code, 34
ER_TOO_LONG_FIELD_COMMENT error code, 48
ER_TOO_LONG_IDENT error code, 8
ER_TOO_LONG_INDEX_COMMENT error code, 52
ER_TOO_LONG_KEY error code, 9
ER_TOO_LONG_STRING error code, 15
ER_TOO_LONG_TABLE_COMMENT error code, 48
ER_TOO_LONG_TABLE_PARTITION_COMMENT error
code, 61
ER_TOO_MANY_CONCURRENT_TRXS error code, 49
ER_TOO_MANY_FIELDS error code, 12
ER_TOO_MANY_KEYS error code, 9
ER_TOO_MANY_KEY_PARTS error code, 9
ER_TOO_MANY_PARTITIONS_ERROR error code, 39
ER_TOO_MANY_PARTITION_FUNC_FIELDS_ERROR
error code, 50
ER_TOO_MANY_ROWS error code, 16
ER_TOO_MANY_TABLES error code, 12
ER_TOO_MANY_USER_CONNECTIONS error code, 18
134
ER_TOO_MANY_VALUES_ERROR error code, 50
ER_TOO_MUCH_AUTO_TIMESTAMP_COLS error
code, 24
ER_TRANSACTION_ROLLBACK_DURING_COMMIT
error code, 82
ER_TRANS_CACHE_FULL error code, 17
ER_TRG_ALREADY_EXISTS error code, 29
ER_TRG_CANT_CHANGE_ROW error code, 29
ER_TRG_CANT_OPEN_TABLE error code, 46
ER_TRG_CORRUPTED_FILE error code, 46
ER_TRG_DOES_NOT_EXIST error code, 29
ER_TRG_INVALID_CREATION_CTX error code, 46
ER_TRG_IN_WRONG_SCHEMA error code, 34
ER_TRG_NO_CREATION_CTX error code, 46
ER_TRG_NO_DEFINER error code, 35
ER_TRG_NO_SUCH_ROW_IN_TRG error code, 29
ER_TRG_ON_VIEW_OR_TEMP_TABLE error code, 29
ER_TRUNCATED_WRONG_VALUE error code, 24
ER_TRUNCATED_WRONG_VALUE_FOR_FIELD error
code, 29
ER_TRUNCATE_ILLEGAL_FK error code, 53
ER_UDF_ERROR error code, 94
ER_UDF_EXISTS error code, 13
ER_UDF_NO_PATHS error code, 12
ER_UNDO_RECORD_TOO_BIG error code, 54
ER_UNEXPECTED_EOF error code, 6
ER_UNION_TABLES_IN_DIFFERENT_DIR error code,
19
ER_UNIQUE_KEY_NEED_ALL_FIELDS_IN_PF error
code, 39
ER_UNKNOWN_ALTER_ALGORITHM error code, 62
ER_UNKNOWN_ALTER_LOCK error code, 62
ER_UNKNOWN_CHARACTER_SET error code, 12
ER_UNKNOWN_COLLATION error code, 23
ER_UNKNOWN_COM_ERROR error code, 7
ER_UNKNOWN_ERROR error code, 11
ER_UNKNOWN_EXPLAIN_FORMAT error code, 61
ER_UNKNOWN_KEY_CACHE error code, 24
ER_UNKNOWN_LOCALE error code, 49
ER_UNKNOWN_PARTITION error code, 56
ER_UNKNOWN_PROCEDURE error code, 11
ER_UNKNOWN_STMT_HANDLER error code, 21
ER_UNKNOWN_STORAGE_ENGINE error code, 24
ER_UNKNOWN_SYSTEM_VARIABLE error code, 17
ER_UNKNOWN_TABLE error code, 11
ER_UNKNOWN_TARGET_BINLOG error code, 30
ER_UNKNOWN_TIME_ZONE error code, 25
ER_UNRESOLVED_HINT_NAME error code, 85
ER_UNSUPORTED_LOG_ENGINE error code, 44
ER_UNSUPPORTED_ACTION_ON_GENERATED_COLUMN
error code, 82
ER_UNSUPPORTED_ALTER_ENCRYPTION_INPLACE
error code, 92
ER_UNSUPPORTED_ALTER_INPLACE_ON_VIRTUAL_COLUMerror code, 82
ER_UNSUPPORTED_ALTER_ONLINE_ON_VIRTUAL_COLUMNerror code, 91
ER_UNSUPPORTED_BY_REPLICATION_THREAD
error code, 77
ER_UNSUPPORTED_ENGINE error code, 55
ER_UNSUPPORTED_EXTENSION error code, 12
ER_UNSUPPORTED_PS error code, 24
ER_UNTIL_COND_IGNORED error code, 23
ER_UNUSED1 error code, 14
ER_UNUSED2 error code, 14
ER_UNUSED3 error code, 15
ER_UNUSED4 error code, 51
ER_UNUSED5 error code, 64
ER_UNUSED6 error code, 66
ER_UPDATE_INFO error code, 13
ER_UPDATE_LOG_DEPRECATED_IGNORED error
code, 26
ER_UPDATE_LOG_DEPRECATED_TRANSLATED
error code, 26
ER_UPDATE_TABLE_USED error code, 10
ER_UPDATE_WITHOUT_KEY_IN_SAFE_MODE error
code, 16
ER_USERNAME error code, 36
ER_USER_ALREADY_EXISTS error code, 89
ER_USER_COLUMN_OLD_LENGTH error code, 92
ER_USER_DOES_NOT_EXIST error code, 89
ER_USER_LIMIT_REACHED error code, 20
ER_USER_LOCK_DEADLOCK error code, 77
ER_USER_LOCK_WRONG_NAME error code, 77
ER_VALUES_IS_NOT_INT_TYPE_ERROR error code,
53
ER_VARIABLE_IS_NOT_STRUCT error code, 23
ER_VARIABLE_IS_READONLY error code, 48
ER_VARIABLE_NOT_SETTABLE_IN_SF_OR_TRIGGER
error code, 58
ER_VARIABLE_NOT_SETTABLE_IN_SP error code, 65
ER_VARIABLE_NOT_SETTABLE_IN_TRANSACTION
error code, 58
ER_VAR_CANT_BE_READ error code, 20
ER_VIEW_CHECKSUM error code, 31
ER_VIEW_CHECK_FAILED error code, 30
ER_VIEW_DELETE_MERGE_VIEW error code, 31
ER_VIEW_FRM_NO_USER error code, 35
ER_VIEW_INVALID error code, 29
ER_VIEW_INVALID_CREATION_CTX error code, 46
ER_VIEW_MULTIUPDATE error code, 31
ER_VIEW_NONUPD_CHECK error code, 29
ER_VIEW_NO_CREATION_CTX error code, 46
ER_VIEW_NO_EXPLAIN error code, 28
ER_VIEW_NO_INSERT_FIELD_LIST error code, 31
ER_VIEW_OTHER_USER error code, 35
ER_VIEW_PREVENT_UPDATE error code, 35
135
ER_VIEW_RECURSIVE error code, 36
ER_VIEW_SELECT_CLAUSE error code, 28
ER_VIEW_SELECT_DERIVED error code, 28
ER_VIEW_SELECT_DERIVED_UNUSED error code, 28
ER_VIEW_SELECT_TMPTABLE error code, 28
ER_VIEW_SELECT_VARIABLE error code, 28
ER_VIEW_WRONG_LIST error code, 28
ER_VTOKEN_PLUGIN_TOKEN_MISMATCH error code,
86
ER_VTOKEN_PLUGIN_TOKEN_NOT_FOUND error
code, 87
ER_WARNING_NOT_COMPLETE_ROLLBACK error
code, 17
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_CREATED_TEMP_TABLE
error code, 57
ER_WARNING_NOT_COMPLETE_ROLLBACK_WITH_DROPPED_TEMP_TABLE
error code, 57
ER_WARN_ALLOWED_PACKET_OVERFLOWED error
code, 25
ER_WARN_BAD_MAX_EXECUTION_TIME error code,
85
ER_WARN_CANT_DROP_DEFAULT_KEYCACHE error
code, 34
ER_WARN_CONFLICTING_HINT error code, 85
ER_WARN_DATA_OUT_OF_RANGE error code, 22
ER_WARN_DEPRECATED_SQLMODE error code, 80
ER_WARN_DEPRECATED_SQLMODE_UNSET error
code, 85
ER_WARN_DEPRECATED_SYNTAX error code, 24
ER_WARN_DEPRECATED_SYNTAX_NO_REPLACEMENT
error code, 52
ER_WARN_DEPRECATED_SYNTAX_WITH_VER error
code, 43
ER_WARN_DEPRECATED_SYSVAR_UPDATE error
code, 80
ER_WARN_DEPRECATED_TLS_VERSION error code,
97
ER_WARN_ENGINE_TRANSACTION_ROLLBACK error
code, 48
ER_WARN_FIELD_RESOLVED error code, 23
ER_WARN_HOSTNAME_WONT_WORK error code, 24
ER_WARN_INDEX_NOT_APPLICABLE error code, 56
ER_WARN_INVALID_TIMESTAMP error code, 25
ER_WARN_I_S_SKIPPED_TABLE error code, 52
ER_WARN_LEGACY_SYNTAX_CONVERTED error
code, 71
ER_WARN_NULL_TO_NOTNULL error code, 22
ER_WARN_ONLY_MASTER_LOG_FILE_NO_POS error
code, 73
ER_WARN_ON_MODIFYING_GTID_EXECUTED_TABLE
error code, 86
ER_WARN_OPEN_TEMP_TABLES_MUST_BE_ZERO
error code, 73
ER_WARN_OPTIMIZER_HINT_SYNTAX_ERROR error
code, 85
ER_WARN_PURGE_LOG_IN_USE error code, 68
ER_WARN_PURGE_LOG_IS_ACTIVE error code, 68
ER_WARN_QC_RESIZE error code, 24
ER_WARN_TOO_FEW_RECORDS error code, 22
ER_WARN_TOO_MANY_RECORDS error code, 22
ER_WARN_TRIGGER_DOESNT_HAVE_CREATED
error code, 72
ER_WARN_UNKNOWN_QB_NAME error code, 85
ER_WARN_UNSUPPORTED_MAX_EXECUTION_TIME
error code, 85
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID
error code, 93
ER_WARN_USING_GEOMFROMWKB_TO_SET_SRID_ZERO
error code, 93
ER_WARN_USING_OTHER_HANDLER error code, 22
ER_WARN_VIEW_MERGE error code, 29
ER_WARN_VIEW_WITHOUT_KEY error code, 29
ER_WARN_WRONG_NATIVE_TABLE_STRUCTURE
error code, 97
ER_WRITE_SET_EXCEEDS_LIMIT error code, 97
ER_WRONG_ARGUMENTS error code, 18
ER_WRONG_AUTO_KEY error code, 9
ER_WRONG_COLUMN_NAME error code, 15
ER_WRONG_DB_NAME error code, 11
ER_WRONG_EXPR_IN_PARTITION_FUNC_ERROR
error code, 38
ER_WRONG_FIELD_SPEC error code, 8
ER_WRONG_FIELD_TERMINATORS error code, 9
ER_WRONG_FIELD_WITH_GROUP error code, 8
ER_WRONG_FIELD_WITH_GROUP_V2 error code, 80
ER_WRONG_FILE_NAME error code, 85
ER_WRONG_FK_DEF error code, 21
ER_WRONG_FK_OPTION_FOR_GENERATED_COLUMN
error code, 82
ER_WRONG_GROUP_FIELD error code, 8
ER_WRONG_KEY_COLUMN error code, 15
ER_WRONG_LOCK_OF_SYSTEM_TABLE error code,
34
ER_WRONG_MAGIC error code, 31
ER_WRONG_MRG_TABLE error code, 15
ER_WRONG_NAME_FOR_CATALOG error code, 24
ER_WRONG_NAME_FOR_INDEX error code, 23
ER_WRONG_NATIVE_TABLE_STRUCTURE error
code, 52
ER_WRONG_NUMBER_OF_COLUMNS_IN_SELECT
error code, 20
ER_WRONG_OBJECT error code, 28
ER_WRONG_OUTER_JOIN error code, 12
ER_WRONG_PARAMCOUNT_TO_NATIVE_FCT error
code, 45
ER_WRONG_PARAMCOUNT_TO_PROCEDURE error
code, 11
136
ER_WRONG_PARAMETERS_TO_NATIVE_FCT error
code, 45
ER_WRONG_PARAMETERS_TO_PROCEDURE error
code, 11
ER_WRONG_PARAMETERS_TO_STORED_FCT error
code, 45
ER_WRONG_PARTITION_NAME error code, 43
ER_WRONG_PERFSCHEMA_USAGE error code, 52
ER_WRONG_SIZE_NUMBER error code, 41
ER_WRONG_SPVAR_TYPE_IN_LIMIT error code, 53
ER_WRONG_STRING_LENGTH error code, 37
ER_WRONG_SUB_KEY error code, 10
ER_WRONG_SUM_SELECT error code, 8
ER_WRONG_TABLESPACE_NAME error code, 84
ER_WRONG_TABLE_NAME error code, 11
ER_WRONG_TYPE_COLUMN_VALUE_ERROR error
code, 50
ER_WRONG_TYPE_FOR_VAR error code, 20
ER_WRONG_USAGE error code, 19
ER_WRONG_VALUE error code, 40
ER_WRONG_VALUE_COUNT error code, 8
ER_WRONG_VALUE_COUNT_ON_ROW error code, 13
ER_WRONG_VALUE_FOR_TYPE error code, 32
ER_WRONG_VALUE_FOR_VAR error code, 20
ER_WSAS_FAILED error code, 30
ER_XAER_DUPID error code, 34
ER_XAER_INVAL error code, 31
ER_XAER_NOTA error code, 31
ER_XAER_OUTSIDE error code, 32
ER_XAER_RMERR error code, 32
ER_XAER_RMFAIL error code, 32
ER_XA_RBDEADLOCK error code, 47
ER_XA_RBROLLBACK error code, 32
ER_XA_RBTIMEOUT error code, 47
ER_XA_REPLICATION_FILTERS error code, 97
ER_XA_RETRY error code, 93
ER_YES error code, 3
ER_ZLIB_Z_BUF_ERROR error code, 22
ER_ZLIB_Z_DATA_ERROR error code, 22
ER_ZLIB_Z_MEM_ERROR error code, 22
W
WARN_AES_KEY_SIZE error code, 98
WARN_COND_ITEM_TRUNCATED error code, 49
WARN_DATA_TRUNCATED error code, 22
WARN_DEPRECATED_MAXDB_SQL_MODE_FOR_TIMESTAMP
error code, 97
WARN_NAMED_PIPE_ACCESS_EVERYONE error
code, 70
WARN_NON_ASCII_SEPARATOR_NOT_IMPLEMENTED
error code, 49
WARN_NO_MASTER_INFO error code, 47
WARN_ON_BLOCKHOLE_IN_RBR error code, 68
WARN_OPTION_BELOW_LIMIT error code, 54
WARN_OPTION_IGNORED error code, 47
WARN_PLUGIN_BUSY error code, 47
WARN_PLUGIN_DELETE_BUILTIN error code, 47
137
138