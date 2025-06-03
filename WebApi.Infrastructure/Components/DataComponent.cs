using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Infrastructure.Components;

public class DataComponent(string connectionString)
{
    public IQueryable<User> Users => new DatabaseContext(connectionString).Users;
    public IQueryable<BlockUser> BlockUsers => new DatabaseContext(connectionString).BlockUsers;
    public IQueryable<Exercise> Exercises => new DatabaseContext(connectionString).Exercises;
    public IQueryable<Group> Groups => new DatabaseContext(connectionString).Groups;
    public IQueryable<GroupStudent> GroupStudents => new DatabaseContext(connectionString).GroupStudents;
    public IQueryable<Role> Roles => new DatabaseContext(connectionString).Roles;
    public IQueryable<StudentExercise> StudentExercises => new DatabaseContext(connectionString).StudentExercises;
    public IQueryable<Variant> Variants => new DatabaseContext(connectionString).Variants;
    public IQueryable<VariantAssignment> VariantAssignments => new DatabaseContext(connectionString).VariantAssignments;
    public IQueryable<VariantExercise> VariantExercises => new DatabaseContext(connectionString).VariantExercises;

    public async Task<bool> Insert<T>(T entityItem) where T : class
    {
        try
        {
            await using var context = new DatabaseContext(connectionString);
            await context.AddAsync(entityItem);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<bool> Update<T>(T entityItem) where T : class
    {
        try
        {
            await using var context = new DatabaseContext(connectionString);
            context.Entry(entityItem).State = EntityState.Modified;
            context.Update(entityItem);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Delete<T>(int entityId) where T : class
    {
        try
        {
            await using var context = new DatabaseContext(connectionString);
            var entity = await context.Set<T>().FindAsync(entityId);

            if (entity != null)
            {
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}