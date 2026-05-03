using System.Globalization;
using System.Reflection;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Store.DbServices.Context;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedStoreDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        await db.Database.MigrateAsync(ct);

        await SeedCatalogFromLegacyDataAsync(db, logger, ct);
        await SeedRolesAndUsersAsync(db, logger, ct);
        await SeedAllRemainingEmptyTablesAsync(db, logger, ct);
    }

    private static async Task SeedCatalogFromLegacyDataAsync(StoreDbContext db, ILogger logger, CancellationToken ct)
    {
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Name = "Beverges", Description = "Beverages", ImagePath = "img/planet-1l.png" },
                new Category { Name = "Groceries", Description = "General groceries", ImagePath = "img/parle-g.png" },
                new Category { Name = "Alcohols", Description = "Alcoholic drinks" },
                new Category { Name = "Vegitebles", Description = "Vegetables", ImagePath = "img/tomat-ss.png" },
                new Category { Name = "Fish", Description = "Fish and seafood" },
                new Category { Name = "Meat", Description = "Meat products" },
                new Category { Name = "Detegents", Description = "Cleaning products", ImagePath = "img/omo-s.png" }
            );
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded legacy categories.");
        }

        if (!db.Units.Any())
        {
            db.Units.AddRange(
                new Unit { Name = "Retail", Abbreviation = "retail", Description = "Single retail unit" },
                new Unit { Name = "Pack", Abbreviation = "pack", Description = "Packaged unit" },
                new Unit { Name = "Tray", Abbreviation = "tray", Description = "Tray unit" },
                new Unit { Name = "Sachet", Abbreviation = "sachet", Description = "Sachet unit" },
                new Unit { Name = "Bottle", Abbreviation = "bottle", Description = "Bottle unit" }
            );
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded base units.");
        }

        if (!db.Manufacturers.Any())
        {
            db.Manufacturers.Add(new Manufacturer
            {
                Name = "Clexan Foods",
                RegistrationNumber = "CLX-001",
                Website = "https://clexan.local",
                Notes = "Seed manufacturer from legacy dataset",
                ImagePath = "img/chinchin-pkg.png"
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default manufacturer.");
        }

        if (!db.Items.Any())
        {
            var categories = db.Categories.AsNoTracking().ToDictionary(x => x.Name, x => x.CategoryId);
            var units = db.Units.AsNoTracking().ToDictionary(x => x.Abbreviation, x => x.UnitId);
            var manufacturerId = db.Manufacturers.AsNoTracking().Select(m => m.ManufacturerId).First();

            var items = new[]
            {
                new { Name = "Chin-chin", Variant = "small", Price = 100m, Stock = 50, Img = "img/chinchin-pkg.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Chin-chin", Variant = "medium", Price = 500m, Stock = 21, Img = "img/chinchin-p.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Chin-chin", Variant = "large", Price = 1000m, Stock = 5, Img = "img/peanuts-g.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Dough-nuts", Variant = "regular", Price = 100m, Stock = 44, Img = "img/doughnt.png", Cat = "Groceries", Unit = "retail" },
                new { Name = "Pea nuts", Variant = "small", Price = 100m, Stock = 76, Img = "img/peanuts-fl.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Pea nuts", Variant = "large", Price = 1300m, Stock = 10, Img = "img/peanuts-g.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Eggs", Variant = "retail", Price = 100m, Stock = 90, Img = "img/egg-cr.png", Cat = "Groceries", Unit = "retail" },
                new { Name = "Eggs", Variant = "tray", Price = 2200m, Stock = 4, Img = "img/eggs-tray.png", Cat = "Groceries", Unit = "tray" },
                new { Name = "Maggi", Variant = "5-pieces", Price = 50m, Stock = 50, Img = "img/maggi-s-c.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Salt", Variant = "small", Price = 50m, Stock = 100, Img = "img/salt.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Sugar", Variant = "5-cube", Price = 25m, Stock = 300, Img = "img/sugar-c.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Parle G", Variant = "small size", Price = 25m, Stock = 62, Img = "img/parle-g.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Planet", Variant = "1.25l", Price = 500m, Stock = 12, Img = "img/planet-1l.png", Cat = "Beverges", Unit = "bottle" },
                new { Name = "Sponge", Variant = "soft", Price = 50m, Stock = 13, Img = "img/sponge-1.png", Cat = "Detegents", Unit = "retail" },
                new { Name = "Sponge", Variant = "small-strong", Price = 50m, Stock = 8, Img = "img/sponge-2.png", Cat = "Detegents", Unit = "retail" },
                new { Name = "Tomatoes", Variant = "Sachet", Price = 100m, Stock = 42, Img = "img/tomat-ss.png", Cat = "Vegitebles", Unit = "sachet" },
                new { Name = "Spagetti", Variant = "small-250", Price = 250m, Stock = 12, Img = "img/spagg-s.png", Cat = "Groceries", Unit = "pack" },
                new { Name = "Omo", Variant = "local", Price = 50m, Stock = 30, Img = "img/omo-s.png", Cat = "Detegents", Unit = "pack" },
                new { Name = "Top milk", Variant = "small", Price = 50m, Stock = 8, Img = "img/top-milk.png", Cat = "Beverges", Unit = "pack" }
            };

            foreach (var seed in items)
            {
                db.Items.Add(new Item
                {
                    Name = seed.Name,
                    Description = seed.Variant,
                    UnitPrice = seed.Price,
                    CostPrice = decimal.Round(seed.Price * 0.7m, 2, MidpointRounding.AwayFromZero),
                    InStock = seed.Stock,
                    ReorderLevel = 5,
                    Type = ItemType.Product,
                    Barcode = $"SEED-{seed.Name[..Math.Min(seed.Name.Length, 3)].ToUpperInvariant()}-{seed.Stock.ToString(CultureInfo.InvariantCulture)}",
                    IsActive = true,
                    ImagePath = seed.Img,
                    CategoryId = categories.GetValueOrDefault(seed.Cat),
                    UnitId = units.GetValueOrDefault(seed.Unit),
                    ManufacturerId = manufacturerId
                });
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded legacy items ({Count} rows).", items.Length);
        }
    }

    // -------------------------------------------------------------------------
    // Roles, Permissions, Departments, Branches, Employees, Test Users
    // -------------------------------------------------------------------------

    private static async Task SeedRolesAndUsersAsync(StoreDbContext db, ILogger logger, CancellationToken ct)
    {
        // ── 1. Roles ──────────────────────────────────────────────────────────
        var roleNames = new[] { "Admin", "Manager", "Cashier" };
        var roleDescriptions = new Dictionary<string, string>
        {
            ["Admin"]   = "Full system access",
            ["Manager"] = "Inventory, pricing, cash and reports access",
            ["Cashier"] = "Cash operations access only"
        };

        foreach (var name in roleNames)
        {
            if (!db.Roles.Any(r => r.Name == name))
            {
                db.Roles.Add(new Role { Name = name, Description = roleDescriptions[name] });
            }
        }
        await db.SaveChangesAsync(ct);

        var roles = db.Roles.AsNoTracking().ToDictionary(r => r.Name, r => r.RoleId);

        // ── 2. Role Permissions ───────────────────────────────────────────────
        // Remove any auto-seeder junk rows
        var junkKeys = db.RolePermissions
            .Where(rp => rp.PermissionKey.StartsWith("seed_"))
            .ToList();
        if (junkKeys.Count > 0)
        {
            db.RolePermissions.RemoveRange(junkKeys);
            await db.SaveChangesAsync(ct);
        }

        var rolePerms = new Dictionary<string, string[]>
        {
            ["Admin"] = PermissionKeys.All,
            ["Manager"] =
            [
                PermissionKeys.InventoryRead,
                PermissionKeys.InventoryWrite,
                PermissionKeys.PricingRead,
                PermissionKeys.PricingWrite,
                PermissionKeys.CashRead,
                PermissionKeys.CashWrite,
                PermissionKeys.ReportsRead,
                PermissionKeys.PaymentsRead
            ],
            ["Cashier"] =
            [
                PermissionKeys.CashRead,
                PermissionKeys.CashWrite
            ]
        };

        foreach (var (roleName, keys) in rolePerms)
        {
            if (!roles.TryGetValue(roleName, out var roleId)) continue;

            foreach (var key in keys)
            {
                if (!db.RolePermissions.Any(rp => rp.RoleId == roleId && rp.PermissionKey == key))
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionKey = key,
                        IsAllowed = true
                    });
                }
            }
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded roles and permissions.");

        // ── 3. Departments ────────────────────────────────────────────────────
        var deptNames = new[] { "Management", "Sales", "Operations" };
        foreach (var dept in deptNames)
        {
            if (!db.Departments.Any(d => d.Name == dept))
                db.Departments.Add(new Department { Name = dept, Description = dept + " department" });
        }
        await db.SaveChangesAsync(ct);

        var depts = db.Departments.AsNoTracking().ToDictionary(d => d.Name, d => d.DepartmentId);

        // ── 4. Main Branch ────────────────────────────────────────────────────
        if (!db.Branches.Any(b => b.Code == "HQ"))
        {
            db.Branches.Add(new Branch
            {
                Name = "Main Branch (HQ)",
                Code = "HQ",
                Address = "1 Store Avenue, City Centre",
                IsActive = true
            });
            await db.SaveChangesAsync(ct);
        }

        var mainBranchId = db.Branches.AsNoTracking()
            .Where(b => b.Code == "HQ")
            .Select(b => b.BranchId)
            .First();

        // ── 5. Test accounts ──────────────────────────────────────────────────
        //  username │ role    │ password      │ first name │ last name │ dept
        //  admin    │ Admin   │ Admin@123     │ Alice      │ Admin     │ Management
        //  manager  │ Manager │ Manager@123   │ Mike       │ Manager   │ Sales
        //  cashier  │ Cashier │ Cashier@123   │ Chris      │ Cashier   │ Operations

        var accounts = new[]
        {
            new { Username = "admin",   Role = "Admin",   Password = "Admin@123",   First = "Alice", Last = "Admin",   Dept = "Management" },
            new { Username = "manager", Role = "Manager", Password = "Manager@123", First = "Mike",  Last = "Manager", Dept = "Sales"      },
            new { Username = "cashier", Role = "Cashier", Password = "Cashier@123", First = "Chris", Last = "Cashier", Dept = "Operations" }
        };

        foreach (var acc in accounts)
        {
            if (!roles.TryGetValue(acc.Role, out var roleId))
                continue;

            var deptId = depts.TryGetValue(acc.Dept, out var did) ? (int?)did : null;

            var existingUser = db.Users.FirstOrDefault(u => u.Username == acc.Username);
            if (existingUser is not null)
            {
                // Correct role if it was assigned before roles were seeded
                if (existingUser.RoleId != roleId)
                {
                    existingUser.RoleId = roleId;
                    db.Users.Update(existingUser);
                }

                // Link employee if not already linked
                if (existingUser.EmployeeId is null)
                {
                    var emp = new Employee
                    {
                        FirstName    = acc.First,
                        LastName     = acc.Last,
                        DateEmployed = DateTime.UtcNow,
                        Status       = EmployeeStatus.Active,
                        DepartmentId = deptId,
                        Gender       = Gender.NotSpecified,
                        ImagePath    = "img/user_default.png"
                    };
                    db.Employees.Add(emp);
                    await db.SaveChangesAsync(ct);
                    existingUser.EmployeeId = emp.EmployeeId;
                    db.Users.Update(existingUser);
                }

                await db.SaveChangesAsync(ct);

                // Ensure branch-role assignment
                if (!db.UserBranchRoles.Any(ubr => ubr.UserId == existingUser.UserId && ubr.BranchId == mainBranchId))
                {
                    db.UserBranchRoles.Add(new UserBranchRole
                    {
                        UserId   = existingUser.UserId,
                        BranchId = mainBranchId,
                        RoleId   = roleId
                    });
                    await db.SaveChangesAsync(ct);
                }

                logger.LogInformation("Updated existing user: {Username} → Role: {Role}", acc.Username, acc.Role);
                continue;
            }

            // Create employee
            var employee = new Employee
            {
                FirstName    = acc.First,
                LastName     = acc.Last,
                DateEmployed = DateTime.UtcNow,
                Status       = EmployeeStatus.Active,
                DepartmentId = deptId,
                Gender       = Gender.NotSpecified,
                ImagePath    = "img/user_default.png"
            };
            db.Employees.Add(employee);
            await db.SaveChangesAsync(ct);

            // Create user linked to employee
            var user = new User
            {
                Username   = acc.Username,
                RoleId     = roleId,
                EmployeeId = employee.EmployeeId,
                Status     = UserStatus.Active,
                ImagePath  = "img/user_default.png"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            // Password
            db.UserPasswords.Add(new UserPassword
            {
                UserId       = user.UserId,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(acc.Password, 12)
            });

            // Branch role assignment
            db.UserBranchRoles.Add(new UserBranchRole
            {
                UserId   = user.UserId,
                BranchId = mainBranchId,
                RoleId   = roleId
            });

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded test user: {Username} / {Password} (Role: {Role})", acc.Username, acc.Password, acc.Role);
        }
    }

    private static async Task SeedAllRemainingEmptyTablesAsync(StoreDbContext db, ILogger logger, CancellationToken ct)
    {
        var model = db.Model;
        var entityTypes = model.GetEntityTypes()
            .Where(e => e.ClrType is not null && !e.IsOwned())
            .ToList();

        var order = TopologicalSort(entityTypes);
        var seededInstanceByType = new Dictionary<Type, object>();

        var remaining = new HashSet<IEntityType>(order);
        var pass = 0;

        while (remaining.Count > 0 && pass < 5)
        {
            pass++;
            var progress = false;

            foreach (var entityType in order.Where(remaining.Contains).ToList())
            {
                var clrType = entityType.ClrType;
                if (clrType is null)
                {
                    remaining.Remove(entityType);
                    continue;
                }

                if (HasAnyRows(db, clrType))
                {
                    remaining.Remove(entityType);
                    continue;
                }

                if (!TryCreateSeedEntity(db, entityType, seededInstanceByType, out var entity))
                    continue;

                try
                {
                    db.Add(entity);
                    await db.SaveChangesAsync(ct);
                    seededInstanceByType[clrType] = entity;
                    remaining.Remove(entityType);
                    progress = true;
                }
                catch (Exception ex)
                {
                    db.ChangeTracker.Clear();
                    logger.LogWarning(ex, "Skipping seed for entity type {EntityType} due to constraint mismatch.", clrType.Name);
                }
            }

            if (!progress)
                break;
        }

        if (remaining.Count > 0)
        {
            logger.LogWarning("Some tables were not auto-seeded after {Passes} passes: {Tables}",
                pass,
                string.Join(", ", remaining.Select(r => r.GetTableName() ?? r.ClrType?.Name ?? "unknown")));
        }
        else
        {
            logger.LogInformation("Seed pass completed. All code-first tables have at least one row.");
        }
    }

    private static List<IEntityType> TopologicalSort(List<IEntityType> entityTypes)
    {
        var edges = entityTypes.ToDictionary(x => x, _ => new HashSet<IEntityType>());
        var inDegree = entityTypes.ToDictionary(x => x, _ => 0);

        foreach (var dependent in entityTypes)
        {
            foreach (var fk in dependent.GetForeignKeys().Where(f => f.IsRequired))
            {
                var principal = fk.PrincipalEntityType;
                if (!entityTypes.Contains(principal) || principal == dependent)
                    continue;

                if (edges[principal].Add(dependent))
                    inDegree[dependent]++;
            }
        }

        var queue = new Queue<IEntityType>(inDegree.Where(x => x.Value == 0).Select(x => x.Key));
        var result = new List<IEntityType>(entityTypes.Count);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var next in edges[current])
            {
                inDegree[next]--;
                if (inDegree[next] == 0)
                    queue.Enqueue(next);
            }
        }

        if (result.Count < entityTypes.Count)
        {
            var missing = entityTypes.Except(result);
            result.AddRange(missing);
        }

        return result;
    }

    private static bool TryCreateSeedEntity(
        StoreDbContext db,
        IEntityType entityType,
        IReadOnlyDictionary<Type, object> seededInstanceByType,
        out object entity)
    {
        entity = null!;
        var clrType = entityType.ClrType;
        if (clrType is null)
            return false;

        entity = Activator.CreateInstance(clrType)!;
        if (entity is null)
            return false;

        // Required foreign keys first.
        foreach (var fk in entityType.GetForeignKeys())
        {
            var principalType = fk.PrincipalEntityType.ClrType;
            if (principalType is null)
                continue;

            var principal = seededInstanceByType.TryGetValue(principalType, out var cached)
                ? cached
                : GetFirstEntity(db, principalType);

            if (principal is null)
            {
                if (fk.IsRequired)
                    return false;

                continue;
            }

            for (var i = 0; i < fk.Properties.Count; i++)
            {
                var depProp = fk.Properties[i];
                var principalProp = fk.PrincipalKey.Properties[i];

                if (depProp.IsShadowProperty() || principalProp.IsShadowProperty())
                    continue;

                var depInfo = clrType.GetProperty(depProp.Name, BindingFlags.Public | BindingFlags.Instance);
                var principalInfo = principalType.GetProperty(principalProp.Name, BindingFlags.Public | BindingFlags.Instance);

                if (depInfo is null || principalInfo is null || !depInfo.CanWrite)
                    continue;

                var principalValue = principalInfo.GetValue(principal);
                depInfo.SetValue(entity, principalValue);
            }
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is not null)
        {
            foreach (var pkProp in primaryKey.Properties.Where(p => !p.IsShadowProperty()))
            {
                var pkInfo = clrType.GetProperty(pkProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (pkInfo is null || !pkInfo.CanWrite)
                    continue;

                var current = pkInfo.GetValue(entity);
                if (HasMeaningfulValue(current, pkInfo.PropertyType))
                    continue;

                if (pkInfo.PropertyType == typeof(Guid))
                    pkInfo.SetValue(entity, Guid.NewGuid());
                else if (pkInfo.PropertyType == typeof(string))
                    pkInfo.SetValue(entity, $"seed_{ToSnakeCase(clrType.Name)}_id");
                else if (pkInfo.PropertyType == typeof(int) && pkProp.ValueGenerated == ValueGenerated.Never)
                    pkInfo.SetValue(entity, 1);
            }
        }

        foreach (var property in entityType.GetProperties())
        {
            if (property.IsShadowProperty() || property.IsPrimaryKey() || property.IsForeignKey())
                continue;

            var propInfo = clrType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo is null || !propInfo.CanWrite)
                continue;

            var current = propInfo.GetValue(entity);
            if (HasMeaningfulValue(current, propInfo.PropertyType))
                continue;

            if (property.IsNullable)
                continue;

            var fallback = BuildFallbackValue(clrType.Name, property, propInfo.PropertyType);
            if (fallback is not null)
                propInfo.SetValue(entity, fallback);
        }

        return true;
    }

    private static object? BuildFallbackValue(string entityName, IProperty property, Type propertyType)
    {
        var target = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var maxLength = property.GetMaxLength();
        var propName = property.Name;

        if (target == typeof(string))
        {
            var value = propName switch
            {
                "Username" => "seed_user",
                "PasswordHash" => BCrypt.Net.BCrypt.EnhancedHashPassword("ChangeMe123!", 12),
                "Token" => $"seed_token_{Guid.NewGuid():N}",
                "RefreshTokenHash" => $"seed_refresh_{Guid.NewGuid():N}",
                _ when propName.Contains("Email", StringComparison.OrdinalIgnoreCase) => $"seed_{ToSnakeCase(entityName)}@example.com",
                _ when propName.Contains("Phone", StringComparison.OrdinalIgnoreCase) => "+237600000001",
                _ when propName.Contains("OrderNumber", StringComparison.OrdinalIgnoreCase) => $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                _ when propName.EndsWith("Code", StringComparison.OrdinalIgnoreCase) => "SEED-CODE",
                _ when propName.EndsWith("Name", StringComparison.OrdinalIgnoreCase) => $"Seed {entityName}",
                _ => $"seed_{ToSnakeCase(entityName)}_{ToSnakeCase(propName)}"
            };

            if (maxLength is > 0 && value.Length > maxLength.Value)
                value = value[..maxLength.Value];

            return value;
        }

        if (target == typeof(Guid)) return Guid.NewGuid();
        if (target == typeof(int)) return 1;
        if (target == typeof(long)) return 1L;
        if (target == typeof(short)) return (short)1;
        if (target == typeof(byte)) return (byte)1;
        if (target == typeof(bool)) return true;
        if (target == typeof(decimal)) return 1.0m;
        if (target == typeof(double)) return 1.0d;
        if (target == typeof(float)) return 1.0f;
        if (target == typeof(DateTime)) return DateTime.UtcNow;
        if (target == typeof(DateOnly)) return DateOnly.FromDateTime(DateTime.UtcNow);
        if (target == typeof(TimeOnly)) return TimeOnly.FromDateTime(DateTime.UtcNow);
        if (target == typeof(TimeSpan)) return TimeSpan.FromMinutes(1);
        if (target == typeof(byte[])) return new byte[] { 1, 2, 3 };
        if (target.IsEnum) return Enum.GetValues(target).GetValue(0);

        return null;
    }

    private static bool HasAnyRows(StoreDbContext db, Type clrType)
    {
        var set = GetQueryableForType(db, clrType);
        var anyMethod = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Queryable.Any)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1)
            .MakeGenericMethod(clrType);

        return (bool)(anyMethod.Invoke(null, new object[] { set }) ?? false);
    }

    private static object? GetFirstEntity(StoreDbContext db, Type clrType)
    {
        var set = GetQueryableForType(db, clrType);
        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Queryable.FirstOrDefault)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1)
            .MakeGenericMethod(clrType);

        return method.Invoke(null, new object[] { set });
    }

    private static IQueryable GetQueryableForType(StoreDbContext db, Type clrType)
    {
        var setMethod = typeof(DbContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == nameof(DbContext.Set)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 0)
            .MakeGenericMethod(clrType);

        return (IQueryable)(setMethod.Invoke(db, null)
                            ?? throw new InvalidOperationException($"Could not resolve DbSet for type {clrType.Name}."));
    }

    private static bool HasMeaningfulValue(object? value, Type propertyType)
    {
        if (value is null)
            return false;

        var target = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (target == typeof(string))
            return !string.IsNullOrWhiteSpace((string)value);
        if (target == typeof(Guid))
            return (Guid)value != Guid.Empty;

        if (target.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(target);
            return !Equals(value, defaultValue);
        }

        return true;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var chars = new List<char>(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1])))
                    chars.Add('_');

                chars.Add(char.ToLowerInvariant(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }
}