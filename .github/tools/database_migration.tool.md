# Database Migrations

read this docs and when u update models, please create a new migration and update the database
### 1. Create New Migration
```bash
# Add a new migration when you change your models
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project DatabaseMigration

# Example:
dotnet ef migrations add AddOrderTable --project Infrastructure --startup-project DatabaseMigration
```

### 2. Review Migration
- Check the generated migration files in `Infrastructure/Migrations/`
- Verify the `Up()` and `Down()` methods are correct
- Review SQL that will be executed

### 3. Apply Migration
```bash
# Apply migrations to database
dotnet ef database update --project Infrastructure --startup-project DatabaseMigration
```