# Commands

```
dotnet ef database update --project Infrastructure --startup-project DatabaseMigration
```

# Entity Framework Core Workflow Summary

## Quick Commands

```bash
# Update database with latest migrations
dotnet ef database update --project Infrastructure --startup-project DatabaseMigration
```

## EF Core Migration Workflow

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

# Apply to specific migration
dotnet ef database update <MigrationName> --project Infrastructure --startup-project DatabaseMigration
```

### 4. Rollback Migration
```bash
# Rollback to previous migration
dotnet ef database update <PreviousMigrationName> --project Infrastructure --startup-project DatabaseMigration

# Remove last migration (if not applied to database)
dotnet ef migrations remove --project Infrastructure --startup-project DatabaseMigration
```

## Useful Commands

### Database Operations
```bash
# Drop database
dotnet ef database drop --project Infrastructure --startup-project DatabaseMigration

# Generate SQL script for migrations
dotnet ef migrations script --project Infrastructure --startup-project DatabaseMigration

# Generate SQL for specific migration range
dotnet ef migrations script <FromMigration> <ToMigration> --project Infrastructure --startup-project DatabaseMigration
```

### Information Commands
```bash
# List all migrations
dotnet ef migrations list --project Infrastructure --startup-project DatabaseMigration

# Show DbContext info
dotnet ef dbcontext info --project Infrastructure --startup-project DatabaseMigration

# List all DbContexts
dotnet ef dbcontext list --project Infrastructure --startup-project DatabaseMigration
```

## Best Practices

### Migration Naming
- Use descriptive names: `AddUserTable`, `UpdateOrderStatus`, `RemoveObsoleteColumn`
- Include ticket/feature numbers if applicable: `TICKET123_AddPaymentTable`

### Before Creating Migration
1. Build the solution to ensure no compilation errors
2. Review your model changes
3. Consider data migration if needed

### Before Applying Migration
1. Backup production database
2. Test migration on development/staging environment
3. Review generated SQL script
4. Plan for rollback if needed

### Production Deployment
1. Generate SQL script: `dotnet ef migrations script`
2. Review script with DBA if required
3. Apply during maintenance window
4. Verify application functionality

## Troubleshooting

### Common Issues
- **Migration pending**: Run `dotnet ef database update`
- **Context not found**: Ensure startup project references Infrastructure
- **Connection string**: Verify appsettings.json configuration
- **Permissions**: Ensure database user has schema modification rights

### Reset Database (Development Only)
```bash
# Delete all migrations and recreate
dotnet ef database drop --project Infrastructure --startup-project DatabaseMigration
dotnet ef migrations remove --project Infrastructure --startup-project DatabaseMigration
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project DatabaseMigration
dotnet ef database update --project Infrastructure --startup-project DatabaseMigration
```

## Project Structure
- **Infrastructure**: Contains DbContext, entities, and migrations
- **DatabaseMigration**: Console app for running migrations
- **Migrations**: Auto-generated migration files (don't edit manually)