-- Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814
-- Reference schema. Entity Framework creates the working ArcaneVault.db file on first launch.
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS ArcaneVaultUserRoles (
  RoleId INTEGER NOT NULL PRIMARY KEY,
  RoleName TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS ArcaneVaultUsers (
  UserName TEXT NOT NULL PRIMARY KEY,
  Email TEXT NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,
  IsDeleted INTEGER NOT NULL DEFAULT 0,
  RoleId INTEGER NOT NULL,
  FOREIGN KEY (RoleId) REFERENCES ArcaneVaultUserRoles(RoleId)
);
CREATE TABLE IF NOT EXISTS Categories (
  CategoryCode TEXT NOT NULL PRIMARY KEY,
  CategoryName TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS CollectionItems (
  ItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ItemName TEXT NOT NULL,
  Description TEXT NOT NULL DEFAULT '',
  IsDeleted INTEGER NOT NULL DEFAULT 0,
  StartingQuantity INTEGER NOT NULL,
  CurrentQuantity INTEGER NOT NULL,
  EstimatedUnitValue TEXT NOT NULL,
  DateAdded TEXT NOT NULL,
  ImageUrl TEXT NOT NULL,
  UserName TEXT NOT NULL,
  FOREIGN KEY (UserName) REFERENCES ArcaneVaultUsers(UserName)
);
CREATE TABLE IF NOT EXISTS CollectionItemCategories (
  ItemId INTEGER NOT NULL,
  CategoryCode TEXT NOT NULL,
  PRIMARY KEY (ItemId, CategoryCode),
  FOREIGN KEY (ItemId) REFERENCES CollectionItems(ItemId),
  FOREIGN KEY (CategoryCode) REFERENCES Categories(CategoryCode)
);
CREATE TABLE IF NOT EXISTS AcquisitionRecords (
  AcquisitionId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ItemId INTEGER NOT NULL,
  UserName TEXT NOT NULL,
  Quantity INTEGER NOT NULL,
  UnitPrice TEXT NOT NULL,
  PurchaseDate TEXT NOT NULL,
  PurchaseSource TEXT NOT NULL,
  Condition TEXT NOT NULL,
  CreatedAt TEXT NOT NULL,
  FOREIGN KEY (ItemId) REFERENCES CollectionItems(ItemId),
  FOREIGN KEY (UserName) REFERENCES ArcaneVaultUsers(UserName)
);
CREATE TABLE IF NOT EXISTS CollectibleCatalog (
  CatalogItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ItemName TEXT NOT NULL,
  CategoryCode TEXT NOT NULL,
  Brand TEXT NOT NULL DEFAULT '',
  Series TEXT NOT NULL DEFAULT '',
  ReferenceNumber TEXT NOT NULL DEFAULT '',
  ReleaseYear TEXT NOT NULL DEFAULT '',
  Description TEXT NOT NULL DEFAULT '',
  ImageUrl TEXT NOT NULL DEFAULT '/images/products/placeholder.webp'
);

PRAGMA user_version = 5;
