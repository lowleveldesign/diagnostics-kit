
MySql Log Store
===============

Current documentation is available here: https://github.com/lowleveldesign/diagnostics-kit/wiki/3.2.log-store-mysql
Current version can be downloaded from: http://www.lowleveldesign.org/diagnosticskit

Installation
------------

Download the zip file from the http://www.lowleveldesign.org/diagnosticskit and unpack it in the bin folder 
of the Diagnostics Castle.Remove previously used log stores or change the Castle configuration to point to 
your log store.

Configuration
-------------

In order to use MySql database as a store for your logs the following section must be added to 
the Diagnostics Castle web.config:

	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
	  <configSections>
		<section name="mySqlLogStore" type="LowLevelDesign.Diagnostics.LogStore.MySql.MySqlConfigSection, 
				LowLevelDesign.Diagnostics.LogStore.MySql" />
	  </configSections>
	  ...
	  <mySqlLogStore connectionStringName="mysqlconn" />
	</configuration>

connectionStringName is a name of an existing connection string to a MySql database. The user assigned 
to the connection should have rights to create/alter/drop tables. These rights are required as 
the MySql log store will automatically create log tables when processing log events.