using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductAttributeColumnsAndStoreId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProductAttributes]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [ProductAttributes] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'StoreId')
                BEGIN
                    ALTER TABLE [CategoryAttributes] ADD [StoreId] uniqueidentifier NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000');
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [CategoryAttributes] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'IX_CategoryAttributes_StoreId')
                BEGIN
                    CREATE INDEX [IX_CategoryAttributes_StoreId] ON [CategoryAttributes] ([StoreId]);
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProductAttributeValues]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [ProductAttributeValues] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'IX_CategoryAttributes_StoreId')
                BEGIN
                    DROP INDEX [IX_CategoryAttributes_StoreId] ON [CategoryAttributes];
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProductAttributeValues]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [ProductAttributeValues] DROP COLUMN [CreatedAt];
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [CategoryAttributes] DROP COLUMN [CreatedAt];
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CategoryAttributes]') AND name = 'StoreId')
                BEGIN
                    ALTER TABLE [CategoryAttributes] DROP COLUMN [StoreId];
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProductAttributes]') AND name = 'CreatedAt')
                BEGIN
                    ALTER TABLE [ProductAttributes] DROP COLUMN [CreatedAt];
                END
            ");
        }
    }
}
