/*
 Navicat Premium Data Transfer

 Source Server         : ITH_MSSQL
 Source Server Type    : SQL Server
 Source Server Version : 13001601
 Source Host           : db.8kk.win,18235:1433
 Source Catalog        : YiWen
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 13001601
 File Encoding         : 65001

 Date: 01/09/2018 18:19:56
*/


-- ----------------------------
-- Table structure for _join
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[_join]') AND type IN ('U'))
	DROP TABLE [dbo].[_join]
GO

CREATE TABLE [dbo].[_join] (
  [uid] int  NOT NULL,
  [did] int  NOT NULL,
  [jname] char(16) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [jtype] int  NOT NULL,
  [father] char(32) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [cmnt] char(32) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [id] int  IDENTITY(1,1) NOT NULL,
  [state] char(10) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[_join] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for AgentNumber
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[AgentNumber]') AND type IN ('U'))
	DROP TABLE [dbo].[AgentNumber]
GO

CREATE TABLE [dbo].[AgentNumber] (
  [Number] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Platform] int  NULL,
  [CreateTime] datetime  NULL,
  [AgentNumberID] int  IDENTITY(1,1) NOT NULL,
  [CallOutNumber] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Sync] int  NULL
)
GO

ALTER TABLE [dbo].[AgentNumber] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'中间号',
'SCHEMA', N'dbo',
'TABLE', N'AgentNumber',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'云平台，0无，1电信，2移动，3联通，4其它',
'SCHEMA', N'dbo',
'TABLE', N'AgentNumber',
'COLUMN', N'Platform'
GO

EXEC sp_addextendedproperty
'MS_Description', N'0 未同步，1已同步',
'SCHEMA', N'dbo',
'TABLE', N'AgentNumber',
'COLUMN', N'Sync'
GO


-- ----------------------------
-- Table structure for Count
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Count]') AND type IN ('U'))
	DROP TABLE [dbo].[Count]
GO

CREATE TABLE [dbo].[Count] (
  [Date] smalldatetime  NOT NULL,
  [Address] int  NOT NULL,
  [LbsAndWifi] int  NOT NULL,
  [LbsAndWifiCache] int DEFAULT ((0)) NOT NULL,
  [LbsAndWifiFail] int DEFAULT ((0)) NOT NULL,
  [AddressTotal] int DEFAULT ((0)) NOT NULL,
  [LbsAndWifiTotal] int DEFAULT ((0)) NOT NULL,
  [SMS] int  NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[Count] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'Count',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'Count',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Table structure for Dealer
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Dealer]') AND type IN ('U'))
	DROP TABLE [dbo].[Dealer]
GO

CREATE TABLE [dbo].[Dealer] (
  [DealerId] int  IDENTITY(1,1) NOT NULL,
  [ParentId] int DEFAULT ((0)) NOT NULL,
  [Status] int DEFAULT ((0)) NOT NULL,
  [DealerType] int DEFAULT ((0)) NOT NULL,
  [Name] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Address] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[Dealer] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'经销商编号',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'DealerId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态，0 表示停用 1表示正常',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1,经销商,2代理商.3,厂家',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'DealerType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'Name'
GO

EXEC sp_addextendedproperty
'MS_Description', N'联系电话',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'联系地址',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'Address'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'Dealer',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Table structure for DealerDevice
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DealerDevice]') AND type IN ('U'))
	DROP TABLE [dbo].[DealerDevice]
GO

CREATE TABLE [dbo].[DealerDevice] (
  [DealerId] int  NOT NULL,
  [DeviceId] int  NOT NULL,
  [StockTime] smalldatetime  NULL,
  [Purchaser] int  NULL,
  [SalesTime] smalldatetime  NULL,
  [ReworkTime] smalldatetime  NULL,
  [Status] int DEFAULT ((0)) NOT NULL,
  [Remark] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DealerDevice] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'经销商编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'DealerId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'DeviceId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'StockTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'购买的人，0表示零售',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'Purchaser'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'SalesTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'返修时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'ReworkTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态 0路途  1入库 2销售 3返修 4换货',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注，仅供该经销商查看',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerDevice',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Table structure for DealerNotification
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DealerNotification]') AND type IN ('U'))
	DROP TABLE [dbo].[DealerNotification]
GO

CREATE TABLE [dbo].[DealerNotification] (
  [DealerNotificationId] int  IDENTITY(1,1) NOT NULL,
  [DealerId] int  NOT NULL,
  [NotificationID] int  NOT NULL,
  [Status] tinyint  NOT NULL,
  [UserCount] int  NOT NULL,
  [Content] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DealerNotification] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerNotification',
'COLUMN', N'DealerNotificationId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'经销商编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerNotification',
'COLUMN', N'DealerId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerNotification',
'COLUMN', N'NotificationID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息状态,0正在发送,1发送完成',
'SCHEMA', N'dbo',
'TABLE', N'DealerNotification',
'COLUMN', N'Status'
GO


-- ----------------------------
-- Table structure for DealerUser
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DealerUser]') AND type IN ('U'))
	DROP TABLE [dbo].[DealerUser]
GO

CREATE TABLE [dbo].[DealerUser] (
  [DealerUserId] int  IDENTITY(1,1) NOT NULL,
  [DealerId] int  NOT NULL,
  [Status] int DEFAULT ((0)) NOT NULL,
  [UserName] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Password] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [LoginID] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [UserType] int DEFAULT ((0)) NOT NULL,
  [Power] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Name] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL,
  [Salt] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[DealerUser] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'经销商编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'DealerId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态，0 表示停用 1表示正常',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'登陆用户名',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'UserName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'登陆密码',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'Password'
GO

EXEC sp_addextendedproperty
'MS_Description', N'登录编号',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'LoginID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1普通用户,2管理员,3系统管理员',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'UserType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'权限',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'Power'
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'Name'
GO

EXEC sp_addextendedproperty
'MS_Description', N'联系电话',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'DealerUser',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Table structure for Device
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Device]') AND type IN ('U'))
	DROP TABLE [dbo].[Device]
GO

CREATE TABLE [dbo].[Device] (
  [DeviceID] int  IDENTITY(1,1) NOT NULL,
  [SerialNumber] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [State] int DEFAULT ((0)) NOT NULL,
  [Deviceflag] int  NULL,
  [Deleted] bit DEFAULT ((0)) NOT NULL,
  [UserId] int  NOT NULL,
  [DeviceModelID] int  NOT NULL,
  [BindNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Firmware] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [CurrentFirmware] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [SetVersionNO] int  NULL,
  [ContactVersionNO] int  NULL,
  [OperatorType] int  NULL,
  [SmsNumber] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NULL,
  [SmsBalanceKey] nvarchar(15) COLLATE Chinese_PRC_CI_AS  NULL,
  [SmsFlowKey] nvarchar(15) COLLATE Chinese_PRC_CI_AS  NULL,
  [Photo] varchar(500) COLLATE Chinese_PRC_CI_AS  NULL,
  [BabyName] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneCornet] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Gender] bit  NULL,
  [Birthday] smalldatetime  NULL,
  [Grade] int  NULL,
  [SchoolAddress] nvarchar(250) COLLATE Chinese_PRC_CI_AS  NULL,
  [SchoolLng] decimal(25,20)  NULL,
  [SchoolLat] decimal(25,20)  NULL,
  [HomeAddress] nvarchar(250) COLLATE Chinese_PRC_CI_AS  NULL,
  [HomeLng] decimal(25,20)  NULL,
  [HomeLat] decimal(25,20)  NULL,
  [IsGuard] bit  NULL,
  [Password] nvarchar(50) COLLATE Chinese_PRC_CI_AS DEFAULT ((123456)) NULL,
  [DeviceNote] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [DeviceCustomer] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [DeviceProject] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [ActiveDate] datetime  NULL,
  [HireStartDate] datetime  NULL,
  [HireExpireDate] datetime  NULL,
  [DeviceType] int DEFAULT ((1)) NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL,
  [LatestTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [sta] tinyint DEFAULT ((0)) NULL,
  [mis] tinyint DEFAULT ((0)) NULL,
  [tz] tinyint DEFAULT ((0)) NULL,
  [lang] tinyint DEFAULT ((1)) NULL,
  [cash] datetime  NULL,
  [CloudPlatform] int DEFAULT ((0)) NULL,
  [LengthCountType] int  NULL
)
GO

ALTER TABLE [dbo].[Device] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SerialNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'0未激活,1已激活',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'State'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否删除',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Deleted'
GO

EXEC sp_addextendedproperty
'MS_Description', N'管理员',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'UserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备类型',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'DeviceModelID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备绑定号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'BindNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'需要升级的固件版本号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Firmware'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前固件版本号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'CurrentFirmware'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备配置版本号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SetVersionNO'
GO

EXEC sp_addextendedproperty
'MS_Description', N'通信录同步版本号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'ContactVersionNO'
GO

EXEC sp_addextendedproperty
'MS_Description', N'运营商类型，1表示移动，2表示联通，3表示电信。0表示默认值，默认为移动',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'OperatorType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'查询流量及短信的运营商号码',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SmsNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'查询话费的指令',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SmsBalanceKey'
GO

EXEC sp_addextendedproperty
'MS_Description', N'查询流量的指令',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SmsFlowKey'
GO

EXEC sp_addextendedproperty
'MS_Description', N'宝贝头像地址',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Photo'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备名称，宝贝名称',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'BabyName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备电话号码',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'短号、亲情号',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'PhoneCornet'
GO

EXEC sp_addextendedproperty
'MS_Description', N'宝贝性别：真为男孩，假为女孩',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Gender'
GO

EXEC sp_addextendedproperty
'MS_Description', N'生日',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Birthday'
GO

EXEC sp_addextendedproperty
'MS_Description', N'宝贝读书年纪，从未读书：0，幼儿园小：1、中：2、大：3班，学前班：4，到小学六年级：10，到其他：11',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Grade'
GO

EXEC sp_addextendedproperty
'MS_Description', N'学校地址',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SchoolAddress'
GO

EXEC sp_addextendedproperty
'MS_Description', N'学校经度',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SchoolLng'
GO

EXEC sp_addextendedproperty
'MS_Description', N'学校纬度',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'SchoolLat'
GO

EXEC sp_addextendedproperty
'MS_Description', N'家地址',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'HomeAddress'
GO

EXEC sp_addextendedproperty
'MS_Description', N'家经度',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'HomeLng'
GO

EXEC sp_addextendedproperty
'MS_Description', N'家纬度',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'HomeLat'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否开启守护功能，开启守护功能有消息提醒。',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'IsGuard'
GO

EXEC sp_addextendedproperty
'MS_Description', N'预留，设备密码',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'Password'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备活动时间',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'ActiveDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备激活时间',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'HireStartDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备到期时间',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'HireExpireDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'云平台，0无，1电信，2移动，3联通，4其它',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'CloudPlatform'
GO

EXEC sp_addextendedproperty
'MS_Description', N'长度计算方式',
'SCHEMA', N'dbo',
'TABLE', N'Device',
'COLUMN', N'LengthCountType'
GO


-- ----------------------------
-- Table structure for DeviceContact
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceContact]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceContact]
GO

CREATE TABLE [dbo].[DeviceContact] (
  [DeviceContactId] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [Type] tinyint  NOT NULL,
  [ObjectId] int DEFAULT ((0)) NOT NULL,
  [Name] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneShort] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Photo] tinyint  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL,
  [HeadImg] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [HeadImgVersion] int DEFAULT ((0)) NULL,
  [AgentNumber] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [CallOutNumber] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Sync] int  NULL,
  [UserId] int  NULL
)
GO

ALTER TABLE [dbo].[DeviceContact] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'通讯录编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'DeviceContactId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1 表示普通联系人 2 表示用户 3表示手表',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当 Type=2 的时候 这个是用户编号 当 Type=3 的 设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'ObjectId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'Name'
GO

EXEC sp_addextendedproperty
'MS_Description', N'手机号码',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'短号码',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'PhoneShort'
GO

EXEC sp_addextendedproperty
'MS_Description', N'头像编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'Photo'
GO

EXEC sp_addextendedproperty
'MS_Description', N'云平台中间号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'AgentNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'云平台呼出中间号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'CallOutNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'云平台同步，0 未同步，1已同步',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'Sync'
GO

EXEC sp_addextendedproperty
'MS_Description', N'关联到用户表的ID',
'SCHEMA', N'dbo',
'TABLE', N'DeviceContact',
'COLUMN', N'UserId'
GO


-- ----------------------------
-- Table structure for DeviceException
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceException]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceException]
GO

CREATE TABLE [dbo].[DeviceException] (
  [DeviceExceptionID] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NULL,
  [Type] int  NULL,
  [Content] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [Latitude] decimal(25,20)  NULL,
  [Longitude] decimal(25,20)  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DeviceException] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备异常信息编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'DeviceExceptionID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'异常类型',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'异常内容',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'Content'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发生异常的纬度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'Latitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发生异常的精度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceException',
'COLUMN', N'Longitude'
GO


-- ----------------------------
-- Table structure for DeviceFriend
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceFriend]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceFriend]
GO

CREATE TABLE [dbo].[DeviceFriend] (
  [DeviceFriendId] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [Type] tinyint  NOT NULL,
  [ObjectId] int DEFAULT ((1)) NOT NULL,
  [Name] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DeviceFriend] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'好友表编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceFriend',
'COLUMN', N'DeviceFriendId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceFriend',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1 加好友中 2 被通知完，3 为好友',
'SCHEMA', N'dbo',
'TABLE', N'DeviceFriend',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'好友名称',
'SCHEMA', N'dbo',
'TABLE', N'DeviceFriend',
'COLUMN', N'Name'
GO


-- ----------------------------
-- Table structure for DevicePhoto
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DevicePhoto]') AND type IN ('U'))
	DROP TABLE [dbo].[DevicePhoto]
GO

CREATE TABLE [dbo].[DevicePhoto] (
  [DevicePhotoId] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [Source] varchar(20) COLLATE Chinese_PRC_CI_AS  NULL,
  [DeviceTime] datetime  NULL,
  [Latitude] decimal(25,20)  NULL,
  [Longitude] decimal(25,20)  NULL,
  [State] tinyint DEFAULT ((0)) NOT NULL,
  [TotalPackage] smallint DEFAULT ((0)) NOT NULL,
  [CurrentPackage] smallint DEFAULT ((0)) NOT NULL,
  [Mark] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Path] varchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL,
  [Thumb] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[DevicePhoto] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备表，ID号',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'来源,用户请求的照片这里则是用户手机号码,手表发的,这里为空',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Source'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备时间',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'DeviceTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'纬度',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Latitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'经度',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Longitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态 0 正在接收 1接收完成',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'State'
GO

EXEC sp_addextendedproperty
'MS_Description', N'总包数',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'TotalPackage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前包',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'CurrentPackage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'图片标识',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Mark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'保存路径',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Path'
GO

EXEC sp_addextendedproperty
'MS_Description', N'getdate()',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'getdate()',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'UpdateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'视频缩略图路径',
'SCHEMA', N'dbo',
'TABLE', N'DevicePhoto',
'COLUMN', N'Thumb'
GO


-- ----------------------------
-- Table structure for DeviceSet
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceSet]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceSet]
GO

CREATE TABLE [dbo].[DeviceSet] (
  [DeviceID] int  NOT NULL,
  [SetInfo] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [ClassDisabled1] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [ClassDisabled2] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [WeekDisabled] nvarchar(7) COLLATE Chinese_PRC_CI_AS  NULL,
  [TimerOpen] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [TimerClose] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [BrightScreen] int  NULL,
  [WeekAlarm1] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [WeekAlarm2] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [WeekAlarm3] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Alarm1] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Alarm2] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Alarm3] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [LocationMode] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [LocationTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [FlowerNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [SleepCalculate] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [StepCalculate] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [HrCalculate] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [SosMsgswitch] nvarchar(50) COLLATE Chinese_PRC_CI_AS DEFAULT ((1)) NULL,
  [CreateTime] datetime DEFAULT (getdate()) NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NULL
)
GO

ALTER TABLE [dbo].[DeviceSet] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1-1-1-1-1-1-1-1-1-1-1-1-1，配置信息。自动接通、报告通话位置、体感接听、预留应急电量、上课禁用、定时开关机、拒绝陌生人来电、防脱落、手表来电响铃、手表来电振动、手表来消息响铃、手表来消息振动',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'SetInfo'
GO

EXEC sp_addextendedproperty
'MS_Description', N'上课禁用时间段1，如：08:10-11:30',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'ClassDisabled1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'上课禁用时间段2，如：08:10-11:30',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'ClassDisabled2'
GO

EXEC sp_addextendedproperty
'MS_Description', N'禁用时间段星期说明13567，表示周一、周三、周五、周六和周日',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'WeekDisabled'
GO

EXEC sp_addextendedproperty
'MS_Description', N'定时开机时间',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'TimerOpen'
GO

EXEC sp_addextendedproperty
'MS_Description', N'定时关机时间',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'TimerClose'
GO

EXEC sp_addextendedproperty
'MS_Description', N'亮屏时间，单位为“秒”',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSet',
'COLUMN', N'BrightScreen'
GO


-- ----------------------------
-- Table structure for DeviceSMS
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceSMS]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceSMS]
GO

CREATE TABLE [dbo].[DeviceSMS] (
  [DeviceSMSID] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [Type] tinyint  NOT NULL,
  [State] tinyint DEFAULT ((0)) NOT NULL,
  [Phone] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [SMS] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DeviceSMS] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备短信编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'DeviceSMSID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'类型 1 设备发送 2 设备接收',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态 0 未发送，1已发送 2发送成功 3发送失败',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'State'
GO

EXEC sp_addextendedproperty
'MS_Description', N'号码',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'Phone'
GO

EXEC sp_addextendedproperty
'MS_Description', N'短信内容',
'SCHEMA', N'dbo',
'TABLE', N'DeviceSMS',
'COLUMN', N'SMS'
GO


-- ----------------------------
-- Table structure for DeviceState
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceState]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceState]
GO

CREATE TABLE [dbo].[DeviceState] (
  [DeviceID] int  NOT NULL,
  [Online] bit  NOT NULL,
  [SocketId] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [ServerTime] datetime  NULL,
  [DeviceTime] datetime  NULL,
  [Latitude] decimal(25,20)  NULL,
  [Longitude] decimal(25,20)  NULL,
  [Speed] decimal(10,2)  NOT NULL,
  [Course] decimal(5)  NOT NULL,
  [Altitude] float(53)  NOT NULL,
  [SatelliteNumber] int  NOT NULL,
  [GSM] int  NOT NULL,
  [Electricity] int  NOT NULL,
  [TqInfo] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [LocationSource] int  NULL,
  [LocationType] int  NOT NULL,
  [LBS] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Wifi] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Radius] int  NULL,
  [StopSendVoice] bit DEFAULT ((0)) NOT NULL,
  [Step] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Health] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime  NOT NULL,
  [UpdateTime] datetime  NOT NULL,
  [IsLowPowerAlarmed] bit  NULL
)
GO

ALTER TABLE [dbo].[DeviceState] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备表，ID号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否在线',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Online'
GO

EXEC sp_addextendedproperty
'MS_Description', N'网关连接编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'SocketId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'服务器时间',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'ServerTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备时间',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'DeviceTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'纬度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Latitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'经度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Longitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'速度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Speed'
GO

EXEC sp_addextendedproperty
'MS_Description', N'方向',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Course'
GO

EXEC sp_addextendedproperty
'MS_Description', N'海拔，单位：米',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Altitude'
GO

EXEC sp_addextendedproperty
'MS_Description', N'卫星数量',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'SatelliteNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'通信卡信号：0-100',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'GSM'
GO

EXEC sp_addextendedproperty
'MS_Description', N'电量，表示百分比',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Electricity'
GO

EXEC sp_addextendedproperty
'MS_Description', N'定位类型  1表示GPS  2表示LBS  3表示WIFI',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'LocationType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'此字段用来存放基站数据：基站数量|基站TA-国家码(MCC)-运营商(MNC)|基站1区域码(LAC)-基站1编码(CellID)-基站1信号强度|基站2区域码(LAC)-基站2编码(CellID)-基站2信号强度|基站n区域码(LAC)-基站n编码(CellID)-基站n信号强度。如：2|1-460-02|465-797-90|6864-789-80',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'LBS'
GO

EXEC sp_addextendedproperty
'MS_Description', N'此字段描述wifi序列信息：wifi数量|wifi1名称-MAC地址-信号强度|wifi2-n名称-MAC地址-信号强度。如：3|abc-1c:fa:68:13:a5:b4-80|abc-1c:fa:68:13:a5:b4-80',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Wifi'
GO

EXEC sp_addextendedproperty
'MS_Description', N'定位精度,单位米',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'Radius'
GO

EXEC sp_addextendedproperty
'MS_Description', N'控制是否继续对终端发送语音包，true表示停止发送，false表示可以继续发送语音包',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'StopSendVoice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'getdate()',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'getdate()',
'SCHEMA', N'dbo',
'TABLE', N'DeviceState',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Table structure for DeviceTq
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceTq]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceTq]
GO

CREATE TABLE [dbo].[DeviceTq] (
  [ID] int  NOT NULL,
  [City] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Info] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Temperature] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Latitude] decimal(25,20)  NULL,
  [Longitude] decimal(25,20)  NULL,
  [CreateTime] datetime  NULL,
  [UpdateTime] datetime  NULL
)
GO

ALTER TABLE [dbo].[DeviceTq] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for DeviceVoice
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceVoice]') AND type IN ('U'))
	DROP TABLE [dbo].[DeviceVoice]
GO

CREATE TABLE [dbo].[DeviceVoice] (
  [DeviceVoiceId] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [State] tinyint DEFAULT ((0)) NOT NULL,
  [TotalPackage] smallint DEFAULT ((0)) NOT NULL,
  [CurrentPackage] smallint DEFAULT ((0)) NOT NULL,
  [Type] tinyint  NOT NULL,
  [ObjectId] int  NOT NULL,
  [Mark] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Path] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Length] float(53)  NOT NULL,
  [MsgType] int  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[DeviceVoice] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备语音编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'DeviceVoiceId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态 0 正在接收 1接收完成 2正在发送 3 发送完成',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'State'
GO

EXEC sp_addextendedproperty
'MS_Description', N'总包数',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'TotalPackage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前包',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'CurrentPackage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1 表发给指定的人( ObjectId 人的编号 )  2表发给通讯录所有人(ObjectId=0) 3.人发给表及其他人 (ObjectId 人的编号) 4 表发给表（ObjectId 目标表的编号） ',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'语音标识',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'Mark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'保存路径',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'Path'
GO

EXEC sp_addextendedproperty
'MS_Description', N'语音长度',
'SCHEMA', N'dbo',
'TABLE', N'DeviceVoice',
'COLUMN', N'Length'
GO


-- ----------------------------
-- Table structure for Feedback
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Feedback]') AND type IN ('U'))
	DROP TABLE [dbo].[Feedback]
GO

CREATE TABLE [dbo].[Feedback] (
  [FeedbackID] int  IDENTITY(1,1) NOT NULL,
  [QuestionType] int  NULL,
  [QuestionUserID] int  NULL,
  [QuestionImg] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [QuestionContent] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [AnswerUserID] int  NULL,
  [AnswerContent] nchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [FeedbackState] int  NULL,
  [CreateTime] datetime  NULL,
  [HandleTime] datetime  NULL,
  [HandleUserID] int  NULL,
  [Deleted] bit  NULL
)
GO

ALTER TABLE [dbo].[Feedback] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'问答类型：0表示APP，1表示设备',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'QuestionType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'提问用户',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'QuestionUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'问题截图',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'QuestionImg'
GO

EXEC sp_addextendedproperty
'MS_Description', N'问题内容',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'QuestionContent'
GO

EXEC sp_addextendedproperty
'MS_Description', N'回答用户ID，一般为系统超级管理员',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'AnswerUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'反馈状态：0表示提问，1表示回答，2表示被甄选为经典问答解疑内容',
'SCHEMA', N'dbo',
'TABLE', N'Feedback',
'COLUMN', N'FeedbackState'
GO


-- ----------------------------
-- Table structure for GeoFence
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[GeoFence]') AND type IN ('U'))
	DROP TABLE [dbo].[GeoFence]
GO

CREATE TABLE [dbo].[GeoFence] (
  [GeofenceID] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NULL,
  [Enable] bit DEFAULT ((0)) NULL,
  [FenceName] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Entry] bit  NULL,
  [Exit] bit  NULL,
  [LatAndLng] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Description] ntext COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime  NOT NULL,
  [UpdateTime] datetime  NOT NULL
)
GO

ALTER TABLE [dbo].[GeoFence] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'Lat-Lng,Lat1-Lng1,.....电子围栏的经纬度',
'SCHEMA', N'dbo',
'TABLE', N'GeoFence',
'COLUMN', N'LatAndLng'
GO


-- ----------------------------
-- Table structure for Notification
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification]') AND type IN ('U'))
	DROP TABLE [dbo].[Notification]
GO

CREATE TABLE [dbo].[Notification] (
  [NotificationID] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int DEFAULT ((0)) NOT NULL,
  [Type] int  NOT NULL,
  [Content] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[Notification] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备编号',
'SCHEMA', N'dbo',
'TABLE', N'Notification',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'通知类型 Type:1 语音信息，2发信息给管理员关联确认，3管理员确认关联，4管理员拒绝关联, 5设备升级成功 6设备配置已经同步 7设备通讯录已经同步 8设备收到短信 9解除关联',
'SCHEMA', N'dbo',
'TABLE', N'Notification',
'COLUMN', N'Type'
GO


-- ----------------------------
-- Table structure for Project
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[Project]') AND type IN ('U'))
	DROP TABLE [dbo].[Project]
GO

CREATE TABLE [dbo].[Project] (
  [ProjectId] varchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Name] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [SMSKey] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [SMSReg] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [SMSForgot] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [AndroidVersion] int DEFAULT ((0)) NOT NULL,
  [AppleVersion] int DEFAULT ((0)) NULL,
  [AndroidUrl] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [AppleUrl] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NULL,
  [AndroidDescription] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [AppleDescription] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [AD] varchar(500) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[Project] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for SchoolGuardian
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[SchoolGuardian]') AND type IN ('U'))
	DROP TABLE [dbo].[SchoolGuardian]
GO

CREATE TABLE [dbo].[SchoolGuardian] (
  [SchoolGuardianID] int  IDENTITY(1,1) NOT NULL,
  [DeviceID] int  NOT NULL,
  [SchoolDay] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [GuardState] int DEFAULT ((0)) NULL,
  [SchoolArriveContent] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [SchoolArriveTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [SchoolLeaveContent] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [SchoolLeaveTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [RoadStayContent] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [RoadStayTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [HomeBackContent] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [HomeBackTime] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[SchoolGuardian] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'设备ID号',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'DeviceID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'格式为：2015-10-25',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'SchoolDay'
GO

EXEC sp_addextendedproperty
'MS_Description', N'守护状态，0表示等待状态，1表示已经提醒一次',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'GuardState'
GO

EXEC sp_addextendedproperty
'MS_Description', N'到校提醒内容',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'SchoolArriveContent'
GO

EXEC sp_addextendedproperty
'MS_Description', N'到校提醒时间',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'SchoolArriveTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'离校提醒内容',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'SchoolLeaveContent'
GO

EXEC sp_addextendedproperty
'MS_Description', N'离校提醒时间',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'SchoolLeaveTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'路上逗留内容',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'RoadStayContent'
GO

EXEC sp_addextendedproperty
'MS_Description', N'路上逗留时间',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'RoadStayTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'最迟到家内容',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'HomeBackContent'
GO

EXEC sp_addextendedproperty
'MS_Description', N'最迟到家时间',
'SCHEMA', N'dbo',
'TABLE', N'SchoolGuardian',
'COLUMN', N'HomeBackTime'
GO


-- ----------------------------
-- Table structure for SystemInfo
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemInfo]') AND type IN ('U'))
	DROP TABLE [dbo].[SystemInfo]
GO

CREATE TABLE [dbo].[SystemInfo] (
  [ServiceAgreement] ntext COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[SystemInfo] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户服务协议',
'SCHEMA', N'dbo',
'TABLE', N'SystemInfo',
'COLUMN', N'ServiceAgreement'
GO


-- ----------------------------
-- Table structure for User
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND type IN ('U'))
	DROP TABLE [dbo].[User]
GO

CREATE TABLE [dbo].[User] (
  [UserID] int  IDENTITY(1,1) NOT NULL,
  [PhoneNumber] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Password] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [LoginID] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [LoginType] tinyint DEFAULT ((0)) NOT NULL,
  [UserType] int DEFAULT ((0)) NOT NULL,
  [Name] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Deleted] bit DEFAULT ((0)) NOT NULL,
  [Notification] bit DEFAULT ((1)) NOT NULL,
  [NotificationSound] bit DEFAULT ((1)) NOT NULL,
  [NotificationVibration] bit DEFAULT ((1)) NOT NULL,
  [ActivityTime] datetime  NULL,
  [AppID] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [Project] varchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL,
  [BindNumber] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NULL,
  [Salt] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[User] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户编号',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'电话号码',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'密码',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'Password'
GO

EXEC sp_addextendedproperty
'MS_Description', N'登录编号',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'LoginID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'登录方式,0,未登录,1 Android,2 IOS,3 Web',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'LoginType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'1,普通用户.2,管理员',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'UserType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'Name'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否删除 0否，1是',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'Deleted'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否推送通知',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'Notification'
GO

EXEC sp_addextendedproperty
'MS_Description', N'推送通知声音提醒',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'NotificationSound'
GO

EXEC sp_addextendedproperty
'MS_Description', N'推送通知震动提醒',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'NotificationVibration'
GO

EXEC sp_addextendedproperty
'MS_Description', N'苹果推送ID',
'SCHEMA', N'dbo',
'TABLE', N'User',
'COLUMN', N'AppID'
GO


-- ----------------------------
-- Table structure for UserDevice
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDevice]') AND type IN ('U'))
	DROP TABLE [dbo].[UserDevice]
GO

CREATE TABLE [dbo].[UserDevice] (
  [UserID] int  NOT NULL,
  [DeviceID] int  NOT NULL,
  [Status] tinyint  NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[UserDevice] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态  0表示 待确认，1表示确认',
'SCHEMA', N'dbo',
'TABLE', N'UserDevice',
'COLUMN', N'Status'
GO


-- ----------------------------
-- Table structure for UserLog
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[UserLog]') AND type IN ('U'))
	DROP TABLE [dbo].[UserLog]
GO

CREATE TABLE [dbo].[UserLog] (
  [UserLogID] int  IDENTITY(1,1) NOT NULL,
  [UserID] int  NOT NULL,
  [TypeID] int  NULL,
  [LogContent] nvarchar(300) COLLATE Chinese_PRC_CI_AS  NULL,
  [ObjectName] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [ObjectID] int  NULL,
  [IP] varchar(20) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime  NULL
)
GO

ALTER TABLE [dbo].[UserLog] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for UserNotification
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[UserNotification]') AND type IN ('U'))
	DROP TABLE [dbo].[UserNotification]
GO

CREATE TABLE [dbo].[UserNotification] (
  [UserNotificationId] int  IDENTITY(1,1) NOT NULL,
  [UserID] int  NOT NULL,
  [DeviceID] int DEFAULT ((0)) NOT NULL,
  [Type] int  NOT NULL,
  [ObjectId] int  NOT NULL,
  [Get] bit DEFAULT ((0)) NOT NULL,
  [Notification] bit DEFAULT ((0)) NOT NULL,
  [CreateTime] datetime DEFAULT (getdate()) NOT NULL,
  [UpdateTime] datetime DEFAULT (getdate()) NOT NULL
)
GO

ALTER TABLE [dbo].[UserNotification] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户编号',
'SCHEMA', N'dbo',
'TABLE', N'UserNotification',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'Type 1 普通消息 2 语音 3 短信 4 报警',
'SCHEMA', N'dbo',
'TABLE', N'UserNotification',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当类型为语音的时候对应设备语音编号 当类型是消息的时候对应消息编号',
'SCHEMA', N'dbo',
'TABLE', N'UserNotification',
'COLUMN', N'ObjectId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否获取',
'SCHEMA', N'dbo',
'TABLE', N'UserNotification',
'COLUMN', N'Get'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否推送语音信息',
'SCHEMA', N'dbo',
'TABLE', N'UserNotification',
'COLUMN', N'Notification'
GO


-- ----------------------------
-- Primary Key structure for table _join
-- ----------------------------
ALTER TABLE [dbo].[_join] ADD CONSTRAINT [PK___join__3213E83FE0705545] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table AgentNumber
-- ----------------------------
ALTER TABLE [dbo].[AgentNumber] ADD CONSTRAINT [PK__AgentNum__78A1A19C7C338466] PRIMARY KEY CLUSTERED ([Number], [AgentNumberID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Count
-- ----------------------------
ALTER TABLE [dbo].[Count] ADD CONSTRAINT [PK_Count] PRIMARY KEY CLUSTERED ([Date])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Dealer
-- ----------------------------
ALTER TABLE [dbo].[Dealer] ADD CONSTRAINT [PK_Dealer] PRIMARY KEY CLUSTERED ([DealerId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DealerDevice
-- ----------------------------
ALTER TABLE [dbo].[DealerDevice] ADD CONSTRAINT [PK_DealerDevice] PRIMARY KEY CLUSTERED ([DealerId], [DeviceId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DealerNotification
-- ----------------------------
ALTER TABLE [dbo].[DealerNotification] ADD CONSTRAINT [PK_DealerNotification] PRIMARY KEY CLUSTERED ([DealerNotificationId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DealerUser
-- ----------------------------
ALTER TABLE [dbo].[DealerUser] ADD CONSTRAINT [PK_DealerUser] PRIMARY KEY CLUSTERED ([DealerUserId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Device
-- ----------------------------
ALTER TABLE [dbo].[Device] ADD CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED ([DeviceID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceContact
-- ----------------------------
ALTER TABLE [dbo].[DeviceContact] ADD CONSTRAINT [PK_DeviceContac] PRIMARY KEY CLUSTERED ([DeviceContactId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceException
-- ----------------------------
ALTER TABLE [dbo].[DeviceException] ADD CONSTRAINT [PK_ExceptionMessage] PRIMARY KEY CLUSTERED ([DeviceExceptionID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Uniques structure for table DeviceFriend
-- ----------------------------
ALTER TABLE [dbo].[DeviceFriend] ADD CONSTRAINT [uq_friend_row] UNIQUE NONCLUSTERED ([DeviceID] ASC, [ObjectId] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceFriend
-- ----------------------------
ALTER TABLE [dbo].[DeviceFriend] ADD CONSTRAINT [PK_DeviceFriend] PRIMARY KEY CLUSTERED ([DeviceFriendId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DevicePhoto
-- ----------------------------
ALTER TABLE [dbo].[DevicePhoto] ADD CONSTRAINT [PK_DevicePhoto] PRIMARY KEY CLUSTERED ([DevicePhotoId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceSet
-- ----------------------------
ALTER TABLE [dbo].[DeviceSet] ADD CONSTRAINT [PK_DeviceSet] PRIMARY KEY CLUSTERED ([DeviceID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceSMS
-- ----------------------------
ALTER TABLE [dbo].[DeviceSMS] ADD CONSTRAINT [PK_DeviceSMS] PRIMARY KEY CLUSTERED ([DeviceSMSID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceState
-- ----------------------------
ALTER TABLE [dbo].[DeviceState] ADD CONSTRAINT [PK_LDeviceState] PRIMARY KEY CLUSTERED ([DeviceID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceTq
-- ----------------------------
ALTER TABLE [dbo].[DeviceTq] ADD CONSTRAINT [PK_DeviceTq] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table DeviceVoice
-- ----------------------------
ALTER TABLE [dbo].[DeviceVoice] ADD CONSTRAINT [PK_DeviceVoice] PRIMARY KEY CLUSTERED ([DeviceVoiceId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Feedback
-- ----------------------------
ALTER TABLE [dbo].[Feedback] ADD CONSTRAINT [PK_FeedbackMessage] PRIMARY KEY CLUSTERED ([FeedbackID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table GeoFence
-- ----------------------------
ALTER TABLE [dbo].[GeoFence] ADD CONSTRAINT [GeoFence_PK] PRIMARY KEY CLUSTERED ([GeofenceID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Notification
-- ----------------------------
ALTER TABLE [dbo].[Notification] ADD CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED ([NotificationID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table Project
-- ----------------------------
ALTER TABLE [dbo].[Project] ADD CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED ([ProjectId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table SchoolGuardian
-- ----------------------------
ALTER TABLE [dbo].[SchoolGuardian] ADD CONSTRAINT [PK_SchoolGuardian] PRIMARY KEY CLUSTERED ([SchoolGuardianID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table User
-- ----------------------------
ALTER TABLE [dbo].[User] ADD CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table UserDevice
-- ----------------------------
ALTER TABLE [dbo].[UserDevice] ADD CONSTRAINT [PK_UserDevice] PRIMARY KEY CLUSTERED ([UserID], [DeviceID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table UserLog
-- ----------------------------
ALTER TABLE [dbo].[UserLog] ADD CONSTRAINT [PK_UserLog] PRIMARY KEY CLUSTERED ([UserLogID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table UserNotification
-- ----------------------------
ALTER TABLE [dbo].[UserNotification] ADD CONSTRAINT [PK_UserNotification] PRIMARY KEY CLUSTERED ([UserNotificationId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

