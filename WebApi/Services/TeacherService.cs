using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class TeacherService(DataComponent component)
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

    public async Task<List<ExerciseDto>> GetExercises(int userId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.ModerationPassed && ex.TeacherId == userId)
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

    public async Task<bool> AddExercise(AddExercise request)
    {
        var newEntry = new Exercise
        {
            Text = request.Text,
            Answer = request.Answer,
            Year = request.Year,
            ExerciseFilePath = request.ExerciseFilePath ?? "",
            SolutionFilePath = request.SolutionFilePath ?? "",
            EgeNumber = request.EgeNumber,
            AttachmentRequired = request.AttachmentRequired,
            TeacherId = request.TeacherId,
            ModerationPassed = false,
        };

        return await component.Insert(newEntry);
    }

    public async Task<List<VariantDto>> GetVariants(int teacherId)
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

    public async Task<VariantDto> GetVariant(int variantId, int teacherId)
    {
        if (!await component.Variants.AnyAsync(v => v.Id == variantId && v.TeacherId == teacherId))
            throw new Exception("Вариант не найден.");

        var exercises = await GetExercisesForVariant(variantId);
        var assignedStudents = await GetAssignedStudentsForVariant(variantId);

        return await component.Variants
            .Where(v => v.Id == variantId)
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

    private async Task<List<UserForAdminDto>> GetAssignedStudentsForVariant(int variantId)
    {
        return await component.VariantAssignments
            .Include(v => v.Student)
            .Where(v => v.VariantId == variantId)
            .Select(va => new UserForAdminDto
            {
                Id = va.StudentId,
                Email = va.Student.Email,
                FirstName = va.Student.FirstName,
                LastName = va.Student.LastName,
            })
            .ToListAsync();
    }

    public async Task<List<UserForAdminDto>> GetStudents()
    {
        return await component.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == "Student")
            .Select(u => new UserForAdminDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .ToListAsync();
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
}