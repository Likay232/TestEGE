using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AdminService(DataComponent component)
{
    public async Task<List<UserDto>> GetUsers()
    {
        var users = await component.Users
            .Include(x => x.Role)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var blockMap = await component.BlockUsers
            .Where(b => b.BlockedUntil > now)
            .Select(u => u.UserId)
            .ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            About = u.About,
            DateOfBirth = u.DateOfBirth,
            TimeZone = u.TimeZone,
            RoleName = u.Role.RoleName,
            IsBlocked = blockMap.Contains(u.Id)
        }).ToList();
    }

    public async Task<EditUser> GetUserToEdit(int userId)
    {
        if (!await component.Users.AnyAsync(u => u.Id == userId))
            throw new Exception("Пользователь с данным Id не найден.");

        var blockedUntil = await component.BlockUsers
            .Where(b => b.UserId == userId && b.BlockedUntil > DateTime.UtcNow)
            .Select(u => u.BlockedUntil)
            .FirstOrDefaultAsync();

        return await component.Users
            .Include(x => x.Role)
            .Select(u => new EditUser
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                About = u.About,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                RoleName = u.Role.RoleName,
                Password = u.Password,
                BlockedUntil = blockedUntil
            })
            .FirstAsync(u => u.Id == userId);
    }

    public async Task<bool> EditUser(EditUser user)
    {
        var userEntry = await component.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userEntry == null)
            throw new Exception("Пользователь с заданным Id не найден.");

        var roleEntry = await component.Roles
            .FirstOrDefaultAsync(r => r.RoleName == user.RoleName);

        if (roleEntry == null)
            throw new Exception($"Роль {user.RoleName} не найдена.");

        userEntry.FirstName = user.FirstName;
        userEntry.LastName = user.LastName;
        userEntry.Email = user.Email;
        userEntry.About = user.About;
        userEntry.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
        userEntry.TimeZone = user.TimeZone;
        userEntry.RoleId = roleEntry.Id;

        await UpdateBlockStatusAsync(user.Id, user.BlockedUntil);

        return await component.Update(userEntry);
    }

    private async Task UpdateBlockStatusAsync(int userId, DateTime? blockedUntil)
    {
        var existingBlock = await component.BlockUsers
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (blockedUntil == null || blockedUntil < DateTime.UtcNow)
        {
            if (existingBlock != null)
                await component.Delete<BlockUser>(existingBlock.Id);
        }
        else
        {
            if (existingBlock == null)
            {
                var newBlock = new BlockUser
                {
                    UserId = userId,
                    BlockedUntil = DateTime.SpecifyKind(blockedUntil.Value, DateTimeKind.Utc)
                };
                await component.Insert(newBlock);
            }
            else
            {
                existingBlock.BlockedUntil = blockedUntil.Value;
                await component.Update(existingBlock);
            }
        }
    }

    public async Task<List<VariantDto>> GetVariants()
    {
        return await component.Variants
            .Include(v => v.Teacher)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<VariantDto> GetVariant(int id)
    {
        if (!await component.Variants.AnyAsync(v => v.Id == id))
            throw new Exception("Вариант с заданным Id не найден.");

        var exercises = await GetExercisesForVariant(id);
        var assignedStudents = await GetAssignedStudentsForVariant(id);

        return await component.Variants
            .Where(v => v.Id == id)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
                Exercises = exercises,
                AssignedUsers = assignedStudents
            })
            .FirstAsync();
    }

    private async Task<List<ExerciseDto>> GetExercisesForVariant(int variantId)
    {
        return await component.VariantExercises
            .Include(v => v.Exercise)
            .ThenInclude(e => e.Teacher)
            .Where(v => v.VariantId == variantId)
            .Select(v => new ExerciseDto
            {
                Id = v.ExerciseId,
                EgeNumber = v.Exercise.EgeNumber,
                Year = v.Exercise.Year,
                TeacherFirstName = v.Exercise.Teacher.FirstName,
                TeacherLastName = v.Exercise.Teacher.LastName,
            })
            .ToListAsync();
    }

    private async Task<List<UserDto>> GetAssignedStudentsForVariant(int variantId)
    {
        return await component.VariantAssignments
            .Include(v => v.Student)
            .Where(v => v.VariantId == variantId)
            .Select(va => new UserDto
            {
                Id = va.StudentId,
                Email = va.Student.Email,
                FirstName = va.Student.FirstName,
                LastName = va.Student.LastName,
            })
            .ToListAsync();
    }

    public async Task<bool> EditVariant(VariantDto updatedVariant)
    {
        var variantEntry = await component.Variants
            .Where(v => v.Id == updatedVariant.Id)
            .FirstOrDefaultAsync();

        if (variantEntry == null) throw new Exception("Вариант с заданным Id не найден.");

        var currentVariantExercises = await GetExercisesForVariant(updatedVariant.Id);
        var currentAssignedStudents = await GetAssignedStudentsForVariant(updatedVariant.Id);

        foreach (var exercise in currentVariantExercises)
        {
            if (!updatedVariant.Exercises.Exists(e => e.Id == exercise.Id))
            {
                await component.Delete<VariantExercise>(exercise.Id);
            }
        }

        foreach (var exercise in updatedVariant.Exercises)
        {
            if (currentVariantExercises.All(e => e.Id != exercise.Id))
            {
                await component.Insert(new VariantExercise
                {
                    VariantId = variantEntry.Id,
                    ExerciseId = exercise.Id
                });
            }
        }

        foreach (var assignment in currentAssignedStudents)
        {
            if (updatedVariant.AssignedUsers.All(s => s.Id != assignment.Id))
            {
                var variantAssignmentEntry = await component.VariantAssignments
                    .Include(v => v.Variant)
                    .Include(v => v.Student)
                    .Where(v => v.VariantId == variantEntry.Id && v.StudentId == assignment.Id)
                    .FirstOrDefaultAsync();

                if (variantAssignmentEntry == null) continue;

                await component.Delete<VariantAssignment>(variantAssignmentEntry.Id);
            }
        }

        foreach (var student in updatedVariant.AssignedUsers)
        {
            if (currentAssignedStudents.All(a => a.Id != student.Id))
            {
                await component.Insert(new VariantAssignment
                {
                    VariantId = updatedVariant.Id,
                    StudentId = student.Id
                });
            }
        }

        variantEntry.Title = updatedVariant.Title;
        variantEntry.TeacherId = updatedVariant.TeacherId;

        return await component.Update(variantEntry);
    }

    public async Task<List<UserDto>> GetStudents()
    {
        return await component.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == "Student")
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .ToListAsync();
    }

    public async Task<List<ExerciseDto>> GetExercises()
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.ModerationPassed)
            .Select(ex => new ExerciseDto
            {
                Id = ex.Id,
                Text = ex.Text,
                Answer = ex.Answer,
                Year = ex.Year,
                ExerciseFilePath = ex.ExerciseFilePath,
                SolutionFilePath = ex.SolutionFilePath,
                EgeNumber = ex.EgeNumber,
                AttachmentRequired = ex.AttachmentRequired,
                ModerationPassed = ex.ModerationPassed,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<EditExercise> GetExerciseToEdit(int exerciseId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.Id == exerciseId)
            .Select(ex => new EditExercise
            {
                Id = ex.Id,
                Text = ex.Text,
                Answer = ex.Answer,
                Year = ex.Year,
                ExerciseFilePath = ex.ExerciseFilePath,
                SolutionFilePath = ex.SolutionFilePath,
                EgeNumber = ex.EgeNumber,
                AttachmentRequired = ex.AttachmentRequired,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .FirstAsync();
    }

    //TODO: дописать обновление задания.
    public async Task<bool> EditExercise(EditExercise updatedExercise)
    {
        var exerciseEntry = await component.Exercises.FirstOrDefaultAsync(e => e.Id == updatedExercise.Id);

        if (exerciseEntry == null)
            throw new Exception("Задание с Id не найдено.");

        return true;
    }

    public async Task<List<SelectListItem>> GetTeachers()
    {
        return await component.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == "Teacher")
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.LastName + " " + u.FirstName
            })
            .ToListAsync();
    }

    public async Task<List<ExerciseDto>> GetExercisesToModerate()
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => !ex.ModerationPassed)
            .Select(ex => new ExerciseDto
            {
                Id = ex.Id,
                Text = ex.Text,
                Answer = ex.Answer,
                Year = ex.Year,
                ExerciseFilePath = ex.ExerciseFilePath,
                SolutionFilePath = ex.SolutionFilePath,
                EgeNumber = ex.EgeNumber,
                AttachmentRequired = ex.AttachmentRequired,
                ModerationPassed = ex.ModerationPassed,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<bool> ModerateExercise(Moderation request)
    {
        var exerciseEntry = await component.Exercises
            .FirstOrDefaultAsync(ex => ex.Id == request.ExerciseId);

        if (exerciseEntry == null)
            throw new Exception("Задание с заданным ID не найдено.");

        if (request.Approved)
        {
            exerciseEntry.ModerationPassed = true;
            return await component.Update(exerciseEntry);
        }

        var variantExercises = await component.VariantExercises
            .Where(v => v.ExerciseId == exerciseEntry.Id)
            .ToListAsync();

        foreach (var exercise in variantExercises)
        {
            await component.Delete<VariantExercise>(exercise.Id);
        }

        var studentExercises = await component.StudentExercises
            .Where(v => v.ExerciseId == exerciseEntry.Id)
            .ToListAsync();

        foreach (var exercise in studentExercises)
        {
            await component.Delete<StudentExercise>(exercise.Id);
        }

        return await component.Delete<Exercise>(exerciseEntry.Id);
    }

    public async Task<bool> DeleteVariant(int variantId)
    {
        await DeleteByForeignKey(component.VariantExercises, variantId, "VariantId");
        await DeleteByForeignKey(component.VariantAssignments, variantId, "VariantId");
        return await component.Delete<Variant>(variantId);
    }

    public async Task<bool> DeleteExercise(int exerciseId)
    {
        await DeleteByForeignKey(component.VariantExercises, exerciseId, "ExerciseId");
        await DeleteByForeignKey(component.StudentExercises, exerciseId, "ExerciseId");
        return await component.Delete<Exercise>(exerciseId);
    }

    public async Task<bool> DeleteUser(int userId)
    {
        var userEntry = await component.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (userEntry == null) throw new Exception("Пользователь с данным Id не найден.");

        if (userEntry.Role.RoleName == "Student")
            return await DeleteStudent(userEntry);
        if (userEntry.Role.RoleName == "Teacher")
            return await DeleteTeacher(userEntry);

        return false;
    }

    private async Task<bool> DeleteStudent(User student)
    {
        await DeleteByForeignKey(component.StudentExercises, student.Id, "StudentId");
        await DeleteByForeignKey(component.GroupStudents, student.Id, "StudentId");
        await DeleteByForeignKey(component.ResetPasses, student.Id, "UserId");
        await DeleteByForeignKey(component.VariantAssignments, student.Id, "StudentId");
        await DeleteByForeignKey(component.BlockUsers, student.Id, "UserId");

        return await component.Delete<User>(student.Id);
    }

    private async Task<bool> DeleteTeacher(User teacher)
    {
        await DeleteByForeignKey(component.VariantAssignments, teacher.Id, "TeacherId");
        await DeleteByForeignKey(component.ResetPasses, teacher.Id, "UserId");
        await DeleteByForeignKey(component.BlockUsers, teacher.Id, "UserId");

        await DeleteByForeignKey(component.Groups, teacher.Id, "TeacherId", DeleteGroup);
        await DeleteByForeignKey(component.Exercises, teacher.Id, "TeacherId", DeleteExercise);
        await DeleteByForeignKey(component.Variants, teacher.Id, "TeacherId", DeleteVariant);

        return await component.Delete<User>(teacher.Id);
    }
    
    private async Task<bool> DeleteGroup(int groupId)
    {
        await DeleteByForeignKey(component.GroupStudents, groupId, "GroupId");
        return await component.Delete<Group>(groupId);
    }

    private async Task DeleteByForeignKey<T>(
        IQueryable<T> queryable,
        int foreignKeyValue,
        string foreignKeyName,
        Func<int, Task>? deleteFunc = null
    ) where T : class
    {
        var ids = await queryable
            .Where(e => EF.Property<int>(e, foreignKeyName) == foreignKeyValue)
            .Select(e => EF.Property<int>(e, "Id"))
            .ToListAsync();

        foreach (var id in ids)
        {
            if (deleteFunc != null)
                await deleteFunc(id);
            else
                await component.Delete<T>(id);
        }
    }
}