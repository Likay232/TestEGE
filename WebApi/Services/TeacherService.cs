using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class TeacherService(DataComponent component, FileService fileService)
{
    public async Task<ProfileInfo> GetProfileInfo(int userId)
    {
        return await component.Users
            .Select(u => new ProfileInfo
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Password = u.Password,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                About = u.About
            })
            .FirstAsync(x => x.Id == userId);
    }

    public async Task<bool> EditProfileInfo(ProfileInfo profileInfo)
    {
        var userEntry = await component.Users
            .FirstOrDefaultAsync(u => u.Id == profileInfo.Id);
        if (userEntry == null) return false;

        userEntry.FirstName = profileInfo.FirstName;
        userEntry.LastName = profileInfo.LastName;
        userEntry.Email = profileInfo.Email;
        userEntry.Password = profileInfo.Password;
        userEntry.DateOfBirth = DateTime.SpecifyKind(profileInfo.DateOfBirth, DateTimeKind.Utc);
        userEntry.TimeZone = profileInfo.TimeZone;
        userEntry.About = profileInfo.About;

        return await component.Update(userEntry);
    }

    public async Task<List<ExerciseDto>> GetAllExercises()
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

    public async Task<List<ExerciseDto>> GetMyExercises(int userId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.TeacherId == userId)
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
                ModerationPassed = ex.ModerationPassed,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .FirstAsync();
    }

    public async Task<bool> EditExercise(EditExercise updatedExercise)
    {
        var exerciseEntry = await component.Exercises.FirstOrDefaultAsync(e => e.Id == updatedExercise.Id);

        if (exerciseEntry == null)
            throw new Exception("Задание с таким Id не найдено.");

        exerciseEntry.Text = updatedExercise.Text;
        exerciseEntry.Answer = updatedExercise.Answer;
        exerciseEntry.Year = updatedExercise.Year;
        exerciseEntry.EgeNumber = updatedExercise.EgeNumber;
        exerciseEntry.AttachmentRequired = updatedExercise.AttachmentRequired;
        exerciseEntry.TeacherId = updatedExercise.TeacherId;
        exerciseEntry.ModerationPassed = updatedExercise.ModerationPassed;

        if (updatedExercise.ExerciseFile != null)
        {
            var newExerciseFileName = $"{exerciseEntry.Id}_exercise{Path.GetExtension(updatedExercise.ExerciseFile.FileName)}";
            await fileService.SaveFileToRepo(updatedExercise.ExerciseFile, newExerciseFileName);
            exerciseEntry.ExerciseFilePath = newExerciseFileName;
        }

        if (updatedExercise.SolutionFile != null)
        {
            var newSolutionFileName = $"{exerciseEntry.Id}_solution{Path.GetExtension(updatedExercise.SolutionFile.FileName)}";
            await fileService.SaveFileToRepo(updatedExercise.SolutionFile, newSolutionFileName);
            exerciseEntry.SolutionFilePath = newSolutionFileName;
        }

        return await component.Update(exerciseEntry);
    }

    public async Task<bool> AddExercise(AddExercise request)
    {
        var newEntry = new Exercise
        {
            Text = request.Text,
            Answer = request.Answer,
            Year = request.Year,
            ExerciseFilePath = "",
            SolutionFilePath = "",
            EgeNumber = request.EgeNumber,
            AttachmentRequired = request.AttachmentRequired,
            TeacherId = request.TeacherId,
            ModerationPassed = false,
        };

        var inserted = await component.Insert(newEntry);
        if (!inserted)
            return false;

        var updated = false;

        if (request.ExerciseFile != null)
        {
            var exerciseFileName = $"{newEntry.Id}_exercise{Path.GetExtension(request.ExerciseFile.FileName)}";
            await fileService.SaveFileToRepo(request.ExerciseFile, exerciseFileName);
            newEntry.ExerciseFilePath = exerciseFileName;
            updated = true;
        }

        if (request.SolutionFile != null)
        {
            var solutionFileName = $"{newEntry.Id}_solution{Path.GetExtension(request.SolutionFile.FileName)}";
            await fileService.SaveFileToRepo(request.SolutionFile, solutionFileName);
            newEntry.SolutionFilePath = solutionFileName;
            updated = true;
        }

        if (updated)
            await component.Update(newEntry);

        return true;
    }

    public async Task<List<VariantDto>> GetMyVariants(int teacherId)
    {
        return await component.Variants
            .Include(v => v.Teacher)
            .Where(v => v.TeacherId == teacherId)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<List<VariantDto>> GetAllVariants()
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

    public async Task<VariantDto> GetVariant(int variantId)
    {
        if (!await component.Variants.AnyAsync(v => v.Id == variantId))
            throw new Exception("Вариант не найден.");

        var exercises = await GetExercisesForVariant(variantId);
        var assignedStudents = await GetAssignedStudentsForVariant(variantId);

        return await component.Variants
            .Where(v => v.Id == variantId)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherId = v.TeacherId,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
                Exercises = exercises,
                AssignedUsers = assignedStudents
            })
            .FirstAsync();
    }
    
    public async Task<bool> AddVariant(AddVariant request)
    {
        var newVariant = new Variant
        {
            Title = request.Title,
            TeacherId = request.TeacherId,
        };
        
        await component.Insert(newVariant);
        
        foreach (var exercise in request.Exercises)
        {
            var newVariantExercise = new VariantExercise
            {
                VariantId = newVariant.Id,
                ExerciseId = exercise,
            };
            
            await component.Insert(newVariantExercise);
        }

        foreach (var user in request.AssignedUsers)
        {
            var newAssignment = new VariantAssignment
            {
                VariantId = newVariant.Id,
                StudentId = user,
                TeacherId = newVariant.TeacherId,
            };
            
            await component.Insert(newAssignment);
        }

        return true;
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
    
    public async Task<UserDto> GetStudent(int userId)
    {
        if (!await component.Users.AnyAsync(u => u.Id == userId))
            throw new Exception("Пользователь с заданным Id не найден.");
        
        return await component.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                About = u.About
            })
            .FirstAsync(u => u.Id == userId);
    }
    
    public async Task<List<GroupDto>> GetGroups(int teacherId)
    {
        return await component.Groups
            .Include(g => g.Teacher)
            .Where(g => g.TeacherId == teacherId)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                TeacherFirstName = g.Teacher.FirstName,
                TeacherLastName = g.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<GroupDto> GetGroup(int groupId)
    {
        var group = await component.Groups
            .Include(g => g.Teacher)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                TeacherFirstName = g.Teacher.FirstName,
                TeacherLastName = g.Teacher.LastName,
            })
            .FirstOrDefaultAsync(g => g.Id == groupId);
        
        if (group == null) throw new Exception("Группа с данным Id не найдена.");

        var students = await GetAssignedStudentsForGroup(groupId);
        
        group.Students = students;
        
        return group;
    }

    private async Task<List<UserDto>> GetAssignedStudentsForGroup(int groupId)
    {
        return await component.GroupStudents
            .Include(g => g.Student)
            .Where(g => g.GroupId == groupId)
            .Select(g => new UserDto
            {
                Id = g.StudentId,
                FirstName = g.Student.FirstName,
                LastName = g.Student.LastName,
                Email = g.Student.Email
            })
            .ToListAsync();
    }

    public async Task<bool> EditGroup(EditGroup group)
    {
        if (!await component.Groups.AnyAsync(g => g.Id == group.GroupId))
            return false;

        var groupEntry = await component.Groups
            .FirstAsync(g => g.Id == group.GroupId);

        groupEntry.Name = group.GroupName;
        await component.Update(groupEntry);

        var currentStudents = await component.GroupStudents
            .Where(gs => gs.GroupId == group.GroupId)
            .ToListAsync();

        var newStudentIds = group.StudentIds ?? new List<int>();
        
        var toRemove = currentStudents
            .Where(link => !newStudentIds.Contains(link.StudentId))
            .ToList();

        foreach (var link in toRemove)
        {
            await component.Delete<GroupStudent>(link.Id);
        }

        var existingIds = currentStudents.Select(x => x.StudentId).ToHashSet();
        var toAdd = newStudentIds
            .Where(id => !existingIds.Contains(id))
            .ToList();

        foreach (var id in toAdd)
        {
            var newEntry = new GroupStudent
            {
                GroupId = group.GroupId,
                StudentId = id
            };

            await component.Insert(newEntry);
        }

        return true;
    }

    public async Task<bool> AddGroup(AddGroup request)
    {
        if (request.StudentIds == null)
            return false;
        
        var group = new Group
        {
            Name = request.Name,
            TeacherId = request.TeacherId
        };

        await component.Insert(group);

        foreach (var id in request.StudentIds)
        {
            await component.Insert(new GroupStudent
            {
                GroupId = group.Id,
                StudentId = id
            });
        }
        
        return true;
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
    
    public async Task<bool> DeleteGroup(int groupId)
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
    
    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        return await fileService.GetFileBytes(fileName);
    }
}